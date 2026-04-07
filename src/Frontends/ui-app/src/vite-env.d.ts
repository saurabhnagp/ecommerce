/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_USER_SERVICE_URL: string;
  /** Google OAuth Web client ID (browser). */
  readonly VITE_GOOGLE_CLIENT_ID?: string;
  /** Meta Facebook Login app ID. */
  readonly VITE_FACEBOOK_APP_ID?: string;
  /** X (Twitter) OAuth 2.0 client ID. */
  readonly VITE_TWITTER_CLIENT_ID?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
