import { Link, useLocation, useNavigate } from "react-router-dom";
import type { OrderConfirmation } from "../api/checkout";
import { getAccessToken } from "../auth/storage";
import { OrderReceiptDetails } from "../components/OrderReceipt";
import "./CheckoutPage.css";

export function OrderConfirmationPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const order = (location.state as { order?: OrderConfirmation } | null)?.order;
  const signedIn = !!getAccessToken();

  if (!order) {
    return (
      <div className="checkout-page">
        <h1 className="checkout-page__title">Order confirmation</h1>
        <p className="muted">No order details to show. If you just completed a purchase, this page may have refreshed.</p>
        <Link to="/">Continue shopping</Link>
      </div>
    );
  }

  const created = new Date(order.createdAt);

  return (
    <div className="checkout-page">
      <h1 className="checkout-page__title">Thank you for your order</h1>
      <p className="checkout-page__lead">
        Order <strong>{order.orderNumber}</strong> was placed on{" "}
        {created.toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" })}.
      </p>

      <div className="checkout-page__grid" style={{ marginTop: "1.5rem" }}>
        <OrderReceiptDetails order={order} />

        <aside className="checkout-page__summary">
          <h3>What&apos;s next?</h3>
          <p style={{ fontSize: "0.85rem", color: "#555", margin: "0 0 1rem" }}>
            A confirmation email will be sent when the email service is connected.
          </p>
          {signedIn && (
            <p style={{ fontSize: "0.85rem", margin: "0 0 1rem" }}>
              <Link to="/account/orders">View order history</Link>
            </p>
          )}
          <button
            type="button"
            className="checkout-page__submit"
            style={{ width: "100%" }}
            onClick={() => navigate("/")}
          >
            Continue shopping
          </button>
        </aside>
      </div>
    </div>
  );
}
