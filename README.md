# Smart Workspace Manager 🚀

A full-stack collaborative workspace management platform inspired by Jira and Trello. The application helps teams organize projects, manage tasks, collaborate in real time, and track progress through an intuitive Kanban board interface.

## 🌐 Live Demo

* **Frontend:** https://smart-workspace-manager.vercel.app
* **Backend API (Swagger):** https://smart-workspace-manager.onrender.com/swagger/index.html

---

## ✨ Features

### 🔐 Authentication & Authorization

* User registration and login using JWT authentication.
* Secure API access with role-based authorization support.

### 🏢 Workspace Management

* Create and manage multiple workspaces.
* Invite and manage workspace members.
* Isolated collaboration environment for each workspace.

### 📋 Kanban Board Management

* Create boards and customize workflow columns.
* Create, update, delete, and organize tasks.
* Drag-and-drop functionality for seamless task movement.

### 👥 Task Collaboration

* Assign members to tasks.
* Set and update due dates.
* Add task comments for discussions.
* Upload and manage file attachments.

### ⚡ Real-time Features

* Real-time board synchronization using SignalR.
* Instant workspace chat for team communication.
* Live updates across connected users without page refresh.

### ☁️ Cloud Deployment

* Frontend deployed on Vercel.
* Backend deployed on Render.
* PostgreSQL database hosted on Supabase.
* File storage managed through Supabase Storage.

---

## 🏗️ Architecture

This project follows a Monorepo structure and a layered architecture approach to improve maintainability, scalability, and separation of concerns.

```text
SmartWorkspaceManager
├── frontend
└── backend
    ├── API
    ├── Application
    ├── Domain
    ├── Infrastructure
    └── Persistence
```

### Backend Layers

* **API** – Controllers, middleware, dependency injection configuration.
* **Application** – Business logic, services, DTOs, interfaces.
* **Domain** – Core entities, enums, and domain models.
* **Infrastructure** – External integrations such as authentication, notifications, and storage services.
* **Persistence** – Entity Framework Core, repositories, and database configurations.

---

## 🛠️ Tech Stack

### Backend

* ASP.NET Core 8 Web API
* Entity Framework Core
* PostgreSQL
* SignalR
* JWT Authentication

### Frontend

* React 19
* TypeScript
* Vite
* Tailwind CSS
* TanStack Query
* React Router

### Cloud & DevOps

* Supabase (PostgreSQL & Storage)
* Render
* Vercel
* GitHub

---

## 📸 Key Functionalities

* Workspace and member management
* Kanban board workflow
* Task assignment and due dates
* Task comments
* File attachments
* Real-time board updates
* Real-time workspace chat
* Cloud-based deployment

---

## 🚀 Getting Started

### Clone Repository

```bash
git clone https://github.com/<your-username>/SmartWorkspaceManager.git
cd SmartWorkspaceManager
```

### Backend

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

---

## 📚 Future Improvements

* Activity Log
* Notification Center
* Role & Permission Management
* Email Invitations
* Dashboard & Analytics
* Unit & Integration Testing

---

## 👨‍💻 Author

Developed by Trung as a personal project to explore modern full-stack development with ASP.NET Core, React, SignalR, and cloud-native services.
