

## 1. Seat Hold Duration (`HOLD_DURATION_SECONDS`)
- **Assumption:** When a student initiates a seat hold, the seat is reserved for a default duration of **60 seconds** before expiring automatically if not confirmed.
- **Configuration:** This duration can be configured via environment variables or `appsettings.json` using the `HOLD_DURATION_SECONDS` key.

## 2. Concurrency Management & Overbooking Prevention
- **Challenge:** In high-traffic admission portals, multiple students might try to grab the last remaining seat simultaneously.
- **Solution:** 
  * Implemented database transactions (`BeginTransactionAsync`) in the production service (`AdmissionService`) to ensure atomicity.
  * For automated unit testing (`xUnit` with EF Core InMemory), where transactions are not natively supported, a thread-safe `SemaphoreSlim` mechanism is utilized to simulate isolation and prevent race conditions.

## 3. Automated Database Migration
- **Assumption:** In containerized environments, manual database migration commands (`dotnet ef database update`) are prone to friction.
- **Solution:** Configured `dbContext.Database.Migrate()` directly in `Program.cs` on startup, ensuring the PostgreSQL schema is automatically created or updated when the backend container boots up.

## 4. UI Customization & Frontend Docker Setup
- **Decision:** Kept production-grade original `Dockerfile` (Node build + Nginx container serving on port 80, mapped to `5173:80` in Compose) to avoid duplicate development configuration files.
- **UI Customization:** Upgraded default UI to a modern Glassmorphism dark-mode theme utilizing Tailwind CSS and `framer-motion` for micro-interactions to distinguish the portal from generic templates.