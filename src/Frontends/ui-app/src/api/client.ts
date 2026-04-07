/** Shared API base (same as auth). */
export function base() {
  const u = import.meta.env.VITE_USER_SERVICE_URL?.trim();
  return u ? u.replace(/\/$/, "") : "";
}
