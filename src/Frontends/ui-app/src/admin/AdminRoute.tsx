import { Navigate } from "react-router-dom";
import { getAccessToken, getStoredUser } from "../auth/storage";

export function AdminRoute({ children }: { children: React.ReactNode }) {
  const token = getAccessToken();
  if (!token) {
    return <Navigate to="/login" replace state={{ fromProtected: true }} />;
  }

  const user = getStoredUser();
  if (user?.role !== "admin") {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
