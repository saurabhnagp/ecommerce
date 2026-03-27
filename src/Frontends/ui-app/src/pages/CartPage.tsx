import { useState } from "react";
import { Link } from "react-router-dom";
import {
  applyCartCoupon,
  removeCartCoupon,
  removeCartItem,
  updateCartItemQuantity,
} from "../api/cart";
import { useCart } from "../cart/CartContext";
import "./CartPage.css";

const PLACEHOLDER =
  "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=120&h=120&fit=crop";

function money(amount: number, currency: string) {
  const sym = currency === "USD" ? "$" : "₹";
  return `${sym} ${amount.toLocaleString(currency === "USD" ? "en-US" : "en-IN", {
    minimumFractionDigits: 2,
  })}`;
}

export function CartPage() {
  const { cart, loading, refreshCart } = useCart();
  const [couponInput, setCouponInput] = useState("");
  const [couponErr, setCouponErr] = useState("");
  const [busy, setBusy] = useState<string | null>(null);

  async function handleQty(productId: string, next: number) {
    if (next < 1) return;
    setBusy(productId);
    setCouponErr("");
    try {
      await updateCartItemQuantity(productId, next);
      await refreshCart();
    } catch (e) {
      setCouponErr(e instanceof Error ? e.message : "Update failed");
    } finally {
      setBusy(null);
    }
  }

  async function handleRemove(productId: string) {
    setBusy(productId);
    setCouponErr("");
    try {
      await removeCartItem(productId);
      await refreshCart();
    } catch (e) {
      setCouponErr(e instanceof Error ? e.message : "Remove failed");
    } finally {
      setBusy(null);
    }
  }

  async function handleApplyCoupon(e: React.FormEvent) {
    e.preventDefault();
    const code = couponInput.trim();
    if (!code) return;
    setBusy("coupon");
    setCouponErr("");
    try {
      await applyCartCoupon(code);
      setCouponInput("");
      await refreshCart();
    } catch (err) {
      setCouponErr(err instanceof Error ? err.message : "Invalid coupon");
    } finally {
      setBusy(null);
    }
  }

  async function handleRemoveCoupon() {
    setBusy("coupon");
    setCouponErr("");
    try {
      await removeCartCoupon();
      await refreshCart();
    } catch (e) {
      setCouponErr(e instanceof Error ? e.message : "Could not remove coupon");
    } finally {
      setBusy(null);
    }
  }

  if (loading && !cart) {
    return <p className="muted cart-page__state">Loading cart…</p>;
  }

  const c = cart;
  const currency = c?.currency ?? "INR";
  const items = c?.items ?? [];

  return (
    <div className="cart-page">
      <h1 className="cart-page__title">Shopping cart</h1>

      {couponErr && <p className="cart-page__err">{couponErr}</p>}

      {items.length === 0 ? (
        <p className="muted cart-page__state">
          Your cart is empty.{" "}
          <Link to="/products">Browse products</Link>
        </p>
      ) : !cart ? null : (
        <div className="cart-page__layout">
          <ul className="cart-page__lines">
            {items.map((line) => {
              const img = line.primaryImageUrl?.trim() || PLACEHOLDER;
              const name = line.productAvailable
                ? line.productName ?? "Product"
                : "Unavailable";
              const disabled = busy === line.productId;
              return (
                <li key={line.cartItemId} className="cart-page__line">
                  <img className="cart-page__thumb" src={img} alt="" />
                  <div className="cart-page__line-meta">
                    <div className="cart-page__line-name">{name}</div>
                    {!line.productAvailable && (
                      <span className="cart-page__badge">No longer available</span>
                    )}
                    <div className="cart-page__line-price muted">
                      {money(line.unitPrice, currency)} each
                    </div>
                  </div>
                  <div className="cart-page__qty">
                    <button
                      type="button"
                      aria-label="Decrease quantity"
                      disabled={disabled || line.quantity <= 1}
                      onClick={() => handleQty(line.productId, line.quantity - 1)}
                    >
                      −
                    </button>
                    <span>{line.quantity}</span>
                    <button
                      type="button"
                      aria-label="Increase quantity"
                      disabled={disabled}
                      onClick={() => handleQty(line.productId, line.quantity + 1)}
                    >
                      +
                    </button>
                  </div>
                  <div className="cart-page__line-sub">
                    {money(line.lineSubtotal, currency)}
                  </div>
                  <button
                    type="button"
                    className="cart-page__remove"
                    disabled={disabled}
                    onClick={() => handleRemove(line.productId)}
                  >
                    Remove
                  </button>
                </li>
              );
            })}
          </ul>

          <aside className="cart-page__summary">
            <div className="cart-page__row">
              <span>Subtotal</span>
              <span>{money(cart.subtotal, currency)}</span>
            </div>
            {cart.discountAmount > 0 && (
              <div className="cart-page__row cart-page__row--discount">
                <span>Discount{cart.couponCode ? ` (${cart.couponCode})` : ""}</span>
                <span>− {money(cart.discountAmount, currency)}</span>
              </div>
            )}
            <div className="cart-page__row cart-page__row--total">
              <span>Total</span>
              <span>{money(cart.total, currency)}</span>
            </div>

            <form className="cart-page__coupon" onSubmit={handleApplyCoupon}>
              <label className="cart-page__coupon-label" htmlFor="coupon-code">
                Coupon code
              </label>
              <div className="cart-page__coupon-row">
                <input
                  id="coupon-code"
                  value={couponInput}
                  onChange={(e) => setCouponInput(e.target.value)}
                  placeholder="e.g. WELCOME10"
                  disabled={busy === "coupon"}
                />
                <button type="submit" disabled={busy === "coupon"}>
                  Apply
                </button>
              </div>
              {cart.couponCode && (
                <button
                  type="button"
                  className="cart-page__remove-coupon"
                  disabled={busy === "coupon"}
                  onClick={handleRemoveCoupon}
                >
                  Remove coupon
                </button>
              )}
            </form>

            <p className="cart-page__hint muted">
              Try <strong>WELCOME10</strong> (10% off) or <strong>FLAT50</strong> (₹50 off orders
              ≥ ₹200).
            </p>

            <Link to="/checkout" className="cart-page__checkout">
              Proceed to checkout
            </Link>
          </aside>
        </div>
      )}
    </div>
  );
}
