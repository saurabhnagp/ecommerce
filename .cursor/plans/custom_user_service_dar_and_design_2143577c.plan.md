---
name: Custom User Service DAR and Design
overview: Create a new DAR document that evaluates and recommends a custom in-house User Service for authentication (instead of AWS Cognito), plus a standalone technical design document describing the architecture, schema, and implementation approach for that custom auth solution.
todos: []
isProject: false
---

# Custom User Service DAR and Technical Design

## Goal

Produce two deliverables:

1. **DAR document** – Decision Analysis and Resolution that evaluates **Custom .NET User Service (in-house auth)** versus **AWS Cognito** (and optionally Keycloak), with a recommendation for the custom approach when the organization prefers no vendor lock-in, full control, or data residency.
2. **Technical design document** – Architecture, data model, flows, and implementation approach for building the custom User Service that owns all authentication (no Cognito).

Both documents will align with existing AmCart docs: [docs/DAR-Cognito-APIGateway-Selection.md](docs/DAR-Cognito-APIGateway-Selection.md) (structure and scoring style), [docs/Database-Schema-User.md](docs/Database-Schema-User.md) (current user schema), [docs/ADR/ADR-006-authentication.md](docs/ADR/ADR-006-authentication.md) (JWT flows and security), and [docs/API-Specifications.md](docs/API-Specifications.md) (auth endpoints).

---

## 1. DAR Document: Custom User Service Authentication

**File:** `docs/DAR-Custom-User-Service-Auth.md`

**Purpose:** Standalone DAR for the alternative “build” option: custom User Service handles auth end-to-end (register, login, JWT, refresh, password reset, email verification, optional social login). Use when the project chooses **not** to use Cognito.

**Structure (mirror [DAR-Cognito-APIGateway-Selection.md](docs/DAR-Cognito-APIGateway-Selection.md)):**

- **Document control** – Title: “Authentication - Custom User Service (In-House)”; ID e.g. DAR-AMCART-AUTH-002; version, status, dates.
- **Introduction** – Objective: evaluate in-house custom auth vs managed (Cognito). Scope: same auth capabilities (email/password, MFA, social, JWT, rate limiting) without Cognito; API protection via Nginx + JWT validation at gateway or services.
- **Requirements at a glance** – Reuse or slightly adapt the same FR/NFR table from the Cognito DAR (email/password, social, MFA, JWT, refresh, rate limiting, lockout, etc.) so comparison is like-for-like.
- **Options (3–4):**
  - **Custom .NET User Service** – User Service (.NET 8) owns credentials (password hash in PostgreSQL), issues/validates JWT, refresh tokens (Redis or DB), email verification, password reset, optional Google/Facebook OAuth; Nginx or API Gateway in front; no Cognito.
  - **AWS Cognito + API Gateway** – As in existing DAR (reference only; no need to duplicate full text).
  - **Keycloak (self-hosted)** – Open-source IdP; optional for comparison.
  - Optionally **Auth0** – If useful for stakeholder comparison.
- **Comparison analysis:**
  - **Point matrix** – Same “direct points out of max” style as Cognito DAR (e.g. 20 points for “Full control / no lock-in”, 15 for “Cost predictability”, 15 for “Data residency”, 10 for “.NET integration”, 10 for “Operational burden”, etc.). Custom scores high on control, residency, .NET; lower on “ease of setup” and “managed MFA/social”. Cognito scores high on AWS integration and operational simplicity. Include **Total Score** row (sum of points).
  - **Feature comparison** – Table: Email/Password, Google/Facebook OAuth, MFA (SMS/TOTP), JWT, Rate limiting, Where credentials live, Managed vs self-hosted.
  - **Cost comparison** – Scenario: 100K MAU, 10M API calls. Custom: no Cognito/IdP licence; infra (EKS, RDS, Redis) + one-time dev (e.g. 2–4 dev-months) + ongoing maintenance. Cognito: as in existing DAR. 5-year TCO for Custom vs Cognito.
  - **Security comparison** – SOC 2 (self vs AWS), encryption, brute-force/lockout, token handling, audit logs; note that with custom auth the team owns security implementation and compliance evidence.
- **Recommendation** – **Custom User Service** when: (1) no vendor lock-in desired, (2) strict data residency or compliance needs, (3) long-term cost predictability and no per-MAU fees, (4) team has capacity to build and maintain auth. State that the existing DAR (Cognito) remains the recommended choice when prioritizing time-to-market and managed security/MFA.
- **Assumptions** – e.g. team can implement OAuth and MFA; PostgreSQL + Redis already in use; Nginx/API Gateway used for routing (not Cognito Authorizer).
- **Risks** – Security ownership, implementation bugs, ongoing maintenance, MFA/social login complexity; mitigations (OWASP, libraries, security review).
- **Appendix** – References (OWASP, JWT best practices, .NET Identity), short glossary (JWT, refresh token, etc.), revision history.

**Executive summary (short paragraph at top):** This DAR evaluates a custom in-house User Service for authentication as an alternative to AWS Cognito. It recommends the custom approach when the organization prioritizes full control, data residency, and no per-user licensing over managed MFA and faster initial setup. The technical design for the custom approach is described in a separate document.

---

## 2. Technical Design Document: Custom User Service Authentication

**File:** `docs/Technical-Design-Custom-User-Service-Auth.md`

**Purpose:** Single reference for how to build and operate the custom User Service that performs all authentication (no Cognito): APIs, schema, flows, security, and integration with the rest of AmCart.

**Suggested sections:**

1. **Overview**
  - Goals: full ownership of credentials, JWT issuance/validation, same UX as SRS (login, register, social, password reset, email verification).
  - Out of scope: authorization rules inside other services (only “who is this user?”); payment security; admin auth (can be same service, different role).
2. **Architecture**
  - Diagram (Mermaid): Client (Nuxt) → Nginx/API Gateway → User Service (.NET 8); User Service ↔ PostgreSQL (users, addresses, sessions, refresh_tokens if DB-backed), Redis (refresh tokens and/or blacklist); optional Notification Service for emails.
  - No Cognito, no Lambda triggers. JWT validation: either at Nginx (e.g. `auth_request` to User Service or small JWT validation) or at each service using shared JWT secret/JWKS.
  - List responsibilities: register, login, refresh, logout, forgot-password, reset-password, verify-email, social callback (Google/Facebook), token validation endpoint or shared library.
3. **Database schema changes (PostgreSQL)**
  - **users:** Add `password_hash` (nullable for social-only users); add `email_verification_token`, `email_verification_token_expires_at`; add `password_reset_token`, `password_reset_token_expires_at`. Remove requirement for `cognito_sub` (drop NOT NULL or make column optional and backfill for existing). Keep `google_id`, `facebook_id`, `auth_provider`, `failed_login_attempts`, `lockout_end`, `last_login_at`, etc. Reference [Database-Schema-User.md](docs/Database-Schema-User.md) and call out only deltas.
  - **refresh_tokens** (if DB-backed): id, user_id, token_hash, device_info, expires_at, revoked_at; index on (token_hash), (user_id, expires_at). Alternative: store refresh tokens only in Redis (key `refresh:{token_hash}` or `refresh:{user_id}:{family}`) and document TTL and rotation.
  - **user_sessions:** Remove `cognito_session_id`; keep device/location fields for audit. Optional: link session to refresh token or keep as audit log only.
  - One short ERD (Mermaid or ASCII) showing users, refresh_tokens, user_sessions.
4. **API surface**
  - Align with [API-Specifications.md](docs/API-Specifications.md): POST /auth/register, /auth/login, /auth/refresh, /auth/logout, /auth/forgot-password, /auth/reset-password; GET /auth/verify-email; GET /auth/google, /auth/google/callback; GET /auth/facebook, /auth/facebook/callback. Request/response shapes stay as in API spec; only implementation changes (User Service performs all logic instead of Cognito).
5. **Authentication flows**
  - **Registration:** User → POST /auth/register → User Service creates user (password hash, verification token), sends email → User clicks link → GET /auth/verify-email → Service marks verified. Mermaid sequence diagram.
  - **Login:** POST /auth/login → validate credentials, check lockout, verify password hash → issue access + refresh token (store refresh in Redis/DB) → return tokens. Mermaid sequence diagram.
  - **Refresh:** POST /auth/refresh with refresh token → validate, optionally rotate refresh token → return new access (and optionally new refresh). Diagram optional.
  - **Forgot/Reset password:** Request reset → email with token → POST /auth/reset-password with token + new password. One diagram.
  - **Social login:** Redirect to Google/Facebook → callback with code → User Service exchanges code for profile, finds or creates user, issues JWT. Reference [ADR-006](docs/ADR/ADR-006-authentication.md) flows; one high-level diagram.
6. **Security**
  - Passwords: bcrypt (cost 12) or Argon2id; no plaintext storage.
  - JWT: HS256 or RS256; short-lived access (e.g. 15 min), longer refresh (e.g. 7 days); claims: sub (user id), email, role, iss, aud, exp, iat.
  - Refresh tokens: random, hashed before storage; rotation on use; revocation on logout.
  - Rate limiting: auth endpoints (e.g. 10/min per IP or per email); account lockout after N failed attempts (e.g. 5) for 30 min.
  - HTTPS only; HTTP-only cookie for refresh token (if cookie-based); CSRF protection for cookie-based flows.
  - Table: threat vs mitigation (brute force, token theft, session fixation, etc.).
7. **Integration with rest of AmCart**
  - Other microservices: accept `Authorization: Bearer <access_token>`, validate JWT (shared secret or JWKS from User Service), read `sub` and role. No call to Cognito.
  - Nginx/API Gateway: no Cognito Authorizer; either (a) pass-through and each service validates JWT, or (b) Nginx `auth_request` to User Service `/auth/validate` or a small JWT-validation sidecar. Document chosen option or both as alternatives.
  - Frontend (Nuxt): call User Service for login/register/refresh; store access token in memory or secure storage; send in header; refresh token in cookie or secure storage.
8. **Optional: MFA and social login**
  - MFA: TOTP (e.g. OtpNet) or SMS via Notification Service; store TOTP secret in DB (encrypted); flow: enable MFA → verify code → require at login. Brief subsection.
  - Social: Google/Facebook OAuth 2.0; server-side code exchange; create/link user by email; issue same JWT as email login. Reference ADR-006.
9. **Configuration and deployment**
  - Config: JWT secret/issuer/audience, token lifetimes, DB and Redis connection strings, email provider, OAuth client IDs/secrets, rate limit and lockout settings. Example `appsettings` structure.
  - Deployment: User Service as a service on EKS; same CI/CD as other .NET services; secrets in AWS Secrets Manager or equivalent.
10. **References**
  - Links to [ADR-006-authentication.md](docs/ADR/ADR-006-authentication.md), [API-Specifications.md](docs/API-Specifications.md), [Database-Schema-User.md](docs/Database-Schema-User.md), OWASP, JWT best practices.

Use Mermaid for sequence/flow diagrams; avoid spaces in node IDs and follow project Mermaid guidelines.

---

## 3. Relationship to existing documents


| Document                                                                        | Action                                                                                                                                                                                                                                                                                                                         |
| ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| [DAR-Cognito-APIGateway-Selection.md](docs/DAR-Cognito-APIGateway-Selection.md) | No change. Reference it in the new DAR as “the Cognito option” and state that Cognito remains recommended when not choosing custom.                                                                                                                                                                                            |
| [ADR-006-authentication.md](docs/ADR/ADR-006-authentication.md)                 | No change. It already describes JWT, bcrypt, refresh tokens, and flows; the technical design will reference and extend it for the “custom only” deployment (no Cognito).                                                                                                                                                       |
| [Database-Schema-User.md](docs/Database-Schema-User.md)                         | No change in this task. The technical design will document the **deltas** (add password_hash, verification/reset tokens; remove cognito_sub requirement; refresh_tokens table or Redis) so that a future change to Database-Schema-User can add a “Custom Auth variant” or a separate “Custom User Schema” section if desired. |
| [API-Specifications.md](docs/API-Specifications.md)                             | No change. Auth endpoints stay as-is; technical design references them.                                                                                                                                                                                                                                                        |


---

## 4. Deliverables summary


| #   | Deliverable          | Description                                                                                                                                                                                                                                                                          |
| --- | -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1   | **DAR document**     | `docs/DAR-Custom-User-Service-Auth.md` – Evaluation of Custom User Service vs Cognito (and optionally Keycloak/Auth0), direct-point matrix, cost/TCO, security comparison, recommendation for custom when control/residency/cost matter, plus executive summary, assumptions, risks. |
| 2   | **Technical design** | `docs/Technical-Design-Custom-User-Service-Auth.md` – Architecture, schema deltas, API alignment, auth flows (with Mermaid), security measures, integration (Nginx, other services, frontend), optional MFA/social, config and deployment.                                           |


No code or schema file edits in this plan; only the two new markdown documents.