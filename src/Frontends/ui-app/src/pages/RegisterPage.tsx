import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { register } from "../api/auth";
import { notifyAuthChange } from "../auth/notify";
import { saveSession } from "../auth/storage";
import "./auth-pages.css";

function defaultNameFromEmail(email: string) {
  const local = email.split("@")[0] ?? "User";
  return local.replace(/[._-]+/g, " ").replace(/\b\w/g, (c) => c.toUpperCase()) || "User";
}

export function RegisterPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [phone, setPhone] = useState("");
  const [gender, setGender] = useState<"Male" | "Female" | "">("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const name = defaultNameFromEmail(email.trim());
      const res = await register({
        email: email.trim(),
        password,
        name,
        phone: phone.trim() || undefined,
        gender: gender || undefined,
      });
      if (res.data?.tokens && res.data.user) {
        saveSession(res.data.tokens, res.data.user);
        notifyAuthChange();
        navigate("/", { replace: true });
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card register-card">
        <div className="auth-page-header">
          <span>Register</span>
          <Link to="/" aria-label="Home">
            ×
          </Link>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          {error && <p className="form-error">{error}</p>}
          <div className="register-fields">
            <input
              type="email"
              placeholder="Your Email Address"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
            />
            <input
              type="password"
              placeholder="Choose Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              minLength={8}
              autoComplete="new-password"
            />
            <input
              type="tel"
              placeholder="Mobile Number (For order status updates)"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              autoComplete="tel"
            />
            <div className="gender-row">
              <span className="gender-label">I&apos;m a</span>
              <label>
                <input
                  type="radio"
                  name="gender"
                  checked={gender === "Male"}
                  onChange={() => setGender("Male")}
                />
                Male
              </label>
              <label>
                <input
                  type="radio"
                  name="gender"
                  checked={gender === "Female"}
                  onChange={() => setGender("Female")}
                />
                Female
              </label>
            </div>
          </div>

          <button type="submit" className="btn-register-submit" disabled={loading}>
            {loading ? "…" : "REGISTER"}
          </button>

          <p className="auth-footer">
            Already have an account? <Link to="/login">Sign in</Link>
          </p>
        </form>
      </div>
    </div>
  );
}
