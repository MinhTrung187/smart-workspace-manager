# Smart Workspace Manager 🚀

A modern, minimalist task management and team collaboration platform. It features real-time updates and interactive Kanban boards to streamline team workflows.

## 🌐 Live Demo
* **Frontend (Vercel):** [https://smart-workspace-manager.vercel.app](https://smart-workspace-manager.vercel.app)
* **Backend API (Render + Swagger):** [https://smart-workspace-manager.onrender.com/swagger/index.html](https://smart-workspace-manager.onrender.com/swagger/index.html)

---

## ✨ Core Features
* **🔐 Secure Auth:** User registration and login using JWT Bearer Tokens.
* **💼 Workspaces:** Create independent team workspaces and manage member invitations.
* **📋 Real-time Kanban Board:** Create, update, and drag-and-drop tasks across custom columns. Powered by **SignalR** for instant updates across all users.
* **💬 Real-time Chat:** Built-in chat system within each workspace for seamless team communication.

---

## 🏗️ Tech Stack
This project is structured as a **Monorepo** containing both the Frontend and Backend services.

### Backend (`/backend`)
* **Framework:** ASP.NET Core 8.0/9.0 Web API
* **Architecture:** Clean Architecture (Domain, Application, Infrastructure, API)
* **Real-time:** ASP.NET Core SignalR (WebSockets)
* **Database & ORM:** PostgreSQL (Hosted on Supabase) with Entity Framework Core

### Frontend (`/frontend`)
* **Library:** React 19 (Vite)
* **Styling:** Tailwind CSS
