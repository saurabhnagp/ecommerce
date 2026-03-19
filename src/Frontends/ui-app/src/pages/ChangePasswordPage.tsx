import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { changePassword } from "../api/auth";
import { isSessionExpiredError } from "../api/errors";
import { endSessionDueToUnauthorized } from "../auth/endSession";
import { clearSession, getAccessToken } from "../auth/storage";
import "./auth-pages.css";

export function ChangePasswordPage() {
  const navigate = useNavigate();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmNewPassword, setConfirmNewPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    if (newPassword !== confirmNewPassword) {
      setError("New passwords do not match.");
      return;
    }
    const token = getAccessToken();
    if (!token) {
      navigate("/login", { replace: true });
      return;
    }
    setLoading(true);
    try {
      await changePassword(currentPassword, newPassword, confirmNewPassword, token);
      setSuccess(true);
      clearSession();
      setTimeout(() => navigate("/login", { replace: true }), 2000);
    } catch (err) {
      if (isSessionExpiredError(err)) {
        endSessionDueToUnauthorized();
        navigate("/", { replace: true });
        return;
      }
      setError(err instanceof Error ? err.message : "Could not change password");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-page-header">
          <span>Change password</span>
          <Link to="/" aria-label="Home">
            ×
          </Link>
        </div>
        <p className="auth-sub">After changing your password you will need to sign in again.</p>

        {success ? (
          <p className="form-success">Password updated. Redirecting to sign in…</p>
        ) : (
          <form onSubmit={handleSubmit} className="auth-form">
            {error && <p className="form-error">{error}</p>}
            <label className="field-wrap">
              <input
                type="password"
                placeholder="Current password"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                required
                autoComplete="current-password"
              />
              <span className="field-icon" aria-hidden>
                🔒
              </span>
            </label>
            <label className="field-wrap">
              <input
                type="password"
                placeholder="New password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
                minLength={8}
                autoComplete="new-password"
              />
              <span className="field-icon" aria-hidden>
                🔒
              </span>
            </label>
            <label className="field-wrap">
              <input
                type="password"
                placeholder="Confirm new password"
                value={confirmNewPassword}
                onChange={(e) => setConfirmNewPassword(e.target.value)}
                required
                minLength={8}
                autoComplete="new-password"
              />
              <span className="field-icon" aria-hidden>
                🔒
              </span>
            </label>
            <button type="submit" className="btn-login" disabled={loading}>
              {loading ? "…" : "UPDATE PASSWORD"}
            </button>
          </form>
        )}

        <p className="auth-footer">
          <Link to="/">Home</Link>
        </p>
      </div>
    </div>
  );
}
