import { getAccessToken } from "../auth/storage";

function productBase() {
  const u = import.meta.env.VITE_PRODUCT_SERVICE_URL?.trim();
  return u ? u.replace(/\/$/, "") : "/product-api";
}

function authHeaders(): Record<string, string> {
  const token = getAccessToken();
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function productFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${productBase()}${path}`, {
    ...options,
    headers: { ...authHeaders(), ...(options.headers as Record<string, string> ?? {}) },
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(
      (data as { error?: { message?: string } })?.error?.message ??
        `Request failed (${res.status})`
    );
  }
  return data as T;
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
  currency: string;
  quantity: number;
  lowStockThreshold: number;
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
  images: ProductImage[];
  tagNames: string[];
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
  createdAt: string;
  subCategories: CategoryDto[];
};

export type BrandDto = {
  id: string;
  name: string;
  slug: string;
  description?: string;
  logoUrl?: string;
  isActive: boolean;
};

/* ── Products ── */

export function fetchProducts(params?: {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  categoryId?: string;
  brandId?: string;
}) {
  const q = new URLSearchParams();
  q.set("publicOnly", "false");
  if (params?.page) q.set("page", String(params.page));
  if (params?.pageSize) q.set("pageSize", String(params.pageSize));
  if (params?.search) q.set("search", params.search);
  if (params?.status) q.set("status", params.status);
  if (params?.categoryId) q.set("categoryId", params.categoryId);
  if (params?.brandId) q.set("brandId", params.brandId);
  return productFetch<{ data: PagedResult<ProductDto> }>(`/v1/products?${q}`);
}

export function fetchProductById(id: string) {
  return productFetch<{ data: ProductDto }>(`/v1/products/${id}?publicOnly=false`);
}

export function createProduct(body: Record<string, unknown>) {
  return productFetch<{ data: ProductDto }>("/v1/products", {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export function updateProduct(id: string, body: Record<string, unknown>) {
  return productFetch<{ data: ProductDto }>(`/v1/products/${id}`, {
    method: "PUT",
    body: JSON.stringify(body),
  });
}

export function publishProduct(id: string) {
  return productFetch<{ data: ProductDto }>(`/v1/products/${id}/publish`, { method: "POST" });
}

export function deleteProduct(id: string) {
  return productFetch<{ success: boolean }>(`/v1/products/${id}`, { method: "DELETE" });
}

/* ── Categories ── */

export function fetchCategories(includeInactive = true) {
  return productFetch<{ data: CategoryDto[] }>(`/v1/categories?includeInactive=${includeInactive}`);
}

export function createCategory(body: Record<string, unknown>) {
  return productFetch<{ data: CategoryDto }>("/v1/categories", {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export function updateCategory(id: string, body: Record<string, unknown>) {
  return productFetch<{ data: CategoryDto }>(`/v1/categories/${id}`, {
    method: "PUT",
    body: JSON.stringify(body),
  });
}

export function deleteCategory(id: string) {
  return productFetch<{ success: boolean }>(`/v1/categories/${id}`, { method: "DELETE" });
}

/* ── Brands ── */

export function fetchBrands(includeInactive = true) {
  return productFetch<{ data: BrandDto[] }>(`/v1/brands?includeInactive=${includeInactive}`);
}

export function createBrand(body: Record<string, unknown>) {
  return productFetch<{ data: BrandDto }>("/v1/brands", {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export function updateBrand(id: string, body: Record<string, unknown>) {
  return productFetch<{ data: BrandDto }>(`/v1/brands/${id}`, {
    method: "PUT",
    body: JSON.stringify(body),
  });
}

export function deleteBrand(id: string) {
  return productFetch<{ success: boolean }>(`/v1/brands/${id}`, { method: "DELETE" });
}
