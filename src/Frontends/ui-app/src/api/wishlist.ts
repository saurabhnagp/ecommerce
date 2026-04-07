import { getAccessToken } from "../auth/storage";

function base() {
  return (import.meta.env.VITE_PRODUCT_SERVICE_URL?.trim() || "/product-api").replace(
    /\/$/,
    ""
  );
}

async function authFetch<T>(
  path: string,
  options: RequestInit & { token?: string | null } = {}
): Promise<T> {
  const token = options.token ?? getAccessToken();
  const { token: _t, ...init } = options;
  const res = await fetch(`${base()}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init.headers as Record<string, string>),
    },
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

export type WishlistItemDto = {
  id: string;
  productId: string;
  addedAt: string;
  productName?: string | null;
  productSlug?: string | null;
  price?: number | null;
  currency?: string | null;
  primaryImageUrl?: string | null;
  productStatus?: string | null;
  productLoaded: boolean;
};

export async function fetchWishlist(token?: string | null) {
  return authFetch<{ success: boolean; data: WishlistItemDto[] }>("/v1/wishlist", {
    method: "GET",
    token,
  });
}

export async function fetchWishlistProductIds(token?: string | null) {
  return authFetch<{ success: boolean; data: string[] }>("/v1/wishlist/product-ids", {
    method: "GET",
    token,
  });
}

export async function addWishlistItem(productId: string, token?: string | null) {
  return authFetch<{ success: boolean; message?: string }>("/v1/wishlist/items", {
    method: "POST",
    body: JSON.stringify({ productId }),
    token,
  });
}

export async function removeWishlistItem(productId: string, token?: string | null) {
  return authFetch<{ success: boolean; message?: string }>(
    `/v1/wishlist/items/${encodeURIComponent(productId)}`,
    { method: "DELETE", token }
  );
}
