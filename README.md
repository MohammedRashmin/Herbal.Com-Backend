# E-Commerce Backend API

A clean, layered architecture backend API built with ASP.NET Core 10.0, following best practices for separation of concerns, dependency injection, and repository pattern.

## 🏗️ Architecture Overview

This project follows a **layered architecture** with clear separation of concerns:

```
Controllers (Traffic Police)
    ↓
Services (Business Logic)
    ↓
Repositories (Data Access)
    ↓
Database
```

### Key Design Patterns

1. **Dependency Injection (DI)**: Controllers depend on Interfaces (e.g., `IAuthService`), not concrete classes, making the code testable and flexible.

2. **Repository Pattern**: Isolates database logic. If you switch databases later, you only change the Repository layer.

3. **Feature Slicing**: Separating Admin and User logic ensures that complex Admin features don't accidentally break the Customer experience.

## 📁 Project Structure

```
Web.Com/
├── Controllers/
│   ├── Admin/          # Admin-only endpoints (protected with [Authorize(Roles = "Admin")])
│   ├── User/           # User/Customer endpoints (protected with [Authorize(Roles = "Customer")])
│   └── Shared/          # Public or shared endpoints (Auth, etc.)
│
├── Services/
│   ├── User/           # User-specific business logic
│   └── Shared/         # Shared business logic (Auth, etc.)
│
├── Repositories/
│   ├── Interfaces/     # Repository contracts
│   │   ├── Admin/
│   │   ├── User/
│   │   └── Shared/
│   └── Implementations/ # Repository implementations
│       ├── Admin/
│       ├── User/
│       └── Shared/
│
├── Models/
│   └── Identity/       # Database entities (ApplicationUser, etc.)
│
├── DTOs/               # Data Transfer Objects
│   ├── Admin/          # Admin-specific DTOs
│   ├── User/           # User-specific DTOs
│   └── Shared/         # Shared DTOs (Login, Signup, etc.)
│
├── Data/
│   ├── AppDbContext.cs # Entity Framework DbContext
│   └── Migrations/     # Database migrations
│
└── Helpers/
    ├── JwtTokenGenerator.cs  # JWT token generation
    └── Constants/            # Static constants (Roles, OrderStatus, etc.)
```

## 🔐 Authentication Endpoints

### 1. Signup (Register New User)

**Endpoint:** `POST /api/auth/signup`

**Description:** Creates a new user account with email, first name, last name, and password.

**Request Body:**
```json
{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roles": ["Customer"]
}
```

**Error Responses:**
- `400 Bad Request`: Missing required fields or password mismatch
- `409 Conflict`: User with this email already exists
- `500 Internal Server Error`: Server error

---

### 2. Login

**Endpoint:** `POST /api/auth/login`

**Description:** Authenticates a user with email and password, returns JWT token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roles": ["Customer"]
}
```

**Error Responses:**
- `400 Bad Request`: Missing email or password
- `401 Unauthorized`: Invalid email or password
- `500 Internal Server Error`: Server error

---

## 🚀 Getting Started

### Prerequisites

- .NET 10.0 SDK
- SQL Server (LocalDB or full SQL Server instance)
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Web.Com
   ```

2. **Update Connection String**
   
   Edit `appsettings.json` and update the `DefaultConnection` string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WebComDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Update JWT Configuration**
   
   Edit `appsettings.json` and set a secure JWT key (at least 32 characters):
   ```json
   {
     "Jwt": {
       "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
       "Issuer": "Web.Com",
       "Audience": "Web.Com"
     }
   }
   ```

4. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

5. **Create Database Migration**
   ```bash
   dotnet ef migrations add InitialCreate --project Web.Com
   ```

6. **Apply Migrations**
   ```bash
   dotnet ef database update --project Web.Com
   ```

7. **Run the Application**
   ```bash
   dotnet run --project Web.Com
   ```

   The API will be available at `https://localhost:5001` or `http://localhost:5000`

## 📝 Data Flow Example

### User Login Flow

1. **Frontend** sends `POST /api/auth/login` with email and password
2. **Controller** (`AuthController.cs`): Validates input, calls `_authService.LoginAsync()`
3. **Service** (`AuthService.cs`): 
   - Calls `_userRepository.GetByEmailAsync()` to find user
   - Calls `_userRepository.CheckPasswordAsync()` to validate password
   - Calls `_jwtTokenGenerator.GenerateToken()` to create JWT
4. **Repository** (`UserRepository.cs`): Interacts with Identity framework to access database
5. **Response**: Controller returns JWT token and user info as JSON

### User Signup Flow

1. **Frontend** sends `POST /api/auth/signup` with user details
2. **Controller** (`AuthController.cs`): Validates input, calls `_authService.SignupAsync()`
3. **Service** (`AuthService.cs`): 
   - Validates all required fields
   - Checks if passwords match
   - Checks if user already exists
   - Calls `_userRepository.CreateAsync()` to create user
   - Calls `_jwtTokenGenerator.GenerateToken()` to create JWT
4. **Repository** (`UserRepository.cs`): Creates user in database and assigns default "Customer" role
5. **Response**: Controller returns JWT token and user info as JSON

## 🔧 Technology Stack

- **Framework**: ASP.NET Core 10.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity + JWT Bearer Tokens
- **Architecture**: Layered Architecture with Repository Pattern
- **Dependency Injection**: Built-in .NET DI Container

## 📦 NuGet Packages

- `Microsoft.EntityFrameworkCore` (10.0.1)
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.1)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.1)
- `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0.1)
- `System.IdentityModel.Tokens.Jwt` (8.2.1)

## 🧪 Testing the API

### Using Postman or Similar Tool

**Signup Request:**
```
POST https://localhost:5001/api/auth/signup
Content-Type: application/json

{
  "email": "test@example.com",
  "firstName": "Test",
  "lastName": "User",
  "password": "Test123!",
  "confirmPassword": "Test123!"
}
```

**Login Request:**
```
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test123!"
}
```

**Using the Token:**
For protected endpoints, include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## 📚 Layer Responsibilities

### Controllers (Traffic Police)
- Accept HTTP requests (GET, POST, PUT, DELETE)
- Validate inputs
- Call Service layer
- Return JSON responses
- Handle authorization with `[Authorize]` attributes

### Services (Business Logic)
- Contains all business rules
- Never talks to database directly; uses Repositories
- Examples: "If user buys 3 items, apply 10% discount"

### Repositories (Storage Manager)
- Isolates database logic
- Provides abstraction over data access
- Can switch databases by changing only Repository layer

## 🔒 Security Features

- Password hashing using ASP.NET Core Identity
- JWT token-based authentication
- Role-based authorization (Admin, Customer)
- Secure password requirements (min 6 chars, uppercase, lowercase, digit)

## 📄 License

This project is part of a learning exercise following clean architecture principles.

---

**Note**: Remember to change the JWT key in production and use environment variables for sensitive configuration!
