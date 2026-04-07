import { useState } from "react";
import { Link, useSearchParams, useNavigate } from "react-router-dom";
import { resetPassword } from "../api/auth";
import { PasswordInput } from "../components/PasswordInput";
import "./auth-pages.css";

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get("token") ?? "";

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }
    if (!token) {
      setError("Invalid or missing reset link.");
      return;
    }
    setLoading(true);
    try {
      await resetPassword(token, password, confirmPassword);
      navigate("/login", { replace: true, state: { resetOk: true } });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Reset failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-page-header">
          <span>Set new password</span>
          <Link to="/login" aria-label="Sign in">
            ×
          </Link>
        </div>
        {!token && <p className="form-error">This link is invalid. Request a new reset from forgot password.</p>}

        <form onSubmit={handleSubmit} className="auth-form">
          {error && <p className="form-error">{error}</p>}
          <PasswordInput
            placeholder="New password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={8}
            autoComplete="new-password"
          />
          <PasswordInput
            placeholder="Confirm new password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            minLength={8}
            autoComplete="new-password"
          />
          <button type="submit" className="btn-login" disabled={loading || !token}>
            {loading ? "…" : "UPDATE PASSWORD"}
          </button>
        </form>

        <p className="auth-footer">
          <Link to="/login">Sign in</Link>
        </p>
      </div>
    </div>
  );
}
