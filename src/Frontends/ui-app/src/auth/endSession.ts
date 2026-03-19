import { notifyAuthChange } from "./notify";
import { clearSession } from "./storage";

/** Clear tokens and refresh header (Sign in / Register). */
export function endSessionDueToUnauthorized() {
  clearSession();
  notifyAuthChange();
}
