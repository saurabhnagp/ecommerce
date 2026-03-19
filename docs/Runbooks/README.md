# AmCart Operational Runbooks

## Overview

This directory contains operational runbooks for managing the AmCart Ecommerce Platform. These runbooks provide step-by-step procedures for common operational tasks, incident response, and troubleshooting.

## Runbook Index

| Runbook | Description | Audience |
|---------|-------------|----------|
| [01-Service-Health-Check](01-Service-Health-Check.md) | Monitoring and verifying service health | On-call Engineers |
| [02-Database-Operations](02-Database-Operations.md) | Database backup, restore, and maintenance | DBAs, DevOps |
| [03-Deployment-Procedures](03-Deployment-Procedures.md) | Deploying and rolling back services | DevOps |
| [04-Incident-Response](04-Incident-Response.md) | Handling production incidents | On-call Engineers |
| [05-Scaling-Procedures](05-Scaling-Procedures.md) | Scaling services and infrastructure | DevOps |
| [06-Secret-Management](06-Secret-Management.md) | Managing secrets and credentials | Security, DevOps |
| [07-Log-Analysis](07-Log-Analysis.md) | Analyzing logs for debugging | All Engineers |
| [08-Cache-Operations](08-Cache-Operations.md) | Redis cache management | DevOps |

## Quick Reference

### Emergency Contacts

| Role | Contact | Escalation |
|------|---------|------------|
| On-call Primary | PagerDuty | Auto-escalate after 15 min |
| On-call Secondary | PagerDuty | Auto-escalate from primary |
| Engineering Lead | Direct | P1 incidents |
| Database Admin | Direct | Database emergencies |

### Critical Service URLs

| Service | Production | Staging |
|---------|------------|---------|
| API Gateway | api.amcart.com | api.staging.amcart.com |
| Frontend | www.amcart.com | staging.amcart.com |
| Grafana | grafana.internal.amcart.com | - |
| Kibana | kibana.internal.amcart.com | - |
| AWS Console | console.aws.amazon.com | - |

### Severity Levels

| Level | Impact | Response Time | Examples |
|-------|--------|---------------|----------|
| P1 | Site down, all users affected | 15 min | API Gateway down |
| P2 | Major feature broken | 30 min | Payments failing |
| P3 | Minor feature issue | 2 hours | Search degraded |
| P4 | Low impact | Next business day | Minor UI bug |

## Prerequisites

### Tools Required

- `kubectl` configured with EKS cluster
- AWS CLI with appropriate IAM permissions
- `redis-cli` for cache operations
- `psql` for PostgreSQL access
- Access to CloudWatch, Grafana, Kibana

### Common kubectl Commands

```bash
# Set context
kubectl config use-context amcart-prod

# View all pods
kubectl get pods -n amcart-prod

# View pod logs
kubectl logs -f deployment/user-service -n amcart-prod

# Exec into pod
kubectl exec -it deployment/user-service -n amcart-prod -- /bin/sh

# Port forward
kubectl port-forward svc/user-service 5001:5001 -n amcart-prod
```

## How to Use Runbooks

1. **Identify the issue** - Use monitoring dashboards
2. **Find relevant runbook** - Match symptoms to runbook
3. **Follow steps in order** - Don't skip steps
4. **Document actions** - Record what you did
5. **Escalate if needed** - Don't be a hero
6. **Post-incident review** - Update runbooks if needed

