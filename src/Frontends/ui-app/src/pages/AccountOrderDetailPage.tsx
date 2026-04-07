import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { fetchMyOrder } from "../api/orders";
import type { OrderConfirmation } from "../api/checkout";
import { isSessionExpiredError } from "../api/errors";
import { endSessionDueToUnauthorized } from "../auth/endSession";
import { OrderReceiptDetails } from "../components/OrderReceipt";
import "./CheckoutPage.css";

export function AccountOrderDetailPage() {
  const { orderId } = useParams<{ orderId: string }>();
  const navigate = useNavigate();
  const [order, setOrder] = useState<OrderConfirmation | null>(null);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!orderId) {
      setLoading(false);
      setErr("Missing order id.");
      return;
    }
    let cancelled = false;
    setLoading(true);
    setErr("");
    fetchMyOrder(orderId)
      .then((o) => {
        if (!cancelled) setOrder(o);
      })
      .catch((e) => {
        if (cancelled) return;
        if (isSessionExpiredError(e)) {
          endSessionDueToUnauthorized();
          navigate("/login", { replace: true });
          return;
        }
        setErr(e instanceof Error ? e.message : "Could not load order");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [orderId, navigate]);

  if (loading) {
    return (
      <div className="checkout-page">
        <p className="muted">Loading order…</p>
      </div>
    );
  }

  if (err || !order) {
    return (
      <div className="checkout-page">
        <h1 className="checkout-page__title">Order</h1>
        <p className="checkout-page__err" role="alert">
          {err || "Order not found."}
        </p>
        <Link to="/account/orders">← Order history</Link>
      </div>
    );
  }

  const created = new Date(order.createdAt);

  return (
    <div className="checkout-page">
      <h1 className="checkout-page__title">Order details</h1>
      <p className="checkout-page__lead">
        <strong>{order.orderNumber}</strong> · {created.toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" })}
      </p>

      <div className="checkout-page__grid" style={{ marginTop: "1rem" }}>
        <OrderReceiptDetails order={order} />
        <aside className="checkout-page__summary">
          <h3>Actions</h3>
          <Link to="/account/orders" className="checkout-page__back" style={{ display: "block", marginTop: "0.5rem" }}>
            ← All orders
          </Link>
          <button
            type="button"
            className="checkout-page__submit"
            style={{ width: "100%", marginTop: "1rem" }}
            onClick={() => navigate("/")}
          >
            Continue shopping
          </button>
        </aside>
      </div>
    </div>
  );
}
