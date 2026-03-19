---
name: DAR Cognito API Gateway
overview: Create a comprehensive DAR (Decision Analysis and Resolution) document for AWS Cognito and API Gateway selection, following the same template structure as the OpenSearch DAR document.
todos:
  - id: create-dar
    content: Create DAR-Cognito-APIGateway-Selection.md document
    status: pending
---

# DAR Document for AWS Cognito + API Gateway

## Document to Create

**File:** `docs/DAR-Cognito-APIGateway-Selection.md`

## Document Structure (Following OpenSearch DAR Template)

### 1. Introduction

- Objective: Evaluate authentication and API management solutions for AmCart
- Scope: User authentication, MFA, social login, API routing, rate limiting
- Out of Scope: Authorization logic within services, business rules

### 2. Requirements at a Glance

**Functional Requirements:**| ID | Requirement | Priority ||----|-------------|----------|| FR-01 | Email/Password Authentication | Critical || FR-02 | Social Login (Google, Facebook) | High || FR-03 | Multi-Factor Authentication | High || FR-04 | Password Reset Flow | High || FR-05 | Email Verification | High || FR-06 | Token-based Sessions (JWT) | Critical || FR-07 | API Rate Limiting | Medium || FR-08 | API Request Validation | Medium |**Non-Functional Requirements:**| ID | Requirement | Target ||----|-------------|--------|| NFR-01 | Login Latency | < 500ms || NFR-02 | Availability | 99.9% || NFR-03 | Concurrent Users | 10,000+ || NFR-04 | Token Refresh | Seamless |

### 3. Available Tools

**3.1 AWS Cognito + API Gateway**

- Features: User pools, identity providers, MFA, hosted UI, JWT tokens
- Pricing: $0.0055/MAU after 50K free tier

**3.2 Auth0**

- Features: Universal login, social connections, MFA, extensibility
- Pricing: $23/month for 1K MAU (Professional)

**3.3 Keycloak (Self-hosted)**

- Features: Open-source, SSO, social login, LDAP
- Pricing: Free (infrastructure costs)

**3.4 Firebase Authentication**

- Features: Simple SDK, social login, phone auth
- Pricing: Free for auth, Spark plan limitations

**3.5 Custom JWT (.NET Identity)**

- Features: Full control, .NET native
- Pricing: Development cost only

### 4. Comparison Analysis

**4.1 Point Matrix:**| Criteria | Weight | Cognito | Auth0 | Keycloak | Firebase | Custom ||----------|--------|---------|-------|----------|----------|--------|| AWS Integration | 20% | 10 | 6 | 5 | 4 | 7 || Social Login | 15% | 9 | 10 | 8 | 9 | 5 || MFA Support | 15% | 9 | 10 | 8 | 7 | 4 || Cost | 15% | 9 | 5 | 8 | 8 | 10 || Ease of Setup | 15% | 8 | 9 | 5 | 9 | 3 || .NET Support | 10% | 8 | 8 | 7 | 6 | 10 || Scalability | 10% | 10 | 9 | 7 | 9 | 6 || **Weighted Score** | 100% | **8.95** | 7.85 | 6.75 | 7.40 | 6.15 |**4.2 Feature Comparison Table4.3 Cost Comparison (100K MAU scenario)4.4 Security Comparison**

### 5. Recommendation

- Selected: AWS Cognito + API Gateway
- Justification: Native AWS integration, cost-effective, managed MFA, social login support

### 6. Assumptions

- AWS remains primary cloud provider
- 100K MAU within first year
- Team can learn Amplify SDK

### 7. Risks

- Vendor lock-in to AWS
- Limited UI customization
- Cold start latency for Lambda triggers
- Complex token refresh handling

### 8. Appendix

- References (AWS docs, pricing pages)
- Glossary (MAU, JWT, OAuth, OIDC)
- Revision history

---

## Key Sections Unique to This DAR

### API Gateway Evaluation

Compare:

- AWS API Gateway vs Kong vs Nginx vs Ocelot (.NET)

### Integration Architecture Diagram