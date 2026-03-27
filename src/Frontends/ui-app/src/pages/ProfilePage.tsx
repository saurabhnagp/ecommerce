import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { fetchCurrentUser, type UserProfile } from "../api/user";
import { isSessionExpiredError } from "../api/errors";
import { endSessionDueToUnauthorized } from "../auth/endSession";
import { getAccessToken, getStoredUser } from "../auth/storage";

export function ProfilePage() {
  const navigate = useNavigate();
  const token = getAccessToken();
  const stored = getStoredUser();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!token) {
      setLoading(false);
      return;
    }
    fetchCurrentUser(token)
      .then(setProfile)
      .catch((e) => {
        if (isSessionExpiredError(e)) {
          endSessionDueToUnauthorized();
          navigate("/", { replace: true });
          return;
        }
        setErr(e instanceof Error ? e.message : "Failed to load profile");
      })
      .finally(() => setLoading(false));
  }, [token, navigate]);

  if (loading) return <p className="muted">Loading profile…</p>;
  if (err && !profile) {
    return (
      <div className="auth-card">
        <p className="auth-error">{err}</p>
        <p className="muted">Showing saved session data below.</p>
      </div>
    );
  }

  const p = profile;
  return (
    <div className="auth-card profile-page">
      <h1 className="auth-title">Your profile</h1>
      {p ? (
        <dl className="profile-dl">
          <dt>Name</dt>
          <dd>{p.name || `${p.firstName} ${p.lastName}`.trim() || "—"}</dd>
          <dt>Email</dt>
          <dd>{p.email}</dd>
          <dt>Role</dt>
          <dd>{p.role}</dd>
          <dt>Status</dt>
          <dd>{p.status}</dd>
          <dt>Verified</dt>
          <dd>{p.isVerified ? "Yes" : "No"}</dd>
          {p.phone && (
            <>
              <dt>Phone</dt>
              <dd>{p.phone}</dd>
            </>
          )}
          {p.lastLoginAt && (
            <>
              <dt>Last login</dt>
              <dd>{new Date(p.lastLoginAt).toLocaleString()}</dd>
            </>
          )}
        </dl>
      ) : (
        <dl className="profile-dl">
          <dt>Email</dt>
          <dd>{stored?.email ?? "—"}</dd>
          <dt>Name</dt>
          <dd>{stored?.name ?? "—"}</dd>
        </dl>
      )}
      <p className="profile-actions">
        <Link to="/">Home</Link>
      </p>
    </div>
  );
}
