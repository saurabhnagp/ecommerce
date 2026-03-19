import { useCallback, useEffect, useState } from "react";
import { Link, Route, Routes, useNavigate } from "react-router-dom";
import { fetchCurrentUser } from "./api/user";
import { isSessionExpiredError } from "./api/errors";
import { endSessionDueToUnauthorized } from "./auth/endSession";
import { onAuthChange } from "./auth/notify";
import { getAccessToken } from "./auth/storage";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { UserMenuDropdown } from "./components/UserMenuDropdown";
import { ChangePasswordPage } from "./pages/ChangePasswordPage";
import { ForgotPasswordPage } from "./pages/ForgotPasswordPage";
import { HomePage } from "./pages/HomePage";
import { LoginPage } from "./pages/LoginPage";
import { ProfilePage } from "./pages/ProfilePage";
import { RegisterPage } from "./pages/RegisterPage";
import { ResetPasswordPage } from "./pages/ResetPasswordPage";
import "./App.css";

export default function App() {
  const navigate = useNavigate();
  const [, bump] = useState(0);
  const refresh = useCallback(() => bump((n) => n + 1), []);

  useEffect(() => {
    return onAuthChange(refresh);
  }, [refresh]);

  const signedIn = !!getAccessToken();

  /** If token is invalid/expired, clear session and go home so header shows Sign in / Register. */
  useEffect(() => {
    if (!signedIn) return;
    const token = getAccessToken();
    if (!token) return;
    let cancelled = false;
    fetchCurrentUser(token).catch((e) => {
      if (cancelled || !isSessionExpiredError(e)) return;
      endSessionDueToUnauthorized();
      navigate("/", { replace: true });
    });
    return () => {
      cancelled = true;
    };
  }, [signedIn, navigate]);

  return (
    <div className="app">
      <header className="app-header">
        <Link to="/" className="logo-link">
          <h1 className="logo">AmCart</h1>
        </Link>
        <nav className="nav">
          {signedIn ? (
            <UserMenuDropdown onLogout={refresh} />
          ) : (
            <>
              <Link to="/login" className="btn-outline link-btn">
                Sign in
              </Link>
              <Link to="/register" className="btn-primary link-btn">
                Register
              </Link>
            </>
          )}
        </nav>
      </header>

      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/auth/reset-password" element={<ResetPasswordPage />} />
        <Route
          path="/account/profile"
          element={
            <ProtectedRoute>
              <ProfilePage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/account/change-password"
          element={
            <ProtectedRoute>
              <ChangePasswordPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </div>
  );
}
