# User Database Schema

## PostgreSQL Schema for AmCart E-commerce (Cognito Integration)

---

## Table of Contents

1. [Overview](#overview)
2. [Entity Relationship Diagram](#entity-relationship-diagram)
3. [Tables Schema](#tables-schema)
4. [Indexes](#indexes)
5. [Sample Data](#sample-data)
6. [Common Queries](#common-queries)
7. [EF Core Entity Models](#ef-core-entity-models)
8. [Cognito Sync](#cognito-sync)

---

## Overview

### Design Principles

| Principle | Implementation |
|-----------|----------------|
| **Cognito as Auth Provider** | AWS Cognito handles authentication, passwords, MFA |
| **PostgreSQL for Profiles** | Extended user data stored locally |
| **Sync via Lambda** | Cognito triggers sync user data to PostgreSQL |
| **Soft Delete** | Users can be deactivated, not hard deleted |
| **Address Management** | Multiple addresses per user |

### Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                      User Data Architecture                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────┐          ┌─────────────────────────────────┐  │
│  │  AWS Cognito    │          │         PostgreSQL              │  │
│  │  User Pool      │──Sync───▶│                                 │  │
│  │                 │          │  ┌───────────┐  ┌────────────┐  │  │
│  │  - Email/Pass   │          │  │   users   │──│ addresses  │  │  │
│  │  - Google OAuth │          │  │           │  └────────────┘  │  │
│  │  - Facebook     │          │  │  - Profile│                  │  │
│  │  - MFA          │          │  │  - Prefs  │  ┌────────────┐  │  │
│  │  - JWT Tokens   │          │  │           │──│ user_prefs │  │  │
│  └─────────────────┘          │  └───────────┘  └────────────┘  │  │
│                               │                                  │  │
│                               └─────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### Database: `amcart_users`

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          User Schema ERD                                     │
└─────────────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────────────────┐
                    │           users             │
                    ├─────────────────────────────┤
                    │ id (PK)                     │
                    │ cognito_sub (unique)        │
                    │ email                       │
                    │ first_name                  │
                    │ last_name                   │
                    │ phone                       │
                    │ avatar_url                  │
                    │ auth_provider               │
                    │ role                        │
                    │ status                      │
                    │ preferences (JSONB)         │
                    │ metadata (JSONB)            │
                    └──────────────┬──────────────┘
                                   │
          ┌────────────────────────┼────────────────────────┐
          │                        │                        │
          ▼ 1:N                    ▼ 1:N                    ▼ 1:N
┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐
│     addresses       │  │   user_sessions     │  │  notification_      │
├─────────────────────┤  ├─────────────────────┤  │  preferences        │
│ id (PK)             │  │ id (PK)             │  ├─────────────────────┤
│ user_id (FK)        │  │ user_id (FK)        │  │ id (PK)             │
│ type                │  │ device_info         │  │ user_id (FK)        │
│ is_default          │  │ ip_address          │  │ email_orders        │
│ first_name          │  │ last_active_at      │  │ email_promotions    │
│ last_name           │  │ created_at          │  │ sms_orders          │
│ phone               │  └─────────────────────┘  │ push_enabled        │
│ address_line1       │                           └─────────────────────┘
│ address_line2       │
│ city                │  ┌─────────────────────┐
│ state               │  │   user_activity_    │
│ postal_code         │  │   logs              │
│ country             │  ├─────────────────────┤
│ landmark            │  │ id (PK)             │
│ coordinates (JSONB) │  │ user_id (FK)        │
└─────────────────────┘  │ activity_type       │
                         │ details (JSONB)     │
                         │ ip_address          │
                         │ created_at          │
                         └─────────────────────┘
```

---

## Tables Schema

### 1. Users Table

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Cognito Integration
    cognito_sub VARCHAR(255) NOT NULL UNIQUE, -- Cognito User Pool subject ID
    
    -- Basic Info
    email VARCHAR(255) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    phone_verified BOOLEAN DEFAULT false,
    
    -- Profile
    avatar_url VARCHAR(500),
    date_of_birth DATE,
    gender VARCHAR(20), -- 'male', 'female', 'other', 'prefer_not_to_say'
    
    -- Authentication Provider
    auth_provider VARCHAR(20) DEFAULT 'email',
    -- 'email', 'google', 'facebook'
    google_id VARCHAR(255),
    facebook_id VARCHAR(255),
    
    -- Role & Permissions
    role VARCHAR(20) DEFAULT 'customer',
    -- 'customer', 'seller', 'admin', 'super_admin'
    permissions JSONB DEFAULT '[]',
    -- Custom permissions beyond role: ["manage_products", "view_reports"]
    
    -- Account Status
    status VARCHAR(20) DEFAULT 'active',
    -- 'pending', 'active', 'suspended', 'deactivated'
    is_email_verified BOOLEAN DEFAULT false,
    email_verified_at TIMESTAMP WITH TIME ZONE,
    
    -- Preferences (JSONB for flexibility)
    preferences JSONB DEFAULT '{
        "language": "en",
        "currency": "INR",
        "timezone": "Asia/Kolkata",
        "theme": "light"
    }',
    
    -- Additional Metadata (JSONB)
    metadata JSONB DEFAULT '{}',
    -- {
    --   "referral_code": "ABC123",
    --   "referred_by": "user_id",
    --   "signup_source": "mobile_app",
    --   "marketing_consent": true
    -- }
    
    -- Stats (denormalized)
    total_orders INTEGER DEFAULT 0,
    total_spent DECIMAL(12,2) DEFAULT 0.00,
    loyalty_points INTEGER DEFAULT 0,
    
    -- Security
    failed_login_attempts INTEGER DEFAULT 0,
    lockout_end TIMESTAMP WITH TIME ZONE,
    last_login_at TIMESTAMP WITH TIME ZONE,
    last_login_ip VARCHAR(45),
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE, -- Soft delete
    
    -- Constraints
    CONSTRAINT valid_role CHECK (role IN ('customer', 'seller', 'admin', 'super_admin')),
    CONSTRAINT valid_status CHECK (status IN ('pending', 'active', 'suspended', 'deactivated')),
    CONSTRAINT valid_auth_provider CHECK (auth_provider IN ('email', 'google', 'facebook'))
);

-- Trigger for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 2. Addresses Table

```sql
CREATE TABLE addresses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    -- Address Type
    address_type VARCHAR(20) DEFAULT 'shipping',
    -- 'shipping', 'billing', 'both'
    
    -- Label
    label VARCHAR(50), -- 'Home', 'Office', 'Other'
    
    -- Recipient Info
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(255),
    company VARCHAR(200),
    
    -- Address Details
    address_line1 VARCHAR(255) NOT NULL,
    address_line2 VARCHAR(255),
    landmark VARCHAR(255),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(100) NOT NULL,
    postal_code VARCHAR(20) NOT NULL,
    country VARCHAR(100) DEFAULT 'India',
    country_code VARCHAR(2) DEFAULT 'IN',
    
    -- Geolocation (for delivery)
    coordinates JSONB, -- {"lat": 28.6139, "lng": 77.2090}
    
    -- Flags
    is_default_shipping BOOLEAN DEFAULT false,
    is_default_billing BOOLEAN DEFAULT false,
    is_verified BOOLEAN DEFAULT false, -- Address verification service
    
    -- GST (for business addresses)
    gstin VARCHAR(15), -- GST Identification Number
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT valid_address_type CHECK (address_type IN ('shipping', 'billing', 'both'))
);

-- Ensure only one default shipping address per user
CREATE UNIQUE INDEX idx_one_default_shipping 
    ON addresses(user_id) 
    WHERE is_default_shipping = true;

-- Ensure only one default billing address per user
CREATE UNIQUE INDEX idx_one_default_billing 
    ON addresses(user_id) 
    WHERE is_default_billing = true;

CREATE TRIGGER trigger_addresses_updated_at
    BEFORE UPDATE ON addresses
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 3. User Sessions Table

```sql
CREATE TABLE user_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    -- Session Info (from Cognito tokens)
    cognito_session_id VARCHAR(255),
    
    -- Device Information
    device_id VARCHAR(255),
    device_type VARCHAR(50), -- 'desktop', 'mobile', 'tablet'
    device_name VARCHAR(200),
    browser VARCHAR(100),
    os VARCHAR(100),
    user_agent TEXT,
    
    -- Location
    ip_address VARCHAR(45),
    location JSONB, -- {"city": "Mumbai", "country": "India"}
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_active_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP WITH TIME ZONE,
    ended_at TIMESTAMP WITH TIME ZONE
);
```

### 4. Notification Preferences Table

```sql
CREATE TABLE notification_preferences (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE UNIQUE,
    
    -- Email Notifications
    email_order_updates BOOLEAN DEFAULT true,
    email_shipping_updates BOOLEAN DEFAULT true,
    email_promotions BOOLEAN DEFAULT true,
    email_newsletters BOOLEAN DEFAULT false,
    email_price_drops BOOLEAN DEFAULT true, -- Wishlist items
    email_back_in_stock BOOLEAN DEFAULT true,
    
    -- SMS Notifications
    sms_order_updates BOOLEAN DEFAULT true,
    sms_delivery_updates BOOLEAN DEFAULT true,
    sms_promotions BOOLEAN DEFAULT false,
    sms_otp BOOLEAN DEFAULT true, -- Can't disable OTP
    
    -- Push Notifications (Mobile App)
    push_enabled BOOLEAN DEFAULT true,
    push_order_updates BOOLEAN DEFAULT true,
    push_promotions BOOLEAN DEFAULT true,
    push_chat_messages BOOLEAN DEFAULT true,
    
    -- WhatsApp
    whatsapp_enabled BOOLEAN DEFAULT false,
    whatsapp_order_updates BOOLEAN DEFAULT false,
    
    -- Frequency
    promotion_frequency VARCHAR(20) DEFAULT 'weekly',
    -- 'daily', 'weekly', 'monthly', 'never'
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TRIGGER trigger_notification_prefs_updated_at
    BEFORE UPDATE ON notification_preferences
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 5. User Activity Logs Table

```sql
CREATE TABLE user_activity_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    -- Activity Details
    activity_type VARCHAR(50) NOT NULL,
    -- 'login', 'logout', 'password_change', 'profile_update', 
    -- 'address_add', 'address_update', 'order_placed', 'review_posted'
    
    activity_description TEXT,
    
    -- Additional Details (JSONB)
    details JSONB DEFAULT '{}',
    -- {"order_id": "uuid", "old_value": {}, "new_value": {}}
    
    -- Context
    ip_address VARCHAR(45),
    user_agent TEXT,
    device_type VARCHAR(50),
    
    -- Reference
    entity_type VARCHAR(50), -- 'order', 'address', 'profile'
    entity_id UUID,
    
    -- Timestamp
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Partition by date for large activity logs
-- CREATE TABLE user_activity_logs_2026_01 PARTITION OF user_activity_logs
--     FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');
```

### 6. User Wishlists Table

```sql
CREATE TABLE wishlists (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    -- Wishlist Info
    name VARCHAR(100) DEFAULT 'My Wishlist',
    description TEXT,
    is_default BOOLEAN DEFAULT false,
    is_public BOOLEAN DEFAULT false, -- Shareable wishlist
    
    -- Share Settings
    share_token VARCHAR(100) UNIQUE,
    
    -- Stats
    item_count INTEGER DEFAULT 0,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE wishlist_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    wishlist_id UUID NOT NULL REFERENCES wishlists(id) ON DELETE CASCADE,
    product_id UUID NOT NULL, -- Reference to Product Service
    variant_id UUID, -- Specific variant if selected
    
    -- Price at time of adding (to show price changes)
    price_when_added DECIMAL(12,2),
    
    -- Notes
    notes TEXT,
    priority INTEGER DEFAULT 0, -- For sorting
    
    -- Timestamps
    added_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Unique product per wishlist
    CONSTRAINT unique_wishlist_product UNIQUE (wishlist_id, product_id, variant_id)
);
```

### 7. Saved Payment Methods Table

```sql
CREATE TABLE saved_payment_methods (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    
    -- Payment Type
    payment_type VARCHAR(30) NOT NULL,
    -- 'card', 'upi', 'net_banking', 'wallet'
    
    -- Card Details (tokenized)
    card_token VARCHAR(500), -- Tokenized by payment gateway
    card_last_four VARCHAR(4),
    card_brand VARCHAR(20), -- 'visa', 'mastercard', 'rupay', 'amex'
    card_expiry_month INTEGER,
    card_expiry_year INTEGER,
    card_holder_name VARCHAR(200),
    
    -- UPI Details
    upi_id VARCHAR(255), -- example@upi
    
    -- Bank Details
    bank_name VARCHAR(100),
    bank_code VARCHAR(20),
    
    -- Wallet Details
    wallet_provider VARCHAR(50), -- 'paytm', 'phonepe', 'amazon_pay'
    
    -- Display
    display_name VARCHAR(100), -- "HDFC Visa ending 4242"
    
    -- Flags
    is_default BOOLEAN DEFAULT false,
    is_verified BOOLEAN DEFAULT false,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_used_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE -- For cards
);

-- One default payment method per user
CREATE UNIQUE INDEX idx_one_default_payment 
    ON saved_payment_methods(user_id) 
    WHERE is_default = true;
```

---

## Indexes

```sql
-- Users
CREATE INDEX idx_users_cognito_sub ON users(cognito_sub);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_phone ON users(phone) WHERE phone IS NOT NULL;
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_status ON users(status);
CREATE INDEX idx_users_auth_provider ON users(auth_provider);
CREATE INDEX idx_users_created ON users(created_at DESC);
CREATE INDEX idx_users_google_id ON users(google_id) WHERE google_id IS NOT NULL;
CREATE INDEX idx_users_facebook_id ON users(facebook_id) WHERE facebook_id IS NOT NULL;

-- Full-text search on name
CREATE INDEX idx_users_name_search ON users USING GIN (
    to_tsvector('english', first_name || ' ' || last_name)
);

-- Addresses
CREATE INDEX idx_addresses_user ON addresses(user_id);
CREATE INDEX idx_addresses_type ON addresses(address_type);
CREATE INDEX idx_addresses_city ON addresses(city);
CREATE INDEX idx_addresses_postal_code ON addresses(postal_code);

-- Sessions
CREATE INDEX idx_sessions_user ON user_sessions(user_id);
CREATE INDEX idx_sessions_active ON user_sessions(user_id, is_active) WHERE is_active = true;
CREATE INDEX idx_sessions_device ON user_sessions(device_id);

-- Activity Logs
CREATE INDEX idx_activity_user ON user_activity_logs(user_id);
CREATE INDEX idx_activity_type ON user_activity_logs(activity_type);
CREATE INDEX idx_activity_created ON user_activity_logs(created_at DESC);
CREATE INDEX idx_activity_entity ON user_activity_logs(entity_type, entity_id);

-- Wishlists
CREATE INDEX idx_wishlists_user ON wishlists(user_id);
CREATE INDEX idx_wishlist_items_wishlist ON wishlist_items(wishlist_id);
CREATE INDEX idx_wishlist_items_product ON wishlist_items(product_id);

-- Payment Methods
CREATE INDEX idx_payment_methods_user ON saved_payment_methods(user_id);
CREATE INDEX idx_payment_methods_type ON saved_payment_methods(payment_type);
```

---

## Sample Data

```sql
-- Insert user (synced from Cognito)
INSERT INTO users (
    id, cognito_sub, email, first_name, last_name, phone,
    auth_provider, role, status, is_email_verified, preferences
) VALUES (
    'uuuuuuuu-0000-0000-0000-000000000001',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890', -- Cognito sub
    'john.doe@example.com',
    'John',
    'Doe',
    '+919876543210',
    'email',
    'customer',
    'active',
    true,
    '{
        "language": "en",
        "currency": "INR",
        "timezone": "Asia/Kolkata",
        "theme": "light"
    }'::jsonb
);

-- Insert addresses
INSERT INTO addresses (
    user_id, address_type, label, first_name, last_name, phone,
    address_line1, address_line2, city, state, postal_code, country,
    is_default_shipping, is_default_billing
) VALUES 
(
    'uuuuuuuu-0000-0000-0000-000000000001',
    'both', 'Home', 'John', 'Doe', '+919876543210',
    '123 Main Street', 'Apartment 4B', 'Mumbai', 'Maharashtra', '400001', 'India',
    true, true
),
(
    'uuuuuuuu-0000-0000-0000-000000000001',
    'shipping', 'Office', 'John', 'Doe', '+919876543211',
    '456 Business Park', 'Floor 5', 'Mumbai', 'Maharashtra', '400051', 'India',
    false, false
);

-- Insert notification preferences
INSERT INTO notification_preferences (user_id)
VALUES ('uuuuuuuu-0000-0000-0000-000000000001');
```

---

## Common Queries

### 1. Get User with Default Addresses

```sql
SELECT 
    u.*,
    (
        SELECT json_build_object(
            'id', a.id,
            'label', a.label,
            'address_line1', a.address_line1,
            'city', a.city,
            'postal_code', a.postal_code
        )
        FROM addresses a 
        WHERE a.user_id = u.id AND a.is_default_shipping = true
        LIMIT 1
    ) as default_shipping_address,
    (
        SELECT json_build_object(
            'id', a.id,
            'label', a.label,
            'address_line1', a.address_line1,
            'city', a.city,
            'postal_code', a.postal_code
        )
        FROM addresses a 
        WHERE a.user_id = u.id AND a.is_default_billing = true
        LIMIT 1
    ) as default_billing_address
FROM users u
WHERE u.cognito_sub = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    AND u.deleted_at IS NULL;
```

### 2. Get User Profile Complete

```sql
SELECT 
    u.id,
    u.email,
    u.first_name,
    u.last_name,
    u.phone,
    u.avatar_url,
    u.role,
    u.preferences,
    u.total_orders,
    u.total_spent,
    u.loyalty_points,
    (
        SELECT json_agg(
            json_build_object(
                'id', a.id,
                'type', a.address_type,
                'label', a.label,
                'full_address', a.address_line1 || ', ' || a.city || ' - ' || a.postal_code,
                'is_default_shipping', a.is_default_shipping,
                'is_default_billing', a.is_default_billing
            )
        )
        FROM addresses a WHERE a.user_id = u.id
    ) as addresses,
    np.email_promotions,
    np.sms_promotions,
    np.push_enabled
FROM users u
LEFT JOIN notification_preferences np ON np.user_id = u.id
WHERE u.id = 'uuuuuuuu-0000-0000-0000-000000000001';
```

### 3. Admin: Search Users

```sql
SELECT 
    u.id,
    u.email,
    u.first_name || ' ' || u.last_name as full_name,
    u.phone,
    u.role,
    u.status,
    u.total_orders,
    u.total_spent,
    u.created_at,
    u.last_login_at
FROM users u
WHERE 
    u.deleted_at IS NULL
    AND (
        u.email ILIKE '%search%'
        OR u.first_name ILIKE '%search%'
        OR u.last_name ILIKE '%search%'
        OR u.phone LIKE '%search%'
    )
ORDER BY u.created_at DESC
LIMIT 20 OFFSET 0;
```

---

## EF Core Entity Models

### User Entity

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmCart.UserService.Domain.Entities;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    [Column("cognito_sub")]
    public string CognitoSub { get; set; } = null!;
    
    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = null!;
    
    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }
    
    [Column("phone_verified")]
    public bool PhoneVerified { get; set; } = false;
    
    [MaxLength(500)]
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
    
    [Column("date_of_birth")]
    public DateOnly? DateOfBirth { get; set; }
    
    [MaxLength(20)]
    [Column("gender")]
    public string? Gender { get; set; }
    
    [MaxLength(20)]
    [Column("auth_provider")]
    public string AuthProvider { get; set; } = "email";
    
    [MaxLength(255)]
    [Column("google_id")]
    public string? GoogleId { get; set; }
    
    [MaxLength(255)]
    [Column("facebook_id")]
    public string? FacebookId { get; set; }
    
    [MaxLength(20)]
    [Column("role")]
    public string Role { get; set; } = "customer";
    
    [Column("permissions", TypeName = "jsonb")]
    public List<string> Permissions { get; set; } = new();
    
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active";
    
    [Column("is_email_verified")]
    public bool IsEmailVerified { get; set; } = false;
    
    [Column("email_verified_at")]
    public DateTime? EmailVerifiedAt { get; set; }
    
    [Column("preferences", TypeName = "jsonb")]
    public UserPreferences Preferences { get; set; } = new();
    
    [Column("metadata", TypeName = "jsonb")]
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    [Column("total_orders")]
    public int TotalOrders { get; set; } = 0;
    
    [Column("total_spent", TypeName = "decimal(12,2)")]
    public decimal TotalSpent { get; set; } = 0;
    
    [Column("loyalty_points")]
    public int LoyaltyPoints { get; set; } = 0;
    
    [Column("failed_login_attempts")]
    public int FailedLoginAttempts { get; set; } = 0;
    
    [Column("lockout_end")]
    public DateTime? LockoutEnd { get; set; }
    
    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }
    
    [MaxLength(45)]
    [Column("last_login_ip")]
    public string? LastLoginIp { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
    
    // Computed
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
    
    // Navigation
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
    public virtual NotificationPreferences? NotificationPreferences { get; set; }
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public virtual ICollection<SavedPaymentMethod> SavedPaymentMethods { get; set; } = new List<SavedPaymentMethod>();
}

public class UserPreferences
{
    public string Language { get; set; } = "en";
    public string Currency { get; set; } = "INR";
    public string Timezone { get; set; } = "Asia/Kolkata";
    public string Theme { get; set; } = "light";
}
```

### Address Entity

```csharp
[Table("addresses")]
public class Address
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("user_id")]
    public Guid UserId { get; set; }
    
    [MaxLength(20)]
    [Column("address_type")]
    public string AddressType { get; set; } = "shipping";
    
    [MaxLength(50)]
    [Column("label")]
    public string? Label { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = null!;
    
    [Required]
    [MaxLength(20)]
    [Column("phone")]
    public string Phone { get; set; } = null!;
    
    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }
    
    [MaxLength(200)]
    [Column("company")]
    public string? Company { get; set; }
    
    [Required]
    [MaxLength(255)]
    [Column("address_line1")]
    public string AddressLine1 { get; set; } = null!;
    
    [MaxLength(255)]
    [Column("address_line2")]
    public string? AddressLine2 { get; set; }
    
    [MaxLength(255)]
    [Column("landmark")]
    public string? Landmark { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Column("city")]
    public string City { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    [Column("state")]
    public string State { get; set; } = null!;
    
    [Required]
    [MaxLength(20)]
    [Column("postal_code")]
    public string PostalCode { get; set; } = null!;
    
    [MaxLength(100)]
    [Column("country")]
    public string Country { get; set; } = "India";
    
    [MaxLength(2)]
    [Column("country_code")]
    public string CountryCode { get; set; } = "IN";
    
    [Column("coordinates", TypeName = "jsonb")]
    public Coordinates? Coordinates { get; set; }
    
    [Column("is_default_shipping")]
    public bool IsDefaultShipping { get; set; } = false;
    
    [Column("is_default_billing")]
    public bool IsDefaultBilling { get; set; } = false;
    
    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;
    
    [MaxLength(15)]
    [Column("gstin")]
    public string? Gstin { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual User User { get; set; } = null!;
}

public class Coordinates
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}
```

---

## Cognito Sync

### Lambda Trigger: Post Confirmation

```csharp
// Lambda function triggered after Cognito email verification
public class CognitoPostConfirmationHandler
{
    private readonly HttpClient _httpClient;
    
    public async Task<CognitoTriggerResponse> HandleAsync(CognitoPostConfirmationEvent @event)
    {
        var userAttributes = @event.Request.UserAttributes;
        
        var syncRequest = new UserSyncRequest
        {
            CognitoSub = userAttributes["sub"],
            Email = userAttributes["email"],
            FirstName = userAttributes.GetValueOrDefault("given_name", ""),
            LastName = userAttributes.GetValueOrDefault("family_name", ""),
            Phone = userAttributes.GetValueOrDefault("phone_number"),
            AuthProvider = GetAuthProvider(userAttributes),
            GoogleId = userAttributes.GetValueOrDefault("custom:google_id"),
            FacebookId = userAttributes.GetValueOrDefault("custom:facebook_id"),
            AvatarUrl = userAttributes.GetValueOrDefault("picture"),
            IsEmailVerified = true
        };
        
        await _httpClient.PostAsJsonAsync(
            "https://api.amcart.com/internal/users/sync",
            syncRequest
        );
        
        return @event.Response;
    }
    
    private string GetAuthProvider(Dictionary<string, string> attrs)
    {
        if (attrs.ContainsKey("identities"))
        {
            var identities = JsonSerializer.Deserialize<List<CognitoIdentity>>(attrs["identities"]);
            return identities?.FirstOrDefault()?.ProviderName?.ToLower() ?? "email";
        }
        return "email";
    }
}
```

### Internal Sync Endpoint

```csharp
[ApiController]
[Route("internal/users")]
public class UserSyncController : ControllerBase
{
    private readonly IUserService _userService;
    
    [HttpPost("sync")]
    public async Task<IActionResult> SyncUser([FromBody] UserSyncRequest request)
    {
        var existingUser = await _userService.GetByCognitoSubAsync(request.CognitoSub);
        
        if (existingUser == null)
        {
            // Create new user
            var user = new User
            {
                CognitoSub = request.CognitoSub,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                AuthProvider = request.AuthProvider,
                GoogleId = request.GoogleId,
                FacebookId = request.FacebookId,
                AvatarUrl = request.AvatarUrl,
                IsEmailVerified = request.IsEmailVerified,
                Status = "active"
            };
            
            await _userService.CreateAsync(user);
            
            // Create default notification preferences
            await _userService.CreateNotificationPreferencesAsync(user.Id);
            
            // Create default wishlist
            await _userService.CreateDefaultWishlistAsync(user.Id);
        }
        else
        {
            // Update existing user
            existingUser.Email = request.Email;
            existingUser.FirstName = request.FirstName;
            existingUser.LastName = request.LastName;
            existingUser.AvatarUrl = request.AvatarUrl ?? existingUser.AvatarUrl;
            
            await _userService.UpdateAsync(existingUser);
        }
        
        return Ok();
    }
}
```

---

## Summary

| Table | Purpose | Key Features |
|-------|---------|--------------|
| `users` | Core user data | Cognito sync, JSONB preferences |
| `addresses` | Shipping/billing addresses | Default flags, coordinates |
| `user_sessions` | Active sessions tracking | Device info, location |
| `notification_preferences` | Communication settings | Email, SMS, push preferences |
| `user_activity_logs` | Audit trail | All user actions |
| `wishlists` | Product wishlists | Shareable, multiple lists |
| `wishlist_items` | Wishlist products | Price tracking |
| `saved_payment_methods` | Tokenized payment info | Cards, UPI, wallets |

This schema supports:
- ✅ AWS Cognito integration
- ✅ Multiple addresses per user
- ✅ Social login (Google, Facebook)
- ✅ Notification preferences
- ✅ Session management
- ✅ Activity audit trail
- ✅ Wishlists with sharing
- ✅ Saved payment methods
- ✅ Soft delete

