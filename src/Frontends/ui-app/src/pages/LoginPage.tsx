import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { mergeCart } from "../api/cart";
import { login } from "../api/auth";
import { clearCartSessionId, getOrCreateCartSessionId } from "../cart/cartSession";
import { notifyAuthChange } from "../auth/notify";
import { saveSession } from "../auth/storage";
import { PasswordInput } from "../components/PasswordInput";
import "./auth-pages.css";

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const resetOk = (location.state as { resetOk?: boolean } | null)?.resetOk;
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const guestSessionId = getOrCreateCartSessionId();
      const { user, tokens } = await login(email.trim(), password, rememberMe);
      saveSession(tokens, user);
      try {
        await mergeCart(tokens.accessToken, guestSessionId);
      } catch {
        /* empty or invalid guest cart is fine */
      }
      clearCartSessionId();
      notifyAuthChange();
      navigate("/", { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-page-header">
          <span>Sign in</span>
          <Link to="/" aria-label="Home">
            ×
          </Link>
        </div>

        <div className="signin-using">
          <div className="signin-using-line" />
          <span>Sign in using</span>
          <div className="signin-using-line" />
        </div>
        <div className="social-row">
          <button type="button" className="btn-social btn-facebook" disabled title="Coming soon">
            <span>f</span> FACEBOOK
          </button>
          <button type="button" className="btn-social btn-twitter" disabled title="Coming soon">
            𝕏 TWITTER
          </button>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          {resetOk && <p className="form-success">Password reset. Sign in with your new password.</p>}
          {error && <p className="form-error">{error}</p>}
          <label className="field-wrap">
            <input
              type="email"
              placeholder="Email / Login"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
            />
            <span className="field-icon" aria-hidden>
              ✉
            </span>
          </label>
          <PasswordInput
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
          />
          <label className="remember">
            <input
              type="checkbox"
              checked={rememberMe}
              onChange={(e) => setRememberMe(e.target.checked)}
            />
            remember me
          </label>
          <button type="submit" className="btn-login" disabled={loading}>
            {loading ? "…" : "LOGIN"}
          </button>
        </form>

        <Link to="/forgot-password" className="link-auth">
          Forget your password?
        </Link>

        <hr className="auth-divider" />
        <Link to="/register" className="btn-register-footer">
          REGISTER
        </Link>
      </div>
    </div>
  );
}
