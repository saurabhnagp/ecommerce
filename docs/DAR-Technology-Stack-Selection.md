# DECISION ANALYSIS AND RESOLUTION (DAR) DOCUMENT

## AmCart E-commerce Platform - Complete Technology Stack Selection

---

## Document Control Information

| Field | Value |
|:------|:------|
| **Document Title** | AmCart E-commerce Platform - Complete Technology Stack Selection |
| **Document ID** | DAR-AMCART-TECH-001 |
| **Project Name** | AmCart Ecommerce Platform |
| **Version** | 1.0 |
| **Status** | Approved |
| **Created Date** | January 2026 |
| **Last Updated** | January 2026 |
| **Prepared By** | Technical Architecture Team |
| **Reviewed By** | [Reviewer Name] |
| **Approved By** | [Approver Name] |

---

## Executive Summary

This document provides a comprehensive analysis of all technology decisions made for the AmCart e-commerce platform. The selected technology stack is optimized for scalability, maintainability, cost-effectiveness, and AWS cloud deployment.

### Technology Stack at a Glance

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        AmCart Technology Stack                                   │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │  FRONTEND                          │  CDN & STATIC                      │   │
│  │  ✅ Nuxt.js 3 + Vue 3              │  ✅ Amazon CloudFront              │   │
│  │  ✅ TypeScript                     │  ✅ Amazon S3                      │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                          │                                      │
│                                          ▼                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │  API GATEWAY & AUTHENTICATION                                           │   │
│  │  ✅ AWS API Gateway                │  ✅ AWS Cognito                    │   │
│  │  ✅ Nginx (Internal Load Balancer) │  ✅ JWT Tokens                     │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                          │                                      │
│                                          ▼                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │  BACKEND MICROSERVICES                                                  │   │
│  │  ✅ .NET 8 / C# 12                 │  ✅ ASP.NET Core Web API           │   │
│  │  ✅ Entity Framework Core          │  ✅ MassTransit                    │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                          │                                      │
│         ┌────────────────────────────────┼────────────────────────────────┐    │
│         │                                │                                │    │
│         ▼                                ▼                                ▼    │
│  ┌─────────────┐                ┌─────────────┐                ┌─────────────┐ │
│  │  DATABASES  │                │   SEARCH    │                │  MESSAGING  │ │
│  │             │                │             │                │             │ │
│  │ PostgreSQL  │                │ OpenSearch  │                │ RabbitMQ    │ │
│  │ (RDS)       │                │ (AWS)       │                │ (Amazon MQ) │ │
│  │             │                │             │                │             │ │
│  │ DocumentDB  │                └─────────────┘                └─────────────┘ │
│  │ (MongoDB)   │                                                               │
│  │             │                ┌─────────────┐                ┌─────────────┐ │
│  │ ElastiCache │                │ MONITORING  │                │   STORAGE   │ │
│  │ (Redis)     │                │             │                │             │ │
│  └─────────────┘                │ CloudWatch  │                │ Amazon S3   │ │
│                                 │ X-Ray       │                │             │ │
│                                 └─────────────┘                └─────────────┘ │
│                                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────┐   │
│  │  CONTAINER ORCHESTRATION & CI/CD                                        │   │
│  │  ✅ Amazon EKS (Kubernetes)        │  ✅ Jenkins                        │   │
│  │  ✅ Docker                         │  ✅ Helm Charts                    │   │
│  │  ✅ Amazon ECR                     │  ✅ Blue/Green Deployment          │   │
│  └─────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### Selected Technologies Summary

| Category | Selected Technology | Alternative Considered | Key Reason |
|:---------|:-------------------|:----------------------|:-----------|
| **Frontend Framework** | Nuxt.js 3 + Vue 3 | Next.js, Angular | SSR, SEO, developer productivity |
| **Backend Framework** | .NET 8 / C# 12 | Java Spring Boot, Node.js | Performance, type safety, AWS support |
| **API Gateway** | AWS API Gateway + Nginx | Kong, Ocelot | AWS integration, managed service |
| **Authentication** | AWS Cognito | Auth0, Keycloak | Cost, AWS native, social login |
| **Primary Database** | PostgreSQL (RDS) | MySQL, SQL Server | JSONB support, cost, reliability |
| **Document Database** | DocumentDB (MongoDB) | DynamoDB, Cosmos DB | Flexible schema, MongoDB compatible |
| **Cache** | ElastiCache (Redis) | Memcached, Hazelcast | Data structures, persistence |
| **Search Engine** | Amazon OpenSearch | Elasticsearch, Algolia | AWS native, cost, features |
| **Message Broker** | Amazon MQ (RabbitMQ) | Amazon SQS, Kafka | Routing, reliability, patterns |
| **Cloud Provider** | AWS | Azure, GCP | Market leader, services, team expertise |
| **Container Orchestration** | Amazon EKS | ECS, Fargate | Kubernetes standard, flexibility |
| **CI/CD** | Jenkins + Docker + Helm | GitHub Actions, GitLab CI | Flexibility, self-hosted, plugins |
| **Monitoring** | CloudWatch + X-Ray | Datadog, New Relic | AWS native, cost, integration |
| **Object Storage** | Amazon S3 | Azure Blob, GCS | Industry standard, CDN integration |

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Project Requirements Overview](#2-project-requirements-overview)
3. [Frontend Technology](#3-frontend-technology)
4. [Backend Technology](#4-backend-technology)
5. [API Gateway & Authentication](#5-api-gateway--authentication)
6. [Database Technologies](#6-database-technologies)
7. [Search Engine](#7-search-engine)
8. [Message Broker](#8-message-broker)
9. [Cloud Provider](#9-cloud-provider)
10. [Container Orchestration](#10-container-orchestration)
11. [CI/CD Pipeline](#11-cicd-pipeline)
12. [Monitoring & Observability](#12-monitoring--observability)
13. [Storage Services](#13-storage-services)
14. [Consolidated Technology Stack](#14-consolidated-technology-stack)
15. [Assumptions](#15-assumptions)
16. [Risks](#16-risks)
17. [Appendix](#17-appendix)

---

## 1. Introduction

### 1.1 Objective and Scope

#### Objective

This Decision Analysis and Resolution (DAR) document evaluates and documents all technology decisions for the AmCart e-commerce platform. It provides a comprehensive analysis of each technology choice, including alternatives considered, evaluation criteria, and justification for selections.

#### Scope

This document covers technology selection for:

- Frontend framework and UI technologies
- Backend framework and programming language
- API Gateway and authentication services
- Database technologies (SQL, NoSQL, Cache)
- Search engine for product discovery
- Message broker for event-driven architecture
- Cloud provider and infrastructure services
- Container orchestration platform
- CI/CD pipeline tools
- Monitoring and observability solutions
- Storage services

#### Out of Scope

- Detailed implementation guides
- Infrastructure as Code (Terraform) specifics
- Detailed security configurations
- Third-party integrations (Payment gateways, Shipping providers)

### 1.2 Project Context

AmCart is a multi-vendor e-commerce platform with the following characteristics:

| Attribute | Value |
|:----------|:------|
| **Business Model** | Multi-vendor marketplace (B2C) |
| **Target Users** | Customers, Sellers, Admin |
| **Expected Products** | 100,000+ SKUs |
| **Expected Users** | 100,000+ registered users |
| **Daily Orders** | 1,000+ orders/day (initial) |
| **Geographic Region** | India (primary) |
| **Architecture** | Microservices |
| **Deployment** | Cloud-native (AWS) |

### 1.3 Microservices Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           AmCart Microservices                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │    User     │ │   Product   │ │    Cart     │ │    Order    │           │
│  │   Service   │ │   Service   │ │   Service   │ │   Service   │           │
│  │             │ │             │ │             │ │             │           │
│  │ PostgreSQL  │ │ DocumentDB  │ │ PostgreSQL  │ │ PostgreSQL  │           │
│  │ + Redis     │ │ + Redis     │ │ + Redis     │ │             │           │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘           │
│                                                                             │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │   Payment   │ │   Search    │ │Notification │ │   Review    │           │
│  │   Service   │ │   Service   │ │   Service   │ │   Service   │           │
│  │             │ │             │ │             │ │             │           │
│  │ PostgreSQL  │ │ OpenSearch  │ │ PostgreSQL  │ │ PostgreSQL  │           │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘           │
│                                                                             │
│  ┌─────────────┐                                                           │
│  │  Inventory  │  All services communicate via:                            │
│  │   Service   │  • Synchronous: REST APIs                                 │
│  │             │  • Asynchronous: RabbitMQ (MassTransit)                   │
│  │ PostgreSQL  │                                                           │
│  └─────────────┘                                                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Project Requirements Overview

### 2.1 Functional Requirements Summary

| Category | Key Requirements |
|:---------|:-----------------|
| **User Management** | Registration, authentication, profile, addresses, social login |
| **Product Catalog** | Categories, products, variants, images, attributes, search |
| **Shopping Cart** | Add/remove items, save for later, guest cart, merge on login |
| **Order Management** | Checkout, order tracking, cancellation, returns |
| **Payment** | Multiple payment methods, refunds, seller payouts |
| **Inventory** | Multi-warehouse, stock reservation, low stock alerts |
| **Search** | Full-text search, filters, autocomplete, relevance ranking |
| **Notifications** | Email, SMS, push notifications, order updates |
| **Reviews** | Product reviews, ratings, seller ratings |
| **Seller Portal** | Product management, order fulfillment, analytics |

### 2.2 Non-Functional Requirements

| Requirement | Target | Priority |
|:------------|:-------|:---------|
| **Availability** | 99.9% uptime | Critical |
| **Response Time** | < 200ms API, < 100ms search | Critical |
| **Scalability** | Handle 10x growth | High |
| **Security** | PCI-DSS compliance ready | Critical |
| **Data Consistency** | Strong for orders/payments | Critical |
| **Recovery** | RPO < 1 hour, RTO < 4 hours | High |
| **Maintainability** | Clear documentation, tests | High |

### 2.3 Technical Constraints

| Constraint | Description |
|:-----------|:------------|
| **Cloud Provider** | AWS (organizational decision) |
| **Budget** | Cost-optimized, prefer managed services |
| **Team Size** | 8-10 developers |
| **Timeline** | MVP in 6 months |
| **Team Expertise** | Strong .NET, moderate AWS, limited DevOps |

---

## 3. Frontend Technology

### 3.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| FE-01 | Server-Side Rendering (SSR) for SEO | Critical |
| FE-02 | Fast page load times | Critical |
| FE-03 | Responsive design (mobile-first) | Critical |
| FE-04 | Progressive Web App (PWA) support | High |
| FE-05 | TypeScript support | High |
| FE-06 | Component-based architecture | High |
| FE-07 | State management | High |
| FE-08 | Good developer experience | Medium |

### 3.2 Available Options

#### 3.2.1 Nuxt.js 3 (Vue 3)

| Aspect | Details |
|:-------|:--------|
| **Type** | Vue.js meta-framework |
| **SSR** | Built-in, hybrid rendering modes |
| **Language** | TypeScript (first-class support) |
| **State** | Pinia (official), useState composable |
| **Styling** | Any CSS framework, Tailwind CSS |
| **Learning Curve** | Moderate |
| **Community** | Large Vue ecosystem |

#### 3.2.2 Next.js 14 (React)

| Aspect | Details |
|:-------|:--------|
| **Type** | React meta-framework |
| **SSR** | Built-in, App Router, Server Components |
| **Language** | TypeScript (first-class support) |
| **State** | Redux, Zustand, React Query |
| **Styling** | Any CSS framework |
| **Learning Curve** | Moderate-High |
| **Community** | Largest ecosystem |

#### 3.2.3 Angular 17

| Aspect | Details |
|:-------|:--------|
| **Type** | Full framework |
| **SSR** | Angular Universal |
| **Language** | TypeScript (required) |
| **State** | NgRx, Services |
| **Styling** | Angular Material, any CSS |
| **Learning Curve** | High |
| **Community** | Enterprise-focused |

#### 3.2.4 SvelteKit

| Aspect | Details |
|:-------|:--------|
| **Type** | Svelte meta-framework |
| **SSR** | Built-in |
| **Language** | TypeScript support |
| **State** | Built-in stores |
| **Styling** | Any CSS framework |
| **Learning Curve** | Low |
| **Community** | Growing, smaller |

### 3.3 Comparison Matrix

| Criteria | Max Points | Nuxt.js 3 | Next.js 14 | Angular 17 | SvelteKit |
|:---------|:----------:|:---------:|:----------:|:----------:|:---------:|
| **SSR/SEO Support** | 20 | 19 | 20 | 16 | 18 |
| **Performance** | 15 | 14 | 14 | 12 | 15 |
| **Developer Experience** | 15 | 14 | 13 | 11 | 14 |
| **TypeScript Support** | 10 | 9 | 10 | 10 | 8 |
| **Ecosystem/Plugins** | 10 | 9 | 10 | 9 | 6 |
| **Learning Curve** | 10 | 8 | 7 | 5 | 9 |
| **Team Expertise** | 10 | 8 | 6 | 5 | 4 |
| **Community Support** | 5 | 4 | 5 | 4 | 3 |
| **Long-term Viability** | 5 | 4 | 5 | 5 | 3 |
| **Total Score** | **100** | **89** | 90 | 77 | 80 |

### 3.4 Recommendation: Nuxt.js 3

**Selected:** Nuxt.js 3 with Vue 3 and TypeScript

| Factor | Rationale |
|:-------|:----------|
| **SSR Excellence** | Built-in hybrid rendering (SSR, SSG, ISR, CSR) |
| **SEO Optimization** | Excellent SEO support out of the box |
| **Developer Productivity** | File-based routing, auto-imports, great DX |
| **Team Alignment** | Better team familiarity with Vue ecosystem |
| **Performance** | Lightweight, fast, efficient bundling |
| **Flexibility** | Nitro server, multiple deployment targets |

**Tech Stack:**

```
Frontend Stack:
├── Framework: Nuxt.js 3.x
├── UI Library: Vue 3.x (Composition API)
├── Language: TypeScript 5.x
├── State: Pinia
├── Styling: Tailwind CSS + HeadlessUI
├── HTTP Client: $fetch (built-in), Axios
├── Forms: VeeValidate + Zod
├── Testing: Vitest + Vue Test Utils
└── Build: Vite
```

---

## 4. Backend Technology

### 4.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| BE-01 | High performance for API requests | Critical |
| BE-02 | Strong typing and compile-time checks | High |
| BE-03 | Excellent ORM support | High |
| BE-04 | Microservices patterns support | High |
| BE-05 | AWS SDK support | Critical |
| BE-06 | Container-friendly | High |
| BE-07 | Good testing frameworks | High |
| BE-08 | Active community and LTS | High |

### 4.2 Available Options

#### 4.2.1 .NET 8 / C# 12

| Aspect | Details |
|:-------|:--------|
| **Type** | Compiled, strongly typed |
| **Performance** | Excellent (top-tier benchmarks) |
| **ORM** | Entity Framework Core, Dapper |
| **API** | ASP.NET Core Minimal APIs, Controllers |
| **Microservices** | MassTransit, Dapr, Orleans |
| **AWS SDK** | Official AWS SDK for .NET |
| **Container** | Excellent Docker support, small images |
| **LTS** | .NET 8 LTS until Nov 2026 |

#### 4.2.2 Java 21 / Spring Boot 3

| Aspect | Details |
|:-------|:--------|
| **Type** | Compiled, strongly typed |
| **Performance** | Very good (GraalVM for native) |
| **ORM** | Hibernate, Spring Data JPA |
| **API** | Spring MVC, WebFlux |
| **Microservices** | Spring Cloud, Netflix OSS |
| **AWS SDK** | Official AWS SDK for Java |
| **Container** | Good, larger images |
| **LTS** | Java 21 LTS |

#### 4.2.3 Node.js / NestJS

| Aspect | Details |
|:-------|:--------|
| **Type** | Interpreted, dynamic (TypeScript optional) |
| **Performance** | Good for I/O, limited for CPU |
| **ORM** | TypeORM, Prisma, Sequelize |
| **API** | Express, Fastify, NestJS |
| **Microservices** | NestJS microservices |
| **AWS SDK** | Official AWS SDK for JavaScript |
| **Container** | Small images |
| **LTS** | Node.js 20 LTS |

#### 4.2.4 Go / Gin

| Aspect | Details |
|:-------|:--------|
| **Type** | Compiled, statically typed |
| **Performance** | Excellent |
| **ORM** | GORM, sqlx |
| **API** | Gin, Echo, Fiber |
| **Microservices** | go-kit, go-micro |
| **AWS SDK** | Official AWS SDK for Go |
| **Container** | Smallest images |
| **LTS** | Regular releases |

### 4.3 Comparison Matrix

| Criteria | Max Points | .NET 8 | Spring Boot 3 | NestJS | Go/Gin |
|:---------|:----------:|:------:|:-------------:|:------:|:------:|
| **Performance** | 20 | 19 | 17 | 14 | 20 |
| **Type Safety** | 15 | 15 | 15 | 12 | 14 |
| **ORM/Database** | 15 | 14 | 14 | 12 | 10 |
| **Microservices Support** | 15 | 14 | 15 | 12 | 11 |
| **AWS Integration** | 10 | 9 | 9 | 8 | 8 |
| **Team Expertise** | 10 | 9 | 5 | 6 | 3 |
| **Ecosystem/Libraries** | 5 | 4 | 5 | 4 | 3 |
| **Container Efficiency** | 5 | 4 | 3 | 4 | 5 |
| **Learning Curve** | 5 | 4 | 3 | 4 | 3 |
| **Total Score** | **100** | **92** | 86 | 76 | 77 |

### 4.4 Recommendation: .NET 8

**Selected:** .NET 8 with C# 12 and ASP.NET Core

| Factor | Rationale |
|:-------|:----------|
| **Performance** | Top-tier performance, minimal overhead |
| **Type Safety** | Strong compile-time checks, fewer runtime errors |
| **Modern C#** | C# 12 features: primary constructors, collection expressions |
| **Team Expertise** | Strong .NET experience in team |
| **AWS Support** | Excellent AWS SDK, Lambda support |
| **Ecosystem** | MassTransit, MediatR, FluentValidation, AutoMapper |

**Tech Stack:**

```
Backend Stack:
├── Framework: .NET 8 LTS
├── Language: C# 12
├── API: ASP.NET Core Minimal APIs + Controllers
├── ORM: Entity Framework Core 8
├── Messaging: MassTransit + RabbitMQ
├── Validation: FluentValidation
├── Mapping: Mapster
├── CQRS: MediatR
├── Testing: xUnit + Moq + FluentAssertions
├── API Docs: Swagger/OpenAPI
└── Logging: Serilog + Seq
```

---

## 5. API Gateway & Authentication

### 5.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| AG-01 | Route management for microservices | Critical |
| AG-02 | Rate limiting and throttling | High |
| AG-03 | Request/response transformation | Medium |
| AG-04 | SSL termination | Critical |
| AG-05 | Authentication integration | Critical |
| AUTH-01 | User registration with email verification | Critical |
| AUTH-02 | Social login (Google, Facebook) | High |
| AUTH-03 | JWT token management | Critical |
| AUTH-04 | Multi-factor authentication | Medium |
| AUTH-05 | Password policies and reset | High |

### 5.2 API Gateway Options

#### 5.2.1 AWS API Gateway

| Aspect | Details |
|:-------|:--------|
| **Type** | Managed service |
| **Routing** | Path-based, method-based |
| **Auth** | Cognito, Lambda authorizers, IAM |
| **Rate Limiting** | Built-in throttling |
| **Monitoring** | CloudWatch integration |
| **Cost** | Pay per request ($3.50/million) |

#### 5.2.2 Kong Gateway

| Aspect | Details |
|:-------|:--------|
| **Type** | Open-source / Enterprise |
| **Routing** | Advanced routing, plugins |
| **Auth** | Multiple auth plugins |
| **Rate Limiting** | Advanced rate limiting |
| **Monitoring** | Prometheus, custom |
| **Cost** | Free (OSS) / Paid (Enterprise) |

#### 5.2.3 Nginx + Custom

| Aspect | Details |
|:-------|:--------|
| **Type** | Open-source |
| **Routing** | Configuration-based |
| **Auth** | Module-based, custom |
| **Rate Limiting** | ngx_http_limit_req_module |
| **Monitoring** | Custom integration |
| **Cost** | Free |

### 5.3 Authentication Options

#### 5.3.1 AWS Cognito

| Aspect | Details |
|:-------|:--------|
| **Type** | Managed identity service |
| **Features** | User pools, identity pools, social login |
| **MFA** | SMS, TOTP, email |
| **Cost** | Free tier: 50K MAU, then $0.0055/MAU |
| **Integration** | Native AWS, SDKs |

#### 5.3.2 Auth0

| Aspect | Details |
|:-------|:--------|
| **Type** | IDaaS |
| **Features** | Universal login, social connections |
| **MFA** | Multiple factors |
| **Cost** | Free: 7.5K users, Paid: $23+/month |
| **Integration** | Extensive SDKs |

#### 5.3.3 Keycloak

| Aspect | Details |
|:-------|:--------|
| **Type** | Open-source IAM |
| **Features** | SSO, identity brokering |
| **MFA** | OTP, WebAuthn |
| **Cost** | Free (self-hosted) |
| **Integration** | OIDC, SAML |

### 5.4 Comparison Matrix - API Gateway

| Criteria | Max Points | AWS API GW | Kong | Nginx |
|:---------|:----------:|:----------:|:----:|:-----:|
| **AWS Integration** | 20 | 20 | 12 | 10 |
| **Ease of Management** | 20 | 18 | 14 | 10 |
| **Features** | 15 | 14 | 15 | 10 |
| **Performance** | 15 | 14 | 15 | 15 |
| **Cost** | 15 | 12 | 14 | 15 |
| **Scalability** | 10 | 10 | 9 | 8 |
| **Monitoring** | 5 | 5 | 4 | 3 |
| **Total Score** | **100** | **93** | 83 | 71 |

### 5.5 Comparison Matrix - Authentication

| Criteria | Max Points | AWS Cognito | Auth0 | Keycloak |
|:---------|:----------:|:-----------:|:-----:|:--------:|
| **AWS Integration** | 20 | 20 | 14 | 10 |
| **Social Login** | 15 | 14 | 15 | 13 |
| **Cost Effectiveness** | 15 | 14 | 10 | 15 |
| **Ease of Setup** | 15 | 14 | 15 | 10 |
| **Security Features** | 15 | 14 | 15 | 14 |
| **Scalability** | 10 | 10 | 10 | 8 |
| **Customization** | 10 | 8 | 9 | 10 |
| **Total Score** | **100** | **94** | 88 | 80 |

### 5.6 Recommendation: AWS API Gateway + Cognito

**Selected:** AWS API Gateway + AWS Cognito + Nginx (internal)

| Factor | Rationale |
|:-------|:----------|
| **AWS Native** | Seamless integration with other AWS services |
| **Managed Service** | Reduced operational overhead |
| **Cost Effective** | Cognito free tier covers initial scale |
| **Security** | Built-in WAF integration, DDoS protection |
| **Social Login** | Native Google, Facebook, Apple support |
| **Scalability** | Auto-scaling, no capacity planning |

**Architecture:**

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     API Gateway + Authentication Flow                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   Client (Web/Mobile)                                                       │
│         │                                                                   │
│         ▼                                                                   │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │                      Amazon CloudFront                               │  │
│   │                      (CDN + WAF)                                     │  │
│   └────────────────────────────┬────────────────────────────────────────┘  │
│                                │                                            │
│         ┌──────────────────────┴──────────────────────┐                    │
│         │                                             │                    │
│         ▼                                             ▼                    │
│   ┌─────────────────┐                          ┌─────────────────┐        │
│   │  AWS Cognito    │                          │ AWS API Gateway │        │
│   │                 │                          │                 │        │
│   │ • User Pools    │◀─── Token Validation ───│ • REST APIs     │        │
│   │ • Social Login  │                          │ • Rate Limiting │        │
│   │ • MFA           │                          │ • Throttling    │        │
│   │ • JWT Tokens    │                          │                 │        │
│   └─────────────────┘                          └────────┬────────┘        │
│                                                         │                  │
│                                                         ▼                  │
│                                                ┌─────────────────┐         │
│                                                │  Nginx (ALB)    │         │
│                                                │  Load Balancer  │         │
│                                                └────────┬────────┘         │
│                                                         │                  │
│                                                         ▼                  │
│                                                ┌─────────────────┐         │
│                                                │  Microservices  │         │
│                                                │  (EKS Pods)     │         │
│                                                └─────────────────┘         │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Database Technologies

### 6.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| DB-01 | ACID transactions for orders/payments | Critical |
| DB-02 | Flexible schema for products | High |
| DB-03 | High-performance caching | Critical |
| DB-04 | Horizontal scalability | High |
| DB-05 | Managed service preferred | High |
| DB-06 | Backup and point-in-time recovery | Critical |
| DB-07 | Multi-AZ for high availability | High |

### 6.2 Database Strategy: Polyglot Persistence

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      Polyglot Persistence Strategy                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌───────────────────────────────────────────────────────────────────┐    │
│   │                     PostgreSQL (Amazon RDS)                        │    │
│   │                                                                    │    │
│   │  Use Cases:                                                        │    │
│   │  • Users & Authentication                                          │    │
│   │  • Orders & Order Items                                            │    │
│   │  • Payments & Transactions                                         │    │
│   │  • Shopping Carts                                                  │    │
│   │  • Reviews & Ratings                                               │    │
│   │  • Inventory (transactional)                                       │    │
│   │                                                                    │    │
│   │  Why: ACID compliance, relational integrity, mature tooling        │    │
│   └───────────────────────────────────────────────────────────────────┘    │
│                                                                             │
│   ┌───────────────────────────────────────────────────────────────────┐    │
│   │                  DocumentDB (MongoDB Compatible)                   │    │
│   │                                                                    │    │
│   │  Use Cases:                                                        │    │
│   │  • Product Catalog (flexible attributes)                           │    │
│   │  • Categories & Brands                                             │    │
│   │  • Product Variants                                                │    │
│   │  • Seller Profiles                                                 │    │
│   │                                                                    │    │
│   │  Why: Flexible schema, embedded documents, natural fit for catalog │    │
│   └───────────────────────────────────────────────────────────────────┘    │
│                                                                             │
│   ┌───────────────────────────────────────────────────────────────────┐    │
│   │                     Redis (Amazon ElastiCache)                     │    │
│   │                                                                    │    │
│   │  Use Cases:                                                        │    │
│   │  • Session storage                                                 │    │
│   │  • Cart caching (fast access)                                      │    │
│   │  • Product cache (frequently accessed)                             │    │
│   │  • Inventory cache (real-time stock)                               │    │
│   │  • Rate limiting counters                                          │    │
│   │  • Distributed locks                                               │    │
│   │                                                                    │    │
│   │  Why: Sub-millisecond latency, rich data structures, pub/sub       │    │
│   └───────────────────────────────────────────────────────────────────┘    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6.3 Relational Database Comparison

| Criteria | Max Points | PostgreSQL | MySQL | SQL Server |
|:---------|:----------:|:----------:|:-----:|:----------:|
| **Feature Set** | 20 | 20 | 16 | 18 |
| **JSONB Support** | 15 | 15 | 10 | 12 |
| **Performance** | 15 | 14 | 14 | 14 |
| **AWS RDS Support** | 15 | 15 | 15 | 14 |
| **Cost** | 15 | 14 | 14 | 8 |
| **Community/Ecosystem** | 10 | 9 | 9 | 8 |
| **Extensions** | 10 | 10 | 7 | 6 |
| **Total Score** | **100** | **97** | 85 | 80 |

### 6.4 Document Database Comparison

| Criteria | Max Points | DocumentDB | DynamoDB | MongoDB Atlas |
|:---------|:----------:|:----------:|:--------:|:-------------:|
| **AWS Integration** | 20 | 20 | 20 | 12 |
| **MongoDB Compatibility** | 20 | 18 | 0 | 20 |
| **Query Flexibility** | 15 | 14 | 10 | 15 |
| **Aggregation Pipeline** | 15 | 14 | 8 | 15 |
| **Cost** | 15 | 12 | 14 | 10 |
| **Scalability** | 10 | 9 | 10 | 9 |
| **Management** | 5 | 5 | 5 | 4 |
| **Total Score** | **100** | **92** | 67 | 85 |

### 6.5 Cache Comparison

| Criteria | Max Points | Redis | Memcached | Hazelcast |
|:---------|:----------:|:-----:|:---------:|:---------:|
| **Data Structures** | 20 | 20 | 8 | 16 |
| **Persistence** | 15 | 15 | 0 | 12 |
| **Performance** | 20 | 19 | 20 | 17 |
| **AWS ElastiCache** | 15 | 15 | 15 | 8 |
| **Pub/Sub** | 10 | 10 | 0 | 8 |
| **Cluster Mode** | 10 | 9 | 8 | 9 |
| **Ecosystem** | 10 | 10 | 7 | 6 |
| **Total Score** | **100** | **98** | 58 | 76 |

### 6.6 Recommendation: PostgreSQL + DocumentDB + Redis

| Database | AWS Service | Use Case |
|:---------|:------------|:---------|
| **PostgreSQL** | Amazon RDS | Transactional data (orders, users, payments) |
| **DocumentDB** | Amazon DocumentDB | Product catalog (flexible schema) |
| **Redis** | Amazon ElastiCache | Caching, sessions, real-time data |

**Configuration:**

| Service | Development | Production |
|:--------|:------------|:-----------|
| **RDS PostgreSQL** | db.t3.micro | db.r6g.large (Multi-AZ) |
| **DocumentDB** | db.t3.medium | db.r6g.large (3 nodes) |
| **ElastiCache Redis** | cache.t3.micro | cache.r6g.large (cluster) |

---

## 7. Search Engine

### 7.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| SE-01 | Full-text search | Critical |
| SE-02 | Autocomplete/suggestions | High |
| SE-03 | Faceted navigation | High |
| SE-04 | Relevance ranking | High |
| SE-05 | Fuzzy matching | Medium |
| SE-06 | < 100ms latency | Critical |

### 7.2 Comparison Matrix

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

### 7.3 Recommendation: Amazon OpenSearch

**Selected:** Amazon OpenSearch Service

| Factor | Rationale |
|:-------|:----------|
| **AWS Native** | Managed service with VPC integration |
| **Cost Effective** | All features free (security, alerting, ML) |
| **Open Source** | Apache 2.0 license, no vendor lock-in |
| **Feature Rich** | Full-text search, aggregations, k-NN |
| **API Compatible** | Elasticsearch 7.x compatible |

---

## 8. Message Broker

### 8.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| MB-01 | Reliable message delivery | Critical |
| MB-02 | Message routing patterns | High |
| MB-03 | Dead letter queues | High |
| MB-04 | Message persistence | High |
| MB-05 | .NET integration | Critical |
| MB-06 | AWS managed service | High |

### 8.2 Available Options

#### 8.2.1 Amazon MQ (RabbitMQ)

| Aspect | Details |
|:-------|:--------|
| **Type** | Managed message broker |
| **Protocol** | AMQP 0.9.1 |
| **Patterns** | Pub/Sub, Work Queues, Routing |
| **.NET** | MassTransit, RabbitMQ.Client |
| **Cost** | Instance-based pricing |

#### 8.2.2 Amazon SQS + SNS

| Aspect | Details |
|:-------|:--------|
| **Type** | Managed queue service |
| **Protocol** | HTTP/HTTPS |
| **Patterns** | Queue, Fan-out |
| **.NET** | AWS SDK, MassTransit |
| **Cost** | Pay per request |

#### 8.2.3 Amazon MSK (Kafka)

| Aspect | Details |
|:-------|:--------|
| **Type** | Managed Kafka |
| **Protocol** | Kafka protocol |
| **Patterns** | Event streaming, log aggregation |
| **.NET** | Confluent.Kafka |
| **Cost** | Instance + storage based |

### 8.3 Comparison Matrix

| Criteria | Max Points | Amazon MQ | SQS + SNS | MSK (Kafka) |
|:---------|:----------:|:---------:|:---------:|:-----------:|
| **Message Patterns** | 20 | 19 | 14 | 18 |
| **Reliability** | 20 | 18 | 19 | 19 |
| **.NET Integration** | 15 | 15 | 13 | 12 |
| **Ease of Use** | 15 | 13 | 15 | 10 |
| **Cost** | 15 | 11 | 15 | 9 |
| **Scalability** | 10 | 8 | 10 | 10 |
| **Learning Curve** | 5 | 4 | 5 | 2 |
| **Total Score** | **100** | **88** | 91 | 80 |

### 8.4 Recommendation: Amazon MQ (RabbitMQ)

**Selected:** Amazon MQ with RabbitMQ engine

| Factor | Rationale |
|:-------|:----------|
| **MassTransit Support** | Excellent integration with MassTransit |
| **Message Patterns** | Rich routing, exchanges, dead letter |
| **Reliability** | Persistent messages, acknowledgments |
| **Team Familiarity** | RabbitMQ is well-known |
| **Management Console** | Built-in RabbitMQ management UI |

**Note:** SQS + SNS scored higher and could be used for simpler scenarios. RabbitMQ selected for:
- Complex routing patterns (topic exchanges)
- MassTransit saga support
- Message acknowledgment patterns
- Outbox pattern implementation

---

## 9. Cloud Provider

### 9.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| CP-01 | Managed Kubernetes service | Critical |
| CP-02 | Managed database services | Critical |
| CP-03 | Global CDN | High |
| CP-04 | Identity management | Critical |
| CP-05 | Cost management tools | High |
| CP-06 | India region availability | Critical |

### 9.2 Comparison Matrix

| Criteria | Max Points | AWS | Azure | GCP |
|:---------|:----------:|:---:|:-----:|:---:|
| **Service Breadth** | 20 | 20 | 18 | 17 |
| **India Regions** | 15 | 15 | 14 | 12 |
| **Managed Databases** | 15 | 15 | 14 | 13 |
| **Kubernetes (EKS/AKS/GKE)** | 15 | 14 | 14 | 15 |
| **Cost** | 15 | 13 | 13 | 14 |
| **Team Expertise** | 10 | 9 | 6 | 5 |
| **Documentation** | 5 | 5 | 4 | 4 |
| **Enterprise Support** | 5 | 5 | 5 | 4 |
| **Total Score** | **100** | **96** | 88 | 84 |

### 9.3 Recommendation: AWS

**Selected:** Amazon Web Services (AWS)

| Factor | Rationale |
|:-------|:----------|
| **Market Leader** | Largest cloud provider, mature services |
| **India Presence** | Mumbai (ap-south-1), Hyderabad regions |
| **Service Breadth** | All required services available |
| **Team Expertise** | Team has AWS experience |
| **Enterprise Support** | Strong enterprise support options |

**AWS Services Used:**

| Category | AWS Service |
|:---------|:------------|
| Compute | EKS, EC2 (worker nodes) |
| Database | RDS, DocumentDB, ElastiCache |
| Search | OpenSearch Service |
| Messaging | Amazon MQ |
| Storage | S3 |
| CDN | CloudFront |
| Auth | Cognito |
| API | API Gateway |
| Monitoring | CloudWatch, X-Ray |
| Secrets | Secrets Manager |
| DNS | Route 53 |
| Registry | ECR |

---

## 10. Container Orchestration

### 10.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| CO-01 | Auto-scaling | Critical |
| CO-02 | Service discovery | Critical |
| CO-03 | Load balancing | Critical |
| CO-04 | Rolling deployments | High |
| CO-05 | Health checks | High |
| CO-06 | Resource management | High |

### 10.2 Comparison Matrix

| Criteria | Max Points | EKS | ECS | ECS Fargate |
|:---------|:----------:|:---:|:---:|:-----------:|
| **Flexibility** | 20 | 20 | 14 | 12 |
| **Kubernetes Standard** | 20 | 20 | 5 | 5 |
| **Ecosystem** | 15 | 15 | 10 | 10 |
| **Operational Overhead** | 15 | 10 | 13 | 15 |
| **Cost** | 15 | 11 | 13 | 12 |
| **Scalability** | 10 | 10 | 9 | 10 |
| **Portability** | 5 | 5 | 2 | 2 |
| **Total Score** | **100** | **91** | 66 | 66 |

### 10.3 Recommendation: Amazon EKS

**Selected:** Amazon Elastic Kubernetes Service (EKS)

| Factor | Rationale |
|:-------|:----------|
| **Industry Standard** | Kubernetes is the de-facto standard |
| **Portability** | Can migrate to other clouds if needed |
| **Ecosystem** | Helm, Istio, ArgoCD, rich tooling |
| **Flexibility** | Full control over cluster configuration |
| **Multi-cloud Ready** | Skills transfer to any Kubernetes |

**Cluster Configuration:**

| Setting | Development | Production |
|:--------|:------------|:-----------|
| **Node Type** | t3.medium | m6i.large |
| **Node Count** | 2 | 3-6 (auto-scaling) |
| **Namespaces** | default | dev, staging, prod |
| **Ingress** | Nginx | AWS ALB Ingress Controller |

---

## 11. CI/CD Pipeline

### 11.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| CI-01 | Automated builds | Critical |
| CI-02 | Automated testing | Critical |
| CI-03 | Container image building | Critical |
| CI-04 | Security scanning | High |
| CI-05 | Deployment automation | Critical |
| CI-06 | Rollback capability | High |

### 11.2 Available Options

#### 11.2.1 Jenkins

| Aspect | Details |
|:-------|:--------|
| **Type** | Self-hosted CI/CD |
| **Flexibility** | Highly customizable |
| **Plugins** | 1800+ plugins |
| **Cost** | Free (self-hosted) |
| **Learning Curve** | Moderate |

#### 11.2.2 GitHub Actions

| Aspect | Details |
|:-------|:--------|
| **Type** | Cloud CI/CD |
| **Integration** | Native GitHub |
| **Marketplace** | Large action marketplace |
| **Cost** | Free tier, then usage-based |
| **Learning Curve** | Low |

#### 11.2.3 AWS CodePipeline

| Aspect | Details |
|:-------|:--------|
| **Type** | Managed CI/CD |
| **Integration** | AWS native |
| **Flexibility** | AWS-focused |
| **Cost** | $1/pipeline/month |
| **Learning Curve** | Low-Moderate |

### 11.3 Comparison Matrix

| Criteria | Max Points | Jenkins | GitHub Actions | CodePipeline |
|:---------|:----------:|:-------:|:--------------:|:------------:|
| **Flexibility** | 20 | 20 | 16 | 14 |
| **Plugin Ecosystem** | 15 | 15 | 14 | 10 |
| **AWS Integration** | 15 | 13 | 12 | 15 |
| **Ease of Setup** | 15 | 10 | 15 | 13 |
| **Cost** | 15 | 14 | 13 | 12 |
| **Self-hosted Option** | 10 | 10 | 8 | 5 |
| **Community** | 10 | 9 | 10 | 7 |
| **Total Score** | **100** | **91** | 88 | 76 |

### 11.4 Recommendation: Jenkins + Docker + Helm

**Selected:** Jenkins with Docker and Helm for Kubernetes deployments

| Factor | Rationale |
|:-------|:----------|
| **Flexibility** | Full control over pipeline configuration |
| **Self-hosted** | No per-minute charges, data stays internal |
| **Plugins** | AWS, Docker, Kubernetes plugins available |
| **Blue/Green** | Easy to implement with Kubernetes |

**CI/CD Pipeline Architecture:**

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           CI/CD Pipeline Flow                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐ │
│   │  GitHub │───▶│ Jenkins │───▶│  Build  │───▶│  Test   │───▶│  Scan   │ │
│   │  Push   │    │ Trigger │    │ .NET    │    │ xUnit   │    │ Trivy   │ │
│   └─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘ │
│                                                                      │      │
│                                                                      ▼      │
│   ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐ │
│   │   EKS   │◀───│  Helm   │◀───│  Push   │◀───│ Docker  │◀───│ Quality │ │
│   │ Deploy  │    │ Upgrade │    │  ECR    │    │  Build  │    │  Gate   │ │
│   └─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘ │
│                                                                             │
│   Deployment Strategy: Blue/Green                                           │
│   ─────────────────────────────                                            │
│   1. Deploy to Green environment                                            │
│   2. Run smoke tests                                                        │
│   3. Switch traffic (ALB target group)                                      │
│   4. Monitor for issues                                                     │
│   5. Rollback if needed (switch back to Blue)                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Tools Stack:**

| Tool | Purpose |
|:-----|:--------|
| **Jenkins** | CI/CD orchestration |
| **Docker** | Container building |
| **Amazon ECR** | Container registry |
| **Helm** | Kubernetes package manager |
| **Trivy** | Security scanning |
| **SonarQube** | Code quality (optional) |

---

## 12. Monitoring & Observability

### 12.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| MO-01 | Metrics collection | Critical |
| MO-02 | Log aggregation | Critical |
| MO-03 | Distributed tracing | High |
| MO-04 | Alerting | Critical |
| MO-05 | Dashboards | High |
| MO-06 | APM | Medium |

### 12.2 Comparison Matrix

| Criteria | Max Points | CloudWatch + X-Ray | Datadog | New Relic |
|:---------|:----------:|:------------------:|:-------:|:---------:|
| **AWS Integration** | 25 | 25 | 20 | 18 |
| **Features** | 20 | 16 | 20 | 20 |
| **Ease of Setup** | 15 | 14 | 15 | 14 |
| **Cost** | 20 | 18 | 10 | 10 |
| **Dashboards** | 10 | 8 | 10 | 10 |
| **Alerting** | 10 | 9 | 10 | 10 |
| **Total Score** | **100** | **90** | 85 | 82 |

### 12.3 Recommendation: CloudWatch + X-Ray

**Selected:** Amazon CloudWatch + AWS X-Ray

| Factor | Rationale |
|:-------|:----------|
| **AWS Native** | Integrated with all AWS services |
| **Cost Effective** | No per-host licensing |
| **Tracing** | X-Ray for distributed tracing |
| **Logs** | CloudWatch Logs Insights for queries |
| **Alarms** | SNS integration for notifications |

**Observability Stack:**

| Component | Tool |
|:----------|:-----|
| **Metrics** | CloudWatch Metrics |
| **Logs** | CloudWatch Logs |
| **Tracing** | AWS X-Ray |
| **Alerting** | CloudWatch Alarms + SNS |
| **Dashboards** | CloudWatch Dashboards |
| **Application Logs** | Serilog → CloudWatch |

---

## 13. Storage Services

### 13.1 Requirements

| ID | Requirement | Priority |
|:---|:------------|:---------|
| ST-01 | Object storage for images | Critical |
| ST-02 | CDN integration | High |
| ST-03 | Lifecycle policies | Medium |
| ST-04 | Cost optimization | High |
| ST-05 | High durability | Critical |

### 13.2 Recommendation: Amazon S3 + CloudFront

**Selected:** Amazon S3 with CloudFront CDN

| Factor | Rationale |
|:-------|:----------|
| **Durability** | 99.999999999% (11 9's) |
| **Integration** | Native CloudFront integration |
| **Cost** | Pay for what you use |
| **Lifecycle** | Automatic tiering (S3 Intelligent-Tiering) |
| **Security** | Bucket policies, pre-signed URLs |

**Storage Structure:**

```
amcart-assets-bucket/
├── products/
│   ├── images/
│   │   ├── {product_id}/
│   │   │   ├── original/
│   │   │   ├── large/
│   │   │   ├── medium/
│   │   │   └── thumbnail/
├── brands/
│   └── logos/
├── categories/
│   └── images/
├── users/
│   └── avatars/
└── temp/
    └── uploads/
```

---

## 14. Consolidated Technology Stack

### 14.1 Complete Stack Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    AmCart Complete Technology Stack                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  LAYER                    TECHNOLOGY                    AWS SERVICE         │
│  ─────                    ──────────                    ───────────         │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ FRONTEND               Nuxt.js 3 + Vue 3             CloudFront     │   │
│  │                        TypeScript, Tailwind CSS       S3 (static)   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                     │                                       │
│                                     ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ API GATEWAY            AWS API Gateway                API Gateway   │   │
│  │                        + Nginx (internal LB)          ALB           │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                     │                                       │
│                                     ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ AUTHENTICATION         AWS Cognito                    Cognito       │   │
│  │                        JWT, OAuth 2.0, Social Login                 │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                     │                                       │
│                                     ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ BACKEND                .NET 8 / C# 12                 EKS           │   │
│  │                        ASP.NET Core, EF Core          (Kubernetes)  │   │
│  │                        MassTransit, MediatR                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                     │                                       │
│          ┌──────────────────────────┼──────────────────────────┐           │
│          │                          │                          │           │
│          ▼                          ▼                          ▼           │
│  ┌───────────────┐         ┌───────────────┐         ┌───────────────┐    │
│  │ SQL DATABASE  │         │ DOCUMENT DB   │         │    CACHE      │    │
│  │               │         │               │         │               │    │
│  │ PostgreSQL    │         │ DocumentDB    │         │ Redis         │    │
│  │ (RDS)         │         │ (MongoDB)     │         │ (ElastiCache) │    │
│  └───────────────┘         └───────────────┘         └───────────────┘    │
│                                                                             │
│  ┌───────────────┐         ┌───────────────┐         ┌───────────────┐    │
│  │ SEARCH        │         │ MESSAGING     │         │ STORAGE       │    │
│  │               │         │               │         │               │    │
│  │ OpenSearch    │         │ RabbitMQ      │         │ S3            │    │
│  │ (AWS)         │         │ (Amazon MQ)   │         │ + CloudFront  │    │
│  └───────────────┘         └───────────────┘         └───────────────┘    │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ CI/CD                  Jenkins + Docker + Helm       ECR           │   │
│  │                        Blue/Green Deployment                        │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ MONITORING             CloudWatch + X-Ray            CloudWatch    │   │
│  │                        Serilog                       X-Ray         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 14.2 Technology Decisions Summary

| # | Category | Technology | Score | Runner-up | Key Differentiator |
|:-:|:---------|:-----------|:-----:|:----------|:-------------------|
| 1 | Frontend | Nuxt.js 3 | 89/100 | Next.js (90) | Team expertise, Vue preference |
| 2 | Backend | .NET 8 | 92/100 | Spring Boot (86) | Performance, team expertise |
| 3 | API Gateway | AWS API Gateway | 93/100 | Kong (83) | AWS native, managed |
| 4 | Auth | AWS Cognito | 94/100 | Auth0 (88) | Cost, AWS integration |
| 5 | SQL DB | PostgreSQL | 97/100 | MySQL (85) | JSONB, features |
| 6 | Document DB | DocumentDB | 92/100 | MongoDB Atlas (85) | AWS native |
| 7 | Cache | Redis | 98/100 | Memcached (58) | Data structures |
| 8 | Search | OpenSearch | 92/100 | Algolia (85) | AWS native, cost |
| 9 | Messaging | Amazon MQ | 88/100 | SQS (91) | Routing patterns |
| 10 | Cloud | AWS | 96/100 | Azure (88) | Services, expertise |
| 11 | Container | EKS | 91/100 | ECS (66) | Kubernetes standard |
| 12 | CI/CD | Jenkins | 91/100 | GitHub Actions (88) | Flexibility, self-hosted |
| 13 | Monitoring | CloudWatch | 90/100 | Datadog (85) | AWS native, cost |
| 14 | Storage | S3 | N/A | N/A | Industry standard |

### 14.3 Version Matrix

| Technology | Version | LTS/Support Until |
|:-----------|:--------|:------------------|
| .NET | 8.0 | November 2026 |
| C# | 12 | N/A (with .NET 8) |
| Nuxt.js | 3.x | Active development |
| Vue.js | 3.x | Active development |
| TypeScript | 5.x | Active development |
| PostgreSQL | 16 | November 2028 |
| MongoDB API | 5.0 | DocumentDB compatible |
| Redis | 7.x | Active development |
| OpenSearch | 2.x | Active development |
| RabbitMQ | 3.12+ | Active development |
| Kubernetes | 1.28+ | EKS managed |
| Docker | 24.x | Active development |
| Helm | 3.x | Active development |

---

## 15. Assumptions

| ID | Assumption | Impact if Invalid |
|:---|:-----------|:------------------|
| A1 | AWS remains primary cloud provider | Major architecture changes |
| A2 | Team size remains 8-10 developers | May need more/less automation |
| A3 | Traffic stays within projected range | May need different scaling strategy |
| A4 | Budget allows for managed services | Would need self-hosted alternatives |
| A5 | India remains primary market | Multi-region architecture needed |
| A6 | .NET expertise available in team | Training or hiring required |
| A7 | Kubernetes skills can be developed | May need simpler orchestration |
| A8 | Open-source preference continues | May consider commercial alternatives |

---

## 16. Risks

### 16.1 Risk Register

| ID | Risk | Probability | Impact | Mitigation |
|:---|:-----|:------------|:-------|:-----------|
| R1 | Cloud vendor lock-in | Medium | High | Use Kubernetes, standard APIs |
| R2 | Cost overrun | Medium | Medium | Budget alerts, reserved instances |
| R3 | Technology obsolescence | Low | Medium | Use LTS versions, monitor roadmaps |
| R4 | Skill gaps in team | Medium | High | Training, documentation, hiring |
| R5 | Integration complexity | Medium | Medium | Start simple, iterate |
| R6 | Performance issues | Low | High | Load testing, monitoring |
| R7 | Security vulnerabilities | Medium | High | Regular scanning, updates |
| R8 | Data loss | Low | Critical | Backups, multi-AZ, replication |

### 16.2 Risk Response Plan

| Risk | Strategy | Actions |
|:-----|:---------|:--------|
| R1 | Mitigate | Abstract cloud services, use Kubernetes |
| R2 | Mitigate | Set budgets, use Spot instances, right-size |
| R3 | Accept | Regular technology reviews |
| R4 | Mitigate | Training budget, pair programming |
| R5 | Mitigate | Incremental implementation, POCs |
| R6 | Mitigate | Performance testing, APM tools |
| R7 | Mitigate | Trivy scanning, dependency updates |
| R8 | Mitigate | Automated backups, disaster recovery |

---

## 17. Appendix

### 17.1 References

| # | Reference | URL |
|:--|:----------|:----|
| 1 | AWS Well-Architected Framework | https://aws.amazon.com/architecture/well-architected/ |
| 2 | .NET 8 Documentation | https://learn.microsoft.com/en-us/dotnet/ |
| 3 | Nuxt.js Documentation | https://nuxt.com/docs |
| 4 | Amazon EKS | https://aws.amazon.com/eks/ |
| 5 | Amazon RDS | https://aws.amazon.com/rds/ |
| 6 | Amazon DocumentDB | https://aws.amazon.com/documentdb/ |
| 7 | Amazon ElastiCache | https://aws.amazon.com/elasticache/ |
| 8 | Amazon OpenSearch | https://aws.amazon.com/opensearch-service/ |
| 9 | AWS Cognito | https://aws.amazon.com/cognito/ |
| 10 | Amazon MQ | https://aws.amazon.com/amazon-mq/ |

### 17.2 Glossary

| Term | Definition |
|:-----|:-----------|
| **ACID** | Atomicity, Consistency, Isolation, Durability |
| **CDN** | Content Delivery Network |
| **CQRS** | Command Query Responsibility Segregation |
| **DDD** | Domain-Driven Design |
| **EKS** | Elastic Kubernetes Service |
| **IAM** | Identity and Access Management |
| **JWT** | JSON Web Token |
| **LTS** | Long Term Support |
| **MAU** | Monthly Active Users |
| **RDS** | Relational Database Service |
| **SSR** | Server-Side Rendering |
| **VPC** | Virtual Private Cloud |

### 17.3 Related Documents

| Document | Description |
|:---------|:------------|
| Main-Design-Plan.md | Overall architecture design |
| API-Specifications.md | API documentation |
| Deployment-Guide.md | Deployment procedures |
| ADR/ | Architecture Decision Records |
| Database-Schema-*.md | Database schema documents |

### 17.4 Revision History

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
| Engineering Manager | | | |
