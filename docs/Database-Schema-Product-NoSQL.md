# Product Database Schema (NoSQL)

## MongoDB / Amazon DocumentDB Approach for AmCart E-commerce

---

## Table of Contents

1. [Overview](#overview)
2. [MongoDB vs PostgreSQL Comparison](#mongodb-vs-postgresql-comparison)
3. [Collection Schemas](#collection-schemas)
4. [Document Models](#document-models)
5. [Indexes](#indexes)
6. [Sample Documents](#sample-documents)
7. [Common Queries](#common-queries)
8. [C# Entity Models](#c-entity-models)
9. [Data Access Patterns](#data-access-patterns)
10. [OpenSearch Sync](#opensearch-sync)

---

## Overview

### Design Principles

| Principle | Implementation |
|-----------|----------------|
| **Document-Oriented** | Products stored as rich documents with embedded data |
| **Denormalization** | Embed frequently accessed data (brand, category) for read performance |
| **Flexible Schema** | Native support for varying attributes per category |
| **Read Optimization** | Single document fetch for product details |
| **Write Considerations** | Separate collections for high-write data (inventory) |

### Database: `amcart_products`

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    MongoDB Product Database Schema                        │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                     products (Collection)                         │    │
│  │  ┌─────────────────────────────────────────────────────────┐    │    │
│  │  │  {                                                       │    │    │
│  │  │    _id, name, slug, description, basePrice,             │    │    │
│  │  │    brand: { embedded },                                  │    │    │
│  │  │    category: { embedded },                               │    │    │
│  │  │    attributes: { native document },                      │    │    │
│  │  │    variants: [ embedded array ],                         │    │    │
│  │  │    images: [ embedded array ]                            │    │    │
│  │  │  }                                                       │    │    │
│  │  └─────────────────────────────────────────────────────────┘    │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐          │
│  │   categories    │  │     brands      │  │    sellers      │          │
│  │   (reference)   │  │   (reference)   │  │   (reference)   │          │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘          │
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐          │
│  │   warehouses    │  │    inventory    │  │  price_history  │          │
│  │   (separate)    │  │   (separate)    │  │   (separate)    │          │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘          │
│                                                                          │
└──────────────────────────────────────────────────────────────────────────┘
```

### AWS Service: Amazon DocumentDB

| Feature | Details |
|---------|---------|
| **Compatibility** | MongoDB 5.0 API compatible |
| **Managed Service** | Fully managed, auto-scaling, automatic backups |
| **High Availability** | Multi-AZ deployment with read replicas |
| **Security** | VPC, encryption at rest/transit, IAM integration |
| **Pricing** | On-demand or provisioned instances |

---

## Entity Relationship Diagram

### Document Relationships Overview

```
                                    ┌─────────────────────────┐
                                    │        sellers          │
                                    │      (Collection)       │
                                    ├─────────────────────────┤
                                    │ _id: ObjectId           │
                                    │ userId: String          │
                                    │ businessName: String    │
                                    │ rating: Number          │
                                    │ status: String          │
                                    └───────────┬─────────────┘
                                                │
                                                │ Referenced (1:N)
                                                ▼
┌─────────────────────────┐         ┌─────────────────────────────────────────────────┐
│        brands           │         │                   products                       │
│      (Collection)       │         │                 (Collection)                     │
├─────────────────────────┤         ├─────────────────────────────────────────────────┤
│ _id: ObjectId           │◀────────│ _id: ObjectId                                   │
│ name: String            │Embedded │ sellerId: ObjectId ──────────────────────────▶ │
│ slug: String            │(partial)│                                                 │
│ logoUrl: String         │         │ brand: {                      ◀── Embedded     │
└─────────────────────────┘         │   _id, name, slug, logoUrl                      │
                                    │ }                                               │
┌─────────────────────────┐         │                                                 │
│      categories         │         │ category: {                   ◀── Embedded     │
│      (Collection)       │◀────────│   _id, name, slug, path, level                  │
├─────────────────────────┤Embedded │ }                                               │
│ _id: ObjectId           │(partial)│                                                 │
│ parentId: ObjectId      │         │ name: String                                    │
│ name: String            │         │ slug: String                                    │
│ slug: String            │         │ description: String                             │
│ path: String            │         │ basePrice: Decimal128                           │
│ level: Number           │         │                                                 │
│ attributeTemplate: []   │         │ attributes: {                 ◀── Native Doc   │
└─────────────────────────┘         │   material, fit, color, ...                     │
                                    │ }                                               │
                                    │                                                 │
                                    │ variants: [                   ◀── Embedded     │
                                    │   { _id, sku, options, price }  Array           │
                                    │ ]                                               │
                                    │                                                 │
                                    │ images: [                     ◀── Embedded     │
                                    │   { _id, url, isPrimary }       Array           │
                                    │ ]                                               │
                                    │                                                 │
                                    │ stats: { viewCount, salesCount }                │
                                    │ inventory: { totalStock, inStock }              │
                                    └───────────────────┬─────────────────────────────┘
                                                        │
                          ┌─────────────────────────────┼─────────────────────────────┐
                          │                             │                             │
                          ▼ Referenced                  ▼ Referenced                  ▼ Referenced
            ┌─────────────────────────┐   ┌─────────────────────────┐   ┌─────────────────────────┐
            │       inventory         │   │     price_history       │   │      collections        │
            │      (Collection)       │   │      (Collection)       │   │      (Collection)       │
            ├─────────────────────────┤   ├─────────────────────────┤   ├─────────────────────────┤
            │ _id: ObjectId           │   │ _id: ObjectId           │   │ _id: ObjectId           │
            │ variantId: ObjectId ────┼──▶│ productId: ObjectId ────┼──▶│ productIds: [ObjectId]  │
            │ productId: ObjectId     │   │ variantId: ObjectId     │   │ name: String            │
            │ warehouseId: ObjectId ──┼─┐ │ price: Decimal128       │   │ slug: String            │
            │ sku: String             │ │ │ effectiveFrom: Date     │   │ rules: Document         │
            │ quantity: Number        │ │ │ reason: String          │   └─────────────────────────┘
            │ reservedQuantity: Number│ │ └─────────────────────────┘
            │ reorderLevel: Number    │ │
            └───────────┬─────────────┘ │
                        │               │
                        │ Referenced    │
                        ▼               │
            ┌─────────────────────────┐ │
            │ inventory_transactions  │ │
            │      (Collection)       │ │
            ├─────────────────────────┤ │
            │ _id: ObjectId           │ │
            │ inventoryId: ObjectId   │ │
            │ variantId: ObjectId     │ │
            │ transactionType: String │ │
            │ quantityChange: Number  │ │
            │ referenceId: ObjectId   │ │
            └─────────────────────────┘ │
                                        │
                                        ▼
                          ┌─────────────────────────┐
                          │       warehouses        │
                          │      (Collection)       │
                          ├─────────────────────────┤
                          │ _id: ObjectId           │
                          │ code: String            │
                          │ name: String            │
                          │ sellerId: ObjectId      │
                          │ address: Document       │
                          │ location: GeoJSON       │
                          │ isActive: Boolean       │
                          └─────────────────────────┘
```

### Relationship Types in MongoDB

| Relationship | Type | Reason |
|--------------|------|--------|
| Product → Brand | **Embedded (partial)** | Read-heavy, frequently accessed together |
| Product → Category | **Embedded (partial)** | Read-heavy, used in every listing |
| Product → Variants | **Embedded (full)** | Always fetched together, < 100 items |
| Product → Images | **Embedded (full)** | Always fetched together, < 20 items |
| Product → Seller | **Referenced** | Large document, shared across products |
| Inventory → Product | **Referenced** | High-write frequency, separate scaling |
| Inventory → Warehouse | **Referenced** | Many-to-many through inventory |
| Price History → Product | **Referenced** | Unbounded growth, audit trail |

### Embedded vs Referenced Decision Tree

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Should I Embed or Reference?                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Is the data frequently accessed together?                              │
│      │                                                                  │
│      ├── YES ──▶ Is the embedded array bounded (< 100 items)?          │
│      │              │                                                   │
│      │              ├── YES ──▶ Is the data updated frequently?        │
│      │              │              │                                    │
│      │              │              ├── YES ──▶ REFERENCE (separate)    │
│      │              │              │           e.g., inventory          │
│      │              │              │                                    │
│      │              │              └── NO ───▶ EMBED                   │
│      │              │                          e.g., variants, images   │
│      │              │                                                   │
│      │              └── NO ───▶ REFERENCE or BUCKET PATTERN            │
│      │                                                                  │
│      └── NO ───▶ REFERENCE                                             │
│                  e.g., price_history, inventory_transactions           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Product Document Structure (Visual)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          Product Document                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  _id: ObjectId("...")                                                   │
│  sellerId: ObjectId("...") ────────────────────────▶ sellers._id       │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ brand: {                                    EMBEDDED (partial)   │   │
│  │   _id: ObjectId("..."),                                          │   │
│  │   name: "Nike",                                                  │   │
│  │   slug: "nike",                                                  │   │
│  │   logoUrl: "https://..."                                         │   │
│  │ }                                                                │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ category: {                                 EMBEDDED (partial)   │   │
│  │   _id: ObjectId("..."),                                          │   │
│  │   name: "T-Shirts",                                              │   │
│  │   slug: "men-tshirts",                                           │   │
│  │   path: "/men/clothing/tshirts"                                  │   │
│  │ }                                                                │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  name: "Nike Dri-FIT Cotton T-Shirt"                                   │
│  slug: "nike-dri-fit-cotton-tshirt"                                    │
│  basePrice: NumberDecimal("1299.00")                                   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ attributes: {                               NATIVE DOCUMENT      │   │
│  │   material: "100% Cotton",                  (No JSONB needed!)   │   │
│  │   fit: "Regular",                                                │   │
│  │   neckType: "Round Neck",                                        │   │
│  │   sleeveType: "Half Sleeve"                                      │   │
│  │ }                                                                │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ variants: [                                 EMBEDDED ARRAY       │   │
│  │   {                                                              │   │
│  │     _id: ObjectId("v1"),                                         │   │
│  │     sku: "NIKE-DF-BLK-S",                                        │   │
│  │     options: { color: "Black", size: "S" },                      │   │
│  │     priceAdjustment: 0                                           │   │
│  │   },                                                             │   │
│  │   {                                                              │   │
│  │     _id: ObjectId("v2"),                                         │   │
│  │     sku: "NIKE-DF-BLK-M",                                        │   │
│  │     options: { color: "Black", size: "M" },                      │   │
│  │     priceAdjustment: 0,                                          │   │
│  │     isDefault: true                                              │   │
│  │   }                                                              │   │
│  │ ]                                                                │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ images: [                                   EMBEDDED ARRAY       │   │
│  │   { _id: "...", url: "...", isPrimary: true },                   │   │
│  │   { _id: "...", url: "...", isPrimary: false }                   │   │
│  │ ]                                                                │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  stats: { viewCount: 1500, salesCount: 250, averageRating: 4.5 }       │
│  inventory: { totalStock: 365, inStock: true }                         │
│                                                                         │
│  createdAt: ISODate("...")                                              │
│  updatedAt: ISODate("...")                                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
                    │
                    │ variants._id referenced in:
                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        Inventory Document                                │
├─────────────────────────────────────────────────────────────────────────┤
│  _id: ObjectId("...")                                                   │
│  variantId: ObjectId("v2") ─────────────────▶ products.variants._id    │
│  productId: ObjectId("...") ────────────────▶ products._id             │
│  warehouseId: ObjectId("...") ──────────────▶ warehouses._id           │
│  sku: "NIKE-DF-BLK-M"                                                   │
│  quantity: 100                                                          │
│  reservedQuantity: 5                                                    │
│  reorderLevel: 10                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

### Category Hierarchy (Self-Reference)

```
┌─────────────────────────┐
│ categories Collection   │
└─────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ { _id: "cat-men", parentId: null, name: "Men", path: "/men", level: 0 } │
└─────────────────────────────────────────────────────────────────────────┘
           │
           │ parentId references
           ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│ { _id: "cat-men-clothing", parentId: "cat-men", name: "Clothing",               │
│   path: "/men/clothing", level: 1 }                                             │
└─────────────────────────────────────────────────────────────────────────────────┘
           │
           │ parentId references
           ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│ { _id: "cat-tshirts", parentId: "cat-men-clothing", name: "T-Shirts",           │
│   path: "/men/clothing/tshirts", level: 2,                                      │
│   pathIds: ["cat-men", "cat-men-clothing", "cat-tshirts"] }                     │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## MongoDB vs PostgreSQL Comparison

### When to Use Each

| Criteria | MongoDB/DocumentDB | PostgreSQL + JSONB |
|----------|-------------------|-------------------|
| **Product Attributes** | ✅ Native documents - no special syntax | ⚠️ JSONB operators needed |
| **Schema Flexibility** | ✅ Fully flexible per document | ⚠️ Fixed columns + JSONB for flexibility |
| **Read Performance** | ✅ Single document fetch | ⚠️ May need JOINs |
| **Write Performance** | ⚠️ Embedded updates can be complex | ✅ Normalized updates |
| **ACID Transactions** | ⚠️ Multi-doc transactions (4.0+) | ✅ Full ACID |
| **Complex Queries** | ⚠️ Aggregation pipeline learning curve | ✅ SQL is familiar |
| **Referential Integrity** | ❌ Manual enforcement | ✅ Foreign keys |
| **Reporting/Analytics** | ⚠️ Requires aggregation | ✅ SQL analytics |
| **Team Expertise** | Depends on team | Depends on team |

### Recommendation Summary

| Scenario | Recommended |
|----------|-------------|
| High read volume, simple writes | MongoDB |
| Complex transactions, reporting | PostgreSQL |
| Highly variable product attributes | MongoDB |
| Strong referential integrity needed | PostgreSQL |
| Rapid prototyping | MongoDB |
| AmCart (Hybrid) | PostgreSQL for Orders/Users, MongoDB for Products |

---

## Collection Schemas

### 1. Products Collection

The main collection with embedded variants and images for optimal read performance.

```javascript
// Collection: products
{
  _id: ObjectId("507f1f77bcf86cd799439011"),
  
  // Seller Reference
  sellerId: ObjectId("seller-id"),
  
  // Embedded Brand (denormalized for reads)
  brand: {
    _id: ObjectId("brand-id"),
    name: "Nike",
    slug: "nike",
    logoUrl: "https://cdn.amcart.com/brands/nike.png"
  },
  
  // Embedded Category (denormalized for reads)
  category: {
    _id: ObjectId("category-id"),
    name: "T-Shirts",
    slug: "men-tshirts",
    path: "/men/clothing/tshirts",
    level: 2
  },
  
  // Basic Info
  name: "Nike Dri-FIT Cotton T-Shirt",
  slug: "nike-dri-fit-cotton-tshirt",
  description: "Premium cotton t-shirt with Dri-FIT technology...",
  shortDescription: "Premium cotton t-shirt with Dri-FIT technology",
  
  // Pricing
  basePrice: NumberDecimal("1299.00"),
  compareAtPrice: NumberDecimal("1599.00"),
  costPrice: NumberDecimal("650.00"),
  currency: "INR",
  
  // Tax
  taxCategory: "standard",
  hsnCode: "6109",
  
  // Native Attributes (NO JSONB needed!)
  attributes: {
    material: "100% Cotton with Dri-FIT",
    fit: "Regular",
    careInstructions: "Machine wash cold, tumble dry low",
    countryOfOrigin: "Vietnam",
    neckType: "Round Neck",
    sleeveType: "Half Sleeve",
    pattern: "Solid"
  },
  
  // Native Specifications
  specifications: {
    fabricWeight: "180 GSM",
    stretch: "Non-stretch"
  },
  
  // Tags Array
  tags: ["summer", "casual", "sports", "bestseller"],
  
  // Physical Properties
  weight: 0.25,
  weightUnit: "kg",
  dimensions: {
    length: 70,
    width: 50,
    height: 2,
    unit: "cm"
  },
  
  // SEO
  seo: {
    title: "Men's Cotton T-Shirt | AmCart",
    description: "Buy premium cotton t-shirts...",
    keywords: ["cotton tshirt", "mens wear"],
    canonicalUrl: "/products/mens-cotton-tshirt"
  },
  
  // Embedded Variants Array
  variants: [
    {
      _id: ObjectId("variant-1"),
      sku: "NIKE-DF-BLK-S",
      barcode: "8901234567890",
      name: "Black / S",
      options: {
        color: "Black",
        size: "S"
      },
      priceAdjustment: NumberDecimal("0.00"),
      overridePrice: null,
      costPrice: NumberDecimal("650.00"),
      imageUrl: "https://cdn.amcart.com/products/nike-df-black.jpg",
      isActive: true,
      isDefault: false,
      sortOrder: 0
    },
    {
      _id: ObjectId("variant-2"),
      sku: "NIKE-DF-BLK-M",
      barcode: "8901234567891",
      name: "Black / M",
      options: {
        color: "Black",
        size: "M"
      },
      priceAdjustment: NumberDecimal("0.00"),
      overridePrice: null,
      costPrice: NumberDecimal("650.00"),
      imageUrl: "https://cdn.amcart.com/products/nike-df-black.jpg",
      isActive: true,
      isDefault: true,  // Default variant
      sortOrder: 1
    },
    {
      _id: ObjectId("variant-3"),
      sku: "NIKE-DF-BLK-XXL",
      barcode: "8901234567895",
      name: "Black / XXL",
      options: {
        color: "Black",
        size: "XXL"
      },
      priceAdjustment: NumberDecimal("100.00"),  // XXL costs more
      overridePrice: null,
      costPrice: NumberDecimal("700.00"),
      imageUrl: "https://cdn.amcart.com/products/nike-df-black.jpg",
      isActive: true,
      isDefault: false,
      sortOrder: 5
    }
  ],
  
  // Embedded Images Array
  images: [
    {
      _id: ObjectId("img-1"),
      url: "https://cdn.amcart.com/products/nike-df-1.jpg",
      thumbnailUrl: "https://cdn.amcart.com/products/nike-df-1-thumb.jpg",
      mediumUrl: "https://cdn.amcart.com/products/nike-df-1-medium.jpg",
      altText: "Nike Dri-FIT T-Shirt Front View",
      isPrimary: true,
      sortOrder: 0
    },
    {
      _id: ObjectId("img-2"),
      url: "https://cdn.amcart.com/products/nike-df-2.jpg",
      thumbnailUrl: "https://cdn.amcart.com/products/nike-df-2-thumb.jpg",
      mediumUrl: "https://cdn.amcart.com/products/nike-df-2-medium.jpg",
      altText: "Nike Dri-FIT T-Shirt Back View",
      isPrimary: false,
      sortOrder: 1
    }
  ],
  
  // Status
  status: "active",  // draft, pending_review, active, inactive, discontinued
  isActive: true,
  isFeatured: false,
  isNew: true,
  isBestseller: true,
  
  // Publishing
  publishedAt: ISODate("2026-01-15T10:00:00Z"),
  
  // Denormalized Stats (updated via aggregation/events)
  stats: {
    viewCount: 1500,
    salesCount: 250,
    reviewCount: 45,
    averageRating: 4.5
  },
  
  // Denormalized Inventory Summary
  inventory: {
    totalStock: 365,
    hasVariants: true,
    inStock: true
  },
  
  // Timestamps
  createdAt: ISODate("2026-01-01T00:00:00Z"),
  updatedAt: ISODate("2026-01-15T12:00:00Z"),
  deletedAt: null  // Soft delete
}
```

### 2. Categories Collection

Hierarchical categories with attribute templates.

```javascript
// Collection: categories
{
  _id: ObjectId("cat-tshirts"),
  parentId: ObjectId("cat-men-clothing"),  // null for root
  
  name: "T-Shirts",
  slug: "men-tshirts",
  description: "Men's t-shirts and tops",
  imageUrl: "https://cdn.amcart.com/categories/tshirts.jpg",
  
  // Hierarchy
  level: 2,
  path: "/men/clothing/tshirts",
  pathIds: [
    ObjectId("cat-men"),
    ObjectId("cat-men-clothing"),
    ObjectId("cat-tshirts")
  ],
  
  // Attribute Template for products in this category
  attributeTemplate: [
    {
      name: "size",
      type: "select",
      options: ["XS", "S", "M", "L", "XL", "XXL"],
      required: true
    },
    {
      name: "color",
      type: "select",
      options: [],  // Populated dynamically
      required: true
    },
    {
      name: "material",
      type: "text",
      required: false
    },
    {
      name: "fit",
      type: "select",
      options: ["Slim", "Regular", "Loose"],
      required: false
    }
  ],
  
  // Display
  sortOrder: 1,
  isActive: true,
  isFeatured: false,
  
  // SEO
  metaTitle: "Men's T-Shirts | AmCart",
  metaDescription: "Shop the latest men's t-shirts...",
  
  // Stats (denormalized)
  productCount: 1250,
  
  createdAt: ISODate("2026-01-01T00:00:00Z"),
  updatedAt: ISODate("2026-01-15T00:00:00Z")
}
```

### 3. Brands Collection

```javascript
// Collection: brands
{
  _id: ObjectId("brand-nike"),
  
  name: "Nike",
  slug: "nike",
  description: "Just Do It - Nike is a global sportswear brand...",
  
  // Media
  logoUrl: "https://cdn.amcart.com/brands/nike.png",
  bannerUrl: "https://cdn.amcart.com/brands/nike-banner.jpg",
  
  // Status
  isActive: true,
  isFeatured: true,
  
  // SEO
  metaTitle: "Nike Products | AmCart",
  metaDescription: "Shop Nike products...",
  websiteUrl: "https://www.nike.com",
  
  // Stats (denormalized)
  productCount: 500,
  
  createdAt: ISODate("2026-01-01T00:00:00Z"),
  updatedAt: ISODate("2026-01-15T00:00:00Z")
}
```

### 4. Sellers Collection

```javascript
// Collection: sellers
{
  _id: ObjectId("seller-001"),
  userId: "cognito-sub-12345",  // Reference to Cognito
  
  // Business Info
  businessName: "Nike India Pvt Ltd",
  businessType: "brand",  // individual, company, brand
  description: "Official Nike store on AmCart",
  
  // Contact
  email: "seller@nike.in",
  phone: "+919876543210",
  
  // Address
  address: {
    line1: "Nike India Office",
    line2: "Building A, Floor 5",
    city: "Mumbai",
    state: "Maharashtra",
    postalCode: "400001",
    country: "India"
  },
  
  // Verification
  isVerified: true,
  verificationDocuments: [
    {
      type: "gst",
      number: "27AABCN1234A1Z5",
      verifiedAt: ISODate("2026-01-05T00:00:00Z")
    },
    {
      type: "pan",
      number: "AABCN1234A",
      verifiedAt: ISODate("2026-01-05T00:00:00Z")
    }
  ],
  verifiedAt: ISODate("2026-01-05T00:00:00Z"),
  
  // Ratings
  rating: 4.8,
  totalReviews: 15000,
  totalProducts: 500,
  totalSales: 50000,
  
  // Banking (encrypted)
  bankAccount: {
    accountNumber: "encrypted-value",
    ifscCode: "HDFC0001234",
    accountName: "Nike India Pvt Ltd"
  },
  
  // Commission
  commissionRate: 8.00,  // Percentage
  
  // Status
  status: "active",  // pending, active, suspended, inactive
  
  createdAt: ISODate("2026-01-01T00:00:00Z"),
  updatedAt: ISODate("2026-01-15T00:00:00Z")
}
```

### 5. Warehouses Collection

```javascript
// Collection: warehouses
{
  _id: ObjectId("wh-mum-01"),
  
  code: "WH-MUM-01",
  name: "Mumbai Fulfillment Center",
  
  // Type
  warehouseType: "fulfillment",  // fulfillment, seller, returns, dark_store
  
  // Owner
  sellerId: null,  // null for platform warehouses
  
  // Address
  address: {
    line1: "Plot 45, MIDC Industrial Area",
    line2: "Andheri East",
    city: "Mumbai",
    state: "Maharashtra",
    postalCode: "400093",
    country: "India"
  },
  
  // Geolocation
  location: {
    type: "Point",
    coordinates: [72.8697, 19.1136]  // [longitude, latitude]
  },
  
  // Service Area
  serviceablePincodes: ["400001", "400002", "400003", "400093"],
  maxDeliveryRadiusKm: 50,
  
  // Contact
  contact: {
    name: "Rajesh Kumar",
    phone: "+919876543210",
    email: "warehouse.mum@amcart.com"
  },
  
  // Operating Hours
  operatingHours: {
    monday: { open: "09:00", close: "18:00" },
    tuesday: { open: "09:00", close: "18:00" },
    wednesday: { open: "09:00", close: "18:00" },
    thursday: { open: "09:00", close: "18:00" },
    friday: { open: "09:00", close: "18:00" },
    saturday: { open: "09:00", close: "14:00" },
    sunday: null  // Closed
  },
  
  // Capacity
  maxCapacity: 100000,
  currentUtilization: 65000,
  storageType: "ambient",  // ambient, cold_storage, mixed
  
  // Priority
  priority: 10,  // Higher = preferred
  
  // Capabilities
  supportsCod: true,
  supportsReturns: true,
  supportsSameDay: true,
  
  // Status
  isActive: true,
  isAcceptingInventory: true,
  
  createdAt: ISODate("2026-01-01T00:00:00Z"),
  updatedAt: ISODate("2026-01-15T00:00:00Z")
}
```

### 6. Inventory Collection (Separate for High-Write)

Inventory is kept separate because it has high write frequency.

```javascript
// Collection: inventory
{
  _id: ObjectId("inv-001"),
  
  // References
  variantId: ObjectId("variant-2"),  // Reference to product.variants._id
  productId: ObjectId("product-id"),  // Denormalized for queries
  warehouseId: ObjectId("wh-mum-01"),
  
  // Denormalized for quick access
  sku: "NIKE-DF-BLK-M",
  
  // Location within warehouse
  locationCode: "A-12-3",  // Aisle-Shelf-Bin
  
  // Stock Levels
  quantity: 100,
  reservedQuantity: 5,  // Reserved for pending orders
  // availableQuantity = quantity - reservedQuantity (computed)
  
  // Thresholds
  reorderLevel: 10,
  reorderQuantity: 50,
  maxQuantity: 200,
  
  // Cost Tracking
  averageCost: NumberDecimal("650.00"),
  
  // Tracking
  lastRestockAt: ISODate("2026-01-10T00:00:00Z"),
  lastSoldAt: ISODate("2026-01-15T14:30:00Z"),
  
  createdAt: ISODate("2026-01-01T00:00:00Z"),
  updatedAt: ISODate("2026-01-15T14:30:00Z")
}
```

### 7. Inventory Transactions Collection (Audit Log)

```javascript
// Collection: inventory_transactions
{
  _id: ObjectId("tx-001"),
  
  inventoryId: ObjectId("inv-001"),
  variantId: ObjectId("variant-2"),
  productId: ObjectId("product-id"),
  warehouseId: ObjectId("wh-mum-01"),
  
  // Transaction
  transactionType: "sale",  // restock, sale, return, adjustment, reserved, released, damaged, transfer
  
  // Quantity Change
  quantityChange: -2,  // Negative for decrease
  quantityBefore: 102,
  quantityAfter: 100,
  
  // Reference
  referenceType: "order",
  referenceId: ObjectId("order-id"),
  
  // Notes
  notes: "Sold via order #ORD-2026-001234",
  performedBy: "system",  // or user ID
  
  createdAt: ISODate("2026-01-15T14:30:00Z")
}
```

### 8. Price History Collection

```javascript
// Collection: price_history
{
  _id: ObjectId("ph-001"),
  
  productId: ObjectId("product-id"),
  variantId: ObjectId("variant-2"),  // null for base price change
  
  // Price Info
  price: NumberDecimal("1299.00"),
  compareAtPrice: NumberDecimal("1599.00"),
  currency: "INR",
  
  // Validity
  effectiveFrom: ISODate("2026-01-01T00:00:00Z"),
  effectiveTo: ISODate("2026-01-31T23:59:59Z"),
  
  // Reason
  reason: "promotion",  // initial, promotion, price_increase, price_decrease, sale
  notes: "Republic Day Sale",
  
  changedBy: ObjectId("user-id"),
  
  createdAt: ISODate("2026-01-01T00:00:00Z")
}
```

### 9. Collections (Product Groupings)

```javascript
// Collection: collections
{
  _id: ObjectId("col-summer-2026"),
  
  name: "Summer Collection 2026",
  slug: "summer-collection-2026",
  description: "Beat the heat with our summer essentials",
  imageUrl: "https://cdn.amcart.com/collections/summer-2026.jpg",
  
  // Type
  collectionType: "manual",  // manual, automated
  
  // For automated collections
  rules: {
    tags: { $in: ["summer"] },
    "category.path": { $regex: "^/men" },
    basePrice: { $lte: 2000 }
  },
  
  // Manual product list (for manual collections)
  productIds: [
    ObjectId("product-1"),
    ObjectId("product-2"),
    ObjectId("product-3")
  ],
  
  // Display
  isActive: true,
  isFeatured: true,
  sortOrder: 1,
  
  // Validity
  startDate: ISODate("2026-03-01T00:00:00Z"),
  endDate: ISODate("2026-06-30T23:59:59Z"),
  
  createdAt: ISODate("2026-01-15T00:00:00Z"),
  updatedAt: ISODate("2026-01-15T00:00:00Z")
}
```

---

## Document Models

### Embedded vs Referenced Data

| Data | Strategy | Reason |
|------|----------|--------|
| **Brand** | Embedded (partial) | Read-heavy, rarely changes |
| **Category** | Embedded (partial) | Read-heavy, used in listings |
| **Variants** | Embedded | Always fetched with product |
| **Images** | Embedded | Always fetched with product |
| **Inventory** | Referenced (separate) | High-write frequency |
| **Price History** | Referenced (separate) | Audit/compliance, unbounded growth |
| **Seller** | Referenced | Large document, shared across products |

### Document Size Considerations

```javascript
// Maximum BSON document size: 16 MB
// Typical product document: 5-50 KB

// If variants exceed reasonable limit (>100), consider:
{
  // Option 1: Separate variants collection
  variants: "reference",  // Store only count
  variantCount: 150,
  
  // Option 2: Bucket pattern
  variantBuckets: [
    { bucketId: 1, variants: [...first 50...] },
    { bucketId: 2, variants: [...next 50...] }
  ]
}
```

---

## Indexes

### Products Collection Indexes

```javascript
// Create indexes for products collection
db.products.createIndexes([
  // Basic lookups
  { key: { slug: 1 }, unique: true },
  { key: { sellerId: 1 } },
  { key: { "brand._id": 1 } },
  { key: { "category._id": 1 } },
  { key: { status: 1 } },
  
  // Filtering
  { key: { isActive: 1, status: 1 } },
  { key: { isFeatured: 1 }, partialFilterExpression: { isFeatured: true } },
  { key: { isBestseller: 1 }, partialFilterExpression: { isBestseller: true } },
  
  // Sorting
  { key: { basePrice: 1 } },
  { key: { "stats.averageRating": -1 } },
  { key: { "stats.salesCount": -1 } },
  { key: { createdAt: -1 } },
  
  // Category + price range queries
  { key: { "category._id": 1, isActive: 1, basePrice: 1 } },
  
  // Seller products
  { key: { sellerId: 1, status: 1, createdAt: -1 } },
  
  // Text search (backup to OpenSearch)
  { 
    key: { 
      name: "text", 
      description: "text", 
      "brand.name": "text",
      tags: "text"
    },
    weights: {
      name: 10,
      "brand.name": 5,
      tags: 3,
      description: 1
    },
    name: "product_text_search"
  },
  
  // Variant SKU lookup
  { key: { "variants.sku": 1 } },
  
  // Tags (multikey index)
  { key: { tags: 1 } },
  
  // Soft delete filter
  { key: { deletedAt: 1 }, partialFilterExpression: { deletedAt: null } }
]);
```

### Categories Collection Indexes

```javascript
db.categories.createIndexes([
  { key: { slug: 1 }, unique: true },
  { key: { parentId: 1 } },
  { key: { path: 1 } },
  { key: { level: 1, sortOrder: 1 } },
  { key: { isActive: 1 } }
]);
```

### Inventory Collection Indexes

```javascript
db.inventory.createIndexes([
  // Unique constraint
  { key: { variantId: 1, warehouseId: 1 }, unique: true },
  
  // Stock queries
  { key: { productId: 1 } },
  { key: { warehouseId: 1 } },
  { key: { sku: 1 } },
  
  // Low stock alerts
  { 
    key: { quantity: 1, reorderLevel: 1 },
    partialFilterExpression: { 
      $expr: { $lte: ["$quantity", "$reorderLevel"] }
    }
  }
]);
```

### Warehouses Collection Indexes

```javascript
db.warehouses.createIndexes([
  { key: { code: 1 }, unique: true },
  { key: { sellerId: 1 } },
  { key: { isActive: 1 } },
  { key: { "address.city": 1, "address.state": 1 } },
  
  // Geospatial index for location-based queries
  { key: { location: "2dsphere" } }
]);
```

---

## Sample Documents

### Complete Product Document

```javascript
db.products.insertOne({
  _id: ObjectId(),
  sellerId: ObjectId("6790a1b2c3d4e5f6a7b8c9d0"),
  
  brand: {
    _id: ObjectId("6790a1b2c3d4e5f6a7b8c9d1"),
    name: "Nike",
    slug: "nike",
    logoUrl: "https://cdn.amcart.com/brands/nike.png"
  },
  
  category: {
    _id: ObjectId("6790a1b2c3d4e5f6a7b8c9d2"),
    name: "T-Shirts",
    slug: "men-tshirts",
    path: "/men/clothing/tshirts",
    level: 2
  },
  
  name: "Nike Dri-FIT Cotton T-Shirt",
  slug: "nike-dri-fit-cotton-tshirt",
  description: "Premium cotton t-shirt with Dri-FIT technology for superior comfort and moisture-wicking performance. Perfect for workouts and everyday wear.",
  shortDescription: "Premium cotton t-shirt with Dri-FIT technology",
  
  basePrice: NumberDecimal("1299.00"),
  compareAtPrice: NumberDecimal("1599.00"),
  costPrice: NumberDecimal("650.00"),
  currency: "INR",
  
  taxCategory: "standard",
  hsnCode: "6109",
  
  // Native attributes - NO JSONB!
  attributes: {
    material: "100% Cotton with Dri-FIT",
    fit: "Regular",
    careInstructions: "Machine wash cold, tumble dry low",
    countryOfOrigin: "Vietnam",
    neckType: "Round Neck",
    sleeveType: "Half Sleeve",
    pattern: "Solid"
  },
  
  specifications: {
    fabricWeight: "180 GSM",
    stretch: "Non-stretch"
  },
  
  tags: ["summer", "casual", "sports", "bestseller", "new-arrival"],
  
  weight: 0.25,
  weightUnit: "kg",
  dimensions: { length: 70, width: 50, height: 2, unit: "cm" },
  
  seo: {
    title: "Nike Dri-FIT Cotton T-Shirt | AmCart",
    description: "Buy premium Nike Dri-FIT cotton t-shirts online at AmCart. Free shipping on orders over ₹499.",
    keywords: ["nike tshirt", "dri-fit", "cotton tshirt", "mens wear"],
    canonicalUrl: "/products/nike-dri-fit-cotton-tshirt"
  },
  
  variants: [
    {
      _id: ObjectId(),
      sku: "NIKE-DF-BLK-S",
      barcode: "8901234567890",
      name: "Black / S",
      options: { color: "Black", size: "S" },
      priceAdjustment: NumberDecimal("0.00"),
      overridePrice: null,
      costPrice: NumberDecimal("650.00"),
      imageUrl: "https://cdn.amcart.com/products/nike-df-black.jpg",
      isActive: true,
      isDefault: false,
      sortOrder: 0
    },
    {
      _id: ObjectId(),
      sku: "NIKE-DF-BLK-M",
      barcode: "8901234567891",
      name: "Black / M",
      options: { color: "Black", size: "M" },
      priceAdjustment: NumberDecimal("0.00"),
      overridePrice: null,
      costPrice: NumberDecimal("650.00"),
      imageUrl: "https://cdn.amcart.com/products/nike-df-black.jpg",
      isActive: true,
      isDefault: true,
      sortOrder: 1
    },
    {
      _id: ObjectId(),
      sku: "NIKE-DF-BLK-L",
      barcode: "8901234567892",
      name: "Black / L",
      options: { color: "Black", size: "L" },
      priceAdjustment: NumberDecimal("0.00"),
      overridePrice: null,
      costPrice: NumberDecimal("650.00"),
      imageUrl: "https://cdn.amcart.com/products/nike-df-black.jpg",
      isActive: true,
      isDefault: false,
      sortOrder: 2
    },
    {
      _id: ObjectId(),
      sku: "NIKE-DF-WHT-M",
      barcode: "8901234567893",
      name: "White / M",
      options: { color: "White", size: "M" },
      priceAdjustment: NumberDecimal("0.00"),
      overridePrice: null,
      costPrice: NumberDecimal("650.00"),
      imageUrl: "https://cdn.amcart.com/products/nike-df-white.jpg",
      isActive: true,
      isDefault: false,
      sortOrder: 3
    },
    {
      _id: ObjectId(),
      sku: "NIKE-DF-BLK-XXL",
      barcode: "8901234567895",
      name: "Black / XXL",
      options: { color: "Black", size: "XXL" },
      priceAdjustment: NumberDecimal("100.00"),
      overridePrice: null,
      costPrice: NumberDecimal("700.00"),
      imageUrl: "https://cdn.amcart.com/products/nike-df-black.jpg",
      isActive: true,
      isDefault: false,
      sortOrder: 5
    }
  ],
  
  images: [
    {
      _id: ObjectId(),
      url: "https://cdn.amcart.com/products/nike-df-1.jpg",
      thumbnailUrl: "https://cdn.amcart.com/products/nike-df-1-thumb.jpg",
      mediumUrl: "https://cdn.amcart.com/products/nike-df-1-medium.jpg",
      altText: "Nike Dri-FIT T-Shirt Front View",
      isPrimary: true,
      sortOrder: 0
    },
    {
      _id: ObjectId(),
      url: "https://cdn.amcart.com/products/nike-df-2.jpg",
      thumbnailUrl: "https://cdn.amcart.com/products/nike-df-2-thumb.jpg",
      mediumUrl: "https://cdn.amcart.com/products/nike-df-2-medium.jpg",
      altText: "Nike Dri-FIT T-Shirt Back View",
      isPrimary: false,
      sortOrder: 1
    },
    {
      _id: ObjectId(),
      url: "https://cdn.amcart.com/products/nike-df-3.jpg",
      thumbnailUrl: "https://cdn.amcart.com/products/nike-df-3-thumb.jpg",
      mediumUrl: "https://cdn.amcart.com/products/nike-df-3-medium.jpg",
      altText: "Nike Dri-FIT T-Shirt Detail",
      isPrimary: false,
      sortOrder: 2
    }
  ],
  
  status: "active",
  isActive: true,
  isFeatured: true,
  isNew: true,
  isBestseller: true,
  
  publishedAt: new Date("2026-01-15T10:00:00Z"),
  
  stats: {
    viewCount: 1500,
    salesCount: 250,
    reviewCount: 45,
    averageRating: 4.5
  },
  
  inventory: {
    totalStock: 365,
    hasVariants: true,
    inStock: true
  },
  
  createdAt: new Date("2026-01-01T00:00:00Z"),
  updatedAt: new Date("2026-01-15T12:00:00Z"),
  deletedAt: null
});
```

---

## Common Queries

### 1. Get Products by Category with Filters

```javascript
// Products in Men's T-Shirts, price range, with pagination
db.products.find({
  "category._id": ObjectId("cat-tshirts"),
  isActive: true,
  status: "active",
  basePrice: { $gte: 500, $lte: 2000 },
  "attributes.material": { $regex: /cotton/i }
})
.sort({ "stats.salesCount": -1 })
.skip(0)
.limit(20)
.projection({
  name: 1,
  slug: 1,
  basePrice: 1,
  compareAtPrice: 1,
  "stats.averageRating": 1,
  "stats.reviewCount": 1,
  "brand.name": 1,
  "images": { $elemMatch: { isPrimary: true } },
  "inventory.inStock": 1
});
```

### 2. Get Product Details by Slug

```javascript
// Single document fetch - no JOINs needed!
db.products.findOne({
  slug: "nike-dri-fit-cotton-tshirt",
  isActive: true,
  deletedAt: null
});
```

### 3. Search Products by Text

```javascript
// Text search with scoring
db.products.find(
  { 
    $text: { $search: "cotton tshirt nike" },
    isActive: true,
    status: "active"
  },
  { 
    score: { $meta: "textScore" } 
  }
)
.sort({ score: { $meta: "textScore" } })
.limit(20);
```

### 4. Filter by Attributes (Native - No JSONB!)

```javascript
// Find products with specific attributes
db.products.find({
  isActive: true,
  status: "active",
  "attributes.material": { $regex: /cotton/i },
  "attributes.fit": "Regular",
  tags: { $in: ["summer", "casual"] }
});
```

### 5. Get Products with Stock Information

```javascript
// Aggregation to join with inventory
db.products.aggregate([
  { $match: { isActive: true, status: "active" } },
  {
    $lookup: {
      from: "inventory",
      localField: "variants._id",
      foreignField: "variantId",
      as: "stockInfo"
    }
  },
  {
    $addFields: {
      totalStock: { $sum: "$stockInfo.quantity" },
      availableStock: {
        $sum: {
          $map: {
            input: "$stockInfo",
            as: "inv",
            in: { $subtract: ["$$inv.quantity", "$$inv.reservedQuantity"] }
          }
        }
      }
    }
  },
  { $match: { availableStock: { $gt: 0 } } },
  { $limit: 20 }
]);
```

### 6. Get Low Stock Alerts

```javascript
db.inventory.aggregate([
  {
    $match: {
      $expr: {
        $lte: [
          { $subtract: ["$quantity", "$reservedQuantity"] },
          "$reorderLevel"
        ]
      }
    }
  },
  {
    $lookup: {
      from: "products",
      localField: "productId",
      foreignField: "_id",
      as: "product"
    }
  },
  { $unwind: "$product" },
  {
    $project: {
      sku: 1,
      productName: "$product.name",
      quantity: 1,
      reservedQuantity: 1,
      availableQuantity: { $subtract: ["$quantity", "$reservedQuantity"] },
      reorderLevel: 1,
      warehouseId: 1
    }
  },
  { $sort: { availableQuantity: 1 } }
]);
```

### 7. Reserve Stock (Transaction)

```javascript
const session = client.startSession();

try {
  session.startTransaction();
  
  // Reserve stock
  const result = await db.inventory.updateOne(
    {
      variantId: ObjectId("variant-id"),
      warehouseId: ObjectId("warehouse-id"),
      $expr: {
        $gte: [
          { $subtract: ["$quantity", "$reservedQuantity"] },
          2  // Quantity to reserve
        ]
      }
    },
    {
      $inc: { reservedQuantity: 2 },
      $set: { updatedAt: new Date() }
    },
    { session }
  );
  
  if (result.modifiedCount === 0) {
    throw new Error("Insufficient stock");
  }
  
  // Log transaction
  await db.inventory_transactions.insertOne({
    inventoryId: ObjectId("inv-id"),
    variantId: ObjectId("variant-id"),
    transactionType: "reserved",
    quantityChange: 2,
    referenceType: "order",
    referenceId: ObjectId("order-id"),
    createdAt: new Date()
  }, { session });
  
  await session.commitTransaction();
} catch (error) {
  await session.abortTransaction();
  throw error;
} finally {
  session.endSession();
}
```

### 8. Category Tree Query

```javascript
// Get all subcategories of a category
db.categories.aggregate([
  { $match: { _id: ObjectId("cat-men") } },
  {
    $graphLookup: {
      from: "categories",
      startWith: "$_id",
      connectFromField: "_id",
      connectToField: "parentId",
      as: "subcategories",
      maxDepth: 3
    }
  }
]);

// Or using path prefix
db.categories.find({
  path: { $regex: "^/men" },
  isActive: true
}).sort({ level: 1, sortOrder: 1 });
```

### 9. Faceted Search (for Filters)

```javascript
db.products.aggregate([
  { $match: { "category.path": { $regex: "^/men/clothing" }, isActive: true } },
  {
    $facet: {
      // Price ranges
      priceRanges: [
        {
          $bucket: {
            groupBy: "$basePrice",
            boundaries: [0, 500, 1000, 2000, 5000, Infinity],
            default: "Other",
            output: { count: { $sum: 1 } }
          }
        }
      ],
      // Brands
      brands: [
        { $group: { _id: "$brand.name", count: { $sum: 1 } } },
        { $sort: { count: -1 } }
      ],
      // Colors (from variants)
      colors: [
        { $unwind: "$variants" },
        { $group: { _id: "$variants.options.color", count: { $sum: 1 } } },
        { $sort: { count: -1 } }
      ],
      // Sizes
      sizes: [
        { $unwind: "$variants" },
        { $group: { _id: "$variants.options.size", count: { $sum: 1 } } }
      ],
      // Rating distribution
      ratings: [
        {
          $bucket: {
            groupBy: "$stats.averageRating",
            boundaries: [0, 3, 4, 4.5, 5],
            default: "Unrated",
            output: { count: { $sum: 1 } }
          }
        }
      ],
      // Total count
      totalCount: [{ $count: "count" }]
    }
  }
]);
```

### 10. Update Embedded Brand

```javascript
// When brand info changes, update all products
db.products.updateMany(
  { "brand._id": ObjectId("brand-nike") },
  {
    $set: {
      "brand.name": "Nike (Updated)",
      "brand.logoUrl": "https://cdn.amcart.com/brands/nike-new.png",
      updatedAt: new Date()
    }
  }
);
```

---

## C# Entity Models

### Product Model

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AmCart.ProductService.Domain.Entities;

[BsonIgnoreExtraElements]
public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string SellerId { get; set; } = null!;
    
    // Embedded documents
    public EmbeddedBrand Brand { get; set; } = null!;
    public EmbeddedCategory Category { get; set; } = null!;
    
    // Basic Info
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    
    // Pricing
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal BasePrice { get; set; }
    
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? CompareAtPrice { get; set; }
    
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? CostPrice { get; set; }
    
    public string Currency { get; set; } = "INR";
    
    // Tax
    public string TaxCategory { get; set; } = "standard";
    public string? HsnCode { get; set; }
    
    // Native document attributes - NO JSONB conversion needed!
    public ProductAttributes Attributes { get; set; } = new();
    public ProductSpecifications Specifications { get; set; } = new();
    
    // Tags
    public List<string> Tags { get; set; } = new();
    
    // Physical
    public double? Weight { get; set; }
    public string WeightUnit { get; set; } = "kg";
    public Dimensions? Dimensions { get; set; }
    
    // SEO
    public SeoMetadata? Seo { get; set; }
    
    // Embedded arrays
    public List<ProductVariant> Variants { get; set; } = new();
    public List<ProductImage> Images { get; set; } = new();
    
    // Status
    public string Status { get; set; } = "draft";
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsNew { get; set; } = true;
    public bool IsBestseller { get; set; } = false;
    
    // Publishing
    public DateTime? PublishedAt { get; set; }
    
    // Denormalized stats
    public ProductStats Stats { get; set; } = new();
    
    // Denormalized inventory
    public InventorySummary Inventory { get; set; } = new();
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    
    // Computed
    [BsonIgnore]
    public ProductImage? PrimaryImage => Images.FirstOrDefault(i => i.IsPrimary);
    
    [BsonIgnore]
    public ProductVariant? DefaultVariant => Variants.FirstOrDefault(v => v.IsDefault);
}

// Embedded Brand (denormalized)
public class EmbeddedBrand
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LogoUrl { get; set; }
}

// Embedded Category (denormalized)
public class EmbeddedCategory
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Path { get; set; } = null!;
    public int Level { get; set; }
}

// Native attributes - flexible per product!
public class ProductAttributes
{
    public string? Material { get; set; }
    public string? Fit { get; set; }
    public string? CareInstructions { get; set; }
    public string? CountryOfOrigin { get; set; }
    public string? NeckType { get; set; }
    public string? SleeveType { get; set; }
    public string? Pattern { get; set; }
    
    // For truly dynamic attributes, use BsonExtraElements
    [BsonExtraElements]
    public BsonDocument? ExtraAttributes { get; set; }
}

public class ProductSpecifications
{
    public string? FabricWeight { get; set; }
    public string? Stretch { get; set; }
    
    [BsonExtraElements]
    public BsonDocument? ExtraSpecs { get; set; }
}

public class Dimensions
{
    public double? Length { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string Unit { get; set; } = "cm";
}

public class SeoMetadata
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Keywords { get; set; }
    public string? CanonicalUrl { get; set; }
}

public class ProductStats
{
    public int ViewCount { get; set; }
    public int SalesCount { get; set; }
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
}

public class InventorySummary
{
    public int TotalStock { get; set; }
    public bool HasVariants { get; set; }
    public bool InStock { get; set; }
}
```

### ProductVariant Model (Embedded)

```csharp
public class ProductVariant
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public string Sku { get; set; } = null!;
    public string? Barcode { get; set; }
    public string? Name { get; set; }
    
    // Variant options - native document
    public VariantOptions Options { get; set; } = new();
    
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PriceAdjustment { get; set; } = 0;
    
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? OverridePrice { get; set; }
    
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? CostPrice { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}

public class VariantOptions
{
    public string? Color { get; set; }
    public string? Size { get; set; }
    
    // For other variant types
    [BsonExtraElements]
    public BsonDocument? ExtraOptions { get; set; }
}
```

### ProductImage Model (Embedded)

```csharp
public class ProductImage
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public string Url { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public string? MediumUrl { get; set; }
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
```

### Category Model

```csharp
[BsonIgnoreExtraElements]
public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }
    
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    
    // Hierarchy
    public int Level { get; set; }
    public string Path { get; set; } = null!;
    public List<string> PathIds { get; set; } = new();
    
    // Attribute template for products
    public List<AttributeDefinition> AttributeTemplate { get; set; } = new();
    
    // Display
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    
    // Stats
    public int ProductCount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AttributeDefinition
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = "text";  // text, select, number, boolean
    public List<string>? Options { get; set; }
    public bool Required { get; set; } = false;
}
```

### Inventory Model

```csharp
[BsonIgnoreExtraElements]
public class Inventory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string VariantId { get; set; } = null!;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; } = null!;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string WarehouseId { get; set; } = null!;
    
    public string Sku { get; set; } = null!;
    public string? LocationCode { get; set; }
    
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    
    [BsonIgnore]
    public int AvailableQuantity => Quantity - ReservedQuantity;
    
    public int ReorderLevel { get; set; } = 10;
    public int ReorderQuantity { get; set; } = 50;
    public int? MaxQuantity { get; set; }
    
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? AverageCost { get; set; }
    
    public DateTime? LastRestockAt { get; set; }
    public DateTime? LastSoldAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [BsonIgnore]
    public bool IsLowStock => AvailableQuantity <= ReorderLevel;
    
    [BsonIgnore]
    public bool IsOutOfStock => AvailableQuantity <= 0;
}
```

### Warehouse Model

```csharp
[BsonIgnoreExtraElements]
public class Warehouse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string WarehouseType { get; set; } = "fulfillment";
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string? SellerId { get; set; }
    
    public Address Address { get; set; } = null!;
    
    // GeoJSON Point for geospatial queries
    public GeoJsonPoint Location { get; set; } = null!;
    
    public List<string> ServiceablePincodes { get; set; } = new();
    public int? MaxDeliveryRadiusKm { get; set; }
    
    public Contact Contact { get; set; } = new();
    public Dictionary<string, OperatingHours?> OperatingHours { get; set; } = new();
    
    public int? MaxCapacity { get; set; }
    public int CurrentUtilization { get; set; }
    public string? StorageType { get; set; }
    
    public int Priority { get; set; } = 0;
    
    public bool SupportsCod { get; set; } = true;
    public bool SupportsReturns { get; set; } = true;
    public bool SupportsSameDay { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    public bool IsAcceptingInventory { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Address
{
    public string Line1 { get; set; } = null!;
    public string? Line2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = "India";
}

public class GeoJsonPoint
{
    public string Type { get; set; } = "Point";
    public double[] Coordinates { get; set; } = new double[2];  // [longitude, latitude]
}

public class Contact
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class OperatingHours
{
    public string Open { get; set; } = "09:00";
    public string Close { get; set; } = "18:00";
}
```

---

## Data Access Patterns

### MongoDB Context

```csharp
using MongoDB.Driver;

namespace AmCart.ProductService.Infrastructure.Data;

public class ProductMongoContext
{
    private readonly IMongoDatabase _database;
    
    public ProductMongoContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB");
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "amcart_products";
        
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }
    
    public IMongoCollection<Product> Products => 
        _database.GetCollection<Product>("products");
    
    public IMongoCollection<Category> Categories => 
        _database.GetCollection<Category>("categories");
    
    public IMongoCollection<Brand> Brands => 
        _database.GetCollection<Brand>("brands");
    
    public IMongoCollection<Seller> Sellers => 
        _database.GetCollection<Seller>("sellers");
    
    public IMongoCollection<Warehouse> Warehouses => 
        _database.GetCollection<Warehouse>("warehouses");
    
    public IMongoCollection<Inventory> Inventory => 
        _database.GetCollection<Inventory>("inventory");
    
    public IMongoCollection<InventoryTransaction> InventoryTransactions => 
        _database.GetCollection<InventoryTransaction>("inventory_transactions");
    
    public IMongoCollection<PriceHistory> PriceHistory => 
        _database.GetCollection<PriceHistory>("price_history");
}
```

### Product Repository

```csharp
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AmCart.ProductService.Infrastructure.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<(List<Product> Products, long TotalCount)> GetByCategoryAsync(
        string categoryId, ProductFilter filter, int page, int pageSize);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> SoftDeleteAsync(string id);
}

public class ProductRepository : IProductRepository
{
    private readonly ProductMongoContext _context;
    
    public ProductRepository(ProductMongoContext context)
    {
        _context = context;
    }
    
    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _context.Products
            .Find(p => p.Id == id && p.DeletedAt == null)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .Find(p => p.Slug == slug && p.IsActive && p.DeletedAt == null)
            .FirstOrDefaultAsync();
    }
    
    public async Task<(List<Product> Products, long TotalCount)> GetByCategoryAsync(
        string categoryId, ProductFilter filter, int page, int pageSize)
    {
        var filterBuilder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>
        {
            filterBuilder.Eq(p => p.Category.Id, categoryId),
            filterBuilder.Eq(p => p.IsActive, true),
            filterBuilder.Eq(p => p.Status, "active"),
            filterBuilder.Eq(p => p.DeletedAt, null)
        };
        
        // Price range
        if (filter.MinPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.BasePrice, filter.MinPrice.Value));
        if (filter.MaxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.BasePrice, filter.MaxPrice.Value));
        
        // Brand filter
        if (filter.BrandIds?.Any() == true)
            filters.Add(filterBuilder.In(p => p.Brand.Id, filter.BrandIds));
        
        // Tags filter
        if (filter.Tags?.Any() == true)
            filters.Add(filterBuilder.AnyIn(p => p.Tags, filter.Tags));
        
        // Rating filter
        if (filter.MinRating.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Stats.AverageRating, filter.MinRating.Value));
        
        // In stock filter
        if (filter.InStockOnly)
            filters.Add(filterBuilder.Eq(p => p.Inventory.InStock, true));
        
        var combinedFilter = filterBuilder.And(filters);
        
        // Sorting
        var sortBuilder = Builders<Product>.Sort;
        SortDefinition<Product> sort = filter.SortBy switch
        {
            "price_asc" => sortBuilder.Ascending(p => p.BasePrice),
            "price_desc" => sortBuilder.Descending(p => p.BasePrice),
            "rating" => sortBuilder.Descending(p => p.Stats.AverageRating),
            "newest" => sortBuilder.Descending(p => p.CreatedAt),
            _ => sortBuilder.Descending(p => p.Stats.SalesCount)  // popularity
        };
        
        var totalCount = await _context.Products.CountDocumentsAsync(combinedFilter);
        
        var products = await _context.Products
            .Find(combinedFilter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        
        return (products, totalCount);
    }
    
    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.Products.InsertOneAsync(product);
        return product;
    }
    
    public async Task<bool> UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        
        var result = await _context.Products.ReplaceOneAsync(
            p => p.Id == product.Id,
            product
        );
        
        return result.ModifiedCount > 0;
    }
    
    public async Task<bool> SoftDeleteAsync(string id)
    {
        var update = Builders<Product>.Update
            .Set(p => p.DeletedAt, DateTime.UtcNow)
            .Set(p => p.IsActive, false)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);
        
        var result = await _context.Products.UpdateOneAsync(
            p => p.Id == id,
            update
        );
        
        return result.ModifiedCount > 0;
    }
}

public class ProductFilter
{
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string>? BrandIds { get; set; }
    public List<string>? Tags { get; set; }
    public double? MinRating { get; set; }
    public bool InStockOnly { get; set; }
    public string SortBy { get; set; } = "popularity";
}
```

### Inventory Repository

```csharp
public interface IInventoryRepository
{
    Task<bool> ReserveStockAsync(string variantId, string warehouseId, int quantity, string orderId);
    Task<bool> DeductStockAsync(string variantId, string warehouseId, int quantity, string orderId);
    Task<bool> ReleaseReservationAsync(string variantId, string warehouseId, int quantity, string orderId);
    Task<List<Inventory>> GetLowStockAsync();
}

public class InventoryRepository : IInventoryRepository
{
    private readonly ProductMongoContext _context;
    private readonly IMongoClient _client;
    
    public InventoryRepository(ProductMongoContext context, IMongoClient client)
    {
        _context = context;
        _client = client;
    }
    
    public async Task<bool> ReserveStockAsync(
        string variantId, string warehouseId, int quantity, string orderId)
    {
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        
        try
        {
            // Find and update inventory
            var filter = Builders<Inventory>.Filter.And(
                Builders<Inventory>.Filter.Eq(i => i.VariantId, variantId),
                Builders<Inventory>.Filter.Eq(i => i.WarehouseId, warehouseId),
                Builders<Inventory>.Filter.Where(i => 
                    i.Quantity - i.ReservedQuantity >= quantity)
            );
            
            var update = Builders<Inventory>.Update
                .Inc(i => i.ReservedQuantity, quantity)
                .Set(i => i.UpdatedAt, DateTime.UtcNow);
            
            var result = await _context.Inventory.UpdateOneAsync(
                session, filter, update);
            
            if (result.ModifiedCount == 0)
            {
                await session.AbortTransactionAsync();
                return false;  // Insufficient stock
            }
            
            // Log transaction
            var inventory = await _context.Inventory
                .Find(session, Builders<Inventory>.Filter.And(
                    Builders<Inventory>.Filter.Eq(i => i.VariantId, variantId),
                    Builders<Inventory>.Filter.Eq(i => i.WarehouseId, warehouseId)))
                .FirstOrDefaultAsync();
            
            var transaction = new InventoryTransaction
            {
                InventoryId = inventory!.Id,
                VariantId = variantId,
                ProductId = inventory.ProductId,
                WarehouseId = warehouseId,
                TransactionType = "reserved",
                QuantityChange = quantity,
                QuantityBefore = inventory.Quantity,
                QuantityAfter = inventory.Quantity,
                ReferenceType = "order",
                ReferenceId = orderId,
                CreatedAt = DateTime.UtcNow
            };
            
            await _context.InventoryTransactions.InsertOneAsync(session, transaction);
            
            await session.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
    
    public async Task<bool> DeductStockAsync(
        string variantId, string warehouseId, int quantity, string orderId)
    {
        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        
        try
        {
            var filter = Builders<Inventory>.Filter.And(
                Builders<Inventory>.Filter.Eq(i => i.VariantId, variantId),
                Builders<Inventory>.Filter.Eq(i => i.WarehouseId, warehouseId),
                Builders<Inventory>.Filter.Gte(i => i.ReservedQuantity, quantity)
            );
            
            var update = Builders<Inventory>.Update
                .Inc(i => i.Quantity, -quantity)
                .Inc(i => i.ReservedQuantity, -quantity)
                .Set(i => i.LastSoldAt, DateTime.UtcNow)
                .Set(i => i.UpdatedAt, DateTime.UtcNow);
            
            var result = await _context.Inventory.UpdateOneAsync(session, filter, update);
            
            if (result.ModifiedCount == 0)
            {
                await session.AbortTransactionAsync();
                return false;
            }
            
            await session.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
    
    public async Task<List<Inventory>> GetLowStockAsync()
    {
        return await _context.Inventory
            .Find(i => i.Quantity - i.ReservedQuantity <= i.ReorderLevel)
            .SortBy(i => i.Quantity)
            .ToListAsync();
    }
    
    public async Task<bool> ReleaseReservationAsync(
        string variantId, string warehouseId, int quantity, string orderId)
    {
        var update = Builders<Inventory>.Update
            .Inc(i => i.ReservedQuantity, -quantity)
            .Set(i => i.UpdatedAt, DateTime.UtcNow);
        
        var result = await _context.Inventory.UpdateOneAsync(
            i => i.VariantId == variantId && i.WarehouseId == warehouseId,
            update
        );
        
        return result.ModifiedCount > 0;
    }
}
```

---

## OpenSearch Sync

### Change Stream for Real-time Sync

```csharp
using MongoDB.Bson;
using MongoDB.Driver;

public class ProductChangeStreamService : BackgroundService
{
    private readonly ProductMongoContext _context;
    private readonly IOpenSearchClient _openSearchClient;
    private readonly ILogger<ProductChangeStreamService> _logger;
    
    public ProductChangeStreamService(
        ProductMongoContext context,
        IOpenSearchClient openSearchClient,
        ILogger<ProductChangeStreamService> logger)
    {
        _context = context;
        _openSearchClient = openSearchClient;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Product>>()
            .Match(change => 
                change.OperationType == ChangeStreamOperationType.Insert ||
                change.OperationType == ChangeStreamOperationType.Update ||
                change.OperationType == ChangeStreamOperationType.Replace ||
                change.OperationType == ChangeStreamOperationType.Delete);
        
        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
        };
        
        using var cursor = await _context.Products.WatchAsync(pipeline, options, stoppingToken);
        
        _logger.LogInformation("Started watching product changes");
        
        await cursor.ForEachAsync(async change =>
        {
            try
            {
                switch (change.OperationType)
                {
                    case ChangeStreamOperationType.Insert:
                    case ChangeStreamOperationType.Update:
                    case ChangeStreamOperationType.Replace:
                        if (change.FullDocument != null)
                        {
                            await IndexProductAsync(change.FullDocument);
                        }
                        break;
                    
                    case ChangeStreamOperationType.Delete:
                        await DeleteProductFromIndexAsync(change.DocumentKey["_id"].AsObjectId.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing change for document {DocumentId}", 
                    change.DocumentKey["_id"]);
            }
        }, stoppingToken);
    }
    
    private async Task IndexProductAsync(Product product)
    {
        if (product.Status != "active" || !product.IsActive || product.DeletedAt != null)
        {
            await DeleteProductFromIndexAsync(product.Id);
            return;
        }
        
        var searchDoc = new ProductSearchDocument
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            BasePrice = product.BasePrice,
            CompareAtPrice = product.CompareAtPrice,
            CategoryId = product.Category.Id,
            CategoryName = product.Category.Name,
            CategoryPath = product.Category.Path,
            BrandId = product.Brand.Id,
            BrandName = product.Brand.Name,
            SellerId = product.SellerId,
            Attributes = product.Attributes,
            Tags = product.Tags,
            AverageRating = product.Stats.AverageRating,
            ReviewCount = product.Stats.ReviewCount,
            SalesCount = product.Stats.SalesCount,
            IsFeatured = product.IsFeatured,
            IsNew = product.IsNew,
            IsBestseller = product.IsBestseller,
            InStock = product.Inventory.InStock,
            ImageUrl = product.PrimaryImage?.Url,
            CreatedAt = product.CreatedAt,
            Variants = product.Variants.Select(v => new VariantSearchDocument
            {
                Id = v.Id,
                Sku = v.Sku,
                Options = v.Options,
                Price = v.OverridePrice ?? product.BasePrice + v.PriceAdjustment,
                InStock = true  // Would need inventory lookup
            }).ToList()
        };
        
        await _openSearchClient.IndexAsync(searchDoc, i => i.Index("products").Id(product.Id));
        
        _logger.LogDebug("Indexed product {ProductId} to OpenSearch", product.Id);
    }
    
    private async Task DeleteProductFromIndexAsync(string productId)
    {
        await _openSearchClient.DeleteAsync<ProductSearchDocument>(productId, d => d.Index("products"));
        
        _logger.LogDebug("Deleted product {ProductId} from OpenSearch", productId);
    }
}
```

### OpenSearch Index Mapping

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
      "shortDescription": { "type": "text" },
      "basePrice": { "type": "float" },
      "compareAtPrice": { "type": "float" },
      "categoryId": { "type": "keyword" },
      "categoryName": { "type": "keyword" },
      "categoryPath": { "type": "keyword" },
      "brandId": { "type": "keyword" },
      "brandName": { 
        "type": "text",
        "fields": { "keyword": { "type": "keyword" } }
      },
      "sellerId": { "type": "keyword" },
      "attributes": { "type": "flattened" },
      "tags": { "type": "keyword" },
      "averageRating": { "type": "float" },
      "reviewCount": { "type": "integer" },
      "salesCount": { "type": "integer" },
      "isFeatured": { "type": "boolean" },
      "isNew": { "type": "boolean" },
      "isBestseller": { "type": "boolean" },
      "inStock": { "type": "boolean" },
      "imageUrl": { "type": "keyword", "index": false },
      "createdAt": { "type": "date" },
      "variants": {
        "type": "nested",
        "properties": {
          "id": { "type": "keyword" },
          "sku": { "type": "keyword" },
          "options": { "type": "flattened" },
          "price": { "type": "float" },
          "inStock": { "type": "boolean" }
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

---

## Summary

### Collections Overview

| Collection | Purpose | Key Features |
|------------|---------|--------------|
| `products` | Main product data | Embedded variants, images, brand, category |
| `categories` | Hierarchical categories | Materialized path, attribute templates |
| `brands` | Product brands | SEO metadata |
| `sellers` | Multi-vendor support | Verification, commission |
| `warehouses` | Multi-warehouse locations | Geospatial, capabilities |
| `inventory` | Stock per warehouse (separate) | High-write optimized |
| `inventory_transactions` | Stock audit trail | All stock movements |
| `price_history` | Price tracking | Effective dates |
| `collections` | Product groupings | Manual/automated |

### MongoDB vs PostgreSQL Trade-offs for AmCart

| Aspect | MongoDB Advantage | PostgreSQL Advantage |
|--------|-------------------|----------------------|
| **Product Listing** | ✅ Single document fetch | ⚠️ JOINs needed |
| **Flexible Attributes** | ✅ Native documents | ⚠️ JSONB operators |
| **Inventory Transactions** | ⚠️ Multi-doc transactions | ✅ Full ACID |
| **Complex Reports** | ⚠️ Aggregation pipeline | ✅ SQL analytics |
| **Schema Evolution** | ✅ No migrations | ⚠️ Schema migrations |
| **Team Learning Curve** | ⚠️ MongoDB specific | ✅ SQL familiar |

### This Schema Supports

- ✅ Multi-vendor marketplace
- ✅ Native flexible product attributes (no JSONB)
- ✅ Embedded product variants
- ✅ Embedded product images
- ✅ Multi-warehouse inventory
- ✅ Inventory reservations
- ✅ Price history tracking
- ✅ Category hierarchy
- ✅ OpenSearch sync via Change Streams
- ✅ Geospatial warehouse queries
- ✅ Soft delete for data retention

### AWS Deployment

- **Database**: Amazon DocumentDB (MongoDB 5.0 compatible)
- **Search**: Amazon OpenSearch Service
- **Cache**: Amazon ElastiCache (Redis)
- **Storage**: Amazon S3 (product images)

