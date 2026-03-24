const KEY = "amcart_cart_session_id";

/** Guest cart identity; persisted until login merge or manual clear. */
export function getOrCreateCartSessionId(): string {
  let v = localStorage.getItem(KEY);
  if (!v || v.trim().length < 8) {
    v = crypto.randomUUID();
    localStorage.setItem(KEY, v);
  }
  return v;
}

export function clearCartSessionId() {
  localStorage.removeItem(KEY);
}
