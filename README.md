# E-Commerce Project

Welcome to the E-Commerce repository! This project is a full-stack web application built using **ASP.NET Core** (targeting .NET 8.0), designed to provide a scalable foundation for modern online commerce solutions.

---

## 📚 Table of Contents

- [Overview](#-overview)
- [Architecture & Technologies](#-architecture--technologies)
- [Project Structure](#-project-structure)
- [Getting Started](#️-getting-started)
  - [Clone this Repository](#1-clone-this-repository)
  - [Prerequisites](#2-prerequisites)
  - [Configure the Database](#3-configure-the-database)
  - [Database Migration](#4-database-migration)
  - [Run the Application](#5-run-the-application)
- [API Documentation](#-api-documentation)
- [Security & Sensitive Configurations](#-security--sensitive-configurations)
- [Authentication & Authorization](#-authentication--authorization)
- [Example Features](#-example-features)
- [Contributing](#-contributing)
- [Contact](#-contact)

---

## 🚀 Overview

This E-Commerce platform implements core online shopping features, including user authentication, product management, order processing, and more. The backend is powered by ASP.NET Core Web API with Entity Framework Core for data access, Identity for user management, and JWT for secure authentication.

---
<a name="-architecture--technologies"></a>
## 🏗️ Architecture & Technologies

- **Backend:** ASP.NET Core Web API
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Identity & JWT Bearer Tokens
- **Database:** SQL Server (configurable in `appsettings.json`)
- **Dependency Injection:** Built-in .NET Core DI
- **API Documentation:** Swagger (easy to enable in development)
- **Other:** CORS enabled for cross-origin API consumption

---

## 📁 Project Structure

- `/Project/Data/DB/` — Database context and migration logic (`AppDbContext`).
- `/Project/Services/Interfaces/` — Service interfaces for business logic abstraction.
- `/Project/Services/Implementations/` — Implementations of service logic (e.g., `EmailService`, `TokenService`).
- `/Project/Data/Tables/` — Entity classes representing database tables (e.g., `Person` for users).
- `Program.cs` — Main configuration for the app: DI container, authentication, CORS, and more.

---

## 🛠️ Getting Started

### 1. Clone this Repository

```bash
git clone https://github.com/Shady-Mo/E-Commerce.git
cd E-Commerce
```

### 2. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download) installed
- SQL Server instance available (local or remote)

### 3. Configure the Database

- Copy `appsettings.json` (example below) into your project root.
- **DO NOT commit `appsettings.json` with real credentials!** (see Security section)

<details>
<summary>Example <code>appsettings.json</code> (click to expand)</summary>

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "JWT": {
    "Issuer": "http://localhost:YOUR_PORT",
    "Audience": "http://localhost:YOUR_PORT",
    "SecretKey": "YOUR_SECRET_KEY"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.yourprovider.com",
    "Port": 587,
    "Email": "your-email@example.com",
    "AppPassword": "your-app-password"
  }
}
```
</details>

- Adjust the connection string and other settings as needed.
- To keep secrets safe, use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development.

### 4. Database Migration

If EF Core migrations are set up, run:

```bash
dotnet ef database update
```

Otherwise, you may need to generate migrations (see EF Core docs).

### 5. Run the Application

```bash
dotnet run
```

The API will start and listen on the default port (usually http://localhost:5000).

---

## 🧪 API Documentation

For easier API exploration, Swagger UI can be enabled in development by uncommenting the related lines in `Program.cs`:

```csharp
// builder.Services.AddSwaggerGen();
```

Once enabled, navigate to `/swagger` in your browser to view and test the API endpoints.

---

## 🔒 Security & Sensitive Configurations

**Never commit sensitive data!**

Some files and folders are excluded from the repository for security and organizational reasons, such as:

- Local configuration files (`appsettings.json`, `appsettings.Development.json`, `appsettings.Local.json`)
- Build output folders (`bin/`, `obj/`)
- Personal Visual Studio settings (`.vs/`, `*.user`, `*.suo`)
- Local database files (`*.db`, `*.sqlite`)
- Uploaded or generated images (`wwwroot/Product_Image/`, `wwwroot/Profile_Image/`)

These files and folders are listed in the project's `.gitignore` and will not be included in the repository.

If you clone or pull the project, you will need to create your own configuration files (`appsettings.json`, `appsettings.Development.json`) as they are not included for security reasons.

### Example of `appsettings.Development.json` content

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

- Edit the values according to your development environment.
- Do **not** share any sensitive data like passwords or API keys in the repository.
- Use environment variables or User Secrets for API keys, connection strings, and credentials.
- Review all code and configuration before making the repository public.

> **Note:** If you accidentally commit sensitive data, immediately remove it from git history and rotate any exposed secrets.

---

## 👤 Authentication & Authorization

- Uses ASP.NET Identity for user and role management.
- JWT Bearer tokens are used for stateless API authentication.
- Configure token signing keys and other sensitive settings outside of version control.

---

## 🛒 Example Features

While the repository may evolve, the typical E-Commerce functionality includes:

- User registration and login
- Role-based access (e.g., admin, customer)
- Product catalog CRUD
- Shopping cart management
- Order creation and tracking
- Email notifications (via `IEmailService`)

---

## 🤝 Contributing

Contributions are welcome! Please fork the repo and submit a pull request.

- Follow C# and .NET coding conventions.
- Write clear, descriptive commit messages.
- Add tests for new features where applicable.

---

## 📬 Contact

Maintained by [Shady-Mo](https://github.com/Shady-Mo).  
For questions or feedback, please open an issue.

---
