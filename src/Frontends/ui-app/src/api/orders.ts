import { SessionExpiredError } from "./errors";
import { getAccessToken } from "../auth/storage";
import type { OrderConfirmation } from "./checkout";

/** Same host as cart/checkout — ProductService. Do not use the user-service BFF here: that path requires UserService→ProductService HTTP from the server, which often fails in local dev while /product-api works. */
function productBase() {
  return (import.meta.env.VITE_PRODUCT_SERVICE_URL?.trim() || "/product-api").replace(/\/$/, "");
}

export type OrderHistoryItem = {
  orderId: string;
  orderNumber: string;
  createdAt: string;
  status: string;
  shippedAt?: string | null;
  total: number;
  currency: string;
  previewProductName?: string | null;
  previewImageUrl?: string | null;
  lineItemCount: number;
};

export type PagedOrders = {
  items: OrderHistoryItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

function authHeaders(): HeadersInit {
  const token = getAccessToken();
  if (!token) throw new Error("Sign in to view orders.");
  return {
    Authorization: `Bearer ${token}`,
    Accept: "application/json",
  };
}

export async function fetchMyOrders(page = 1, pageSize = 20): Promise<PagedOrders> {
  const res = await fetch(
    `${productBase()}/v1/orders?page=${page}&pageSize=${pageSize}`,
    { headers: authHeaders() }
  );
  if (res.status === 401) throw new SessionExpiredError();
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(
      (data as { error?: { message?: string } })?.error?.message ?? `Orders request failed (${res.status})`
    );
  }
  const body = data as { success?: boolean; data?: PagedOrders };
  if (!body.success || !body.data) throw new Error("Invalid orders response");
  return body.data;
}

export async function fetchMyOrder(orderId: string): Promise<OrderConfirmation> {
  const res = await fetch(`${productBase()}/v1/orders/${encodeURIComponent(orderId)}`, {
    headers: authHeaders(),
  });
  if (res.status === 401) throw new SessionExpiredError();
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(
      (data as { error?: { message?: string } })?.error?.message ?? `Order request failed (${res.status})`
    );
  }
  const body = data as { success?: boolean; data?: OrderConfirmation };
  if (!body.success || !body.data) throw new Error("Invalid order response");
  return body.data;
}
