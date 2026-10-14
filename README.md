# R2D2 - Low-Code / No-Code Platform MVP

R2D2 is a modular platform that allows you to build applications visually. You can create custom fields, design tables, build forms, configure business rules, and design visual workflows without writing a single line of code.

## 🚀 Quick Start

### 1. Database Setup
The application requires SQL Server. The default connection string is configured for `Server=AP27-SERVER` and `Database=R2D2`.

Ensure you have created the database `R2D2` on your SQL Server instance.

### 2. Running the API
```powershell
cd src/R2D2.API
dotnet run
```
- **Port:** `https://localhost:5001`
- **Swagger Documentation:** `https://localhost:5001/swagger`

### 3. Running the Blazor Frontend
```powershell
cd frontend/R2D2.Blazor
dotnet run
```
- **Port:** `http://localhost:5002`

## 🔑 Default Credentials
- **Username:** `admin`
- **Password:** `admin123`

## 🏗️ Architecture
The project follows a Clean Architecture / Layered approach:
- **src/R2D2.Domain**: Core entities (Fields, Lists, Forms, Rules, Workflows).
- **src/R2D2.Application**: Business logic, Services, Rule Execution Engine.
- **src/R2D2.Infrastructure**: Entity Framework Core with SQL Server, EAV storage pattern.
- **src/R2D2.API**: RESTful API with JWT Authentication and Swagger.
- **frontend/R2D2.Blazor**: Modern Blazor Server UI for platform management.

## ✨ Features
- **Dynamic Fields**: Configure fields with technical names, labels, types, and validation.
- **Dynamic Lists**: Group fields into tables (entities).
- **Form Builder**: Design layouts for data entry.
- **Business Rules**: Define validation, calculations, and automated actions.
- **Workflows**: (MVP) Visual steps to execute business logic.
- **Data Management**: Full CRUD for all dynamically created entities.

## 🛠️ Tech Stack
- .NET 8.0
- Entity Framework Core
- SQL Server
- Blazor Server
- Serilog (Structured Logging)
- FluentValidation
- JWT Bearer Authentication
