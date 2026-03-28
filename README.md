# DotnetBilling

DotnetBilling is a clean, MVP-style billing and invoice management API for small businesses.

## Stack

- ASP.NET Core Web API
- .NET 10 (LTS)
- Entity Framework Core + SQL Server
- Swagger / OpenAPI
- QuestPDF for invoice PDF export

## Features

- Customer CRUD
- Product CRUD
- Invoice create, update, delete, detail, and list
- Invoice search by invoice number
- Invoice filtering by status and customer
- Line item tax calculation per item
- Payment recording and paid-status update
- Invoice PDF generation
- Seed data for quick local testing
- Global exception handling middleware

## Solution structure

```text
DotnetBilling.sln
src/
  DotnetBilling.API/
  DotnetBilling.Application/
  DotnetBilling.Domain/
  DotnetBilling.Infrastructure/
```

## Business rules implemented

- Invoice number format: `INV-YYYY-0001`
- Invoice totals are calculated from line items
- Tax is calculated per line item
- An invoice becomes `Paid` when total payments are greater than or equal to the invoice total
- Invoices with payments cannot be edited or deleted in this MVP
- Overdue invoices are resolved dynamically from due date when returned from the API

## Create the solution manually with dotnet CLI

```bash
dotnet new sln -n DotnetBilling
dotnet new webapi -n DotnetBilling.API -o src/DotnetBilling.API
dotnet new classlib -n DotnetBilling.Application -o src/DotnetBilling.Application
dotnet new classlib -n DotnetBilling.Domain -o src/DotnetBilling.Domain
dotnet new classlib -n DotnetBilling.Infrastructure -o src/DotnetBilling.Infrastructure

dotnet sln DotnetBilling.sln add src/DotnetBilling.API/DotnetBilling.API.csproj
dotnet sln DotnetBilling.sln add src/DotnetBilling.Application/DotnetBilling.Application.csproj
dotnet sln DotnetBilling.sln add src/DotnetBilling.Domain/DotnetBilling.Domain.csproj
dotnet sln DotnetBilling.sln add src/DotnetBilling.Infrastructure/DotnetBilling.Infrastructure.csproj

dotnet add src/DotnetBilling.Application/DotnetBilling.Application.csproj reference src/DotnetBilling.Domain/DotnetBilling.Domain.csproj
dotnet add src/DotnetBilling.Infrastructure/DotnetBilling.Infrastructure.csproj reference src/DotnetBilling.Application/DotnetBilling.Application.csproj
dotnet add src/DotnetBilling.Infrastructure/DotnetBilling.Infrastructure.csproj reference src/DotnetBilling.Domain/DotnetBilling.Domain.csproj
dotnet add src/DotnetBilling.API/DotnetBilling.API.csproj reference src/DotnetBilling.Application/DotnetBilling.Application.csproj
dotnet add src/DotnetBilling.API/DotnetBilling.API.csproj reference src/DotnetBilling.Infrastructure/DotnetBilling.Infrastructure.csproj
```

## NuGet packages

```bash
dotnet add src/DotnetBilling.Infrastructure package Microsoft.EntityFrameworkCore --version 10.0.5
dotnet add src/DotnetBilling.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.5
dotnet add src/DotnetBilling.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 10.0.5
dotnet add src/DotnetBilling.Infrastructure package QuestPDF --version 2026.2.3
dotnet add src/DotnetBilling.API package Swashbuckle.AspNetCore --version 10.1.5
```

## appsettings.json

Update the connection string in `src/DotnetBilling.API/appsettings.json` if you are not using LocalDB.

## Migrations

From the repository root:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/DotnetBilling.Infrastructure --startup-project src/DotnetBilling.API --output-dir Data/Migrations
dotnet ef database update --project src/DotnetBilling.Infrastructure --startup-project src/DotnetBilling.API
```

## Run

```bash
dotnet restore
dotnet build
dotnet run --project src/DotnetBilling.API
```

Swagger will be available at:

```text
https://localhost:7001/swagger
http://localhost:5001/swagger
```

## Seed data

On first startup the app adds:

- 2 customers
- 3 products

## Example API flow

### Create a customer

```http
POST /api/customers
Content-Type: application/json

{
  "name": "Wayne Enterprises",
  "email": "billing@wayne.com",
  "phone": "+1-555-0101",
  "address": "1007 Mountain Drive, Gotham",
  "taxNumber": "GSTIN-001"
}
```

### Create a product

```http
POST /api/products
Content-Type: application/json

{
  "name": "Website Maintenance",
  "description": "Monthly website support retainer",
  "unitPrice": 2500,
  "taxRate": 18
}
```

### Create an invoice

```http
POST /api/invoices
Content-Type: application/json

{
  "customerId": "00000000-0000-0000-0000-000000000000",
  "issueDate": "2026-03-15T00:00:00Z",
  "dueDate": "2026-03-22T00:00:00Z",
  "status": "Draft",
  "items": [
    {
      "productName": "Website Maintenance",
      "quantity": 1,
      "unitPrice": 2500,
      "taxRate": 18
    }
  ]
}
```

### Record payment

```http
POST /api/payments
Content-Type: application/json

{
  "invoiceId": "00000000-0000-0000-0000-000000000000",
  "paidAmount": 2950,
  "paidDate": "2026-03-16T10:00:00Z",
  "paymentMethod": "Bank Transfer",
  "referenceNumber": "UTR-987654"
}
```

### Download invoice PDF

```http
GET /api/invoices/{id}/pdf
```
