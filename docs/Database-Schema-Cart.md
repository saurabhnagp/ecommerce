# Cart Database Schema

## PostgreSQL + Redis Hybrid for AmCart E-commerce

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Tables Schema](#tables-schema)
4. [Redis Schema](#redis-schema)
5. [Indexes](#indexes)
6. [Sample Data](#sample-data)
7. [Common Queries](#common-queries)
8. [EF Core Entity Models](#ef-core-entity-models)
9. [Cart Operations](#cart-operations)

---

## Overview

### Design Principles

| Principle | Implementation |
|-----------|----------------|
| **Hybrid Storage** | Redis for fast access, PostgreSQL for persistence |
| **Guest Cart Support** | Anonymous users can have carts (session-based) |
| **Cart Merge** | Guest cart merges with user cart on login |
| **Price Snapshot** | Store price at time of adding for comparison |
| **Expiration** | Guest carts expire, user carts persist |

### Why Hybrid Approach?

| Scenario | Storage | Reason |
|----------|---------|--------|
| Active cart operations | Redis | Sub-millisecond reads |
| Cart persistence | PostgreSQL | Durability, analytics |
| Guest carts | Redis only | Temporary, expires |
| User carts | Both | Fast + durable |
| Abandoned cart analysis | PostgreSQL | Historical queries |

---

## Architecture

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         Cart Architecture                                     │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User/Guest                                                                 │
│       │                                                                      │
│       ▼                                                                      │
│  ┌─────────────────┐                                                        │
│  │   Cart Service  │                                                        │
│  │     (.NET)      │                                                        │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           ├──────────────────────────────────────────┐                      │
│           │                                          │                      │
│           ▼                                          ▼                      │
│  ┌─────────────────┐                      ┌─────────────────────┐           │
│  │     Redis       │                      │     PostgreSQL      │           │
│  │  (ElastiCache)  │                      │       (RDS)         │           │
│  │                 │                      │                     │           │
│  │  ┌───────────┐  │   Sync on           │  ┌───────────────┐  │           │
│  │  │ cart:{id} │  │──────────────────▶  │  │    carts      │  │           │
│  │  │ {items}   │  │   - Add/Update      │  └───────────────┘  │           │
│  │  └───────────┘  │   - Checkout        │         │           │           │
│  │                 │                      │         ▼           │           │
│  │  ┌───────────┐  │                      │  ┌───────────────┐  │           │
│  │  │ guest:    │  │                      │  │  cart_items   │  │           │
│  │  │ {session} │  │                      │  └───────────────┘  │           │
│  │  └───────────┘  │                      │                     │           │
│  │                 │                      │  ┌───────────────┐  │           │
│  │  TTL: 30 days   │                      │  │abandoned_carts│  │           │
│  │  (user carts)   │                      │  └───────────────┘  │           │
│  │                 │                      │                     │           │
│  │  TTL: 7 days    │                      │                     │           │
│  │  (guest carts)  │                      │                     │           │
│  └─────────────────┘                      └─────────────────────┘           │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## Entity Relationship Diagram

### PostgreSQL Tables Relationships

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              Cart Database ERD                                   │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│                           ┌─────────────────────┐                               │
│                           │   User Service      │                               │
│                           │   (External)        │                               │
│                           └──────────┬──────────┘                               │
│                                      │                                          │
│                                      │ user_id (Reference)                      │
│                                      ▼                                          │
│   ┌──────────────────────────────────────────────────────────────────────────┐ │
│   │                              carts                                        │ │
│   ├──────────────────────────────────────────────────────────────────────────┤ │
│   │ id (PK)                    UUID                                          │ │
│   │ user_id                    UUID (nullable) ──────────────────▶ users     │ │
│   │ session_id                 VARCHAR(255) (for guests)                     │ │
│   │ status                     VARCHAR(20)                                   │ │
│   │ currency                   VARCHAR(3)                                    │ │
│   │ subtotal                   DECIMAL(12,2)                                 │ │
│   │ discount_amount            DECIMAL(12,2)                                 │ │
│   │ tax_amount                 DECIMAL(12,2)                                 │ │
│   │ shipping_estimate          DECIMAL(12,2)                                 │ │
│   │ total                      DECIMAL(12,2)                                 │ │
│   │ item_count                 INTEGER                                       │ │
│   │ applied_coupons            JSONB                                         │ │
│   │ shipping_address_id        UUID ─────────────────────────▶ addresses     │ │
│   │ order_id                   UUID ─────────────────────────▶ orders        │ │
│   │ created_at, updated_at     TIMESTAMP                                     │ │
│   └───────────────────────────────┬──────────────────────────────────────────┘ │
│                                   │                                             │
│               ┌───────────────────┼───────────────────┐                         │
│               │                   │                   │                         │
│               ▼ 1:N               ▼ 1:N               ▼ 1:1                      │
│   ┌───────────────────┐ ┌─────────────────┐ ┌────────────────────┐             │
│   │    cart_items     │ │  cart_coupons   │ │  abandoned_carts   │             │
│   ├───────────────────┤ ├─────────────────┤ ├────────────────────┤             │
│   │ id (PK)           │ │ id (PK)         │ │ id (PK)            │             │
│   │ cart_id (FK) ─────┼─│ cart_id (FK) ───┼─│ cart_id (FK) ──────┤             │
│   │ product_id ───────┼▶│ coupon_id       │ │ user_id            │             │
│   │ variant_id ───────┼▶│ coupon_code     │ │ cart_total         │             │
│   │ seller_id ────────┼▶│ discount_type   │ │ item_count         │             │
│   │ quantity          │ │ discount_value  │ │ items_summary      │             │
│   │ unit_price        │ │ discount_amount │ │ abandoned_at_step  │             │
│   │ line_total        │ │ is_valid        │ │ recovery_emails    │             │
│   │ product_snapshot  │ │ applied_at      │ │ recovered          │             │
│   │ is_available      │ └─────────────────┘ │ recovered_order_id │             │
│   │ is_saved_for_later│                     └────────────────────┘             │
│   │ added_at          │                                                         │
│   └───────────────────┘                                                         │
│           │                                                                     │
│           │ References (External Services)                                      │
│           ▼                                                                     │
│   ┌───────────────────────────────────────────────────────────────────────┐    │
│   │                        Product Service                                 │    │
│   │  ┌─────────────┐   ┌─────────────────┐   ┌─────────────┐              │    │
│   │  │  products   │   │ product_variants│   │   sellers   │              │    │
│   │  │   (id)      │   │     (id)        │   │    (id)     │              │    │
│   │  └─────────────┘   └─────────────────┘   └─────────────┘              │    │
│   └───────────────────────────────────────────────────────────────────────┘    │
│                                                                                 │
│                                                                                 │
│   ┌───────────────────────────────────────────────────────────────────────┐    │
│   │                      saved_for_later                                   │    │
│   ├───────────────────────────────────────────────────────────────────────┤    │
│   │ id (PK)                    UUID                                       │    │
│   │ user_id                    UUID NOT NULL ────────────────▶ users      │    │
│   │ product_id                 UUID NOT NULL ────────────────▶ products   │    │
│   │ variant_id                 UUID NOT NULL ────────────────▶ variants   │    │
│   │ price_when_saved           DECIMAL(12,2)                              │    │
│   │ product_snapshot           JSONB                                      │    │
│   │ saved_at                   TIMESTAMP                                  │    │
│   │                                                                       │    │
│   │ UNIQUE (user_id, variant_id)                                          │    │
│   └───────────────────────────────────────────────────────────────────────┘    │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Detailed Table Relationships

```
                    ┌─────────────────────────────────────────┐
                    │                 carts                    │
                    ├─────────────────────────────────────────┤
                    │ PK: id                                  │
                    │ UK: (user_id) WHERE status='active'     │
                    │ UK: (session_id) WHERE status='active'  │
                    └────────────────────┬────────────────────┘
                                         │
          ┌──────────────────────────────┼──────────────────────────────┐
          │                              │                              │
          ▼                              ▼                              ▼
┌─────────────────────┐      ┌─────────────────────┐      ┌─────────────────────┐
│     cart_items      │      │    cart_coupons     │      │   abandoned_carts   │
├─────────────────────┤      ├─────────────────────┤      ├─────────────────────┤
│ PK: id              │      │ PK: id              │      │ PK: id              │
│ FK: cart_id ────────┼──────│ FK: cart_id ────────┼──────│ FK: cart_id         │
│ UK: (cart_id,       │      │ UK: (cart_id,       │      │                     │
│      variant_id)    │      │      coupon_code)   │      │                     │
│                     │      │                     │      │                     │
│ Computed:           │      │                     │      │                     │
│ line_total =        │      │                     │      │                     │
│ quantity * unit_price│     │                     │      │                     │
└─────────────────────┘      └─────────────────────┘      └─────────────────────┘
```

### Cart Item Product Snapshot (JSONB Structure)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        cart_items.product_snapshot                           │
├─────────────────────────────────────────────────────────────────────────────┤
│ {                                                                           │
│   "name": "Nike Dri-FIT T-Shirt",        ◀── Product name at add time      │
│   "slug": "nike-dri-fit-tshirt",                                            │
│   "image_url": "https://cdn.amcart.com/...",                                │
│   "sku": "NIKE-DF-BLK-M",                ◀── Variant SKU                   │
│   "variant_name": "Black / M",            ◀── Human-readable variant       │
│   "options": {                                                              │
│     "color": "Black",                                                       │
│     "size": "M"                                                             │
│   },                                                                        │
│   "brand": "Nike",                                                          │
│   "category": "Men's T-Shirts"                                              │
│ }                                                                           │
│                                                                             │
│ Purpose: Display cart items without calling Product Service                 │
│ Updated: When product changes or during price refresh                       │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Redis Cache Structure (ERD)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Redis Cache Schema                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   User/Session Mapping                        Cart Data                     │
│   ──────────────────                          ─────────                     │
│                                                                             │
│   ┌───────────────────────┐                  ┌───────────────────────────┐ │
│   │ user_cart:{user_id}   │                  │     cart:{cart_id}        │ │
│   │ Type: String          │──────────────────│     Type: Hash            │ │
│   │ Value: cart_id        │                  ├───────────────────────────┤ │
│   │ TTL: 30 days          │                  │ id: "uuid"                │ │
│   └───────────────────────┘                  │ user_id: "uuid"           │ │
│                                              │ session_id: "string"      │ │
│   ┌───────────────────────┐                  │ status: "active"          │ │
│   │ guest_cart:{session}  │                  │ subtotal: "1299.00"       │ │
│   │ Type: String          │──────────────────│ discount_amount: "0.00"   │ │
│   │ Value: cart_id        │                  │ total: "1299.00"          │ │
│   │ TTL: 7 days           │                  │ item_count: "2"           │ │
│   └───────────────────────┘                  │ updated_at: "ISO8601"     │ │
│                                              │ TTL: 7-30 days            │ │
│                                              └─────────────┬─────────────┘ │
│                                                            │               │
│                                                            │ 1:N          │
│                                                            ▼               │
│                                              ┌───────────────────────────┐ │
│                                              │ cart:{cart_id}:items      │ │
│                                              │ Type: List                │ │
│                                              ├───────────────────────────┤ │
│                                              │ [                         │ │
│                                              │   {                       │ │
│                                              │     "id": "uuid",         │ │
│                                              │     "product_id": "uuid", │ │
│                                              │     "variant_id": "uuid", │ │
│                                              │     "quantity": 2,        │ │
│                                              │     "unit_price": 1299,   │ │
│                                              │     "line_total": 2598,   │ │
│                                              │     "product_snapshot":{} │ │
│                                              │   },                      │ │
│                                              │   ...                     │ │
│                                              │ ]                         │ │
│                                              │ TTL: Same as cart         │ │
│                                              └───────────────────────────┘ │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Cart Lifecycle State Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Cart Status Transitions                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                              ┌──────────┐                                   │
│                              │  START   │                                   │
│                              └────┬─────┘                                   │
│                                   │                                         │
│                                   ▼                                         │
│    ┌──────────────────────────────────────────────────────────────────┐    │
│    │                           ACTIVE                                  │    │
│    │  • Guest creates cart (session_id)                               │    │
│    │  • User creates cart (user_id)                                   │    │
│    │  • Items can be added/removed                                    │    │
│    │  • Coupons can be applied                                        │    │
│    └───────┬──────────────────┬─────────────────┬────────────────────┘    │
│            │                  │                 │                          │
│            │ User logs in     │ Checkout        │ No activity              │
│            │ (guest cart)     │ completed       │ > 30 days                │
│            ▼                  ▼                 ▼                          │
│    ┌──────────────┐   ┌──────────────┐   ┌──────────────┐                 │
│    │   MERGED     │   │  CONVERTED   │   │  ABANDONED   │                 │
│    │              │   │              │   │              │                 │
│    │ Guest cart   │   │ Cart became  │   │ Tracked for  │                 │
│    │ merged into  │   │ an order     │   │ recovery     │                 │
│    │ user cart    │   │              │   │ emails       │                 │
│    └──────────────┘   └──────┬───────┘   └──────────────┘                 │
│                              │                                             │
│                              ▼                                             │
│                       ┌──────────────┐                                     │
│                       │   orders     │                                     │
│                       │  (External)  │                                     │
│                       └──────────────┘                                     │
│                                                                             │
│    ┌──────────────┐                                                        │
│    │   EXPIRED    │  ◀── Guest carts after TTL (7 days)                   │
│    │              │      No items preserved                                │
│    └──────────────┘                                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Data Flow: Add Item to Cart

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Add Item to Cart Flow                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   Client                 Cart Service              Redis         PostgreSQL │
│     │                         │                      │                │     │
│     │  POST /cart/items       │                      │                │     │
│     │  {product_id,           │                      │                │     │
│     │   variant_id, qty}      │                      │                │     │
│     │ ───────────────────────▶│                      │                │     │
│     │                         │                      │                │     │
│     │                         │  GET cart:{id}       │                │     │
│     │                         │ ────────────────────▶│                │     │
│     │                         │                      │                │     │
│     │                         │◀──── cart data ──────│                │     │
│     │                         │                      │                │     │
│     │                         │  Check stock/price   │                │     │
│     │                         │ ─────────────────────┼───────────────▶│     │
│     │                         │                      │ Product Service│     │
│     │                         │                      │                │     │
│     │                         │  RPUSH items         │                │     │
│     │                         │ ────────────────────▶│                │     │
│     │                         │                      │                │     │
│     │                         │  HSET cart totals    │                │     │
│     │                         │ ────────────────────▶│                │     │
│     │                         │                      │                │     │
│     │                         │  INSERT cart_items   │                │     │
│     │                         │ ─────────────────────┼───────────────▶│     │
│     │                         │                      │                │     │
│     │                         │  (Async) Trigger     │                │     │
│     │                         │  updates cart totals │                │     │
│     │                         │                      │                │     │
│     │◀─── Updated Cart ───────│                      │                │     │
│     │                         │                      │                │     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Relationships Summary

| Parent | Child | Relationship | Cascade |
|--------|-------|--------------|---------|
| `carts` | `cart_items` | 1:N | ON DELETE CASCADE |
| `carts` | `cart_coupons` | 1:N | ON DELETE CASCADE |
| `carts` | `abandoned_carts` | 1:1 | Reference only |
| `carts.user_id` | User Service | Reference | External |
| `cart_items.product_id` | Product Service | Reference | External |
| `cart_items.variant_id` | Product Service | Reference | External |
| `cart_items.seller_id` | Product Service | Reference | External |

### Unique Constraints

| Table | Constraint | Purpose |
|-------|------------|---------|
| `carts` | `(user_id) WHERE status='active'` | One active cart per user |
| `carts` | `(session_id) WHERE status='active'` | One active cart per guest |
| `cart_items` | `(cart_id, variant_id)` | One entry per variant in cart |
| `cart_coupons` | `(cart_id, coupon_code)` | One application per coupon |
| `saved_for_later` | `(user_id, variant_id)` | One saved entry per variant |

---

## Tables Schema

### 1. Carts Table

```sql
CREATE TABLE carts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Owner (nullable for guest carts)
    user_id UUID, -- Reference to User Service
    
    -- Guest Identification
    session_id VARCHAR(255), -- For guest carts
    device_fingerprint VARCHAR(255),
    
    -- Cart Status
    status VARCHAR(20) DEFAULT 'active',
    -- 'active', 'merged', 'converted', 'abandoned', 'expired'
    
    -- Currency
    currency VARCHAR(3) DEFAULT 'INR',
    
    -- Totals (calculated, cached)
    subtotal DECIMAL(12,2) DEFAULT 0.00,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    tax_amount DECIMAL(12,2) DEFAULT 0.00,
    shipping_estimate DECIMAL(12,2) DEFAULT 0.00,
    total DECIMAL(12,2) DEFAULT 0.00,
    
    -- Item Count
    item_count INTEGER DEFAULT 0,
    unique_item_count INTEGER DEFAULT 0,
    
    -- Applied Discounts
    applied_coupons JSONB DEFAULT '[]',
    -- [{"code": "SAVE10", "discount_type": "percentage", "value": 10, "discount_amount": 129.90}]
    
    -- Shipping Info (selected during checkout)
    shipping_address_id UUID,
    shipping_method_id UUID,
    
    -- Checkout Progress
    checkout_started_at TIMESTAMP WITH TIME ZONE,
    checkout_step VARCHAR(50), -- 'address', 'shipping', 'payment', 'review'
    
    -- Metadata
    metadata JSONB DEFAULT '{}',
    -- {"source": "mobile_app", "utm_campaign": "summer_sale"}
    
    -- Notes
    notes TEXT,
    gift_message TEXT,
    is_gift BOOLEAN DEFAULT false,
    
    -- Converted Order Reference
    order_id UUID, -- Set when cart is converted to order
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_activity_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP WITH TIME ZONE, -- For guest carts
    converted_at TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT valid_status CHECK (status IN ('active', 'merged', 'converted', 'abandoned', 'expired')),
    CONSTRAINT positive_totals CHECK (subtotal >= 0 AND total >= 0)
);

-- Each user can have only one active cart
CREATE UNIQUE INDEX idx_one_active_cart_per_user 
    ON carts(user_id) 
    WHERE status = 'active' AND user_id IS NOT NULL;

-- Each session can have only one active guest cart
CREATE UNIQUE INDEX idx_one_active_guest_cart 
    ON carts(session_id) 
    WHERE status = 'active' AND user_id IS NULL AND session_id IS NOT NULL;

-- Trigger for updated_at
CREATE TRIGGER trigger_carts_updated_at
    BEFORE UPDATE ON carts
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 2. Cart Items Table

```sql
CREATE TABLE cart_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cart_id UUID NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
    
    -- Product Reference
    product_id UUID NOT NULL, -- Reference to Product Service
    variant_id UUID NOT NULL, -- Reference to Product Variant
    seller_id UUID NOT NULL,  -- Reference to Seller
    
    -- Quantity
    quantity INTEGER NOT NULL DEFAULT 1,
    
    -- Pricing at time of adding (snapshot)
    unit_price DECIMAL(12,2) NOT NULL,          -- Price when added
    compare_at_price DECIMAL(12,2),             -- Original price (for showing discount)
    
    -- Current pricing (updated periodically)
    current_unit_price DECIMAL(12,2),           -- Current price
    price_changed BOOLEAN DEFAULT false,         -- Flag if price changed
    price_change_amount DECIMAL(12,2),          -- Difference
    
    -- Calculated
    line_total DECIMAL(12,2) GENERATED ALWAYS AS (quantity * unit_price) STORED,
    
    -- Product Snapshot (denormalized for display)
    product_snapshot JSONB NOT NULL,
    -- {
    --   "name": "Nike Dri-FIT T-Shirt",
    --   "slug": "nike-dri-fit-tshirt",
    --   "image_url": "https://...",
    --   "sku": "NIKE-DF-BLK-M",
    --   "variant_name": "Black / M",
    --   "options": {"color": "Black", "size": "M"},
    --   "brand": "Nike",
    --   "category": "Men's T-Shirts"
    -- }
    
    -- Availability
    is_available BOOLEAN DEFAULT true,
    availability_message VARCHAR(255), -- "Only 2 left", "Out of stock"
    stock_at_add_time INTEGER,
    current_stock INTEGER,
    
    -- Item-level Discount
    item_discount DECIMAL(12,2) DEFAULT 0.00,
    discount_reason VARCHAR(255),
    
    -- Gift Options
    is_gift BOOLEAN DEFAULT false,
    gift_wrap BOOLEAN DEFAULT false,
    gift_message TEXT,
    
    -- Personalization
    personalization JSONB DEFAULT '{}',
    -- {"engraving": "John Doe", "custom_text": "Happy Birthday"}
    
    -- Flags
    is_saved_for_later BOOLEAN DEFAULT false,
    
    -- Timestamps
    added_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT positive_quantity CHECK (quantity > 0),
    CONSTRAINT positive_price CHECK (unit_price >= 0),
    
    -- Unique product variant per cart
    CONSTRAINT unique_cart_variant UNIQUE (cart_id, variant_id)
);

CREATE TRIGGER trigger_cart_items_updated_at
    BEFORE UPDATE ON cart_items
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Trigger to update cart totals when items change
CREATE OR REPLACE FUNCTION update_cart_totals()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE carts 
    SET 
        subtotal = (
            SELECT COALESCE(SUM(line_total), 0) 
            FROM cart_items 
            WHERE cart_id = COALESCE(NEW.cart_id, OLD.cart_id) 
            AND is_saved_for_later = false
        ),
        item_count = (
            SELECT COALESCE(SUM(quantity), 0) 
            FROM cart_items 
            WHERE cart_id = COALESCE(NEW.cart_id, OLD.cart_id)
            AND is_saved_for_later = false
        ),
        unique_item_count = (
            SELECT COUNT(*) 
            FROM cart_items 
            WHERE cart_id = COALESCE(NEW.cart_id, OLD.cart_id)
            AND is_saved_for_later = false
        ),
        last_activity_at = CURRENT_TIMESTAMP,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = COALESCE(NEW.cart_id, OLD.cart_id);
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_cart_totals
    AFTER INSERT OR UPDATE OR DELETE ON cart_items
    FOR EACH ROW
    EXECUTE FUNCTION update_cart_totals();
```

### 3. Saved for Later Table

```sql
-- Separate table for "Save for Later" functionality
CREATE TABLE saved_for_later (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL, -- Must be logged in
    
    -- Product Reference
    product_id UUID NOT NULL,
    variant_id UUID NOT NULL,
    
    -- Price when saved
    price_when_saved DECIMAL(12,2),
    
    -- Product Snapshot
    product_snapshot JSONB NOT NULL,
    
    -- Timestamps
    saved_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Unique product per user
    CONSTRAINT unique_saved_variant UNIQUE (user_id, variant_id)
);
```

### 4. Cart Coupons Table

```sql
CREATE TABLE cart_coupons (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cart_id UUID NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
    
    -- Coupon Reference
    coupon_id UUID NOT NULL, -- Reference to Coupon Service/Table
    coupon_code VARCHAR(50) NOT NULL,
    
    -- Discount Details
    discount_type VARCHAR(20) NOT NULL, -- 'percentage', 'fixed', 'free_shipping'
    discount_value DECIMAL(12,2) NOT NULL,
    discount_amount DECIMAL(12,2) NOT NULL, -- Calculated discount
    
    -- Restrictions Applied
    minimum_order_met BOOLEAN DEFAULT true,
    applicable_items JSONB, -- Item IDs this coupon applies to
    
    -- Status
    is_valid BOOLEAN DEFAULT true,
    validation_message VARCHAR(255),
    
    -- Timestamps
    applied_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- One coupon code per cart (usually)
    CONSTRAINT unique_coupon_per_cart UNIQUE (cart_id, coupon_code)
);
```

### 5. Abandoned Carts Table (Analytics)

```sql
CREATE TABLE abandoned_carts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cart_id UUID NOT NULL REFERENCES carts(id),
    user_id UUID,
    
    -- Cart Value
    cart_total DECIMAL(12,2),
    item_count INTEGER,
    
    -- Items Summary
    items_summary JSONB,
    -- [{"product_name": "...", "quantity": 2, "price": 1299}]
    
    -- Abandonment Details
    abandoned_at_step VARCHAR(50), -- 'cart', 'address', 'payment'
    time_in_cart INTERVAL, -- How long they had the cart
    
    -- Recovery Attempts
    recovery_emails_sent INTEGER DEFAULT 0,
    last_recovery_email_at TIMESTAMP WITH TIME ZONE,
    
    -- Recovery Status
    recovered BOOLEAN DEFAULT false,
    recovered_order_id UUID,
    recovered_at TIMESTAMP WITH TIME ZONE,
    
    -- User Contact
    user_email VARCHAR(255),
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

---

## Redis Schema

### Cart Hash Structure

```
Key: cart:{cart_id}
Type: Hash

Fields:
- id: UUID
- user_id: UUID (optional)
- session_id: string (for guests)
- status: active|merged|converted
- subtotal: decimal
- discount_amount: decimal
- total: decimal
- item_count: integer
- currency: INR
- updated_at: timestamp

TTL: 
- User carts: 30 days
- Guest carts: 7 days
```

### Cart Items List

```
Key: cart:{cart_id}:items
Type: List of JSON strings

Each item:
{
  "id": "uuid",
  "product_id": "uuid",
  "variant_id": "uuid",
  "seller_id": "uuid",
  "quantity": 2,
  "unit_price": 1299.00,
  "line_total": 2598.00,
  "product_snapshot": {
    "name": "Nike T-Shirt",
    "image_url": "https://...",
    "sku": "NIKE-BLK-M",
    "variant_name": "Black / M"
  },
  "is_available": true
}

TTL: Same as cart
```

### Guest Cart Mapping

```
Key: guest_cart:{session_id}
Type: String (cart_id)
TTL: 7 days
```

### User Active Cart

```
Key: user_cart:{user_id}
Type: String (cart_id)
TTL: 30 days
```

### Redis Commands Example

```redis
# Create/Update Cart
HSET cart:abc123 id "abc123" user_id "user456" subtotal "2598.00" item_count "2"
EXPIRE cart:abc123 2592000  # 30 days

# Add Item
RPUSH cart:abc123:items '{"id":"item1","product_id":"prod1","quantity":2}'
EXPIRE cart:abc123:items 2592000

# Get Cart
HGETALL cart:abc123

# Get Items
LRANGE cart:abc123:items 0 -1

# Map user to cart
SET user_cart:user456 "abc123"
EXPIRE user_cart:user456 2592000

# Update quantity (requires Lua script or read-modify-write)
```

---

## Indexes

```sql
-- Carts
CREATE INDEX idx_carts_user ON carts(user_id) WHERE user_id IS NOT NULL;
CREATE INDEX idx_carts_session ON carts(session_id) WHERE session_id IS NOT NULL;
CREATE INDEX idx_carts_status ON carts(status);
CREATE INDEX idx_carts_active ON carts(status) WHERE status = 'active';
CREATE INDEX idx_carts_last_activity ON carts(last_activity_at DESC);
CREATE INDEX idx_carts_abandoned ON carts(status, last_activity_at) 
    WHERE status = 'active';

-- Cart Items
CREATE INDEX idx_cart_items_cart ON cart_items(cart_id);
CREATE INDEX idx_cart_items_product ON cart_items(product_id);
CREATE INDEX idx_cart_items_variant ON cart_items(variant_id);
CREATE INDEX idx_cart_items_seller ON cart_items(seller_id);
CREATE INDEX idx_cart_items_saved ON cart_items(cart_id) WHERE is_saved_for_later = true;

-- Saved for Later
CREATE INDEX idx_saved_user ON saved_for_later(user_id);
CREATE INDEX idx_saved_product ON saved_for_later(product_id);

-- Abandoned Carts
CREATE INDEX idx_abandoned_user ON abandoned_carts(user_id);
CREATE INDEX idx_abandoned_created ON abandoned_carts(created_at DESC);
CREATE INDEX idx_abandoned_not_recovered ON abandoned_carts(recovered) WHERE recovered = false;
```

---

## Sample Data

```sql
-- Create cart for user
INSERT INTO carts (
    id, user_id, status, currency, subtotal, total, item_count
) VALUES (
    'cccccccc-0000-0000-0000-000000000001',
    'uuuuuuuu-0000-0000-0000-000000000001',
    'active',
    'INR',
    2598.00,
    2598.00,
    2
);

-- Add cart items
INSERT INTO cart_items (
    cart_id, product_id, variant_id, seller_id,
    quantity, unit_price, product_snapshot
) VALUES 
(
    'cccccccc-0000-0000-0000-000000000001',
    'pppppppp-0000-0000-0000-000000000001',
    'vvvvvvvv-0000-0000-0000-000000000002',
    'ssssssss-0000-0000-0000-000000000001',
    2,
    1299.00,
    '{
        "name": "Nike Dri-FIT T-Shirt",
        "slug": "nike-dri-fit-tshirt",
        "image_url": "https://cdn.amcart.com/products/nike-tshirt.jpg",
        "sku": "NIKE-DF-BLK-M",
        "variant_name": "Black / M",
        "options": {"color": "Black", "size": "M"},
        "brand": "Nike",
        "category": "Men''s T-Shirts"
    }'::jsonb
);

-- Create guest cart
INSERT INTO carts (
    id, session_id, status, currency,
    expires_at
) VALUES (
    'cccccccc-0000-0000-0000-000000000002',
    'sess_abc123def456',
    'active',
    'INR',
    CURRENT_TIMESTAMP + INTERVAL '7 days'
);
```

---

## Common Queries

### 1. Get User's Active Cart with Items

```sql
SELECT 
    c.id,
    c.status,
    c.subtotal,
    c.discount_amount,
    c.total,
    c.item_count,
    c.applied_coupons,
    (
        SELECT json_agg(
            json_build_object(
                'id', ci.id,
                'product_id', ci.product_id,
                'variant_id', ci.variant_id,
                'quantity', ci.quantity,
                'unit_price', ci.unit_price,
                'line_total', ci.line_total,
                'product_snapshot', ci.product_snapshot,
                'is_available', ci.is_available,
                'availability_message', ci.availability_message,
                'price_changed', ci.price_changed,
                'current_unit_price', ci.current_unit_price
            ) ORDER BY ci.added_at
        )
        FROM cart_items ci 
        WHERE ci.cart_id = c.id AND ci.is_saved_for_later = false
    ) as items,
    (
        SELECT json_agg(
            json_build_object(
                'id', ci.id,
                'product_id', ci.product_id,
                'product_snapshot', ci.product_snapshot
            )
        )
        FROM cart_items ci 
        WHERE ci.cart_id = c.id AND ci.is_saved_for_later = true
    ) as saved_for_later
FROM carts c
WHERE c.user_id = 'uuuuuuuu-0000-0000-0000-000000000001'
    AND c.status = 'active';
```

### 2. Add Item to Cart

```sql
-- Upsert: Add or update quantity
INSERT INTO cart_items (
    cart_id, product_id, variant_id, seller_id,
    quantity, unit_price, current_unit_price, stock_at_add_time,
    product_snapshot
) VALUES (
    $1, $2, $3, $4, $5, $6, $6, $7, $8
)
ON CONFLICT (cart_id, variant_id) 
DO UPDATE SET
    quantity = cart_items.quantity + EXCLUDED.quantity,
    updated_at = CURRENT_TIMESTAMP
RETURNING *;
```

### 3. Merge Guest Cart to User Cart

```sql
-- When user logs in, merge guest cart items
WITH guest_cart AS (
    SELECT id FROM carts 
    WHERE session_id = $1 AND status = 'active'
),
user_cart AS (
    SELECT id FROM carts 
    WHERE user_id = $2 AND status = 'active'
)
INSERT INTO cart_items (
    cart_id, product_id, variant_id, seller_id,
    quantity, unit_price, product_snapshot
)
SELECT 
    (SELECT id FROM user_cart),
    ci.product_id,
    ci.variant_id,
    ci.seller_id,
    ci.quantity,
    ci.unit_price,
    ci.product_snapshot
FROM cart_items ci
JOIN guest_cart gc ON ci.cart_id = gc.id
ON CONFLICT (cart_id, variant_id)
DO UPDATE SET
    quantity = cart_items.quantity + EXCLUDED.quantity,
    updated_at = CURRENT_TIMESTAMP;

-- Mark guest cart as merged
UPDATE carts SET status = 'merged' 
WHERE session_id = $1 AND status = 'active';
```

### 4. Find Abandoned Carts (for recovery emails)

```sql
SELECT 
    c.id,
    c.user_id,
    u.email,
    u.first_name,
    c.subtotal,
    c.item_count,
    c.last_activity_at,
    CURRENT_TIMESTAMP - c.last_activity_at as time_abandoned,
    (
        SELECT json_agg(
            json_build_object(
                'name', ci.product_snapshot->>'name',
                'image', ci.product_snapshot->>'image_url',
                'price', ci.unit_price
            )
        )
        FROM cart_items ci WHERE ci.cart_id = c.id
        LIMIT 3
    ) as top_items
FROM carts c
JOIN users u ON c.user_id = u.id
WHERE c.status = 'active'
    AND c.user_id IS NOT NULL
    AND c.last_activity_at < CURRENT_TIMESTAMP - INTERVAL '24 hours'
    AND c.subtotal > 0
    AND NOT EXISTS (
        SELECT 1 FROM abandoned_carts ac 
        WHERE ac.cart_id = c.id 
        AND ac.recovery_emails_sent >= 3
    )
ORDER BY c.subtotal DESC
LIMIT 100;
```

---

## EF Core Entity Models

### Cart Entity

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmCart.CartService.Domain.Entities;

[Table("carts")]
public class Cart
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("user_id")]
    public Guid? UserId { get; set; }
    
    [MaxLength(255)]
    [Column("session_id")]
    public string? SessionId { get; set; }
    
    [MaxLength(255)]
    [Column("device_fingerprint")]
    public string? DeviceFingerprint { get; set; }
    
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active";
    
    [MaxLength(3)]
    [Column("currency")]
    public string Currency { get; set; } = "INR";
    
    [Column("subtotal", TypeName = "decimal(12,2)")]
    public decimal Subtotal { get; set; } = 0;
    
    [Column("discount_amount", TypeName = "decimal(12,2)")]
    public decimal DiscountAmount { get; set; } = 0;
    
    [Column("tax_amount", TypeName = "decimal(12,2)")]
    public decimal TaxAmount { get; set; } = 0;
    
    [Column("shipping_estimate", TypeName = "decimal(12,2)")]
    public decimal ShippingEstimate { get; set; } = 0;
    
    [Column("total", TypeName = "decimal(12,2)")]
    public decimal Total { get; set; } = 0;
    
    [Column("item_count")]
    public int ItemCount { get; set; } = 0;
    
    [Column("unique_item_count")]
    public int UniqueItemCount { get; set; } = 0;
    
    [Column("applied_coupons", TypeName = "jsonb")]
    public List<AppliedCoupon> AppliedCoupons { get; set; } = new();
    
    [Column("shipping_address_id")]
    public Guid? ShippingAddressId { get; set; }
    
    [Column("shipping_method_id")]
    public Guid? ShippingMethodId { get; set; }
    
    [Column("checkout_started_at")]
    public DateTime? CheckoutStartedAt { get; set; }
    
    [MaxLength(50)]
    [Column("checkout_step")]
    public string? CheckoutStep { get; set; }
    
    [Column("metadata", TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    [Column("notes")]
    public string? Notes { get; set; }
    
    [Column("gift_message")]
    public string? GiftMessage { get; set; }
    
    [Column("is_gift")]
    public bool IsGift { get; set; } = false;
    
    [Column("order_id")]
    public Guid? OrderId { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("last_activity_at")]
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    
    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }
    
    [Column("converted_at")]
    public DateTime? ConvertedAt { get; set; }
    
    // Navigation
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    public virtual ICollection<CartCoupon> Coupons { get; set; } = new List<CartCoupon>();
    
    // Computed
    [NotMapped]
    public IEnumerable<CartItem> ActiveItems => Items.Where(i => !i.IsSavedForLater);
    
    [NotMapped]
    public IEnumerable<CartItem> SavedItems => Items.Where(i => i.IsSavedForLater);
}

public class AppliedCoupon
{
    public string Code { get; set; } = null!;
    public string DiscountType { get; set; } = null!;
    public decimal Value { get; set; }
    public decimal DiscountAmount { get; set; }
}
```

### CartItem Entity

```csharp
[Table("cart_items")]
public class CartItem
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("cart_id")]
    public Guid CartId { get; set; }
    
    [Column("product_id")]
    public Guid ProductId { get; set; }
    
    [Column("variant_id")]
    public Guid VariantId { get; set; }
    
    [Column("seller_id")]
    public Guid SellerId { get; set; }
    
    [Column("quantity")]
    public int Quantity { get; set; } = 1;
    
    [Column("unit_price", TypeName = "decimal(12,2)")]
    public decimal UnitPrice { get; set; }
    
    [Column("compare_at_price", TypeName = "decimal(12,2)")]
    public decimal? CompareAtPrice { get; set; }
    
    [Column("current_unit_price", TypeName = "decimal(12,2)")]
    public decimal? CurrentUnitPrice { get; set; }
    
    [Column("price_changed")]
    public bool PriceChanged { get; set; } = false;
    
    [Column("price_change_amount", TypeName = "decimal(12,2)")]
    public decimal? PriceChangeAmount { get; set; }
    
    [Column("line_total", TypeName = "decimal(12,2)")]
    public decimal LineTotal { get; private set; } // Computed column
    
    [Column("product_snapshot", TypeName = "jsonb")]
    public ProductSnapshot ProductSnapshot { get; set; } = null!;
    
    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;
    
    [MaxLength(255)]
    [Column("availability_message")]
    public string? AvailabilityMessage { get; set; }
    
    [Column("stock_at_add_time")]
    public int? StockAtAddTime { get; set; }
    
    [Column("current_stock")]
    public int? CurrentStock { get; set; }
    
    [Column("item_discount", TypeName = "decimal(12,2)")]
    public decimal ItemDiscount { get; set; } = 0;
    
    [MaxLength(255)]
    [Column("discount_reason")]
    public string? DiscountReason { get; set; }
    
    [Column("is_gift")]
    public bool IsGift { get; set; } = false;
    
    [Column("gift_wrap")]
    public bool GiftWrap { get; set; } = false;
    
    [Column("gift_message")]
    public string? GiftMessage { get; set; }
    
    [Column("personalization", TypeName = "jsonb")]
    public Dictionary<string, string> Personalization { get; set; } = new();
    
    [Column("is_saved_for_later")]
    public bool IsSavedForLater { get; set; } = false;
    
    [Column("added_at")]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Cart Cart { get; set; } = null!;
}

public class ProductSnapshot
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string? VariantName { get; set; }
    public Dictionary<string, string> Options { get; set; } = new();
    public string? Brand { get; set; }
    public string? Category { get; set; }
}
```

---

## Cart Operations

### Cart Service Interface

```csharp
public interface ICartService
{
    Task<Cart> GetOrCreateCartAsync(Guid? userId, string? sessionId);
    Task<Cart> GetCartAsync(Guid cartId);
    Task<CartItem> AddItemAsync(Guid cartId, AddCartItemRequest request);
    Task<CartItem> UpdateQuantityAsync(Guid cartId, Guid itemId, int quantity);
    Task RemoveItemAsync(Guid cartId, Guid itemId);
    Task<Cart> ApplyCouponAsync(Guid cartId, string couponCode);
    Task RemoveCouponAsync(Guid cartId, string couponCode);
    Task<Cart> MergeCartsAsync(Guid guestCartId, Guid userCartId);
    Task SaveForLaterAsync(Guid cartId, Guid itemId);
    Task MoveToCartAsync(Guid cartId, Guid itemId);
    Task<Cart> RefreshPricesAsync(Guid cartId);
    Task ClearCartAsync(Guid cartId);
    Task<Cart> ConvertToOrderAsync(Guid cartId);
}
```

### Redis Cache Service

```csharp
public class CartCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    
    private const int UserCartTtlDays = 30;
    private const int GuestCartTtlDays = 7;
    
    public async Task<CartCacheModel?> GetCartAsync(Guid cartId)
    {
        var hash = await _db.HashGetAllAsync($"cart:{cartId}");
        if (hash.Length == 0) return null;
        
        var cart = new CartCacheModel
        {
            Id = Guid.Parse(hash.FirstOrDefault(h => h.Name == "id").Value!),
            UserId = hash.FirstOrDefault(h => h.Name == "user_id").Value.HasValue 
                ? Guid.Parse(hash.FirstOrDefault(h => h.Name == "user_id").Value!) 
                : null,
            Subtotal = decimal.Parse(hash.FirstOrDefault(h => h.Name == "subtotal").Value ?? "0"),
            Total = decimal.Parse(hash.FirstOrDefault(h => h.Name == "total").Value ?? "0"),
            ItemCount = int.Parse(hash.FirstOrDefault(h => h.Name == "item_count").Value ?? "0")
        };
        
        // Get items
        var itemsJson = await _db.ListRangeAsync($"cart:{cartId}:items");
        cart.Items = itemsJson
            .Select(j => JsonSerializer.Deserialize<CartItemCacheModel>(j!))
            .Where(i => i != null)
            .ToList()!;
        
        return cart;
    }
    
    public async Task SetCartAsync(Cart cart)
    {
        var key = $"cart:{cart.Id}";
        var ttl = TimeSpan.FromDays(cart.UserId.HasValue ? UserCartTtlDays : GuestCartTtlDays);
        
        await _db.HashSetAsync(key, new HashEntry[]
        {
            new("id", cart.Id.ToString()),
            new("user_id", cart.UserId?.ToString() ?? ""),
            new("subtotal", cart.Subtotal.ToString()),
            new("total", cart.Total.ToString()),
            new("item_count", cart.ItemCount.ToString()),
            new("updated_at", DateTime.UtcNow.ToString("O"))
        });
        
        await _db.KeyExpireAsync(key, ttl);
        
        // Cache items
        var itemsKey = $"cart:{cart.Id}:items";
        await _db.KeyDeleteAsync(itemsKey);
        
        foreach (var item in cart.Items.Where(i => !i.IsSavedForLater))
        {
            var itemJson = JsonSerializer.Serialize(new CartItemCacheModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal,
                ProductSnapshot = item.ProductSnapshot,
                IsAvailable = item.IsAvailable
            });
            
            await _db.ListRightPushAsync(itemsKey, itemJson);
        }
        
        await _db.KeyExpireAsync(itemsKey, ttl);
        
        // Map user/session to cart
        if (cart.UserId.HasValue)
        {
            await _db.StringSetAsync($"user_cart:{cart.UserId}", cart.Id.ToString(), ttl);
        }
        else if (!string.IsNullOrEmpty(cart.SessionId))
        {
            await _db.StringSetAsync($"guest_cart:{cart.SessionId}", cart.Id.ToString(), ttl);
        }
    }
    
    public async Task InvalidateCartAsync(Guid cartId)
    {
        await _db.KeyDeleteAsync($"cart:{cartId}");
        await _db.KeyDeleteAsync($"cart:{cartId}:items");
    }
}
```

---

## Summary

| Table | Purpose | Key Features |
|-------|---------|--------------|
| `carts` | Main cart data | User/guest support, totals, coupons |
| `cart_items` | Cart line items | Product snapshot, price tracking |
| `saved_for_later` | Saved items | Separate from active cart |
| `cart_coupons` | Applied discounts | Validation status |
| `abandoned_carts` | Analytics | Recovery tracking |

| Redis Key | Purpose | TTL |
|-----------|---------|-----|
| `cart:{id}` | Cart hash | 7-30 days |
| `cart:{id}:items` | Items list | 7-30 days |
| `user_cart:{user_id}` | User mapping | 30 days |
| `guest_cart:{session}` | Guest mapping | 7 days |

This schema supports:
- ✅ Guest and user carts
- ✅ Cart merge on login
- ✅ Price change detection
- ✅ Save for later
- ✅ Multiple coupons
- ✅ Abandoned cart recovery
- ✅ Redis caching
- ✅ Checkout progress tracking

