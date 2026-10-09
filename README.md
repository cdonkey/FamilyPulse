# FamilyPulse 🌿

> **A privacy-first, zero-knowledge family wellness and identity platform built with C# 12 and .NET 8/9 Clean Architecture.**

FamilyPulse provides a secure, anonymous space for families to perform weekly wellness check-ins, manage family roles, and receive streaming insights—without ever surrendering personal identifiable information (PII) to a centralized auth provider.

---

## 🏛️ Architectural Overview

FamilyPulse strictly adheres to **Clean Architecture** principles, maintaining clear separation of concerns, strict dependency inversion, and isolated domain logic.

```
                  ┌────────────────────────┐
                  │    FamilyPulse.Api     │  (Minimal APIs, EndpointFilters, wwwroot)
                  └───────────┬────────────┘
                              │
                  ┌───────────▼────────────┐
                  │ FamilyPulse.Application│  (Interfaces, Services, DTOs, Commands)
                  └───────────┬────────────┘
                              │
                  ┌───────────▼────────────┐
                  │  FamilyPulse.Domain    │  (Entities, Value Objects, Domain Events)
                  └───────────▲────────────┘
                              │
                  ┌───────────┴────────────┐
                  │FamilyPulse.Infrastructure│ (EF Core, SQLite, Security Hasher, HMAC)
                  └────────────────────────┘
```

---

## 💡 Key Technical & Design Decisions

### 1. Zero-Knowledge Identity Model
* **No Passwords or Emails**: Authenticates accounts using a **4-word deterministic passphrase** paired with a **Virtual Landmark ID**.
* **PBKDF2 Hashing**: Identity hashes are computed deterministically via `Pbkdf2IdentityHasher` using 100,000 SHA-256 iterations and a server-side pepper, ensuring account verification occurs without storing readable credentials.
* **Cryptographic House Key Export**: Families can export a portable, HMAC SHA-256 signed `family-key.json` file via `HouseKeyService` for zero-trust offline backup and instant single-click recovery.

### 2. High-Performance Minimal APIs & Validation
* Uses .NET Minimal APIs for ultra-low latency routing and reduced memory footprint.
* Implements custom, reusable **native `EndpointFilters`** (e.g., `ValidationFilter<T>`) to validate incoming payloads before they hit application handlers.

### 3. Lightweight Persistence with EF Core & SQLite
* Standardized on **SQLite** with EF Core to keep infrastructure self-contained, enabling rapid local development and low-cost cloud deployment with persistent storage volumes.
* Robust migration lifecycle managed explicitly via EF Core CLI migrations.

### 4. Comprehensive Testing Suite
* Fully automated **xUnit** integration and unit test suite.
* Exercises repository queries, cryptographic key generation, and identity matching using EF Core In-Memory providers.

---

## 📦 Project Structure

```
src/
├── FamilyPulse.Domain/          # Core Domain Aggregate Roots (FamilyAccount, Member, Rating)
├── FamilyPulse.Application/     # Application contracts, DTOs, and Service implementations
├── FamilyPulse.Infrastructure/  # DbContext, EF Configurations, Security & Crypto logic
└── FamilyPulse.Api/             # API Endpoints, Middleware, Validation Filters & Vanilla JS Console
tests/
└── FamilyPulse.Tests/           # xUnit test suite for Domain, Identity, and Integration
```

---

## 🛠️ Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or higher
* EF Core CLI tool (`dotnet tool install --global dotnet-ef`)

### 1. Clone & Build
```bash
git clone [https://github.com/your-username/FamilyPulse.git](https://github.com/your-username/FamilyPulse.git)
cd FamilyPulse
dotnet build
```

### 2. Run Database Migrations
```bash
dotnet ef database update --project src/FamilyPulse.Infrastructure --startup-project src/FamilyPulse.Api
```

### 3. Launch Application
```bash
dotnet run --project src/FamilyPulse.Api
```
Navigate to `http://localhost:5000` (or the configured HTTPS port) in your browser to access the built-in identity & member console.

### 4. Run Test Suite
```bash
dotnet test
```

---

## 🤝 Open Collaborations & Roadmap

I am actively building FamilyPulse as an open-source project and welcome contributions from peer developers, frontend engineers, and UX designers!

### Current Roadmap
- [x] Zero-knowledge registration & HMAC key export/recovery
- [x] Family member CRUD operations with Clean Architecture
- [ ] **Weekly Wellness Check-in Domain Engine** (Ratings, aggregate scores, trends)
- [ ] **Real-time Coaching via SSE** (Server-Sent Events streaming AI wellness advice)
- [ ] **Frontend Overhaul** (Looking for collaborators to build a modern React / Blazor / Tailwind UI)

---

## 📜 License

Distributed under the MIT License. See `LICENSE` for more information.
