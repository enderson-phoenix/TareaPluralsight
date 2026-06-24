# CalSystem — Guía para Claude Code

## Stack

- .NET 10 / C# 13
- Clean Architecture (Domain → Application → Infrastructure → Api)
- Blazor WebAssembly 10.0.9 — frontend en `src/CalSystem.Web/` (puerto 5200)
- EF Core 10 + SQLite — migraciones formales (`Migrate()`, no `EnsureCreated`)
- MediatR 12 para CQRS
- xUnit + Moq + FluentAssertions
- Swagger / Swashbuckle para documentación HTTP

## Arquitectura

```
src/
  CalSystem.Domain/          # Entidades, enums, interfaces de repositorio
  CalSystem.Application/     # Commands, Queries, Handlers (casos de uso)
  CalSystem.Infrastructure/  # AppDbContext, repositorios, migraciones
  CalSystem.Api/             # Controllers, Program.cs, appsettings (puerto 5112)
  CalSystem.Web/             # Blazor WASM: páginas, componentes, servicios HTTP
tests/
  CalSystem.Tests/           # Tests unitarios con Moq y FluentAssertions
```

**Jerarquía de dependencias:**
- Domain → sin dependencias externas
- Application → referencia Domain solamente
- Infrastructure → referencia Application + Domain
- Api → referencia Application + Infrastructure
- CalSystem.Web → sin referencias al backend; consume la API vía HttpClient

## Convenciones

- Nombres en inglés (código). Español solo en strings de dominio y comentarios.
- Commands y Queries como `record` inmutables que implementan `IRequest<T>`.
- Un handler por archivo, en el mismo directorio que su command/query.
- Propiedades de entidades con `private set`. Solo los métodos de la entidad cambian su estado.
- Constructores privados en entidades; factory method `Create(...)` estático.
- Blazor: servicios en `CalSystem.Web/Services/`, modelos en `CalSystem.Web/Models/`, páginas en `CalSystem.Web/Pages/`, componentes reutilizables en `CalSystem.Web/Components/`.

## Rutas importantes

- `src/CalSystem.Domain/Entities/` — ServiceOrder, Technician
- `src/CalSystem.Domain/Interfaces/` — IServiceOrderRepository, ITechnicianRepository
- `src/CalSystem.Application/Orders/Commands/` — CreateOrder, AssignTechnician, CloseOrder
- `src/CalSystem.Application/Orders/Queries/` — GetOrdersByStatus
- `src/CalSystem.Application/Technicians/Commands/` — CreateTechnician
- `src/CalSystem.Application/Technicians/Queries/` — GetAllTechnicians
- `src/CalSystem.Infrastructure/Persistence/` — AppDbContext, repositorios
- `src/CalSystem.Infrastructure/Migrations/` — migraciones EF Core
- `src/CalSystem.Api/Controllers/` — ServiceOrdersController, TechniciansController
- `src/CalSystem.Web/Pages/` — Home.razor (Kanban), Technicians.razor
- `src/CalSystem.Web/Components/` — OrderCard, AssignTechnicianModal, CloseOrderModal, CreateOrderModal
- `src/CalSystem.Web/Services/` — OrderApiService
- `tests/CalSystem.Tests/Orders/` — tests unitarios de órdenes
- `tests/CalSystem.Tests/Technicians/` — tests unitarios de técnicos

## Restricciones

- No agregar lógica de negocio fuera del Domain.
- No referenciar Infrastructure desde Application o Domain.
- No usar datos en memoria en producción — siempre SQLite vía EF Core.
- Siempre agregar `AsNoTracking()` en queries de solo lectura (excepto en `GetByIdAsync` que precede un `Update`).
- Nunca usar `.Result` o `.Wait()` — todo async/await.
- Nuevas entidades requieren migración EF Core — no modificar `EnsureCreated` (ya removido).
- Cambios de endpoints en la API deben reflejarse en `OrderApiService.cs` del frontend.

## Comandos frecuentes

```bash
dotnet build                              # Compilar toda la solución
dotnet test                               # Correr los 11 tests
dotnet run --project src/CalSystem.Api --urls http://localhost:5112   # API + Swagger
dotnet run --project src/CalSystem.Web --urls http://localhost:5200   # Frontend Blazor

# Migraciones EF Core
dotnet ef migrations add NombreMigracion \
  --project src/CalSystem.Infrastructure \
  --startup-project src/CalSystem.Api
dotnet ef database update \
  --project src/CalSystem.Infrastructure \
  --startup-project src/CalSystem.Api
```

## Claude Code Tooling

- `/validate` — valida el cambio actual (compilación, tests, arquitectura, DI)
- `/validate-agent` — delega al agente change-validator en subcontexto aislado
- `/validate-smart` — validación interactiva por severidad (🔴🟡🔵)
- `/new-entity` — genera una entidad de dominio DDD lista para usar
- `/consult` — orquestador: auto-detecta capas afectadas y enruta a expertos
- `/frontend-expert` — experto Blazor WASM (componentes, servicios, CSS)
- `change-validator` — agente quality gate con 7 checks automáticos
- Hooks en `.claude/settings.json` — validación automática en edición y commits
