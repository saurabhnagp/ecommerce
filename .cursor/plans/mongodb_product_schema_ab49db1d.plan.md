---
name: MongoDB Product Schema
overview: Create a comprehensive MongoDB/DocumentDB-based Product database schema as an alternative to the PostgreSQL approach, leveraging native document structure for flexible product attributes.
todos:
  - id: create-nosql-schema
    content: Create Database-Schema-Product-NoSQL.md with MongoDB collections and documents
    status: completed
isProject: false
---

# MongoDB Product Database Schema

## Overview

Create a new document `Database-Schema-Product-NoSQL.md` that provides a complete MongoDB/Amazon DocumentDB schema for the Product domain, eliminating the need for JSONB by using native document structures.

## Key Differences from PostgreSQL Approach

| Aspect | PostgreSQL + JSONB | MongoDB/DocumentDB |

|--------|-------------------|-------------------|

| **Attributes** | JSONB columns | Native embedded documents |

| **Variants** | Separate table | Embedded array in product |

| **Images** | Separate table | Embedded array in product |

| **Relationships** | Foreign keys | References (manual) or embedded |

| **Transactions** | Full ACID | Multi-document transactions (4.0+) |

| **Schema** | Strict + flexible JSONB | Fully flexible |

## Document Structure

### Products Collection (Denormalized)

```javascript
{
  _id: ObjectId,
  sellerId: ObjectId,
  brand: { _id, name, slug, logoUrl },  // Embedded (read-heavy)
  category: { _id, name, slug, path },  // Embedded
  name: "Nike Dri-FIT T-Shirt",
  slug: "nike-dri-fit-tshirt",
  description: "...",
  basePrice: 1299.00,
  
  // Native document - no JSONB needed!
  attributes: {
    material: "100% Cotton",
    fit: "Regular",
    neckType: "Round Neck"
  },
  
  // Embedded variants array
  variants: [
    {
      _id: ObjectId,
      sku: "NIKE-DF-BLK-M",
      options: { color: "Black", size: "M" },
      priceAdjustment: 0,
      stock: 100
    }
  ],
  
  // Embedded images
  images: [
    { url: "...", isPrimary: true, sortOrder: 0 }
  ],
  
  tags: ["summer", "casual"],
  status: "active",
  createdAt: ISODate,
  updatedAt: ISODate
}
```

## Files to Create

- `[docs/Database-Schema-Product-NoSQL.md](docs/Database-Schema-Product-NoSQL.md)` - Complete MongoDB schema document

## Document Sections

1. **Overview** - Design principles, MongoDB vs PostgreSQL comparison
2. **Collection Schemas** - Products, Categories, Brands, Sellers, Warehouses, Inventory
3. **Document Models** - Full document structure with examples
4. **Indexes** - MongoDB index strategies (compound, text, geospatial)
5. **Sample Documents** - Real example data
6. **Common Queries** - MongoDB query patterns (find, aggregate, $lookup)
7. **C# Entity Models** - MongoDB.Driver entity classes
8. **Data Access Patterns** - Repository pattern with MongoDB
9. **OpenSearch Sync** - Change streams for real-time sync

## AWS Service

- **Amazon DocumentDB** (MongoDB-compatible) for production
- Fully managed, highly available
- Compatible with MongoDB 5.0 API

