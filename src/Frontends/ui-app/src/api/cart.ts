import { getAccessToken } from "../auth/storage";
import { getOrCreateCartSessionId } from "../cart/cartSession";

function base() {
  return (import.meta.env.VITE_PRODUCT_SERVICE_URL?.trim() || "/product-api").replace(
    /\/$/,
    ""
  );
}

function cartHeaders(): HeadersInit {
  const h: Record<string, string> = { "Content-Type": "application/json" };
  const token = getAccessToken();
  if (token) h.Authorization = `Bearer ${token}`;
  else h["X-Cart-Session-Id"] = getOrCreateCartSessionId();
  return h;
}

async function parseCartResponse(res: Response) {
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(
      (data as { error?: { message?: string } })?.error?.message ??
        `Request failed (${res.status})`
    );
  }
  return (data as { success: boolean; data: CartDto }).data;
}

export type CartLineDto = {
  cartItemId: string;
  productId: string;
  productName?: string | null;
  productSlug?: string | null;
  primaryImageUrl?: string | null;
  quantity: number;
  unitPrice: number;
  compareAtPrice?: number | null;
  lineSubtotal: number;
  currency: string;
  productAvailable: boolean;
};

export type CartDto = {
  cartId: string;
  items: CartLineDto[];
  subtotal: number;
  couponCode?: string | null;
  discountAmount: number;
  total: number;
  currency: string;
};

export async function fetchCart(): Promise<CartDto> {
  const res = await fetch(`${base()}/v1/cart`, { headers: cartHeaders() });
  return parseCartResponse(res);
}

export async function addCartItem(productId: string, quantity = 1): Promise<CartDto> {
  const res = await fetch(`${base()}/v1/cart/items`, {
    method: "POST",
    headers: cartHeaders(),
    body: JSON.stringify({ productId, quantity }),
  });
  return parseCartResponse(res);
}

export async function updateCartItemQuantity(
  productId: string,
  quantity: number
): Promise<CartDto> {
  const res = await fetch(`${base()}/v1/cart/items/${encodeURIComponent(productId)}`, {
    method: "PUT",
    headers: cartHeaders(),
    body: JSON.stringify({ quantity }),
  });
  return parseCartResponse(res);
}

export async function removeCartItem(productId: string): Promise<CartDto> {
  const res = await fetch(`${base()}/v1/cart/items/${encodeURIComponent(productId)}`, {
    method: "DELETE",
    headers: cartHeaders(),
  });
  return parseCartResponse(res);
}

export async function applyCartCoupon(code: string): Promise<CartDto> {
  const res = await fetch(`${base()}/v1/cart/coupon`, {
    method: "POST",
    headers: cartHeaders(),
    body: JSON.stringify({ code }),
  });
  return parseCartResponse(res);
}

export async function removeCartCoupon(): Promise<CartDto> {
  const res = await fetch(`${base()}/v1/cart/coupon`, {
    method: "DELETE",
    headers: cartHeaders(),
  });
  return parseCartResponse(res);
}

export async function mergeCart(accessToken: string, sessionId: string): Promise<CartDto> {
  const res = await fetch(`${base()}/v1/cart/merge`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({ sessionId }),
  });
  return parseCartResponse(res);
}
