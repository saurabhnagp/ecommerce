/** Call after login so header re-reads session without full reload. */
const AUTH_CHANGE = "amcart-auth-change";

export function notifyAuthChange() {
  window.dispatchEvent(new Event(AUTH_CHANGE));
}

export function onAuthChange(cb: () => void) {
  window.addEventListener(AUTH_CHANGE, cb);
  return () => window.removeEventListener(AUTH_CHANGE, cb);
}
