# Order Database Schema

## PostgreSQL Schema for AmCart E-commerce

---

## Table of Contents

1. [Overview](#overview)
2. [Entity Relationship Diagram](#entity-relationship-diagram)
3. [Tables Schema](#tables-schema)
4. [Order State Machine](#order-state-machine)
5. [Indexes](#indexes)
6. [Sample Data](#sample-data)
7. [Common Queries](#common-queries)
8. [EF Core Entity Models](#ef-core-entity-models)
9. [Event Integration](#event-integration)

---

## Overview

### Design Principles

| Principle | Implementation |
|-----------|----------------|
| **Immutable Snapshots** | Order captures all data at time of purchase |
| **State Machine** | Clear order status transitions |
| **Multi-Seller Support** | Sub-orders per seller for marketplace |
| **Audit Trail** | Complete history of status changes |
| **Event Sourcing Ready** | All changes emit events |

### Database: `amcart_orders`

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         Order Database Architecture                           │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────┐        ┌─────────────────┐       ┌─────────────────┐   │
│  │     orders      │───────▶│  order_items    │◀──────│  seller_orders  │   │
│  │                 │        │                 │       │  (per seller)   │   │
│  │ - Main order    │        │ - Line items    │       │ - Seller view   │   │
│  │ - Customer info │        │ - Product snap  │       │ - Commission    │   │
│  │ - Totals        │        │ - Pricing       │       │ - Payouts       │   │
│  └────────┬────────┘        └─────────────────┘       └─────────────────┘   │
│           │                                                                  │
│           │                                                                  │
│           ├───────────────────────┬───────────────────────┐                 │
│           │                       │                       │                 │
│           ▼                       ▼                       ▼                 │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐         │
│  │ order_addresses │    │ order_status_   │    │   shipments     │         │
│  │                 │    │    history      │    │                 │         │
│  │ - Billing snap  │    │                 │    │ - Tracking      │         │
│  │ - Shipping snap │    │ - All changes   │    │ - Carrier info  │         │
│  └─────────────────┘    └─────────────────┘    └─────────────────┘         │
│                                                         │                   │
│                                                         ▼                   │
│                                                ┌─────────────────┐          │
│                                                │shipment_tracking│          │
│                                                │                 │          │
│                                                │ - Events        │          │
│                                                │ - Location      │          │
│                                                └─────────────────┘          │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              Order Schema ERD                                    │
└─────────────────────────────────────────────────────────────────────────────────┘

                           ┌───────────────────────────────┐
                           │           orders              │
                           ├───────────────────────────────┤
                           │ id (PK)                       │
                           │ order_number (unique)         │
                           │ user_id (FK)                  │
                           │ status                        │
                           │ payment_status                │
                           │ subtotal                      │
                           │ discount_amount               │
                           │ shipping_amount               │
                           │ tax_amount                    │
                           │ total                         │
                           │ currency                      │
                           │ notes                         │
                           │ metadata (JSONB)              │
                           └───────────────┬───────────────┘
                                           │
       ┌───────────────────┬───────────────┼───────────────┬───────────────────┐
       │                   │               │               │                   │
       ▼ 1:N               ▼ 1:N           ▼ 1:2           ▼ 1:N               ▼ 1:N
┌─────────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐ ┌─────────────┐
│  order_items    │ │seller_orders│ │order_address│ │order_status_    │ │  shipments  │
├─────────────────┤ ├─────────────┤ ├─────────────┤ │   history       │ ├─────────────┤
│ id (PK)         │ │ id (PK)     │ │ id (PK)     │ ├─────────────────┤ │ id (PK)     │
│ order_id (FK)   │ │ order_id    │ │ order_id    │ │ id (PK)         │ │ order_id    │
│ seller_order_id │ │ seller_id   │ │ type        │ │ order_id (FK)   │ │ seller_order│
│ product_id      │ │ status      │ │ first_name  │ │ from_status     │ │ carrier     │
│ variant_id      │ │ subtotal    │ │ last_name   │ │ to_status       │ │ tracking_no │
│ quantity        │ │ commission  │ │ phone       │ │ changed_by      │ │ status      │
│ unit_price      │ │ payout_amt  │ │ address     │ │ reason          │ │ shipped_at  │
│ product_snapshot│ │ payout_stat │ │ city        │ │ created_at      │ │ delivered_at│
│ status          │ └─────────────┘ │ postal_code │ └─────────────────┘ └──────┬──────┘
└─────────────────┘                 └─────────────┘                            │
                                                                               ▼ 1:N
                                                                    ┌─────────────────┐
                                                                    │shipment_tracking│
                                                                    ├─────────────────┤
                                                                    │ id (PK)         │
                                                                    │ shipment_id (FK)│
                                                                    │ status          │
                                                                    │ location        │
                                                                    │ description     │
                                                                    │ timestamp       │
                                                                    └─────────────────┘
```

---

## Tables Schema

### 1. Orders Table (Main)

```sql
CREATE TABLE orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Order Identifier
    order_number VARCHAR(50) NOT NULL UNIQUE,
    -- Format: AMC-2026-0000001 (prefix-year-sequence)
    
    -- Customer
    user_id UUID NOT NULL, -- Reference to User Service
    customer_email VARCHAR(255) NOT NULL,
    customer_phone VARCHAR(20),
    customer_name VARCHAR(200) NOT NULL,
    
    -- Source
    cart_id UUID, -- Reference to cart that was converted
    
    -- Order Status
    status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'confirmed', 'processing', 'ready_to_ship', 
    -- 'shipped', 'out_for_delivery', 'delivered', 
    -- 'cancelled', 'returned', 'refunded'
    
    -- Payment Status
    payment_status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'authorized', 'captured', 'partially_refunded', 
    -- 'refunded', 'failed', 'cancelled'
    
    payment_method VARCHAR(50),
    payment_id UUID, -- Reference to Payment Service
    
    -- Pricing
    currency VARCHAR(3) DEFAULT 'INR',
    subtotal DECIMAL(12,2) NOT NULL,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    shipping_amount DECIMAL(12,2) DEFAULT 0.00,
    tax_amount DECIMAL(12,2) DEFAULT 0.00,
    total DECIMAL(12,2) NOT NULL,
    
    -- Tax Details
    tax_breakdown JSONB DEFAULT '{}',
    -- {"cgst": 50.00, "sgst": 50.00, "igst": 0}
    
    -- Applied Discounts
    applied_coupons JSONB DEFAULT '[]',
    -- [{"code": "SAVE10", "discount": 100.00}]
    
    -- Item Summary
    item_count INTEGER DEFAULT 0,
    seller_count INTEGER DEFAULT 0,
    
    -- Shipping
    shipping_method VARCHAR(100),
    shipping_carrier VARCHAR(100),
    estimated_delivery_date DATE,
    actual_delivery_date DATE,
    
    -- Customer Notes
    order_notes TEXT,
    gift_message TEXT,
    is_gift BOOLEAN DEFAULT false,
    
    -- Invoice
    invoice_number VARCHAR(50),
    invoice_generated_at TIMESTAMP WITH TIME ZONE,
    invoice_url VARCHAR(500),
    
    -- Cancellation/Return
    cancellation_reason TEXT,
    cancelled_by VARCHAR(20), -- 'customer', 'seller', 'admin', 'system'
    cancelled_at TIMESTAMP WITH TIME ZONE,
    
    return_requested_at TIMESTAMP WITH TIME ZONE,
    return_reason TEXT,
    
    -- Metadata
    metadata JSONB DEFAULT '{}',
    -- {"source": "mobile_app", "ip_address": "...", "user_agent": "..."}
    
    -- Fraud Check
    fraud_score INTEGER,
    fraud_check_status VARCHAR(20), -- 'passed', 'review', 'failed'
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    confirmed_at TIMESTAMP WITH TIME ZONE,
    shipped_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT valid_status CHECK (status IN (
        'pending', 'confirmed', 'processing', 'ready_to_ship',
        'shipped', 'out_for_delivery', 'delivered',
        'cancelled', 'returned', 'refunded'
    )),
    CONSTRAINT valid_payment_status CHECK (payment_status IN (
        'pending', 'authorized', 'captured', 'partially_refunded',
        'refunded', 'failed', 'cancelled'
    )),
    CONSTRAINT positive_amounts CHECK (
        subtotal >= 0 AND total >= 0 AND 
        discount_amount >= 0 AND shipping_amount >= 0 AND tax_amount >= 0
    )
);

-- Order number sequence
CREATE SEQUENCE order_number_seq START 1;

-- Generate order number function
CREATE OR REPLACE FUNCTION generate_order_number()
RETURNS TRIGGER AS $$
BEGIN
    NEW.order_number := 'AMC-' || TO_CHAR(CURRENT_DATE, 'YYYY') || '-' || 
                        LPAD(nextval('order_number_seq')::TEXT, 7, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_generate_order_number
    BEFORE INSERT ON orders
    FOR EACH ROW
    WHEN (NEW.order_number IS NULL)
    EXECUTE FUNCTION generate_order_number();

CREATE TRIGGER trigger_orders_updated_at
    BEFORE UPDATE ON orders
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 2. Order Items Table

```sql
CREATE TABLE order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    seller_order_id UUID, -- Reference to seller_orders
    
    -- Product Reference (at time of order)
    product_id UUID NOT NULL,
    variant_id UUID NOT NULL,
    seller_id UUID NOT NULL,
    
    -- SKU
    sku VARCHAR(100) NOT NULL,
    
    -- Quantity
    quantity INTEGER NOT NULL,
    
    -- Pricing
    unit_price DECIMAL(12,2) NOT NULL,
    compare_at_price DECIMAL(12,2),
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    tax_amount DECIMAL(12,2) DEFAULT 0.00,
    line_total DECIMAL(12,2) NOT NULL,
    
    -- Cost (for profit calculation)
    unit_cost DECIMAL(12,2),
    
    -- Product Snapshot (immutable record of product at purchase time)
    product_snapshot JSONB NOT NULL,
    -- {
    --   "name": "Nike Dri-FIT T-Shirt",
    --   "slug": "nike-dri-fit-tshirt",
    --   "description": "Premium cotton...",
    --   "image_url": "https://...",
    --   "images": ["url1", "url2"],
    --   "variant_name": "Black / M",
    --   "options": {"color": "Black", "size": "M"},
    --   "brand": "Nike",
    --   "brand_id": "uuid",
    --   "category": "Men's T-Shirts",
    --   "category_id": "uuid",
    --   "seller_name": "Nike Official",
    --   "weight": 0.2,
    --   "dimensions": {"length": 30, "width": 20, "height": 2}
    -- }
    
    -- Item Status
    status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'confirmed', 'processing', 'shipped', 
    -- 'delivered', 'cancelled', 'returned', 'refunded'
    
    -- Fulfillment
    fulfillment_status VARCHAR(30) DEFAULT 'unfulfilled',
    -- 'unfulfilled', 'partially_fulfilled', 'fulfilled'
    fulfilled_quantity INTEGER DEFAULT 0,
    
    -- Return/Refund
    return_status VARCHAR(30),
    -- 'requested', 'approved', 'rejected', 'received', 'refunded'
    return_reason TEXT,
    return_requested_at TIMESTAMP WITH TIME ZONE,
    returned_quantity INTEGER DEFAULT 0,
    refunded_amount DECIMAL(12,2) DEFAULT 0.00,
    
    -- Gift
    is_gift BOOLEAN DEFAULT false,
    gift_message TEXT,
    
    -- Personalization
    personalization JSONB DEFAULT '{}',
    
    -- Shipment Reference
    shipment_id UUID,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT positive_quantity CHECK (quantity > 0),
    CONSTRAINT positive_prices CHECK (unit_price >= 0 AND line_total >= 0)
);

CREATE TRIGGER trigger_order_items_updated_at
    BEFORE UPDATE ON order_items
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 3. Seller Orders Table (Marketplace)

```sql
-- Split order by seller for marketplace fulfillment
CREATE TABLE seller_orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    seller_id UUID NOT NULL, -- Reference to Seller
    
    -- Seller Order Number
    seller_order_number VARCHAR(50) NOT NULL UNIQUE,
    -- Format: AMC-2026-0000001-S1
    
    -- Status (independent from main order)
    status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'accepted', 'processing', 'ready_to_ship',
    -- 'shipped', 'delivered', 'cancelled'
    
    -- Pricing for this seller
    subtotal DECIMAL(12,2) NOT NULL,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    shipping_amount DECIMAL(12,2) DEFAULT 0.00,
    tax_amount DECIMAL(12,2) DEFAULT 0.00,
    total DECIMAL(12,2) NOT NULL,
    
    -- Commission
    commission_rate DECIMAL(5,2) NOT NULL, -- Percentage
    commission_amount DECIMAL(12,2) NOT NULL,
    
    -- Seller Payout
    payout_amount DECIMAL(12,2) NOT NULL, -- total - commission
    payout_status VARCHAR(20) DEFAULT 'pending',
    -- 'pending', 'processing', 'completed', 'on_hold'
    payout_id UUID, -- Reference to payout transaction
    payout_at TIMESTAMP WITH TIME ZONE,
    
    -- Item Count
    item_count INTEGER DEFAULT 0,
    
    -- Shipping
    shipping_label_url VARCHAR(500),
    tracking_number VARCHAR(100),
    carrier VARCHAR(100),
    
    -- Response Time
    accepted_at TIMESTAMP WITH TIME ZONE,
    response_deadline TIMESTAMP WITH TIME ZONE, -- Auto-cancel if not accepted
    
    -- Notes
    seller_notes TEXT,
    admin_notes TEXT,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    shipped_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE,
    
    -- Unique seller per order
    CONSTRAINT unique_seller_per_order UNIQUE (order_id, seller_id)
);

CREATE TRIGGER trigger_seller_orders_updated_at
    BEFORE UPDATE ON seller_orders
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 4. Order Addresses Table

```sql
-- Snapshot of addresses at time of order (immutable)
CREATE TABLE order_addresses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    
    -- Address Type
    address_type VARCHAR(20) NOT NULL, -- 'shipping', 'billing'
    
    -- Original Address Reference
    original_address_id UUID, -- Reference if from saved addresses
    
    -- Address Snapshot (immutable)
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(255),
    company VARCHAR(200),
    
    address_line1 VARCHAR(255) NOT NULL,
    address_line2 VARCHAR(255),
    landmark VARCHAR(255),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(100) NOT NULL,
    postal_code VARCHAR(20) NOT NULL,
    country VARCHAR(100) DEFAULT 'India',
    country_code VARCHAR(2) DEFAULT 'IN',
    
    -- GST
    gstin VARCHAR(15),
    
    -- Verification
    is_verified BOOLEAN DEFAULT false,
    
    -- Coordinates
    coordinates JSONB, -- {"lat": 28.6139, "lng": 77.2090}
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Unique type per order
    CONSTRAINT unique_address_type_per_order UNIQUE (order_id, address_type)
);
```

### 5. Order Status History Table

```sql
CREATE TABLE order_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    seller_order_id UUID REFERENCES seller_orders(id) ON DELETE CASCADE,
    
    -- Status Change
    from_status VARCHAR(30),
    to_status VARCHAR(30) NOT NULL,
    
    -- Who Made the Change
    changed_by_type VARCHAR(20) NOT NULL, -- 'system', 'customer', 'seller', 'admin'
    changed_by_id UUID, -- User/Admin ID
    changed_by_name VARCHAR(200),
    
    -- Reason
    reason TEXT,
    
    -- Additional Context
    metadata JSONB DEFAULT '{}',
    -- {"ip_address": "...", "source": "admin_panel", "automated": true}
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Auto-log status changes
CREATE OR REPLACE FUNCTION log_order_status_change()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.status IS DISTINCT FROM NEW.status THEN
        INSERT INTO order_status_history (
            order_id, from_status, to_status, changed_by_type, reason
        ) VALUES (
            NEW.id, OLD.status, NEW.status, 'system', NULL
        );
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_log_order_status
    AFTER UPDATE ON orders
    FOR EACH ROW
    WHEN (OLD.status IS DISTINCT FROM NEW.status)
    EXECUTE FUNCTION log_order_status_change();
```

### 6. Shipments Table

```sql
CREATE TABLE shipments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    seller_order_id UUID REFERENCES seller_orders(id),
    
    -- Shipment Identifier
    shipment_number VARCHAR(50) NOT NULL UNIQUE,
    
    -- Carrier Details
    carrier VARCHAR(100) NOT NULL, -- 'delhivery', 'bluedart', 'fedex'
    carrier_service VARCHAR(100), -- 'express', 'standard'
    tracking_number VARCHAR(100),
    tracking_url VARCHAR(500),
    
    -- Status
    status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'label_created', 'picked_up', 'in_transit',
    -- 'out_for_delivery', 'delivered', 'failed', 'returned'
    
    -- Package Details
    package_count INTEGER DEFAULT 1,
    total_weight DECIMAL(10,3), -- kg
    dimensions JSONB, -- {"length": 30, "width": 20, "height": 10, "unit": "cm"}
    
    -- Shipping Label
    label_url VARCHAR(500),
    label_created_at TIMESTAMP WITH TIME ZONE,
    
    -- Cost
    shipping_cost DECIMAL(12,2),
    insurance_cost DECIMAL(12,2) DEFAULT 0.00,
    cod_amount DECIMAL(12,2) DEFAULT 0.00, -- Cash on Delivery
    
    -- Estimated Dates
    estimated_pickup_date DATE,
    estimated_delivery_date DATE,
    
    -- Actual Dates
    picked_up_at TIMESTAMP WITH TIME ZONE,
    shipped_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE,
    
    -- Delivery Details
    delivered_to VARCHAR(200),
    delivery_signature_url VARCHAR(500),
    delivery_photo_url VARCHAR(500),
    
    -- Notes
    special_instructions TEXT,
    carrier_notes TEXT,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TRIGGER trigger_shipments_updated_at
    BEFORE UPDATE ON shipments
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 7. Shipment Tracking Table

```sql
CREATE TABLE shipment_tracking (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    shipment_id UUID NOT NULL REFERENCES shipments(id) ON DELETE CASCADE,
    
    -- Tracking Event
    status VARCHAR(50) NOT NULL,
    status_code VARCHAR(20),
    description TEXT,
    
    -- Location
    location VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    country VARCHAR(100),
    postal_code VARCHAR(20),
    
    -- Carrier Data
    carrier_status_code VARCHAR(50),
    carrier_description TEXT,
    
    -- Event Time
    event_timestamp TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- Raw Data
    raw_data JSONB, -- Original carrier response
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### 8. Order Refunds Table

```sql
CREATE TABLE order_refunds (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id),
    order_item_id UUID REFERENCES order_items(id),
    
    -- Refund Reference
    refund_number VARCHAR(50) NOT NULL UNIQUE,
    payment_refund_id UUID, -- Reference to Payment Service refund
    
    -- Type
    refund_type VARCHAR(30) NOT NULL,
    -- 'full', 'partial', 'item', 'shipping'
    
    -- Status
    status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'approved', 'processing', 'completed', 'failed', 'rejected'
    
    -- Amount
    refund_amount DECIMAL(12,2) NOT NULL,
    
    -- Reason
    reason_category VARCHAR(50),
    -- 'defective', 'wrong_item', 'not_as_described', 'changed_mind', 'late_delivery'
    reason_details TEXT,
    
    -- Evidence
    evidence JSONB DEFAULT '[]',
    -- [{"type": "image", "url": "..."}, {"type": "video", "url": "..."}]
    
    -- Processing
    processed_by UUID,
    processed_at TIMESTAMP WITH TIME ZONE,
    rejection_reason TEXT,
    
    -- Refund Method
    refund_method VARCHAR(30), -- 'original_payment', 'store_credit', 'bank_transfer'
    refund_destination VARCHAR(255), -- Last 4 digits or account hint
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE
);
```

### 9. Order Returns Table

```sql
CREATE TABLE order_returns (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders(id),
    
    -- Return Reference
    return_number VARCHAR(50) NOT NULL UNIQUE,
    
    -- Status
    status VARCHAR(30) DEFAULT 'requested',
    -- 'requested', 'approved', 'pickup_scheduled', 'picked_up',
    -- 'received', 'inspecting', 'completed', 'rejected'
    
    -- Return Type
    return_type VARCHAR(20), -- 'return', 'exchange'
    
    -- Reason
    reason_category VARCHAR(50),
    reason_details TEXT,
    
    -- Items
    items JSONB NOT NULL,
    -- [{"order_item_id": "uuid", "quantity": 1, "reason": "defective"}]
    
    -- Pickup
    pickup_address_id UUID,
    pickup_scheduled_at TIMESTAMP WITH TIME ZONE,
    pickup_completed_at TIMESTAMP WITH TIME ZONE,
    pickup_carrier VARCHAR(100),
    pickup_tracking VARCHAR(100),
    
    -- Inspection
    inspection_notes TEXT,
    inspection_completed_at TIMESTAMP WITH TIME ZONE,
    inspection_passed BOOLEAN,
    
    -- Resolution
    refund_id UUID REFERENCES order_refunds(id),
    exchange_order_id UUID, -- New order for exchange
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

---

## Order State Machine

```
                              ┌───────────────┐
                              │    PENDING    │
                              │  (Awaiting    │
                              │   Payment)    │
                              └───────┬───────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    │                 │                 │
                    ▼                 ▼                 ▼
           ┌───────────────┐ ┌───────────────┐ ┌───────────────┐
           │   CONFIRMED   │ │   CANCELLED   │ │    FAILED     │
           │  (Payment OK) │ │  (User/Admin) │ │  (Payment)    │
           └───────┬───────┘ └───────────────┘ └───────────────┘
                   │
                   ▼
           ┌───────────────┐
           │  PROCESSING   │
           │   (Seller     │
           │  preparing)   │
           └───────┬───────┘
                   │
                   ▼
           ┌───────────────┐
           │ READY_TO_SHIP │
           │   (Packed)    │
           └───────┬───────┘
                   │
                   ▼
           ┌───────────────┐
           │    SHIPPED    │────────────────┐
           │  (In transit) │                │
           └───────┬───────┘                │
                   │                        │
                   ▼                        ▼
           ┌───────────────┐       ┌───────────────┐
           │OUT_FOR_DELIVER│       │   RETURNED    │
           │    (Last      │       │  (RTO/Return) │
           │     mile)     │       └───────────────┘
           └───────┬───────┘
                   │
                   ▼
           ┌───────────────┐
           │   DELIVERED   │────────────────┐
           │   (Complete)  │                │
           └───────────────┘                │
                                            ▼
                                   ┌───────────────┐
                                   │   REFUNDED    │
                                   │ (After return)│
                                   └───────────────┘
```

### Valid Status Transitions

```sql
-- Helper function to validate status transitions
CREATE OR REPLACE FUNCTION is_valid_order_status_transition(
    from_status VARCHAR,
    to_status VARCHAR
) RETURNS BOOLEAN AS $$
BEGIN
    RETURN CASE
        WHEN from_status = 'pending' THEN 
            to_status IN ('confirmed', 'cancelled', 'failed')
        WHEN from_status = 'confirmed' THEN 
            to_status IN ('processing', 'cancelled')
        WHEN from_status = 'processing' THEN 
            to_status IN ('ready_to_ship', 'cancelled')
        WHEN from_status = 'ready_to_ship' THEN 
            to_status IN ('shipped', 'cancelled')
        WHEN from_status = 'shipped' THEN 
            to_status IN ('out_for_delivery', 'delivered', 'returned')
        WHEN from_status = 'out_for_delivery' THEN 
            to_status IN ('delivered', 'returned')
        WHEN from_status = 'delivered' THEN 
            to_status IN ('returned', 'refunded')
        WHEN from_status = 'returned' THEN 
            to_status IN ('refunded')
        ELSE FALSE
    END;
END;
$$ LANGUAGE plpgsql;
```

---

## Indexes

```sql
-- Orders
CREATE INDEX idx_orders_user ON orders(user_id);
CREATE INDEX idx_orders_number ON orders(order_number);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_orders_payment_status ON orders(payment_status);
CREATE INDEX idx_orders_created ON orders(created_at DESC);
CREATE INDEX idx_orders_user_status ON orders(user_id, status);

-- Composite for common queries
CREATE INDEX idx_orders_user_created ON orders(user_id, created_at DESC);
CREATE INDEX idx_orders_status_created ON orders(status, created_at DESC);

-- Order Items
CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_order_items_seller_order ON order_items(seller_order_id);
CREATE INDEX idx_order_items_product ON order_items(product_id);
CREATE INDEX idx_order_items_seller ON order_items(seller_id);
CREATE INDEX idx_order_items_status ON order_items(status);

-- Seller Orders
CREATE INDEX idx_seller_orders_order ON seller_orders(order_id);
CREATE INDEX idx_seller_orders_seller ON seller_orders(seller_id);
CREATE INDEX idx_seller_orders_status ON seller_orders(status);
CREATE INDEX idx_seller_orders_payout ON seller_orders(payout_status);
CREATE INDEX idx_seller_orders_seller_status ON seller_orders(seller_id, status);
CREATE INDEX idx_seller_orders_seller_created ON seller_orders(seller_id, created_at DESC);

-- Status History
CREATE INDEX idx_status_history_order ON order_status_history(order_id);
CREATE INDEX idx_status_history_created ON order_status_history(created_at DESC);

-- Shipments
CREATE INDEX idx_shipments_order ON shipments(order_id);
CREATE INDEX idx_shipments_seller_order ON shipments(seller_order_id);
CREATE INDEX idx_shipments_tracking ON shipments(tracking_number);
CREATE INDEX idx_shipments_status ON shipments(status);

-- Shipment Tracking
CREATE INDEX idx_tracking_shipment ON shipment_tracking(shipment_id);
CREATE INDEX idx_tracking_timestamp ON shipment_tracking(event_timestamp DESC);

-- Refunds
CREATE INDEX idx_refunds_order ON order_refunds(order_id);
CREATE INDEX idx_refunds_status ON order_refunds(status);

-- Returns
CREATE INDEX idx_returns_order ON order_returns(order_id);
CREATE INDEX idx_returns_status ON order_returns(status);
```

---

## Sample Data

```sql
-- Create order
INSERT INTO orders (
    id, user_id, customer_email, customer_phone, customer_name,
    status, payment_status, payment_method,
    subtotal, discount_amount, shipping_amount, tax_amount, total,
    item_count, seller_count
) VALUES (
    'oooooooo-0000-0000-0000-000000000001',
    'uuuuuuuu-0000-0000-0000-000000000001',
    'john.doe@example.com',
    '+919876543210',
    'John Doe',
    'confirmed',
    'captured',
    'card',
    2598.00,
    0.00,
    99.00,
    467.64,
    3164.64,
    2,
    1
);

-- Create order items
INSERT INTO order_items (
    order_id, product_id, variant_id, seller_id, sku,
    quantity, unit_price, tax_amount, line_total,
    product_snapshot, status
) VALUES (
    'oooooooo-0000-0000-0000-000000000001',
    'pppppppp-0000-0000-0000-000000000001',
    'vvvvvvvv-0000-0000-0000-000000000002',
    'ssssssss-0000-0000-0000-000000000001',
    'NIKE-DF-BLK-M',
    2,
    1299.00,
    467.64,
    2598.00,
    '{
        "name": "Nike Dri-FIT T-Shirt",
        "slug": "nike-dri-fit-tshirt",
        "image_url": "https://cdn.amcart.com/products/nike-tshirt.jpg",
        "variant_name": "Black / M",
        "options": {"color": "Black", "size": "M"},
        "brand": "Nike",
        "category": "Men''s T-Shirts"
    }'::jsonb,
    'confirmed'
);

-- Create order addresses
INSERT INTO order_addresses (
    order_id, address_type, first_name, last_name, phone,
    address_line1, city, state, postal_code, country
) VALUES 
(
    'oooooooo-0000-0000-0000-000000000001',
    'shipping',
    'John', 'Doe', '+919876543210',
    '123 Main Street, Apartment 4B', 'Mumbai', 'Maharashtra', '400001', 'India'
),
(
    'oooooooo-0000-0000-0000-000000000001',
    'billing',
    'John', 'Doe', '+919876543210',
    '123 Main Street, Apartment 4B', 'Mumbai', 'Maharashtra', '400001', 'India'
);
```

---

## Common Queries

### 1. Get Order with All Details

```sql
SELECT 
    o.*,
    (
        SELECT json_agg(
            json_build_object(
                'id', oi.id,
                'sku', oi.sku,
                'quantity', oi.quantity,
                'unit_price', oi.unit_price,
                'line_total', oi.line_total,
                'status', oi.status,
                'product_snapshot', oi.product_snapshot
            ) ORDER BY oi.created_at
        )
        FROM order_items oi WHERE oi.order_id = o.id
    ) as items,
    (
        SELECT json_build_object(
            'first_name', oa.first_name,
            'last_name', oa.last_name,
            'phone', oa.phone,
            'address', oa.address_line1 || COALESCE(', ' || oa.address_line2, ''),
            'city', oa.city,
            'state', oa.state,
            'postal_code', oa.postal_code
        )
        FROM order_addresses oa 
        WHERE oa.order_id = o.id AND oa.address_type = 'shipping'
    ) as shipping_address,
    (
        SELECT json_agg(
            json_build_object(
                'status', osh.to_status,
                'changed_at', osh.created_at,
                'reason', osh.reason
            ) ORDER BY osh.created_at DESC
        )
        FROM order_status_history osh WHERE osh.order_id = o.id
    ) as status_history
FROM orders o
WHERE o.order_number = 'AMC-2026-0000001';
```

### 2. Get User's Orders (Paginated)

```sql
SELECT 
    o.id,
    o.order_number,
    o.status,
    o.total,
    o.item_count,
    o.created_at,
    (
        SELECT json_agg(
            json_build_object(
                'name', oi.product_snapshot->>'name',
                'image', oi.product_snapshot->>'image_url',
                'quantity', oi.quantity
            )
        )
        FROM order_items oi WHERE oi.order_id = o.id
        LIMIT 3
    ) as item_preview
FROM orders o
WHERE o.user_id = 'uuuuuuuu-0000-0000-0000-000000000001'
ORDER BY o.created_at DESC
LIMIT 10 OFFSET 0;
```

### 3. Seller Dashboard Orders

```sql
SELECT 
    so.id,
    so.seller_order_number,
    so.status,
    so.total,
    so.commission_amount,
    so.payout_amount,
    so.payout_status,
    so.created_at,
    o.order_number,
    o.customer_name,
    (
        SELECT json_build_object(
            'city', oa.city,
            'state', oa.state
        )
        FROM order_addresses oa 
        WHERE oa.order_id = o.id AND oa.address_type = 'shipping'
    ) as shipping_location,
    (
        SELECT COUNT(*) FROM order_items oi WHERE oi.seller_order_id = so.id
    ) as item_count
FROM seller_orders so
JOIN orders o ON so.order_id = o.id
WHERE so.seller_id = 'ssssssss-0000-0000-0000-000000000001'
    AND so.status IN ('pending', 'accepted', 'processing')
ORDER BY so.created_at DESC;
```

---

## EF Core Entity Models

### Order Entity

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmCart.OrderService.Domain.Entities;

[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Column("order_number")]
    public string OrderNumber { get; set; } = null!;
    
    [Column("user_id")]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(255)]
    [Column("customer_email")]
    public string CustomerEmail { get; set; } = null!;
    
    [MaxLength(20)]
    [Column("customer_phone")]
    public string? CustomerPhone { get; set; }
    
    [Required]
    [MaxLength(200)]
    [Column("customer_name")]
    public string CustomerName { get; set; } = null!;
    
    [Column("cart_id")]
    public Guid? CartId { get; set; }
    
    [MaxLength(30)]
    [Column("status")]
    public string Status { get; set; } = "pending";
    
    [MaxLength(30)]
    [Column("payment_status")]
    public string PaymentStatus { get; set; } = "pending";
    
    [MaxLength(50)]
    [Column("payment_method")]
    public string? PaymentMethod { get; set; }
    
    [Column("payment_id")]
    public Guid? PaymentId { get; set; }
    
    [MaxLength(3)]
    [Column("currency")]
    public string Currency { get; set; } = "INR";
    
    [Column("subtotal", TypeName = "decimal(12,2)")]
    public decimal Subtotal { get; set; }
    
    [Column("discount_amount", TypeName = "decimal(12,2)")]
    public decimal DiscountAmount { get; set; } = 0;
    
    [Column("shipping_amount", TypeName = "decimal(12,2)")]
    public decimal ShippingAmount { get; set; } = 0;
    
    [Column("tax_amount", TypeName = "decimal(12,2)")]
    public decimal TaxAmount { get; set; } = 0;
    
    [Column("total", TypeName = "decimal(12,2)")]
    public decimal Total { get; set; }
    
    [Column("tax_breakdown", TypeName = "jsonb")]
    public TaxBreakdown TaxBreakdown { get; set; } = new();
    
    [Column("applied_coupons", TypeName = "jsonb")]
    public List<AppliedCoupon> AppliedCoupons { get; set; } = new();
    
    [Column("item_count")]
    public int ItemCount { get; set; } = 0;
    
    [Column("seller_count")]
    public int SellerCount { get; set; } = 0;
    
    [MaxLength(100)]
    [Column("shipping_method")]
    public string? ShippingMethod { get; set; }
    
    [Column("estimated_delivery_date")]
    public DateOnly? EstimatedDeliveryDate { get; set; }
    
    [Column("actual_delivery_date")]
    public DateOnly? ActualDeliveryDate { get; set; }
    
    [Column("order_notes")]
    public string? OrderNotes { get; set; }
    
    [Column("gift_message")]
    public string? GiftMessage { get; set; }
    
    [Column("is_gift")]
    public bool IsGift { get; set; } = false;
    
    [MaxLength(50)]
    [Column("invoice_number")]
    public string? InvoiceNumber { get; set; }
    
    [Column("invoice_url")]
    public string? InvoiceUrl { get; set; }
    
    [Column("cancellation_reason")]
    public string? CancellationReason { get; set; }
    
    [MaxLength(20)]
    [Column("cancelled_by")]
    public string? CancelledBy { get; set; }
    
    [Column("cancelled_at")]
    public DateTime? CancelledAt { get; set; }
    
    [Column("metadata", TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("confirmed_at")]
    public DateTime? ConfirmedAt { get; set; }
    
    [Column("shipped_at")]
    public DateTime? ShippedAt { get; set; }
    
    [Column("delivered_at")]
    public DateTime? DeliveredAt { get; set; }
    
    // Navigation
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<SellerOrder> SellerOrders { get; set; } = new List<SellerOrder>();
    public virtual ICollection<OrderAddress> Addresses { get; set; } = new List<OrderAddress>();
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    
    // Computed
    [NotMapped]
    public OrderAddress? ShippingAddress => Addresses.FirstOrDefault(a => a.AddressType == "shipping");
    
    [NotMapped]
    public OrderAddress? BillingAddress => Addresses.FirstOrDefault(a => a.AddressType == "billing");
}

public class TaxBreakdown
{
    public decimal Cgst { get; set; }
    public decimal Sgst { get; set; }
    public decimal Igst { get; set; }
}

public class AppliedCoupon
{
    public string Code { get; set; } = null!;
    public decimal Discount { get; set; }
}
```

---

## Event Integration

### Order Events

```csharp
public record OrderCreatedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid UserId,
    decimal Total,
    int ItemCount,
    DateTime CreatedAt
);

public record OrderConfirmedEvent(
    Guid OrderId,
    string OrderNumber,
    Guid PaymentId,
    DateTime ConfirmedAt
);

public record OrderShippedEvent(
    Guid OrderId,
    string OrderNumber,
    string TrackingNumber,
    string Carrier,
    DateTime ShippedAt
);

public record OrderDeliveredEvent(
    Guid OrderId,
    string OrderNumber,
    DateTime DeliveredAt
);

public record OrderCancelledEvent(
    Guid OrderId,
    string OrderNumber,
    string CancelledBy,
    string Reason,
    DateTime CancelledAt
);
```

---

## Summary

| Table | Purpose | Key Features |
|-------|---------|--------------|
| `orders` | Main order data | Order number, totals, status |
| `order_items` | Line items | Product snapshots, fulfillment |
| `seller_orders` | Marketplace split | Commission, payout tracking |
| `order_addresses` | Address snapshots | Immutable billing/shipping |
| `order_status_history` | Audit trail | All status changes |
| `shipments` | Shipping info | Carrier, tracking |
| `shipment_tracking` | Tracking events | Location updates |
| `order_refunds` | Refund records | Amount, status, method |
| `order_returns` | Return requests | Pickup, inspection |

This schema supports:
- ✅ Multi-seller marketplace
- ✅ Complete order lifecycle
- ✅ Status tracking with history
- ✅ Shipment tracking
- ✅ Returns and refunds
- ✅ Commission calculation
- ✅ Invoice generation
- ✅ Event-driven architecture

