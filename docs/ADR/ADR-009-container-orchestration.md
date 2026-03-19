# ADR-009: Container Orchestration

## Status
**Accepted**

## Date
2024-12-19

## Context

AmCart's microservices architecture requires:
- Container orchestration for 9+ services
- Automated deployment and rollback
- Service discovery
- Load balancing
- Auto-scaling based on metrics
- Secret management
- Health monitoring

## Decision

We will use **Kubernetes** via **Amazon EKS** (Elastic Kubernetes Service).

### Cluster Architecture

```
┌───────────────────────────────────────────────────────────────┐
│                     EKS Cluster                                │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                   Control Plane (AWS Managed)           │  │
│  │  API Server │ etcd │ Controller Manager │ Scheduler     │  │
│  └─────────────────────────────────────────────────────────┘  │
│                              │                                 │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                     Node Group 1                        │  │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐                   │  │
│  │  │  Node 1 │ │  Node 2 │ │  Node 3 │                   │  │
│  │  │t3.medium│ │t3.medium│ │t3.medium│                   │  │
│  │  └─────────┘ └─────────┘ └─────────┘                   │  │
│  └─────────────────────────────────────────────────────────┘  │
│                                                                │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                      Namespaces                         │  │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐   │  │
│  │  │production│ │ staging  │ │   dev    │ │monitoring│   │  │
│  │  └──────────┘ └──────────┘ └──────────┘ └──────────┘   │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────┘
```

### Kubernetes Resources Used

| Resource | Purpose |
|----------|---------|
| Deployment | Manage pod replicas |
| Service | Internal load balancing |
| Ingress | External traffic routing |
| ConfigMap | Configuration |
| Secret | Sensitive data |
| HPA | Horizontal Pod Autoscaler |
| PVC | Persistent storage |
| NetworkPolicy | Network isolation |

### Namespace Strategy

| Namespace | Contents |
|-----------|----------|
| `amcart-prod` | Production services |
| `amcart-staging` | Staging environment |
| `amcart-dev` | Development environment |
| `monitoring` | Prometheus, Grafana |
| `logging` | EFK stack |
| `ingress-nginx` | Nginx Ingress Controller |

## Consequences

### Positive

- **Managed Control Plane**: AWS manages the control plane
- **Scalability**: Auto-scaling nodes and pods
- **Portability**: Kubernetes is cloud-agnostic
- **Ecosystem**: Rich ecosystem (Helm, operators)
- **Declarative**: Infrastructure as code
- **Self-Healing**: Automatic pod restart
- **Rolling Updates**: Zero-downtime deployments

### Negative

- **Complexity**: Steep learning curve
- **Cost**: EKS has control plane fee ($73/month)
- **Resource Overhead**: Kubernetes system pods use resources
- **Debugging**: Distributed debugging is harder

### Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Complexity | Use Helm charts, training |
| Cost | Consolidate environments, use Fargate for spiky loads |
| Overhead | Right-size nodes |
| Debugging | Use Lens, k9s, distributed tracing |

## Implementation

### Service Deployment Manifest

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
  namespace: amcart-prod
  labels:
    app: user-service
spec:
  replicas: 2
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
        image: 123456789.dkr.ecr.us-east-1.amazonaws.com/amcart/user-service:v1.0.0
        ports:
        - containerPort: 5001
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: user-service-secrets
              key: db-connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5001
          initialDelaySeconds: 10
          periodSeconds: 5
        livenessProbe:
          httpGet:
            path: /health/live
            port: 5001
          initialDelaySeconds: 15
          periodSeconds: 10
---
apiVersion: v1
kind: Service
metadata:
  name: user-service
  namespace: amcart-prod
spec:
  selector:
    app: user-service
  ports:
  - port: 5001
    targetPort: 5001
  type: ClusterIP
```

### Horizontal Pod Autoscaler

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: user-service-hpa
  namespace: amcart-prod
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
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Ingress Configuration

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: amcart-ingress
  namespace: amcart-prod
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/rate-limit: "100"
spec:
  tls:
  - hosts:
    - api.amcart.com
    secretName: amcart-tls
  rules:
  - host: api.amcart.com
    http:
      paths:
      - path: /api/v1/users
        pathType: Prefix
        backend:
          service:
            name: user-service
            port:
              number: 5001
      - path: /api/v1/products
        pathType: Prefix
        backend:
          service:
            name: product-service
            port:
              number: 5002
```

## Alternatives Considered

### 1. Docker Swarm

**Pros:**
- Simpler than Kubernetes
- Built into Docker
- Lower learning curve

**Cons:**
- Less feature-rich
- Smaller ecosystem
- Limited enterprise support

**Why Rejected:** Not suitable for complex microservices at scale.

### 2. AWS ECS (Fargate)

**Pros:**
- Simpler than Kubernetes
- Serverless option (Fargate)
- Deep AWS integration

**Cons:**
- AWS lock-in
- Less portable
- Fewer features

**Why Rejected:** Want Kubernetes portability and ecosystem.

### 3. Self-managed Kubernetes

**Pros:**
- Full control
- Lower managed service costs
- Any cloud/on-prem

**Cons:**
- High operational burden
- Need Kubernetes expertise
- Must manage control plane

**Why Rejected:** Operational overhead not justified.

## Tooling

| Tool | Purpose |
|------|---------|
| kubectl | CLI interaction |
| Helm | Package management |
| k9s | Terminal UI |
| Lens | Desktop IDE |
| Terraform | IaC for EKS |
| ArgoCD | GitOps (optional) |

## References

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Amazon EKS](https://docs.aws.amazon.com/eks/)
- [Helm Charts](https://helm.sh/)
- [Kubernetes Best Practices](https://cloud.google.com/architecture/best-practices-for-running-cost-effective-kubernetes-applications-on-gke)

