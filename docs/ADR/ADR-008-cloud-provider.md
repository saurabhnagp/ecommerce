# ADR-008: Cloud Provider Selection

## Status
**Accepted**

## Date
2024-12-19

## Context

AmCart needs a cloud provider for:
- Hosting microservices (containers)
- Managed databases (PostgreSQL, MongoDB, Redis, Elasticsearch)
- Object storage (images, static files)
- CDN (content delivery)
- Networking and security
- Monitoring and logging
- CI/CD pipelines (optional)

Requirements:
- High availability (multi-AZ)
- Managed services to reduce operations
- Good .NET support
- Cost-effective for startup scale
- Room to grow

## Decision

We will use **Amazon Web Services (AWS)** as the cloud provider.

### AWS Services Used

| Category | Service | Purpose |
|----------|---------|---------|
| Compute | EKS (Kubernetes) | Container orchestration |
| Compute | EC2/Fargate | Worker nodes |
| Database | RDS PostgreSQL | Relational database |
| Database | DocumentDB | MongoDB-compatible |
| Cache | ElastiCache Redis | Caching, sessions |
| Search | OpenSearch | Full-text search |
| Storage | S3 | Object storage |
| CDN | CloudFront | Content delivery |
| Networking | VPC, ALB, Route53 | Networking, DNS |
| Security | Secrets Manager, IAM | Secret management |
| Monitoring | CloudWatch, X-Ray | Logging, tracing |
| Messaging | Amazon MQ | RabbitMQ managed |

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                           AWS Region                             │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                     Route 53 (DNS)                        │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    CloudFront (CDN)                       │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │               Application Load Balancer                   │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                        EKS Cluster                        │  │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐          │  │
│  │  │ User Service │ │Product Svc │ │ Order Svc   │ ...      │  │
│  │  └─────────────┘ └─────────────┘ └─────────────┘          │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                   Private Subnets                         │  │
│  │  ┌─────────┐ ┌─────────────┐ ┌───────────┐ ┌──────────┐  │  │
│  │  │RDS Pgsql│ │ DocumentDB  │ │ElastiCache│ │OpenSearch│  │  │
│  │  │         │ │  (MongoDB)  │ │  (Redis)  │ │          │  │  │
│  │  └─────────┘ └─────────────┘ └───────────┘ └──────────┘  │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  S3 (Images, Static Files)                                │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Consequences

### Positive

- **Market Leader**: Most mature cloud platform
- **Managed Services**: Reduces operational burden
- **Global Reach**: 30+ regions worldwide
- **Ecosystem**: Largest ecosystem of services
- **.NET Support**: Good AWS SDK for .NET
- **Documentation**: Extensive documentation
- **Compliance**: SOC, PCI, HIPAA certified
- **Scalability**: Can scale to any size

### Negative

- **Cost**: Can be expensive at scale
- **Complexity**: Many services to learn
- **Vendor Lock-in**: Migration to other clouds is costly
- **Pricing Model**: Complex pricing

### Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Cost | Reserved instances, spot instances, right-sizing |
| Complexity | Start with essential services, expand as needed |
| Lock-in | Use Kubernetes and portable abstractions |
| Pricing | Use AWS Cost Explorer, set budgets |

## Estimated Monthly Costs (Initial)

| Service | Instance/Config | Est. Cost/Month |
|---------|-----------------|-----------------|
| EKS | Control plane | $73 |
| EC2 | 3x t3.medium (nodes) | $100 |
| RDS PostgreSQL | db.t3.medium | $50 |
| DocumentDB | db.t3.medium | $85 |
| ElastiCache | cache.t3.micro | $25 |
| OpenSearch | t3.small.search | $60 |
| S3 | 50 GB | $2 |
| CloudFront | 100 GB transfer | $10 |
| ALB | 1 load balancer | $20 |
| Amazon MQ | mq.t3.micro | $25 |
| **Total** | | **~$450/month** |

*Note: Costs vary by usage. Production will be higher.*

## Alternatives Considered

### 1. Microsoft Azure

**Pros:**
- Best .NET integration
- Azure DevOps
- Good for enterprise

**Cons:**
- Second in market share
- Some services less mature than AWS
- Complex pricing

**Why Rejected:** AWS has broader service offerings and team is more familiar with AWS.

### 2. Google Cloud Platform (GCP)

**Pros:**
- Best Kubernetes support (GKE)
- Good pricing
- Strong in ML/AI

**Cons:**
- Smallest market share
- Fewer managed services
- Less enterprise features

**Why Rejected:** Smaller ecosystem and fewer managed database options.

### 3. Self-hosted / On-premises

**Pros:**
- Full control
- Potentially lower long-term cost
- No vendor lock-in

**Cons:**
- High operational burden
- Upfront capital costs
- Scaling challenges

**Why Rejected:** Operational complexity not suitable for startup phase.

## AWS Account Structure

```
AWS Organization
├── Production Account
│   ├── EKS Cluster (prod)
│   ├── RDS (prod)
│   └── ...
├── Staging Account
│   ├── EKS Cluster (staging)
│   ├── RDS (staging)
│   └── ...
└── Development Account
    ├── EKS Cluster (dev)
    └── Shared databases
```

## References

- [AWS Documentation](https://docs.aws.amazon.com/)
- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)
- [AWS Pricing Calculator](https://calculator.aws/)
- [AWS .NET SDK](https://aws.amazon.com/sdk-for-net/)

