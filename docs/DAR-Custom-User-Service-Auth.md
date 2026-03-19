# DECISION ANALYSIS AND RESOLUTION (DAR) DOCUMENT

## Authentication - Custom User Service (In-House)

---

## Executive Summary

This DAR evaluates a custom in-house User Service for authentication as an alternative to AWS Cognito. It recommends the custom approach when the organization prioritizes full control over identity data, data residency and compliance, and long-term cost predictability without per-user licensing. When time-to-market and managed security or MFA are the primary drivers, the existing [DAR-AMCART-AUTH-001 (Cognito + API Gateway)](DAR-Cognito-APIGateway-Selection.md) remains the recommended choice. The technical design for the custom approach is described in [Technical-Design-Custom-User-Service-Auth.md](Technical-Design-Custom-User-Service-Auth.md).

---

## Document Control Information

| Field | Value |
|:------|:------|
| **Document Title** | Authentication - Custom User Service (In-House) |
| **Document ID** | DAR-AMCART-AUTH-002 |
| **Project Name** | AmCart Ecommerce Platform |
| **Version** | 1.0 |
| **Status** | Approved |
| **Created Date** | January 2026 |
| **Last Updated** | January 2026 |
| **Prepared By** | Technical Architecture Team |
| **Reviewed By** | [Reviewer Name] |
| **Approved By** | [Approver Name] |

---

## Table of Contents

1. [Introduction](#1-introduction)
   - 1.1 [Objective and Scope of Document](#11-objective-and-scope-of-document)
2. [Requirements at a Glance](#2-requirements-at-a-glance)
3. [Available Options](#3-available-options)
   - 3.1 [Custom .NET User Service](#31-custom-net-user-service)
   - 3.2 [AWS Cognito + API Gateway](#32-aws-cognito--api-gateway)
   - 3.3 [Keycloak (Self-Hosted)](#33-keycloak-self-hosted)
   - 3.4 [Auth0](#34-auth0)
4. [Comparison Analysis](#4-comparison-analysis)
   - 4.1 [Point Matrix](#41-point-matrix)
   - 4.2 [Feature Comparison](#42-feature-comparison)
   - 4.3 [Cost Comparison](#43-cost-comparison)
   - 4.4 [Security Comparison](#44-security-comparison)
5. [Recommendation](#5-recommendation)
6. [Assumptions](#6-assumptions)
7. [Risks](#7-risks)
8. [Appendix](#8-appendix)
   - 8.1 [References](#81-references)
   - 8.2 [Glossary](#82-glossary)
   - 8.3 [Revision History](#83-revision-history)

---

## 1. Introduction

### 1.1 Objective and Scope of Document

#### Objective

This Decision Analysis and Resolution (DAR) document evaluates an in-house custom User Service for authentication as an alternative to managed identity services (e.g. AWS Cognito). The objective is to support a deliberate choice when the organization prefers full control over user identity, no vendor lock-in, strict data residency, or predictable long-term costs without per-user licensing.

#### Scope

This document covers:

- Evaluation of custom .NET User Service vs. AWS Cognito, Keycloak, and Auth0
- Same authentication capabilities as the Cognito option: email/password, optional MFA, social login (Google/Facebook), JWT and refresh tokens, email verification, password reset
- API protection via Nginx or API Gateway with JWT validation (no Cognito Authorizer)
- Integration with .NET 8 microservices on AWS EKS and existing PostgreSQL/Redis

#### Out of Scope

- Authorization logic within microservices (handled by services)
- Payment security (covered separately)
- Admin authentication (same service, different role)
- Implementation details (see [Technical-Design-Custom-User-Service-Auth.md](Technical-Design-Custom-User-Service-Auth.md))

---

## 2. Requirements at a Glance

### Functional Requirements

| ID | Requirement | Priority | Description |
|:---|:------------|:---------|:------------|
| FR-01 | Email/Password Authentication | Critical | Users register and login with email and password |
| FR-02 | Social Login - Google | High | OAuth 2.0 integration with Google accounts |
| FR-03 | Social Login - Facebook | High | OAuth 2.0 integration with Facebook accounts |
| FR-04 | Multi-Factor Authentication | High | Optional MFA via SMS or Authenticator app (TOTP) |
| FR-05 | Email Verification | High | Verify email address on registration |
| FR-06 | Password Reset | High | Self-service password reset via email |
| FR-07 | Token-based Sessions | Critical | JWT tokens for stateless authentication |
| FR-08 | Token Refresh | High | Seamless token refresh without re-login |
| FR-09 | API Rate Limiting | Medium | Protect APIs from abuse |
| FR-10 | Request Validation | Medium | Validate API request structure |
| FR-11 | Remember Me | Medium | Extended session duration option |
| FR-12 | Account Lockout | Medium | Lock account after failed attempts |

### Non-Functional Requirements

| ID | Requirement | Target | Description |
|:---|:------------|:-------|:------------|
| NFR-01 | Login Latency | < 500ms | P95 authentication response time |
| NFR-02 | Availability | 99.9% | Authentication service uptime |
| NFR-03 | Concurrent Users | 10,000+ | Simultaneous authenticated sessions |
| NFR-04 | Token Expiry | 15 min – 1 hour | Access token lifetime |
| NFR-05 | Refresh Token Expiry | 7–30 days | Refresh token lifetime |
| NFR-06 | MFA Latency | < 2s | Time to send/verify MFA code |
| NFR-07 | Scalability | Horizontal | Handle traffic spikes |
| NFR-08 | Compliance | Configurable | Data residency and compliance requirements |

### Technical Constraints

| Constraint | Description |
|:-----------|:------------|
| Cloud Provider | AWS (existing infrastructure) |
| Backend Technology | .NET 8 / C# microservices |
| Frontend Technology | Nuxt.js 3 / Vue.js |
| Existing Services | EKS, RDS PostgreSQL, ElastiCache Redis |
| Integration | User data and credentials in PostgreSQL; tokens in Redis or DB |

---

## 3. Available Options

### 3.1 Custom .NET User Service

#### Overview

The User Service (.NET 8) owns all authentication: credentials stored in PostgreSQL (password hash), JWT issuance and validation, refresh tokens in Redis or PostgreSQL, email verification and password reset, and optional Google/Facebook OAuth. Nginx or API Gateway sits in front for routing; JWT is validated at the gateway or at each microservice. No Cognito or third-party IdP.

#### Features

| Feature | Details |
|:--------|:--------|
| **Full Control** | Complete ownership of auth flow, data model, and security policies |
| **No Vendor Lock-in** | No dependency on Cognito, Auth0, or Keycloak |
| **Data Residency** | All identity data stays in your PostgreSQL and region |
| **Cost Predictability** | No per-MAU or per-user licensing; infra + dev/maintenance only |
| **.NET Integration** | Native ASP.NET Core Identity or custom JWT; same codebase as rest of services |
| **Credentials** | Passwords hashed (bcrypt/Argon2) in PostgreSQL |
| **JWT** | Issued and validated by User Service or shared secret/JWKS |
| **Refresh Tokens** | Stored in Redis or DB; rotation and revocation under your control |
| **Email Verification / Reset** | Implemented in User Service; emails via Notification Service or SMTP |
| **Social Login** | Google/Facebook OAuth 2.0 implemented in User Service (code exchange, create/link user) |
| **MFA** | TOTP and/or SMS built with libraries (e.g. OtpNet); you own implementation and ops |
| **Rate Limiting** | Implement in Nginx or in User Service (e.g. 10 req/min on auth endpoints) |
| **Account Lockout** | Implement in User Service (e.g. 5 failed attempts, 30 min lockout) |

#### Effort and Cost

| Component | Estimate |
|:----------|:---------|
| **Development** | 2–4 developer months (email/password, JWT, refresh, verification, reset, social, optional MFA) |
| **Infrastructure** | No additional licence; existing EKS, RDS, Redis |
| **Maintenance** | Security patches, dependency updates, compliance evidence (e.g. SOC 2 self-attestation) |

**Risks:**

- Security responsibility entirely on the team (OWASP, key management, audit logging)
- MFA and social login require careful implementation and testing
- Operational burden for availability, scaling, and incident response

---

### 3.2 AWS Cognito + API Gateway

#### Overview

Fully managed user pools and API Gateway; Cognito handles sign-up, sign-in, MFA, social federation, and JWT issuance. Lambda triggers sync user data to PostgreSQL. See [DAR-Cognito-APIGateway-Selection.md](DAR-Cognito-APIGateway-Selection.md) (DAR-AMCART-AUTH-001) for full description.

#### Summary

| Aspect | Detail |
|:-------|:-------|
| **Credentials** | Managed by Cognito; not stored in your DB |
| **JWT** | Issued by Cognito; validated by API Gateway Cognito Authorizer or services |
| **Cost** | Per MAU (e.g. first 50K free; ~$285/month for 100K MAU + API Gateway) |
| **Pros** | Managed security, MFA, social login; minimal auth code; SOC 2 compliant |
| **Cons** | Vendor lock-in; data in AWS; per-user cost at scale |

---

### 3.3 Keycloak (Self-Hosted)

#### Overview

Open-source identity and access management; self-hosted on EKS or EC2. Supports user federation, social brokering, MFA, and OIDC/JWT.

#### Summary

| Aspect | Detail |
|:-------|:-------|
| **Credentials** | Stored in Keycloak DB or federated (e.g. LDAP) |
| **JWT** | Issued by Keycloak; validated by services or gateway |
| **Cost** | No licence; infra and ops (e.g. ~$200/month + DevOps effort) |
| **Pros** | No per-user fee; flexible; can sync to PostgreSQL via adapters |
| **Cons** | Self-hosted ops; Java stack; less AWS-native than Cognito |

---

### 3.4 Auth0

#### Overview

Identity-as-a-service; universal login, social connections, MFA, rules and actions. See DAR-AMCART-AUTH-001 for comparison.

#### Summary

| Aspect | Detail |
|:-------|:-------|
| **Credentials** | Managed by Auth0 |
| **Cost** | Per MAU (e.g. Professional tier $1,000+ per month for 100K MAU) |
| **Pros** | Rich features, great DX, managed security |
| **Cons** | Cost at scale; data outside your perimeter |

---

## 4. Comparison Analysis

### 4.1 Point Matrix

Criteria are weighted for the question: *When should we choose custom in-house auth over a managed IdP?* Higher scores indicate better fit for that criterion.

| Criteria | Max Points | Custom User Service | Cognito+APIGW | Keycloak | Auth0 |
|:---------|:----------:|:-------------------:|:-------------:|:--------:|:-----:|
| **Full Control / No Vendor Lock-in** | 20 | 20 | 6 | 14 | 4 |
| **Data Residency / Compliance Control** | 15 | 15 | 10 | 14 | 8 |
| **Cost Predictability (no per-MAU)** | 15 | 14 | 8 | 12 | 6 |
| **.NET / Code Integration** | 10 | 10 | 7 | 6 | 7 |
| **Ease of Setup / Time to Market** | 10 | 3 | 9 | 5 | 9 |
| **Operational Simplicity** | 10 | 4 | 10 | 5 | 9 |
| **Managed Security / MFA** | 10 | 5 | 10 | 8 | 10 |
| **Scalability** | 10 | 6 | 10 | 6 | 9 |
| **Total Score** | **100** | **77** | 70 | 65 | 62 |

**Scoring:** Points awarded out of max for each criterion (higher = better for that criterion).

Custom scores highest when control, data residency, cost predictability, and .NET integration are prioritized. Cognito and Auth0 score higher on ease of setup, operational simplicity, and managed security.

### 4.2 Feature Comparison

| Feature | Custom User Service | Cognito+APIGW | Keycloak | Auth0 |
|:--------|:--------------------|:--------------|:---------|:------|
| Email/Password | Yes (build) | Yes | Yes | Yes |
| Google OAuth | Yes (build) | Yes | Yes | Yes |
| Facebook OAuth | Yes (build) | Yes | Yes | Yes |
| MFA - SMS | Build | Yes | Yes | Yes |
| MFA - TOTP | Build | Yes | Yes | Yes |
| JWT Tokens | Yes (build) | Native | Native | Native |
| Refresh Tokens | Yes (Redis/DB) | Yes | Yes | Yes |
| Email Verification | Yes (build) | Yes | Yes | Yes |
| Password Reset | Yes (build) | Yes | Yes | Yes |
| Rate Limiting | Build (Nginx/service) | API Gateway | External | Paid |
| Credentials Location | PostgreSQL | Cognito | Keycloak DB | Auth0 |
| Managed Service | No | Yes | No | Yes |
| Data in Your Boundary | Yes | No | Optional | No |

### 4.3 Cost Comparison

**Scenario:** 100,000 Monthly Active Users, 10 Million API requests/month

| Solution | Monthly Infra/Licence | Year 1 Total | Notes |
|:---------|:----------------------|:-------------|:------|
| **Custom User Service** | ~$100 (EKS/RDS/Redis share) | ~$1,200 + one-time dev (~$50K–80K) | No per-MAU fee |
| **Cognito + APIGW** | ~$285 | ~$3,420 | 50K free MAU; see DAR-AMCART-AUTH-001 |
| **Keycloak** | ~$200 | ~$2,400 + setup | Infra only; DevOps effort |
| **Auth0** | ~$1,000+ | ~$12,000+ | Per-MAU pricing |

**5-Year TCO (illustrative):**

| Solution | Infrastructure | Licensing | Development | Operations | Total 5-Year |
|:---------|:---------------|:----------|:------------|:-----------|:-------------|
| **Custom User Service** | $6,000 | $0 | $70,000 | $25,000 | **$101,000** |
| **Cognito + APIGW** | $5,000 | $12,000 | $5,000 | $3,000 | $25,000 |
| **Keycloak** | $12,000 | $0 | $15,000 | $20,000 | $47,000 |
| **Auth0** | $0 | $60,000 | $3,000 | $2,000 | $65,000 |

Custom has higher upfront development and ongoing operations but zero licensing and full control; TCO can be favourable at scale or when compliance and residency are mandatory.

### 4.4 Security Comparison

| Security Feature | Custom User Service | Cognito+APIGW | Keycloak | Auth0 |
|:-----------------|:--------------------|:--------------|:---------|:------|
| SOC 2 Compliant | Self-attest / audit | Yes (AWS) | Self | Yes |
| GDPR / Data Residency | Full control | AWS region | Self-host control | Provider |
| Encryption at Rest | Config (RDS) | Yes | Config | Yes |
| Encryption in Transit | HTTPS/TLS | Yes | Yes | Yes |
| Brute Force / Lockout | Build | Built-in | Built-in | Built-in |
| Compromised Credential Check | Build or third-party | Advanced | No | Yes |
| Audit Logs | Build (application + DB) | CloudTrail | Yes | Yes |
| Key Management | Your responsibility | AWS | Self | Auth0 |
| Security Ownership | Full | AWS | Self | Auth0 |

With custom auth, the team is responsible for implementing and evidencing security and compliance.

---

## 5. Recommendation

### When to Choose: Custom User Service (In-House)

**Custom .NET User Service** is recommended when one or more of the following apply:

1. **No vendor lock-in** – Organization requires full ownership of identity logic and data with no dependency on Cognito or other IdPs.
2. **Data residency and compliance** – All identity data must remain in your PostgreSQL and region; no third-party identity store.
3. **Long-term cost predictability** – Willing to invest in build and ops in exchange for no per-MAU or per-user licensing at scale.
4. **Team capacity** – Team can implement and maintain auth (including OAuth, optional MFA, rate limiting, lockout) and security practices (OWASP, key management, audit logs).

### When to Choose: AWS Cognito (or Another Managed IdP)

The existing [DAR-AMCART-AUTH-001](DAR-Cognito-APIGateway-Selection.md) remains the recommended choice when:

- **Time-to-market** is the priority and you want minimal auth implementation.
- **Managed security and MFA** are preferred (SOC 2, built-in MFA, compromised credential detection).
- **Operational simplicity** is preferred (no auth servers to run, patch, or scale).
- **Lower upfront cost** is preferred over long-term control (Cognito 5-year TCO is lower in the illustrative scenario above).

### Summary

| Driver | Recommended Option |
|:-------|:-------------------|
| Control, residency, no lock-in, cost at scale | Custom User Service |
| Time-to-market, managed security, lower ops | Cognito + API Gateway |

Technical implementation of the custom approach is described in [Technical-Design-Custom-User-Service-Auth.md](Technical-Design-Custom-User-Service-Auth.md).

---

## 6. Assumptions

| ID | Assumption | Impact if Invalid |
|:---|:-----------|:------------------|
| A1 | Team can implement OAuth 2.0 (Google/Facebook) and optional MFA (TOTP/SMS) with acceptable quality and security | May need to adopt Keycloak or Cognito for social/MFA |
| A2 | PostgreSQL and Redis are already used for AmCart; no new data stores for custom auth | Would need to account for new stores |
| A3 | Nginx or API Gateway is used for routing; JWT validation can be done at gateway or in each service | May need a dedicated auth gateway or sidecar |
| A4 | Same FR/NFR as Cognito option (email/password, social, MFA, JWT, refresh, lockout) are required for custom | Scope would change |
| A5 | Development budget allows 2–4 developer months for auth and related security work | May favour Cognito or Keycloak |
| A6 | Organization accepts responsibility for security implementation and compliance evidence (e.g. SOC 2 self-attestation) | May favour managed IdP |

---

## 7. Risks

| ID | Risk | Probability | Impact | Mitigation |
|:---|:-----|:------------|:-------|:-----------|
| R1 | Security vulnerabilities in custom implementation | Medium | High | Follow OWASP auth cheatsheet; use established libraries (bcrypt, JWT); security review and penetration testing |
| R2 | Bugs in OAuth or MFA flows (e.g. redirect, token exchange) | Medium | High | Use well-tested libraries; integration tests; reference ADR-006 and technical design |
| R3 | Ongoing maintenance burden (dependencies, CVEs, key rotation) | High | Medium | Dedicate ownership; automate dependency updates; document runbooks |
| R4 | Insufficient audit logging for compliance | Medium | High | Design audit events (login, fail, lockout, password reset) from the start; store in DB or centralized logs |
| R5 | Key compromise (JWT signing secret) | Low | Critical | Store secret in AWS Secrets Manager or equivalent; rotate periodically; limit access |
| R6 | Scale or availability issues under load | Medium | Medium | Design for horizontal scaling; rate limiting; load testing |

### Risk Response Plan

| Risk | Response Strategy | Owner |
|:-----|:------------------|:------|
| R1 | Mitigate – OWASP, libraries, security review | Security / Tech Lead |
| R2 | Mitigate – Tests, code review, ADR-006 alignment | Backend Lead |
| R3 | Accept/Mitigate – Clear ownership; automate where possible | DevOps / Tech Lead |
| R4 | Mitigate – Audit logging in design and implementation | Tech Lead |
| R5 | Mitigate – Secrets manager; rotation policy | DevOps / Security |
| R6 | Mitigate – Load tests; scaling and rate-limit design | DevOps |

---

## 8. Appendix

### 8.1 References

| # | Reference | URL |
|:--|:----------|:---|
| 1 | DAR-AMCART-AUTH-001 (Cognito + API Gateway) | [DAR-Cognito-APIGateway-Selection.md](DAR-Cognito-APIGateway-Selection.md) |
| 2 | Technical Design - Custom User Service Auth | [Technical-Design-Custom-User-Service-Auth.md](Technical-Design-Custom-User-Service-Auth.md) |
| 3 | ADR-006 Authentication Strategy | [ADR/ADR-006-authentication.md](ADR/ADR-006-authentication.md) |
| 4 | OWASP Authentication Cheat Sheet | https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html |
| 5 | JWT Best Practices (RFC 8725) | https://datatracker.ietf.org/doc/html/rfc8725 |
| 6 | ASP.NET Core Security | https://docs.microsoft.com/aspnet/core/security |
| 7 | OAuth 2.0 Specification | https://oauth.net/2/ |

### 8.2 Glossary

| Term | Definition |
|:-----|:-----------|
| **IdP** | Identity Provider – service that manages user identities and authentication |
| **JWT** | JSON Web Token – compact, URL-safe token format for claims |
| **MAU** | Monthly Active Users – unique users who authenticate in a month |
| **MFA** | Multi-Factor Authentication – additional verification (e.g. TOTP, SMS) |
| **TOTP** | Time-based One-Time Password (e.g. authenticator app) |
| **TCO** | Total Cost of Ownership |

### 8.3 Revision History

| Version | Date | Author | Changes |
|:--------|:-----|:-------|:--------|
| 1.0 | January 2026 | Architecture Team | Initial version |

---

**Document Status:** Approved

**Approval Signatures:**

| Role | Name | Signature | Date |
|:-----|:-----|:----------|:-----|
| Technical Architect | | | |
| Project Manager | | | |
| Product Owner | | | |
| Security Lead | | | |
