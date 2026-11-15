# FamilyPulse 🌿

> **A privacy-first, zero-knowledge family wellness and systemic AI coaching platform built with C# 12 and .NET 8/9 Clean Architecture.**

FamilyPulse provides a secure, anonymous space for families to perform weekly wellness check-ins, manage family roles, and receive real-time streaming insights—without ever surrendering personal identifiable information (PII) to a centralized auth provider.

---

## 🎥 3-Minute Interactive Walkthrough

https://github.com/user-attachments/assets/3e9ea4d5-1a13-4e51-8d73-ed6f47f5e910

*Video walkthrough demonstrating one-click zero-knowledge family registration, check-in telemetry logging, member context switching, and live SSE AI coaching stream.*

<details>
<summary><strong>📸 Inspect Showcase Dashboard, Identity Workstation, Swagger API & JSON Contracts</strong></summary>

<br>

### 1. Interactive Showcase Dashboard (`/demo`)

<img width="1019" height="708" alt="Screenshot From 2026-11-15 09-02-50" src="https://github.com/user-attachments/assets/af422154-04c6-4bab-9870-6d42c76f047f" />
<br>
<img width="1025" height="429" alt="Screenshot From 2026-11-15 08-55-48" src="https://github.com/user-attachments/assets/ea960bd9-8d5f-4414-898a-88c47a81855d" />
<br>
<img width="1019" height="708" alt="Screenshot From 2026-11-15 08-58-21" src="https://github.com/user-attachments/assets/43dac880-8e9e-4e38-bd56-7ac945110f4b" />

### 2. Identity Verification & Member Workstation (`/test`)

<img width="844" height="440" alt="Screenshot From 2026-11-15 09-16-45" src="https://github.com/user-attachments/assets/9e106ddb-c0e7-40fe-906e-a1f24a2cb0b3" />
<br>
<img width="817" height="386" alt="Screenshot From 2026-11-15 09-36-21" src="https://github.com/user-attachments/assets/9eb6e9d0-a873-4ecd-9427-f0ee181110cd" />
<br>
<img width="889" height="503" alt="Screenshot From 2026-11-15 09-18-58" src="https://github.com/user-attachments/assets/07d70ddd-aec6-460e-8dea-c5688dee4098" />

### 3. OpenAPI / Swagger Endpoints (`/swagger`)

<img width="1332" height="760" alt="Screenshot From 2026-11-15 09-24-49" src="https://github.com/user-attachments/assets/f4865087-6cbb-4029-82bf-1292ef39e5c3" />

### 4. Sample Cryptographic House Key & Telemetry Contracts
```json
{
  "familyAccountId": "53ac73e0-ef1f-4c16-a5a8-1bea95ecabec",
  "memberId": "358b1d60-86a2-43cd-96e5-e7a94ad6e44e",
  "type": "WeeklyRoutine",
  "category": "Financial",
  "stressLevel": 4,
  "closenessLevel": 2,
  "energyLevel": 2,
  "note": "Worried about upcoming utility bills and school expenses.",
  "tags": ["budget", "overtime", "financial-stress"]
}
```

</details>

---

## 🏛️ Architectural Overview

FamilyPulse strictly adheres to **Clean Architecture** principles, maintaining clear separation of concerns, strict dependency inversion, and isolated domain logic.

```
                  ┌────────────────────────┐
                  │    FamilyPulse.Api     │  (Minimal APIs, SSE Streaming, EndpointFilters, wwwroot)
                  └───────────┬────────────┘
                              │
                  ┌───────────▼────────────┐
                  │ FamilyPulse.Application│  (AI Coaching Orchestrator, Interfaces, Services, DTOs)
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
- **No Passwords or Emails**: Authenticates accounts using a **4-word deterministic passphrase** paired with a **Virtual Landmark ID**.
- **PBKDF2 Hashing**: Identity hashes are computed deterministically via `Pbkdf2IdentityHasher` using 100,000 SHA-256 iterations and a server-side pepper, ensuring account verification occurs without storing readable credentials.
- **Cryptographic House Key Export**: Families can export a portable, HMAC SHA-256 signed `family-key.json` file via `HouseKeyService` for zero-trust offline backup and instant single-click recovery.

### 2. Systemic AI Coaching via Server-Sent Events (SSE)
- **Context-Aware Prompt Orchestration**: Aggregates short-term member telemetry (stress, closeness, and energy ratings plus contextual `#tags`) alongside relational household roles to generate tailored systemic insights.
- **Real-Time Token Streaming**: Streams LLM responses over native HTTP **Server-Sent Events (SSE)** directly to the client for low-latency, progressive Markdown rendering.

### 3. High-Performance Minimal APIs & Validation
- Uses .NET Minimal APIs for ultra-low latency routing and reduced memory footprint.
- Implements custom, reusable **native `EndpointFilters`** (e.g., `ValidationFilter<T>`) to validate incoming payloads before they hit application handlers.

### 4. Lightweight Persistence with EF Core & SQLite
- Standardized on **SQLite** with EF Core to keep infrastructure self-contained, enabling rapid local development, zero-config GitHub Codespaces execution, and persistent storage volumes.
- Robust migration lifecycle managed explicitly via EF Core CLI migrations.

### 5. Comprehensive Testing Suite
- Fully automated **xUnit** integration and unit test suite.
- Exercises repository queries, cryptographic key generation, and identity matching using EF Core In-Memory providers.

---

## 📦 Project Structure

```
src/
├── FamilyPulse.Domain/          # Core Domain Aggregate Roots (FamilyAccount, Member, Rating)
├── FamilyPulse.Application/     # AI Orchestrator, Application contracts, DTOs, and Services
├── FamilyPulse.Infrastructure/  # DbContext, EF Configurations, Security & Crypto logic
└── FamilyPulse.Api/             # API Endpoints, SSE Streaming, Filters & wwwroot UIs
tests/
└── FamilyPulse.Tests/           # xUnit test suite for Domain, Identity, and Integration
```

---

## 🛠️ Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or higher
- EF Core CLI tool (`dotnet tool install --global dotnet-ef`)
- An LLM API key (Groq / OpenAI-compatible) configured via User Secrets or environment variables

### 1. Clone & Build
```bash
git clone https://github.com/cdonkey/FamilyPulse.git
cd FamilyPulse
dotnet build
```

### 2. Configure AI Secret & Run Database Migrations
```bash
dotnet user-secrets set "AiProvider:ApiKey" "your-api-key-here" --project src/FamilyPulse.Api
dotnet ef database update --project src/FamilyPulse.Infrastructure --startup-project src/FamilyPulse.Api
```

### 3. Launch Application
```bash
dotnet run --project src/FamilyPulse.Api
```
Navigate to `http://localhost:5000` in your browser to access:
- **`/demo`** — Interactive AI Coaching Showcase Dashboard
- **`/test`** — Identity Verification & Member Management Workstation
- **`/swagger`** — Interactive OpenAPI Specification

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
- [x] **Weekly Wellness Check-in Domain Engine** (Ratings, aggregate scores, tags)
- [x] **Real-time Coaching via SSE** (Server-Sent Events streaming AI wellness advice)
- [ ] **Frontend Overhaul** (Looking for collaborators to build a modern React / Blazor / Tailwind UI)

---

## 📜 License

Distributed under the MIT License. See `LICENSE` for more information.
