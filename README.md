# BusinessIQ

**AI Business Intelligence & Advisory Platform**

BusinessIQ combines the planned areas of **Business Management + Financial Intelligence + Multi-Business Dashboard + AI Business Adviser**. It targets organizations with businesses across industries; hospitality is one optional specialization.

Product direction follows the supplied functional roadmap, version 1.0, September 2026. See [ROADMAP.md](docs/ROADMAP.md) for future releases and [ARCHITECTURE.md](docs/ARCHITECTURE.md) for implementation boundaries.

## Implemented foundation

- A Business management slice: create, read, update, paginated search and archive.
- Controller -> Service -> Repository -> BusinessDbContext, which inherits FrameworkDbContext.
- Shared Framework base controllers, services, repositories, entities, DTOs and response/pagination contracts.
- AutoMapper write mappings and read projections, with server-controlled ownership.
- Organization isolation and production owner authorization for Business management.
- Document ingestion and evidence-backed Q&A using Framework AI services and Local/Azure adapters.
- Cross-industry profiles, Swagger, integration and architecture checks.

Organization onboarding, locations, persisted memberships, financial transactions/calculations, dashboards and structured-data adviser tools remain planned. This is an application foundation, not a completed financial platform.

## Solution layout

```text
BusinessIQ.sln
src/
  BusinessIQ.Api/              Controllers, identity/request adapters, composition
  BusinessIQ.Application/     Services/interfaces, mappings, document profiles
  BusinessIQ.Domain/          Business entities
  BusinessIQ.Contracts/       Business DTOs
  BusinessIQ.Infrastructure/  Repository implementations and EF model
tests/
  BusinessIQ.IntegrationTests/
docs/
  ARCHITECTURE.md
  ROADMAP.md
```

The local checkout may still be named `GastroIQ.App`. The solution/project names are BusinessIQ. Keep a sibling `Framework` checkout; cross-repository references resolve to `../Framework/src` from this root. Both repositories' changes are needed to build. For independent distribution, publish versioned shared packages later.

## Run locally

Requires .NET 9 SDK and the sibling Framework checkout:

```powershell
dotnet restore BusinessIQ.sln
dotnet test BusinessIQ.sln
dotnet run --project src/BusinessIQ.Api --launch-profile BusinessIQ
```

Open **BusinessIQ.sln** in Visual Studio and set **BusinessIQ.Api** as startup project. Swagger is at `http://localhost:5180/swagger`. The [HTTP examples](BusinessIQ.http) cover business CRUD and documents.

Development uses in-memory business storage and Local AI by default. Both reset on restart. No database/cloud account is needed. Development skips production role checks and accepts a selected organization header for business requests; use it only on your development machine.

## Framework reuse

| Library | Used for |
| --- | --- |
| `Framework.Web` | Inherited `BaseControllerCRUD` and `BaseControllerRO` methods |
| `Framework.Application` | `BaseService`, service/repository interfaces, shared registration and knowledge workflows |
| `Framework.Infrastructure` | `BaseRepository<TEntity, TId, TContext>` with BusinessDbContext |
| `Framework.Domain` | `BaseEntity`, `IEntity`, domain errors |
| `Framework.Contracts` | `BaseDto`, `IEntityContract`, common results, entity IDs, pagination and knowledge DTOs |
| `BuildingBlocks.AI.Core/Local/Azure` | AI ports, models and implementations |

BusinessIQ does not reference the runnable Framework.Api. The reusable controllers have been extracted into Framework.Web. It also does not call the stock `AddInfrastructure()` because that wires FrameworkDbContext and AI together; its own composition selects the application context and existing shared adapters.

## Business API

| Method | Endpoint | Behavior |
| --- | --- | --- |
| POST | `/api/v1/businesses` | Create from `name` and `industry`; return shared entity-ID result |
| GET | `/api/v1/businesses/{id}` | Detail DTO, or 404 outside the organization/after archive |
| GET | `/api/v1/businesses/filtered-search` | Shared pagination and projected grid DTOs |
| PUT | `/api/v1/businesses` | Update from `id`, `name`, `industry` |
| DELETE | `/api/v1/businesses/{id}` | Archive; retained row disappears from normal queries |

Search accepts `PageNumber`, `PageSize`, `SortBy`, and `SortDirection`. Page size defaults to 20 and has a maximum of 500. Use `SortBy=Name` for explicit ordering.

Successful results use `CommonResult<T>`. Invalid input returns Problem Details with 400; inaccessible/missing business IDs return 404. Organization selection failures return 403.

Example in Development:

```powershell
$headers = @{ 'X-Organization-Id' = '11111111-1111-1111-1111-111111111111' }
$body = @{ name = 'North Construction'; industry = 'Construction' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:5180/api/v1/businesses' -Headers $headers -ContentType 'application/json' -Body $body
```

Outside Development, Business management requires an authenticated `role=owner` and valid `tenant_id` membership. A single membership selects the organization automatically; multiple memberships require `X-Organization-Id`, which must match an authorized claim. The role must be issued for organization-wide administration. Business/location-restricted manager access is not implemented for these endpoints.

## Document API

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/v1/ai/document-profiles` | List profiles |
| POST | `/api/v1/knowledge/documents` | Ingest text |
| POST | `/api/v1/ai/answers` | Answer with retrieved evidence/citations |
| GET | `/health/live` | Process liveness; no database/Azure connectivity check |

Profiles are `business-operations`, `staff-onboarding`, `financial-documents`, `business-planning`, and the retained optional `restaurant-operations`. Financial/planning profiles add human-review notices; they do not perform authoritative calculations or create approval workflows.

Knowledge `tenantId` corresponds to an organization. Optional `scopeId` is an exact document access boundary, not the complete Business/Location hierarchy. A scoped query sees organization-wide and matching-scope documents; an unscoped query sees organization-wide documents only.

Production knowledge requests require matching `tenant_id` and, when scoped, `scope_id` claims. Ingestion also requires `role=manager`; an owner-only token does not implicitly acquire that separate permission. Identity provisioning and invitations are still future work.

## Configuration and deployment

Outside Development set `Authentication__Authority` and `Authentication__Audience`. The Local AI provider is rejected there; configure `AI__Provider=Azure` and the five settings:

- `AI__Azure__OpenAiEndpoint`
- `AI__Azure__ChatDeployment`
- `AI__Azure__EmbeddingDeployment`
- `AI__Azure__SearchEndpoint`
- `AI__Azure__SearchIndex`

Business persistence supports `Database__Provider=PostgreSql` or `SqlServer` with `ConnectionStrings__DefaultConnection`. Development defaults to InMemory, while production defaults to PostgreSql. InMemory is rejected outside Development. Relational use requires schema/migration provisioning, which is not implemented yet; startup does not create a database.

See [Framework infrastructure](../Framework/infrastructure/README.md) for shared Azure configuration/search schema. Use separate BusinessIQ resources and deployment state. Framework's Dockerfile packages Framework.Api, so a BusinessIQ image/deployment is still needed. Terminate HTTPS at the hosting ingress.

## Validation and limitations

Tests cover inherited CRUD, AutoMapper validation, projections, pagination, organization isolation, archive behavior, production authorization, layer dependencies and document workflows. Test stores/adapters are in-memory; live relational/Azure deployments are not covered.

Before production, implement migrations, durable storage/jobs, audit history, concurrency handling, full organization/business/location memberships and dependency readiness checks. Financial records, deterministic metrics and dashboards precede the structured-data adviser.

