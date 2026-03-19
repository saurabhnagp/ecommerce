# AmCart Draw.io Diagrams

This directory contains all architecture and flow diagrams for the AmCart ecommerce platform in draw.io format.

## Diagram Files

### 1. System Architecture (`01-system-architecture.drawio`)
**Overview**: Complete system architecture showing all microservices, databases, and external services.

**Contents**:
- Frontend (Nuxt.js 3)
- Nginx API Gateway
- 9 Microservices (.NET 8)
- Databases (PostgreSQL, MongoDB, Redis, OpenSearch)
- Message Queue (RabbitMQ)
- External Services (Razorpay, Email, OAuth)

**Use Case**: High-level architecture documentation, onboarding new team members, stakeholder presentations.

---

### 2. Authentication Flows (`02-authentication-flows.drawio`)
**Overview**: Detailed authentication flows with multiple tabs for different scenarios.

**Tabs**:
- **Email Registration**: User registration with email verification flow
- **Email Login**: Email/password login with JWT token generation
- **Google OAuth**: Google OAuth 2.0 login flow
- **Facebook OAuth**: Facebook OAuth 2.0 login flow

**Use Case**: Understanding authentication implementation, debugging auth issues, developer reference.

---

### 3. Purchase Flow (`03-purchase-flow.drawio`)
**Overview**: Complete user journey from product browsing to order confirmation.

**Flow Steps**:
1. Browse Products
2. View Product Details
3. Add to Cart
4. View Cart
5. Proceed to Checkout
6. Enter Billing/Shipping
7. Place Order
8. Initiate Payment
9. Complete Payment
10. Payment Callback
11. Order Confirmation

**Use Case**: User experience design, testing scenarios, business process documentation.

---

### 4. Database Schema (`04-database-schema.drawio`)
**Overview**: Entity Relationship Diagram (ERD) showing all database tables and relationships.

**Databases Covered**:
- **PostgreSQL**: Users, Products, Orders, Payments, Inventory, Categories, Addresses
- **MongoDB**: Reviews, Notifications
- **Redis**: Cart, Sessions, Cache
- **OpenSearch**: Product Search Index

**Use Case**: Database design reference, understanding data relationships, migration planning.

---

### 5. AWS Deployment (`05-aws-deployment.drawio`)
**Overview**: Complete AWS infrastructure deployment architecture.

**Components**:
- Route 53 (DNS)
- CloudFront (CDN)
- VPC with Public/Private Subnets
- Application Load Balancer (ALB)
- EKS Cluster with Microservices Pods
- RDS PostgreSQL (Multi-AZ)
- DocumentDB (MongoDB)
- ElastiCache (Redis)
- OpenSearch Cluster
- Amazon MQ (RabbitMQ)
- S3, CloudWatch, Secrets Manager

**Use Case**: Infrastructure planning, deployment documentation, cost estimation, security review.

---

### 6. Service Communication (`06-service-communication.drawio`)
**Overview**: Communication patterns between microservices (REST and Event-Driven).

**Communication Types**:
- **REST/HTTP**: Synchronous request/response (green solid lines)
- **RabbitMQ Events**: Asynchronous event-driven (orange dashed lines)

**Key Events**:
- OrderCreated
- PaymentCompleted
- PaymentFailed
- ProductUpdated
- UserRegistered

**Use Case**: Understanding service dependencies, debugging distributed systems, event flow analysis.

---

### 7. AWS System Design (`07-aws-system-design.drawio`)
**Overview**: Comprehensive AWS system design architecture following enterprise cloud patterns.

**Components**:
- **External Access**: Users, External Services → Global Accelerator → Route 53
- **AWS Region**:
  - Nginx API Gateway (EC2)
  - CloudFront CDN
  - S3 (Static Assets, Frontend Apps)
  - Core Services (EKS) - API Gateway Layer
- **Private Subnet**:
  - **Microservices Groups**:
    - User Services (User, Auth)
    - Product Services (Product, Category)
    - Order & Cart Services
    - Payment Services (Payment, Invoice)
    - Inventory Services
    - Search & Review Services
    - Notification Services (Notification, Email)
  - **Caching & Message Broker**: ElastiCache Redis, Amazon MQ (RabbitMQ)
  - **Databases**: RDS PostgreSQL, RDS MSSQL, DocumentDB (MongoDB), OpenSearch
  - **Infrastructure**: CloudWatch, X-Ray, Secrets Manager, S3 Backup

**Use Case**: Enterprise architecture documentation, AWS infrastructure planning, stakeholder presentations, compliance reviews.

---

### 8. Order Flow Clean (`08-order-flow-clean.drawio`)
**Overview**: Clean, vertically-oriented order flow diagram with minimal arrow crossings.

**Sections** (color-coded):
- **👤 Customer** (Yellow): Browse → Add to Cart → View Cart → Checkout → Shipping → Place Order
- **📦 Order Processing** (Green): Validate Cart → Create Order → Check Inventory → Reserve
- **💳 Payment Processing** (Purple): Redirect to Razorpay → Payment Status decision
- **❌ Failure Path** (Red): Update Failed → Release Inventory → Notification → Retry Option
- **✅ Success Path** (Green): Confirm Order → Confirm Inventory → Email → SMS → Success Page

**Design Features**:
- Vertical top-to-bottom flow
- Orthogonal edges (no diagonal arrows)
- Error paths routed along outer edges
- Clear decision diamonds with labeled branches
- Color-coded for easy understanding

**Use Case**: Presentations, documentation, onboarding, process reviews.

---

## How to Open and Edit Diagrams

### Option 1: Draw.io Desktop App
1. Download draw.io desktop app from [https://github.com/jgraph/drawio-desktop/releases](https://github.com/jgraph/drawio-desktop/releases)
2. Install the application
3. Open draw.io
4. File → Open → Select the `.drawio` file
5. Edit and save

### Option 2: Draw.io Web (diagrams.net)
1. Go to [https://app.diagrams.net/](https://app.diagrams.net/)
2. Click "Open Existing Diagram"
3. Select "Device" and choose the `.drawio` file
4. Edit online
5. File → Export as → Choose format (PNG, SVG, PDF, etc.)

### Option 3: VS Code Extension
1. Install "Draw.io Integration" extension in VS Code
2. Open the `.drawio` file in VS Code
3. Edit directly in VS Code
4. Save changes

---

## Exporting Diagrams

### Export Formats
- **PNG**: For presentations, documentation (high resolution)
- **SVG**: For web pages, scalable graphics
- **PDF**: For documentation, printing
- **XML**: Source format (already in this format)

### Export Steps (draw.io)
1. Open the diagram
2. File → Export as → Choose format
3. Adjust settings (resolution, quality)
4. Click "Export"
5. Save to desired location

---

## Diagram Maintenance

### Updating Diagrams
1. Always keep diagrams in sync with code changes
2. Update diagrams when:
   - New services are added
   - Database schema changes
   - API endpoints change
   - Infrastructure changes
   - New authentication methods added

### Version Control
- All diagrams are version controlled in Git
- Commit diagram changes with descriptive messages
- Review diagram changes in pull requests

### Best Practices
- Use consistent colors and shapes across diagrams
- Keep diagrams readable (avoid overcrowding)
- Add labels and descriptions where needed
- Use swimlanes for process flows
- Group related components

---

## Color Coding

| Color | Meaning |
|-------|---------|
| **Blue** | Frontend/Client components |
| **Yellow** | API Gateway, Load Balancers |
| **Green** | Microservices, Application Layer |
| **Purple** | External Services, OAuth |
| **Red** | Databases, Data Storage |
| **Orange** | Message Queues, Event Brokers |

---

## Related Documentation

- [API Specifications](../API-Specifications.md) - Detailed API endpoints
- [Deployment Guide](../Deployment-Guide.md) - Deployment instructions
- [Architecture Decision Records](../ADR/) - ADR documents
- [Runbooks](../Runbooks/) - Operational procedures

---

## Questions or Issues?

If you need to:
- Add new diagrams
- Update existing diagrams
- Fix diagram errors
- Request new diagram types

Please create an issue or contact the architecture team.

---

**Last Updated**: December 19, 2024

