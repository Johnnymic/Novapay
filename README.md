# NovaPay / NovaWallet Ledger Service

A secure wallet ledger API built with **ASP.NET Core .NET 8**, **Entity Framework Core**, **MySQL**, **JWT authentication**, **FluentValidation**, and **Docker**.

The project demonstrates how to build a wallet service that is designed to prevent duplicate money movement, protect against concurrent transfers, enforce daily transfer limits, and maintain an auditable transaction history.

---

## 1. Project Overview

NovaWallet provides the following core capabilities:

- Customer registration and login
- JWT-based authentication
- Role-based authorization
- Wallet creation
- Wallet balance enquiry
- Wallet credit
- Wallet-to-wallet transfer
- Paginated wallet statement
- Idempotent financial requests
- Concurrency-safe transfers
- Daily outbound transfer limit
- Immutable-style audit trail
- Transactional outbox for reliable downstream events
- Request Trace ID / correlation
- Structured validation and errors
- Dockerized MySQL environment

---

## 2. Architecture

The solution follows a **Clean Architecture / layered architecture** approach.

```text
                    +----------------------+
                    |      API / Client    |
                    | Swagger / Frontend   |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    |    NovaPay API       |
                    | Controllers / Auth   |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    | Application Layer    |
                    | Services / DTOs      |
                    | Validation / Mapper  |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    | Domain Layer         |
                    | Entities / Enums     |
                    | Business Concepts    |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    | Data Access Layer    |
                    | EF Core / UoW / Repo |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    |       MySQL          |
                    +----------------------+
```

### Main projects

Typical solution structure:

```text
NovaPay/
│
├── NovaPay.sln
│
├── NovaPay/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── Nova.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   ├── Validators/
│   ├── Profiles/
│   └── Helpers/
│
├── Nova.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── ...
│
├── Nova.DataAccess/
│   ├── Configurations/
│   ├── Migrations/
│   ├── Repositories/
│   ├── UnitOfWork/
│   └── NovaWalletDbContext.cs
│
└── tests/
    └── ...
```

> If your local project/folder names differ slightly, use the equivalent API, Application, Domain and DataAccess projects.

---

# 3. Prerequisites

Before running the application, install the following.

## .NET SDK

The application targets:

```text
.NET 8
```

Check your installed SDK:

```powershell
dotnet --version
```

or:

```powershell
dotnet --list-sdks
```

You should have a .NET 8 SDK installed.

---

## MySQL

You can either:

1. Install MySQL locally, or
2. Run MySQL using Docker Compose.

Docker is recommended for a consistent development environment.

---

## Docker Desktop

Install Docker Desktop if you want to run MySQL using the supplied Docker configuration.

Verify Docker:

```powershell
docker --version
```

Verify Docker Compose:

```powershell
docker compose version
```

---

# 4. Clone the Project

Clone the repository:

```powershell
git clone <YOUR_REPOSITORY_URL>
```

Move into the project directory:

```powershell
cd NovaPay
```

Restore dependencies:

```powershell
dotnet restore
```

Build the solution:

```powershell
dotnet build
```

---

# 5. Database Configuration

The application uses MySQL.

Example connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=NovaWalletDb;User=novawallet;Password=novawallet123;"
  }
}
```

Do not commit real production passwords or secrets into source control.

For production, use environment variables or a secrets-management solution.

---

# 6. Running MySQL with Docker

The project can use a Docker Compose configuration similar to:

```yaml
services:
  mysql:
    image: mysql:8.4
    container_name: novawallet-mysql
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: NovaWalletDb
      MYSQL_USER: novawallet
      MYSQL_PASSWORD: novawallet123
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost", "-u", "root", "-proot"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  mysql_data:
```

Start MySQL:

```powershell
docker compose up -d mysql
```

Check the container:

```powershell
docker ps
```

You should see:

```text
novawallet-mysql
```

Check MySQL logs if necessary:

```powershell
docker logs novawallet-mysql
```

Stop MySQL:

```powershell
docker compose down
```

To remove the database volume as well:

```powershell
docker compose down -v
```

> `docker compose down -v` deletes the MySQL data volume. Use this only when you intentionally want to reset the database.

---

# 7. JWT Configuration

The API uses JWT Bearer authentication.

Example configuration:

```json
{
  "Jwt": {
    "Key": "YOUR-DEVELOPMENT-SECRET-KEY",
    "Issuer": "NovaPay",
    "Audience": "NovaPayClient",
    "ExpiresIn": "60"
  }
}
```

For production:

- Use a strong secret.
- Do not commit the secret to Git.
- Use environment variables or a secret manager.
- Use HTTPS.
- Rotate credentials when required.

---

# 8. Wallet Configuration

The daily outbound transfer limit is configurable.

Example:

```json
{
  "WalletSettings": {
    "DailyTransferLimit": 500000.00
  }
}
```

The service uses this value instead of hard-coding the limit in the transfer logic.

The business date is calculated using Nigeria/WAT time.

Example:

```text
Daily limit: ₦500,000

Used today:     ₦300,000
New transfer:   ₦150,000
Remaining:      ₦200,000

Result: Allowed
```

If:

```text
Used today:     ₦300,000
New transfer:   ₦250,000
Total:          ₦550,000
```

the transfer is rejected.

---

# 9. Entity Framework Core Migrations

If the project already contains migrations, update the database with:

```powershell
dotnet ef database update --project Nova.DataAccess --startup-project NovaPay
```

If `dotnet ef` is not installed:

```powershell
dotnet tool install --global dotnet-ef
```

Check:

```powershell
dotnet ef --version
```

---

## Creating a migration

After changing an entity or EF configuration:

```powershell
dotnet ef migrations add MigrationName --project Nova.DataAccess --startup-project NovaPay
```

Example:

```powershell
dotnet ef migrations add AddCustomerRole --project Nova.DataAccess --startup-project NovaPay
```

Then apply it:

```powershell
dotnet ef database update --project Nova.DataAccess --startup-project NovaPay
```

### Visual Studio Package Manager Console

You can also use:

```powershell
Add-Migration AddCustomerRole -Project Nova.DataAccess -StartupProject NovaPay
```

Then:

```powershell
Update-Database -Project Nova.DataAccess -StartupProject NovaPay
```

---

# 10. Run the API

From the solution directory:

```powershell
dotnet run --project NovaPay
```

Or run the API project directly from Visual Studio.

You should see something similar to:

```text
Now listening on: https://localhost:xxxx
Now listening on: http://localhost:xxxx
```

The actual port depends on the project's `launchSettings.json`.

---

# 11. Swagger

Once the API is running, open Swagger using the URL configured by the API.

Typical development URL:

```text
https://localhost:<port>/swagger
```

Swagger allows you to:

- Register a customer
- Login
- Copy the JWT
- Authorize Swagger
- Create a wallet
- View balance
- Credit a wallet
- Transfer money
- View wallet statement

---

# 12. Recommended API Flow

For a new user, follow this sequence.

```text
1. Register
      |
      v
2. Login
      |
      v
3. Receive JWT
      |
      v
4. Authorize API
      |
      v
5. Create wallet
      |
      v
6. Get wallet balance
      |
      v
7. Credit wallet
      |
      v
8. Transfer to another wallet
      |
      v
9. View wallet statement
```

---

# 13. Authentication

After login, the API returns a JWT.

The JWT contains claims such as:

```text
sub
customerId
name
email
role
jti
```

Example role:

```text
Customer
```

Admin/Operations endpoints can be protected using role authorization:

```csharp
[Authorize(Roles = "Admin")]
```

or:

```csharp
[Authorize(Roles = "Admin,Operations")]
```

---

# 14. API Endpoints

## Authentication

### Register

```http
POST /api/v1/auth/register
```

Example:

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "08012345678",
  "email": "john@example.com",
  "password": "Password@123",
  "bvn": "12345678901",
  "nin": "12345678901"
}
```

---

### Login

```http
POST /api/v1/auth/login
```

Example:

```json
{
  "email": "john@example.com",
  "password": "Password@123"
}
```

Copy the returned JWT and authorize Swagger.

---

# 15. Wallet APIs

## Create Wallet

```http
POST /api/v1/wallets
```

Example:

```json
{
  "currency": "NGN"
}
```

A new wallet starts with:

```text
Balance = ₦0.00
Status = Active
Currency = NGN
```

---

## Get Wallet Balance

```http
GET /api/v1/wallets/{walletId}/balance
```

The API verifies that the wallet belongs to the authenticated customer.

---

## Credit Wallet

```http
POST /api/v1/wallets/{walletId}/credits
```

Example:

```json
{
  "amount": 100000.00,
  "reference": "CREDIT-001",
  "description": "Initial funding"
}
```

---

## Transfer

```http
POST /api/v1/transfers
```

Example:

```json
{
  "destinationWalletId": "DESTINATION-WALLET-GUID",
  "amount": 50000.00,
  "reference": "TRF-001",
  "description": "Wallet transfer"
}
```

The transfer performs:

```text
Authenticate
     |
Validate request
     |
Start DB transaction
     |
Lock source + destination wallets
     |
Validate wallet status
     |
Validate balance
     |
Check idempotency
     |
Check daily limit
     |
Create transfer
     |
Create debit transaction
     |
Create credit transaction
     |
Update balances
     |
Create audit records
     |
Create idempotency record
     |
Create outbox event
     |
Commit
```

---

# 16. Idempotency

Financial APIs should protect against duplicate requests.

For financial requests, send:

```http
Idempotency-Key: ABC123456
```

Example:

```http
POST /api/v1/transfers
Idempotency-Key: TRF-REQUEST-001
```

The service generates a request hash from the request data.

For a transfer, the hash includes:

```text
SourceWalletId
DestinationWalletId
Amount
Reference
Description
```

For a credit:

```text
WalletId
Amount
Reference
Description
```

### Same key + same request

The original response can be returned.

### Same key + different request

The request is rejected.

Example:

```text
Idempotency-Key = ABC123

First request:
Amount = ₦50,000

Retry:
Amount = ₦50,000

Result:
Same operation / original response
```

But:

```text
Idempotency-Key = ABC123

First request:
Amount = ₦50,000

Second request:
Amount = ₦100,000

Result:
Rejected because the payload changed.
```

This prevents accidental duplicate money movement.

---

# 17. Trace ID

The API supports:

```http
X-Trace-Id
```

Example:

```http
X-Trace-Id: 7f8c8d9e-transfer-001
```

If the client does not provide one, the application can generate/use a request trace identifier.

The Trace ID is useful for following a request through:

```text
API
 |
Service
 |
Database transaction
 |
AuditLog
 |
Outbox
 |
External systems
```

Relevant audit records store the Trace ID.

This makes production troubleshooting easier.

---

# 18. Concurrency Protection

Transfers use a database transaction and wallet row locking.

Conceptually:

```sql
SELECT Id
FROM Wallets
WHERE Id = @walletId
FOR UPDATE;
```

The source and destination wallets are locked before balances are modified.

Wallets are locked in deterministic ID order to reduce deadlock risk.

Example:

```text
Transfer A:
Wallet A → Wallet B

Transfer B:
Wallet B → Wallet A
```

Both operations use the same ordering strategy when acquiring locks.

This prevents inconsistent balance updates caused by concurrent requests.

---

# 19. Transaction Atomicity

A transfer consists of multiple database operations:

```text
WalletTransfer
WalletTransaction - Debit
WalletTransaction - Credit
Source Wallet Balance
Destination Wallet Balance
AuditLog
IdempotencyRequest
OutboxMessage
```

These operations are committed as one database transaction.

If an error occurs:

```text
ROLLBACK
```

Therefore, the application should not end up with:

```text
Source wallet debited
Destination wallet not credited
```

or:

```text
Balance updated
Transaction record missing
```

The financial state changes together.

---

# 20. Wallet Statement

Wallet statements are paginated.

Example:

```http
GET /api/v1/wallets/{walletId}/statement?pageNumber=1&pageSize=20
```

The API returns:

```text
WalletId
Currency
CurrentBalance
PageNumber
PageSize
TotalCount
TotalPages
HasPreviousPage
HasNextPage
Items
```

Transactions are sorted newest first.

Pagination is executed at database level using:

```csharp
Skip(...)
Take(...)
```

The maximum page size should be restricted to protect the database and API from excessive result sets.

---

# 21. Audit Trail

Financial operations create audit records containing information such as:

```text
EntityType
EntityId
Action
Amount
Actor
Metadata
CreatedAt
TraceId
```

Example:

```text
EntityType: WalletTransfer
EntityId: ...
Action: TransferCompleted
Amount: ₦50,000
Actor: customer-id
TraceId: transfer-001
CreatedAt: ...
```

Audit information should not contain passwords, JWTs or other unnecessary secrets.

---

# 22. Transactional Outbox

The project uses an Outbox pattern for reliable downstream integration.

When a transfer succeeds:

```text
Database Transaction
        |
        +-- Wallet changes
        +-- Transactions
        +-- Audit
        +-- Idempotency
        +-- OutboxMessage
        |
        +-- COMMIT
```

A background worker can later publish the outbox message.

For example:

```text
Outbox
   |
   v
Notification Service
   |
   v
SMS / Email / Push
```

If the external service is temporarily unavailable, the message remains in the database and can be retried.

The outbox does not perform the money movement itself.

---

# 23. Database Important Constraints

Important database constraints include:

### Customer

```text
CustomerReference UNIQUE
Email UNIQUE
PhoneNumber UNIQUE
```

### Wallet

Wallet belongs to a customer.

### Idempotency

```text
IdempotencyKey UNIQUE
```

### Daily Transfer Limit

```text
WalletId + BusinessDate UNIQUE
```

### Transactions

Transaction references should be unique when the model requires it.

For transfer transactions, use unique references such as:

```text
TRF-001-DR
TRF-001-CR
```

rather than using the same reference for both debit and credit if the database has a unique index on transaction reference.

---

# 24. MySQL JSON Configuration

Because the project uses MySQL, JSON columns should use:

```csharp
.HasColumnType("json")
```

not:

```csharp
.HasColumnType("jsonb")
```

`jsonb` is associated with PostgreSQL and can cause MySQL migration/database errors.

---

# 25. Configuration Example

A development `appsettings.json` can contain:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=NovaWalletDb;User=novawallet;Password=novawallet123;"
  },

  "Jwt": {
    "Key": "YOUR-DEVELOPMENT-SECRET",
    "Issuer": "NovaPay",
    "Audience": "NovaPayClient",
    "ExpiresIn": "60"
  },

  "WalletSettings": {
    "DailyTransferLimit": 500000.00
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Do not use these example credentials in production.

---

# 26. Running Tests

If a test project exists:

```powershell
dotnet test
```

For a specific test project:

```powershell
dotnet test tests/NovaPay.Tests
```

Recommended test categories:

```text
Unit Tests
Integration Tests
API Tests
Concurrency Tests
Idempotency Tests
Security Tests
```

Important scenarios to test:

- Cannot credit zero amount
- Cannot credit negative amount
- Cannot transfer zero amount
- Cannot transfer more than balance
- Cannot transfer to the same wallet
- Cannot transfer from another customer's wallet
- Cannot exceed daily limit
- Same idempotency key + same request is safe
- Same idempotency key + changed request is rejected
- Concurrent transfers cannot double-spend
- Failed transactions roll back
- Audit records are created
- Trace ID is persisted
- Pagination works correctly

---

# 27. Running the Complete Application with Docker

If the project includes a Dockerfile for the API and Docker Compose configuration for the complete application:

```powershell
docker compose up -d --build
```

Check containers:

```powershell
docker compose ps
```

View API logs:

```powershell
docker compose logs -f <api-service-name>
```

View MySQL logs:

```powershell
docker compose logs -f mysql
```

Stop everything:

```powershell
docker compose down
```

---

# 28. Common Problems

## `dotnet ef` is not recognized

Install:

```powershell
dotnet tool install --global dotnet-ef
```

Then restart the terminal if necessary.

---

## `Add-Migration` is not recognized

Install EF Core tooling/package support and use either:

```powershell
Add-Migration MigrationName
```

from Visual Studio Package Manager Console, or:

```powershell
dotnet ef migrations add MigrationName
```

from the terminal.

---

## MySQL connection refused

Check:

```powershell
docker ps
```

Make sure the MySQL container is running.

Check:

```powershell
docker logs novawallet-mysql
```

Verify:

```text
Host
Port
Database
Username
Password
```

---

## Migration fails because of `jsonb`

Search the EF configuration/migration for:

```text
jsonb
```

Change MySQL JSON properties to:

```csharp
.HasColumnType("json")
```

Then recreate the development migration if appropriate.

---

## Existing customers fail after adding Role

If `Role` is a new required column and existing customer records already exist, the migration must provide a default/backfill value.

For example:

```text
Customer = 1
```

for existing users, if that matches the intended migration policy.

Do not allow public registration to choose:

```text
Admin
Operations
```

Normal registration should assign:

```csharp
customer.Role = UserRole.Customer;
```

---

## Duplicate transaction reference

If the database has a unique index on transaction reference, do not reuse the same reference for both sides of a transfer.

Use:

```text
TRF-001-DR
TRF-001-CR
```

---

# 29. Recommended Development Workflow

When making a change:

```text
1. Update Domain entity
        ↓
2. Update EF configuration
        ↓
3. Update DTO
        ↓
4. Update validator
        ↓
5. Update application service
        ↓
6. Update controller/API contract
        ↓
7. Add migration
        ↓
8. Update database
        ↓
9. Run tests
        ↓
10. Test using Swagger
```

Example:

```powershell
dotnet build

dotnet ef migrations add AddNewFeature `
  --project Nova.DataAccess `
  --startup-project NovaPay

dotnet ef database update `
  --project Nova.DataAccess `
  --startup-project NovaPay

dotnet test

dotnet run --project NovaPay
```

---

# 30. Production Considerations

Before production deployment, additionally implement/verify:

- HTTPS everywhere
- Secure secret management
- Database encryption/backup strategy
- Rate limiting
- Account lockout / login protection
- Strong password policy
- Token expiration and rotation strategy
- Refresh token strategy if required
- Fraud/risk controls
- Reconciliation processes
- Monitoring and alerting
- Distributed tracing
- Health checks
- Database connection pool tuning
- Transaction deadlock monitoring
- Outbox retry and dead-letter handling
- Audit-log retention and archival
- Sensitive-data masking
- CI/CD pipeline
- Vulnerability scanning
- Automated database backup
- Disaster recovery procedures

---

# 31. Financial Safety Principles

The most important design principles in NovaWallet are:

### Never trust the client balance

The server/database is the source of truth.

### Never perform financial updates outside a transaction

Balance and ledger records must remain consistent.

### Never rely only on application-level locks

Use database transaction isolation/row locking for concurrent wallet mutations.

### Never execute the same financial request twice

Use an idempotency key and request hash.

### Never silently change an idempotency request

The same key with a different payload must be rejected.

### Never expose persistence entities directly

Use DTOs.

### Never store passwords in plain text

Use password hashing.

### Never log secrets

Do not log:

```text
Password
JWT
PIN
Secret keys
Sensitive authentication information
```

---

# 32. Quick Start

For a quick local setup:

```powershell
# 1. Clone repository
git clone <YOUR_REPOSITORY_URL>

# 2. Enter project
cd NovaPay

# 3. Restore packages
dotnet restore

# 4. Start MySQL
docker compose up -d mysql

# 5. Apply database migrations
dotnet ef database update `
  --project Nova.DataAccess `
  --startup-project NovaPay

# 6. Build
dotnet build

# 7. Run tests
dotnet test

# 8. Start API
dotnet run --project NovaPay
```

Then open Swagger:

```text
https://localhost:<configured-port>/swagger
```

Register → Login → Authorize → Create Wallet → Credit → Transfer → View Statement.

---

# 33. End-to-End Example

A typical transaction looks like:

```text
Customer
   |
   | POST /auth/login
   v
JWT issued
   |
   | POST /wallets
   v
Wallet created
Balance = ₦0
   |
   | POST /wallets/{id}/credits
   | Amount = ₦100,000
   v
Balance = ₦100,000
   |
   | POST /transfers
   | Amount = ₦50,000
   | Idempotency-Key = ABC123
   v
Lock source + destination
   |
Validate
   |
Check idempotency
   |
Check daily limit
   |
Debit source ₦50,000
   |
Credit destination ₦50,000
   |
Create ledger transactions
   |
Create audit records
   |
Create outbox event
   |
Commit
   |
   v
Transfer completed
```

---

# 34. Final Architecture Summary

NovaWallet is designed around four key properties:

```text
             NOVAWALLET
                  |
       +----------+----------+
       |          |          |
       v          v          v
    SECURE     CONSISTENT  AUDITABLE
       |          |          |
      JWT       DB Tx       AuditLog
      RBAC      Locking     TraceId
      Hashing   Atomicity   Outbox
       |          |          |
       +----------+----------+
                  |
                  v
             EXTENSIBLE
                  |
          Clean Architecture
          DTOs / Services
          MySQL / EF Core
          Docker
```

The central financial principle is:

> **Every financial state change must be atomic, concurrency-safe, retry-safe, traceable and auditable.**

---

## License

This project is intended for assessment, learning and demonstration purposes unless a separate license is provided.
