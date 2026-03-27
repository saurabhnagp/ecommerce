export function productServiceBase() {
  return (import.meta.env.VITE_PRODUCT_SERVICE_URL?.trim() || "/product-api").replace(
    /\/$/,
    ""
  );
}

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${productServiceBase()}${path}`);
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

export type ProductVariantDto = {
  id: string;
  sku: string;
  name: string;
  price: number;
  compareAtPrice?: number;
  quantity: number;
  optionsJson?: string | null;
  imageUrl?: string | null;
};

export type ProductAttributeDto = {
  id: string;
  name: string;
  value: string;
  displayOrder: number;
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
  lowStockThreshold?: number;
  trackInventory?: boolean;
  status: string;
  isFeatured: boolean;
  isDigital?: boolean;
  categoryId?: string;
  brandId?: string;
  categoryName?: string;
  categorySlug?: string;
  brandName?: string;
  averageRating: number;
  reviewCount: number;
  createdAt: string;
  publishedAt?: string;
  images?: ProductImage[];
  primaryImageUrl?: string;
  tagNames?: string[];
  variants?: ProductVariantDto[];
  attributes?: ProductAttributeDto[];
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
  /** API: outOfStock (public) | lowStock (admin token only). */
  stockFilter?: "outOfStock" | "lowStock";
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
  if (params.stockFilter === "outOfStock") q.set("stockFilter", "outOfStock");
  if (params.stockFilter === "lowStock") q.set("stockFilter", "lowStock");
  return get<{ data: PagedResult<ProductDto> }>(`/v1/products?${q}`);
}

/** Root categories with nested `subCategories` (tree). Do not use flat `/v1/categories` for storefront filters. */
export async function fetchPublicCategories() {
  const res = await get<{ data: CategoryDto[] }>(
    "/v1/categories?includeInactive=false"
  );
  const flat = res.data ?? [];

  const byId = new Map<string, CategoryDto>();
  for (const c of flat) {
    byId.set(c.id, { ...c, subCategories: [] });
  }

  const roots: CategoryDto[] = [];
  for (const c of byId.values()) {
    if (c.parentCategoryId && byId.has(c.parentCategoryId)) {
      byId.get(c.parentCategoryId)!.subCategories!.push(c);
    } else {
      roots.push(c);
    }
  }

  const sortTree = (nodes: CategoryDto[]) => {
    nodes.sort(
      (a, b) =>
        a.displayOrder - b.displayOrder ||
        a.name.localeCompare(b.name, undefined, { sensitivity: "base" })
    );
    for (const n of nodes) sortTree(n.subCategories ?? []);
  };
  sortTree(roots);

  return { data: roots };
}

export async function fetchPublicBrands() {
  return get<{ data: BrandDto[] }>("/v1/brands?includeInactive=false");
}

export async function fetchFeaturedProducts(count = 10) {
  return get<{ data: ProductDto[] }>(`/v1/products/featured?count=${count}`);
}

export type ProductNeighborDto = { slug: string; name: string };

export type ProductNeighborsDto = {
  previous: ProductNeighborDto | null;
  next: ProductNeighborDto | null;
};

export async function fetchProductBySlug(slug: string) {
  const path = `/v1/products/by-slug/${encodeURIComponent(slug)}?publicOnly=true`;
  return get<{ data: ProductDto }>(path);
}

export async function fetchProductNeighbors(productId: string) {
  return get<{ data: ProductNeighborsDto }>(
    `/v1/products/${encodeURIComponent(productId)}/neighbors`
  );
}
