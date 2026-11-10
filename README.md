# .NET 8 School Admission System

A multi-container web application designed for managing school admissions, seat allocation, and preventing overbooking through concurrency control. Built with .NET 8 Web API, Entity Framework Core, PostgreSQL, and React.

## Tech Stack
- **Backend:** .NET 8 Web API, EF Core, xUnit (Testing)
- **Database:** PostgreSQL (Production), EF Core InMemory (Testing)
- **Frontend:** React (Vite), Tailwind CSS, Framer Motion
- **Containerization:** Docker & Docker Compose

---

## Key Features & UI Enhancements
- **Concurrency & Hold Management:** Prevents overbooking using database transactions and automated seat expiration timers.
- **Strict NID Validation:** Verifies student identity via 17-digit National ID / Birth Registration Number before holding seats.
- **Modern Unique UI/UX:** Transformed from default templates to a custom Dark-Mode Glassmorphism interface styled with Emerald accents and smooth entrance animations (`framer-motion`).
- **Live Seat Tracking:** Dynamic countdown timers for active seat holds and real-time dashboard analytics.

---

## Prerequisites
Make sure you have the following installed on your system:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## How to Run the Application (Docker)

1. Open a terminal in the project root folder (`D:\dotnet_project`).
2. Run the following command to build and start all services (PostgreSQL, Backend API, and Frontend):
   ```bash
   docker compose up --build -d