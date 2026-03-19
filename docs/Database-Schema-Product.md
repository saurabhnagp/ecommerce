# Product Database Schema

## PostgreSQL + JSONB Approach for AmCart E-commerce

---

## Table of Contents

1. [Overview](#overview)
2. [Entity Relationship Diagram](#entity-relationship-diagram)
3. [Tables Schema](#tables-schema)
4. [Indexes](#indexes)
5. [Sample Data](#sample-data)
6. [Common Queries](#common-queries)
7. [EF Core Entity Models](#ef-core-entity-models)
8. [OpenSearch Sync](#opensearch-sync)

---

## Overview

### Design Principles

| Principle | Implementation |
|-----------|----------------|
| **Source of Truth** | PostgreSQL stores all product data |
| **Flexible Attributes** | JSONB columns for category-specific fields |
| **Performance** | Strategic indexes + Redis cache + OpenSearch |
| **Data Integrity** | Foreign keys, constraints, triggers |
| **Audit Trail** | Created/Updated timestamps, soft delete |

### Database: `amcart_products`

```
┌──────────────────────────────────────────────────────────────────────────┐
│                        Product Database Schema                            │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────────────┐      │
│  │ categories  │◀────▶│  products   │─────▶│  product_variants   │      │
│  │             │      │             │      │                     │      │
│  │ (hierarchy) │      │ (JSONB for  │      │ (size, color, SKU)  │      │
│  └─────────────┘      │  attributes)│      └──────────┬──────────┘      │
│        │              └──────┬──────┘                 │                  │
│        │                     │                        │                  │
│        ▼                     ▼                        ▼                  │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────────────┐      │
│  │ category_   │      │  product_   │      │     inventory       │      │
│  │ attributes  │      │   images    │      │         │           │      │
│  │ (templates) │      │             │      │  (stock tracking)   │      │
│  └─────────────┘      └─────────────┘      └─────────┬───────────┘      │
│                                                      │                   │
│  ┌─────────────┐      ┌─────────────┐      ┌────────▼────────────┐      │
│  │   brands    │      │   sellers   │      │    warehouses       │      │
│  └─────────────┘      └─────────────┘      │  (multi-location)   │      │
│                                            └─────────────────────┘      │
│  ┌───────────────────────────────────────────────────────────────┐      │
│  │                      price_history                             │      │
│  └───────────────────────────────────────────────────────────────┘      │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## Entity Relationship Diagram

```
                                    ┌─────────────────┐
                                    │     sellers     │
                                    ├─────────────────┤
                                    │ id (PK)         │
                                    │ user_id (FK)    │
                                    │ business_name   │
                                    │ rating          │
                                    │ is_verified     │
                                    └────────┬────────┘
                                             │
                                             │ 1:N
                                             ▼
┌─────────────────┐    1:N    ┌─────────────────────────────────────┐
│     brands      │◀──────────│              products               │
├─────────────────┤           ├─────────────────────────────────────┤
│ id (PK)         │           │ id (PK)                             │
│ name            │           │ seller_id (FK)                      │
│ slug            │           │ brand_id (FK)                       │
│ logo_url        │           │ category_id (FK)                    │
│ description     │           │ name                                │
└─────────────────┘           │ slug                                │
                              │ description                         │
┌─────────────────┐    1:N    │ short_description                   │
│   categories    │◀──────────│ base_price                          │
├─────────────────┤           │ compare_at_price                    │
│ id (PK)         │           │ cost_price                          │
│ parent_id (FK)  │           │ attributes (JSONB)                  │
│ name            │           │ specifications (JSONB)              │
│ slug            │           │ tags (JSONB)                        │
│ level           │           │ seo_metadata (JSONB)                │
│ path            │           │ status                              │
│ attribute_      │           │ is_featured                         │
│   template      │           │ created_at                          │
└─────────────────┘           │ updated_at                          │
                              └───────────────┬─────────────────────┘
                                              │
                    ┌─────────────────────────┼─────────────────────────┐
                    │                         │                         │
                    ▼ 1:N                     ▼ 1:N                     ▼ 1:N
        ┌───────────────────┐     ┌───────────────────┐     ┌───────────────────┐
        │  product_images   │     │ product_variants  │     │  price_history    │
        ├───────────────────┤     ├───────────────────┤     ├───────────────────┤
        │ id (PK)           │     │ id (PK)           │     │ id (PK)           │
        │ product_id (FK)   │     │ product_id (FK)   │     │ product_id (FK)   │
        │ url               │     │ sku               │     │ variant_id (FK)   │
        │ alt_text          │     │ name              │     │ price             │
        │ is_primary        │     │ options (JSONB)   │     │ effective_from    │
        │ sort_order        │     │ price_adjustment  │     │ effective_to      │
        └───────────────────┘     │ stock_quantity    │     │ reason            │
                                  │ is_active         │     └───────────────────┘
                                  └─────────┬─────────┘
                                            │
                                            ▼ 1:N
                                  ┌───────────────────┐
                                  │    inventory      │
                                  ├───────────────────┤
                                  │ id (PK)           │
                                  │ variant_id (FK)   │
                                  │ warehouse_id (FK) │
                                  │ quantity          │
                                  │ reserved          │
                                  │ reorder_level     │
                                  │ reorder_quantity  │
                                  └─────────┬─────────┘
                                            │
                                            ▼ N:1
                                  ┌───────────────────┐
                                  │    warehouses     │
                                  ├───────────────────┤
                                  │ id (PK)           │
                                  │ code (unique)     │
                                  │ name              │
                                  │ seller_id (FK)    │
                                  │ city              │
                                  │ state             │
                                  │ is_active         │
                                  └───────────────────┘
```

---

## Tables Schema

### 1. Categories Table

```sql
-- Categories with hierarchical structure (Adjacency List + Materialized Path)
CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id UUID REFERENCES categories(id) ON DELETE SET NULL,
    
    -- Basic Info
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    image_url VARCHAR(500),
    
    -- Hierarchy
    level INTEGER NOT NULL DEFAULT 0,
    path VARCHAR(500) NOT NULL DEFAULT '', -- e.g., '/men/clothing/tshirts'
    path_ids UUID[] DEFAULT '{}',          -- Array of ancestor IDs for easy queries
    
    -- For flexible product attributes
    attribute_template JSONB DEFAULT '[]',
    -- Example: [
    --   {"name": "size", "type": "select", "options": ["XS","S","M","L","XL"], "required": true},
    --   {"name": "color", "type": "select", "options": ["Red","Blue","Green"], "required": true},
    --   {"name": "material", "type": "text", "required": false}
    -- ]
    
    -- Display
    sort_order INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    is_featured BOOLEAN DEFAULT false,
    
    -- SEO
    meta_title VARCHAR(200),
    meta_description VARCHAR(500),
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Auto-update path when category changes
CREATE OR REPLACE FUNCTION update_category_path()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.parent_id IS NULL THEN
        NEW.level := 0;
        NEW.path := '/' || NEW.slug;
        NEW.path_ids := ARRAY[NEW.id];
    ELSE
        SELECT 
            level + 1,
            path || '/' || NEW.slug,
            path_ids || NEW.id
        INTO NEW.level, NEW.path, NEW.path_ids
        FROM categories 
        WHERE id = NEW.parent_id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_update_category_path
    BEFORE INSERT OR UPDATE ON categories
    FOR EACH ROW
    EXECUTE FUNCTION update_category_path();
```

### 2. Brands Table

```sql
CREATE TABLE brands (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Basic Info
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    
    -- Media
    logo_url VARCHAR(500),
    banner_url VARCHAR(500),
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    is_featured BOOLEAN DEFAULT false,
    
    -- SEO
    meta_title VARCHAR(200),
    meta_description VARCHAR(500),
    website_url VARCHAR(500),
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### 3. Sellers Table

```sql
CREATE TABLE sellers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL, -- Reference to User Service (Cognito sub)
    
    -- Business Info
    business_name VARCHAR(200) NOT NULL,
    business_type VARCHAR(50), -- 'individual', 'company', 'brand'
    description TEXT,
    
    -- Contact
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    
    -- Address
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(100),
    postal_code VARCHAR(20),
    country VARCHAR(100) DEFAULT 'India',
    
    -- Verification
    is_verified BOOLEAN DEFAULT false,
    verification_documents JSONB DEFAULT '[]',
    verified_at TIMESTAMP WITH TIME ZONE,
    
    -- Ratings
    rating DECIMAL(3,2) DEFAULT 0.00,
    total_reviews INTEGER DEFAULT 0,
    total_products INTEGER DEFAULT 0,
    total_sales INTEGER DEFAULT 0,
    
    -- Banking (encrypted in production)
    bank_account_info JSONB, -- Store encrypted
    
    -- Commission
    commission_rate DECIMAL(5,2) DEFAULT 10.00, -- Percentage
    
    -- Status
    status VARCHAR(20) DEFAULT 'pending', -- 'pending', 'active', 'suspended', 'inactive'
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT valid_status CHECK (status IN ('pending', 'active', 'suspended', 'inactive'))
);
```

### 4. Products Table (Main)

```sql
CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Relationships
    seller_id UUID NOT NULL REFERENCES sellers(id) ON DELETE CASCADE,
    brand_id UUID REFERENCES brands(id) ON DELETE SET NULL,
    category_id UUID NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    
    -- Basic Info
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    description TEXT,
    short_description VARCHAR(500),
    
    -- Pricing (base price, variants may have adjustments)
    base_price DECIMAL(12,2) NOT NULL,
    compare_at_price DECIMAL(12,2), -- Original price for showing discounts
    cost_price DECIMAL(12,2),       -- For profit calculations
    currency VARCHAR(3) DEFAULT 'INR',
    
    -- Tax
    tax_category VARCHAR(50) DEFAULT 'standard',
    hsn_code VARCHAR(20), -- Harmonized System Nomenclature for India GST
    
    -- Flexible Attributes (JSONB)
    attributes JSONB DEFAULT '{}',
    -- Example for clothing:
    -- {
    --   "material": "100% Cotton",
    --   "fit": "Regular",
    --   "care_instructions": "Machine wash cold",
    --   "country_of_origin": "India"
    -- }
    
    -- Technical Specifications (JSONB)
    specifications JSONB DEFAULT '{}',
    -- Example for electronics:
    -- {
    --   "screen_size": "6.5 inches",
    --   "ram": "8GB",
    --   "storage": "128GB",
    --   "battery": "5000mAh"
    -- }
    
    -- Tags for search and filtering
    tags JSONB DEFAULT '[]',
    -- Example: ["summer", "casual", "bestseller", "new-arrival"]
    
    -- Physical Properties (for shipping)
    weight DECIMAL(10,3),           -- in kg
    weight_unit VARCHAR(10) DEFAULT 'kg',
    dimensions JSONB DEFAULT '{}',  -- {"length": 10, "width": 5, "height": 2, "unit": "cm"}
    
    -- SEO Metadata (JSONB)
    seo_metadata JSONB DEFAULT '{}',
    -- {
    --   "title": "Men's Cotton T-Shirt | AmCart",
    --   "description": "Buy premium cotton t-shirts...",
    --   "keywords": ["cotton tshirt", "mens wear"],
    --   "canonical_url": "/products/mens-cotton-tshirt"
    -- }
    
    -- Status
    status VARCHAR(20) DEFAULT 'draft',
    -- 'draft', 'pending_review', 'active', 'inactive', 'out_of_stock', 'discontinued'
    
    -- Visibility
    is_active BOOLEAN DEFAULT true,
    is_featured BOOLEAN DEFAULT false,
    is_new BOOLEAN DEFAULT true,
    is_bestseller BOOLEAN DEFAULT false,
    
    -- Publishing
    published_at TIMESTAMP WITH TIME ZONE,
    
    -- Stats (denormalized for performance)
    view_count INTEGER DEFAULT 0,
    sales_count INTEGER DEFAULT 0,
    review_count INTEGER DEFAULT 0,
    average_rating DECIMAL(3,2) DEFAULT 0.00,
    
    -- Inventory summary (denormalized)
    total_stock INTEGER DEFAULT 0,
    has_variants BOOLEAN DEFAULT false,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP WITH TIME ZONE, -- Soft delete
    
    -- Constraints
    CONSTRAINT valid_status CHECK (status IN ('draft', 'pending_review', 'active', 'inactive', 'out_of_stock', 'discontinued')),
    CONSTRAINT positive_price CHECK (base_price >= 0),
    CONSTRAINT valid_rating CHECK (average_rating >= 0 AND average_rating <= 5),
    
    -- Unique slug per seller
    CONSTRAINT unique_product_slug UNIQUE (seller_id, slug)
);
```

### 5. Product Variants Table

```sql
CREATE TABLE product_variants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    
    -- SKU (Stock Keeping Unit)
    sku VARCHAR(100) NOT NULL UNIQUE,
    barcode VARCHAR(100), -- UPC, EAN, ISBN
    
    -- Variant Name (auto-generated from options)
    name VARCHAR(255), -- e.g., "Red / Large"
    
    -- Variant Options (JSONB)
    options JSONB NOT NULL DEFAULT '{}',
    -- Example: {"color": "Red", "size": "L"}
    -- Example: {"storage": "128GB", "color": "Black"}
    
    -- Pricing
    price_adjustment DECIMAL(12,2) DEFAULT 0.00, -- Added to base_price
    -- Final price = product.base_price + variant.price_adjustment
    
    override_price DECIMAL(12,2), -- If set, ignores base_price + adjustment
    cost_price DECIMAL(12,2),
    
    -- Stock (managed in inventory table, cached here)
    stock_quantity INTEGER DEFAULT 0,
    
    -- Physical Properties (if different from product)
    weight DECIMAL(10,3),
    dimensions JSONB,
    
    -- Media
    image_url VARCHAR(500),
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    is_default BOOLEAN DEFAULT false, -- Default variant shown
    
    -- Sort
    sort_order INTEGER DEFAULT 0,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT positive_stock CHECK (stock_quantity >= 0)
);

-- Ensure only one default variant per product
CREATE UNIQUE INDEX idx_one_default_variant 
    ON product_variants(product_id) 
    WHERE is_default = true;
```

### 6. Product Images Table

```sql
CREATE TABLE product_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    variant_id UUID REFERENCES product_variants(id) ON DELETE CASCADE,
    
    -- Image URLs
    url VARCHAR(500) NOT NULL,
    thumbnail_url VARCHAR(500),
    medium_url VARCHAR(500),
    large_url VARCHAR(500),
    
    -- Metadata
    alt_text VARCHAR(255),
    title VARCHAR(255),
    
    -- Type
    image_type VARCHAR(20) DEFAULT 'gallery', -- 'primary', 'gallery', 'thumbnail', 'zoom'
    
    -- Display
    is_primary BOOLEAN DEFAULT false,
    sort_order INTEGER DEFAULT 0,
    
    -- File info
    file_size INTEGER, -- in bytes
    width INTEGER,
    height INTEGER,
    format VARCHAR(10), -- 'jpg', 'png', 'webp'
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Ensure only one primary image per product
CREATE UNIQUE INDEX idx_one_primary_image 
    ON product_images(product_id) 
    WHERE is_primary = true AND variant_id IS NULL;
```

### 7. Warehouses Table

```sql
CREATE TABLE warehouses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Identifier
    code VARCHAR(20) NOT NULL UNIQUE,  -- 'WH-MUM-01', 'WH-DEL-01'
    name VARCHAR(100) NOT NULL,
    
    -- Type
    warehouse_type VARCHAR(30) DEFAULT 'fulfillment',
    -- 'fulfillment' - Platform warehouse
    -- 'seller' - Seller's own warehouse
    -- 'returns' - Returns processing center
    -- 'dark_store' - Quick commerce store
    
    -- Owner (for marketplace - seller warehouses)
    seller_id UUID REFERENCES sellers(id) ON DELETE CASCADE,
    -- NULL for platform-owned warehouses
    
    -- Address
    address_line1 VARCHAR(255) NOT NULL,
    address_line2 VARCHAR(255),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(100) NOT NULL,
    postal_code VARCHAR(20) NOT NULL,
    country VARCHAR(100) DEFAULT 'India',
    
    -- Geolocation (for delivery distance calculation)
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    
    -- Service Area
    serviceable_pincodes JSONB DEFAULT '[]',
    -- ["400001", "400002", "400003"] or use pincode ranges
    max_delivery_radius_km INTEGER, -- Alternative to pincode list
    
    -- Contact
    contact_name VARCHAR(100),
    contact_phone VARCHAR(20),
    contact_email VARCHAR(255),
    
    -- Operating Hours
    operating_hours JSONB DEFAULT '{}',
    -- {
    --   "monday": {"open": "09:00", "close": "18:00"},
    --   "tuesday": {"open": "09:00", "close": "18:00"},
    --   ...
    -- }
    
    -- Capacity
    max_capacity INTEGER,           -- Maximum SKUs/items
    current_utilization INTEGER DEFAULT 0,
    storage_type VARCHAR(50),       -- 'ambient', 'cold_storage', 'mixed'
    
    -- Priority (for fulfillment selection)
    priority INTEGER DEFAULT 0,     -- Higher = preferred
    
    -- Capabilities
    supports_cod BOOLEAN DEFAULT true,
    supports_returns BOOLEAN DEFAULT true,
    supports_same_day BOOLEAN DEFAULT false,
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    is_accepting_inventory BOOLEAN DEFAULT true,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Index for finding warehouses by location
CREATE INDEX idx_warehouses_location ON warehouses(city, state);
CREATE INDEX idx_warehouses_seller ON warehouses(seller_id) WHERE seller_id IS NOT NULL;
CREATE INDEX idx_warehouses_active ON warehouses(is_active) WHERE is_active = true;
CREATE INDEX idx_warehouses_type ON warehouses(warehouse_type);

-- Geospatial index (if using PostGIS extension)
-- CREATE INDEX idx_warehouses_geo ON warehouses USING GIST (
--     ST_SetSRID(ST_MakePoint(longitude, latitude), 4326)
-- );

CREATE TRIGGER trigger_warehouses_updated_at
    BEFORE UPDATE ON warehouses
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

### 8. Inventory Table

```sql
CREATE TABLE inventory (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    variant_id UUID NOT NULL REFERENCES product_variants(id) ON DELETE CASCADE,
    
    -- Warehouse Reference
    warehouse_id UUID REFERENCES warehouses(id) ON DELETE CASCADE,
    -- NULL means "default/unassigned" warehouse
    
    -- Bin/Shelf Location within warehouse
    location_code VARCHAR(50), -- 'A-12-3' (Aisle-Shelf-Bin)
    
    -- Stock Levels
    quantity INTEGER NOT NULL DEFAULT 0,
    reserved_quantity INTEGER DEFAULT 0, -- Reserved for pending orders
    available_quantity INTEGER GENERATED ALWAYS AS (quantity - reserved_quantity) STORED,
    
    -- Thresholds
    reorder_level INTEGER DEFAULT 10,    -- Alert when stock falls below
    reorder_quantity INTEGER DEFAULT 50, -- Suggested reorder amount
    max_quantity INTEGER,                -- Maximum stock level
    
    -- Tracking
    last_restock_at TIMESTAMP WITH TIME ZONE,
    last_sold_at TIMESTAMP WITH TIME ZONE,
    
    -- Cost Tracking (for FIFO/LIFO)
    average_cost DECIMAL(12,2),  -- Moving average cost
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT positive_quantity CHECK (quantity >= 0),
    CONSTRAINT positive_reserved CHECK (reserved_quantity >= 0),
    CONSTRAINT reserved_not_exceed CHECK (reserved_quantity <= quantity),
    
    -- One inventory record per variant per warehouse
    CONSTRAINT unique_variant_warehouse UNIQUE (variant_id, warehouse_id)
);

-- Index for low stock alerts
CREATE INDEX idx_inventory_low_stock ON inventory(available_quantity) 
    WHERE available_quantity <= reorder_level;
```

### 9. Inventory Transactions Table (Audit)

```sql
CREATE TABLE inventory_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    inventory_id UUID NOT NULL REFERENCES inventory(id),
    variant_id UUID NOT NULL REFERENCES product_variants(id),
    
    -- Transaction Type
    transaction_type VARCHAR(30) NOT NULL,
    -- 'restock', 'sale', 'return', 'adjustment', 'reserved', 'released', 'damaged', 'transfer'
    
    -- Quantity Change
    quantity_change INTEGER NOT NULL, -- Positive for increase, negative for decrease
    quantity_before INTEGER NOT NULL,
    quantity_after INTEGER NOT NULL,
    
    -- Reference
    reference_type VARCHAR(50), -- 'order', 'return', 'manual', 'sync'
    reference_id UUID,          -- Order ID, Return ID, etc.
    
    -- Notes
    notes TEXT,
    performed_by UUID, -- User ID who performed the action
    
    -- Timestamp
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### 10. Price History Table

```sql
CREATE TABLE price_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    variant_id UUID REFERENCES product_variants(id) ON DELETE CASCADE,
    
    -- Price Info
    price DECIMAL(12,2) NOT NULL,
    compare_at_price DECIMAL(12,2),
    currency VARCHAR(3) DEFAULT 'INR',
    
    -- Validity
    effective_from TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    effective_to TIMESTAMP WITH TIME ZONE,
    
    -- Reason
    reason VARCHAR(100), -- 'initial', 'promotion', 'price_increase', 'price_decrease', 'sale'
    notes TEXT,
    
    -- Who changed it
    changed_by UUID,
    
    -- Timestamp
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

### 11. Product Collections Table (Optional)

```sql
-- For grouping products (e.g., "Summer Collection", "Best Sellers")
CREATE TABLE collections (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    image_url VARCHAR(500),
    
    -- Type
    collection_type VARCHAR(30) DEFAULT 'manual',
    -- 'manual', 'automated' (based on rules)
    
    -- Rules for automated collections (JSONB)
    rules JSONB DEFAULT '{}',
    -- Example: {"category_id": "uuid", "min_price": 500, "tags": ["summer"]}
    
    -- Display
    is_active BOOLEAN DEFAULT true,
    is_featured BOOLEAN DEFAULT false,
    sort_order INTEGER DEFAULT 0,
    
    -- Validity
    start_date TIMESTAMP WITH TIME ZONE,
    end_date TIMESTAMP WITH TIME ZONE,
    
    -- Timestamps
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE collection_products (
    collection_id UUID NOT NULL REFERENCES collections(id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    sort_order INTEGER DEFAULT 0,
    added_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    PRIMARY KEY (collection_id, product_id)
);
```

---

## Indexes

```sql
-- Categories
CREATE INDEX idx_categories_parent ON categories(parent_id);
CREATE INDEX idx_categories_slug ON categories(slug);
CREATE INDEX idx_categories_path ON categories(path);
CREATE INDEX idx_categories_active ON categories(is_active) WHERE is_active = true;

-- Brands
CREATE INDEX idx_brands_slug ON brands(slug);
CREATE INDEX idx_brands_active ON brands(is_active) WHERE is_active = true;

-- Sellers
CREATE INDEX idx_sellers_user ON sellers(user_id);
CREATE INDEX idx_sellers_status ON sellers(status);
CREATE INDEX idx_sellers_verified ON sellers(is_verified) WHERE is_verified = true;

-- Products (most important)
CREATE INDEX idx_products_seller ON products(seller_id);
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_brand ON products(brand_id);
CREATE INDEX idx_products_slug ON products(slug);
CREATE INDEX idx_products_status ON products(status);
CREATE INDEX idx_products_active ON products(is_active, status) WHERE is_active = true;
CREATE INDEX idx_products_featured ON products(is_featured) WHERE is_featured = true;
CREATE INDEX idx_products_price ON products(base_price);
CREATE INDEX idx_products_rating ON products(average_rating DESC);
CREATE INDEX idx_products_sales ON products(sales_count DESC);
CREATE INDEX idx_products_created ON products(created_at DESC);

-- JSONB Indexes (GIN for flexible queries)
CREATE INDEX idx_products_attributes ON products USING GIN (attributes);
CREATE INDEX idx_products_specifications ON products USING GIN (specifications);
CREATE INDEX idx_products_tags ON products USING GIN (tags);

-- Composite indexes for common queries
CREATE INDEX idx_products_category_active_price ON products(category_id, is_active, base_price) 
    WHERE is_active = true AND status = 'active';
CREATE INDEX idx_products_seller_status ON products(seller_id, status);

-- Full-text search (backup to OpenSearch)
CREATE INDEX idx_products_search ON products USING GIN (
    to_tsvector('english', name || ' ' || COALESCE(description, '') || ' ' || COALESCE(short_description, ''))
);

-- Variants
CREATE INDEX idx_variants_product ON product_variants(product_id);
CREATE INDEX idx_variants_sku ON product_variants(sku);
CREATE INDEX idx_variants_options ON product_variants USING GIN (options);
CREATE INDEX idx_variants_active ON product_variants(is_active) WHERE is_active = true;

-- Images
CREATE INDEX idx_images_product ON product_images(product_id);
CREATE INDEX idx_images_variant ON product_images(variant_id);

-- Inventory
CREATE INDEX idx_inventory_variant ON inventory(variant_id);
CREATE INDEX idx_inventory_low_stock ON inventory(available_quantity) 
    WHERE available_quantity <= reorder_level;

-- Price History
CREATE INDEX idx_price_history_product ON price_history(product_id);
CREATE INDEX idx_price_history_variant ON price_history(variant_id);
CREATE INDEX idx_price_history_dates ON price_history(effective_from, effective_to);
```

---

## Sample Data

### Categories

```sql
INSERT INTO categories (id, parent_id, name, slug, level, attribute_template) VALUES
-- Level 0 (Root)
('11111111-0000-0000-0000-000000000001', NULL, 'Men', 'men', 0, '[]'),
('11111111-0000-0000-0000-000000000002', NULL, 'Women', 'women', 0, '[]'),

-- Level 1
('22222222-0000-0000-0000-000000000001', '11111111-0000-0000-0000-000000000001', 'Clothing', 'men-clothing', 1, '[]'),
('22222222-0000-0000-0000-000000000002', '11111111-0000-0000-0000-000000000002', 'Clothing', 'women-clothing', 1, '[]'),

-- Level 2 (with attribute templates)
('33333333-0000-0000-0000-000000000001', '22222222-0000-0000-0000-000000000001', 'T-Shirts', 'men-tshirts', 2, 
 '[{"name": "size", "type": "select", "options": ["XS","S","M","L","XL","XXL"], "required": true},
   {"name": "color", "type": "select", "options": [], "required": true},
   {"name": "material", "type": "text", "required": false},
   {"name": "fit", "type": "select", "options": ["Slim","Regular","Loose"], "required": false}]'),

('33333333-0000-0000-0000-000000000002', '22222222-0000-0000-0000-000000000001', 'Jeans', 'men-jeans', 2,
 '[{"name": "waist", "type": "select", "options": ["28","30","32","34","36","38"], "required": true},
   {"name": "length", "type": "select", "options": ["30","32","34"], "required": true},
   {"name": "fit", "type": "select", "options": ["Skinny","Slim","Regular","Relaxed"], "required": true}]');
```

### Brands

```sql
INSERT INTO brands (id, name, slug, logo_url) VALUES
('aaaaaaaa-0000-0000-0000-000000000001', 'Nike', 'nike', 'https://cdn.amcart.com/brands/nike.png'),
('aaaaaaaa-0000-0000-0000-000000000002', 'Adidas', 'adidas', 'https://cdn.amcart.com/brands/adidas.png'),
('aaaaaaaa-0000-0000-0000-000000000003', 'Levis', 'levis', 'https://cdn.amcart.com/brands/levis.png');
```

### Warehouses

```sql
-- Platform warehouses
INSERT INTO warehouses (
    id, code, name, warehouse_type, seller_id,
    address_line1, city, state, postal_code, country,
    latitude, longitude, contact_name, contact_phone,
    is_active, priority, supports_same_day
) VALUES 
-- Mumbai Warehouse (Primary)
(
    'wwwwwwww-0000-0000-0000-000000000001',
    'WH-MUM-01',
    'Mumbai Fulfillment Center',
    'fulfillment',
    NULL, -- Platform owned
    'Plot 45, MIDC Industrial Area, Andheri East',
    'Mumbai',
    'Maharashtra',
    '400093',
    'India',
    19.1136,
    72.8697,
    'Rajesh Kumar',
    '+919876543210',
    true,
    10, -- High priority
    true -- Same day delivery
),
-- Delhi Warehouse
(
    'wwwwwwww-0000-0000-0000-000000000002',
    'WH-DEL-01',
    'Delhi NCR Fulfillment Center',
    'fulfillment',
    NULL,
    'Sector 63, Noida',
    'Noida',
    'Uttar Pradesh',
    '201301',
    'India',
    28.6139,
    77.2090,
    'Amit Singh',
    '+919876543211',
    true,
    9,
    true
),
-- Seller Warehouse Example
(
    'wwwwwwww-0000-0000-0000-000000000003',
    'WH-SEL-001',
    'Nike India Warehouse',
    'seller',
    'ssssssss-0000-0000-0000-000000000001', -- Nike seller
    '123 Industrial Estate, Gurgaon',
    'Gurgaon',
    'Haryana',
    '122001',
    'India',
    28.4595,
    77.0266,
    'Seller Contact',
    '+919876543212',
    true,
    5,
    false
);
```

### Products with JSONB Attributes

```sql
INSERT INTO products (
    id, seller_id, brand_id, category_id, name, slug, description, short_description,
    base_price, compare_at_price, attributes, specifications, tags, status, has_variants
) VALUES (
    'pppppppp-0000-0000-0000-000000000001',
    'ssssssss-0000-0000-0000-000000000001', -- seller_id
    'aaaaaaaa-0000-0000-0000-000000000001', -- Nike
    '33333333-0000-0000-0000-000000000001', -- Men's T-Shirts
    'Nike Dri-FIT Cotton T-Shirt',
    'nike-dri-fit-cotton-tshirt',
    'Premium cotton t-shirt with Dri-FIT technology for superior comfort and moisture-wicking performance.',
    'Premium cotton t-shirt with Dri-FIT technology',
    1299.00,
    1599.00,
    '{
        "material": "100% Cotton with Dri-FIT",
        "fit": "Regular",
        "care_instructions": "Machine wash cold, tumble dry low",
        "country_of_origin": "Vietnam",
        "neck_type": "Round Neck",
        "sleeve_type": "Half Sleeve",
        "pattern": "Solid"
    }'::jsonb,
    '{
        "fabric_weight": "180 GSM",
        "stretch": "Non-stretch"
    }'::jsonb,
    '["summer", "casual", "sports", "bestseller"]'::jsonb,
    'active',
    true
);
```

### Product Variants

```sql
INSERT INTO product_variants (
    id, product_id, sku, name, options, price_adjustment, stock_quantity, is_default
) VALUES
-- Small sizes
('vvvvvvvv-0000-0000-0000-000000000001', 'pppppppp-0000-0000-0000-000000000001', 
 'NIKE-DF-BLK-S', 'Black / S', '{"color": "Black", "size": "S"}'::jsonb, 0.00, 50, false),
('vvvvvvvv-0000-0000-0000-000000000002', 'pppppppp-0000-0000-0000-000000000001', 
 'NIKE-DF-BLK-M', 'Black / M', '{"color": "Black", "size": "M"}'::jsonb, 0.00, 100, true),
('vvvvvvvv-0000-0000-0000-000000000003', 'pppppppp-0000-0000-0000-000000000001', 
 'NIKE-DF-BLK-L', 'Black / L', '{"color": "Black", "size": "L"}'::jsonb, 0.00, 75, false),

-- White variants
('vvvvvvvv-0000-0000-0000-000000000004', 'pppppppp-0000-0000-0000-000000000001', 
 'NIKE-DF-WHT-M', 'White / M', '{"color": "White", "size": "M"}'::jsonb, 0.00, 80, false),
('vvvvvvvv-0000-0000-0000-000000000005', 'pppppppp-0000-0000-0000-000000000001', 
 'NIKE-DF-WHT-L', 'White / L', '{"color": "White", "size": "L"}'::jsonb, 0.00, 60, false),

-- XXL costs more
('vvvvvvvv-0000-0000-0000-000000000006', 'pppppppp-0000-0000-0000-000000000001', 
 'NIKE-DF-BLK-XXL', 'Black / XXL', '{"color": "Black", "size": "XXL"}'::jsonb, 100.00, 30, false);
```

---

## Common Queries

### 1. Get Products by Category with Filters

```sql
-- Products in Men's T-Shirts, price range, with pagination
SELECT 
    p.id,
    p.name,
    p.slug,
    p.base_price,
    p.compare_at_price,
    p.average_rating,
    p.review_count,
    p.attributes,
    b.name as brand_name,
    (SELECT url FROM product_images WHERE product_id = p.id AND is_primary = true LIMIT 1) as image_url,
    (SELECT MIN(stock_quantity) FROM product_variants WHERE product_id = p.id AND is_active = true) as min_stock
FROM products p
LEFT JOIN brands b ON p.brand_id = b.id
WHERE 
    p.category_id = '33333333-0000-0000-0000-000000000001' -- Men's T-Shirts
    AND p.is_active = true 
    AND p.status = 'active'
    AND p.base_price BETWEEN 500 AND 2000
    AND p.attributes->>'material' ILIKE '%cotton%'
ORDER BY p.sales_count DESC
LIMIT 20 OFFSET 0;
```

### 2. Get Product Details with Variants

```sql
SELECT 
    p.*,
    b.name as brand_name,
    b.slug as brand_slug,
    c.name as category_name,
    c.path as category_path,
    s.business_name as seller_name,
    s.rating as seller_rating,
    (
        SELECT json_agg(
            json_build_object(
                'id', pv.id,
                'sku', pv.sku,
                'name', pv.name,
                'options', pv.options,
                'price', COALESCE(pv.override_price, p.base_price + pv.price_adjustment),
                'stock', pv.stock_quantity,
                'is_default', pv.is_default,
                'image_url', pv.image_url
            ) ORDER BY pv.sort_order
        )
        FROM product_variants pv 
        WHERE pv.product_id = p.id AND pv.is_active = true
    ) as variants,
    (
        SELECT json_agg(
            json_build_object(
                'id', pi.id,
                'url', pi.url,
                'thumbnail_url', pi.thumbnail_url,
                'alt_text', pi.alt_text,
                'is_primary', pi.is_primary
            ) ORDER BY pi.is_primary DESC, pi.sort_order
        )
        FROM product_images pi 
        WHERE pi.product_id = p.id
    ) as images
FROM products p
JOIN brands b ON p.brand_id = b.id
JOIN categories c ON p.category_id = c.id
JOIN sellers s ON p.seller_id = s.id
WHERE p.slug = 'nike-dri-fit-cotton-tshirt'
    AND p.is_active = true;
```

### 3. Search Products by JSONB Attributes

```sql
-- Find all cotton products
SELECT id, name, base_price, attributes
FROM products
WHERE attributes->>'material' ILIKE '%cotton%'
    AND is_active = true;

-- Find products with specific tag
SELECT id, name, base_price
FROM products
WHERE tags ? 'bestseller'  -- JSONB contains key
    AND is_active = true;

-- Find products with multiple attribute conditions
SELECT id, name, base_price, attributes
FROM products
WHERE attributes @> '{"fit": "Slim"}'::jsonb
    AND attributes->>'material' ILIKE '%cotton%'
    AND is_active = true;
```

### 4. Get Low Stock Products

```sql
SELECT 
    p.name as product_name,
    pv.sku,
    pv.name as variant_name,
    i.quantity,
    i.reserved_quantity,
    i.available_quantity,
    i.reorder_level,
    s.business_name as seller_name
FROM inventory i
JOIN product_variants pv ON i.variant_id = pv.id
JOIN products p ON pv.product_id = p.id
JOIN sellers s ON p.seller_id = s.id
WHERE i.available_quantity <= i.reorder_level
ORDER BY i.available_quantity ASC;
```

### 5. Update Stock (with Transaction)

```sql
-- Reserve stock for order
BEGIN;

-- Check and reserve
UPDATE inventory 
SET 
    reserved_quantity = reserved_quantity + 2,
    updated_at = CURRENT_TIMESTAMP
WHERE variant_id = 'vvvvvvvv-0000-0000-0000-000000000002'
    AND (quantity - reserved_quantity) >= 2
RETURNING *;

-- If update affected 0 rows, insufficient stock - rollback
-- Otherwise commit

-- Log transaction
INSERT INTO inventory_transactions (
    inventory_id, variant_id, transaction_type, 
    quantity_change, quantity_before, quantity_after,
    reference_type, reference_id, notes
) VALUES (
    'inventory-id', 'vvvvvvvv-0000-0000-0000-000000000002', 'reserved',
    2, 100, 100, -- reserved doesn't change quantity
    'order', 'order-uuid', 'Reserved for order #12345'
);

COMMIT;
```

### 6. Category Tree Query

```sql
-- Get full category tree
WITH RECURSIVE category_tree AS (
    -- Base case: root categories
    SELECT id, name, slug, parent_id, level, path, 1 as depth
    FROM categories
    WHERE parent_id IS NULL AND is_active = true
    
    UNION ALL
    
    -- Recursive case: children
    SELECT c.id, c.name, c.slug, c.parent_id, c.level, c.path, ct.depth + 1
    FROM categories c
    JOIN category_tree ct ON c.parent_id = ct.id
    WHERE c.is_active = true
)
SELECT * FROM category_tree
ORDER BY path;
```

---

## EF Core Entity Models

### Product Entity

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmCart.ProductService.Domain.Entities;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("seller_id")]
    public Guid SellerId { get; set; }
    
    [Column("brand_id")]
    public Guid? BrandId { get; set; }
    
    [Column("category_id")]
    public Guid CategoryId { get; set; }
    
    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = null!;
    
    [Required]
    [MaxLength(255)]
    [Column("slug")]
    public string Slug { get; set; } = null!;
    
    [Column("description")]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    [Column("short_description")]
    public string? ShortDescription { get; set; }
    
    [Column("base_price", TypeName = "decimal(12,2)")]
    public decimal BasePrice { get; set; }
    
    [Column("compare_at_price", TypeName = "decimal(12,2)")]
    public decimal? CompareAtPrice { get; set; }
    
    [Column("cost_price", TypeName = "decimal(12,2)")]
    public decimal? CostPrice { get; set; }
    
    [MaxLength(3)]
    [Column("currency")]
    public string Currency { get; set; } = "INR";
    
    // JSONB columns mapped to Dictionary or custom types
    [Column("attributes", TypeName = "jsonb")]
    public Dictionary<string, object> Attributes { get; set; } = new();
    
    [Column("specifications", TypeName = "jsonb")]
    public Dictionary<string, object> Specifications { get; set; } = new();
    
    [Column("tags", TypeName = "jsonb")]
    public List<string> Tags { get; set; } = new();
    
    [Column("seo_metadata", TypeName = "jsonb")]
    public SeoMetadata? SeoMetadata { get; set; }
    
    [Column("weight", TypeName = "decimal(10,3)")]
    public decimal? Weight { get; set; }
    
    [Column("dimensions", TypeName = "jsonb")]
    public ProductDimensions? Dimensions { get; set; }
    
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "draft";
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    
    [Column("is_featured")]
    public bool IsFeatured { get; set; } = false;
    
    [Column("is_new")]
    public bool IsNew { get; set; } = true;
    
    [Column("is_bestseller")]
    public bool IsBestseller { get; set; } = false;
    
    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }
    
    [Column("view_count")]
    public int ViewCount { get; set; } = 0;
    
    [Column("sales_count")]
    public int SalesCount { get; set; } = 0;
    
    [Column("review_count")]
    public int ReviewCount { get; set; } = 0;
    
    [Column("average_rating", TypeName = "decimal(3,2)")]
    public decimal AverageRating { get; set; } = 0;
    
    [Column("total_stock")]
    public int TotalStock { get; set; } = 0;
    
    [Column("has_variants")]
    public bool HasVariants { get; set; } = false;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public virtual Seller Seller { get; set; } = null!;
    public virtual Brand? Brand { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}

// JSONB mapping classes
public class SeoMetadata
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Keywords { get; set; }
    public string? CanonicalUrl { get; set; }
}

public class ProductDimensions
{
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string Unit { get; set; } = "cm";
}
```

### ProductVariant Entity

```csharp
[Table("product_variants")]
public class ProductVariant
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("product_id")]
    public Guid ProductId { get; set; }
    
    [Required]
    [MaxLength(100)]
    [Column("sku")]
    public string Sku { get; set; } = null!;
    
    [MaxLength(100)]
    [Column("barcode")]
    public string? Barcode { get; set; }
    
    [MaxLength(255)]
    [Column("name")]
    public string? Name { get; set; }
    
    [Column("options", TypeName = "jsonb")]
    public Dictionary<string, string> Options { get; set; } = new();
    
    [Column("price_adjustment", TypeName = "decimal(12,2)")]
    public decimal PriceAdjustment { get; set; } = 0;
    
    [Column("override_price", TypeName = "decimal(12,2)")]
    public decimal? OverridePrice { get; set; }
    
    [Column("cost_price", TypeName = "decimal(12,2)")]
    public decimal? CostPrice { get; set; }
    
    [Column("stock_quantity")]
    public int StockQuantity { get; set; } = 0;
    
    [Column("weight", TypeName = "decimal(10,3)")]
    public decimal? Weight { get; set; }
    
    [Column("dimensions", TypeName = "jsonb")]
    public ProductDimensions? Dimensions { get; set; }
    
    [MaxLength(500)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    
    [Column("is_default")]
    public bool IsDefault { get; set; } = false;
    
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    
    // Computed property
    [NotMapped]
    public int TotalStock => Inventories.Sum(i => i.AvailableQuantity);
    
    [NotMapped]
    public decimal FinalPrice => OverridePrice ?? (Product?.BasePrice ?? 0) + PriceAdjustment;
}
```

### Warehouse Entity

```csharp
[Table("warehouses")]
public class Warehouse
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(20)]
    [Column("code")]
    public string Code { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = null!;
    
    [MaxLength(30)]
    [Column("warehouse_type")]
    public string WarehouseType { get; set; } = "fulfillment";
    
    [Column("seller_id")]
    public Guid? SellerId { get; set; }
    
    [Required]
    [MaxLength(255)]
    [Column("address_line1")]
    public string AddressLine1 { get; set; } = null!;
    
    [MaxLength(255)]
    [Column("address_line2")]
    public string? AddressLine2 { get; set; }
    
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
    
    [Column("latitude", TypeName = "decimal(10,8)")]
    public decimal? Latitude { get; set; }
    
    [Column("longitude", TypeName = "decimal(11,8)")]
    public decimal? Longitude { get; set; }
    
    [Column("serviceable_pincodes", TypeName = "jsonb")]
    public List<string> ServiceablePincodes { get; set; } = new();
    
    [Column("max_delivery_radius_km")]
    public int? MaxDeliveryRadiusKm { get; set; }
    
    [MaxLength(100)]
    [Column("contact_name")]
    public string? ContactName { get; set; }
    
    [MaxLength(20)]
    [Column("contact_phone")]
    public string? ContactPhone { get; set; }
    
    [MaxLength(255)]
    [Column("contact_email")]
    public string? ContactEmail { get; set; }
    
    [Column("operating_hours", TypeName = "jsonb")]
    public Dictionary<string, OperatingHours>? OperatingHours { get; set; }
    
    [Column("max_capacity")]
    public int? MaxCapacity { get; set; }
    
    [Column("current_utilization")]
    public int CurrentUtilization { get; set; } = 0;
    
    [Column("priority")]
    public int Priority { get; set; } = 0;
    
    [Column("supports_cod")]
    public bool SupportsCod { get; set; } = true;
    
    [Column("supports_returns")]
    public bool SupportsReturns { get; set; } = true;
    
    [Column("supports_same_day")]
    public bool SupportsSameDay { get; set; } = false;
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    
    [Column("is_accepting_inventory")]
    public bool IsAcceptingInventory { get; set; } = true;
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Seller? Seller { get; set; }
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}

public class OperatingHours
{
    public string Open { get; set; } = "09:00";
    public string Close { get; set; } = "18:00";
}
```

### Inventory Entity

```csharp
[Table("inventory")]
public class Inventory
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("variant_id")]
    public Guid VariantId { get; set; }
    
    [Column("warehouse_id")]
    public Guid? WarehouseId { get; set; }
    
    [MaxLength(50)]
    [Column("location_code")]
    public string? LocationCode { get; set; }
    
    [Column("quantity")]
    public int Quantity { get; set; } = 0;
    
    [Column("reserved_quantity")]
    public int ReservedQuantity { get; set; } = 0;
    
    [Column("available_quantity")]
    public int AvailableQuantity { get; private set; } // Computed column
    
    [Column("reorder_level")]
    public int ReorderLevel { get; set; } = 10;
    
    [Column("reorder_quantity")]
    public int ReorderQuantity { get; set; } = 50;
    
    [Column("max_quantity")]
    public int? MaxQuantity { get; set; }
    
    [Column("last_restock_at")]
    public DateTime? LastRestockAt { get; set; }
    
    [Column("last_sold_at")]
    public DateTime? LastSoldAt { get; set; }
    
    [Column("average_cost", TypeName = "decimal(12,2)")]
    public decimal? AverageCost { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual ProductVariant Variant { get; set; } = null!;
    public virtual Warehouse? Warehouse { get; set; }
    
    // Computed
    [NotMapped]
    public bool IsLowStock => AvailableQuantity <= ReorderLevel;
    
    [NotMapped]
    public bool IsOutOfStock => AvailableQuantity <= 0;
}
```

### DbContext Configuration

```csharp
using Microsoft.EntityFrameworkCore;

namespace AmCart.ProductService.Infrastructure.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
    
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<PriceHistory> PriceHistory => Set<PriceHistory>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Slug);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.SellerId, e.Slug }).IsUnique();
            
            // JSONB columns
            entity.Property(e => e.Attributes).HasColumnType("jsonb");
            entity.Property(e => e.Specifications).HasColumnType("jsonb");
            entity.Property(e => e.Tags).HasColumnType("jsonb");
            entity.Property(e => e.SeoMetadata).HasColumnType("jsonb");
            entity.Property(e => e.Dimensions).HasColumnType("jsonb");
            
            // Soft delete filter
            entity.HasQueryFilter(e => e.DeletedAt == null);
            
            // Relationships
            entity.HasOne(e => e.Seller)
                .WithMany(s => s.Products)
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(e => e.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ProductVariant configuration
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.ProductId);
            
            entity.Property(e => e.Options).HasColumnType("jsonb");
            entity.Property(e => e.Dimensions).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Warehouse configuration
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => new { e.City, e.State });
            entity.HasIndex(e => e.IsActive);
            
            entity.Property(e => e.ServiceablePincodes).HasColumnType("jsonb");
            entity.Property(e => e.OperatingHours).HasColumnType("jsonb");
            
            entity.HasOne(e => e.Seller)
                .WithMany()
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Inventory configuration
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasIndex(e => new { e.VariantId, e.WarehouseId }).IsUnique();
            entity.HasIndex(e => e.WarehouseId);
            
            entity.HasOne(e => e.Variant)
                .WithMany(v => v.Inventories)
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.Inventories)
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Category self-reference
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasOne(e => e.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.Property(e => e.AttributeTemplate).HasColumnType("jsonb");
            entity.Property(e => e.PathIds).HasColumnType("uuid[]");
        });
    }
}
```

---

## OpenSearch Sync

### Product Index Mapping

```json
{
  "mappings": {
    "properties": {
      "id": { "type": "keyword" },
      "name": { 
        "type": "text",
        "analyzer": "standard",
        "fields": {
          "keyword": { "type": "keyword" },
          "autocomplete": { 
            "type": "text",
            "analyzer": "autocomplete"
          }
        }
      },
      "slug": { "type": "keyword" },
      "description": { "type": "text" },
      "short_description": { "type": "text" },
      "base_price": { "type": "float" },
      "compare_at_price": { "type": "float" },
      "category_id": { "type": "keyword" },
      "category_name": { "type": "keyword" },
      "category_path": { "type": "keyword" },
      "brand_id": { "type": "keyword" },
      "brand_name": { 
        "type": "text",
        "fields": { "keyword": { "type": "keyword" } }
      },
      "seller_id": { "type": "keyword" },
      "seller_name": { "type": "keyword" },
      "attributes": { "type": "flattened" },
      "tags": { "type": "keyword" },
      "average_rating": { "type": "float" },
      "review_count": { "type": "integer" },
      "sales_count": { "type": "integer" },
      "is_featured": { "type": "boolean" },
      "is_new": { "type": "boolean" },
      "is_bestseller": { "type": "boolean" },
      "in_stock": { "type": "boolean" },
      "image_url": { "type": "keyword", "index": false },
      "created_at": { "type": "date" },
      "variants": {
        "type": "nested",
        "properties": {
          "id": { "type": "keyword" },
          "sku": { "type": "keyword" },
          "options": { "type": "flattened" },
          "price": { "type": "float" },
          "in_stock": { "type": "boolean" }
        }
      }
    }
  },
  "settings": {
    "analysis": {
      "analyzer": {
        "autocomplete": {
          "tokenizer": "autocomplete",
          "filter": ["lowercase"]
        }
      },
      "tokenizer": {
        "autocomplete": {
          "type": "edge_ngram",
          "min_gram": 2,
          "max_gram": 20,
          "token_chars": ["letter", "digit"]
        }
      }
    }
  }
}
```

### Sync Service (C#)

```csharp
public class ProductSearchSyncService
{
    private readonly IOpenSearchClient _openSearchClient;
    private readonly ProductDbContext _dbContext;
    
    public async Task SyncProductAsync(Guid productId)
    {
        var product = await _dbContext.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId);
            
        if (product == null || product.Status != "active")
        {
            await _openSearchClient.DeleteAsync<ProductSearchDocument>(productId.ToString());
            return;
        }
        
        var searchDoc = new ProductSearchDocument
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            BasePrice = product.BasePrice,
            CompareAtPrice = product.CompareAtPrice,
            CategoryId = product.CategoryId.ToString(),
            CategoryName = product.Category.Name,
            CategoryPath = product.Category.Path,
            BrandId = product.BrandId?.ToString(),
            BrandName = product.Brand?.Name,
            SellerId = product.SellerId.ToString(),
            SellerName = product.Seller.BusinessName,
            Attributes = product.Attributes,
            Tags = product.Tags,
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            SalesCount = product.SalesCount,
            IsFeatured = product.IsFeatured,
            IsNew = product.IsNew,
            IsBestseller = product.IsBestseller,
            InStock = product.Variants.Any(v => v.StockQuantity > 0),
            ImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.Url,
            CreatedAt = product.CreatedAt,
            Variants = product.Variants.Select(v => new VariantSearchDocument
            {
                Id = v.Id.ToString(),
                Sku = v.Sku,
                Options = v.Options,
                Price = v.OverridePrice ?? product.BasePrice + v.PriceAdjustment,
                InStock = v.StockQuantity > 0
            }).ToList()
        };
        
        await _openSearchClient.IndexAsync(searchDoc, i => i.Index("products"));
    }
}
```

---

## Summary

| Table | Purpose | Key Features |
|-------|---------|--------------|
| `categories` | Hierarchical product categories | Materialized path, attribute templates |
| `brands` | Product brands | SEO metadata |
| `sellers` | Multi-vendor support | Verification, commission |
| `products` | Main product data | JSONB attributes, soft delete |
| `product_variants` | Size/color variants | SKU, price adjustments |
| `product_images` | Product gallery | Primary image flag |
| `warehouses` | Multi-warehouse locations | Platform & seller warehouses, geolocation |
| `inventory` | Stock per warehouse | Reserved quantity, reorder levels |
| `inventory_transactions` | Stock audit trail | All stock movements |
| `price_history` | Price tracking | Effective dates |
| `collections` | Product groupings | Manual/automated |

This schema supports:
- ✅ Multi-vendor marketplace
- ✅ Flexible product attributes via JSONB
- ✅ Product variants (size, color, etc.)
- ✅ **Multi-warehouse inventory management**
- ✅ Inventory reservations for orders
- ✅ Price history tracking
- ✅ Category hierarchy
- ✅ OpenSearch sync for search
- ✅ Soft delete for data retention
- ✅ **Seller and platform warehouses**
- ✅ **Geolocation for delivery optimization**

