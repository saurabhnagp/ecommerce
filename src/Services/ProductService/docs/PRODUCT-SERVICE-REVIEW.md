# ProductService – Review: Missing Features & Wrong Implementations

This document lists **missing features** and **wrong or incomplete implementations** in the ProductService as of the review date. The service has a complete **Domain** (entities), **Infrastructure** (DbContext, configurations, repositories), and **migrations**, but almost no **Application** or **API** surface.

---

## 1. Missing Features (Not Yet Implemented)

### 1.1 Application Layer (Entirely Missing)

| # | Missing Item | Description |
|---|----------------|-------------|
| 1 | **Application services** | No `IProductService`, `ICategoryService`, `IBrandService`, `IProductReviewService`. Business logic (slug generation, validation, updating aggregate ratings) and orchestration should live here; API would call services, not repositories. |
| 2 | **DTOs (Data transfer objects)** | No request/response DTOs. Examples: `ProductDto`, `ProductListDto`, `CreateProductRequest`, `UpdateProductRequest`, `CategoryDto`, `BrandDto`, `ProductReviewDto`, `PagedResult<T>`, etc. Without these, API would expose domain entities (tight coupling, circular refs, over-posting). |
| 3 | **Mapping (Entity ↔ DTO)** | No `ProductMapping`, `CategoryMapping`, etc., to map between entities and DTOs. |
| 4 | **Input validation** | No validation attributes or FluentValidation for requests (e.g. required name, valid price, slug format). |

### 1.2 API Layer (Only Health Exists)

| # | Missing Item | Description |
|---|----------------|-------------|
| 5 | **Products API** | No controller for products. Expected (at least): `GET /api/v1/products` (paged, filter by category/brand/status, search, sort), `GET /api/v1/products/{id}`, `GET /api/v1/products/by-slug/{slug}`, `GET /api/v1/products/featured`, `POST` (create), `PUT` (update), `DELETE` (soft delete). |
| 6 | **Categories API** | No controller for categories. Expected: `GET /api/v1/categories` (tree or flat), `GET /api/v1/categories/roots`, `GET /api/v1/categories/{id}`, `GET /api/v1/categories/by-slug/{slug}`, `POST`, `PUT`, `DELETE`. |
| 7 | **Brands API** | No controller for brands. Expected: `GET /api/v1/brands`, `GET /api/v1/brands/{id}`, `GET /api/v1/brands/by-slug/{slug}`, `POST`, `PUT`, `DELETE`. |
| 8 | **Product reviews API** | No controller for reviews. Expected: `GET /api/v1/products/{productId}/reviews` (paged), `POST` (submit review), and optionally `PUT`/`DELETE` or moderate (approve). |
| 9 | **API versioning** | Health uses `api/[controller]`; no consistent version prefix (e.g. `api/v1/...`) for future compatibility. |
| 10 | **Health check for database** | `AddHealthChecks()` is called but only the default check is used. No `AddNpgSql()` or `AddDbContextCheck<ProductDbContext>()` to verify DB connectivity. |

### 1.3 Domain / Product Lifecycle

| # | Missing Item | Description |
|---|----------------|-------------|
| 11 | **Slug generation** | Slug is required on `Product` (and Category/Brand) but there is no logic to generate a unique slug from the name (e.g. "Blue Chair" → "blue-chair", with uniqueness check and optional suffix). |
| 12 | **Publish workflow** | Product has `Status` (draft/active/archived) and `PublishedAt` but no explicit “publish” step that sets `Status = "active"` and `PublishedAt = DateTime.UtcNow`. |
| 13 | **Stock / availability** | Product and variants have `Quantity` and `TrackInventory`, but no service method or API to “reserve” or “decrease stock” (e.g. when order is placed). No “low stock” or “out of stock” semantics exposed. |

### 1.4 Reviews and Ratings

| # | Missing Item | Description |
|---|----------------|-------------|
| 14 | **Update Product.AverageRating and ReviewCount** | When a review is added, updated, or deleted (or approved), `Product.AverageRating` and `Product.ReviewCount` are never recalculated. They exist on the entity but are not maintained (see Wrong Implementations below). |
| 15 | **Review submission rules** | No business rules enforced in application layer: e.g. one review per user per product, only verified purchasers can review, rating 1–5, approval workflow. |

### 1.5 Infrastructure / DevOps

| # | Missing Item | Description |
|---|----------------|-------------|
| 16 | **Unit tests** | No unit test project for ProductService (no tests for repositories, services, or API). |
| 17 | **Integration tests** | No tests that run against a real or in-memory database. |
| 18 | **Dockerfile** | ProductService has a Dockerfile at repo root for the service; confirm it is present and correct for the current project layout. |

### 1.6 Security and Multi-Tenancy

| # | Missing Item | Description |
|---|----------------|-------------|
| 19 | **Authorization** | No auth on any endpoint (only Health exists). Product create/update/delete and category/brand/review management should be protected (e.g. admin or seller role). |
| 20 | **Seller / tenant scope** | `Product` has `SellerId` but no repository or API filters by seller; no multi-tenant or “my products” support. |

---

## 2. Wrong or Incomplete Implementations

### 2.1 Product aggregate (rating and review count)

| # | Issue | Location | Description |
|---|--------|----------|-------------|
| 1 | **AverageRating and ReviewCount never updated** | Domain + Application | `Product.AverageRating` and `Product.ReviewCount` are stored on the entity but no code updates them when reviews are added, updated, deleted, or approved. So they will stay at 0 (or stale). **Fix:** After any review change, recalculate and set `product.AverageRating` and `product.ReviewCount` (e.g. in a service that uses `IProductReviewRepository` and `IProductRepository`). |

### 2.2 Repository / data access

| # | Issue | Location | Description |
|---|--------|----------|-------------|
| 2 | **GetByIdAsync / GetBySlugAsync return draft products** | `ProductRepository` | For a **public** catalog, `GetByIdAsync` and `GetBySlugAsync` should typically return only products with `Status == "active"`. Currently they return any non-deleted product, so draft products could be exposed. **Fix:** Either add a parameter (e.g. `forPublicCatalog`) or separate methods like `GetByIdForPublicAsync` that filter by `Status == "active"`. |
| 3 | **GetPagedAsync default behavior** | `ProductRepository` | When `status` is not supplied, the paged list includes all statuses (draft, active, archived). For a public listing API, default should usually be `status = "active"`. **Fix:** In application service or repository, default `status` to `"active"` for public endpoints. |
| 4 | **Soft-deleted product not retrievable** | `ProductConfiguration` + `ProductRepository` | Global query filter `DeletedAt == null` means soft-deleted products are never returned by `GetByIdAsync` or any query. So “restore” or “admin view deleted” is impossible. **Fix:** If needed, add repository methods (or overloads) that use `IgnoreQueryFilters()` for admin/restore scenarios. |
| 5 | **Product update and child collections** | `ProductRepository.UpdateAsync` | `Update(Product product)` is used with a single entity. If the API or service passes a **detached** product with modified `Images`, `Variants`, `Attributes`, or `Tags`, EF Core may not correctly persist adds/updates/deletes of these collections. **Fix:** Prefer “load entity (with includes) → modify in memory → SaveChanges” in a service, or explicitly attach and handle collection changes (e.g. clear and re-add, or track each child). |

### 2.3 Categories and brands

| # | Issue | Location | Description |
|---|--------|----------|-------------|
| 6 | **CategoryRepository.HasProductsAsync** | `CategoryRepository` | Uses `_db.Products.AnyAsync(p => p.CategoryId == categoryId)`. Because of the global query filter on `Product`, this correctly counts only **non-deleted** products. So “has products” is correct. No change needed; listed for completeness. |
| 7 | **Category delete with subcategories** | Domain / API | If a category has subcategories, deleting it may leave orphans or require a business rule (e.g. disallow delete, or cascade). No logic implemented yet; when you add Category delete API, implement the rule. |

### 2.4 Unique constraints and soft delete

| # | Issue | Location | Description |
|---|--------|----------|-------------|
| 8 | **Unique slug and soft delete** | DB + Product | The unique index on `slug` is at the database level. After soft-deleting a product, another product cannot reuse the same slug. If the business wants to allow slug reuse after delete, you’d need to remove the DB unique constraint and enforce uniqueness only among non-deleted rows (e.g. in code or with a partial unique index). |

---

## 3. Summary Table

| Category | Missing | Wrong / Incomplete |
|----------|---------|--------------------|
| **Application** | Services, DTOs, mapping, validation (1–4) | — |
| **API** | Products, Categories, Brands, Reviews controllers; versioning; DB health (5–10) | — |
| **Domain / lifecycle** | Slug generation, publish workflow, stock (11–13) | — |
| **Reviews** | Review rules (14–15) | Rating/count not updated (1) |
| **Infrastructure / DevOps** | Unit/integration tests, Dockerfile check (16–18) | — |
| **Security** | Authorization, seller scope (19–20) | — |
| **Repository / data** | — | Public vs draft (2–3), soft-delete retrieval (4), update children (5), category delete rule (7), slug uniqueness (8) |

---

## 4. Recommended Order of Work

1. **Application layer:** DTOs, mapping, then `IProductService` (and optionally `ICategoryService`, `IBrandService`, `IProductReviewService`) with slug generation and **recalculation of AverageRating/ReviewCount** on review changes.
2. **API layer:** Products controller (paged list, get by id/slug, featured, create, update, soft delete) with **public vs admin** behavior (e.g. public only `status = "active"`).
3. **Categories and Brands API:** Controllers and, if needed, services.
4. **Reviews API:** Get reviews for product, submit review; in application layer, update `Product.AverageRating` and `Product.ReviewCount` on add/update/delete/approve.
5. **Health:** Add DB health check.
6. **Auth:** Add authorization for write/admin endpoints.
7. **Tests:** Unit tests for services and repositories; optionally integration tests for API or DbContext.

This document can be used as a checklist to implement missing features and fix wrong implementations in the ProductService.

---

## 5. Repository and data access layer – implemented

The following repository/data access fixes from §2.2 have been implemented:

- **GetByIdAsync / GetBySlugAsync (item 2):** Added optional parameter `publicOnly`. When `true`, only products with `Status == "active"` are returned.
- **GetPagedAsync (item 3):** Added parameter `defaultToActiveStatus`. When `true` and `status` is null, the query uses `status = "active"`.
- **Soft-deleted retrieval (item 4):** Added `GetByIdIncludingDeletedAsync` and `GetBySlugIncludingDeletedAsync` using `IgnoreQueryFilters()`.
- **Product update with children (item 5):** Added `UpdateProductAsync(Guid id, Func<Product, Task> apply)` which loads the product with all includes (tracked), runs the delegate, then saves. Use this when modifying Images, Variants, Attributes, or Tags.
- **Rating/review count:** Added `UpdateRatingAndReviewCountAsync(Guid productId, double averageRating, int reviewCount)` for the application layer to call after review add/update/delete/approve.

---

## 6. Application layer, API, DI, and health – implemented

The following have been implemented (addressing §1.1, §1.2 items 5–8, §1.4 item 14, §2.1 item 1, §9–10):

**Application layer (§1.1):**
- **DTOs:** `PagedResult<T>`, `ProductDto`, `ProductListDto`, `ProductImageDto`, `ProductVariantDto`, `ProductAttributeDto`, `CreateProductRequest`, `UpdateProductRequest`, `CategoryDto`, `CreateCategoryRequest`, `UpdateCategoryRequest`, `BrandDto`, `CreateBrandRequest`, `UpdateBrandRequest`, `ProductReviewDto`, `CreateProductReviewRequest`, `UpdateProductReviewRequest` with validation attributes where appropriate.
- **Mapping:** `ProductMapping`, `CategoryMapping`, `BrandMapping`, `ProductReviewMapping` (entity ↔ DTO).
- **Validation:** `[Required]`, `[MaxLength]`, `[Range]`, `[Url]` on request DTOs.
- **Slug generation (§1.3 item 11):** `SlugGenerator.FromName` and `GetUniqueSlugAsync` for unique slugs (Category, Brand, Product).
- **Services:** `IProductService` / `ProductService`, `ICategoryService` / `CategoryService`, `IBrandService` / `BrandService`, `IProductReviewService` / `ProductReviewService`. Product: GetById, GetBySlug (publicOnly), GetPaged (defaultToActiveStatus), GetFeatured, GetByCategoryId, GetByBrandId, Create (slug, SKU uniqueness), Update (via repository load-modify-save), Publish (§1.3 item 12), SoftDelete, GetByIdIncludingDeleted. Category: CRUD, GetRootCategories, GetSubCategories; delete blocked if has products or subcategories (§2.3 item 7). Brand: CRUD; delete blocked if has products. Reviews: GetById, GetByProductId (paged), Create (one review per user per product), Update, Approve, Delete.
- **Rating/count fix (§1.4 item 14, §2.1 item 1):** After review add/update/approve/delete, `ProductReviewService` recalculates and calls `UpdateRatingAndReviewCountAsync`.

**API layer (§1.2):**
- **Products:** `api/v1/products` – GET (paged, filters, publicOnly/defaultToActiveStatus), GET featured, GET by id, GET by slug, POST create, PUT update, POST publish, DELETE soft delete.
- **Categories:** `api/v1/categories` – GET all, GET roots, GET by id, GET by slug, GET subcategories, POST, PUT, DELETE.
- **Brands:** `api/v1/brands` – GET all, GET by id, GET by slug, POST, PUT, DELETE.
- **Product reviews:** `api/v1/products/{productId}/reviews` – GET (paged), POST create, GET by id, PUT update, POST approve, DELETE.
- **Health (§1.2 items 9–10):** Health controller under `api/v1/health`; `AddHealthChecks().AddDbContextCheck<ProductDbContext>()`; additional `api/v1/health` endpoint returning JSON with status, service name, version, and check results.

**Infrastructure:**
- Application services registered in DI: `IProductService`, `ICategoryService`, `IBrandService`, `IProductReviewService`.
