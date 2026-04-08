# AmCart — E-Commerce Platform

A production-grade e-commerce platform built with a **microservices architecture**, featuring a React SPA storefront, .NET 8 backend services, polyglot persistence, and automated CI/CD to AWS.

---

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Docker Builds](#docker-builds)
- [Deployment](#deployment)
- [CI/CD Pipelines](#cicd-pipelines)
- [Documentation](#documentation)
- [Contributing](#contributing)

---

## Overview

AmCart is a full-featured e-commerce application designed as a monorepo. It demonstrates enterprise patterns including clean architecture, domain-driven design, event-driven communication, polyglot persistence, and cloud-native deployment on AWS.

The platform supports:

- Product catalog browsing with full-text search, faceted filters, and autocomplete
- Shopping cart and checkout orchestration
- User authentication (email/password + social login via Google, Facebook, X/Twitter)
- Admin panel for managing products, categories, brands, sales, testimonials, and coupons
- Order lifecycle management with payment gateway integration
- Real-time notifications via SNS/SQS event bus

---

## Key Features

| Area | Details |
|------|---------|
| **Authentication** | Custom JWT-based auth service with OAuth 2.0 social login (Google, Facebook, X/Twitter using PKCE) |
| **Product Management** | CRUD with multiple images, variants, attributes, tags, categories, and brands |
| **Search** | Elasticsearch (self-hosted) powering full-text search, autocomplete, and faceted filtering |
| **Shopping Cart** | Redis-backed cart with guest session support and coupon application |
| **Checkout** | Orchestrated checkout flow with 3rd-party payment integration (Stripe, PayPal) |
| **Event-Driven** | SNS topics + SQS queues for order events, payment events, stock updates, cache invalidation, and notifications |
| **Admin Dashboard** | Full admin panel for products, categories, brands, sales/discounts, testimonials, and user management |
| **Observability** | Health check endpoints (`/health/live`, `/health/ready`), structured logging, CloudWatch, and X-Ray tracing |
| **Multi-AZ & DR** | Designed for multi-AZ deployment with Pilot Light disaster recovery strategy |

---

## Architecture

```
User → Route 53 → WAF → ALB → Nginx Ingress → Ocelot API Gateway → Microservices → Data Stores
                    ↓
              CloudFront → S3 (React SPA + Static Assets)
```

**Microservices** (running on EKS / Docker Compose):

| Service | Responsibility | Database |
|---------|---------------|----------|
| **UserService** | Auth, JWT, social login, profiles | PostgreSQL (RDS) |
| **ProductService** | Catalog, stock, categories, brands, reviews | MongoDB (DocumentDB) |
| **CartService** | Cart management, coupons, totals | Redis (ElastiCache) |
| **OrderService** | Checkout orchestration, order lifecycle | PostgreSQL (RDS) |
| **PaymentService** | Stripe/PayPal integration | PostgreSQL (RDS) |
| **SearchService** | Elasticsearch client, indexing | Elasticsearch |
| **SaleService** | Discounts, promotional rules | PostgreSQL (RDS) |
| **ReviewService** | Product reviews, testimonials | MongoDB (DocumentDB) |
| **NotificationService** | SQS consumer, SES email dispatch | — |

> See the full architecture diagrams in [`docs/diagrams/`](docs/diagrams/) or [`docs/images/architecture/`](docs/images/architecture/).

---

## Tech Stack

### Backend

| Technology | Version | Purpose |
|-----------|---------|---------|
| .NET | 8.0 | Microservice runtime |
| Entity Framework Core | 8.0 | ORM / data access |
| ASP.NET Core | 8.0 | Web API framework |
| JWT Bearer Auth | 8.0 | Token-based authentication |
| Swashbuckle | 6.6 | Swagger / OpenAPI docs |

### Frontend

| Technology | Version | Purpose |
|-----------|---------|---------|
| React | 18.3 | UI library |
| TypeScript | 5.6 | Type-safe JavaScript |
| Vite | 5.4 | Build tool and dev server |
| React Router | 6.28 | Client-side routing |

### Infrastructure

| Technology | Purpose |
|-----------|---------|
| Docker | Containerization |
| Nginx | Reverse proxy, TLS termination, rate limiting |
| Ocelot | API Gateway (routing, JWT validation, circuit breaker) |
| GitHub Actions | CI/CD pipelines |
| AWS ECR | Container image registry |
| AWS EKS / EC2 | Container orchestration / single-instance deployment |

### Data Stores (Polyglot Persistence)

| Store | Use Case |
|-------|----------|
| **PostgreSQL** (RDS) | Users, orders, sales, payments (ACID transactions) |
| **MongoDB** (DocumentDB) | Product catalog, reviews, testimonials (flexible schema) |
| **Redis** (ElastiCache) | Shopping cart, sessions, cache, stock counters (sub-ms latency) |
| **Elasticsearch** | Full-text search, autocomplete, faceted filtering |

---

## Project Structure

```
AmCart/
├── .github/workflows/       # CI and deployment pipelines
│   ├── ci.yml               # Build, test, Docker smoke on push/PR
│   └── deploy-ec2.yml       # Build → ECR → deploy to EC2 via SSH
│
├── docs/                    # Comprehensive documentation
│   ├── dars/                # docx
|   |   ├── word docx files  # 3 DAR file (2 invalid becuase of PASS)
│   └── diagrams/            # System design artifacts
│       ├── draw.io          # 6 High-Level Design diagrams (.drawio)
│       └── images/
|            ├──erds/        # 6 Database erd diagrams
|            └──architecture/# 6 architecture images for above drawio
├── infra/ec2/               # Single-EC2 deployment
│   ├── docker-compose.yml   # Production compose (Postgres, services, Nginx)
│   ├── nginx/default.conf   # Reverse proxy config
│   ├── scripts/             # Bootstrap, manage, and pull scripts
│   ├── .env.example         # Environment variable template
│   └── README.md            # EC2 setup guide
│
├── src/
│   ├── AmCart.sln            # .NET solution file
│   ├── Services/
│   │   ├── UserService/      # Auth, users, social login (Clean Architecture)
│   │   │   ├── UserService.Domain/
│   │   │   ├── UserService.Application/
│   │   │   ├── UserService.Infrastructure/
│   │   │   ├── UserService.Api/
│   │   │   └── UserService.UnitTests/
│   │   └── ProductService/   # Catalog, stock, categories, brands
│   │       ├── ProductService.Domain/
│   │       ├── ProductService.Application/
│   │       ├── ProductService.Infrastructure/
│   │       └── ProductService.Api/
│   ├── Frontends/
│   │   └── ui-app/           # React SPA (Vite + TypeScript)
│   └── Databases/
│       └── postgres/         # Local Postgres compose + Kustomize
│
├── .gitignore
└── README.md                 # ← You are here
```

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) and npm
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- (Optional) [AWS CLI v2](https://aws.amazon.com/cli/) for deployment

### Run the Backend Locally

```bash
# Start a local PostgreSQL instance
cd src/Databases/postgres
docker compose up -d

# Build and run UserService
cd ../../Services/UserService/UserService.Api
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger at `/swagger`.

### Run the Frontend Locally

```bash
cd src/Frontends/ui-app

# Copy and configure environment variables
cp .env.example .env

# Install dependencies and start dev server
npm install
npm run dev
```

The UI will be available at `http://localhost:5173`.

### Run Tests

```bash
# Run all .NET tests
dotnet test src/AmCart.sln

# Build the frontend (type-check + bundle)
cd src/Frontends/ui-app && npm run build
```

---

## Docker Builds

Build container images from the repo root:

```bash
# UserService
docker build -f src/Services/UserService/Dockerfile -t amcart/user-service:local src

# ProductService
docker build -f src/Services/ProductService/Dockerfile -t amcart/product-service:local src

# UI (React SPA served by Nginx)
docker build -f src/Frontends/ui-app/Dockerfile -t amcart/ui:local src/Frontends/ui-app
```

---

## Deployment

### Single-EC2 Deployment

The `infra/ec2/` directory contains everything needed for a Docker Compose deployment on a single EC2 instance:

1. Set up an EC2 instance with Docker installed (see [`infra/ec2/README.md`](infra/ec2/README.md))
2. Copy `infra/ec2/.env.example` → `.env` and fill in credentials
3. Run `docker compose up -d`

The compose stack includes: PostgreSQL, UserService, ProductService, UI, and Nginx reverse proxy.

### Production (EKS)

The architecture is designed for AWS EKS deployment with:

- Kubernetes manifests for each microservice
- Nginx Ingress Controller for TLS and path-based routing
- Ocelot API Gateway for JWT validation and circuit breaking
- Multi-AZ deployment across 2 availability zones
- Pilot Light disaster recovery in a secondary region

> For full AWS setup details, see the [network architecture diagram](docs/diagrams/network-architecture.drawio) and [deployment infrastructure diagram](docs/diagrams/deployment-infrastructure.drawio).

---

## CI/CD Pipelines

### CI (`ci.yml`)

Triggered on push/PR to `main`/`master`. Runs three parallel jobs:

| Job | What it does |
|-----|-------------|
| **UserService (.NET)** | `dotnet restore` → `build` → `test` |
| **ProductService (.NET)** | `dotnet restore` → `build` → `test` |
| **ui-app (Node)** | `npm ci` → `npm run build` |
| **Docker smoke** | Builds all 3 Docker images (UserService, ProductService, UI) |

### Deploy (`deploy-ec2.yml`)

Triggered on push to `main`/`master`/`userservice` or manual dispatch:

1. Authenticates to AWS via OIDC
2. Builds and pushes Docker images to Amazon ECR (tagged with git SHA + `latest`)
3. SSH into EC2 and runs `docker compose pull && up -d`

---

## Documentation

This project includes extensive documentation covering architecture, design decisions, and operations.

### Decision Analysis & Resolution (DARs)

| Document | Topic |
|----------|-------|
| [DAR — Cloud Provider](docs/dars/DAR-CloudProvider-Selection.docx) | AWS vs Azure vs GCP |
| [DAR — Polyglot Database](docs/dars/DAR-Ployglot-Database-Selection.docx) | Database architecture selection |
| [DAR — Self-Hosted Search](docs/dars/DAR-Search-Engine-Selection.docx) | Elasticsearch vs Meilisearch vs PG FTS vs Solr |
| [DAR — OpenSearch - Not using](docs/dars/DAR-Open-Search-Selection.docx) | Search engine managed service selection |
| [DAR — AWS Cognito - Not using](docs/dars/DAR-AWSCognito-Selection.docx) | .NET vs Node.js vs Java |

### Architecture & High-Level Design Diagrams

All diagrams are in `.drawio` format (open with [draw.io](https://app.diagrams.net/)):

| Diagram | Description |
|---------|------------|
| [AWS Infrastructure](docs/diagrams/aws-infrastructure.drawio) [Image](docs/images/architecture/aws-infrastructure.png) | Detailed AWS services with official icons |
| [Network Architecture](docs/diagrams/network-architecture.drawio) [Image](docs/images/architecture/network-architecture.png) [Image-DR](docs/images/architecture/network-architecture-disaster-recovery.png) | AWS VPC, subnets, EKS, data stores, DR |
| [Data Flow](docs/diagrams/data-flow.drawio) [Image](docs/images/architecture/data-flow.png) | Polyglot persistence and data flow |
| [CI CD](docs/diagrams/cicd-pipeline.drawio) [Image](docs/images/architecture/cicd-pipeline.png)[Image-future ci/cd](docs/images/architecture/cicd-pipeline-future.png) | CI, CD, HELM and K8s |
| [Deployment Infrastructure](docs/diagrams/deployment-infrastructure.drawio) [Image](docs/images/architecture/deployment-infrastructure.png) | AWS deployment topology |
| [System Context](docs/diagrams/system-context.drawio) [Image](docs/images/architecture/system-context.png) | system boundary and actors |

### Database ERDs

| Domain | Diagram |
|--------|---------|
| [User Domain (PostgreSQL)](docs/images/erds/postgresql-user-domain.png) | Users, roles, social login |
| [Order Domain (PostgreSQL)](docs/images/erds/postgresql-order-domain.png) | Orders, line items, addresses |
| [Product Catalog (MongoDB)](docs/images/erds/mongodb-product-catalog.png) | Products, variants, attributes |
| [Engagement (MongoDB)](docs/images/erds/mongodb-engagement.png) | Reviews, testimonials, wishlists |
| [Redis Data Structures](docs/images/erds/redis-data-structures.png) | Cart, sessions, cache, counters |
| [Elasticsearch Index](docs/images/erds/elasticsearch-product-index.png) | Product search index mapping |



---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes following conventional commit messages
4. Push to the branch and open a Pull Request
5. Ensure CI passes (build, tests, Docker smoke)

**Do not commit:** `node_modules/`, `bin/`/`obj/`, `dist/`, `.env` files, `.pem` keys, or `infra/ec2/.env`. See [.gitignore](.gitignore) for the full exclusion list.

---

## License

This project is part of the NAGP (Nagarro Advanced Growth Program) assignment.


**Owner:** Saurabh Kaushik
