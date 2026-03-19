import { useState } from "react";
import { Link } from "react-router-dom";
import { forgotPassword } from "../api/auth";
import "./auth-pages.css";

export function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setMessage(null);
    setLoading(true);
    try {
      const msg = await forgotPassword(email.trim());
      setMessage(msg);
    } catch {
      setError("Something went wrong. Try again.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-page-header">
          <span>Forgot password</span>
          <Link to="/login" aria-label="Back to sign in">
            ×
          </Link>
        </div>
        <p className="auth-sub">Enter your email and we&apos;ll send reset instructions if an account exists.</p>

        <form onSubmit={handleSubmit} className="auth-form">
          {error && <p className="form-error">{error}</p>}
          {message && <p className="form-success">{message}</p>}
          <label className="field-wrap">
            <input
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
            />
            <span className="field-icon" aria-hidden>
              ✉
            </span>
          </label>
          <button type="submit" className="btn-login" disabled={loading}>
            {loading ? "…" : "SEND RESET LINK"}
          </button>
        </form>

        <p className="auth-footer">
          <Link to="/login">Back to sign in</Link>
        </p>
      </div>
    </div>
  );
}
