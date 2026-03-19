# DECISION ANALYSIS AND RESOLUTION (DAR) DOCUMENT

## Authentication & API Management - AWS Cognito + API Gateway

---

## Document Control Information

| Field | Value |
|:------|:------|
| **Document Title** | Authentication & API Management - AWS Cognito + API Gateway |
| **Document ID** | DAR-AMCART-AUTH-001 |
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
3. [Available Tools](#3-available-tools)
   - 3.1 [AWS Cognito + API Gateway](#31-aws-cognito--api-gateway)
   - 3.2 [Auth0](#32-auth0)
   - 3.3 [Keycloak](#33-keycloak)
   - 3.4 [Firebase Authentication](#34-firebase-authentication)
   - 3.5 [Custom JWT Implementation](#35-custom-jwt-implementation)
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

---

## 1. Introduction

### 1.1 Objective and Scope of Document

#### Objective

This Decision Analysis and Resolution (DAR) document evaluates and compares available authentication and API management solutions for the AmCart ecommerce platform. The objective is to select the most suitable solution that meets the project's security requirements, user experience expectations, scalability needs, and integration capabilities with the existing AWS infrastructure.

#### Scope

This document covers:

- User authentication mechanisms (email/password, social login)
- Multi-Factor Authentication (MFA) implementation
- Token-based session management (JWT)
- API Gateway for request routing and protection
- Rate limiting and throttling
- Integration with .NET 8 microservices on AWS EKS
- User data synchronization with PostgreSQL

#### Out of Scope

- Authorization logic within microservices (handled by services)
- Business rules for access control
- Payment security (covered separately)
- Admin authentication (separate admin pool consideration)

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
| FR-09 | API Rate Limiting | Medium | Protect APIs from abuse (1000 req/s default) |
| FR-10 | Request Validation | Medium | Validate API request structure |
| FR-11 | Remember Me | Medium | Extended session duration option |
| FR-12 | Account Lockout | Medium | Lock account after failed attempts |

### Non-Functional Requirements

| ID | Requirement | Target | Description |
|:---|:------------|:-------|:------------|
| NFR-01 | Login Latency | < 500ms | P95 authentication response time |
| NFR-02 | Availability | 99.9% | Authentication service uptime |
| NFR-03 | Concurrent Users | 10,000+ | Simultaneous authenticated sessions |
| NFR-04 | Token Expiry | 1 hour | Access token lifetime |
| NFR-05 | Refresh Token Expiry | 30 days | Refresh token lifetime |
| NFR-06 | MFA Latency | < 2s | Time to send/verify MFA code |
| NFR-07 | Scalability | Auto | Handle traffic spikes automatically |
| NFR-08 | Compliance | SOC 2 | Security compliance requirements |

### Technical Constraints

| Constraint | Description |
|:-----------|:------------|
| Cloud Provider | AWS (existing infrastructure) |
| Backend Technology | .NET 8 / C# microservices |
| Frontend Technology | Nuxt.js 3 / Vue.js |
| Existing Services | EKS, RDS PostgreSQL, ElastiCache |
| Budget | Cost-effective, avoid high per-user fees |
| Team Expertise | .NET experience, limited OAuth experience |
| Integration | Must sync user data to PostgreSQL |

---

## 3. Available Tools

### 3.1 AWS Cognito + API Gateway

#### Overview

AWS Cognito is a fully managed identity service that provides user sign-up, sign-in, and access control. Combined with AWS API Gateway, it offers a complete solution for authentication and API management within the AWS ecosystem.

#### 3.1.1 Features

| Feature | Details |
|:--------|:--------|
| **User Pools** | Managed user directory with customizable attributes |
| **Identity Providers** | Google, Facebook, Apple, SAML, OIDC federation |
| **MFA** | SMS and TOTP (Authenticator apps) support |
| **Hosted UI** | Pre-built login/signup pages (customizable) |
| **Custom UI** | Full SDK support for custom authentication flows |
| **JWT Tokens** | Industry-standard ID, Access, and Refresh tokens |
| **Lambda Triggers** | Pre/post authentication hooks for customization |
| **Password Policies** | Configurable password requirements |
| **Account Recovery** | Email and phone-based recovery options |
| **Advanced Security** | Adaptive authentication, compromised credential checks |
| **API Gateway Integration** | Native Cognito Authorizer for JWT validation |
| **Rate Limiting** | Built-in throttling at API Gateway level |
| **WAF Integration** | AWS WAF for DDoS and attack protection |
| **SDK Support** | Amplify SDK for JavaScript, iOS, Android, .NET |

#### 3.1.2 Pricing

| Component | Pricing Model |
|:----------|:--------------|
| **Cognito User Pool** | Per Monthly Active User (MAU) |
| **API Gateway** | Per million requests + data transfer |

**Cognito Pricing (MAU-based):**

| MAU Tier | Price per MAU |
|:---------|:--------------|
| First 50,000 | Free |
| 50,001 - 100,000 | $0.0055 |
| 100,001 - 1,000,000 | $0.0046 |
| 1,000,001 - 10,000,000 | $0.00325 |

**API Gateway Pricing:**

| Component | Price |
|:----------|:------|
| REST API Requests | $3.50 per million |
| HTTP API Requests | $1.00 per million |
| WebSocket Messages | $1.00 per million |
| Caching (per hour) | $0.02 - $3.80 |

**Example Monthly Cost (100K MAU, 10M API calls):**

| Item | Cost |
|:-----|:-----|
| Cognito (50K free + 50K × $0.0055) | $275 |
| API Gateway HTTP (10M × $1.00/M) | $10 |
| **Total** | **~$285/month** |

---

### 3.2 Auth0

#### Overview

Auth0 is a leading identity-as-a-service (IDaaS) platform offering universal authentication with extensive customization options and excellent developer experience.

#### 3.2.1 Features

| Feature | Details |
|:--------|:--------|
| **Universal Login** | Centralized, customizable login experience |
| **Social Connections** | 30+ social identity providers |
| **MFA** | Push notifications, SMS, TOTP, WebAuthn |
| **Passwordless** | Magic links, biometrics |
| **Rules & Actions** | Extensible authentication pipeline |
| **Branding** | Full UI customization |
| **Anomaly Detection** | Brute force and breached password detection |
| **SSO** | Enterprise Single Sign-On |
| **Machine-to-Machine** | Service authentication |
| **SDKs** | Comprehensive SDK support including .NET |

#### 3.2.2 Pricing

| Plan | MAU Limit | Monthly Cost |
|:-----|:----------|:-------------|
| Free | 7,500 | $0 |
| Essentials | 500 - 5,000 | $23 - $240 |
| Professional | 1,000 - 10,000 | $240 - $1,000+ |
| Enterprise | Unlimited | Custom (typically $50K+/year) |

**Note:** Social connections and MFA may have additional costs on lower tiers.

---

### 3.3 Keycloak

#### Overview

Keycloak is an open-source identity and access management solution by Red Hat. It provides SSO, identity brokering, and user federation capabilities.

#### 3.3.1 Features

| Feature | Details |
|:--------|:--------|
| **Open Source** | Apache 2.0 license, self-hosted |
| **Identity Brokering** | Social login, SAML, OIDC providers |
| **User Federation** | LDAP, Active Directory integration |
| **SSO** | Single Sign-On across applications |
| **Admin Console** | Web-based administration |
| **Themes** | Customizable login pages |
| **MFA** | OTP, WebAuthn support |
| **Fine-Grained Authorization** | Policy-based access control |
| **Clustering** | High availability support |

#### 3.3.2 Pricing

| Deployment | Pricing Model |
|:-----------|:--------------|
| **Self-Hosted** | Free (open-source) |
| **Infrastructure** | EC2/EKS costs (~$100-300/month) |
| **Red Hat SSO** | Commercial support available |

**Hidden Costs:**
- DevOps effort for deployment and maintenance
- Security patching responsibility
- Scaling and monitoring setup

---

### 3.4 Firebase Authentication

#### Overview

Firebase Authentication provides backend services and SDKs for authenticating users. Part of Google's Firebase platform, it's optimized for mobile and web applications.

#### 3.4.1 Features

| Feature | Details |
|:--------|:--------|
| **Email/Password** | Standard authentication |
| **Social Login** | Google, Facebook, Twitter, GitHub, Apple |
| **Phone Auth** | SMS-based authentication |
| **Anonymous Auth** | Guest user sessions |
| **Custom Auth** | Integration with existing systems |
| **SDKs** | iOS, Android, Web, Unity, C++ |
| **Security Rules** | Declarative security for Firestore/RTDB |

#### 3.4.2 Pricing

| Feature | Spark (Free) | Blaze (Pay-as-you-go) |
|:--------|:-------------|:----------------------|
| Email/Password | Unlimited | Unlimited |
| Social Login | Unlimited | Unlimited |
| Phone Auth | 10K/month | $0.01 - $0.06 per verification |
| Anonymous | Unlimited | Unlimited |

**Limitations:**
- Limited MFA options (phone-based only until recently)
- No native SAML/enterprise SSO
- Primarily Google Cloud ecosystem

---

### 3.5 Custom JWT Implementation

#### Overview

Building a custom authentication system using .NET Identity or custom JWT implementation provides maximum control but requires significant development effort.

#### 3.5.1 Features

| Feature | Details |
|:--------|:--------|
| **Full Control** | Complete customization of authentication flow |
| **No Vendor Lock-in** | Own the entire solution |
| **Integration** | Native .NET integration |
| **Cost** | No per-user licensing fees |
| **Compliance** | Full control over data residency |

#### 3.5.2 Pricing

| Component | Cost |
|:----------|:-----|
| **Development** | 2-4 developer months (~$40K-80K) |
| **Infrastructure** | EC2/EKS costs (~$50-150/month) |
| **Maintenance** | Ongoing security updates |
| **Total Year 1** | ~$50K-100K |

**Risks:**
- Security vulnerabilities from custom implementation
- No built-in MFA (must implement)
- Social login OAuth complexity
- Ongoing maintenance burden

---

## 4. Comparison Analysis

### 4.1 Point Matrix

| Criteria | Max Points | Cognito+APIGW | Auth0 | Keycloak | Firebase | Custom |
|:---------|:----------:|:-------------:|:-----:|:--------:|:--------:|:------:|
| **AWS Integration** | 20 | 20 | 12 | 10 | 8 | 14 |
| **Social Login** | 15 | 14 | 15 | 12 | 14 | 8 |
| **MFA Support** | 15 | 14 | 15 | 12 | 9 | 6 |
| **Cost Effectiveness** | 15 | 14 | 8 | 11 | 12 | 9 |
| **Ease of Setup** | 10 | 8 | 9 | 5 | 9 | 3 |
| **.NET Support** | 10 | 8 | 8 | 7 | 6 | 10 |
| **Scalability** | 10 | 10 | 9 | 6 | 9 | 5 |
| **Security Features** | 5 | 5 | 5 | 4 | 4 | 3 |
| **Total Score** | **100** | **93** | 81 | 67 | 71 | 58 |

**Scoring:** Points awarded out of max for each criterion (higher = better)

**Scoring Legend:** 1-10 (10 = Best)

### 4.2 Feature Comparison

| Feature | Cognito+APIGW | Auth0 | Keycloak | Firebase | Custom |
|:--------|:--------------|:------|:---------|:---------|:-------|
| Email/Password | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Google OAuth | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ⚠️ Manual |
| Facebook OAuth | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ⚠️ Manual |
| MFA - SMS | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ❌ Build |
| MFA - TOTP | ✅ Yes | ✅ Yes | ✅ Yes | ⚠️ Limited | ❌ Build |
| MFA - Push | ❌ No | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Passwordless | ⚠️ Limited | ✅ Yes | ⚠️ Limited | ✅ Yes | ❌ Build |
| JWT Tokens | ✅ Native | ✅ Native | ✅ Native | ✅ Native | ✅ Build |
| Lambda/Hooks | ✅ Triggers | ✅ Actions | ✅ SPI | ⚠️ Functions | ✅ Code |
| Rate Limiting | ✅ APIGW | ⚠️ Paid | ❌ External | ❌ External | ❌ Build |
| WAF Protection | ✅ AWS WAF | ❌ No | ❌ External | ❌ No | ❌ External |
| Managed Service | ✅ Yes | ✅ Yes | ❌ Self-host | ✅ Yes | ❌ Self-host |
| AWS Native | ✅ Yes | ❌ No | ❌ No | ❌ No | ✅ Deploy |

### 4.3 Cost Comparison

**Scenario:** 100,000 Monthly Active Users, 10 Million API Requests/month

| Solution | Monthly Cost | Year 1 Total | Notes |
|:---------|:-------------|:-------------|:------|
| **Cognito + APIGW** | ~$285 | ~$3,420 | 50K free tier |
| **Auth0 Professional** | ~$1,000+ | ~$12,000+ | Per-MAU pricing |
| **Keycloak** | ~$200 | ~$2,400 + setup | Infrastructure only |
| **Firebase** | ~$100 | ~$1,200 | Phone auth costs extra |
| **Custom JWT** | ~$100 | ~$60,000+ | Development costs |

**5-Year TCO Analysis:**

| Solution | Infrastructure | Licensing | Development | Operations | Total 5-Year |
|:---------|:---------------|:----------|:------------|:-----------|:-------------|
| **Cognito + APIGW** | $5,000 | $12,000 | $5,000 | $3,000 | **$25,000** |
| **Auth0** | $0 | $60,000 | $3,000 | $2,000 | $65,000 |
| **Keycloak** | $12,000 | $0 | $15,000 | $20,000 | $47,000 |
| **Firebase** | $0 | $6,000 | $8,000 | $3,000 | $17,000 |
| **Custom** | $6,000 | $0 | $80,000 | $30,000 | $116,000 |

### 4.4 Security Comparison

| Security Feature | Cognito+APIGW | Auth0 | Keycloak | Firebase | Custom |
|:-----------------|:--------------|:------|:---------|:---------|:-------|
| SOC 2 Compliant | ✅ Yes | ✅ Yes | ⚠️ Self | ✅ Yes | ⚠️ Self |
| GDPR Ready | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ⚠️ Build |
| Encryption at Rest | ✅ Yes | ✅ Yes | ⚠️ Config | ✅ Yes | ⚠️ Config |
| Encryption in Transit | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Brute Force Protection | ✅ Built-in | ✅ Built-in | ✅ Built-in | ⚠️ Limited | ❌ Build |
| Compromised Credential Check | ✅ Advanced | ✅ Yes | ❌ No | ❌ No | ❌ No |
| IP Blocking | ✅ WAF | ✅ Paid | ⚠️ Manual | ❌ No | ⚠️ Manual |
| DDoS Protection | ✅ Shield | ⚠️ Limited | ❌ External | ⚠️ Limited | ❌ External |
| Audit Logs | ✅ CloudTrail | ✅ Yes | ✅ Yes | ⚠️ Limited | ⚠️ Build |

---

## 5. Recommendation

### Selected Solution: AWS Cognito + API Gateway

Based on the comprehensive evaluation, **AWS Cognito combined with AWS API Gateway** is recommended as the authentication and API management solution for the AmCart ecommerce platform.

### Justification

| Factor | Rationale |
|:-------|:----------|
| **AWS Native Integration** | Seamless integration with existing AWS infrastructure (EKS, RDS, CloudWatch) |
| **Cost-Effective** | 50,000 MAU free tier; ~$285/month for 100K MAU vs $1,000+ for Auth0 |
| **Managed MFA** | Built-in SMS and TOTP support without additional development |
| **Social Login** | Native Google and Facebook OAuth integration |
| **API Protection** | API Gateway provides rate limiting, WAF, and request validation |
| **Lambda Triggers** | Customizable authentication flows via pre/post hooks |
| **Security** | SOC 2 compliant, compromised credential detection, AWS Shield |
| **Scalability** | Auto-scaling to millions of users without infrastructure changes |
| **Operational Simplicity** | No servers to manage, automatic patching and updates |

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              AmCart Authentication Architecture              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌─────────────┐         ┌─────────────────────────────────┐              │
│   │   Nuxt.js   │         │        AWS Cognito              │              │
│   │   Frontend  │────────▶│  ┌─────────────────────────┐    │              │
│   │  (Amplify)  │         │  │      User Pool          │    │              │
│   └─────────────┘         │  │  - Email/Password       │    │              │
│         │                 │  │  - Google OAuth         │    │              │
│         │                 │  │  - Facebook OAuth       │    │              │
│         │                 │  │  - MFA (SMS/TOTP)       │    │              │
│         │                 │  └─────────────────────────┘    │              │
│         │                 │              │                   │              │
│         │                 │              ▼                   │              │
│         │                 │  ┌─────────────────────────┐    │              │
│         │                 │  │    Lambda Triggers      │    │              │
│         │                 │  │  - Post Confirmation    │────┼──────┐       │
│         │                 │  │  - Pre Token Generation │    │      │       │
│         │                 │  └─────────────────────────┘    │      │       │
│         │                 └─────────────────────────────────┘      │       │
│         │                                                          │       │
│         ▼                                                          ▼       │
│   ┌─────────────────────────────────────┐                 ┌─────────────┐  │
│   │         AWS API Gateway             │                 │    User     │  │
│   │  ┌───────────────────────────────┐  │                 │   Service   │  │
│   │  │     Cognito Authorizer        │  │                 │   (.NET)    │  │
│   │  │  - JWT Validation             │  │                 └──────┬──────┘  │
│   │  │  - Token Claims Extraction    │  │                        │         │
│   │  └───────────────────────────────┘  │                        ▼         │
│   │  ┌───────────────────────────────┐  │                 ┌─────────────┐  │
│   │  │     Rate Limiting             │  │                 │  PostgreSQL │  │
│   │  │  - 1000 req/sec default       │  │                 │   (Users)   │  │
│   │  │  - Per-user quotas            │  │                 └─────────────┘  │
│   │  └───────────────────────────────┘  │                                  │
│   │  ┌───────────────────────────────┐  │                                  │
│   │  │     AWS WAF                   │  │                                  │
│   │  │  - SQL injection protection   │  │                                  │
│   │  │  - XSS protection             │  │                                  │
│   │  │  - IP blocking                │  │                                  │
│   │  └───────────────────────────────┘  │                                  │
│   └──────────────────┬──────────────────┘                                  │
│                      │                                                      │
│                      ▼                                                      │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                    EKS Cluster (Nginx Ingress)                       │  │
│   │  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐   │  │
│   │  │  User   │  │ Product │  │  Cart   │  │  Order  │  │ Payment │   │  │
│   │  │ Service │  │ Service │  │ Service │  │ Service │  │ Service │   │  │
│   │  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘   │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Configuration Recommendations

#### Cognito User Pool Settings

| Setting | Value | Reason |
|:--------|:------|:-------|
| Sign-in Identifier | Email | Standard for ecommerce |
| Password Minimum Length | 8 characters | Balance security/usability |
| Password Requirements | Upper, lower, number, symbol | Strong passwords |
| MFA | Optional | User choice, not forced |
| MFA Methods | TOTP, SMS | Flexibility |
| Account Recovery | Email only | Simpler UX |
| Email Verification | Required | Prevent fake accounts |
| Self-Registration | Enabled | Customer signup |

#### API Gateway Configuration

| Setting | Development | Production |
|:--------|:------------|:-----------|
| API Type | HTTP API | HTTP API |
| Throttling (Default) | 100 req/s | 1000 req/s |
| Burst Limit | 200 | 2000 |
| Caching | Disabled | 5 min (GET) |
| WAF | Disabled | Enabled |
| Logging | Full | Errors only |

---

## 6. Assumptions

| ID | Assumption | Impact if Invalid |
|:---|:-----------|:------------------|
| A1 | Monthly Active Users will not exceed 500,000 in 3 years | Pricing may change significantly |
| A2 | AWS remains the primary cloud provider | Would need to reconsider Auth0 or Keycloak |
| A3 | Team can learn AWS Amplify SDK within 2 weeks | May delay frontend integration |
| A4 | Social login (Google/Facebook) covers 80% of social auth needs | May need to add more providers |
| A5 | SMS MFA costs are acceptable (~$0.05/SMS) | May switch to TOTP-only |
| A6 | Lambda cold starts (200-500ms) are acceptable | May need Provisioned Concurrency |
| A7 | User data sync latency (< 2s) is acceptable | May need synchronous approach |
| A8 | Cognito Hosted UI customization is sufficient | May need fully custom UI |

---

## 7. Risks

| ID | Risk | Probability | Impact | Mitigation |
|:---|:-----|:------------|:-------|:-----------|
| R1 | Vendor lock-in to AWS Cognito | Medium | Medium | Keep user data in PostgreSQL; use standard JWT; document exit strategy |
| R2 | Limited Cognito UI customization | Medium | Low | Use Amplify SDK with custom Nuxt.js UI instead of Hosted UI |
| R3 | Lambda trigger cold starts | Medium | Low | Use Provisioned Concurrency for critical triggers; optimize code size |
| R4 | SMS MFA costs escalate | Low | Medium | Promote TOTP (free) over SMS; set SMS budget alerts |
| R5 | Token refresh complexity | Medium | Medium | Use Amplify SDK which handles refresh automatically |
| R6 | User migration complexity | Low | High | Plan incremental migration; support dual auth during transition |
| R7 | API Gateway costs at scale | Low | Medium | Use HTTP APIs (cheaper); implement caching; optimize request patterns |
| R8 | Social provider changes OAuth policies | Low | Medium | Monitor provider announcements; maintain fallback to email auth |
| R9 | Cognito service limits | Low | Low | Request limit increases proactively; implement retry logic |
| R10 | Security misconfiguration | Medium | High | Follow AWS Well-Architected; regular security audits; enable CloudTrail |

### Risk Response Plan

| Risk | Response Strategy | Owner |
|:-----|:------------------|:------|
| R1 | Accept - Document migration path to Auth0/Keycloak | Architect |
| R2 | Mitigate - Build custom UI from start | Frontend Lead |
| R3 | Mitigate - Implement Provisioned Concurrency | DevOps |
| R4 | Mitigate - Default to TOTP; SMS as backup | Product Owner |
| R5 | Mitigate - Use Amplify SDK; thorough testing | Frontend Lead |
| R6 | Mitigate - Detailed migration plan; parallel running | Tech Lead |
| R7 | Mitigate - Cost monitoring; HTTP API usage | DevOps |
| R8 | Accept - Monitor; email auth as fallback | Tech Lead |
| R9 | Mitigate - Pre-request limit increases | DevOps |
| R10 | Mitigate - Security review; AWS Config rules | Security Lead |

---

## 8. Appendix

### 8.1 References

| # | Reference | URL |
|:--|:----------|:----|
| 1 | AWS Cognito Developer Guide | https://docs.aws.amazon.com/cognito/latest/developerguide/ |
| 2 | AWS API Gateway Documentation | https://docs.aws.amazon.com/apigateway/latest/developerguide/ |
| 3 | Cognito Pricing | https://aws.amazon.com/cognito/pricing/ |
| 4 | API Gateway Pricing | https://aws.amazon.com/api-gateway/pricing/ |
| 5 | AWS Amplify for .NET | https://docs.amplify.aws/ |
| 6 | Cognito Lambda Triggers | https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-user-identity-pools-working-with-aws-lambda-triggers.html |
| 7 | Auth0 Pricing | https://auth0.com/pricing |
| 8 | Keycloak Documentation | https://www.keycloak.org/documentation |
| 9 | Firebase Authentication | https://firebase.google.com/docs/auth |
| 10 | OWASP Authentication Cheat Sheet | https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html |
| 11 | JWT Best Practices | https://datatracker.ietf.org/doc/html/rfc8725 |
| 12 | OAuth 2.0 Specification | https://oauth.net/2/ |

### 8.2 Glossary

| Term | Definition |
|:-----|:-----------|
| **MAU** | Monthly Active Users - unique users who authenticate in a month |
| **JWT** | JSON Web Token - compact, URL-safe token format |
| **OAuth 2.0** | Authorization framework for third-party access |
| **OIDC** | OpenID Connect - identity layer on top of OAuth 2.0 |
| **MFA** | Multi-Factor Authentication - additional verification step |
| **TOTP** | Time-based One-Time Password (e.g., Google Authenticator) |
| **IdP** | Identity Provider - service that manages user identities |
| **User Pool** | Cognito user directory and authentication service |
| **Identity Pool** | Cognito service for AWS credential federation |
| **Lambda Trigger** | Serverless function executed during auth events |
| **Hosted UI** | Pre-built authentication pages provided by Cognito |
| **WAF** | Web Application Firewall - protects against web exploits |

### 8.3 Authentication Flow Diagrams

#### Email/Password Registration Flow

```
User                Frontend             Cognito              Lambda               User Service
 │                     │                    │                    │                      │
 │──Register Form─────▶│                    │                    │                      │
 │                     │──signUp()─────────▶│                    │                      │
 │                     │                    │──Create User──────▶│                      │
 │                     │                    │                    │                      │
 │                     │                    │◀─User Created──────│                      │
 │                     │                    │                    │                      │
 │                     │◀─Confirm Email────│                    │                      │
 │◀─Verification Email─│                    │                    │                      │
 │                     │                    │                    │                      │
 │──Click Link────────▶│                    │                    │                      │
 │                     │──confirmSignUp()──▶│                    │                      │
 │                     │                    │──Post Confirm─────▶│                      │
 │                     │                    │                    │──Sync User──────────▶│
 │                     │                    │                    │◀─Created────────────│
 │                     │                    │◀─Success───────────│                      │
 │                     │◀─Account Active───│                    │                      │
 │◀─Success────────────│                    │                    │                      │
```

#### Social Login Flow

```
User                Frontend             Cognito              Google/FB            User Service
 │                     │                    │                    │                      │
 │──Click Google──────▶│                    │                    │                      │
 │                     │──federatedSignIn()▶│                    │                      │
 │                     │                    │──Redirect─────────▶│                      │
 │◀─────────────────────────────────────────────────────────────│                      │
 │──Authorize─────────▶│                    │                    │                      │
 │                     │                    │◀─OAuth Token───────│                      │
 │                     │                    │──Create/Link User──│                      │
 │                     │                    │──Post Auth────────▶│                      │
 │                     │                    │                    │──Sync User──────────▶│
 │                     │                    │◀─────────────────────────────────────────│
 │                     │◀─JWT Tokens────────│                    │                      │
 │◀─Logged In──────────│                    │                    │                      │
```

### 8.4 Revision History

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

