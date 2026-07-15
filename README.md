# AutoWise — Backend Microservices

> Work in progress. This document describes the current implementation of the backend as of 2026-07-15.

AutoWise's backend is a small set of independently-deployable .NET 10 services fronted by a YARP API gateway. There is no top-level solution file — each service has its own `.slnx` (VS's newer XML solution format).

```
Backend/
├── AutoWise.YarpApiGateway/     API gateway (YARP reverse proxy)
├── AutoWise.UserVehicles/       Clean Architecture service, PostgreSQL
├── AutoWise.VehiclesCatalog/    Vertical-slice service, MongoDB
└── AutoWise.CommonUtilities/    Shared class libraries (consumed as prebuilt DLLs)
```

## Architecture at a glance

```
Client
  │
  ▼
AutoWise.YarpApiGateway  (:5102 / :7080)
  ├── /api/user-vehicles/**      ──HTTP──▶  AutoWise.UserVehicles.API      (:5077 / :7279)
  └── /api/vehicles-catalog/**   ──HTTP──▶  AutoWise.VehiclesCatalog.API   (:4000 / :4040)

AutoWise.UserVehicles.API ──gRPC──▶ AutoWise.VehiclesCatalog (VehicleSpecificationsProtoService, :4040)
```

- **No message broker** — no RabbitMQ/Kafka/MassTransit. The only cross-service call is a synchronous gRPC request from UserVehicles to VehiclesCatalog.
- **No service discovery** — every downstream address is a static URL in `appsettings.json`.
- **No authentication yet** — no JWT/Identity/OAuth anywhere. UserVehicles stands in a hardcoded fake user id (`54cf3f84-ef0b-47e7-9480-a6e5d0be9052`) wherever an authenticated user would normally be used (controllers, audit interceptor).

---

## AutoWise.YarpApiGateway

Reverse proxy built on `Yarp.ReverseProxy`, and the single entry point for clients.

- **Program.cs**: `AddReverseProxy().LoadFromConfig(...)` + a fixed-window rate limiter (`"fixed"` policy: 5 requests / 10s) applied to every route.
- **Routing** (`appsettings.Development.json`):

  | Route | Match | Cluster destination |
  |---|---|---|
  | `user-vehicles-route` | `/api/user-vehicles/{**catch-all}` | `http://localhost:5077/` |
  | `vehicles-catalog-route` | `/api/vehicles-catalog/{**catch-all}` | `http://localhost:4000/` |

- Dev URLs: `http://localhost:5102`, `https://localhost:7080`.

---

## AutoWise.UserVehicles

Manages a user's vehicles and their maintenance/history events. Built with classic **Clean Architecture** layering:

```
AutoWise.UserVehicles.API             — Controllers, Program.cs, DI wiring
AutoWise.UserVehicles.Application     — services, DTOs, interfaces (plain services, no MediatR)
AutoWise.UserVehicles.Domain          — DDD entities (UserVehicle, UserVehicleEvent)
AutoWise.UserVehicles.Infrastructure  — EF Core DbContext, migrations, gRPC client
AutoWise.UserVehicles.Tests           — xUnit
```

**Stack**: ASP.NET Core (.NET 10), EF Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` (PostgreSQL), `Grpc.AspNetCore` (gRPC client), `Microsoft.Extensions.Caching.StackExchangeRedis` (configured, not yet used), `Scalar.AspNetCore` + `Microsoft.AspNetCore.OpenApi` for API docs.

### Domain

- `UserVehicle` (`ModifiedCreatedAuditBaseEntity`) — `Vin` (17-char validated), `Make`, `Model`, `Year`, `LicensePlateNumber`, `UserId`, owns a collection of `UserVehicleEvent`. Created via a `Create(...)` factory; mutations go through invariant-checked methods (`AddEvent`/`UpdateEvent`/`RemoveEvent`, etc.) rather than public setters.
- `UserVehicleEvent` (`ModifiedCreatedAuditBaseEntity`) — `Name`, `Description`, `EventDate`, back-reference to its `UserVehicle`.

### API endpoints (`[Route("api")]`)

**`UserVehicleController`**
| Verb | Route | Purpose |
|---|---|---|
| POST | `api/user-vehicles` | Create a vehicle (enriches Make/Model/Year via gRPC lookup by VIN) |
| GET | `api/user-vehicles/{id:guid}` | Get a vehicle |
| PUT | `api/user-vehicles/{id:guid}` | Update a vehicle's license plate |
| DELETE | `api/user-vehicles/{id:guid}` | Delete a vehicle |

**`UserVehicleEventController`**
| Verb | Route | Purpose |
|---|---|---|
| POST | `api/user-vehicles/{vehicleId:guid}/events` | Add an event |
| GET | `api/user-vehicles/{vehicleId:guid}/events/{eventId:guid}` | Get an event |
| PUT | `api/user-vehicles/{vehicleId:guid}/events/{eventId:guid}` | Update an event |
| DELETE | `api/user-vehicles/{vehicleId:guid}/events/{eventId:guid}` | Delete an event |

### Persistence

- `UserVehiclesDbContext` — PostgreSQL via EF Core, schema applied through the shared `ConfigureDatabaseWithSchema` extension.
- Connection string: `ConnectionStrings:PostgreSQL` (blank in source control — supplied via user secrets/environment).
- Migrations auto-applied on startup (`app.ApplyMigrationsAsync()`); current migration: `InitialCreate`.
- Audit fields (`CreatedOn`, `LastModifiedOn`, etc.) are populated automatically by the shared `AuditableEntityInterceptor`.

### gRPC client (→ VehiclesCatalog)

`VehicleSpecificationsGrpcClient` implements `IVehicleSpecificationsService`, calling `VehicleSpecificationsProtoService.GetVehicleSpecificationsAsync` against `GrpcSettings:VehiclesCatalogUrl` (dev: `https://localhost:4040`). Used by `UserVehiclesService.CreateAsync` to look up Make/Model/Year by VIN before persisting a new vehicle. The `.proto` contract is referenced directly from the VehiclesCatalog project (`GrpcServices="Client"`).

### Tests

xUnit + FluentAssertions + NSubstitute + EFCore.InMemory — domain invariant tests (`UserVehicleTests`, `UserVehicleEventTests`) and application-service tests (`UserVehiclesServiceTests`, `UserVehicleEventServiceTests`) against an in-memory `UserVehiclesDbContext`.

---

## AutoWise.VehiclesCatalog

Vehicle specification lookup/catalog: decodes a VIN via an external paid API (vindecoder.eu) or serves previously-fetched specs, exposed over both HTTP and gRPC.

Single project (`AutoWise.VehiclesCatalog.API`), organized as **vertical slices** rather than layered controllers, using **Carter** (minimal-API endpoint modules) + **MediatR**:

```
Features/VehicleSpecifications/
  AddVehicleSpecifications/     (Dto, Endpoint, Handler, Validator)
  DeleteVehicleSpecifications/  (Dto, Endpoint, Handler, Validator)
  GetVehicleSpecifications/     (Dto, Endpoint, Handler)
  ImportVehicleSpecifications/  (Dto, Endpoint, Handler, Validator)
Grpc/Protos/vehiclespecifications.proto
Grpc/Services/VehicleSpecificationsService.cs
Infrastructure/MongoDbService.cs
Models/Vehicle.cs, Models/VehicleSpecification.cs
Configurations/GetVehicleSpecificationsConfig.cs
```

**Stack**: ASP.NET Core (.NET 10), Carter, MediatR + `ValidationBehaviour<,>` pipeline (validates requests with FluentValidation and throws `BadRequestWithMultipleFailuresException` on failure), `MongoDB.Driver` (primary datastore), `Grpc.AspNetCore` (gRPC **server**), `StackExchangeRedis` cache (configured, not clearly consumed yet), `Scalar.AspNetCore` + OpenAPI.

### API endpoints (Carter modules)

| Verb | Route | Purpose |
|---|---|---|
| GET | `api/vehicles-catalog/specifications/{vin}` | Get specs for a VIN — imports from the external API on first request, then serves from Mongo |
| POST | `api/vehicles-catalog/specifications` | Manually add specs for a VIN (no external API call) |
| DELETE | `api/vehicles-catalog/specifications/{vin}` | Delete specs for a VIN |
| POST | `api/vehicles-catalog/specifications/import` | Force a fresh import from the external VIN decoder API |

### gRPC server

`VehicleSpecificationsProtoService` (`vehiclespecifications.proto`) exposes a single RPC:

```
GetVehicleSpecifications(GetVehicleSpecificationsRequest{ vin })
  → GetVehicleSpecificationsResponseList{ repeated { label, value } }
```

Implementation checks MongoDB first; on a miss it fetches from the VIN decoder API, persists the result, then returns it. This is the RPC consumed by `AutoWise.UserVehicles`.

### Persistence

- MongoDB via `MongoDbService` (wraps `MongoClient`/`IMongoDatabase`), collection `vehicles`.
- Connection string: `ConnectionStrings:MongoDb` (dev: `mongodb://localhost:27017/AutoWise`).
- No EF Core / migrations (document store).

### External dependency: VIN decoder API

`api.vindecoder.eu` — credentials in `appsettings.json` under `VinDecoderSpecifications` (blank in source, via user secrets). The request URL/signature is built in `GetVehicleSpecificationsConfig` using the SHA1 control-sum scheme required by that API.

### Known rough edges

- References `Nancy` (an old, unrelated web framework) — appears unused; candidate for removal.
- Uses the legacy `System.Web.Script.Serialization.JavaScriptSerializer` in `Utils/ImportVehicleSpecificationsUtils.cs` to parse the VIN decoder response.
- No test project yet.

---

## AutoWise.CommonUtilities

Shared class libraries, not a runnable service:

| Project | Contents |
|---|---|
| `BaseModels` | `IdBaseEntity`, `CreatedAuditBaseEntity`, `ModifiedCreatedAuditBaseEntity` (+ interfaces); `QueryOptions`, `PagedQueryResponse`, sorting types |
| `Exceptions` | `BadRequestException`, `NotFoundException`, `InternalServerException`, `BadRequestWithMultipleFailuresException`; `CustomExceptionHandler` (`IExceptionHandler`) mapping exceptions → HTTP problem responses |
| `ExtensionMethods` | `LoggerExtensionMethods`, `NullOrEmptyValuesExtensionMethods` |
| `Mediator` | CQRS abstractions (`ICommand`, `ICommandHandler`, `IQuery`, `IQueryHandler`) and the `ValidationBehaviour<,>` MediatR pipeline behavior |
| `Repository.Abstractions` | `IGenericRepository<TEntity>` |
| `Repository.PostgreSQL` | `GenericRepository<TEntity>`, `ConfigureDatabaseWithSchema`, `AuditableEntityInterceptor`, base EF entity configurations |

**Consumption pattern (rough edge to be aware of)**: these libraries are **not** referenced as NuGet packages or ProjectReferences. Each consuming service keeps a local `Libraries/` folder with prebuilt `.dll` copies, referenced via `<Reference><HintPath>`. Any change to CommonUtilities has to be rebuilt and manually copied into every consumer — a good candidate for converting to a private NuGet feed or direct project references once the services stabilize.

---

## Running locally (development profiles)

| Service | HTTP | HTTPS | Depends on |
|---|---|---|---|
| YarpApiGateway | `:5102` | `:7080` | UserVehicles + VehiclesCatalog running |
| UserVehicles.API | `:5077` | `:7279` | PostgreSQL, Redis, VehiclesCatalog (gRPC) |
| VehiclesCatalog.API | `:4000` | `:4040` | MongoDB, Redis, vindecoder.eu API key |

Each service needs its own secrets configured (PostgreSQL connection string, VIN decoder API credentials) via `dotnet user-secrets` in Development — the checked-in `appsettings.json` files leave these blank intentionally.
