import { Navigate } from "react-router-dom";
import { getAccessToken } from "../auth/storage";

export function ProtectedRoute({ children }: { children: React.ReactNode }) {
  if (!getAccessToken()) {
    return <Navigate to="/login" replace state={{ fromProtected: true }} />;
  }
  return <>{children}</>;
}
