import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { register } from "../api/auth";
import { notifyAuthChange } from "../auth/notify";
import { saveSession } from "../auth/storage";
import { PasswordInput } from "../components/PasswordInput";
import "./auth-pages.css";

export function RegisterPage() {
  const navigate = useNavigate();
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [mobile, setMobile] = useState("");
  const [gender, setGender] = useState<"Male" | "Female" | "">("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }
    setLoading(true);
    try {
      const name = `${firstName.trim()} ${lastName.trim()}`.trim();
      const res = await register({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
        password,
        confirmPassword,
        name,
        phone: mobile.trim() || undefined,
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
          <span>Create Account</span>
          <Link to="/" aria-label="Home">
            &times;
          </Link>
        </div>

        <form onSubmit={handleSubmit} className="auth-form">
          {error && <p className="form-error">{error}</p>}
          <div className="register-fields">
            <div className="register-name-row">
              <input
                type="text"
                placeholder="First Name *"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                required
                autoComplete="given-name"
              />
              <input
                type="text"
                placeholder="Last Name *"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                required
                autoComplete="family-name"
              />
            </div>
            <input
              type="email"
              placeholder="Email Address *"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
            />
            <input
              type="tel"
              placeholder="Mobile Number *"
              value={mobile}
              onChange={(e) => setMobile(e.target.value)}
              required
              autoComplete="tel"
            />
            <div className="gender-row">
              <span className="gender-label">Gender</span>
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

          <PasswordInput
            placeholder="Password (min 8 characters) *"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            minLength={8}
            autoComplete="new-password"
          />
          <PasswordInput
            placeholder="Confirm Password *"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            minLength={8}
            autoComplete="new-password"
          />

          <button type="submit" className="btn-register-submit" disabled={loading}>
            {loading ? "Creating account\u2026" : "REGISTER"}
          </button>

          <p className="auth-footer">
            Already have an account? <Link to="/login">Sign in</Link>
          </p>
        </form>
      </div>
    </div>
  );
}
