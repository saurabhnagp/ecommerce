import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { placeOrder, type CheckoutAddress } from "../api/checkout";
import { fetchCurrentUser } from "../api/user";
import { getAccessToken } from "../auth/storage";
import { useCart } from "../cart/CartContext";
import "./CheckoutPage.css";

function emptyAddress(): CheckoutAddress {
  return {
    firstName: "",
    lastName: "",
    company: "",
    country: "",
    street: "",
    apartment: "",
    city: "",
    state: "",
    zip: "",
    phone: "",
    email: "",
  };
}

function money(amount: number, currency: string) {
  const sym = currency === "USD" ? "$" : "₹";
  return `${sym} ${amount.toLocaleString(currency === "USD" ? "en-US" : "en-IN", {
    minimumFractionDigits: 2,
  })}`;
}

function AddressFields({
  title,
  prefix,
  value,
  onChange,
}: {
  title: string;
  prefix: string;
  value: CheckoutAddress;
  onChange: (next: CheckoutAddress) => void;
}) {
  function set<K extends keyof CheckoutAddress>(key: K, v: CheckoutAddress[K]) {
    onChange({ ...value, [key]: v });
  }

  return (
    <fieldset className="checkout-page__fieldset">
      <legend className="checkout-page__section-title">{title}</legend>
      <div className="checkout-page__fields">
        <div className="checkout-page__row2">
          <div className="checkout-page__field">
            <label htmlFor={`${prefix}-fn`}>First name *</label>
            <input
              id={`${prefix}-fn`}
              value={value.firstName}
              onChange={(e) => set("firstName", e.target.value)}
              autoComplete="given-name"
            />
          </div>
          <div className="checkout-page__field">
            <label htmlFor={`${prefix}-ln`}>Last name *</label>
            <input
              id={`${prefix}-ln`}
              value={value.lastName}
              onChange={(e) => set("lastName", e.target.value)}
              autoComplete="family-name"
            />
          </div>
        </div>
        <div className="checkout-page__field">
          <label htmlFor={`${prefix}-co`}>Company (optional)</label>
          <input
            id={`${prefix}-co`}
            value={value.company ?? ""}
            onChange={(e) => set("company", e.target.value)}
            autoComplete="organization"
          />
        </div>
        <div className="checkout-page__field">
          <label htmlFor={`${prefix}-ct`}>Country *</label>
          <input
            id={`${prefix}-ct`}
            value={value.country}
            onChange={(e) => set("country", e.target.value)}
            autoComplete="country-name"
          />
        </div>
        <div className="checkout-page__field">
          <label htmlFor={`${prefix}-st`}>Street address *</label>
          <input
            id={`${prefix}-st`}
            value={value.street}
            onChange={(e) => set("street", e.target.value)}
            autoComplete="street-address"
          />
        </div>
        <div className="checkout-page__field">
          <label htmlFor={`${prefix}-apt`}>Apartment, suite, etc. *</label>
          <input
            id={`${prefix}-apt`}
            value={value.apartment}
            onChange={(e) => set("apartment", e.target.value)}
            autoComplete="address-line2"
          />
        </div>
        <div className="checkout-page__row2">
          <div className="checkout-page__field">
            <label htmlFor={`${prefix}-city`}>City *</label>
            <input
              id={`${prefix}-city`}
              value={value.city}
              onChange={(e) => set("city", e.target.value)}
              autoComplete="address-level2"
            />
          </div>
          <div className="checkout-page__field">
            <label htmlFor={`${prefix}-state`}>State / province *</label>
            <input
              id={`${prefix}-state`}
              value={value.state}
              onChange={(e) => set("state", e.target.value)}
              autoComplete="address-level1"
            />
          </div>
        </div>
        <div className="checkout-page__row2">
          <div className="checkout-page__field">
            <label htmlFor={`${prefix}-zip`}>ZIP / postal code *</label>
            <input
              id={`${prefix}-zip`}
              value={value.zip}
              onChange={(e) => set("zip", e.target.value)}
              autoComplete="postal-code"
            />
          </div>
          <div className="checkout-page__field">
            <label htmlFor={`${prefix}-ph`}>Phone *</label>
            <input
              id={`${prefix}-ph`}
              type="tel"
              value={value.phone}
              onChange={(e) => set("phone", e.target.value)}
              autoComplete="tel"
            />
          </div>
        </div>
        <div className="checkout-page__field">
          <label htmlFor={`${prefix}-em`}>Email *</label>
          <input
            id={`${prefix}-em`}
            type="email"
            value={value.email}
            onChange={(e) => set("email", e.target.value)}
            autoComplete="email"
          />
        </div>
      </div>
    </fieldset>
  );
}

export function CheckoutPage() {
  const navigate = useNavigate();
  const { cart, loading, refreshCart } = useCart();
  const [billing, setBilling] = useState<CheckoutAddress>(() => emptyAddress());
  const [shipping, setShipping] = useState<CheckoutAddress>(() => emptyAddress());
  const [sameAsBilling, setSameAsBilling] = useState(true);
  const [paymentMethod, setPaymentMethod] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!loading && cart && cart.items.length === 0) {
      navigate("/cart", { replace: true });
    }
  }, [loading, cart, navigate]);

  useEffect(() => {
    const t = getAccessToken();
    if (!t) return;
    let cancelled = false;
    fetchCurrentUser(t)
      .then((u) => {
        if (cancelled) return;
        setBilling((b) => ({
          ...b,
          email: u.email ?? b.email,
          firstName: u.firstName || b.firstName,
          lastName: u.lastName || b.lastName,
          phone: u.phone?.trim() || b.phone,
        }));
      })
      .catch(() => {});
    return () => {
      cancelled = true;
    };
  }, []);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setSubmitting(true);
    try {
      const order = await placeOrder({
        billing,
        sameAsBilling,
        shipping: sameAsBilling ? null : shipping,
        paymentMethod,
      });
      await refreshCart();
      navigate("/order-confirmation", { replace: true, state: { order } });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Checkout failed");
    } finally {
      setSubmitting(false);
    }
  }

  if (loading || !cart) {
    return <p className="muted checkout-page">Loading…</p>;
  }

  if (cart.items.length === 0) {
    return null;
  }

  const currency = cart.currency ?? "INR";
  const hasUnavailable = cart.items.some((l) => !l.productAvailable);

  return (
    <div className="checkout-page">
      <h1 className="checkout-page__title">Checkout</h1>
      <p className="checkout-page__lead muted">
        Enter billing and shipping details, choose a payment method, and place your order.
      </p>

      {error && (
        <p className="checkout-page__err" role="alert">
          {error}
        </p>
      )}

      {hasUnavailable && (
        <p className="checkout-page__err" role="alert">
          Some items are no longer available.{" "}
          <Link to="/cart">Return to cart</Link> to update your order.
        </p>
      )}

      <form className="checkout-page__grid" onSubmit={handleSubmit}>
        <div>
          <AddressFields title="Billing address" prefix="bill" value={billing} onChange={setBilling} />

          <label className="checkout-page__same">
            <input
              type="checkbox"
              checked={sameAsBilling}
              onChange={(e) => setSameAsBilling(e.target.checked)}
            />
            Same as billing address for shipping
          </label>

          {!sameAsBilling && (
            <AddressFields title="Shipping address" prefix="ship" value={shipping} onChange={setShipping} />
          )}

          <h2 className="checkout-page__section-title">Payment method</h2>
          <div className="checkout-page__payments">
            <label className="checkout-page__pay-option">
              <input
                type="radio"
                name="pay"
                value="DirectBankTransfer"
                checked={paymentMethod === "DirectBankTransfer"}
                onChange={() => setPaymentMethod("DirectBankTransfer")}
              />
              Direct Bank Transfer
            </label>
            <label className="checkout-page__pay-option">
              <input
                type="radio"
                name="pay"
                value="Cheque"
                checked={paymentMethod === "Cheque"}
                onChange={() => setPaymentMethod("Cheque")}
              />
              Cheque
            </label>
            <label className="checkout-page__pay-option">
              <input
                type="radio"
                name="pay"
                value="PayPal"
                checked={paymentMethod === "PayPal"}
                onChange={() => setPaymentMethod("PayPal")}
              />
              PayPal
            </label>
          </div>

          <button
            type="submit"
            className="checkout-page__submit"
            disabled={submitting || hasUnavailable}
          >
            {submitting ? "Placing order…" : "Place order"}
          </button>

          <Link to="/cart" className="checkout-page__back">
            ← Back to cart
          </Link>
        </div>

        <aside className="checkout-page__summary">
          <h3>Order summary</h3>
          <div className="checkout-page__summary-row">
            <span>Subtotal</span>
            <span>{money(cart.subtotal, currency)}</span>
          </div>
          {cart.discountAmount > 0 && (
            <div className="checkout-page__summary-row">
              <span>Discount</span>
              <span>− {money(cart.discountAmount, currency)}</span>
            </div>
          )}
          <div className="checkout-page__summary-row">
            <span>Shipping</span>
            <span>{money(0, currency)}</span>
          </div>
          <div className="checkout-page__summary-row checkout-page__summary-row--total">
            <span>Total</span>
            <span>{money(cart.total, currency)}</span>
          </div>
        </aside>
      </form>
    </div>
  );
}
