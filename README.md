# 🧩 Modular CMS

**Modular CMS** is a full-featured Content Management System (CMS) built using ASP.NET Core Web API for the backend and two separate frontend applications: **React** (user interface) and **Angular** (admin dashboard). The project demonstrates modern full-stack development, authentication, modular architecture, and containerized deployment using Docker.

---

## ⚙️ Tech Stack

- **Backend**: ASP.NET Core 8, Entity Framework Core, AutoMapper, JWT Auth
- **Frontend**:
  - `frontend-react/` → React + TypeScript (User interface)
  - `frontend-angular/` → Angular + TypeScript (Admin panel)
- **Database**: PostgreSQL
- **API Docs**: Swagger
- **Containerization**: Docker, Docker Compose

---

## 🧱 Project Structure

```
modular-cms/
├── backend/                 # ASP.NET Core Web API
├── frontend-react/          # React (user site)
├── frontend-angular/        # Angular (admin panel)
├── docker-compose.yml       # Docker container orchestration
└── README.md
```

---

### ▶️ Run the project

```bash
# Clone the repository
git clone https://github.com/ILLIAK31/Modular_CMS.git
cd Modular_CMS

# Build and run the services
docker-compose up --build
```
---

## 🔐 Authentication & Roles

JWT-based authentication with role-based access:

- `Admin` – Full access to CMS
- `Editor` – Can manage content
- `User` – Can read and comment

---

## 📚 Features

- 📝 Full CRUD for blog posts
- 📁 Category management
- 👥 User & role management (in admin panel)
- 🖼️ Optional media file uploads
- 🔐 Login / Registration / JWT-based sessions
- 📖 API docs via Swagger
- 🐳 Fully containerized with Docker

---

## 👤 Author

- GitHub: [https://github.com/ILLIAK31](https://github.com/ILLIAK31)
