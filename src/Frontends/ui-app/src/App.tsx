import { useCallback, useEffect, useState } from "react";
import { Route, Routes, useNavigate } from "react-router-dom";
import { fetchCurrentUser } from "./api/user";
import { isSessionExpiredError } from "./api/errors";
import { endSessionDueToUnauthorized } from "./auth/endSession";
import { onAuthChange } from "./auth/notify";
import { getAccessToken } from "./auth/storage";
import { useInactivityLogout } from "./auth/useInactivityLogout";
import { Header } from "./components/Header";
import { Footer } from "./components/Footer";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { ChangePasswordPage } from "./pages/ChangePasswordPage";
import { ContactPage } from "./pages/ContactPage";
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

  const handleInactivityLogout = useCallback(() => {
    refresh();
    navigate("/", { replace: true });
  }, [refresh, navigate]);

  useInactivityLogout(signedIn, handleInactivityLogout);

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
      <Header signedIn={signedIn} onAuthRefresh={refresh} />

      <main className="app-main">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/contact" element={<ContactPage />} />
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
      </main>

      <Footer />
    </div>
  );
}
