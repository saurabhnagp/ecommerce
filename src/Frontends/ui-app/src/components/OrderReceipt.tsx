import type { OrderConfirmation } from "../api/checkout";
import "../pages/CheckoutPage.css";

export function money(amount: number, currency: string) {
  const sym = currency === "USD" ? "$" : "₹";
  return `${sym} ${amount.toLocaleString(currency === "USD" ? "en-US" : "en-IN", {
    minimumFractionDigits: 2,
  })}`;
}

export function paymentLabel(method: string) {
  switch (method) {
    case "DirectBankTransfer":
      return "Direct Bank Transfer";
    case "Cheque":
      return "Cheque";
    case "PayPal":
      return "PayPal";
    default:
      return method;
  }
}

function formatAddr(a: OrderConfirmation["billing"]) {
  const lines = [
    `${a.firstName} ${a.lastName}`.trim(),
    a.company,
    a.street,
    a.apartment,
    `${a.city}, ${a.state} ${a.zip}`.trim(),
    a.country,
    `Phone: ${a.phone}`,
    a.email,
  ].filter(Boolean);
  return lines;
}

export function OrderReceiptDetails({ order }: { order: OrderConfirmation }) {
  const c = order.currency ?? "INR";
  const hasShipmentInfo =
    order.shippedAt ||
    (order.carrier && order.carrier.trim()) ||
    (order.trackingNumber && order.trackingNumber.trim());

  return (
    <div>
      <h2 className="checkout-page__section-title">Items</h2>
      <ul style={{ listStyle: "none", padding: 0, margin: 0 }}>
        {order.items.map((line) => (
          <li
            key={`${line.productId}-${line.quantity}`}
            style={{
              display: "flex",
              justifyContent: "space-between",
              padding: "0.5rem 0",
              borderBottom: "1px solid #eee",
              fontSize: "0.875rem",
            }}
          >
            <span>
              {line.productName} × {line.quantity}
            </span>
            <span>{money(line.lineTotal, line.currency || c)}</span>
          </li>
        ))}
      </ul>

      <h2 className="checkout-page__section-title" style={{ marginTop: "1.5rem" }}>
        Totals
      </h2>
      <div className="checkout-page__summary" style={{ background: "transparent", border: "none", padding: 0 }}>
        <div className="checkout-page__summary-row">
          <span>Subtotal</span>
          <span>{money(order.subtotal, c)}</span>
        </div>
        {order.discountAmount > 0 && (
          <div className="checkout-page__summary-row">
            <span>Discount{order.couponCode ? ` (${order.couponCode})` : ""}</span>
            <span>− {money(order.discountAmount, c)}</span>
          </div>
        )}
        <div className="checkout-page__summary-row">
          <span>Shipping</span>
          <span>{money(order.shippingAmount, c)}</span>
        </div>
        <div className="checkout-page__summary-row checkout-page__summary-row--total">
          <span>Total</span>
          <span>{money(order.total, c)}</span>
        </div>
        <p style={{ marginTop: "1rem", fontSize: "0.875rem", color: "#555" }}>
          Payment: <strong>{paymentLabel(order.paymentMethod)}</strong>
        </p>
        {(order.status || hasShipmentInfo) && (
          <div style={{ marginTop: "1.25rem", fontSize: "0.875rem", color: "#333" }}>
            <h3 style={{ fontSize: "0.95rem", margin: "0 0 0.5rem" }}>Order &amp; shipment</h3>
            {order.status && (
              <p style={{ margin: "0.25rem 0" }}>
                Status: <strong>{order.status}</strong>
              </p>
            )}
            {order.shippedAt && (
              <p style={{ margin: "0.25rem 0" }}>
                Shipped: {new Date(order.shippedAt).toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" })}
              </p>
            )}
            {order.carrier?.trim() && (
              <p style={{ margin: "0.25rem 0" }}>
                Carrier: {order.carrier}
              </p>
            )}
            {order.trackingNumber?.trim() && (
              <p style={{ margin: "0.25rem 0" }}>
                Tracking: {order.trackingNumber}
              </p>
            )}
          </div>
        )}
      </div>

      <h2 className="checkout-page__section-title" style={{ marginTop: "1.5rem" }}>
        Billing address
      </h2>
      <address style={{ fontStyle: "normal", fontSize: "0.875rem", lineHeight: 1.6 }}>
        {formatAddr(order.billing).map((line, i) => (
          <div key={`b-${i}`}>{line}</div>
        ))}
      </address>

      <h2 className="checkout-page__section-title" style={{ marginTop: "1.5rem" }}>
        Shipping address
      </h2>
      <address style={{ fontStyle: "normal", fontSize: "0.875rem", lineHeight: 1.6 }}>
        {formatAddr(order.shipping).map((line, i) => (
          <div key={`s-${i}`}>{line}</div>
        ))}
      </address>
    </div>
  );
}
