# AmCart Deployment Guide

## Complete Guide for Local Development, Staging, and Production Deployment

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Local Development Setup](#2-local-development-setup)
3. [Docker Deployment](#3-docker-deployment)
4. [AWS Infrastructure Setup](#4-aws-infrastructure-setup)
5. [Kubernetes Deployment (EKS)](#5-kubernetes-deployment-eks)
6. [CI/CD Pipeline Setup](#6-cicd-pipeline-setup)
7. [Database Migrations](#7-database-migrations)
8. [Monitoring & Logging](#8-monitoring--logging)
9. [SSL/TLS Configuration](#9-ssltls-configuration)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. Prerequisites

### 1.1 Development Tools

| Tool | Version | Purpose | Installation |
|------|---------|---------|--------------|
| .NET SDK | 8.0+ | Backend development | [Download](https://dotnet.microsoft.com/download) |
| Node.js | 20.x LTS | Frontend development | [Download](https://nodejs.org) |
| Docker | 24.x+ | Containerization | [Download](https://docker.com) |
| Docker Compose | 2.x+ | Local orchestration | Included with Docker Desktop |
| kubectl | 1.28+ | Kubernetes CLI | [Install](https://kubernetes.io/docs/tasks/tools/) |
| AWS CLI | 2.x | AWS management | [Install](https://aws.amazon.com/cli/) |
| Terraform | 1.6+ | Infrastructure as Code | [Download](https://terraform.io) |
| Helm | 3.x | Kubernetes package manager | [Install](https://helm.sh) |
| Git | Latest | Version control | [Download](https://git-scm.com) |

### 1.2 IDE Recommendations

| IDE | Platform | Plugins/Extensions |
|-----|----------|-------------------|
| **Visual Studio 2022** | Windows | AWS Toolkit, Docker Tools |
| **JetBrains Rider** | Cross-platform | AWS Toolkit, Docker |
| **VS Code** | Cross-platform | C# Dev Kit, Docker, AWS Toolkit |

### 1.3 AWS Account Setup

```bash
# Configure AWS CLI
aws configure
# Enter:
# - AWS Access Key ID
# - AWS Secret Access Key
# - Default region (e.g., ap-south-1 for India)
# - Default output format (json)

# Verify configuration
aws sts get-caller-identity
```

### 1.4 Clone Repository

```bash
# Clone the repository
git clone https://github.com/your-org/amcart.git
cd amcart

# View project structure
ls -la
```

---

## 2. Local Development Setup

### 2.1 Environment Variables

Create `.env` file in root directory:

```env
# Application
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development

# PostgreSQL
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=amcart
POSTGRES_USER=amcart
POSTGRES_PASSWORD=amcart_dev_password

# MongoDB
MONGO_HOST=localhost
MONGO_PORT=27017
MONGO_DB=amcart
MONGO_USER=amcart
MONGO_PASSWORD=amcart_dev_password

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=

# Elasticsearch
ELASTICSEARCH_HOST=localhost
ELASTICSEARCH_PORT=9200

# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=amcart
RABBITMQ_PASSWORD=amcart_dev_password

# JWT
JWT_SECRET=your-super-secret-jwt-key-minimum-32-characters
JWT_ISSUER=amcart-dev
JWT_AUDIENCE=amcart-api
JWT_EXPIRY_MINUTES=15

# Razorpay (Test Keys)
RAZORPAY_KEY_ID=rzp_test_xxxxxxxxxxxxx
RAZORPAY_KEY_SECRET=xxxxxxxxxxxxxxxxxxxxxxxx

# AWS (for S3)
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_REGION=ap-south-1
AWS_S3_BUCKET=amcart-dev-media

# Frontend
NUXT_PUBLIC_API_URL=http://localhost:8000/api/v1
```

### 2.2 Start Infrastructure Services

```bash
# Start only infrastructure (databases, message broker)
docker-compose -f docker-compose.infrastructure.yml up -d

# Verify services are running
docker-compose -f docker-compose.infrastructure.yml ps

# Check logs if needed
docker-compose -f docker-compose.infrastructure.yml logs -f postgres
```

**docker-compose.infrastructure.yml:**

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: amcart-postgres
    environment:
      POSTGRES_USER: amcart
      POSTGRES_PASSWORD: amcart_dev_password
      POSTGRES_DB: amcart
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U amcart"]
      interval: 10s
      timeout: 5s
      retries: 5

  mongodb:
    image: mongo:7
    container_name: amcart-mongodb
    environment:
      MONGO_INITDB_ROOT_USERNAME: amcart
      MONGO_INITDB_ROOT_PASSWORD: amcart_dev_password
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db
    healthcheck:
      test: echo 'db.runCommand("ping").ok' | mongosh localhost:27017/test --quiet
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: amcart-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    container_name: amcart-elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - es_data:/usr/share/elasticsearch/data
    healthcheck:
      test: ["CMD-SHELL", "curl -f http://localhost:9200/_cluster/health || exit 1"]
      interval: 30s
      timeout: 10s
      retries: 5

  rabbitmq:
    image: rabbitmq:3-management
    container_name: amcart-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: amcart
      RABBITMQ_DEFAULT_PASS: amcart_dev_password
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: rabbitmq-diagnostics -q ping
      interval: 30s
      timeout: 10s
      retries: 5

volumes:
  postgres_data:
  mongo_data:
  redis_data:
  es_data:
  rabbitmq_data:
```

### 2.3 Run Backend Services

```bash
# Navigate to solution directory
cd src

# Restore packages
dotnet restore AmCart.sln

# Run database migrations
dotnet ef database update --project Services/UserService/UserService.Infrastructure --startup-project Services/UserService/UserService.Api

dotnet ef database update --project Services/ProductService/ProductService.Infrastructure --startup-project Services/ProductService/ProductService.Api

dotnet ef database update --project Services/OrderService/OrderService.Infrastructure --startup-project Services/OrderService/OrderService.Api

# Run services (each in separate terminal)
cd Services/UserService/UserService.Api && dotnet run
cd Services/ProductService/ProductService.Api && dotnet run
cd Services/CartService/CartService.Api && dotnet run
cd Services/OrderService/OrderService.Api && dotnet run
cd Services/PaymentService/PaymentService.Api && dotnet run

# Or use the run script
./scripts/run-all-services.sh
```

**run-all-services.sh:**

```bash
#!/bin/bash

# Start all services in background
services=(
    "UserService:5001"
    "ProductService:5002"
    "CartService:5003"
    "OrderService:5004"
    "PaymentService:5005"
    "InventoryService:5006"
    "SearchService:5007"
    "NotificationService:5008"
    "ReviewService:5009"
)

for service in "${services[@]}"; do
    IFS=':' read -r name port <<< "$service"
    echo "Starting $name on port $port..."
    cd "Services/$name/$name.Api"
    dotnet run --urls "http://localhost:$port" &
    cd ../../..
done

echo "All services started!"
echo "Press Ctrl+C to stop all services"
wait
```

### 2.4 Run Frontend

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install
# or
pnpm install

# Run development server
npm run dev
# or
pnpm dev

# Frontend will be available at http://localhost:3000
```

### 2.5 Run Nginx (Local)

```bash
# Start Nginx container
docker run -d \
  --name amcart-nginx \
  -p 8000:80 \
  -v $(pwd)/deploy/nginx/nginx.dev.conf:/etc/nginx/nginx.conf:ro \
  --network host \
  nginx:alpine

# Test Nginx
curl http://localhost:8000/health
```

### 2.6 Verify Local Setup

```bash
# Check all services
curl http://localhost:5001/health  # User Service
curl http://localhost:5002/health  # Product Service
curl http://localhost:5003/health  # Cart Service
curl http://localhost:8000/health  # Nginx (API Gateway)
curl http://localhost:3000         # Frontend

# Test API
curl http://localhost:8000/api/v1/products
```

---

## 3. Docker Deployment

### 3.1 Build Docker Images

**Sample Dockerfile for .NET Service:**

```dockerfile
# Dockerfile for UserService
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["Services/UserService/UserService.Api/UserService.Api.csproj", "Services/UserService/UserService.Api/"]
COPY ["Services/UserService/UserService.Application/UserService.Application.csproj", "Services/UserService/UserService.Application/"]
COPY ["Services/UserService/UserService.Domain/UserService.Domain.csproj", "Services/UserService/UserService.Domain/"]
COPY ["Services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "Services/UserService/UserService.Infrastructure/"]
COPY ["BuildingBlocks/Common/Common.Logging/Common.Logging.csproj", "BuildingBlocks/Common/Common.Logging/"]
COPY ["BuildingBlocks/Common/Common.Messaging/Common.Messaging.csproj", "BuildingBlocks/Common/Common.Messaging/"]

RUN dotnet restore "Services/UserService/UserService.Api/UserService.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/Services/UserService/UserService.Api"
RUN dotnet build "UserService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

ENTRYPOINT ["dotnet", "UserService.Api.dll"]
```

**Build all images:**

```bash
# Build all service images
docker-compose -f docker-compose.yml build

# Or build individually
docker build -t amcart/user-service:latest -f src/Services/UserService/Dockerfile src/
docker build -t amcart/product-service:latest -f src/Services/ProductService/Dockerfile src/
docker build -t amcart/cart-service:latest -f src/Services/CartService/Dockerfile src/
docker build -t amcart/order-service:latest -f src/Services/OrderService/Dockerfile src/
docker build -t amcart/payment-service:latest -f src/Services/PaymentService/Dockerfile src/
```

### 3.2 Docker Compose (Full Stack)

**docker-compose.yml:**

```yaml
version: '3.8'

services:
  # ===================
  # Infrastructure
  # ===================
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-amcart}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-amcart_password}
      POSTGRES_DB: ${POSTGRES_DB:-amcart}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U amcart"]
      interval: 10s
      timeout: 5s
      retries: 5

  mongodb:
    image: mongo:7
    environment:
      MONGO_INITDB_ROOT_USERNAME: ${MONGO_USER:-amcart}
      MONGO_INITDB_ROOT_PASSWORD: ${MONGO_PASSWORD:-amcart_password}
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - es_data:/usr/share/elasticsearch/data

  rabbitmq:
    image: rabbitmq:3-management
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER:-amcart}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD:-amcart_password}
    ports:
      - "5672:5672"
      - "15672:15672"

  # ===================
  # API Gateway
  # ===================
  nginx:
    image: nginx:alpine
    ports:
      - "8000:80"
      - "8443:443"
    volumes:
      - ./deploy/nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./deploy/nginx/conf.d:/etc/nginx/conf.d:ro
    depends_on:
      - user-service
      - product-service
      - cart-service
      - order-service
      - payment-service

  # ===================
  # Microservices
  # ===================
  user-service:
    build:
      context: ./src
      dockerfile: Services/UserService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=amcart;Username=amcart;Password=amcart_password
      - Redis__ConnectionString=redis:6379
      - RabbitMQ__Host=rabbitmq
      - JWT__Secret=${JWT_SECRET}
    ports:
      - "5001:5001"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_started
      rabbitmq:
        condition: service_started

  product-service:
    build:
      context: ./src
      dockerfile: Services/ProductService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=amcart;Username=amcart;Password=amcart_password
      - Redis__ConnectionString=redis:6379
      - Elasticsearch__Url=http://elasticsearch:9200
      - RabbitMQ__Host=rabbitmq
    ports:
      - "5002:5002"
    depends_on:
      postgres:
        condition: service_healthy
      elasticsearch:
        condition: service_started

  cart-service:
    build:
      context: ./src
      dockerfile: Services/CartService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - Redis__ConnectionString=redis:6379
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=amcart;Username=amcart;Password=amcart_password
    ports:
      - "5003:5003"
    depends_on:
      - redis
      - postgres

  order-service:
    build:
      context: ./src
      dockerfile: Services/OrderService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=amcart;Username=amcart;Password=amcart_password
      - RabbitMQ__Host=rabbitmq
    ports:
      - "5004:5004"
    depends_on:
      - postgres
      - rabbitmq

  payment-service:
    build:
      context: ./src
      dockerfile: Services/PaymentService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=amcart;Username=amcart;Password=amcart_password
      - Razorpay__KeyId=${RAZORPAY_KEY_ID}
      - Razorpay__KeySecret=${RAZORPAY_KEY_SECRET}
      - RabbitMQ__Host=rabbitmq
    ports:
      - "5005:5005"
    depends_on:
      - postgres
      - rabbitmq

  search-service:
    build:
      context: ./src
      dockerfile: Services/SearchService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - Elasticsearch__Url=http://elasticsearch:9200
      - Redis__ConnectionString=redis:6379
    ports:
      - "5007:5007"
    depends_on:
      - elasticsearch
      - redis

  notification-service:
    build:
      context: ./src
      dockerfile: Services/NotificationService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - MongoDB__ConnectionString=mongodb://amcart:amcart_password@mongodb:27017
      - RabbitMQ__Host=rabbitmq
      - AWS__Region=${AWS_REGION}
      - AWS__AccessKeyId=${AWS_ACCESS_KEY_ID}
      - AWS__SecretAccessKey=${AWS_SECRET_ACCESS_KEY}
    ports:
      - "5008:5008"
    depends_on:
      - mongodb
      - rabbitmq

  review-service:
    build:
      context: ./src
      dockerfile: Services/ReviewService/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Docker
      - MongoDB__ConnectionString=mongodb://amcart:amcart_password@mongodb:27017
      - Redis__ConnectionString=redis:6379
    ports:
      - "5009:5009"
    depends_on:
      - mongodb
      - redis

  # ===================
  # Frontend
  # ===================
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    environment:
      - NUXT_PUBLIC_API_URL=http://nginx/api/v1
    ports:
      - "3000:3000"
    depends_on:
      - nginx

volumes:
  postgres_data:
  mongo_data:
  redis_data:
  es_data:
```

### 3.3 Run Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f user-service

# Check status
docker-compose ps

# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

---

## 4. AWS Infrastructure Setup

### 4.1 Terraform Project Structure

```
deploy/terraform/
├── main.tf
├── variables.tf
├── outputs.tf
├── versions.tf
├── vpc.tf
├── eks.tf
├── rds.tf
├── elasticache.tf
├── opensearch.tf
├── documentdb.tf
├── s3.tf
├── ecr.tf
├── secrets.tf
├── iam.tf
├── terraform.tfvars.example
└── modules/
    ├── vpc/
    ├── eks/
    └── rds/
```

### 4.2 Main Terraform Configuration

**versions.tf:**

```hcl
terraform {
  required_version = ">= 1.6.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.24"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.12"
    }
  }

  backend "s3" {
    bucket         = "amcart-terraform-state"
    key            = "infrastructure/terraform.tfstate"
    region         = "ap-south-1"
    dynamodb_table = "amcart-terraform-locks"
    encrypt        = true
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project     = "AmCart"
      Environment = var.environment
      ManagedBy   = "Terraform"
    }
  }
}
```

**variables.tf:**

```hcl
variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-south-1"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "production"
}

variable "project_name" {
  description = "Project name"
  type        = string
  default     = "amcart"
}

variable "vpc_cidr" {
  description = "VPC CIDR block"
  type        = string
  default     = "10.0.0.0/16"
}

variable "db_username" {
  description = "Database username"
  type        = string
  sensitive   = true
}

variable "db_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}
```

**vpc.tf:**

```hcl
module "vpc" {
  source  = "terraform-aws-modules/vpc/aws"
  version = "5.4.0"

  name = "${var.project_name}-vpc"
  cidr = var.vpc_cidr

  azs             = ["${var.aws_region}a", "${var.aws_region}b", "${var.aws_region}c"]
  private_subnets = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
  public_subnets  = ["10.0.101.0/24", "10.0.102.0/24", "10.0.103.0/24"]

  enable_nat_gateway     = true
  single_nat_gateway     = var.environment != "production"
  enable_dns_hostnames   = true
  enable_dns_support     = true

  # Tags for EKS
  public_subnet_tags = {
    "kubernetes.io/role/elb" = 1
  }

  private_subnet_tags = {
    "kubernetes.io/role/internal-elb" = 1
  }

  tags = {
    "kubernetes.io/cluster/${var.project_name}-eks" = "shared"
  }
}
```

**eks.tf:**

```hcl
module "eks" {
  source  = "terraform-aws-modules/eks/aws"
  version = "19.21.0"

  cluster_name    = "${var.project_name}-eks"
  cluster_version = "1.28"

  vpc_id     = module.vpc.vpc_id
  subnet_ids = module.vpc.private_subnets

  cluster_endpoint_public_access = true

  eks_managed_node_groups = {
    main = {
      name = "${var.project_name}-node-group"

      instance_types = ["t3.medium"]
      capacity_type  = "ON_DEMAND"

      min_size     = 2
      max_size     = 5
      desired_size = 3

      labels = {
        Environment = var.environment
      }
    }
  }

  # Enable IRSA
  enable_irsa = true

  tags = {
    Environment = var.environment
  }
}

# Kubernetes provider configuration
provider "kubernetes" {
  host                   = module.eks.cluster_endpoint
  cluster_ca_certificate = base64decode(module.eks.cluster_certificate_authority_data)

  exec {
    api_version = "client.authentication.k8s.io/v1beta1"
    command     = "aws"
    args        = ["eks", "get-token", "--cluster-name", module.eks.cluster_name]
  }
}

# Helm provider
provider "helm" {
  kubernetes {
    host                   = module.eks.cluster_endpoint
    cluster_ca_certificate = base64decode(module.eks.cluster_certificate_authority_data)

    exec {
      api_version = "client.authentication.k8s.io/v1beta1"
      command     = "aws"
      args        = ["eks", "get-token", "--cluster-name", module.eks.cluster_name]
    }
  }
}
```

**rds.tf:**

```hcl
resource "aws_db_subnet_group" "main" {
  name       = "${var.project_name}-db-subnet"
  subnet_ids = module.vpc.private_subnets

  tags = {
    Name = "${var.project_name}-db-subnet"
  }
}

resource "aws_security_group" "rds" {
  name        = "${var.project_name}-rds-sg"
  description = "Security group for RDS"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [module.eks.node_security_group_id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_db_instance" "main" {
  identifier = "${var.project_name}-postgres"

  engine               = "postgres"
  engine_version       = "15"
  instance_class       = var.environment == "production" ? "db.r6g.large" : "db.t3.medium"
  allocated_storage    = 100
  max_allocated_storage = 500
  storage_type         = "gp3"
  storage_encrypted    = true

  db_name  = "amcart"
  username = var.db_username
  password = var.db_password

  multi_az               = var.environment == "production"
  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [aws_security_group.rds.id]

  backup_retention_period = 7
  backup_window          = "03:00-04:00"
  maintenance_window     = "Mon:04:00-Mon:05:00"

  skip_final_snapshot = var.environment != "production"

  tags = {
    Name = "${var.project_name}-postgres"
  }
}
```

**elasticache.tf:**

```hcl
resource "aws_elasticache_subnet_group" "main" {
  name       = "${var.project_name}-redis-subnet"
  subnet_ids = module.vpc.private_subnets
}

resource "aws_security_group" "redis" {
  name        = "${var.project_name}-redis-sg"
  description = "Security group for Redis"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port       = 6379
    to_port         = 6379
    protocol        = "tcp"
    security_groups = [module.eks.node_security_group_id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_elasticache_replication_group" "main" {
  replication_group_id       = "${var.project_name}-redis"
  description                = "Redis cluster for AmCart"
  node_type                  = var.environment == "production" ? "cache.r6g.large" : "cache.t3.micro"
  num_cache_clusters         = var.environment == "production" ? 2 : 1
  port                       = 6379

  subnet_group_name  = aws_elasticache_subnet_group.main.name
  security_group_ids = [aws_security_group.redis.id]

  at_rest_encryption_enabled = true
  transit_encryption_enabled = true

  automatic_failover_enabled = var.environment == "production"

  tags = {
    Name = "${var.project_name}-redis"
  }
}
```

### 4.3 Deploy Infrastructure

```bash
# Navigate to terraform directory
cd deploy/terraform

# Create terraform.tfvars
cat > terraform.tfvars <<EOF
aws_region   = "ap-south-1"
environment  = "production"
project_name = "amcart"
db_username  = "amcart_admin"
db_password  = "your-secure-password"
EOF

# Initialize Terraform
terraform init

# Plan deployment
terraform plan -out=tfplan

# Apply deployment
terraform apply tfplan

# Get outputs
terraform output
```

---

## 5. Kubernetes Deployment (EKS)

### 5.1 Configure kubectl

```bash
# Update kubeconfig
aws eks update-kubeconfig --name amcart-eks --region ap-south-1

# Verify connection
kubectl cluster-info
kubectl get nodes
```

### 5.2 Create Namespace and Secrets

```bash
# Create namespace
kubectl create namespace amcart

# Create secrets
kubectl create secret generic amcart-secrets \
  --namespace amcart \
  --from-literal=postgres-connection="Host=amcart-postgres.xxx.rds.amazonaws.com;Database=amcart;Username=amcart_admin;Password=xxx" \
  --from-literal=redis-connection="amcart-redis.xxx.cache.amazonaws.com:6379,ssl=true" \
  --from-literal=mongo-connection="mongodb://xxx:xxx@amcart-docdb.cluster-xxx.docdb.amazonaws.com:27017" \
  --from-literal=jwt-secret="your-jwt-secret" \
  --from-literal=razorpay-key-id="rzp_live_xxx" \
  --from-literal=razorpay-key-secret="xxx"
```

### 5.3 Kubernetes Manifests

**deploy/k8s/base/namespace.yaml:**

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: amcart
  labels:
    app.kubernetes.io/name: amcart
```

**deploy/k8s/base/user-service/deployment.yaml:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
  namespace: amcart
  labels:
    app: user-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: user-service
  template:
    metadata:
      labels:
        app: user-service
    spec:
      containers:
      - name: user-service
        image: ${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com/amcart/user-service:${IMAGE_TAG}
        ports:
        - containerPort: 5001
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: amcart-secrets
              key: postgres-connection
        - name: Redis__ConnectionString
          valueFrom:
            secretKeyRef:
              name: amcart-secrets
              key: redis-connection
        - name: JWT__Secret
          valueFrom:
            secretKeyRef:
              name: amcart-secrets
              key: jwt-secret
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5001
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5001
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: user-service
  namespace: amcart
spec:
  selector:
    app: user-service
  ports:
  - port: 5001
    targetPort: 5001
  type: ClusterIP
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: user-service-hpa
  namespace: amcart
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: user-service
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

**deploy/k8s/base/nginx/configmap.yaml:**

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: nginx-config
  namespace: amcart
data:
  nginx.conf: |
    worker_processes auto;
    
    events {
        worker_connections 1024;
    }
    
    http {
        upstream user_service {
            server user-service.amcart.svc.cluster.local:5001;
        }
        
        upstream product_service {
            server product-service.amcart.svc.cluster.local:5002;
        }
        
        upstream cart_service {
            server cart-service.amcart.svc.cluster.local:5003;
        }
        
        upstream order_service {
            server order-service.amcart.svc.cluster.local:5004;
        }
        
        upstream payment_service {
            server payment-service.amcart.svc.cluster.local:5005;
        }
        
        upstream search_service {
            server search-service.amcart.svc.cluster.local:5007;
        }
        
        limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/s;
        limit_req_zone $binary_remote_addr zone=auth_limit:10m rate=10r/s;
        
        server {
            listen 80;
            
            location /health {
                return 200 'OK';
                add_header Content-Type text/plain;
            }
            
            location /api/v1/auth/ {
                limit_req zone=auth_limit burst=20 nodelay;
                proxy_pass http://user_service;
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
            }
            
            location /api/v1/users/ {
                limit_req zone=api_limit burst=50 nodelay;
                proxy_pass http://user_service;
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
            }
            
            location /api/v1/products/ {
                limit_req zone=api_limit burst=100 nodelay;
                proxy_pass http://product_service;
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
            }
            
            location /api/v1/cart/ {
                limit_req zone=api_limit burst=50 nodelay;
                proxy_pass http://cart_service;
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
            }
            
            location /api/v1/orders/ {
                limit_req zone=api_limit burst=30 nodelay;
                proxy_pass http://order_service;
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
            }
            
            location /api/v1/payments/ {
                limit_req zone=api_limit burst=20 nodelay;
                proxy_pass http://payment_service;
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
            }
            
            location /api/v1/search/ {
                limit_req zone=api_limit burst=100 nodelay;
                proxy_pass http://search_service;
                proxy_set_header Host $host;
                proxy_set_header X-Real-IP $remote_addr;
            }
        }
    }
```

### 5.4 Deploy to EKS

```bash
# Push images to ECR
aws ecr get-login-password --region ap-south-1 | docker login --username AWS --password-stdin ${AWS_ACCOUNT_ID}.dkr.ecr.ap-south-1.amazonaws.com

docker tag amcart/user-service:latest ${AWS_ACCOUNT_ID}.dkr.ecr.ap-south-1.amazonaws.com/amcart/user-service:latest
docker push ${AWS_ACCOUNT_ID}.dkr.ecr.ap-south-1.amazonaws.com/amcart/user-service:latest

# Apply Kubernetes manifests
kubectl apply -f deploy/k8s/base/namespace.yaml
kubectl apply -f deploy/k8s/base/ --recursive

# Check deployment status
kubectl get pods -n amcart
kubectl get services -n amcart

# View logs
kubectl logs -f deployment/user-service -n amcart
```

---

## 6. CI/CD Pipeline Setup

### 6.1 GitHub Actions Workflow

**.github/workflows/deploy.yml:**

```yaml
name: Build and Deploy

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  AWS_REGION: ap-south-1
  ECR_REGISTRY: ${{ secrets.AWS_ACCOUNT_ID }}.dkr.ecr.ap-south-1.amazonaws.com
  EKS_CLUSTER: amcart-eks

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore src/AmCart.sln

      - name: Build
        run: dotnet build src/AmCart.sln --no-restore

      - name: Test
        run: dotnet test src/AmCart.sln --no-build --verbosity normal

  build-and-push:
    needs: test
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    
    strategy:
      matrix:
        service:
          - user-service
          - product-service
          - cart-service
          - order-service
          - payment-service
          - inventory-service
          - search-service
          - notification-service
          - review-service

    steps:
      - uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ env.AWS_REGION }}

      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v2

      - name: Build and push Docker image
        env:
          SERVICE_NAME: ${{ matrix.service }}
          IMAGE_TAG: ${{ github.sha }}
        run: |
          SERVICE_PASCAL=$(echo $SERVICE_NAME | sed -r 's/(^|-)([a-z])/\U\2/g')
          docker build \
            -t $ECR_REGISTRY/amcart/$SERVICE_NAME:$IMAGE_TAG \
            -t $ECR_REGISTRY/amcart/$SERVICE_NAME:latest \
            -f src/Services/${SERVICE_PASCAL}/Dockerfile \
            src/
          docker push $ECR_REGISTRY/amcart/$SERVICE_NAME:$IMAGE_TAG
          docker push $ECR_REGISTRY/amcart/$SERVICE_NAME:latest

  deploy:
    needs: build-and-push
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ env.AWS_REGION }}

      - name: Update kubeconfig
        run: aws eks update-kubeconfig --name $EKS_CLUSTER --region $AWS_REGION

      - name: Deploy to EKS
        env:
          IMAGE_TAG: ${{ github.sha }}
        run: |
          # Update image tags in manifests
          find deploy/k8s -name '*.yaml' -exec sed -i "s/\${IMAGE_TAG}/$IMAGE_TAG/g" {} \;
          find deploy/k8s -name '*.yaml' -exec sed -i "s/\${AWS_ACCOUNT_ID}/${{ secrets.AWS_ACCOUNT_ID }}/g" {} \;
          find deploy/k8s -name '*.yaml' -exec sed -i "s/\${AWS_REGION}/$AWS_REGION/g" {} \;
          
          # Apply manifests
          kubectl apply -f deploy/k8s/base/ --recursive
          
          # Wait for rollout
          kubectl rollout status deployment/user-service -n amcart --timeout=300s
          kubectl rollout status deployment/product-service -n amcart --timeout=300s

      - name: Notify on success
        if: success()
        run: echo "Deployment successful!"

      - name: Notify on failure
        if: failure()
        run: echo "Deployment failed!"
```

---

## 7. Database Migrations

### 7.1 EF Core Migrations

```bash
# Create migration
cd src/Services/UserService/UserService.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../UserService.Api

# Apply migration
dotnet ef database update --startup-project ../UserService.Api

# Generate SQL script
dotnet ef migrations script --startup-project ../UserService.Api -o migrations.sql
```

### 7.2 Production Migration Script

```bash
#!/bin/bash
# scripts/run-migrations.sh

set -e

echo "Running database migrations..."

# Get database connection from secrets
DB_CONNECTION=$(kubectl get secret amcart-secrets -n amcart -o jsonpath='{.data.postgres-connection}' | base64 -d)

# Run migrations using a temporary pod
kubectl run db-migrate \
  --namespace amcart \
  --image=${ECR_REGISTRY}/amcart/user-service:latest \
  --rm -it --restart=Never \
  --env="ConnectionStrings__DefaultConnection=$DB_CONNECTION" \
  -- dotnet ef database update

echo "Migrations completed!"
```

---

## 8. Monitoring & Logging

### 8.1 Install Prometheus & Grafana

```bash
# Add Helm repos
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add grafana https://grafana.github.io/helm-charts
helm repo update

# Install Prometheus
helm install prometheus prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  --create-namespace \
  --set prometheus.prometheusSpec.retention=30d \
  --set grafana.adminPassword=admin123

# Get Grafana password
kubectl get secret prometheus-grafana -n monitoring -o jsonpath="{.data.admin-password}" | base64 -d

# Port forward Grafana
kubectl port-forward svc/prometheus-grafana 3000:80 -n monitoring
```

### 8.2 CloudWatch Logging

```bash
# Install Fluent Bit for CloudWatch logs
kubectl apply -f https://raw.githubusercontent.com/aws-samples/amazon-cloudwatch-container-insights/latest/k8s-deployment-manifest-templates/deployment-mode/daemonset/container-insights-monitoring/fluent-bit/fluent-bit.yaml
```

---

## 9. SSL/TLS Configuration

### 9.1 Install cert-manager

```bash
# Install cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Create ClusterIssuer for Let's Encrypt
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@amcart.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

### 9.2 Ingress with TLS

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: amcart-ingress
  namespace: amcart
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
  - hosts:
    - api.amcart.com
    secretName: amcart-tls
  rules:
  - host: api.amcart.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: nginx
            port:
              number: 80
```

---

## 10. Troubleshooting

### 10.1 Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Pod CrashLoopBackOff | App failing to start | Check logs: `kubectl logs <pod> -n amcart` |
| ImagePullBackOff | Cannot pull image | Verify ECR login and image exists |
| Connection refused | Service not ready | Check readiness probe and service endpoints |
| Database connection error | Wrong credentials or network | Verify secrets and security groups |
| Out of memory | Resource limits too low | Increase memory limits |

### 10.2 Debugging Commands

```bash
# Get pod status
kubectl get pods -n amcart -o wide

# Describe pod (see events)
kubectl describe pod <pod-name> -n amcart

# View logs
kubectl logs <pod-name> -n amcart --tail=100

# View previous logs (if restarted)
kubectl logs <pod-name> -n amcart --previous

# Exec into pod
kubectl exec -it <pod-name> -n amcart -- /bin/sh

# Port forward for debugging
kubectl port-forward <pod-name> 5001:5001 -n amcart

# View service endpoints
kubectl get endpoints -n amcart

# Check resource usage
kubectl top pods -n amcart
kubectl top nodes
```

### 10.3 Health Check Endpoints

```bash
# Check all services
for port in 5001 5002 5003 5004 5005 5006 5007 5008 5009; do
  echo "Checking port $port..."
  kubectl exec -n amcart deployment/nginx -- curl -s http://localhost/api/v1/health || echo "Failed"
done
```

---

## Quick Reference

### Useful Commands

```bash
# Local Development
docker-compose up -d                    # Start local stack
dotnet run --project src/Services/...   # Run service

# Docker
docker-compose build                    # Build images
docker-compose logs -f <service>        # View logs

# Terraform
terraform init                          # Initialize
terraform plan                          # Preview changes
terraform apply                         # Apply changes
terraform destroy                       # Destroy infrastructure

# Kubernetes
kubectl apply -f <file>                 # Apply manifest
kubectl get pods -n amcart              # List pods
kubectl logs -f <pod> -n amcart         # View logs
kubectl rollout restart deployment/<name> -n amcart  # Restart deployment

# AWS
aws eks update-kubeconfig --name amcart-eks  # Update kubeconfig
aws ecr get-login-password | docker login... # ECR login
```

---

**END OF DEPLOYMENT GUIDE**

