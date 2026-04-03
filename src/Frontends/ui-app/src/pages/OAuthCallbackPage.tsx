import { useEffect, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { completeExternalLogin } from "../api/auth";
import { mergeCart } from "../api/cart";
import {
  callbackPath,
  clearOAuthSession,
  readStoredState,
  readStoredVerifier,
  redirectUriFor,
  type OAuthProviderId,
} from "../auth/oauthProviders";
import { clearCartSessionId, getOrCreateCartSessionId } from "../cart/cartSession";
import { notifyAuthChange } from "../auth/notify";
import { saveSession } from "../auth/storage";
import "./auth-pages.css";

function parseProvider(raw: string | undefined): OAuthProviderId | null {
  if (raw === "google" || raw === "facebook" || raw === "twitter") return raw;
  return null;
}

export function OAuthCallbackPage() {
  const { provider: raw } = useParams<{ provider: string }>();
  const [search] = useSearchParams();
  const navigate = useNavigate();
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const provider = parseProvider(raw);
    if (!provider) {
      setMessage("Invalid sign-in provider.");
      return;
    }

    const code = search.get("code");
    const state = search.get("state");
    const err = search.get("error_description") || search.get("error");

    if (err) {
      clearOAuthSession(provider);
      setMessage(err.replace(/\+/g, " "));
      return;
    }

    if (!code || !state) {
      setMessage("Missing authorization code. Try signing in again.");
      return;
    }

    const expected = readStoredState(provider);
    if (!expected || expected !== state) {
      clearOAuthSession(provider);
      setMessage("Invalid or expired sign-in session. Please try again.");
      return;
    }

    const verifier = readStoredVerifier(provider);
    if ((provider === "google" || provider === "twitter") && !verifier) {
      clearOAuthSession(provider);
      setMessage("Missing PKCE verifier. Please try signing in again.");
      return;
    }

    const ru = redirectUriFor(provider);
    let cancelled = false;

    (async () => {
      try {
        const guestSessionId = getOrCreateCartSessionId();
        const { user, tokens } = await completeExternalLogin(provider, {
          code,
          codeVerifier: verifier ?? undefined,
          redirectUri: ru,
        });
        clearOAuthSession(provider);
        saveSession(tokens, user);
        try {
          await mergeCart(tokens.accessToken, guestSessionId);
        } catch {
          /* ignore */
        }
        clearCartSessionId();
        notifyAuthChange();
        if (!cancelled) navigate("/", { replace: true });
      } catch (e) {
        clearOAuthSession(provider);
        if (!cancelled) setMessage(e instanceof Error ? e.message : "Sign-in failed.");
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [raw, search, navigate]);

  const provider = parseProvider(raw);

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-page-header">
          <span>Signing you in…</span>
          <Link to="/" aria-label="Home">
            ×
          </Link>
        </div>
        {message && (
          <>
            <p className="form-error">{message}</p>
            <p className="muted" style={{ fontSize: "0.9rem", marginTop: "1rem" }}>
              Redirect URI for provider settings must be exactly:{" "}
              <code style={{ wordBreak: "break-all" }}>
                {provider ? `${window.location.origin}${callbackPath(provider)}` : "—"}
              </code>
            </p>
            <Link to="/login" className="btn-login" style={{ display: "inline-block", marginTop: "1rem" }}>
              Back to sign in
            </Link>
          </>
        )}
        {!message && <p className="muted">Completing sign-in…</p>}
      </div>
    </div>
  );
}
