# AutoWise

> Work in progress.

AutoWise helps users track their vehicles — specs, license plates, maintenance history, and photos/documents — behind a single API gateway. A React frontend is planned.

```
AutoWise/
└── Backend/     .NET 10 microservices + YARP API gateway
```

## Architecture

```
Browser (frontend, planned)
  │  Bearer token (Azure AD)
  ▼
AutoWise.YarpApiGateway  (:7080)
  ├── /api/user-vehicles/**     → AutoWise.UserVehicles.API     (:7279)
  ├── /api/vehicles-catalog/**  → AutoWise.VehiclesCatalog.API  (:4040)
  ├── /api/media/**             → AutoWise.Media.API            (:7037)
  └── /api/users/**             → AutoWise.Users.API            (:7126)

UserVehicles ──gRPC──▶ VehiclesCatalog (VIN → specs, Redis-cached both sides)
Media ◀──RabbitMQ (MassTransit outbox/inbox)──▶ UserVehicles  (attachment upload/removal events)
```

- **Auth**: JWT Bearer at the gateway (Keycloak or Azure AD, chosen via `Auth:ActiveProvider`). The gateway validates the token, resolves/creates the matching internal user via a gRPC call to `AutoWise.Users`, caches that mapping in Redis, and forwards requests with an `X-User-Id` header — downstream services never talk to Azure AD directly. Claim names (external id / email / display name) are configurable per provider, since which claim carries what varies by provider and even by account type.
- **Service discovery**: gateway cluster destinations are resolved via `Microsoft.Extensions.ServiceDiscovery` (config-based) instead of hardcoded URLs.
- **CORS**: gateway allows the frontend's dev origin (`Cors:AllowedOrigins`).

## Services

| Service | Stack | Notes |
|---|---|---|
| `YarpApiGateway` | YARP | Auth, CORS, rate limiting, service discovery, routing |
| `UserVehicles` | Clean Architecture, PostgreSQL | Vehicles, events, attachments; list endpoint supports pagination/search/sort |
| `VehiclesCatalog` | Vertical slices (Carter/MediatR), MongoDB | VIN → specs, via external VIN decoder API; exposed over HTTP + gRPC |
| `Media` | Clean Architecture, PostgreSQL | File uploads, content-hash dedup, pluggable storage (local disk / S3 / Azure Blob) |
| `Users` | Clean Architecture, PostgreSQL | Internal user records, resolved from external identity by the gateway over gRPC; `GET /api/users/current-user` returns the signed-in user |
| `CommonUtilities` | Shared libraries | Base entities, exceptions, messaging abstractions, EF interceptors — consumed as prebuilt DLLs, not project refs |

Messaging between Media and UserVehicles uses MassTransit + RabbitMQ with an EF Core transactional outbox/inbox for reliable, idempotent delivery.

## Frontend

Not yet built — a React frontend is planned. The gateway's CORS policy (`Cors:AllowedOrigins`) is kept in place for it.

## Running locally

| Service | HTTPS port | Depends on |
|---|---|---|
| YarpApiGateway | `7080` | UserVehicles, VehiclesCatalog, Media, Users all running |
| UserVehicles.API | `7279` | PostgreSQL, Redis, RabbitMQ, VehiclesCatalog (gRPC) |
| VehiclesCatalog.API | `4040` | MongoDB, Redis, vindecoder.eu API key |
| Media.API | `7037` | PostgreSQL, RabbitMQ, chosen storage backend |
| Users.API | `7126` | PostgreSQL |

Each service needs its own secrets (connection strings, API keys, Azure AD values) via `dotnet user-secrets` — checked-in `appsettings.json` files leave these blank intentionally. RabbitMQ isn't containerized here; run it yourself (e.g. `docker run -d -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=... -e RABBITMQ_DEFAULT_PASS=... rabbitmq:4-management`, with a dedicated user rather than the default `guest`).

## Keycloak (local auth)

`docker compose up -d keycloak` starts Keycloak at http://localhost:8080 (admin console: `admin` / `admin`). On first start it imports [`Keycloak/autowise-realm.json`](Keycloak/autowise-realm.json), which creates:

- realm `autowise` (self-registration enabled)
- public client `autowise-react`: Authorization Code + PKCE (S256), redirect URIs `http://localhost:5173/*` and `http://localhost:3000/*`
- an audience mapper so access tokens carry `aud: autowise-api`
- a test user `testuser` (password is in the realm file)

The import only runs when the realm doesn't exist yet. To re-import after editing the file, delete the realm in the admin console (or `docker compose down` and remove the `keycloak-data` volume), then start Keycloak again.

Gateway settings for Keycloak:

| Key | Local (`dotnet run`, user secrets) | Docker (`docker-compose.yml`) |
|---|---|---|
| `Auth:ActiveProvider` | `Keycloak` | `Keycloak` |
| `Auth:Keycloak:Authority` | `http://localhost:8080/realms/autowise` | `http://localhost:8080/realms/autowise` |
| `Auth:Keycloak:Audience` | `autowise-api` | `autowise-api` |
| `Auth:Keycloak:MetadataAddress` | — | `http://keycloak:8080/realms/autowise/.well-known/openid-configuration` |

Keycloak runs with `KC_HOSTNAME=http://localhost:8080` and `KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true`, so tokens always say `iss: http://localhost:8080/...` (what the browser sees) while the gateway container fetches keys from `keycloak:8080`. Plain-HTTP metadata is only allowed when the authority/metadata URL is `http://`.

Frontend OIDC settings: authority `http://localhost:8080/realms/autowise`, client id `autowise-react`, scope `openid profile email`.
