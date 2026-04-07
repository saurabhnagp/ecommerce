/** Thrown when the API returns 401 — token missing, expired, or revoked. */
export class SessionExpiredError extends Error {
  override readonly name = "SessionExpiredError";
  constructor() {
    super("Session expired");
  }
}

export function isSessionExpiredError(e: unknown): e is SessionExpiredError {
  return e instanceof SessionExpiredError;
}
