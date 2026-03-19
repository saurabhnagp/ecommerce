# AmCart Order Flow Diagram

This document describes the complete order flow for the AmCart ecommerce platform, including both successful and failure scenarios.

---

## 1. High-Level Order Flow

```mermaid
flowchart TB
    subgraph Customer["👤 Customer"]
        A[Browse Products] --> B[Add to Cart]
        B --> C[View Cart]
        C --> D[Proceed to Checkout]
        D --> E[Enter Shipping/Billing]
        E --> F[Select Payment Method]
        F --> G[Place Order]
    end

    subgraph OrderProcessing["📦 Order Processing"]
        G --> H{Validate Cart}
        H -->|Valid| I[Create Order - PENDING]
        H -->|Invalid| Z1[❌ Show Error]
        
        I --> J{Check Inventory}
        J -->|Available| K[Reserve Inventory]
        J -->|Not Available| Z2[❌ Out of Stock Error]
        
        K --> L[Initiate Payment]
    end

    subgraph PaymentProcessing["💳 Payment Processing"]
        L --> M[Redirect to Razorpay]
        M --> N{Payment Status}
        N -->|Success| O[✅ Payment Completed]
        N -->|Failed| P[❌ Payment Failed]
        N -->|Cancelled| Q[⚠️ Payment Cancelled]
    end

    subgraph SuccessPath["✅ Success Path"]
        O --> R[Update Order - CONFIRMED]
        R --> S[Confirm Inventory Reservation]
        S --> T[Send Order Confirmation Email]
        T --> U[Send SMS Notification]
        U --> V[Show Order Success Page]
    end

    subgraph FailurePath["❌ Failure Path"]
        P --> W[Update Order - FAILED]
        Q --> W
        W --> X[Release Inventory]
        X --> Y[Send Failure Notification]
        Y --> Z3[Show Retry Payment Option]
    end

    Z1 --> C
    Z2 --> C
    Z3 --> F
```

---

## 2. Detailed Order Creation Flow

```mermaid
sequenceDiagram
    autonumber
    participant U as 👤 User
    participant FE as 🖥️ Frontend (Nuxt.js)
    participant NG as 🔀 Nginx Gateway
    participant OS as 📦 Order Service
    participant CS as 🛒 Cart Service
    participant PS as 📝 Product Service
    participant IS as 📊 Inventory Service
    participant DB as 🗄️ PostgreSQL

    U->>FE: Click "Place Order"
    FE->>NG: POST /api/v1/orders
    NG->>OS: Forward Request
    
    OS->>CS: GET /cart/{userId}
    CS-->>OS: Cart Items
    
    loop For each cart item
        OS->>PS: GET /products/{id}
        PS-->>OS: Product Details
        OS->>IS: GET /inventory/{productId}
        IS-->>OS: Stock Available
    end
    
    alt All Items Available
        OS->>DB: INSERT Order (status: PENDING)
        DB-->>OS: Order Created
        OS->>IS: POST /inventory/reserve
        IS->>DB: Reserve Stock
        IS-->>OS: Reservation Confirmed
        OS-->>NG: 201 Created + Order Details
        NG-->>FE: Order Response
        FE-->>U: Redirect to Payment
    else Items Not Available
        OS-->>NG: 422 Unprocessable Entity
        NG-->>FE: Error Response
        FE-->>U: Show "Out of Stock" Error
    end
```

---

## 3. Payment Processing Flow

```mermaid
sequenceDiagram
    autonumber
    participant U as 👤 User
    participant FE as 🖥️ Frontend
    participant PS as 💳 Payment Service
    participant RZ as 🏦 Razorpay
    participant OS as 📦 Order Service
    participant IS as 📊 Inventory Service
    participant NS as 📧 Notification Service
    participant MQ as 📨 RabbitMQ

    FE->>PS: POST /payments/initiate
    PS->>RZ: Create Payment Order
    RZ-->>PS: Payment Order ID
    PS-->>FE: Razorpay Checkout URL
    
    FE->>RZ: Open Razorpay Checkout
    U->>RZ: Enter Payment Details
    U->>RZ: Confirm Payment
    
    alt Payment Successful
        RZ->>PS: Webhook: payment.captured
        PS->>PS: Verify Signature
        PS->>PS: Update Payment Status: SUCCESS
        PS->>MQ: Publish PaymentCompleted Event
        
        MQ->>OS: PaymentCompleted
        OS->>OS: Update Order: CONFIRMED
        
        MQ->>IS: PaymentCompleted
        IS->>IS: Confirm Reservation
        
        MQ->>NS: PaymentCompleted
        NS->>NS: Send Order Confirmation Email
        NS->>NS: Send SMS
        
        PS-->>FE: Payment Success
        FE-->>U: Show Order Confirmation
        
    else Payment Failed
        RZ->>PS: Webhook: payment.failed
        PS->>PS: Update Payment Status: FAILED
        PS->>MQ: Publish PaymentFailed Event
        
        MQ->>OS: PaymentFailed
        OS->>OS: Update Order: FAILED
        
        MQ->>IS: PaymentFailed
        IS->>IS: Release Reserved Stock
        
        MQ->>NS: PaymentFailed
        NS->>NS: Send Payment Failure Email
        
        PS-->>FE: Payment Failed
        FE-->>U: Show Retry Option
    end
```

---

## 4. Order State Machine

```mermaid
stateDiagram-v2
    [*] --> PENDING: Order Created

    PENDING --> PAYMENT_INITIATED: Payment Started
    PENDING --> CANCELLED: User Cancelled
    
    PAYMENT_INITIATED --> CONFIRMED: Payment Success
    PAYMENT_INITIATED --> PAYMENT_FAILED: Payment Failed
    PAYMENT_INITIATED --> CANCELLED: Payment Timeout
    
    PAYMENT_FAILED --> PAYMENT_INITIATED: Retry Payment
    PAYMENT_FAILED --> CANCELLED: Max Retries Reached
    
    CONFIRMED --> PROCESSING: Start Processing
    CONFIRMED --> CANCELLED: Admin Cancelled
    
    PROCESSING --> SHIPPED: Order Shipped
    PROCESSING --> CANCELLED: Admin Cancelled
    
    SHIPPED --> OUT_FOR_DELIVERY: Out for Delivery
    SHIPPED --> RETURNED: Return Initiated
    
    OUT_FOR_DELIVERY --> DELIVERED: Delivery Confirmed
    OUT_FOR_DELIVERY --> RETURNED: Delivery Failed
    
    DELIVERED --> COMPLETED: Order Complete
    DELIVERED --> REFUND_REQUESTED: Refund Requested
    
    REFUND_REQUESTED --> REFUNDED: Refund Processed
    REFUNDED --> [*]
    
    RETURNED --> REFUNDED: Refund Processed
    
    CANCELLED --> [*]
    COMPLETED --> [*]
```

---

## 5. Inventory Reservation Flow

```mermaid
flowchart TB
    subgraph OrderCreation["Order Creation"]
        A[Order Received] --> B{Check Stock}
        B -->|Stock >= Quantity| C[Soft Reserve]
        B -->|Stock < Quantity| D[❌ Reject Order]
        C --> E[Create Reservation Record]
        E --> F[Set Expiry: 15 mins]
    end

    subgraph PaymentOutcome["Payment Outcome"]
        F --> G{Payment Result}
        G -->|Success| H[✅ Confirm Reservation]
        G -->|Failed| I[❌ Release Reservation]
        G -->|Timeout| J[⏰ Auto-Release]
    end

    subgraph StockUpdate["Stock Update"]
        H --> K[Decrease Available Stock]
        K --> L[Update Reserved Stock]
        
        I --> M[Restore Available Stock]
        J --> M
        M --> N[Delete Reservation Record]
    end
```

---

## 6. Error Handling Scenarios

### 6.1 Cart Validation Errors

```mermaid
flowchart LR
    A[Validate Cart] --> B{Price Changed?}
    B -->|Yes| C[Show Price Update Warning]
    C --> D{User Accepts?}
    D -->|Yes| E[Continue]
    D -->|No| F[Return to Cart]
    
    B -->|No| G{Item Removed?}
    G -->|Yes| H[Show Item Unavailable]
    H --> F
    G -->|No| E
```

### 6.2 Payment Retry Flow

```mermaid
flowchart TB
    A[Payment Failed] --> B{Retry Count < 3?}
    B -->|Yes| C[Show Retry Option]
    C --> D{User Retries?}
    D -->|Yes| E[Extend Reservation]
    E --> F[Initiate New Payment]
    D -->|No| G[Cancel Order]
    
    B -->|No| H[Max Retries Exceeded]
    H --> G
    G --> I[Release Inventory]
    I --> J[Send Cancellation Email]
```

---

## 7. Event-Driven Order Flow

```mermaid
flowchart TB
    subgraph Events["Events Published"]
        E1[OrderCreated]
        E2[PaymentCompleted]
        E3[PaymentFailed]
        E4[OrderConfirmed]
        E5[OrderShipped]
        E6[OrderDelivered]
    end

    subgraph Subscribers["Event Subscribers"]
        S1[Inventory Service]
        S2[Notification Service]
        S3[Analytics Service]
        S4[Audit Service]
    end

    E1 -->|Reserve Stock| S1
    E1 -->|Log Event| S4
    
    E2 -->|Confirm Stock| S1
    E2 -->|Send Receipt| S2
    E2 -->|Track Conversion| S3
    
    E3 -->|Release Stock| S1
    E3 -->|Send Failure Email| S2
    E3 -->|Track Failure| S3
    
    E4 -->|Update Analytics| S3
    
    E5 -->|Send Tracking Email| S2
    E5 -->|Update Analytics| S3
    
    E6 -->|Send Delivery Confirmation| S2
    E6 -->|Request Review| S2
```

---

## 8. Order API Endpoints

| Endpoint | Method | Description | Success | Failure |
|----------|--------|-------------|---------|---------|
| `/orders` | POST | Create new order | 201 Created | 400/422 Error |
| `/orders/{id}` | GET | Get order details | 200 OK | 404 Not Found |
| `/orders/{id}/cancel` | POST | Cancel order | 200 OK | 400/409 Error |
| `/orders/{id}/payment` | POST | Initiate payment | 200 + Redirect | 400 Error |
| `/orders/{id}/payment/verify` | POST | Verify payment | 200 OK | 400 Error |
| `/orders/{id}/status` | PATCH | Update status (Admin) | 200 OK | 400/403 Error |

---

## 9. Order Status Reference

| Status | Description | Next States | Actions |
|--------|-------------|-------------|---------|
| `PENDING` | Order created, awaiting payment | PAYMENT_INITIATED, CANCELLED | Reserve inventory |
| `PAYMENT_INITIATED` | Payment in progress | CONFIRMED, PAYMENT_FAILED, CANCELLED | Wait for webhook |
| `PAYMENT_FAILED` | Payment failed | PAYMENT_INITIATED, CANCELLED | Allow retry |
| `CONFIRMED` | Payment successful | PROCESSING, CANCELLED | Confirm inventory |
| `PROCESSING` | Order being prepared | SHIPPED, CANCELLED | Prepare shipment |
| `SHIPPED` | Order shipped | OUT_FOR_DELIVERY, RETURNED | Update tracking |
| `OUT_FOR_DELIVERY` | With delivery agent | DELIVERED, RETURNED | Track delivery |
| `DELIVERED` | Order delivered | COMPLETED, REFUND_REQUESTED | Confirm delivery |
| `COMPLETED` | Order complete | - | Archive |
| `CANCELLED` | Order cancelled | - | Release inventory, refund |
| `REFUND_REQUESTED` | Refund requested | REFUNDED | Process refund |
| `REFUNDED` | Refund processed | - | Archive |
| `RETURNED` | Order returned | REFUNDED | Process return |

---

## 10. Timeout and Expiry Rules

| Scenario | Timeout | Action |
|----------|---------|--------|
| Inventory Reservation | 15 minutes | Auto-release if payment not completed |
| Payment Session | 10 minutes | Cancel payment, show timeout |
| Order Pending | 30 minutes | Auto-cancel if payment not initiated |
| Payment Retry | 24 hours | Max window to retry failed payment |
| Refund Processing | 5-7 business days | Complete refund to original payment method |

---

## 11. Notification Triggers

| Event | Email | SMS | Push |
|-------|-------|-----|------|
| Order Created | ✅ | ❌ | ✅ |
| Payment Success | ✅ | ✅ | ✅ |
| Payment Failed | ✅ | ❌ | ✅ |
| Order Shipped | ✅ | ✅ | ✅ |
| Out for Delivery | ❌ | ✅ | ✅ |
| Delivered | ✅ | ✅ | ✅ |
| Refund Processed | ✅ | ✅ | ✅ |

---

*Last Updated: December 2024*

