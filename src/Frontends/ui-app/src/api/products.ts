const base = () =>
  (import.meta.env.VITE_PRODUCT_SERVICE_URL?.trim() || "/product-api").replace(
    /\/$/,
    ""
  );

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${base()}${path}`);
  const json = await res.json().catch(() => ({}));
  if (!res.ok)
    throw new Error(
      (json as { error?: { message?: string } })?.error?.message ??
        `Request failed (${res.status})`
    );
  return json as T;
}

/* ── Types ── */

export type ProductImage = {
  id?: string;
  url: string;
  altText?: string;
  displayOrder: number;
  isPrimary: boolean;
};

export type ProductDto = {
  id: string;
  name: string;
  slug: string;
  shortDescription?: string;
  description?: string;
  sku: string;
  price: number;
  compareAtPrice?: number;
  currency?: string;
  quantity?: number;
  status: string;
  isFeatured: boolean;
  categoryId?: string;
  brandId?: string;
  categoryName?: string;
  brandName?: string;
  averageRating: number;
  reviewCount: number;
  createdAt: string;
  publishedAt?: string;
  images?: ProductImage[];
  primaryImageUrl?: string;
  tagNames?: string[];
};

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

export type CategoryDto = {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  parentCategoryId?: string;
  displayOrder: number;
  isActive: boolean;
  /** Populated when loading `/v1/categories/roots`; flat `/v1/categories` list leaves this empty. */
  subCategories?: CategoryDto[];
};

export type BrandDto = {
  id: string;
  name: string;
  slug: string;
  description?: string;
  logoUrl?: string;
  isActive: boolean;
};

/* ── Queries ── */

export type ProductQuery = {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  brandId?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  sortDesc?: boolean;
  search?: string;
};

export async function fetchPublicProducts(params: ProductQuery = {}) {
  const q = new URLSearchParams();
  if (params.page) q.set("page", String(params.page));
  if (params.pageSize) q.set("pageSize", String(params.pageSize));
  if (params.categoryId) q.set("categoryId", params.categoryId);
  if (params.brandId) q.set("brandId", params.brandId);
  if (params.minPrice != null && params.minPrice > 0)
    q.set("minPrice", String(params.minPrice));
  if (params.maxPrice != null && params.maxPrice > 0)
    q.set("maxPrice", String(params.maxPrice));
  if (params.sortBy) q.set("sortBy", params.sortBy);
  if (params.sortDesc) q.set("sortDesc", "true");
  if (params.search) q.set("search", params.search);
  return get<{ data: PagedResult<ProductDto> }>(`/v1/products?${q}`);
}

/** Root categories with nested `subCategories` (tree). Do not use flat `/v1/categories` for storefront filters. */
export async function fetchPublicCategories() {
  return get<{ data: CategoryDto[] }>("/v1/categories/roots");
}

export async function fetchPublicBrands() {
  return get<{ data: BrandDto[] }>("/v1/brands?includeInactive=false");
}

export async function fetchFeaturedProducts(count = 10) {
  return get<{ data: ProductDto[] }>(`/v1/products/featured?count=${count}`);
}
