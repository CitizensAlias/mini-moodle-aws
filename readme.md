# Mini-Moodle on AWS

## 1. Application and Problem Statement

A small web platform for distributing learning materials (text and images) to selected users.

**Problem:** an instructor wants to share materials with specific people only, without public links and without users having to register themselves on the platform.

**Main Features**

- One admin who is assigned from a secret upon first startup.
- The admin creates user accounts with a username (assigned by admin) and a one-time password.
- On first login the user must set their own password.
- The admin uploads materials (text posts and images) and selects which users may access which material.
- Users see only materials they were granted. Images are served through short-lived presigned S3 URLs, issued only after an authorization check.

## 2. Architecture

### 2.0 Glossary
* **ACM (AWS Certificate Manager)** - handles provisioning, management, and deployment of SSL/TLS certificates.
* **S3 (Simple Storage Service)** - provides scalable object storage for data, backups, and Terraform state files.
* **ECR (Elastic Container Registry)** - stores, manages, and deploys Docker container images.
* **ECS (Elastic Container Service)** - orchestrates and manages Docker containers.
* **ECS Fargate** - runs containers serverlessly without managing underlying EC2 instances.
* **R53 (Amazon Route 53)** - provides scalable DNS routing to translate domain names into IP addresses.
* **ALB (Application Load Balancer)** - distributes incoming HTTP/HTTPS traffic across multiple targets.
* **RDS (Relational Database Service)** - sets up, operates, and scales managed relational databases.
* **OAC (Origin Access Control)** - restricts access to an S3 bucket so only a specific CloudFront distribution can read its files.



### 2.1 Software architecture

```mermaid
flowchart LR
    U[Browser] -->|HTTPS| FE[Frontend<br/>Blazor WebAssembly + MudBlazor<br/>static files]
    U -->|HTTPS /api/*| BE[Backend<br/>Spring Boot REST API]
    BE -->|JDBC| DB[(PostgreSQL<br/>users, materials, grants)]
    BE -->|upload / delete<br/>validated| FS[(Object storage<br/>image files)]
    U -->|GET via presigned URL| FS

    classDef frontend fill:#386641,color:#fff,stroke:#24422a,stroke-width:2px
classDef backend fill:#2b4c7e,color:#fff,stroke:#1b3152,stroke-width:2px
classDef database fill:#bc4749,color:#fff,stroke:#822e30,stroke-width:2px
    class FE frontend
    class BE,FS backend
    class DB database
```

| Component | Responsibility |
|---|---|
| Frontend | UI, admin screens (users, materials, access), user material view. Talks only to the relative path `/api/*`. |
| Backend | Authentication (BCrypt + JWT), `must_change_password` enforcement, authorization of every material access, upload validation (type, size), presigned URL generation. |
| PostgreSQL | Users, materials (metadata and text), access grants, S3 object keys. |
| Object storage | Image binaries. The bucket is fully private. |

### 2.2 AWS

```mermaid
flowchart TB
    User[Browser]

    subgraph AWS[AWS region us-east-1]
        R53[Route 53<br/> somedomain.de]
        ACM[ACM certificate<br/>somedomain.de<br/>api.somedomain.de]
        CF[CloudFront<br/>HTTPS for users]
        S3FE[(S3: frontend bucket)]
        S3IMG[(S3: uploads bucket<br/>private)]
        ECR[ECR]
        SM[Secrets Manager]
        CW[CloudWatch<br/>Logs, Alarm]

        subgraph VPC[VPC 10.0.0.0/16]
            subgraph PUB[Public subnets, 2 AZs]
                ALB[Application Load Balancer<br/>HTTPS :443<br/>SG: CloudFront prefix list]
                ECS[ECS Fargate task<br/>Spring Boot :8080<br/>public IP, SG: ALB only]
            end
            subgraph PRIV[Private subnets, 2 AZs]
                RDS[(RDS PostgreSQL<br/>db.t4g.micro, single AZ)]
            end
        end
    end

    User -->|HTTPS| CF
    CF -->|"HTTPS via OAC, /*"| S3FE
    CF -->|"HTTPS, /api/*"| ALB
    ALB -->|"HTTP :8080, inside VPC"| ECS
    ECS -->|5432| RDS
    ECS -->|AWS SDK, HTTPS| S3IMG
    User -->|HTTPS, presigned URL| S3IMG
    ECR -.->|image pull| ECS
    SM -.->|DB password, JWT secret,<br/>admin seed| ECS
    ECS -.->|logs| CW
    ALB -.->|5xx metric| CW
    R53 -.->|alias<br/>somedomain.de| CF
    R53 -.->|alias<br/>api.somedomain.de| ALB
    ACM -.->|TLS certificate for<br/>somedomain.de| CF
    ACM -.->|TLS certificate for<br/>api.somedomain.de| ALB

    classDef frontend fill:#386641,color:#fff,stroke:#24422a,stroke-width:2px
    classDef backend fill:#2b4c7e,color:#fff,stroke:#1b3152,stroke-width:2px
    classDef database fill:#bc4749,color:#fff,stroke:#822e30,stroke-width:2px
    classDef shared fill:#a66321,color:#fff,stroke:#6e4014,stroke-width:2px

    class CF,S3FE frontend
    class ALB,ECS,ECR,SM,S3IMG backend
    class RDS database
    class R53,ACM,CW shared
```

| AWS service | Used for |
|---|---|
| S3 (2 buckets) | Static frontend hosting (accessed by CloudFront via OAC); private storage for uploaded images |
| CloudFront | HTTPS entry point for users; serves the frontend and routes `/api/*` to the ALB |
| Route 53 + ACM | DNS name for the ALB and its free TLS certificate (DNS validation) |
| Application Load Balancer | HTTPS termination, health checks, entry point for the backend |
| ECS Fargate + ECR | Runs the backend container; ECR stores images built by the pipeline |
| RDS PostgreSQL | Relational data |
| Secrets Manager | DB password, JWT signing key, initial admin password |
| CloudWatch | Backend log (retention 7 days) |

### 2.3 Network and security groups

| Resource | Subnet | Inbound |
|---|---|---|
| ALB | public | 443 from the CloudFront managed prefix list |
| ECS task | public (public IP) | 8080 from the ALB security group only |
| RDS | private | 5432 from the ECS security group only |

### 2.4 Key architectural decisions

| Decision | Reason | Trade-off |
|---|---|---|
| Blazor **WebAssembly** instead of Blazor Server | Static files, so S3 + CloudFront is enough; no extra container and no persistent SignalR connections | Larger first download |
| `/api/*` routed through CloudFront | One origin for the browser: HTTPS everywhere on the user side, no CORS, backend responds only to frontend | Requires cache policy that forwards `Authorization` |
| ALB with ACM certificate and own domain | End-to-end HTTPS till VPC; certificate is free and auto-renewed | own domain and Route 53 zone needed |
| Fargate instead of EC2 | No OS or Docker to maintain; rolling updates and health checks via ECS | Higher cost than a single EC2 |
| ECS task in a **public** subnet, reachable only from the ALB security group | Avoids NAT Gateway and interface VPC endpoints | Task has a public IP; protected by the security group only |
| RDS in private subnets | Database has no route from the internet | - |
| Files in S3, metadata and grants in PostgreSQL | DB stays small; authorization stays in one place (backend) | Two stores to keep consistent |
| Presigned URLs for downloads | Backend never touches the actual image bytes; bucket stays private | URL stays valid until expiry, even if access is revoked meanwhile |
| Own auth (BCrypt + JWT) instead of Cognito | Easier local development and testing | We maintain security-critical code ourselves |

## 3. Software Delivery Process

```mermaid
flowchart LR
    C[Commit / PR] --> B[Build<br/>Maven + dotnet]
    B --> UT[Unit tests<br/>JUnit, Mockito]
    UT --> IT[Integration tests<br/>Testcontainers + PostgreSQL]
    IT --> SA[Static analysis<br/>SonarCloud, CodeQL]
    SA --> SEC[Security scans<br/>gitleaks, Trivy, Checkov]
    SEC --> TP[terraform validate + plan]
    TP -->|merge to main / tag| IMG[Build and push image to ECR]
    IMG --> TA[terraform apply]
    TA --> DEP[ECS service update<br/>S3 sync + CloudFront invalidation]
```

| Workflow | Trigger | Steps |
|---|---|---|
| `.github/workflows/ci.yml` | PR, push to main | Build, unit tests, integration tests, SonarCloud, CodeQL, gitleaks |
| `.github/workflows/infra.yml` | PR (plan), main/tag (apply) | `terraform fmt -check`, `validate`, Checkov, `plan`, `apply` |
| `.github/workflows/deploy.yml` | main/tag | Docker build, Trivy scan, push to ECR, `aws ecs update-service --force-new-deployment`; frontend: `dotnet publish`, S3 sync, CloudFront invalidation |

## 4. Local Development
(NOT YET IMPLEMENTED)

## 5. Deployment
(NOT YET IMPLEMENTED)

## 6. Repository Layout

```
backend/            Spring Boot application and tests
frontend/           Blazor WebAssembly + MudBlazor
infra/              Terraform (network, rds, alb, ecs, storage, cdn, dns, monitoring)
docker-compose.yml  local PostgreSQL
.github/workflows/  GitHub Actions workflows
```