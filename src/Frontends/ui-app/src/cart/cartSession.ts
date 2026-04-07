const KEY = "amcart_cart_session_id";

/** crypto.randomUUID() requires a secure context (HTTPS); fall back to getRandomValues(). */
function uuidV4(): string {
  if (typeof crypto.randomUUID === "function") return crypto.randomUUID();
  const b = new Uint8Array(16);
  crypto.getRandomValues(b);
  b[6] = (b[6] & 0x0f) | 0x40;
  b[8] = (b[8] & 0x3f) | 0x80;
  const h = Array.from(b, (x) => x.toString(16).padStart(2, "0")).join("");
  return `${h.slice(0, 8)}-${h.slice(8, 12)}-${h.slice(12, 16)}-${h.slice(16, 20)}-${h.slice(20)}`;
}

/** Guest cart identity; persisted until login merge or manual clear. */
export function getOrCreateCartSessionId(): string {
  let v = localStorage.getItem(KEY);
  if (!v || v.trim().length < 8) {
    v = uuidV4();
    localStorage.setItem(KEY, v);
  }
  return v;
}

export function clearCartSessionId() {
  localStorage.removeItem(KEY);
}
