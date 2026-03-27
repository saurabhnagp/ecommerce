import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { fetchMyOrders, type OrderHistoryItem } from "../api/orders";
import { isSessionExpiredError } from "../api/errors";
import { endSessionDueToUnauthorized } from "../auth/endSession";
import { money } from "../components/OrderReceipt";
import "./CheckoutPage.css";
import "./AccountOrdersPage.css";

const PLACEHOLDER =
  "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=120&h=120&fit=crop";

export function AccountOrdersPage() {
  const [data, setData] = useState<OrderHistoryItem[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setErr("");
    fetchMyOrders(page, 10)
      .then((res) => {
        if (!cancelled) {
          setData(res.items);
          setTotalPages(res.totalPages);
        }
      })
      .catch((e) => {
        if (cancelled) return;
        if (isSessionExpiredError(e)) {
          endSessionDueToUnauthorized();
          return;
        }
        setErr(e instanceof Error ? e.message : "Could not load orders");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [page]);

  return (
    <div className="account-orders">
      <h1 className="account-orders__title">Order history</h1>
      <p className="muted" style={{ margin: 0 }}>
        Signed-in purchases only. Guest orders are confirmed at checkout but not listed here.
      </p>

      {err && (
        <p className="checkout-page__err" style={{ marginTop: "1rem" }} role="alert">
          {err}
        </p>
      )}

      {loading ? (
        <p className="muted" style={{ marginTop: "1rem" }}>
          Loading…
        </p>
      ) : data.length === 0 && !err ? (
        <p className="muted" style={{ marginTop: "1rem" }}>
          No orders yet.{" "}
          <Link to="/products">Browse products</Link>
        </p>
      ) : (
        <>
          <ul className="account-orders__list">
            {data.map((o) => {
              const img = o.previewImageUrl?.trim() || PLACEHOLDER;
              const shipNote = o.shippedAt
                ? `Shipped ${new Date(o.shippedAt).toLocaleDateString()}`
                : `Placed ${new Date(o.createdAt).toLocaleDateString()}`;
              return (
                <li key={o.orderId}>
                  <Link to={`/account/orders/${o.orderId}`} className="account-orders__card">
                    <img className="account-orders__thumb" src={img} alt="" />
                    <div className="account-orders__meta">
                      <div className="account-orders__num">{o.orderNumber}</div>
                      <div className="account-orders__name">
                        {o.previewProductName ?? "Order items"}
                        {o.lineItemCount > 1 ? ` +${o.lineItemCount - 1} more` : ""}
                      </div>
                      <div className="account-orders__sub">
                        {o.status} · {shipNote}
                      </div>
                    </div>
                    <div className="account-orders__price">{money(o.total, o.currency)}</div>
                  </Link>
                </li>
              );
            })}
          </ul>
          {totalPages > 1 && (
            <div style={{ marginTop: "1.5rem", display: "flex", gap: "0.75rem", alignItems: "center" }}>
              <button
                type="button"
                className="checkout-page__submit"
                style={{ padding: "0.4rem 1rem" }}
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Previous
              </button>
              <span className="muted" style={{ fontSize: "0.85rem" }}>
                Page {page} of {totalPages}
              </span>
              <button
                type="button"
                className="checkout-page__submit"
                style={{ padding: "0.4rem 1rem" }}
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </button>
            </div>
          )}
        </>
      )}

      <p style={{ marginTop: "1.5rem" }}>
        <Link to="/account/profile">← Back to profile</Link>
      </p>
    </div>
  );
}
