# FamilyPulse 👨‍👩‍👧‍👦

FamilyPulse measures household dynamics across 4 core life domains, benchmarks metrics against peer cohorts to drive healthy family engagement, and delivers automated AI coaching.

---

## 💡 Overview & Value Proposition

FamilyPulse shifts family harmony from subjective guessing to data-driven wellness:
* **4 Core Domains:** Tracks micro-checkins across **Financial**, **Education**, **Marriage**, and **Activities**.
* **Cohort Benchmarking:** Compares household trends against anonymized demographic averages (e.g., *"Urban families with 2 children"*).
* **Healthy Engagement:** Uses relative scoring indices (-100 to +100) and percentile ranks to encourage positive household habits.
* **Automated AI Coaching:** Integrated background agents analyze 12-month behavioral trends to generate structured, actionable advice.

---

## 🏛 System Architecture

The solution follows **Clean Architecture / Explicit Architecture** principles, maintaining strict inward-only dependencies around the pure C# domain model.

```
┌───────────────────────────────────────────────┐
│                FamilyPulse.Api                │
│  (Minimal APIs, OpenAPI/Swagger, Middleware)  │
└───────────────────────┬───────────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────┐
│            FamilyPulse.Application            │
│  (Use Cases, DTOs, Semantic Kernel AI Agent)  │
└───────────────────────┬───────────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────┐
│          FamilyPulse.Infrastructure           │
│ (EF Core, SQLite/PostgreSQL, Telemetry Seeder)│
└───────────────────────┬───────────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────┐
│               FamilyPulse.Domain              │
│ (Pure Entities, Value Objects, Domain Enums)  │
└───────────────────────────────────────────────┘
```

### Key Engineering Features
* **Read-Optimized LINQ Pipelines:** Read endpoints utilize `.AsNoTracking()` and cancellation tokens for high-throughput memory efficiency.
* **Synthetic Telemetry Seeder:** Built-in `DataSeederService` populates 52 weeks (12 months) of realistic historical rating data on initial startup.
* **Semantic Kernel Agent Integration:** Asynchronous pipeline that transforms raw EF Core relational time-series telemetry into structured JSON coaching reports.

---

## 🛠 Tech Stack

* **Framework:** ASP.NET Core (.NET 8 / .NET 9)
* **Language:** C# 12
* **ORM:** Entity Framework Core (EF Core)
* **Database:** SQLite (Default for rapid local/Codespaces demo) / PostgreSQL-ready
* **AI Engine:** Microsoft Semantic Kernel
* **API Documentation:** OpenAPI / Swagger UI
* **Containerization:** Docker / GitHub Codespaces (`devcontainer.json`)

---

## 🚀 Getting Started

### Option 1: GitHub Codespaces (Zero Setup)
1. Click **Code** -> **Codespaces** -> **Create codespace on main**.
2. Once the container loads, run:
   ```bash
   dotnet run --project src/FamilyPulse.Api
   ```

### Option 2: Local CLI Setup
```bash
# Clone the repository
git clone [https://github.com/your-username/FamilyPulse.git](https://github.com/your-username/FamilyPulse.git)
cd FamilyPulse

# Restore dependencies
dotnet restore

# Run the API
dotnet run --project src/FamilyPulse.Api
```

Navigate to `http://localhost:5000/swagger` in your browser.

---

## 📊 Core Endpoints

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/api/members` | Fetches all family members across roles |
| `POST` | `/api/ratings` | Submits a micro-rating (-5 to +5) tagged to a domain |
| `GET` | `/api/reports/annual-harmony` | Computes 12-month domain percentiles & calls Semantic Kernel AI agent |

---

## 📄 License

Distributed under the **MIT License**. See `LICENSE` for details.
