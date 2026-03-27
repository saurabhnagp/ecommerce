import { getAccessToken } from "../auth/storage";
import { getOrCreateCartSessionId } from "../cart/cartSession";

function base() {
  return (import.meta.env.VITE_PRODUCT_SERVICE_URL?.trim() || "/product-api").replace(
    /\/$/,
    ""
  );
}

function checkoutHeaders(): HeadersInit {
  const h: Record<string, string> = { "Content-Type": "application/json" };
  const token = getAccessToken();
  if (token) h.Authorization = `Bearer ${token}`;
  else h["X-Cart-Session-Id"] = getOrCreateCartSessionId();
  return h;
}

export type CheckoutAddress = {
  firstName: string;
  lastName: string;
  company?: string | null;
  country: string;
  street: string;
  apartment: string;
  city: string;
  state: string;
  zip: string;
  phone: string;
  email: string;
};

export type PlaceOrderPayload = {
  billing: CheckoutAddress;
  sameAsBilling: boolean;
  shipping?: CheckoutAddress | null;
  paymentMethod: string;
};

export type OrderLineConfirmation = {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  currency: string;
};

export type OrderConfirmation = {
  orderId: string;
  orderNumber: string;
  createdAt: string;
  paymentMethod: string;
  currency: string;
  subtotal: number;
  discountAmount: number;
  shippingAmount: number;
  total: number;
  couponCode?: string | null;
  status?: string;
  shippedAt?: string | null;
  carrier?: string | null;
  trackingNumber?: string | null;
  billing: CheckoutAddress;
  shipping: CheckoutAddress;
  items: OrderLineConfirmation[];
};

export async function placeOrder(payload: PlaceOrderPayload): Promise<OrderConfirmation> {
  const res = await fetch(`${base()}/v1/checkout/orders`, {
    method: "POST",
    headers: checkoutHeaders(),
    body: JSON.stringify(payload),
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error(
      (data as { error?: { message?: string } })?.error?.message ??
        `Checkout failed (${res.status})`
    );
  }
  const body = data as { success?: boolean; data?: OrderConfirmation };
  if (!body.success || !body.data) throw new Error("Invalid checkout response");
  return body.data;
}
