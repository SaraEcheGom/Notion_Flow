# NotionFlow


# Features

The current version of the application includes:

* Create new tasks
* View a list of tasks
* Mark tasks as completed
* Delete tasks
* Filter tasks (All / Pending / Completed)
* Task statistics (Total, Pending, Completed)
* Local persistence of tasks using JSON

---

# Architecture

The project follows the **MVVM (Model–View–ViewModel)** architecture pattern.

### Models

Represents the data structure used by the application.

Example:

* `TaskItem`

### Views

Responsible for the user interface.

Example:

* `MainPage.xaml`

### ViewModels

Contains the application logic and manages the interaction between the UI and the data.

Example:

* `TaskViewModel`

### Services

Handles data persistence and storage logic.

Example:

* `TaskService`

---

# Technologies Used

* **.NET MAUI**
* **C#**
* **MVVM Pattern**
* **JSON Local Storage**
* **SonarLint (Static Code Analysis)**

---

# Project Structure

```
NotionFlow.App
│
├── Models
│   └── TaskItem.cs
│
├── Services
│   └── TaskService.cs
│
├── ViewModels
│   └── TaskViewModel.cs
│
├── Platforms
│
├── Resources
│
├── MainPage.xaml
└── MainPage.xaml.cs
```

---

# Quality of Software

The project applies basic software quality practices:

* C# naming conventions
* XML documentation comments
* Separation of concerns through MVVM architecture

---

# MVP - Sprint 1

The Minimum Viable Product for Sprint 1 includes:

* Task creation
* Task visualization
* Task completion
* Task deletion
* Data persistence
* Basic task statistics

---

# How to Run the Project

### 1 Clone the repository

```
git clone https://github.com/SaraEcheGom/Notion_Flow.git
```

### 2 Navigate to the project folder

```
cd Notion_Flow/NotionFlow.App
```

### 3 Run the application

```
dotnet run -f net9.0-windows10.0.19041.0
```

---

# Future Improvements

Possible improvements for future sprints:

* Task editing
* Cloud synchronization
* User authentication
* UI improvements
* Task deadlines and reminders

---




