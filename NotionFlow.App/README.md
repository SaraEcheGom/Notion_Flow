# NotionFlow

## Features

The current version of the application includes:

- User registration  
- User authentication (login)  
- Role-based access control (Admin, Profesor, Estudiante)  
- Secure user management  
- Task creation  
- Task visualization  
- Task completion  
- Task deletion  
- Task filtering (All / Pending / Completed)  
- Task statistics (Total, Pending, Completed)  
- Data persistence using PostgreSQL (API) and JSON (App)  

---

## Architecture

The project follows a Clean Architecture approach with clear separation of concerns:

- Presentation Layer (API)  
- Application Layer (Business Logic)  
- Infrastructure Layer (Data Access)  
- Domain Layer (Core Entities)  

The frontend application follows the MVVM (Model–View–ViewModel) pattern.

---

## Layers Description

### Domain

Contains the core business entities and rules.

Examples:
- User  
- Role  

---

### Application (NotionFlow.App)

Contains business logic, services, and use cases.

Examples:
- AuthService  
- UserService  
- TaskService  

---

### Infrastructure

Handles data persistence and database interaction.

Examples:
- DbContext  
- Repositories  
- Entity configurations  
- Migrations  

---

### API (NotionFlow.Api)

Responsible for handling HTTP requests and exposing endpoints.

Examples:
- AuthController  
- UserController  

---

### App (Frontend - .NET MAUI)

Implements the user interface using the MVVM pattern.

Examples:
- MainPage.xaml  
- TaskViewModel  

---

## Technologies Used

- .NET (ASP.NET Core Web API)  
- .NET MAUI  
- C#  
- Entity Framework Core  
- PostgreSQL  
- MVVM Pattern  
- JSON Local Storage  
- Git and GitHub  
- SonarLint  

---

## Project Structure

```bash
Notion_Flow/
│
├── NotionFlow.Api/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Middlewares/
│
├── NotionFlow.App/
│   ├── Models/
│   ├── Services/
│   ├── ViewModels/
│   ├── Views/
│   ├── Platforms/
│   │   ├── Android/
│   │   ├── Windows/
│   │   └── iOS/
│   ├── Resources/
│
├── NotionFlow.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   ├── Configurations/
│   └── Migrations/
│
├── NotionFlow.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── ValueObjects/
│
└── NotionFlow.sln

```
## API Endpoints

### Authentication

- **POST** `/api/auth/register` → Register a new user  
- **POST** `/api/auth/login` → Authenticate a user  

---

## Software Quality

The project applies the following practices:

- Clean Architecture principles  
- MVVM pattern for frontend  
- Separation of concerns  
- Consistent naming conventions in C#  
- Modular and scalable design  
- Basic validation and access control  

---

## MVP - Sprint 1

The Minimum Viable Product includes:

- User registration  
- User authentication  
- Role management  
- Task creation  
- Task visualization  
- Task completion  
- Task deletion  
- Data persistence  

---

## How to Run the Project

### 1. Clone the repository

```bash
git clone https://github.com/SaraEcheGom/Notion_Flow.git
