# DECISION ANALYSIS AND RESOLUTION (DAR) DOCUMENT

## Search Engine Technology Selection - OpenSearch

---

## Document Control Information

| Field | Value |
|:------|:------|
| **Document Title** | Search Engine Technology Selection - OpenSearch |
| **Document ID** | DAR-AMCART-SEARCH-001 |
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
   - 3.1 [OpenSearch](#31-opensearch)
   - 3.2 [Elasticsearch](#32-elasticsearch)
   - 3.3 [Algolia](#33-algolia)
   - 3.4 [Apache Solr](#34-apache-solr)
4. [Comparison Analysis](#4-comparison-analysis)
   - 4.1 [Point Matrix](#41-point-matrix)
   - 4.2 [Feature Comparison](#42-feature-comparison)
   - 4.3 [Cost Comparison](#43-cost-comparison)
   - 4.4 [Performance Comparison](#44-performance-comparison)
5. [Recommendation](#5-recommendation)
6. [Assumptions](#6-assumptions)
7. [Risks](#7-risks)
8. [Appendix](#8-appendix)
   - 8.1 [References](#81-references)

---

## 1. Introduction

### 1.1 Objective and Scope of Document

#### Objective

This Decision Analysis and Resolution (DAR) document evaluates and compares available search engine technologies for the AmCart ecommerce platform. The objective is to select the most suitable search engine that meets the project's functional requirements, performance expectations, cost constraints, and long-term maintainability goals.

#### Scope

This document covers:

- Evaluation of search engine technologies for product search functionality
- Full-text search capabilities for product catalog (10,000+ products)
- Autocomplete and search suggestions
- Faceted navigation (filters by category, brand, price, attributes)
- Relevance scoring and ranking
- Integration with .NET 8 microservices
- AWS cloud deployment compatibility

#### Out of Scope

- Database selection (covered in DAR-AMCART-DB-001)
- Application-level caching strategies
- CDN and static content delivery

---

## 2. Requirements at a Glance

### Functional Requirements

| ID | Requirement | Priority | Description |
|:---|:------------|:---------|:------------|
| FR-01 | Full-Text Search | Critical | Search products by name, description, SKU |
| FR-02 | Autocomplete | High | Real-time search suggestions as user types |
| FR-03 | Faceted Filters | High | Filter by category, brand, price range, color, size |
| FR-04 | Relevance Ranking | High | Sort results by relevance, popularity, price |
| FR-05 | Fuzzy Matching | Medium | Handle typos and misspellings |
| FR-06 | Synonyms | Medium | "t-shirt" should match "tee", "tshirt" |
| FR-07 | Multi-language | Low | Support for Hindi and English |
| FR-08 | Geo Search | Low | Find nearby sellers (future) |

### Non-Functional Requirements

| ID | Requirement | Target | Description |
|:---|:------------|:-------|:------------|
| NFR-01 | Search Latency | < 100ms | P95 search response time |
| NFR-02 | Indexing Latency | < 5s | Time to index new/updated product |
| NFR-03 | Availability | 99.9% | Uptime SLA |
| NFR-04 | Scalability | 10x | Handle 10x current load |
| NFR-05 | Data Volume | 100K+ | Support 100,000+ products |
| NFR-06 | Concurrent Users | 1000+ | Support 1000+ concurrent searches |

### Technical Constraints

| Constraint | Description |
|:-----------|:------------|
| Cloud Provider | AWS (preferred managed services) |
| Backend Technology | .NET 8 / C# |
| Budget | Cost-effective, prefer open-source |
| Team Expertise | Limited Elasticsearch experience |
| Integration | REST API / .NET SDK support required |

---

## 3. Available Tools

### 3.1 OpenSearch

#### Overview

OpenSearch is an open-source, distributed search and analytics engine derived from Elasticsearch 7.10.2. It is maintained by AWS and the OpenSearch community under the Apache 2.0 license.

#### 3.1.1 Features

| Feature | Details |
|:--------|:--------|
| **Full-Text Search** | Lucene-based, BM25 relevance scoring |
| **Query DSL** | Rich query language with bool, match, term, range queries |
| **Aggregations** | Bucket, metric, and pipeline aggregations for facets |
| **Autocomplete** | Completion suggester, edge n-gram tokenizer |
| **Fuzzy Search** | Levenshtein distance-based fuzzy matching |
| **Analyzers** | Custom analyzers, tokenizers, filters |
| **Security** | Built-in security plugin (free) |
| **Alerting** | Built-in alerting (free) |
| **SQL Support** | SQL query interface (free) |
| **k-NN Search** | Vector similarity search (free) |
| **Anomaly Detection** | ML-based anomaly detection (free) |
| **Index Management** | ISM (Index State Management) policies |
| **Horizontal Scaling** | Sharding and replication |
| **REST API** | Full REST API support |
| **.NET Support** | OpenSearch.Client (compatible with NEST) |

#### 3.1.2 Pricing

| Deployment | Pricing Model |
|:-----------|:--------------|
| **Self-Hosted** | Free (open-source, Apache 2.0) |
| **Amazon OpenSearch Service** | Pay-as-you-go based on instance type |

**AWS Pricing Example (us-east-1):**

| Configuration | Monthly Cost |
|:--------------|:-------------|
| t3.small.search (2 nodes) | ~$50/month |
| t3.medium.search (3 nodes) | ~$120/month |
| r6g.large.search (3 nodes) | ~$400/month |

---

### 3.2 Elasticsearch

#### Overview

Elasticsearch is the original distributed search engine built on Apache Lucene. Created by Elastic NV, it changed from Apache 2.0 to SSPL/Elastic License 2.0 in 2021.

#### 3.2.1 Features

| Feature | Details |
|:--------|:--------|
| **Full-Text Search** | Lucene-based, BM25 relevance scoring |
| **Query DSL** | Same rich query language as OpenSearch |
| **Aggregations** | Full aggregation support |
| **Autocomplete** | Completion suggester |
| **Security** | X-Pack Security (some features paid) |
| **Alerting** | Watcher (paid in older versions) |
| **SQL Support** | X-Pack SQL (paid in older versions) |
| **Machine Learning** | ML features (paid) |
| **APM** | Application Performance Monitoring |
| **Canvas** | Data visualization |
| **.NET Support** | NEST / Elastic.Clients.Elasticsearch |

#### 3.2.2 Pricing

| Deployment | Pricing Model |
|:-----------|:--------------|
| **Self-Hosted** | Free (SSPL License - restrictions apply) |
| **Elastic Cloud** | Subscription-based |

**Elastic Cloud Pricing:**

| Tier | Monthly Cost |
|:-----|:-------------|
| Standard | Starting ~$95/month |
| Gold | Starting ~$125/month |
| Platinum | Starting ~$175/month |
| Enterprise | Custom pricing |

**Note:** Some features (Security, Alerting, ML) require paid subscription.

---

### 3.3 Algolia

#### Overview

Algolia is a hosted search-as-a-service platform designed for speed and ease of use. It's a fully managed SaaS solution.

#### 3.3.1 Features

| Feature | Details |
|:--------|:--------|
| **Full-Text Search** | Proprietary engine optimized for speed |
| **Typo Tolerance** | Built-in typo handling |
| **Instant Search** | Sub-millisecond response times |
| **Autocomplete** | Built-in, highly optimized |
| **Faceting** | Easy faceted navigation |
| **Analytics** | Search analytics dashboard |
| **A/B Testing** | Built-in A/B testing |
| **Personalization** | AI-powered personalization |
| **SDK Support** | .NET SDK available |

#### 3.3.2 Pricing

| Plan | Records | Searches | Monthly Cost |
|:-----|:--------|:---------|:-------------|
| Free | 10K | 10K/month | $0 |
| Build | 1M | 1M/month | $29/month |
| Grow | 10M | 10M/month | $100/month+ |
| Premium | Unlimited | Unlimited | Custom |

**Cons:**
- Per-record and per-search pricing can be expensive at scale
- Limited customization compared to self-hosted solutions
- Vendor lock-in

---

### 3.4 Apache Solr

#### Overview

Apache Solr is an open-source enterprise search platform built on Apache Lucene. It's maintained by the Apache Software Foundation.

#### 3.4.1 Features

| Feature | Details |
|:--------|:--------|
| **Full-Text Search** | Lucene-based search |
| **Faceted Search** | Built-in faceting |
| **Highlighting** | Search result highlighting |
| **Spell Check** | Built-in spell checker |
| **Rich Document Handling** | PDF, Word, etc. |
| **Replication** | Master-slave replication |
| **SolrCloud** | Distributed mode |
| **.NET Support** | SolrNet (community) |

#### 3.4.2 Pricing

| Deployment | Pricing Model |
|:-----------|:--------------|
| **Self-Hosted** | Free (Apache 2.0) |
| **Managed Services** | Limited options |

**Cons:**
- No AWS managed service
- Older architecture compared to Elasticsearch/OpenSearch
- Smaller community for ecommerce use cases
- Limited .NET ecosystem support

---

## 4. Comparison Analysis

### 4.1 Point Matrix

| Criteria | Max Points | OpenSearch | Elasticsearch | Algolia | Solr |
|:---------|:----------:|:----------:|:-------------:|:-------:|:----:|
| **Full-Text Search** | 20 | 18 | 18 | 20 | 16 |
| **Autocomplete** | 15 | 14 | 14 | 15 | 11 |
| **Faceted Navigation** | 15 | 14 | 14 | 14 | 12 |
| **AWS Integration** | 15 | 15 | 9 | 11 | 6 |
| **Cost** | 15 | 14 | 9 | 8 | 14 | 
| **.NET Support** | 10 | 8 | 9 | 8 | 6 |
| **Ease of Operations** | 5 | 5 | 4 | 5 | 3 |
| **Community/Support** | 5 | 4 | 5 | 4 | 4 |
| **Total Score** | **100** | **92** | 82 | 85 | 72 |

**Scoring:** Points awarded out of max for each criterion (higher = better)

### 4.2 Feature Comparison

| Feature | OpenSearch | Elasticsearch | Algolia | Solr |
|:--------|:-----------|:--------------|:--------|:-----|
| Full-Text Search | ✅ Excellent | ✅ Excellent | ✅ Excellent | ✅ Good |
| Autocomplete | ✅ Built-in | ✅ Built-in | ✅ Built-in | ✅ Built-in |
| Faceted Search | ✅ Aggregations | ✅ Aggregations | ✅ Built-in | ✅ Built-in |
| Fuzzy Matching | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Synonyms | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Geo Search | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| Security (Free) | ✅ Yes | ⚠️ Limited | ✅ Yes | ⚠️ Basic |
| Alerting (Free) | ✅ Yes | ❌ Paid | ✅ Yes | ❌ No |
| ML Features (Free) | ✅ Yes | ❌ Paid | ⚠️ Limited | ❌ No |
| SQL Queries | ✅ Yes | ⚠️ Paid | ❌ No | ✅ Yes |
| AWS Managed | ✅ Native | ❌ No | ❌ SaaS | ❌ No |
| .NET SDK | ✅ OpenSearch.Client | ✅ NEST | ✅ Algolia.Search | ⚠️ 4 |
| License | Apache 2.0 | SSPL | Proprietary | Apache 2.0 |

### 4.3 Cost Comparison

**Scenario:** 50,000 products, 100,000 searches/day, 3-node cluster

| Solution | Monthly Cost | Notes |
|:---------|:-------------|:------|
| **Amazon OpenSearch** | ~$150-200 | t3.medium.search × 3 |
| **Elasticsearch (Self-hosted)** | ~$100-150 | EC2 + management overhead |
| **Elastic Cloud** | ~$300-500 | Managed, includes premium features |
| **Algolia** | ~$200-400 | Per-search pricing adds up |
| **Solr (Self-hosted)** | ~$100-150 | EC2 + high management overhead |

**5-Year TCO Analysis:**

| Solution | Infrastructure | Licensing | Operations | Total 5-Year |
|:---------|:---------------|:----------|:-----------|:-------------|
| **Amazon OpenSearch** | $12,000 | $0 | $5,000 | **$17,000** |
| **Elasticsearch Cloud** | $0 | $24,000 | $2,000 | $26,000 |
| **Algolia** | $0 | $18,000 | $1,000 | $19,000 |
| **Solr (Self-hosted)** | $9,000 | $0 | $15,000 | $24,000 |

### 4.4 Performance Comparison

| Metric | OpenSearch | Elasticsearch | Algolia | Solr |
|:-------|:-----------|:--------------|:--------|:-----|
| Search Latency (P95) | 20-50ms | 20-50ms | 5-15ms | 30-80ms |
| Indexing Speed | ~5,000 docs/s | ~5,000 docs/s | ~3,000 docs/s | ~3,000 docs/s |
| Concurrent Queries | 1000+ | 1000+ | 10,000+ | 500+ |
| Horizontal Scaling | ✅ Easy | ✅ Easy | ✅ Automatic | ⚠️ Complex |

---

## 5. Recommendation

### Selected Solution: Amazon OpenSearch Service

Based on the comprehensive evaluation, **Amazon OpenSearch Service** is recommended as the search engine for the AmCart ecommerce platform.

### Justification

| Factor | Rationale |
|:-------|:----------|
| **AWS Native Integration** | Seamless integration with existing AWS infrastructure (EKS, RDS, ElastiCache) |
| **Cost-Effective** | All features free (security, alerting, SQL, ML) - no licensing costs |
| **Truly Open Source** | Apache 2.0 license with no vendor lock-in concerns |
| **Managed Service** | Reduced operational overhead - AWS handles scaling, patching, backups |
| **Feature Parity** | All required features (full-text search, facets, autocomplete) available |
| **API Compatibility** | Compatible with Elasticsearch 7.x APIs and .NET clients |
| **Security** | Built-in security plugin with fine-grained access control |
| **Monitoring** | Native CloudWatch integration |

### Implementation Approach

```
┌─────────────────────────────────────────────────────────────────┐
│                 Amazon OpenSearch Service                        │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    OpenSearch Domain                       │  │
│  │                                                           │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐       │  │
│  │  │  Data Node  │  │  Data Node  │  │  Data Node  │       │  │
│  │  │  (Primary)  │  │  (Replica)  │  │  (Replica)  │       │  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘       │  │
│  │                                                           │  │
│  │  Index: products_v1                                       │  │
│  │  - Shards: 3 primary, 1 replica each                     │  │
│  │  - Refresh: 1 second                                      │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  Features Enabled:                                               │
│  ✅ Fine-grained access control                                 │
│  ✅ Encryption at rest (KMS)                                    │
│  ✅ Node-to-node encryption                                     │
│  ✅ Automated snapshots                                         │
│  ✅ CloudWatch monitoring                                       │
└─────────────────────────────────────────────────────────────────┘
```

### Configuration Recommendation

| Setting | Development | Production |
|:--------|:------------|:-----------|
| Instance Type | t3.small.search | r6g.large.search |
| Node Count | 2 | 3 |
| Storage | 20 GB gp3 | 100 GB gp3 |
| Primary Shards | 1 | 3 |
| Replica Shards | 0 | 1 |
| Dedicated Masters | No | Yes (3 × t3.small) |

---

## 6. Assumptions

| ID | Assumption | Impact if Invalid |
|:---|:-----------|:------------------|
| A1 | Product catalog will not exceed 500,000 products in 3 years | May need to resize cluster |
| A2 | Search traffic will not exceed 1M searches/day | May need higher instance types |
| A3 | AWS remains the primary cloud provider | Would need to reconsider self-hosted options |
| A4 | Team will have basic Elasticsearch/OpenSearch training | May increase initial development time |
| A5 | Real-time indexing (< 5s) is acceptable | Would need different architecture for sub-second |
| A6 | English is the primary language | Multi-language may need additional analyzers |
| A7 | OpenSearch maintains API compatibility with Elasticsearch 7.x | Migration path if compatibility breaks |

---

## 7. Risks

| ID | Risk | Probability | Impact | Mitigation |
|:---|:-----|:------------|:-------|:-----------|
| R1 | OpenSearch diverges significantly from Elasticsearch | Low | Medium | Monitor roadmap; abstraction layer in code |
| R2 | Search latency exceeds requirements | Low | High | Performance testing; instance right-sizing |
| R3 | Index corruption or data loss | Low | High | Automated snapshots; cross-AZ deployment |
| R4 | Cost overrun due to traffic spike | Medium | Medium | Set up billing alerts; auto-scaling policies |
| R5 | Security misconfiguration | Medium | High | Follow AWS security best practices; regular audits |
| R6 | Learning curve delays project | Medium | Medium | Training sessions; documentation; start simple |
| R7 | Vendor lock-in to AWS | Low | Medium | Use standard APIs; avoid AWS-specific features |
| R8 | Query performance degrades with data growth | Medium | Medium | Index lifecycle management; regular optimization |

### Risk Response Plan

| Risk | Response Strategy | Owner |
|:-----|:------------------|:------|
| R1 | Accept - Monitor OpenSearch releases | Tech Lead |
| R2 | Mitigate - Load testing before launch | DevOps |
| R3 | Mitigate - Daily snapshots, multi-AZ | DevOps |
| R4 | Mitigate - CloudWatch alarms, budgets | DevOps |
| R5 | Mitigate - Security review, IAM policies | Security |
| R6 | Mitigate - Training, POC first | Tech Lead |
| R7 | Accept - Document exit strategy | Architect |
| R8 | Mitigate - Monitoring, ISM policies | DevOps |

---

## 8. Appendix

### 8.1 References

| # | Reference | URL |
|:--|:----------|:----|
| 1 | OpenSearch Official Documentation | https://opensearch.org/docs/latest/ |
| 2 | Amazon OpenSearch Service | https://aws.amazon.com/opensearch-service/ |
| 3 | OpenSearch .NET Client | https://github.com/opensearch-project/opensearch-net |
| 4 | Elasticsearch Documentation | https://www.elastic.co/guide/en/elasticsearch/reference/current/ |
| 5 | Algolia Documentation | https://www.algolia.com/doc/ |
| 6 | Apache Solr Documentation | https://solr.apache.org/guide/ |
| 7 | OpenSearch vs Elasticsearch | https://opensearch.org/faq/ |
| 8 | AWS OpenSearch Pricing | https://aws.amazon.com/opensearch-service/pricing/ |
| 9 | OpenSearch Security Plugin | https://opensearch.org/docs/latest/security/ |
| 10 | Ecommerce Search Best Practices | https://opensearch.org/blog/ |

### 8.2 Glossary

| Term | Definition |
|:-----|:-----------|
| **BM25** | Best Matching 25 - relevance ranking algorithm |
| **Shard** | A partition of an index distributed across nodes |
| **Replica** | Copy of a shard for redundancy and read scaling |
| **Analyzer** | Component that processes text for indexing/searching |
| **Tokenizer** | Breaks text into individual terms |
| **Facet** | Category or attribute used for filtering search results |
| **ISM** | Index State Management - lifecycle policies for indices |
| **k-NN** | k-Nearest Neighbors - vector similarity search |

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

