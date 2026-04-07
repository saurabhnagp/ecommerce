import { generateCodeVerifier, generateOAuthState, sha256Base64Url } from "./oauthPkce";

export type OAuthProviderId = "google" | "facebook" | "twitter";

export function callbackPath(provider: OAuthProviderId): string {
  return `/auth/callback/${provider}`;
}

export function redirectUriFor(provider: OAuthProviderId): string {
  return `${window.location.origin}${callbackPath(provider)}`;
}

const stateKey = (p: OAuthProviderId) => `oauth_${p}_state`;
const verifierKey = (p: OAuthProviderId) => `oauth_${p}_verifier`;

export function readStoredState(provider: OAuthProviderId): string | null {
  return sessionStorage.getItem(stateKey(provider));
}

export function readStoredVerifier(provider: OAuthProviderId): string | null {
  return sessionStorage.getItem(verifierKey(provider));
}

export function clearOAuthSession(provider: OAuthProviderId): void {
  sessionStorage.removeItem(stateKey(provider));
  sessionStorage.removeItem(verifierKey(provider));
}

export async function startGoogleOAuth(clientId: string): Promise<void> {
  if (!clientId.trim()) return;
  const verifier = generateCodeVerifier();
  const challenge = await sha256Base64Url(verifier);
  const state = generateOAuthState();
  const ru = redirectUriFor("google");
  sessionStorage.setItem(stateKey("google"), state);
  sessionStorage.setItem(verifierKey("google"), verifier);
  const q = new URLSearchParams({
    client_id: clientId.trim(),
    redirect_uri: ru,
    response_type: "code",
    scope: "openid email profile",
    state,
    code_challenge: challenge,
    code_challenge_method: "S256",
  });
  window.location.assign(`https://accounts.google.com/o/oauth2/v2/auth?${q.toString()}`);
}

export function startFacebookOAuth(appId: string): void {
  if (!appId.trim()) return;
  const state = generateOAuthState();
  sessionStorage.setItem(stateKey("facebook"), state);
  const ru = redirectUriFor("facebook");
  const q = new URLSearchParams({
    client_id: appId.trim(),
    redirect_uri: ru,
    response_type: "code",
    scope: "email,public_profile",
    state,
  });
  window.location.assign(`https://www.facebook.com/v21.0/dialog/oauth?${q.toString()}`);
}

export async function startTwitterOAuth(clientId: string): Promise<void> {
  if (!clientId.trim()) return;
  const verifier = generateCodeVerifier();
  const challenge = await sha256Base64Url(verifier);
  const state = generateOAuthState();
  const ru = redirectUriFor("twitter");
  sessionStorage.setItem(stateKey("twitter"), state);
  sessionStorage.setItem(verifierKey("twitter"), verifier);
  const q = new URLSearchParams({
    response_type: "code",
    client_id: clientId.trim(),
    redirect_uri: ru,
    scope: "users.read",
    state,
    code_challenge: challenge,
    code_challenge_method: "S256",
  });
  window.location.assign(`https://twitter.com/i/oauth2/authorize?${q.toString()}`);
}

export function isOAuthConfigured(): { google: boolean; facebook: boolean; twitter: boolean } {
  return {
    google: !!import.meta.env.VITE_GOOGLE_CLIENT_ID?.trim(),
    facebook: !!import.meta.env.VITE_FACEBOOK_APP_ID?.trim(),
    twitter: !!import.meta.env.VITE_TWITTER_CLIENT_ID?.trim(),
  };
}
