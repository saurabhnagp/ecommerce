# Payment Database Schema

## PostgreSQL Schema for AmCart E-commerce

---

## Table of Contents

1. [Overview](#overview)
2. [Entity Relationship Diagram](#entity-relationship-diagram)
3. [Tables Schema](#tables-schema)
4. [Payment Flow](#payment-flow)
5. [Indexes](#indexes)
6. [Sample Data](#sample-data)
7. [Common Queries](#common-queries)
8. [EF Core Entity Models](#ef-core-entity-models)
9. [Payment Gateway Integration](#payment-gateway-integration)

---

## Overview

### Design Principles

| Principle | Implementation |
|-----------|----------------|
| **PCI DSS Compliance** | No raw card data stored, tokenization only |
| **Idempotency** | Unique transaction IDs prevent duplicates |
| **Audit Trail** | Complete payment history |
| **Multi-Gateway** | Support for multiple payment providers |
| **Reconciliation** | Track gateway settlements |

### Payment Methods Supported

| Method | Provider | Details |
|--------|----------|---------|
| Cards | Razorpay, Stripe | Credit/Debit (Visa, MC, Rupay) |
| UPI | Razorpay | Google Pay, PhonePe, Paytm |
| Net Banking | Razorpay | Major banks |
| Wallets | Razorpay | Paytm, PhonePe, Amazon Pay |
| EMI | Razorpay | Card EMI, Bajaj Finserv |
| Pay Later | Simpl, LazyPay | BNPL providers |
| COD | Internal | Cash on Delivery |

### Database: `amcart_payments`

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                       Payment Database Architecture                           │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────┐        ┌─────────────────┐       ┌─────────────────┐   │
│  │   payments      │───────▶│payment_attempts │◀──────│gateway_responses│   │
│  │                 │        │                 │       │                 │   │
│  │ - Main record   │        │ - Each try      │       │ - Raw responses │   │
│  │ - Final status  │        │ - Gateway data  │       │ - Webhooks      │   │
│  └────────┬────────┘        └─────────────────┘       └─────────────────┘   │
│           │                                                                  │
│           │                                                                  │
│           ├───────────────────────────┬───────────────────────┐             │
│           │                           │                       │             │
│           ▼                           ▼                       ▼             │
│  ┌─────────────────┐        ┌─────────────────┐      ┌─────────────────┐   │
│  │    refunds      │        │  transactions   │      │  settlements    │   │
│  │                 │        │                 │      │                 │   │
│  │ - Partial/Full  │        │ - Ledger        │      │ - Gateway       │   │
│  │ - Status track  │        │ - Accounting    │      │ - Bank transfer │   │
│  └─────────────────┘        └─────────────────┘      └─────────────────┘   │
│                                                                              │
│  ┌─────────────────┐        ┌─────────────────┐                             │
│  │ seller_payouts  │        │  payout_items   │                             │
│  │                 │        │                 │                             │
│  │ - Seller money  │        │ - Order details │                             │
│  │ - Bank transfer │        │ - Commission    │                             │
│  └─────────────────┘        └─────────────────┘                             │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                            Payment Schema ERD                                    │
└─────────────────────────────────────────────────────────────────────────────────┘

                           ┌───────────────────────────────┐
                           │          payments             │
                           ├───────────────────────────────┤
                           │ id (PK)                       │
                           │ payment_number (unique)       │
                           │ order_id (FK)                 │
                           │ user_id                       │
                           │ amount                        │
                           │ currency                      │
                           │ status                        │
                           │ payment_method                │
                           │ gateway                       │
                           │ gateway_payment_id            │
                           └───────────────┬───────────────┘
                                           │
       ┌───────────────────┬───────────────┼───────────────┬───────────────────┐
       │                   │               │               │                   │
       ▼ 1:N               ▼ 1:N           ▼ 1:N           ▼ 1:N               ▼ 1:1
┌─────────────────┐ ┌─────────────────┐ ┌─────────────┐ ┌─────────────────┐ ┌───────────────┐
│payment_attempts │ │gateway_responses│ │   refunds   │ │  transactions   │ │  settlements  │
├─────────────────┤ ├─────────────────┤ ├─────────────┤ ├─────────────────┤ ├───────────────┤
│ id (PK)         │ │ id (PK)         │ │ id (PK)     │ │ id (PK)         │ │ id (PK)       │
│ payment_id (FK) │ │ payment_id (FK) │ │ payment_id  │ │ payment_id (FK) │ │ payment_id    │
│ attempt_number  │ │ event_type      │ │ refund_no   │ │ transaction_no  │ │ settlement_id │
│ status          │ │ raw_response    │ │ amount      │ │ type            │ │ settled_amt   │
│ gateway_id      │ │ webhook_id      │ │ status      │ │ amount          │ │ status        │
│ error_code      │ │ processed       │ │ gateway_id  │ │ balance_before  │ │ settled_at    │
└─────────────────┘ └─────────────────┘ └─────────────┘ │ balance_after   │ └───────────────┘
                                                        └─────────────────┘

                           ┌───────────────────────────────┐
                           │       seller_payouts          │
                           ├───────────────────────────────┤
                           │ id (PK)                       │
                           │ seller_id                     │
                           │ payout_number                 │
                           │ amount                        │
                           │ status                        │
                           │ bank_account_id               │
                           │ transfer_id                   │
                           └───────────────┬───────────────┘
                                           │
                                           ▼ 1:N
                           ┌───────────────────────────────┐
                           │        payout_items           │
                           ├───────────────────────────────┤
                           │ id (PK)                       │
                           │ payout_id (FK)                │
                           │ seller_order_id               │
                           │ order_amount                  │
                           │ commission                    │
                           │ payout_amount                 │
                           └───────────────────────────────┘
```

---

## Tables Schema

### 1. Payments Table (Main)

```sql
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Payment Identifier
    payment_number VARCHAR(50) NOT NULL UNIQUE,
    -- Format: PAY-2026-0000001
    
    -- References
    order_id UUID NOT NULL, -- Reference to Order Service
    user_id UUID NOT NULL,  -- Reference to User Service
    
    -- Amount
    amount DECIMAL(12,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'INR',
    
    -- Fees (if applicable)
    gateway_fee DECIMAL(12,2) DEFAULT 0.00,
    platform_fee DECIMAL(12,2) DEFAULT 0.00,
    tax_on_fee DECIMAL(12,2) DEFAULT 0.00,
    net_amount DECIMAL(12,2), -- amount - fees
    
    -- Status
    status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'processing', 'authorized', 'captured', 
    -- 'failed', 'cancelled', 'refunded', 'partially_refunded'
    
    -- Payment Method Details
    payment_method VARCHAR(30) NOT NULL,
    -- 'card', 'upi', 'net_banking', 'wallet', 'emi', 'pay_later', 'cod'
    
    payment_method_details JSONB DEFAULT '{}',
    -- For card: {"card_last4": "4242", "card_brand": "visa", "card_type": "credit"}
    -- For UPI: {"vpa": "user@upi", "app": "gpay"}
    -- For NetBanking: {"bank": "HDFC", "bank_code": "HDFC0000001"}
    -- For Wallet: {"wallet": "paytm"}
    
    -- Payment Gateway
    gateway VARCHAR(30) NOT NULL,
    -- 'razorpay', 'stripe', 'paytm', 'internal' (for COD)
    
    gateway_payment_id VARCHAR(255), -- Gateway's payment ID
    gateway_order_id VARCHAR(255),   -- Gateway's order ID (if created)
    
    -- Authorization (for 2-step payments)
    authorized_amount DECIMAL(12,2),
    authorized_at TIMESTAMP WITH TIME ZONE,
    authorization_expires_at TIMESTAMP WITH TIME ZONE,
    
    -- Capture
    captured_amount DECIMAL(12,2),
    captured_at TIMESTAMP WITH TIME ZONE,
    
    -- Refund Tracking
    refunded_amount DECIMAL(12,2) DEFAULT 0.00,
    refund_count INTEGER DEFAULT 0,
    
    -- 3D Secure / Authentication
    requires_authentication BOOLEAN DEFAULT false,
    authentication_status VARCHAR(30),
    authentication_url VARCHAR(500),
    
    -- Risk Assessment
    risk_score INTEGER,
    risk_level VARCHAR(20), -- 'low', 'medium', 'high'
    
    -- Customer Details (snapshot)
    customer_email VARCHAR(255),
    customer_phone VARCHAR(20),
    customer_name VARCHAR(200),
    
    -- Billing Address
    billing_address JSONB,
    
    -- Description
    description TEXT,
    statement_descriptor VARCHAR(22), -- Shows on bank statement
    
    -- Metadata
    metadata JSONB DEFAULT '{}',
    -- {"order_number": "AMC-2026-0001", "source": "web", "ip": "..."}
    
    -- Idempotency
    idempotency_key VARCHAR(255) UNIQUE,
    
    -- Error Information (if failed)
    error_code VARCHAR(50),
    error_message TEXT,
    error_source VARCHAR(30), -- 'gateway', 'bank', 'network'
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT valid_status CHECK (status IN (
        'pending', 'processing', 'authorized', 'captured',
        'failed', 'cancelled', 'refunded', 'partially_refunded'
    )),
    CONSTRAINT positive_amounts CHECK (amount > 0),
    CONSTRAINT refund_not_exceed CHECK (refunded_amount <= captured_amount OR captured_amount IS NULL)
);

-- Payment number sequence
CREATE SEQUENCE payment_number_seq START 1;

CREATE OR REPLACE FUNCTION generate_payment_number()
RETURNS TRIGGER AS $$
BEGIN
    NEW.payment_number := 'PAY-' || TO_CHAR(CURRENT_DATE, 'YYYY') || '-' || 
                          LPAD(nextval('payment_number_seq')::TEXT, 7, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_generate_payment_number
    BEFORE INSERT ON payments
    FOR EACH ROW
    WHEN (NEW.payment_number IS NULL)
    EXECUTE FUNCTION generate_payment_number();

CREATE TRIGGER trigger_payments_updated_at
    BEFORE UPDATE ON payments
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 2. Payment Attempts Table

```sql
CREATE TABLE payment_attempts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID NOT NULL REFERENCES payments(id) ON DELETE CASCADE,
    
    -- Attempt Info
    attempt_number INTEGER NOT NULL DEFAULT 1,
    
    -- Status
    status VARCHAR(30) NOT NULL,
    -- 'initiated', 'processing', 'success', 'failed', 'cancelled'
    
    -- Gateway Details
    gateway VARCHAR(30) NOT NULL,
    gateway_request_id VARCHAR(255),
    gateway_response_id VARCHAR(255),
    
    -- Payment Method for this attempt
    payment_method VARCHAR(30) NOT NULL,
    payment_method_details JSONB DEFAULT '{}',
    
    -- Amount
    amount DECIMAL(12,2) NOT NULL,
    
    -- Error (if failed)
    error_code VARCHAR(50),
    error_message TEXT,
    decline_code VARCHAR(50), -- Card decline codes
    
    -- Authentication
    authentication_required BOOLEAN DEFAULT false,
    authentication_status VARCHAR(30),
    
    -- Response Time
    response_time_ms INTEGER, -- Gateway response time
    
    -- IP and Device
    ip_address VARCHAR(45),
    user_agent TEXT,
    device_fingerprint VARCHAR(255),
    
    -- Timestamps
    initiated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    
    -- Unique attempt per payment
    CONSTRAINT unique_payment_attempt UNIQUE (payment_id, attempt_number)
);
```

### 3. Gateway Responses Table (Webhook Log)

```sql
CREATE TABLE gateway_responses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID REFERENCES payments(id),
    payment_attempt_id UUID REFERENCES payment_attempts(id),
    
    -- Gateway Info
    gateway VARCHAR(30) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    -- 'payment.authorized', 'payment.captured', 'payment.failed', 'refund.created'
    
    -- Webhook Details
    webhook_id VARCHAR(255),
    webhook_signature VARCHAR(500),
    
    -- Response Data
    raw_request JSONB, -- Original request to gateway
    raw_response JSONB NOT NULL, -- Gateway response
    
    -- Processing
    processed BOOLEAN DEFAULT false,
    processed_at TIMESTAMP WITH TIME ZONE,
    processing_error TEXT,
    
    -- Idempotency
    idempotency_key VARCHAR(255),
    
    -- Timestamps
    received_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Unique webhook processing
    CONSTRAINT unique_webhook UNIQUE (gateway, webhook_id)
);
```

### 4. Refunds Table

```sql
CREATE TABLE refunds (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID NOT NULL REFERENCES payments(id),
    
    -- Refund Identifier
    refund_number VARCHAR(50) NOT NULL UNIQUE,
    -- Format: REF-2026-0000001
    
    -- Order Reference
    order_id UUID NOT NULL,
    order_item_id UUID, -- If partial refund for specific item
    
    -- Gateway
    gateway VARCHAR(30) NOT NULL,
    gateway_refund_id VARCHAR(255),
    
    -- Amount
    amount DECIMAL(12,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'INR',
    
    -- Status
    status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'processing', 'completed', 'failed', 'cancelled'
    
    -- Refund Type
    refund_type VARCHAR(20) NOT NULL,
    -- 'full', 'partial'
    
    -- Reason
    reason_category VARCHAR(50),
    -- 'requested_by_customer', 'duplicate', 'fraudulent', 'order_cancelled'
    reason_details TEXT,
    
    -- Refund Destination
    refund_destination VARCHAR(30) DEFAULT 'original',
    -- 'original', 'store_credit', 'bank_account'
    
    -- For bank account refunds
    bank_account_details JSONB,
    
    -- Processing
    processed_by UUID, -- Admin who processed
    
    -- Gateway Response
    gateway_status VARCHAR(50),
    gateway_response JSONB,
    
    -- ARN (Acquirer Reference Number) for tracking
    arn VARCHAR(100),
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT positive_refund CHECK (amount > 0)
);

-- Refund number sequence
CREATE SEQUENCE refund_number_seq START 1;

CREATE OR REPLACE FUNCTION generate_refund_number()
RETURNS TRIGGER AS $$
BEGIN
    NEW.refund_number := 'REF-' || TO_CHAR(CURRENT_DATE, 'YYYY') || '-' || 
                         LPAD(nextval('refund_number_seq')::TEXT, 7, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_generate_refund_number
    BEFORE INSERT ON refunds
    FOR EACH ROW
    WHEN (NEW.refund_number IS NULL)
    EXECUTE FUNCTION generate_refund_number();

-- Update payment refunded_amount when refund is completed
CREATE OR REPLACE FUNCTION update_payment_refund_total()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status = 'completed' AND (OLD.status IS NULL OR OLD.status != 'completed') THEN
        UPDATE payments 
        SET 
            refunded_amount = COALESCE(refunded_amount, 0) + NEW.amount,
            refund_count = COALESCE(refund_count, 0) + 1,
            status = CASE 
                WHEN COALESCE(refunded_amount, 0) + NEW.amount >= captured_amount THEN 'refunded'
                ELSE 'partially_refunded'
            END,
            updated_at = CURRENT_TIMESTAMP
        WHERE id = NEW.payment_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_payment_refund
    AFTER INSERT OR UPDATE ON refunds
    FOR EACH ROW
    EXECUTE FUNCTION update_payment_refund_total();
```

### 5. Transactions Table (Ledger)

```sql
CREATE TABLE transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Transaction Identifier
    transaction_number VARCHAR(50) NOT NULL UNIQUE,
    
    -- References
    payment_id UUID REFERENCES payments(id),
    refund_id UUID REFERENCES refunds(id),
    payout_id UUID, -- Reference to seller_payouts
    
    -- Type
    transaction_type VARCHAR(30) NOT NULL,
    -- 'payment', 'refund', 'payout', 'fee', 'adjustment', 'chargeback'
    
    -- Direction
    direction VARCHAR(10) NOT NULL, -- 'credit', 'debit'
    
    -- Amount
    amount DECIMAL(12,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'INR',
    
    -- Balance (for account reconciliation)
    balance_before DECIMAL(14,2),
    balance_after DECIMAL(14,2),
    
    -- Entity
    entity_type VARCHAR(30), -- 'customer', 'seller', 'platform'
    entity_id UUID,
    
    -- Description
    description TEXT,
    
    -- Status
    status VARCHAR(20) DEFAULT 'completed',
    -- 'pending', 'completed', 'reversed'
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT valid_direction CHECK (direction IN ('credit', 'debit'))
);

CREATE SEQUENCE transaction_number_seq START 1;

CREATE OR REPLACE FUNCTION generate_transaction_number()
RETURNS TRIGGER AS $$
BEGIN
    NEW.transaction_number := 'TXN-' || TO_CHAR(CURRENT_DATE, 'YYYYMMDD') || '-' || 
                              LPAD(nextval('transaction_number_seq')::TEXT, 8, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_generate_transaction_number
    BEFORE INSERT ON transactions
    FOR EACH ROW
    WHEN (NEW.transaction_number IS NULL)
    EXECUTE FUNCTION generate_transaction_number();
```

### 6. Settlements Table

```sql
CREATE TABLE settlements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Settlement Identifier
    settlement_number VARCHAR(50) NOT NULL UNIQUE,
    
    -- Gateway
    gateway VARCHAR(30) NOT NULL,
    gateway_settlement_id VARCHAR(255) UNIQUE,
    
    -- Period
    settlement_date DATE NOT NULL,
    period_start TIMESTAMP WITH TIME ZONE,
    period_end TIMESTAMP WITH TIME ZONE,
    
    -- Amounts
    gross_amount DECIMAL(14,2) NOT NULL,
    fee_amount DECIMAL(14,2) DEFAULT 0.00,
    tax_amount DECIMAL(14,2) DEFAULT 0.00,
    refund_amount DECIMAL(14,2) DEFAULT 0.00,
    chargeback_amount DECIMAL(14,2) DEFAULT 0.00,
    adjustment_amount DECIMAL(14,2) DEFAULT 0.00,
    net_amount DECIMAL(14,2) NOT NULL,
    
    -- Transaction Counts
    payment_count INTEGER DEFAULT 0,
    refund_count INTEGER DEFAULT 0,
    
    -- Bank Transfer
    bank_account_id UUID,
    bank_reference VARCHAR(100),
    utr_number VARCHAR(100), -- Unique Transaction Reference
    
    -- Status
    status VARCHAR(20) DEFAULT 'pending',
    -- 'pending', 'processing', 'settled', 'failed'
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    settled_at TIMESTAMP WITH TIME ZONE,
    
    -- Raw Data
    raw_data JSONB
);
```

### 7. Settlement Items Table

```sql
CREATE TABLE settlement_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    settlement_id UUID NOT NULL REFERENCES settlements(id),
    payment_id UUID REFERENCES payments(id),
    refund_id UUID REFERENCES refunds(id),
    
    -- Type
    item_type VARCHAR(20) NOT NULL, -- 'payment', 'refund', 'fee', 'adjustment'
    
    -- Amount
    amount DECIMAL(12,2) NOT NULL,
    fee DECIMAL(12,2) DEFAULT 0.00,
    tax DECIMAL(12,2) DEFAULT 0.00,
    net_amount DECIMAL(12,2) NOT NULL,
    
    -- Gateway Reference
    gateway_reference VARCHAR(255),
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### 8. Seller Payouts Table

```sql
CREATE TABLE seller_payouts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Payout Identifier
    payout_number VARCHAR(50) NOT NULL UNIQUE,
    
    -- Seller
    seller_id UUID NOT NULL,
    
    -- Amount
    gross_amount DECIMAL(14,2) NOT NULL, -- Total order value
    commission_amount DECIMAL(14,2) NOT NULL, -- Platform commission
    tax_on_commission DECIMAL(14,2) DEFAULT 0.00, -- GST on commission
    other_deductions DECIMAL(14,2) DEFAULT 0.00, -- Chargebacks, penalties
    net_amount DECIMAL(14,2) NOT NULL, -- Amount to be paid
    currency VARCHAR(3) DEFAULT 'INR',
    
    -- Period
    period_start DATE NOT NULL,
    period_end DATE NOT NULL,
    
    -- Order Summary
    order_count INTEGER DEFAULT 0,
    item_count INTEGER DEFAULT 0,
    
    -- Bank Account
    bank_account_id UUID,
    bank_account_snapshot JSONB,
    -- {"bank_name": "HDFC", "account_number_last4": "1234", "ifsc": "HDFC0001234"}
    
    -- Status
    status VARCHAR(30) DEFAULT 'pending',
    -- 'pending', 'processing', 'completed', 'failed', 'on_hold'
    
    -- Transfer Details
    transfer_mode VARCHAR(20), -- 'neft', 'imps', 'rtgs', 'upi'
    transfer_reference VARCHAR(100),
    utr_number VARCHAR(100),
    
    -- Failure Info
    failure_reason TEXT,
    retry_count INTEGER DEFAULT 0,
    
    -- Approval
    approved_by UUID,
    approved_at TIMESTAMP WITH TIME ZONE,
    
    -- Notes
    notes TEXT,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE
);

-- Payout number sequence
CREATE SEQUENCE payout_number_seq START 1;

CREATE OR REPLACE FUNCTION generate_payout_number()
RETURNS TRIGGER AS $$
BEGIN
    NEW.payout_number := 'PO-' || TO_CHAR(CURRENT_DATE, 'YYYY') || '-' || 
                         LPAD(nextval('payout_number_seq')::TEXT, 7, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_generate_payout_number
    BEFORE INSERT ON seller_payouts
    FOR EACH ROW
    WHEN (NEW.payout_number IS NULL)
    EXECUTE FUNCTION generate_payout_number();
```

### 9. Payout Items Table

```sql
CREATE TABLE payout_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payout_id UUID NOT NULL REFERENCES seller_payouts(id) ON DELETE CASCADE,
    
    -- Order Reference
    seller_order_id UUID NOT NULL,
    order_id UUID NOT NULL,
    order_number VARCHAR(50),
    
    -- Amounts
    order_amount DECIMAL(12,2) NOT NULL,
    commission_rate DECIMAL(5,2) NOT NULL,
    commission_amount DECIMAL(12,2) NOT NULL,
    tax_on_commission DECIMAL(12,2) DEFAULT 0.00,
    deductions DECIMAL(12,2) DEFAULT 0.00,
    payout_amount DECIMAL(12,2) NOT NULL,
    
    -- Order Details
    order_date DATE,
    delivered_date DATE,
    item_count INTEGER,
    
    -- Status
    status VARCHAR(20) DEFAULT 'included',
    -- 'included', 'excluded', 'adjusted'
    
    exclusion_reason TEXT,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### 10. Bank Accounts Table (Seller)

```sql
CREATE TABLE seller_bank_accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    seller_id UUID NOT NULL,
    
    -- Account Details (encrypted at rest)
    account_holder_name VARCHAR(200) NOT NULL,
    account_number_encrypted BYTEA NOT NULL, -- Encrypted
    account_number_last4 VARCHAR(4) NOT NULL,
    ifsc_code VARCHAR(11) NOT NULL,
    bank_name VARCHAR(100) NOT NULL,
    branch_name VARCHAR(200),
    
    -- Account Type
    account_type VARCHAR(20) DEFAULT 'savings',
    -- 'savings', 'current'
    
    -- Verification
    is_verified BOOLEAN DEFAULT false,
    verified_at TIMESTAMP WITH TIME ZONE,
    verification_method VARCHAR(30), -- 'penny_drop', 'manual'
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    is_primary BOOLEAN DEFAULT false,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- One primary account per seller
CREATE UNIQUE INDEX idx_one_primary_bank_account 
    ON seller_bank_accounts(seller_id) 
    WHERE is_primary = true;
```

---

## Payment Flow

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              Payment Flow                                        │
└─────────────────────────────────────────────────────────────────────────────────┘

Customer                    AmCart                      Gateway (Razorpay)
    │                          │                              │
    │  1. Checkout             │                              │
    │─────────────────────────▶│                              │
    │                          │                              │
    │                          │  2. Create Payment Record    │
    │                          │  (status: pending)           │
    │                          │                              │
    │                          │  3. Create Gateway Order     │
    │                          │─────────────────────────────▶│
    │                          │                              │
    │                          │◀─────────────────────────────│
    │                          │  4. Gateway Order ID         │
    │                          │                              │
    │◀─────────────────────────│                              │
    │  5. Payment Options      │                              │
    │                          │                              │
    │  6. Select & Pay         │                              │
    │─────────────────────────────────────────────────────────▶│
    │                          │                              │
    │◀─────────────────────────────────────────────────────────│
    │  7. 3DS / OTP (if needed)│                              │
    │                          │                              │
    │  8. Confirm              │                              │
    │─────────────────────────────────────────────────────────▶│
    │                          │                              │
    │                          │  9. Webhook: Authorized      │
    │                          │◀─────────────────────────────│
    │                          │                              │
    │                          │  10. Verify & Update         │
    │                          │  (status: authorized)        │
    │                          │                              │
    │                          │  11. Capture Payment         │
    │                          │─────────────────────────────▶│
    │                          │                              │
    │                          │  12. Webhook: Captured       │
    │                          │◀─────────────────────────────│
    │                          │                              │
    │                          │  13. Update Payment          │
    │                          │  (status: captured)          │
    │                          │                              │
    │                          │  14. Confirm Order           │
    │                          │  (Order: confirmed)          │
    │                          │                              │
    │◀─────────────────────────│                              │
    │  15. Payment Success     │                              │
    │                          │                              │
```

---

## Indexes

```sql
-- Payments
CREATE INDEX idx_payments_order ON payments(order_id);
CREATE INDEX idx_payments_user ON payments(user_id);
CREATE INDEX idx_payments_number ON payments(payment_number);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_payments_gateway_id ON payments(gateway_payment_id);
CREATE INDEX idx_payments_created ON payments(created_at DESC);
CREATE INDEX idx_payments_idempotency ON payments(idempotency_key) WHERE idempotency_key IS NOT NULL;

-- Payment Attempts
CREATE INDEX idx_attempts_payment ON payment_attempts(payment_id);
CREATE INDEX idx_attempts_status ON payment_attempts(status);
CREATE INDEX idx_attempts_gateway ON payment_attempts(gateway_request_id);

-- Gateway Responses
CREATE INDEX idx_responses_payment ON gateway_responses(payment_id);
CREATE INDEX idx_responses_gateway ON gateway_responses(gateway, event_type);
CREATE INDEX idx_responses_webhook ON gateway_responses(gateway, webhook_id);
CREATE INDEX idx_responses_unprocessed ON gateway_responses(processed) WHERE processed = false;

-- Refunds
CREATE INDEX idx_refunds_payment ON refunds(payment_id);
CREATE INDEX idx_refunds_order ON refunds(order_id);
CREATE INDEX idx_refunds_status ON refunds(status);
CREATE INDEX idx_refunds_gateway ON refunds(gateway_refund_id);

-- Transactions
CREATE INDEX idx_transactions_payment ON transactions(payment_id);
CREATE INDEX idx_transactions_type ON transactions(transaction_type);
CREATE INDEX idx_transactions_entity ON transactions(entity_type, entity_id);
CREATE INDEX idx_transactions_created ON transactions(created_at DESC);

-- Settlements
CREATE INDEX idx_settlements_gateway ON settlements(gateway, gateway_settlement_id);
CREATE INDEX idx_settlements_date ON settlements(settlement_date);
CREATE INDEX idx_settlements_status ON settlements(status);

-- Seller Payouts
CREATE INDEX idx_payouts_seller ON seller_payouts(seller_id);
CREATE INDEX idx_payouts_status ON seller_payouts(status);
CREATE INDEX idx_payouts_period ON seller_payouts(period_start, period_end);
CREATE INDEX idx_payouts_pending ON seller_payouts(status) WHERE status = 'pending';

-- Payout Items
CREATE INDEX idx_payout_items_payout ON payout_items(payout_id);
CREATE INDEX idx_payout_items_seller_order ON payout_items(seller_order_id);

-- Bank Accounts
CREATE INDEX idx_bank_accounts_seller ON seller_bank_accounts(seller_id);
```

---

## Sample Data

```sql
-- Create payment
INSERT INTO payments (
    id, order_id, user_id, amount, currency,
    status, payment_method, gateway,
    payment_method_details, customer_email, customer_name
) VALUES (
    'pppppppp-1111-0000-0000-000000000001',
    'oooooooo-0000-0000-0000-000000000001',
    'uuuuuuuu-0000-0000-0000-000000000001',
    3164.64,
    'INR',
    'captured',
    'card',
    'razorpay',
    '{"card_last4": "4242", "card_brand": "visa", "card_type": "credit"}'::jsonb,
    'john.doe@example.com',
    'John Doe'
);

-- Update with gateway details
UPDATE payments SET
    gateway_payment_id = 'pay_ABC123XYZ',
    gateway_order_id = 'order_XYZ789',
    captured_amount = 3164.64,
    captured_at = CURRENT_TIMESTAMP,
    completed_at = CURRENT_TIMESTAMP
WHERE id = 'pppppppp-1111-0000-0000-000000000001';

-- Create payment attempt
INSERT INTO payment_attempts (
    payment_id, attempt_number, status, gateway,
    payment_method, amount, gateway_response_id
) VALUES (
    'pppppppp-1111-0000-0000-000000000001',
    1,
    'success',
    'razorpay',
    'card',
    3164.64,
    'pay_ABC123XYZ'
);
```

---

## Common Queries

### 1. Get Payment Details

```sql
SELECT 
    p.*,
    (
        SELECT json_agg(
            json_build_object(
                'attempt_number', pa.attempt_number,
                'status', pa.status,
                'payment_method', pa.payment_method,
                'error_code', pa.error_code,
                'error_message', pa.error_message,
                'initiated_at', pa.initiated_at,
                'completed_at', pa.completed_at
            ) ORDER BY pa.attempt_number
        )
        FROM payment_attempts pa WHERE pa.payment_id = p.id
    ) as attempts,
    (
        SELECT json_agg(
            json_build_object(
                'refund_number', r.refund_number,
                'amount', r.amount,
                'status', r.status,
                'reason_category', r.reason_category,
                'created_at', r.created_at
            )
        )
        FROM refunds r WHERE r.payment_id = p.id
    ) as refunds
FROM payments p
WHERE p.payment_number = 'PAY-2026-0000001';
```

### 2. Daily Payment Summary

```sql
SELECT 
    DATE(created_at) as date,
    payment_method,
    COUNT(*) as total_payments,
    COUNT(*) FILTER (WHERE status = 'captured') as successful,
    COUNT(*) FILTER (WHERE status = 'failed') as failed,
    SUM(CASE WHEN status = 'captured' THEN amount ELSE 0 END) as captured_amount,
    SUM(CASE WHEN status = 'captured' THEN gateway_fee ELSE 0 END) as total_fees
FROM payments
WHERE created_at >= CURRENT_DATE - INTERVAL '30 days'
GROUP BY DATE(created_at), payment_method
ORDER BY date DESC, payment_method;
```

### 3. Seller Payout Summary

```sql
SELECT 
    sp.payout_number,
    sp.seller_id,
    sp.gross_amount,
    sp.commission_amount,
    sp.net_amount,
    sp.status,
    sp.period_start,
    sp.period_end,
    sp.order_count,
    (
        SELECT json_agg(
            json_build_object(
                'order_number', pi.order_number,
                'order_amount', pi.order_amount,
                'commission', pi.commission_amount,
                'payout_amount', pi.payout_amount
            )
        )
        FROM payout_items pi WHERE pi.payout_id = sp.id
    ) as items
FROM seller_payouts sp
WHERE sp.seller_id = 'ssssssss-0000-0000-0000-000000000001'
ORDER BY sp.created_at DESC
LIMIT 10;
```

---

## EF Core Entity Models

### Payment Entity

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmCart.PaymentService.Domain.Entities;

[Table("payments")]
public class Payment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Column("payment_number")]
    public string PaymentNumber { get; set; } = null!;
    
    [Column("order_id")]
    public Guid OrderId { get; set; }
    
    [Column("user_id")]
    public Guid UserId { get; set; }
    
    [Column("amount", TypeName = "decimal(12,2)")]
    public decimal Amount { get; set; }
    
    [MaxLength(3)]
    [Column("currency")]
    public string Currency { get; set; } = "INR";
    
    [Column("gateway_fee", TypeName = "decimal(12,2)")]
    public decimal GatewayFee { get; set; } = 0;
    
    [Column("net_amount", TypeName = "decimal(12,2)")]
    public decimal? NetAmount { get; set; }
    
    [MaxLength(30)]
    [Column("status")]
    public string Status { get; set; } = "pending";
    
    [Required]
    [MaxLength(30)]
    [Column("payment_method")]
    public string PaymentMethod { get; set; } = null!;
    
    [Column("payment_method_details", TypeName = "jsonb")]
    public PaymentMethodDetails? PaymentMethodDetails { get; set; }
    
    [Required]
    [MaxLength(30)]
    [Column("gateway")]
    public string Gateway { get; set; } = null!;
    
    [MaxLength(255)]
    [Column("gateway_payment_id")]
    public string? GatewayPaymentId { get; set; }
    
    [MaxLength(255)]
    [Column("gateway_order_id")]
    public string? GatewayOrderId { get; set; }
    
    [Column("authorized_amount", TypeName = "decimal(12,2)")]
    public decimal? AuthorizedAmount { get; set; }
    
    [Column("authorized_at")]
    public DateTime? AuthorizedAt { get; set; }
    
    [Column("captured_amount", TypeName = "decimal(12,2)")]
    public decimal? CapturedAmount { get; set; }
    
    [Column("captured_at")]
    public DateTime? CapturedAt { get; set; }
    
    [Column("refunded_amount", TypeName = "decimal(12,2)")]
    public decimal RefundedAmount { get; set; } = 0;
    
    [Column("refund_count")]
    public int RefundCount { get; set; } = 0;
    
    [Column("requires_authentication")]
    public bool RequiresAuthentication { get; set; } = false;
    
    [MaxLength(255)]
    [Column("customer_email")]
    public string? CustomerEmail { get; set; }
    
    [MaxLength(20)]
    [Column("customer_phone")]
    public string? CustomerPhone { get; set; }
    
    [MaxLength(200)]
    [Column("customer_name")]
    public string? CustomerName { get; set; }
    
    [Column("billing_address", TypeName = "jsonb")]
    public BillingAddress? BillingAddress { get; set; }
    
    [Column("metadata", TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    [MaxLength(255)]
    [Column("idempotency_key")]
    public string? IdempotencyKey { get; set; }
    
    [MaxLength(50)]
    [Column("error_code")]
    public string? ErrorCode { get; set; }
    
    [Column("error_message")]
    public string? ErrorMessage { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
    
    // Navigation
    public virtual ICollection<PaymentAttempt> Attempts { get; set; } = new List<PaymentAttempt>();
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    public virtual ICollection<GatewayResponse> GatewayResponses { get; set; } = new List<GatewayResponse>();
}

public class PaymentMethodDetails
{
    // Card
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }
    public string? CardType { get; set; }
    
    // UPI
    public string? Vpa { get; set; }
    public string? App { get; set; }
    
    // Net Banking
    public string? Bank { get; set; }
    public string? BankCode { get; set; }
    
    // Wallet
    public string? Wallet { get; set; }
}

public class BillingAddress
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}
```

---

## Payment Gateway Integration

### Razorpay Integration Example

```csharp
public interface IPaymentGateway
{
    Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request);
    Task<PaymentResult> VerifyPaymentAsync(VerifyPaymentRequest request);
    Task<CaptureResult> CapturePaymentAsync(string paymentId, decimal amount);
    Task<RefundResult> RefundPaymentAsync(string paymentId, decimal amount, string reason);
}

public class RazorpayGateway : IPaymentGateway
{
    private readonly RazorpayClient _client;
    
    public async Task<CreateOrderResult> CreateOrderAsync(CreateOrderRequest request)
    {
        var orderRequest = new Dictionary<string, object>
        {
            { "amount", (int)(request.Amount * 100) }, // Razorpay uses paise
            { "currency", request.Currency },
            { "receipt", request.PaymentNumber },
            { "notes", new Dictionary<string, string>
                {
                    { "order_id", request.OrderId.ToString() }
                }
            }
        };
        
        var order = await _client.Order.CreateAsync(orderRequest);
        
        return new CreateOrderResult
        {
            GatewayOrderId = order["id"].ToString(),
            Status = "created"
        };
    }
    
    public async Task<PaymentResult> VerifyPaymentAsync(VerifyPaymentRequest request)
    {
        var attributes = new Dictionary<string, string>
        {
            { "razorpay_order_id", request.GatewayOrderId },
            { "razorpay_payment_id", request.GatewayPaymentId },
            { "razorpay_signature", request.Signature }
        };
        
        try
        {
            Utils.verifyPaymentSignature(attributes);
            
            var payment = await _client.Payment.FetchAsync(request.GatewayPaymentId);
            
            return new PaymentResult
            {
                Success = true,
                GatewayPaymentId = request.GatewayPaymentId,
                Status = payment["status"].ToString(),
                Method = payment["method"].ToString(),
                Amount = decimal.Parse(payment["amount"].ToString()) / 100
            };
        }
        catch
        {
            return new PaymentResult { Success = false, Error = "Signature verification failed" };
        }
    }
}
```

---

## Summary

| Table | Purpose | Key Features |
|-------|---------|--------------|
| `payments` | Main payment record | Multi-gateway, idempotency |
| `payment_attempts` | Each payment try | Retry tracking, errors |
| `gateway_responses` | Webhook logs | Raw responses, audit |
| `refunds` | Refund records | Full/partial, status |
| `transactions` | Ledger entries | Accounting, reconciliation |
| `settlements` | Gateway settlements | Bank transfers |
| `seller_payouts` | Seller payments | Commission calculation |
| `payout_items` | Payout details | Per-order breakdown |
| `seller_bank_accounts` | Bank details | Encrypted storage |

This schema supports:
- ✅ Multiple payment gateways
- ✅ Card, UPI, NetBanking, Wallets
- ✅ Full and partial refunds
- ✅ Payment retry handling
- ✅ Webhook processing
- ✅ Settlement reconciliation
- ✅ Seller payouts
- ✅ Commission tracking
- ✅ PCI DSS compliance (no card storage)
- ✅ Idempotent operations

