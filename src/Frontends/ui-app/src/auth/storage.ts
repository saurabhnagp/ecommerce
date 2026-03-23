const ACCESS = "amcart_access_token";
const REFRESH = "amcart_refresh_token";
const USER = "amcart_user";

export function saveSession(tokens: { accessToken: string; refreshToken: string }, user: unknown) {
  localStorage.setItem(ACCESS, tokens.accessToken);
  localStorage.setItem(REFRESH, tokens.refreshToken);
  localStorage.setItem(USER, JSON.stringify(user));
}

export function clearSession() {
  localStorage.removeItem(ACCESS);
  localStorage.removeItem(REFRESH);
  localStorage.removeItem(USER);
}

export function getAccessToken() {
  return localStorage.getItem(ACCESS);
}

export function getStoredUser(): { email?: string; name?: string; phone?: string } | null {
  try {
    const u = localStorage.getItem(USER);
    return u ? JSON.parse(u) : null;
  } catch {
    return null;
  }
}
