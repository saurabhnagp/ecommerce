import { useCallback, useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  fetchWishlist,
  removeWishlistItem,
  type WishlistItemDto,
} from "../api/wishlist";
import { isSessionExpiredError } from "../api/errors";
import { endSessionDueToUnauthorized } from "../auth/endSession";
import { getAccessToken } from "../auth/storage";
import "./WishlistPage.css";

const PLACEHOLDER =
  "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=120&h=120&fit=crop";

export function WishlistPage() {
  const navigate = useNavigate();
  const token = getAccessToken();
  const [items, setItems] = useState<WishlistItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [removingId, setRemovingId] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!token) {
      setLoading(false);
      return;
    }
    setErr("");
    setLoading(true);
    try {
      const res = await fetchWishlist(token);
      setItems(res.data ?? []);
    } catch (e) {
      if (isSessionExpiredError(e)) {
        endSessionDueToUnauthorized();
        navigate("/", { replace: true });
        return;
      }
      setErr(e instanceof Error ? e.message : "Failed to load wishlist");
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, [token, navigate]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleRemove(productId: string) {
    if (!token) return;
    setRemovingId(productId);
    setErr("");
    try {
      await removeWishlistItem(productId, token);
      setItems((prev) => prev.filter((i) => i.productId !== productId));
    } catch (e) {
      if (isSessionExpiredError(e)) {
        endSessionDueToUnauthorized();
        navigate("/", { replace: true });
        return;
      }
      setErr(e instanceof Error ? e.message : "Could not remove item");
    } finally {
      setRemovingId(null);
    }
  }

  if (loading) return <p className="muted wishlist-page__state">Loading wishlist…</p>;

  return (
    <div className="wishlist-page auth-card">
      <h1 className="auth-title">Your wishlist</h1>
      <p className="wishlist-page__hint muted">
        Saved products from the catalog.{" "}
        <Link to="/products">Continue shopping</Link>
      </p>

      {err && <p className="auth-error wishlist-page__err">{err}</p>}

      {items.length === 0 ? (
        <p className="muted wishlist-page__state">No items yet.</p>
      ) : (
        <ul className="wishlist-page__list">
          {items.map((row) => {
            const img = row.primaryImageUrl?.trim() || PLACEHOLDER;
            const name = row.productLoaded ? row.productName ?? "Product" : "Unavailable product";
            const price =
              row.price != null
                ? `₹ ${row.price.toLocaleString("en-IN", { minimumFractionDigits: 2 })}`
                : "—";
            return (
              <li key={row.id} className="wishlist-page__row">
                <img className="wishlist-page__thumb" src={img} alt="" />
                <div className="wishlist-page__meta">
                  <div className="wishlist-page__name">{name}</div>
                  {!row.productLoaded && (
                    <span className="wishlist-page__badge">Removed from catalog</span>
                  )}
                  <div className="wishlist-page__price muted">{price}</div>
                </div>
                <button
                  type="button"
                  className="wishlist-page__remove"
                  disabled={removingId === row.productId}
                  onClick={() => handleRemove(row.productId)}
                >
                  {removingId === row.productId ? "…" : "Remove"}
                </button>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
