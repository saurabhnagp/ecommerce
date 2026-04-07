import { Link } from "react-router-dom";

export function CheckoutPlaceholderPage() {
  return (
    <div className="auth-card" style={{ maxWidth: 560, margin: "1rem auto" }}>
      <h1 className="auth-title">Checkout</h1>
      <p className="muted">
        Billing, shipping, and payment will be implemented with the checkout and order modules.
      </p>
      <p>
        <Link to="/cart">← Back to cart</Link>
      </p>
    </div>
  );
}
