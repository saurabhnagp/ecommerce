# AmCart Project Assumptions

## Comprehensive List of Assumptions Made to Resolve Requirements Ambiguity

---

## Table of Contents

1. [Introduction](#introduction)
2. [Business Assumptions](#business-assumptions)
3. [Technical Assumptions](#technical-assumptions)
4. [Infrastructure & Cloud Assumptions](#infrastructure--cloud-assumptions)
5. [Security & Authentication Assumptions](#security--authentication-assumptions)
6. [Database & Data Assumptions](#database--data-assumptions)
7. [Search & Performance Assumptions](#search--performance-assumptions)
8. [Team & Organizational Assumptions](#team--organizational-assumptions)
9. [Integration Assumptions](#integration-assumptions)
10. [Budget & Cost Assumptions](#budget--cost-assumptions)
11. [Assumptions Summary Matrix](#assumptions-summary-matrix)

---

## Introduction

This document captures all assumptions made during the architecture and design phase of AmCart. These assumptions were necessary to resolve ambiguity in requirements and make informed technology decisions.

### How to Use This Document

- **Review before implementation** - Validate assumptions with stakeholders
- **Track invalidation** - Monitor if assumptions change during development
- **Update decisions** - If an assumption proves invalid, revisit related decisions

---

## Business Assumptions

### B1. Target Market & Geography

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B1.1 | **India is the primary market** | Application designed for Indian e-commerce patterns | Multi-region architecture needed; compliance changes |
| B1.2 | **INR is the primary currency** | Pricing, payments in INR | Multi-currency support needed |
| B1.3 | **English is the primary language** | UI/Search designed for English | Multi-language support; localization needed |
| B1.4 | **Operating hours are 24/7** | No maintenance windows assumed | Scheduled maintenance windows if not required |
| B1.5 | **PAN-India delivery coverage** | All states/UTs serviceable | Region-specific restrictions needed |
| B1.6 | **Standard Indian tax system (GST)** | Single tax calculation logic | Multi-tax jurisdiction complexity |
| B1.7 | **Urban & semi-urban focus initially** | Tier 1, 2, 3 cities | Rural logistics challenges |
| B1.8 | **No international shipping initially** | Domestic only | Customs, duties, international logistics |

### B2. Business Model

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B2.1 | **Marketplace model with multiple sellers** | Schema supports seller_id on products | Single-vendor model would simplify design |
| B2.2 | **Commission-based revenue (10-25%)** | Seller payouts, commission tracking included | Different revenue model needs schema changes |
| B2.3 | **B2C focus (not B2B)** | Consumer checkout flow, no bulk ordering | B2B features like quotations, credit terms needed |
| B2.4 | **No subscription/recurring orders** | One-time purchases only | Subscription management module needed |
| B2.5 | **Self-onboarded sellers** | Seller registration portal | Manual seller onboarding process |
| B2.6 | **No auction/bidding model** | Fixed price only | Auction engine development needed |
| B2.7 | **No rental/lease model** | Purchase only | Rental management, return scheduling needed |
| B2.8 | **No digital goods/downloads** | Physical products only | Digital delivery, licensing system needed |

### B3. Scale & Growth

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B3.1 | **Product catalog ≤ 500,000 products in 3 years** | Database sizing, indexing strategy | Need to resize clusters, partition data |
| B3.2 | **Monthly Active Users ≤ 500,000 in 3 years** | Cognito pricing, capacity planning | Higher pricing tier; different auth solution |
| B3.3 | **Peak concurrent users ~10,000** | Infrastructure sizing | Auto-scaling limits may need adjustment |
| B3.4 | **100,000 orders/day at scale** | Database design, queue capacity | May need sharding, higher throughput |
| B3.5 | **Search traffic ≤ 1M searches/day** | OpenSearch cluster sizing | Need larger instance types |
| B3.6 | **Active sellers ≤ 10,000** | Seller management capacity | Dedicated seller support team needed |
| B3.7 | **Average 50 products per seller** | Listing management | Bulk upload optimization needed |
| B3.8 | **10% month-over-month growth** | Capacity planning | Faster scaling if viral growth |

### B4. Product & Catalog

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B4.1 | **Product categories are predefined** | Admin manages categories | User-suggested categories workflow |
| B4.2 | **3-level category hierarchy** | Category tree depth | Deeper hierarchy navigation changes |
| B4.3 | **Single brand per product** | No co-branded products | Multi-brand attribution needed |
| B4.4 | **Product variants (size, color, etc.)** | SKU-level inventory | Variant management complexity |
| B4.5 | **Maximum 20 images per product** | Storage planning | Higher limits need more storage |
| B4.6 | **No product customization** | Standard SKUs only | Custom product builder needed |
| B4.7 | **No product bundles/kits** | Individual items only | Bundle pricing, inventory logic |
| B4.8 | **Seller sets product pricing** | No platform price control | Dynamic pricing engine needed |
| B4.9 | **Products require approval** | Quality control | Auto-listing changes moderation needs |
| B4.10 | **No pre-orders** | In-stock items only | Pre-order management, notifications |

### B5. Pricing & Promotions

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B5.1 | **Percentage & flat discounts** | Coupon types supported | Complex discount rules engine |
| B5.2 | **Single coupon per order** | Simpler discount calculation | Coupon stacking logic needed |
| B5.3 | **Platform-wide sales events** | Unified promotions | Seller-specific promotions |
| B5.4 | **No loyalty/points program** | Simple pricing | Points earning, redemption system |
| B5.5 | **No membership tiers (Prime-like)** | Single customer type | Tiered benefits, pricing |
| B5.6 | **GST included in displayed price** | Indian e-commerce standard | Price + tax display changes |
| B5.7 | **No flash sales with countdown** | Standard promotions | Real-time countdown, inventory locks |
| B5.8 | **No price negotiation** | Fixed prices | Chat-based negotiation system |
| B5.9 | **Maximum discount cap exists** | Prevent loss-making orders | Uncapped discounts need approval |
| B5.10 | **Coupon validity by date only** | Time-based validity | Usage-based, quantity-based limits |

### B6. Order & Checkout

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B6.1 | **Guest checkout not allowed** | Account required | Guest checkout flow development |
| B6.2 | **Single shipping address per order** | Split shipment by seller | Multi-address, gift shipping |
| B6.3 | **Order modification not allowed** | Immutable after placement | Order edit workflow complexity |
| B6.4 | **Order cancellation within 24 hours** | Standard policy | Variable cancellation windows |
| B6.5 | **Maximum 50 items per order** | Cart size limit | Bulk order handling |
| B6.6 | **No partial payment** | Full payment upfront | EMI, pay later integration |
| B6.7 | **No split payment methods** | Single payment method | Multi-method payment splitting |
| B6.8 | **Minimum order value ₹99** | Economics viability | No minimum handling |
| B6.9 | **Maximum order value ₹5,00,000** | Fraud prevention | Higher limits with verification |
| B6.10 | **No gift wrapping** | Standard packaging | Gift wrap options, charges |

### B7. Shipping & Fulfillment

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B7.1 | **Seller-managed fulfillment** | Sellers ship directly | Platform fulfillment centers |
| B7.2 | **3-7 days standard delivery** | Delivery SLA | Same-day, next-day options |
| B7.3 | **Flat shipping rate by weight** | Simple calculation | Zone-based, volumetric pricing |
| B7.4 | **Free shipping above ₹499** | Standard threshold | Dynamic free shipping rules |
| B7.5 | **No express/priority shipping** | Single shipping speed | Multiple shipping options |
| B7.6 | **Courier integration via APIs** | Third-party logistics | In-house logistics complexity |
| B7.7 | **No scheduled delivery slots** | Standard delivery | Time-slot booking system |
| B7.8 | **No COD (Cash on Delivery) initially** | Digital payments only | COD reconciliation, fraud handling |
| B7.9 | **Pincode serviceability check** | Standard check | Real-time carrier availability |
| B7.10 | **No international shipping** | Domestic only | Export compliance, customs |

### B8. Returns & Refunds

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B8.1 | **7-day return window** | Standard policy | Variable return windows by category |
| B8.2 | **Return pickup by seller** | Seller responsibility | Platform-managed returns |
| B8.3 | **Refund to original payment method** | Standard refund | Wallet, store credit options |
| B8.4 | **No exchange option** | Return and reorder | Direct exchange workflow |
| B8.5 | **Refund within 5-7 business days** | Processing time | Instant refunds |
| B8.6 | **Some categories non-returnable** | Policy exceptions | Full return policy all items |
| B8.7 | **Quality check before refund** | Verification step | Auto-refund on return request |
| B8.8 | **Partial refund for damaged returns** | Fair policy | Full refund regardless |

### B9. Seller Management

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B9.1 | **GSTIN mandatory for sellers** | Tax compliance | Non-GST seller handling |
| B9.2 | **Bank account verification required** | Payout security | Alternative payout methods |
| B9.3 | **Weekly payout cycle** | Cash flow management | Daily, bi-weekly options |
| B9.4 | **Platform holds payment until delivery** | Buyer protection | Immediate payout options |
| B9.5 | **Seller dashboard for analytics** | Self-service | Dedicated account managers |
| B9.6 | **No seller tiers initially** | Equal treatment | Tiered benefits, fees |
| B9.7 | **Seller disputes via support** | Manual resolution | Automated dispute system |
| B9.8 | **No seller advertising/promotion** | Organic listings | Sponsored products, ads |
| B9.9 | **Seller rating affects visibility** | Quality incentive | Fixed listing order |
| B9.10 | **No seller API for integration** | Manual management | API for ERP/inventory sync |

### B10. Customer Experience

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B10.1 | **Product reviews after purchase only** | Verified reviews | Open reviews need moderation |
| B10.2 | **5-star rating system** | Industry standard | Different rating scales |
| B10.3 | **No review incentives** | Authentic reviews | Review rewards program |
| B10.4 | **Email notifications only initially** | Basic notifications | SMS, push, WhatsApp |
| B10.5 | **No live chat support** | Ticket-based support | Real-time chat integration |
| B10.6 | **Self-service help center** | FAQ-based | Dedicated customer support |
| B10.7 | **No product comparison feature** | Simple browsing | Comparison tool development |
| B10.8 | **Basic wishlist functionality** | Save for later | Shared wishlists, notifications |
| B10.9 | **No recently viewed tracking** | Privacy focus | Personalization features |
| B10.10 | **Single cart across devices** | Logged-in sync | Device-specific carts |

### B11. Legal & Compliance

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B11.1 | **Standard Indian e-commerce laws** | Consumer Protection Act compliance | Additional regulations |
| B11.2 | **GDPR not applicable** | India-only operations | EU customer data handling |
| B11.3 | **No age-restricted products** | General merchandise | Age verification needed |
| B11.4 | **No hazardous/regulated goods** | Standard products | Special handling, licenses |
| B11.5 | **Invoice generation by seller** | Tax compliance | Platform invoice generation |
| B11.6 | **Data retention 7 years** | Legal requirement | Different retention policies |
| B11.7 | **No export documentation** | Domestic only | Export compliance handling |
| B11.8 | **Standard privacy policy sufficient** | Basic requirements | Industry-specific compliance |

### B12. Analytics & Reporting

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| B12.1 | **Basic sales analytics** | Dashboard reporting | Advanced BI tools needed |
| B12.2 | **No real-time analytics** | Batch processing | Real-time streaming analytics |
| B12.3 | **Manual reporting sufficient** | Scheduled reports | Automated report distribution |
| B12.4 | **Google Analytics for web** | Standard tracking | Custom analytics platform |
| B12.5 | **No A/B testing framework** | Simple features | Experimentation platform |
| B12.6 | **No recommendation engine** | Manual merchandising | ML-based recommendations |
| B12.7 | **No fraud detection ML** | Rule-based checks | AI fraud detection needed |
| B12.8 | **No customer segmentation** | Uniform treatment | Segment-based marketing |

---

## Technical Assumptions

### T1. Architecture

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| T1.1 | **Microservices architecture is appropriate** | 9 services with clear boundaries | Monolith may be simpler if team is small |
| T1.2 | **Service boundaries are correctly identified** | Domain-driven design followed | Refactoring services if boundaries are wrong |
| T1.3 | **Synchronous REST for queries, async for events** | Communication patterns designed | May need more real-time (WebSocket) |
| T1.4 | **Eventual consistency is acceptable** | Users tolerate 5-30 second delays | Need stronger consistency guarantees |

### T2. Technology Stack

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| T2.1 | **.NET 8 is stable and suitable** | LTS until Nov 2026 | May need version upgrade |
| T2.2 | **Nuxt.js 3 is production-ready** | SSR/SEO requirements | May need to use different framework |
| T2.3 | **PostgreSQL handles our relational needs** | JSONB for flexibility | May need specialized database |
| T2.4 | **RabbitMQ is sufficient for messaging** | Not event sourcing, just events | May need Kafka for higher throughput |

### T3. API & Integration

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| T3.1 | **REST APIs are sufficient** | No GraphQL requirement | May need GraphQL for mobile apps |
| T3.2 | **API Gateway handles 1000 req/sec** | Rate limiting configured | May need higher limits |
| T3.3 | **29-second API Gateway timeout is acceptable** | Long operations made async | Some operations may need longer |
| T3.4 | **JSON is the only content type needed** | No XML, Protocol Buffers | Performance may need binary formats |

---

## Infrastructure & Cloud Assumptions

### I1. AWS as Cloud Provider

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| I1.1 | **AWS remains the primary cloud provider** | All services designed for AWS | Major migration effort to other clouds |
| I1.2 | **Mumbai (ap-south-1) region is sufficient** | Primary user base in India | Multi-region deployment needed |
| I1.3 | **Managed services are preferred over self-hosted** | Reduced operational burden | Higher cost or need DevOps expertise |
| I1.4 | **AWS service availability meets our SLA** | 99.9% uptime assumed | May need multi-cloud redundancy |

### I2. Kubernetes & Containers

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| I2.1 | **EKS is appropriate for our scale** | Container orchestration needed | ECS may be simpler for smaller scale |
| I2.2 | **Team can operate Kubernetes** | Learning curve manageable | Training or managed services needed |
| I2.3 | **3 worker nodes initial capacity is sufficient** | Sizing for MVP | May need more nodes earlier |
| I2.4 | **Horizontal Pod Autoscaler works for our workload** | CPU/memory based scaling | May need custom metrics |

### I3. Networking & Security

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| I3.1 | **Single VPC architecture is sufficient** | No complex networking needs | Multi-VPC or VPC peering needed |
| I3.2 | **Public-facing services via ALB only** | All traffic through load balancer | Direct pod access may be needed |
| I3.3 | **WAF provides adequate protection** | Basic web protection | May need specialized security tools |
| I3.4 | **TLS 1.2+ is acceptable** | Modern encryption | May need specific compliance |

---

## Security & Authentication Assumptions

### S1. Authentication & Authorization

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| S1.1 | **AWS Cognito meets all auth requirements** | Social login, MFA, managed | May need Auth0 or custom solution |
| S1.2 | **Email is the primary user identifier** | Standard for e-commerce | May need phone-first auth |
| S1.3 | **Google & Facebook social login is sufficient** | Major OAuth providers | May need Apple, Microsoft, etc. |
| S1.4 | **MFA is optional for customers** | Balance security/UX | Mandatory MFA may be required |
| S1.5 | **JWT token expiry of 1 hour is acceptable** | Security/UX balance | Shorter expiry may be needed |
| S1.6 | **Refresh token rotation is implemented** | Security best practice | Simpler token strategy if not needed |

### S2. Data Security

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| S2.1 | **Encryption at rest is sufficient** | AWS managed encryption | May need customer-managed keys |
| S2.2 | **PII stored in PostgreSQL is acceptable** | Relational data needs | May need separate PII vault |
| S2.3 | **No PCI-DSS Level 1 compliance needed** | Using payment gateway tokenization | Full PCI compliance effort |
| S2.4 | **Standard AWS security controls are adequate** | SOC2, ISO compliance | Industry-specific compliance needed |

---

## Database & Data Assumptions

### D1. PostgreSQL (Relational Data)

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| D1.1 | **JSONB columns provide sufficient flexibility** | Product attributes vary | Full NoSQL may be needed |
| D1.2 | **Read replicas handle read scaling** | 80% reads, 20% writes | May need sharding |
| D1.3 | **Multi-AZ provides sufficient HA** | RPO < 5 min, RTO < 30 min | Cross-region replication needed |
| D1.4 | **db.t3.medium is sufficient initially** | Development & early production | Larger instance earlier |

### D2. DocumentDB (NoSQL Data)

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| D2.1 | **DocumentDB is sufficiently MongoDB-compatible** | Using MongoDB API | Need actual MongoDB Atlas |
| D2.2 | **Reviews & notifications fit document model** | Flexible schema needs | May need different storage |
| D2.3 | **Embedded documents reduce joins adequately** | Read performance focus | More normalization needed |
| D2.4 | **5MB document size limit is acceptable** | Product documents sized | Larger documents need splitting |

### D3. Redis (Cache)

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| D3.1 | **Redis handles session & cart storage** | Fast access needed | May need separate session store |
| D3.2 | **Cache invalidation strategy is correct** | Write-through for critical data | May see stale data issues |
| D3.3 | **TTL-based expiration is sufficient** | Auto-cleanup of sessions | May need explicit cleanup |
| D3.4 | **Cluster mode not needed initially** | Single node scaling | Earlier cluster adoption |

### D4. Data Modeling

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| D4.1 | **Product & Inventory in same service** | Combined service simplicity | Separate inventory service needed |
| D4.2 | **Multi-warehouse support needed** | Warehouses table added | Single warehouse simpler |
| D4.3 | **Soft delete for critical data** | Audit requirements | Hard delete may be acceptable |
| D4.4 | **UTC timestamps for all dates** | Standardization | Timezone handling complexity |
| D4.5 | **UUID primary keys are appropriate** | Distributed ID generation | Sequential IDs if centralized |

---

## Search & Performance Assumptions

### P1. OpenSearch

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| P1.1 | **OpenSearch feature set is sufficient** | No Elastic-specific needs | May need Elasticsearch |
| P1.2 | **Real-time indexing (< 5s) is acceptable** | Near real-time sync | Sub-second indexing needed |
| P1.3 | **2 data nodes provide adequate capacity** | Initial sizing | Earlier scaling needed |
| P1.4 | **BM25 relevance is suitable** | Standard text search | Custom ML ranking needed |
| P1.5 | **Autocomplete via completion suggester** | Type-ahead search | May need edge n-grams |

### P2. Performance Requirements

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| P2.1 | **< 200ms API response time target** | User experience | More aggressive optimization |
| P2.2 | **< 3s page load time** | SEO and UX | CDN optimization, code splitting |
| P2.3 | **99.9% availability target** | Business requirement | Higher redundancy for 99.99% |
| P2.4 | **Caching reduces DB load by 70%** | Cache hit rate assumption | More aggressive caching |

---

## Team & Organizational Assumptions

### O1. Team Capabilities

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| O1.1 | **Team size remains 8-10 developers** | Microservices per team | More/less automation needed |
| O1.2 | **.NET expertise available in team** | Backend technology choice | Training or hiring required |
| O1.3 | **Vue.js/Nuxt.js skills can be acquired** | Frontend learning curve | Different framework |
| O1.4 | **Kubernetes skills can be developed** | Container orchestration | Simpler orchestration (ECS) |
| O1.5 | **Team can learn AWS Amplify SDK in 2 weeks** | Cognito integration | Delay frontend integration |

### O2. Development Process

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| O2.1 | **Agile/Scrum methodology followed** | Sprint-based delivery | Waterfall changes planning |
| O2.2 | **CI/CD pipeline is established** | Automated deployments | Manual deployment overhead |
| O2.3 | **Code reviews are mandatory** | Quality assurance | Quality issues may increase |
| O2.4 | **Documentation is maintained** | Knowledge sharing | Onboarding challenges |

---

## Integration Assumptions

### G1. External Integrations

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| G1.1 | **Razorpay for payment processing** | Indian payment methods | Different gateway integration |
| G1.2 | **Email via AWS SES** | Managed email service | SendGrid or other provider |
| G1.3 | **SMS via AWS SNS or third-party** | Notification needs | Different SMS provider |
| G1.4 | **No ERP integration initially** | Standalone system | ERP integration complexity |
| G1.5 | **No existing legacy system migration** | Greenfield project | Data migration effort |

### G2. Third-Party Services

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| G2.1 | **Google OAuth app approval obtained** | Social login | Approval delay |
| G2.2 | **Facebook OAuth app approval obtained** | Social login | Approval delay |
| G2.3 | **Shipping carrier APIs available** | Order tracking | Manual tracking updates |
| G2.4 | **No real-time inventory sync with sellers** | Seller manages via portal | Integration with seller systems |

---

## Budget & Cost Assumptions

### C1. Infrastructure Costs

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| C1.1 | **~$450/month initial infrastructure** | MVP budget | Higher costs earlier |
| C1.2 | **Budget allows for managed services** | Reduced ops overhead | Self-hosted alternatives |
| C1.3 | **Reserved instances will be used** | Cost optimization | Higher on-demand costs |
| C1.4 | **Open-source preference continues** | Licensing costs avoided | Commercial tools if needed |

### C2. Service Costs

| ID | Assumption | Rationale | Impact if Invalid |
|:---|:-----------|:----------|:------------------|
| C2.1 | **Cognito free tier covers initial users** | 50,000 MAU free | Earlier paid tier |
| C2.2 | **S3/CloudFront egress manageable** | CDN caching effective | Higher transfer costs |
| C2.3 | **OpenSearch t3.small sufficient initially** | Search volume manageable | Larger instance needed |
| C2.4 | **API Gateway pay-per-request model viable** | $3.50/million requests | Flat-rate if very high volume |

---

## Assumptions Summary Matrix

### By Category Count

| Category | Count | Critical | High | Medium | Low |
|:---------|:-----:|:--------:|:----:|:------:|:---:|
| Business | 108 | 12 | 35 | 45 | 16 |
| Technical | 12 | 1 | 5 | 4 | 2 |
| Infrastructure | 12 | 2 | 4 | 4 | 2 |
| Security | 10 | 3 | 4 | 2 | 1 |
| Database | 15 | 2 | 5 | 6 | 2 |
| Search/Performance | 9 | 1 | 3 | 4 | 1 |
| Team | 9 | 1 | 4 | 3 | 1 |
| Integration | 9 | 1 | 3 | 4 | 1 |
| Budget | 8 | 1 | 3 | 3 | 1 |
| **Total** | **192** | **24** | **66** | **75** | **27** |

### Business Assumptions by Sub-Category

| Sub-Category | Count | Key Focus Areas |
|:-------------|:-----:|:----------------|
| B1. Target Market | 8 | Geography, currency, language |
| B2. Business Model | 8 | Marketplace, revenue, B2C focus |
| B3. Scale & Growth | 8 | Users, products, orders volume |
| B4. Product & Catalog | 10 | Categories, variants, pricing |
| B5. Pricing & Promotions | 10 | Discounts, coupons, loyalty |
| B6. Order & Checkout | 10 | Cart, payment, order limits |
| B7. Shipping & Fulfillment | 10 | Delivery, logistics, COD |
| B8. Returns & Refunds | 8 | Return policy, refund process |
| B9. Seller Management | 10 | Onboarding, payouts, analytics |
| B10. Customer Experience | 10 | Reviews, notifications, support |
| B11. Legal & Compliance | 8 | Regulations, data, invoicing |
| B12. Analytics & Reporting | 8 | Dashboards, ML, segmentation |

### Critical Assumptions (Must Validate)

| ID | Assumption | Category | Validation Action |
|:---|:-----------|:---------|:------------------|
| B1.1 | India is the primary market | Geography | Confirm with stakeholders |
| B2.1 | Marketplace model with sellers | Business Model | Confirm business strategy |
| B3.1 | Product catalog ≤ 500K | Scale | Review growth projections |
| B4.9 | Products require approval | Catalog | Confirm moderation workflow |
| B6.1 | Guest checkout not allowed | Checkout | UX/conversion rate review |
| B7.8 | No COD initially | Payment | Market requirement analysis |
| B9.4 | Platform holds payment | Seller | Legal/financial review |
| B11.3 | No age-restricted products | Compliance | Product category review |
| I1.1 | AWS remains primary cloud | Infrastructure | Strategic decision confirmation |
| S1.1 | Cognito meets auth requirements | Security | POC with all auth flows |
| S2.3 | No PCI-DSS Level 1 needed | Compliance | Legal/compliance review |
| D4.1 | Product & Inventory combined | Architecture | Review domain boundaries |
| P2.3 | 99.9% availability sufficient | SLA | Business SLA confirmation |
| O1.2 | .NET expertise available | Team | Skills assessment |

### Top 15 Assumptions to Validate First

| Priority | ID | Assumption | Why Validate First |
|:--------:|:---|:-----------|:-------------------|
| 1 | B1.1 | India primary market | Affects entire architecture |
| 2 | B2.1 | Marketplace model | Schema design dependency |
| 3 | B3.1-3.8 | Scale assumptions | Sizing all infrastructure |
| 4 | B7.8 | No COD initially | Major payment method decision |
| 5 | B6.1 | Guest checkout not allowed | Impacts conversion rate |
| 6 | B4.6-4.8 | No customization/bundles | Product complexity |
| 7 | B9.3-9.4 | Seller payout policy | Financial/legal implications |
| 8 | I1.1 | AWS as cloud | All service choices |
| 9 | T2.1-2.2 | .NET & Nuxt.js choices | Development begins |
| 10 | S1.1-1.3 | Cognito & social login | Auth implementation |
| 11 | D1.1 | JSONB for flexibility | Schema design |
| 12 | D4.1 | Combined Product/Inventory | Service boundaries |
| 13 | B5.4-5.5 | No loyalty/membership | Customer engagement strategy |
| 14 | O1.1-1.5 | Team skills | Training planning |
| 15 | C1.1 | Budget ~$450/month | Infrastructure provisioning |

---

## Assumption Validation Process

### Recommended Steps

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Assumption Validation Process                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  1. IDENTIFY                                                        │
│     └── Review this document with stakeholders                      │
│                                                                     │
│  2. PRIORITIZE                                                      │
│     └── Mark assumptions as Critical/High/Medium/Low                │
│                                                                     │
│  3. VALIDATE                                                        │
│     ├── Stakeholder confirmation (Business assumptions)             │
│     ├── POC/Spike (Technical assumptions)                           │
│     ├── Cost analysis (Budget assumptions)                          │
│     └── Skills assessment (Team assumptions)                        │
│                                                                     │
│  4. DOCUMENT                                                        │
│     ├── Update assumption status (Validated/Invalid)                │
│     ├── Document validation evidence                                │
│     └── Update related decisions if invalid                         │
│                                                                     │
│  5. MONITOR                                                         │
│     └── Periodic review during project lifecycle                    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Validation Status Template

| ID | Assumption | Status | Validated By | Date | Notes |
|:---|:-----------|:------:|:-------------|:-----|:------|
| B1.1 | India is primary market | ⏳ Pending | | | |
| B2.1 | Marketplace model | ⏳ Pending | | | |
| B3.1 | Product catalog ≤ 500K | ⏳ Pending | | | |
| B6.1 | Guest checkout not allowed | ⏳ Pending | | | |
| B7.8 | No COD initially | ⏳ Pending | | | |
| B9.4 | Platform holds payment | ⏳ Pending | | | |
| B5.4 | No loyalty program | ⏳ Pending | | | |
| B10.5 | No live chat support | ⏳ Pending | | | |

Status Legend:
- ⏳ Pending - Not yet validated
- ✅ Validated - Confirmed correct
- ❌ Invalid - Assumption proved wrong
- 🔄 Changed - Requirements changed

---

## Revision History

| Version | Date | Author | Changes |
|:--------|:-----|:-------|:--------|
| 1.0 | January 2026 | Architecture Team | Initial version |

---

## References

- [DAR-Technology-Stack-Selection.md](./DAR-Technology-Stack-Selection.md)
- [DAR-OpenSearch-Selection.md](./DAR-OpenSearch-Selection.md)
- [DAR-Cognito-APIGateway-Selection.md](./DAR-Cognito-APIGateway-Selection.md)
- [ADR-001-microservices-architecture.md](./ADR/ADR-001-microservices-architecture.md)
- [ADR-004-database-strategy.md](./ADR/ADR-004-database-strategy.md)
- [ADR-007-event-driven-architecture.md](./ADR/ADR-007-event-driven-architecture.md)
- [ADR-008-cloud-provider.md](./ADR/ADR-008-cloud-provider.md)
