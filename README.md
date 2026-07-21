# AutoWise — Backend Microservices

> Work in progress. This document describes the current implementation of the backend as of 2026-07-21.

AutoWise's backend is a small set of independently-deployable .NET 10 services fronted by a YARP API gateway. There is no top-level solution file — each service has its own `.slnx` (VS's newer XML solution format).

```
Backend/
├── AutoWise.YarpApiGateway/     API gateway (YARP reverse proxy)
├── AutoWise.UserVehicles/       Clean Architecture service, PostgreSQL
├── AutoWise.VehiclesCatalog/    Vertical-slice service, MongoDB
├── AutoWise.Media/              Clean Architecture service, PostgreSQL, pluggable file storage
└── AutoWise.CommonUtilities/    Shared class libraries (consumed as prebuilt DLLs)
```

## Architecture at a glance

```
Client
  │
  ▼
AutoWise.YarpApiGateway  (:5102 / :7080)
  ├── /api/user-vehicles/**      ──HTTP──▶  AutoWise.UserVehicles.API      (:5077 / :7279)
  ├── /api/vehicles-catalog/**   ──HTTP──▶  AutoWise.VehiclesCatalog.API   (:4000 / :4040)
  └── /api/media/**              ──HTTP──▶  AutoWise.Media.API             (:5248 / :7037)

AutoWise.UserVehicles.API ──gRPC──▶ AutoWise.VehiclesCatalog (VehicleSpecificationsProtoService, :4040)

                    ┌──────────────────────────┐
  MediaAttachmentUploaded (publish)             │
AutoWise.Media.API ────────────────────────▶ RabbitMQ ────────────────────▶ AutoWise.UserVehicles.API
AutoWise.Media.API ◀──────────────────────── RabbitMQ ◀──────────────────── AutoWise.UserVehicles.API
                    MediaAttachmentRemoved (publish)                    │
                    └──────────────────────────────────────────────────┘
```

- **Message broker: RabbitMQ, via MassTransit** — the first event-driven flow in the project, layered on top of the existing synchronous gRPC call. Media publishes `MediaAttachmentUploaded` after a successful upload; UserVehicles consumes it to record a local `UserVehicleAttachment`. UserVehicles publishes `MediaAttachmentRemoved` when an attachment is deleted; Media consumes it to run its existing reference-counted file deletion. See [Messaging](#messaging-rabbitmq--masstransit) below.
- **No service discovery** — every downstream address is a static URL in `appsettings.json`.
- **No authentication yet** — no JWT/Identity/OAuth anywhere. UserVehicles stands in a hardcoded fake user id (`54cf3f84-ef0b-47e7-9480-a6e5d0be9052`) wherever an authenticated user would normally be used (controllers, audit interceptor).

---

## Messaging (RabbitMQ + MassTransit)

The messaging stack is split into three shared libraries under `AutoWise.CommonUtilities`, following the same ports-and-adapters idea used for Media's pluggable storage — so the broker technology (MassTransit today) can be swapped later without touching any service's Application-layer business logic:

| Library | Contents |
|---|---|
| `Messaging.Contracts` | The event records themselves (e.g. `MediaAttachmentUploaded`, `MediaAttachmentRemoved`, under `Contracts.Media`). Deliberately a **shared, neutral** library rather than per-service (e.g. not `AutoWise.Media.Contracts`) — a consuming service should never need a compile-time dependency on the *publishing* service itself, only on the shared event shape. |
| `Messaging.Abstractions` | `IEventPublisher.PublishAsync<TEvent>(...)` — the only messaging type Application-layer code depends on. |
| `Messaging.MassTransit` | `MassTransitEventPublisher` (implements `IEventPublisher`), and `AddMassTransitMessaging<TDbContext>(configuration, OutboxDatabaseProvider, configureConsumers?)` — the DI extension every service calls, generic over that service's own `DbContext` so the same method serves pure publishers, pure consumers, and future services that do both. |

### Reliability: EF Core transactional outbox/inbox

Every service using `AddMassTransitMessaging<TDbContext>` gets MassTransit's EF Core outbox wired in two parts:
- **Bus outbox** (`o.UseBusOutbox()`) — publishing from regular Application-layer code (not from inside a consumer) stages the message into the *same* `DbContext` transaction as whatever business data is being saved, instead of sending to RabbitMQ immediately. A background delivery service (auto-registered) then actually publishes it, with retries if the broker is briefly unreachable. This solves the classic dual-write problem: without it, "save to Postgres" and "publish to RabbitMQ" are two independent operations that can succeed/fail independently.
- **Consumer inbox** (`x.AddConfigureEndpointsCallback((context, name, cfg) => cfg.UseEntityFrameworkOutbox<TDbContext>(context))`) — applied to every auto-configured receive endpoint, giving idempotent consumption (tracked via `InboxState`) so a redelivered message (brokers are at-least-once) doesn't get processed twice.

Both require `modelBuilder.AddMassTransitInboxOutboxEntities()` in the service's `DbContext.OnModelCreating` (adds `InboxState`, `OutboxMessage`, `OutboxState` tables) — this is unconditional even for publish-only services, since the outbox delivery service's housekeeping touches `InboxState` regardless of whether anything consumes.

**Critical wiring detail**: `IEventPublisher`/consumers depend on `IMediaDbContext`/`IUserVehiclesDbContext`, while the outbox is wired to the *concrete* `DbContext` type. If the interface is registered as `services.AddScoped<IUserVehiclesDbContext, UserVehiclesDbContext>()`, DI resolves **two separate instances** per scope — the outbox stages messages on one instance while Application code calls `SaveChangesAsync` on the other, so staged messages are silently never persisted (no exception, no row, nothing). Both services register the interface as `services.AddScoped<IUserVehiclesDbContext>(sp => sp.GetRequiredService<UserVehiclesDbContext>())` (same for Media) specifically to avoid this.

**Ordering matters**: `Publish()` only stages a message if called *before* the `SaveChangesAsync()` it needs to be bundled with — both `MediaAttachmentService.UploadAsync` and `UserVehicleAttachmentService.RemoveAttachmentAsync` publish first, then save.

### Connection

`ConnectionStrings:RabbitMQ` (e.g. `amqp://user:pass@localhost:5672/`), blank in source, via user secrets — same convention as the other connection strings.

---

## AutoWise.YarpApiGateway

Reverse proxy built on `Yarp.ReverseProxy`, and the single entry point for clients.

- **Program.cs**: `AddReverseProxy().LoadFromConfig(...)` + a fixed-window rate limiter (`"fixed"` policy: 5 requests / 10s) applied to every route.
- **Routing** (`appsettings.Development.json`):

  | Route | Match | Cluster destination |
  |---|---|---|
  | `user-vehicles-route` | `/api/user-vehicles/{**catch-all}` | `http://localhost:5077/` |
  | `vehicles-catalog-route` | `/api/vehicles-catalog/{**catch-all}` | `http://localhost:4000/` |
  | `media-route` | `/api/media/{**catch-all}` | `http://localhost:5248/` |

- Dev URLs: `http://localhost:5102`, `https://localhost:7080`.

---

## AutoWise.UserVehicles

Manages a user's vehicles, their maintenance/history events, and the media attachments linked to them. Built with classic **Clean Architecture** layering:

```
AutoWise.UserVehicles.API             — Controllers, Program.cs, DI wiring
AutoWise.UserVehicles.Application     — services, DTOs, interfaces (plain services, no MediatR)
AutoWise.UserVehicles.Domain          — DDD entities (UserVehicle, UserVehicleEvent, UserVehicleAttachment)
AutoWise.UserVehicles.Infrastructure  — EF Core DbContext, migrations, gRPC client, MassTransit consumer
AutoWise.UserVehicles.Tests           — xUnit
```

**Stack**: ASP.NET Core (.NET 10), EF Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` (PostgreSQL), `Grpc.AspNetCore` (gRPC client), `MassTransit` + `MassTransit.RabbitMQ` + `MassTransit.EntityFrameworkCore`, `Microsoft.Extensions.Caching.StackExchangeRedis` (configured, not yet used), `Scalar.AspNetCore` + `Microsoft.AspNetCore.OpenApi` for API docs.

### Domain

- `UserVehicle` (`ModifiedCreatedAuditBaseEntity`) — `Vin` (17-char validated), `Make`, `Model`, `Year`, `LicensePlateNumber`, `UserId`, owns collections of `UserVehicleEvent` and `UserVehicleAttachment`. Created via a `Create(...)` factory; mutations go through invariant-checked methods (`AddEvent`/`UpdateEvent`/`RemoveEvent`, `AddAttachment`/`RemoveAttachment`) rather than public setters.
- `UserVehicleEvent` (`ModifiedCreatedAuditBaseEntity`) — `Name`, `Description`, `EventDate`, back-reference to its `UserVehicle`.
- `UserVehicleAttachment` (`CreatedAuditBaseEntity` — immutable after creation) — `MediaAttachmentId` (unique-indexed, references the attachment in `AutoWise.Media`), `OriginalFileName`, `ContentType`, `SizeInBytes`, back-reference to its `UserVehicle`. A true aggregate child (like `UserVehicleEvent`), not shared/deduplicated the way Media's own `MediaFile` is — each row belongs to exactly one vehicle.

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

**`UserVehicleAttachmentController`**
| Verb | Route | Purpose |
|---|---|---|
| DELETE | `api/user-vehicles/{vehicleId:guid}/attachments/{attachmentId:guid}` | Remove an attachment (publishes `MediaAttachmentRemoved`) |

Note: there's no `POST`/`GET` here — attachments are only ever *created* via the `MediaAttachmentUploaded` consumer (below), not through this API directly.

### Persistence

- `UserVehiclesDbContext` — PostgreSQL via EF Core, schema applied through the shared `ConfigureDatabaseWithSchema` extension, plus `AddMassTransitInboxOutboxEntities()` for the messaging tables.
- Connection string: `ConnectionStrings:PostgreSQL` (blank in source control — supplied via user secrets/environment).
- Migrations auto-applied on startup (`app.ApplyMigrationsAsync()`); migrations: `InitialCreate`, `VehicleAttachmentAndInboxOutboxTables`.
- Audit fields (`CreatedOn`, `LastModifiedOn`, etc.) are populated automatically by the shared `AuditableEntityInterceptor`.

### gRPC client (→ VehiclesCatalog)

`VehicleSpecificationsGrpcClient` implements `IVehicleSpecificationsService`, calling `VehicleSpecificationsProtoService.GetVehicleSpecificationsAsync` against `GrpcSettings:VehiclesCatalogUrl` (dev: `https://localhost:4040`). Used by `UserVehiclesService.CreateAsync` to look up Make/Model/Year by VIN before persisting a new vehicle. The `.proto` contract is referenced directly from the VehiclesCatalog project (`GrpcServices="Client"`).

### Messaging

- **Consumes** `MediaAttachmentUploaded` (`MediaAttachmentUploadedConsumer`) — ignores messages whose `ParentType` isn't `"UserVehicle"` (normal/expected, since this is a shared event other future parent types may use); if it does match but the referenced vehicle can't be found, throws (an anomaly, surfaced via retry → dead-letter rather than silently discarded). On a match, calls `UserVehicle.AddAttachment(...)` and saves.
- **Publishes** `MediaAttachmentRemoved` (from `UserVehicleAttachmentService.RemoveAttachmentAsync`) after removing the local `UserVehicleAttachment` — named after the `MediaAttachment` concept, not `UserVehicleAttachment`, so any future service that stops referencing a media attachment can publish the same event and Media's consumer needs zero changes regardless of who's calling it.

### Tests

xUnit + FluentAssertions + NSubstitute + EFCore.InMemory — domain invariant tests (`UserVehicleTests`, `UserVehicleEventTests`, `UserVehicleAttachmentTests`), application-service tests (`UserVehiclesServiceTests`, `UserVehicleEventServiceTests`, `UserVehicleAttachmentServiceTests`), and consumer tests (`MediaAttachmentUploadedConsumerTests`) against an in-memory `UserVehiclesDbContext` and a substituted `IEventPublisher`.

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

## AutoWise.Media

Handles file uploads (images, videos, PDFs — allowed types configurable) and associates them with a parent entity elsewhere in the system (e.g. a `UserVehicle`). Built with the same **Clean Architecture** layering as UserVehicles, chosen specifically so the storage backend can be swapped without touching Application/Domain:

```
AutoWise.Media.API             — Controllers, Program.cs, DI wiring
AutoWise.Media.Application      — MediaAttachmentService, DTOs, storage/DB/messaging port interfaces
AutoWise.Media.Domain           — MediaFile, MediaAttachment, MediaStorageProvider enum
AutoWise.Media.Infrastructure   — EF Core DbContext, migrations, storage provider implementations, MassTransit consumer
AutoWise.Media.Tests            — xUnit
```

**Stack**: ASP.NET Core (.NET 10), EF Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL` (PostgreSQL), `MassTransit` + `MassTransit.RabbitMQ` + `MassTransit.EntityFrameworkCore`, `AWSSDK.S3`, `Azure.Storage.Blobs`, `Scalar.AspNetCore` + OpenAPI.

### Domain

- `MediaFile` (`CreatedAuditBaseEntity`) — the content-addressed, deduplicated file record: `ContentHash` (SHA-256 hex, unique), `ContentType`, `FileExtension`, `SizeInBytes`, `StorageProvider` (which backend holds the bytes), `StorageKey`. Generates its own `Id` client-side (`Guid.NewGuid()`, unlike every other entity in the codebase) specifically so a new `MediaFile` and its first `MediaAttachment` can be persisted together in one `SaveChangesAsync` call.
- `MediaAttachment` (`CreatedAuditBaseEntity`) — one upload *request*: `MediaFileId`, `ParentType` (string, e.g. `"UserVehicle"` — deliberately not an enum, so Media doesn't need to know about every consumer's domain types) + `ParentEntityId`, `OriginalFileName`. Many `MediaAttachment`s can point at the same `MediaFile`.

### Deduplication

Uploads are hashed (SHA-256) before being written to storage. If a `MediaFile` with that hash already exists, the upload skips the storage write entirely and just creates a new `MediaAttachment` pointing at the existing file — so ten uploads of the same bytes under different names create one `MediaFile` and ten `MediaAttachment`s. Deleting an attachment is reference-counted: the physical file and its `MediaFile` row are only removed once the last referencing `MediaAttachment` is gone.

### API endpoints (`[Route("api")]`)

**`MediaAttachmentController`**
| Verb | Route | Purpose |
|---|---|---|
| POST | `api/media` | Upload a file (`multipart/form-data`: file + `ParentType` + `ParentEntityId`) |
| GET | `api/media/{id:guid}` | Get attachment metadata |
| GET | `api/media/{id:guid}/content` | Download the actual file bytes (separate from metadata, so listing attachments doesn't require pulling every file's content) |
| DELETE | `api/media/{id:guid}` | Delete an attachment (reference-counted) |

### Persistence

- `MediaDbContext` — PostgreSQL via EF Core, same `ConfigureDatabaseWithSchema` pattern as UserVehicles, plus `AddMassTransitInboxOutboxEntities()`.
- Connection string: `ConnectionStrings:PostgreSQL` (blank in source control — via user secrets).
- Migrations auto-applied on startup; migrations: `InitialCreate`, `InboxOutboxTables`.

### Pluggable storage

- `IFileStorageProvider` (Application port) — `SaveAsync`/`OpenReadAsync`/`DeleteAsync`/`ExistsAsync` by `storageKey`.
- `IFileStorageProviderResolver` — maps a `MediaStorageProvider` enum value to the matching concrete provider (`IEnumerable<IFileStorageProvider>` resolved via DI); also exposes `ResolveActiveProvider()`, which reads `Storage:ActiveProvider` from config so Application never needs to know about Infrastructure-level settings.
- `LocalDiskStorageProvider` — writes/reads relative to `Storage:LocalDisk:RootPath`.
- `AmazonS3StorageProvider` — implemented via `AWSSDK.S3`; credentials/bucket/region under `Storage:AmazonS3`.
- `AzureBlobStorageProvider` — implemented via `Azure.Storage.Blobs`; connection string + container name under `Storage:AzureBlob`.
- All three providers are fully implemented — none are stubs anymore.
- Switching `Storage:ActiveProvider` only changes where *new* uploads go — the resolver still looks up whichever provider a given `MediaFile.StorageProvider` says it was actually saved under, so old files stay readable after a switch.

### Messaging

- **Publishes** `MediaAttachmentUploaded` (`MediaAttachmentId`, `ParentType`, `ParentEntityId`, `OriginalFileName`, `ContentType`, `SizeInBytes`) from `MediaAttachmentService.UploadAsync`, after the attachment is created but before `SaveChangesAsync` (required ordering for the outbox to bundle both into one transaction).
- **Consumes** `MediaAttachmentRemoved` (`MediaAttachmentRemovedConsumer`) — a direct pass-through into the existing `MediaAttachmentService.DeleteAsync`, reusing the reference-counted deletion logic; no new deletion logic needed for this flow.

### Tests

xUnit + FluentAssertions + NSubstitute + EFCore.InMemory — domain invariant tests (`MediaFileTests`, `MediaAttachmentTests`), application-service tests (`MediaAttachmentServiceTests`, covering the dedup path, event publishing, reference-counted delete, and not-found cases), and consumer tests (`MediaAttachmentRemovedConsumerTests`) against an in-memory `MediaDbContext`, substituted storage provider/resolver, and substituted `IEventPublisher`.

### Known gaps

- No Dockerfile yet (consistent with UserVehicles and the gateway; only VehiclesCatalog has one currently).

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
| `Messaging.Contracts` | Shared event records (e.g. `MediaAttachmentUploaded`, `MediaAttachmentRemoved`) |
| `Messaging.Abstractions` | `IEventPublisher` — the only messaging type Application-layer code depends on |
| `Messaging.MassTransit` | `MassTransitEventPublisher`, `AddMassTransitMessaging<TDbContext>`, `AddMassTransitInboxOutboxEntities()` — see [Messaging](#messaging-rabbitmq--masstransit) above |

**Consumption pattern (rough edge to be aware of)**: these libraries are **not** referenced as NuGet packages or ProjectReferences. Each consuming service keeps a local `Libraries/` folder with prebuilt `.dll` copies, referenced via `<Reference><HintPath>`. Any change to CommonUtilities has to be rebuilt and manually copied into every consumer — a good candidate for converting to a private NuGet feed or direct project references once the services stabilize. One consequence already seen in practice: `AutoWise.UserVehicles.Application.csproj`'s `HintPath`s for a couple of these DLLs point at *other services'* `Libraries` folders rather than its own — works today only because the files happen to be identical copies, worth straightening out.

---

## Running locally (development profiles)

| Service | HTTP | HTTPS | Depends on |
|---|---|---|---|
| YarpApiGateway | `:5102` | `:7080` | UserVehicles + VehiclesCatalog + Media running |
| UserVehicles.API | `:5077` | `:7279` | PostgreSQL, Redis, RabbitMQ, VehiclesCatalog (gRPC) |
| VehiclesCatalog.API | `:4000` | `:4040` | MongoDB, Redis, vindecoder.eu API key |
| Media.API | `:5248` | `:7037` | PostgreSQL, RabbitMQ, local disk / S3 / Azure Blob (per `Storage:ActiveProvider`) |

Each service needs its own secrets configured (PostgreSQL connection string, RabbitMQ connection string, VIN decoder API credentials, cloud storage credentials) via `dotnet user-secrets` in Development — the checked-in `appsettings.json` files leave these blank intentionally.

RabbitMQ isn't containerized as part of this repo — run it yourself locally (e.g. `docker run -d -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=... -e RABBITMQ_DEFAULT_PASS=... rabbitmq:4-management`, using a dedicated user rather than the default `guest`, which only accepts loopback connections and can behave inconsistently through Docker Desktop's port forwarding).
