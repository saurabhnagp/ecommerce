import { useCallback, useEffect, useState } from "react";
import { Route, Routes, useNavigate } from "react-router-dom";
import { fetchCurrentUser } from "./api/user";
import { isSessionExpiredError } from "./api/errors";
import { endSessionDueToUnauthorized } from "./auth/endSession";
import { onAuthChange } from "./auth/notify";
import { getAccessToken } from "./auth/storage";
import { useInactivityLogout } from "./auth/useInactivityLogout";
import { CartProvider } from "./cart/CartContext";
import { Header } from "./components/Header";
import { Footer } from "./components/Footer";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { ChangePasswordPage } from "./pages/ChangePasswordPage";
import { ContactPage } from "./pages/ContactPage";
import { ForgotPasswordPage } from "./pages/ForgotPasswordPage";
import { ProductListingPage } from "./pages/ProductListingPage";
import { ProductDetailPage } from "./pages/ProductDetailPage";
import { SearchResultsPage } from "./pages/SearchResultsPage";
import { OutOfStockPage } from "./pages/OutOfStockPage";
import { HomePage } from "./pages/HomePage";
import { LoginPage } from "./pages/LoginPage";
import { OAuthCallbackPage } from "./pages/OAuthCallbackPage";
import { ProfilePage } from "./pages/ProfilePage";
import { WishlistPage } from "./pages/WishlistPage";
import { CartPage } from "./pages/CartPage";
import { CheckoutPage } from "./pages/CheckoutPage";
import { OrderConfirmationPage } from "./pages/OrderConfirmationPage";
import { AccountOrdersPage } from "./pages/AccountOrdersPage";
import { AccountOrderDetailPage } from "./pages/AccountOrderDetailPage";
import { RegisterPage } from "./pages/RegisterPage";
import { ResetPasswordPage } from "./pages/ResetPasswordPage";
import { AdminRoute } from "./admin/AdminRoute";
import { AdminLayout } from "./admin/AdminLayout";
import { AdminDashboard } from "./admin/AdminDashboard";
import { AdminProducts } from "./admin/AdminProducts";
import { AdminCategories } from "./admin/AdminCategories";
import { AdminBrands } from "./admin/AdminBrands";
import { AdminTestimonials } from "./admin/AdminTestimonials";
import { AdminContacts } from "./admin/AdminContacts";
import { AdminNewsletter } from "./admin/AdminNewsletter";
import { AdminLowStock } from "./admin/AdminLowStock";
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
    <CartProvider>
      <div className="app">
        <Header signedIn={signedIn} onAuthRefresh={refresh} />

        <Routes>
        {/* Admin panel — full-width, own layout */}
        <Route
          path="/admin/*"
          element={
            <AdminRoute>
              <Routes>
                <Route element={<AdminLayout />}>
                  <Route index element={<AdminDashboard />} />
                  <Route path="products" element={<AdminProducts />} />
                  <Route path="low-stock" element={<AdminLowStock />} />
                  <Route path="categories" element={<AdminCategories />} />
                  <Route path="brands" element={<AdminBrands />} />
                  <Route path="testimonials" element={<AdminTestimonials />} />
                  <Route path="contacts" element={<AdminContacts />} />
                  <Route path="newsletter" element={<AdminNewsletter />} />
                </Route>
              </Routes>
            </AdminRoute>
          }
        />

        {/* Public / user pages — constrained width */}
        <Route
          path="*"
          element={
            <>
              <main className="app-main">
                <Routes>
                  <Route path="/" element={<HomePage />} />
                  <Route path="/login" element={<LoginPage />} />
                  <Route path="/auth/callback/:provider" element={<OAuthCallbackPage />} />
                  <Route path="/register" element={<RegisterPage />} />
                  <Route path="/search" element={<SearchResultsPage />} />
                  <Route path="/products/:slug" element={<ProductDetailPage />} />
                  <Route path="/products" element={<ProductListingPage />} />
                  <Route path="/new-products" element={<ProductListingPage />} />
                  <Route path="/popular" element={<ProductListingPage />} />
                  <Route path="/sale" element={<ProductListingPage />} />
                  <Route path="/category/:slug" element={<ProductListingPage />} />
                  <Route path="/cart" element={<CartPage />} />
                  <Route path="/checkout" element={<CheckoutPage />} />
                  <Route path="/order-confirmation" element={<OrderConfirmationPage />} />
                  <Route path="/out-of-stock" element={<OutOfStockPage />} />
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
                  <Route
                    path="/account/orders"
                    element={
                      <ProtectedRoute>
                        <AccountOrdersPage />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/account/orders/:orderId"
                    element={
                      <ProtectedRoute>
                        <AccountOrderDetailPage />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/wishlist"
                    element={
                      <ProtectedRoute>
                        <WishlistPage />
                      </ProtectedRoute>
                    }
                  />
                </Routes>
              </main>
              <Footer />
            </>
          }
        />
      </Routes>
      </div>
    </CartProvider>
  );
}
