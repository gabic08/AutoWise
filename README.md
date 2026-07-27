# AutoWise

> Work in progress.

AutoWise helps users track their vehicles — specs, license plates, maintenance history, and photos/documents — behind a single API gateway, with a vanilla-JS frontend.

```
AutoWise/
├── Backend/     .NET 10 microservices + YARP API gateway
└── Frontend/    Vanilla JS SPA (no build tools)
```

## Architecture

```
Browser (Frontend/)
  │  Bearer token (Azure AD)
  ▼
AutoWise.YarpApiGateway  (:7080)
  ├── /api/user-vehicles/**     → AutoWise.UserVehicles.API     (:7279)
  ├── /api/vehicles-catalog/**  → AutoWise.VehiclesCatalog.API  (:4040)
  └── /api/media/**             → AutoWise.Media.API            (:7037)

UserVehicles ──gRPC──▶ VehiclesCatalog (VIN → specs, Redis-cached both sides)
Media ◀──RabbitMQ (MassTransit outbox/inbox)──▶ UserVehicles  (attachment upload/removal events)
```

- **Auth**: JWT Bearer at the gateway (Azure AD today, Keycloak pluggable via `Auth:ActiveProvider`). The gateway validates the token, resolves/creates the matching internal user via a gRPC call to `AutoWise.Users`, caches that mapping in Redis, and forwards requests with an `X-User-Id` header — downstream services never talk to Azure AD directly. Claim names (external id / email / display name) are configurable per provider, since which claim carries what varies by provider and even by account type.
- **Service discovery**: gateway cluster destinations are resolved via `Microsoft.Extensions.ServiceDiscovery` (config-based) instead of hardcoded URLs.
- **CORS**: gateway allows the frontend's dev origin (`Cors:AllowedOrigins`).

## Services

| Service | Stack | Notes |
|---|---|---|
| `YarpApiGateway` | YARP | Auth, CORS, rate limiting, service discovery, routing |
| `UserVehicles` | Clean Architecture, PostgreSQL | Vehicles, events, attachments; list endpoint supports pagination/search/sort |
| `VehiclesCatalog` | Vertical slices (Carter/MediatR), MongoDB | VIN → specs, via external VIN decoder API; exposed over HTTP + gRPC |
| `Media` | Clean Architecture, PostgreSQL | File uploads, content-hash dedup, pluggable storage (local disk / S3 / Azure Blob) |
| `Users` | Clean Architecture, PostgreSQL | Internal user records, resolved from external identity by the gateway (gRPC only, no public HTTP) |
| `CommonUtilities` | Shared libraries | Base entities, exceptions, messaging abstractions, EF interceptors — consumed as prebuilt DLLs, not project refs |

Messaging between Media and UserVehicles uses MassTransit + RabbitMQ with an EF Core transactional outbox/inbox for reliable, idempotent delivery.

## Frontend

Plain HTML/CSS/JS, no framework or build step — `Frontend/JavaScript/`. Signs in via MSAL.js (real Azure AD redirect login), then calls the gateway directly for the vehicle list/detail views. Needs its own `config.js` (gitignored — copy `config.example.js` and fill in your App Registration values) and to be served over `http://localhost:5500` (matching the registered redirect URI and the gateway's CORS/Azure config), not opened as a local file.

## Running locally

| Service | HTTPS port | Depends on |
|---|---|---|
| YarpApiGateway | `7080` | UserVehicles, VehiclesCatalog, Media, Users all running |
| UserVehicles.API | `7279` | PostgreSQL, Redis, RabbitMQ, VehiclesCatalog (gRPC) |
| VehiclesCatalog.API | `4040` | MongoDB, Redis, vindecoder.eu API key |
| Media.API | `7037` | PostgreSQL, RabbitMQ, chosen storage backend |
| Users.API | `7126` | PostgreSQL |

Each service needs its own secrets (connection strings, API keys, Azure AD values) via `dotnet user-secrets` — checked-in `appsettings.json` files leave these blank intentionally. RabbitMQ isn't containerized here; run it yourself (e.g. `docker run -d -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=... -e RABBITMQ_DEFAULT_PASS=... rabbitmq:4-management`, with a dedicated user rather than the default `guest`).
