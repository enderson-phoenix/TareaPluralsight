# CalSystem — Guía para Claude Code

## Stack

- .NET 10 / C# 13
- Clean Architecture (Domain → Application → Infrastructure → Api)
- EF Core 9 con SQLite
- MediatR 12 para CQRS
- xUnit + Moq + FluentAssertions
- Swagger / Swashbuckle para documentación HTTP

## Arquitectura

```
src/
  CalSystem.Domain/          # Entidades, enums, interfaces de repositorio
  CalSystem.Application/     # Commands, Queries, Handlers (casos de uso)
  CalSystem.Infrastructure/  # AppDbContext, repositorios, migraciones
  CalSystem.Api/             # Controllers, Program.cs, appsettings
tests/
  CalSystem.Tests/           # Tests unitarios con Moq y FluentAssertions
```

**Jerarquía de dependencias:**
- Domain → sin dependencias externas
- Application → referencia Domain solamente
- Infrastructure → referencia Application + Domain
- Api → referencia Application + Infrastructure

## Convenciones

- Nombres en inglés (código). Español solo en strings de dominio y comentarios.
- Commands y Queries como `record` inmutables que implementan `IRequest<T>`.
- Un handler por archivo, en el mismo directorio que su command/query.
- Propiedades de entidades con `private set`. Solo los métodos de la entidad cambian su estado.
- Constructores privados en entidades; factory method `Create(...)` estático.

## Rutas importantes

- `src/CalSystem.Domain/Entities/` — ServiceOrder, Technician
- `src/CalSystem.Domain/Interfaces/` — IServiceOrderRepository
- `src/CalSystem.Application/Orders/Commands/` — CreateOrder, AssignTechnician
- `src/CalSystem.Application/Orders/Queries/` — GetOrdersByStatus
- `src/CalSystem.Infrastructure/Persistence/` — AppDbContext, repositorios
- `src/CalSystem.Infrastructure/Migrations/` — migraciones EF Core
- `src/CalSystem.Api/Controllers/` — ServiceOrdersController
- `tests/CalSystem.Tests/Orders/` — tests unitarios por capa

## Restricciones

- No agregar lógica de negocio fuera del Domain.
- No referenciar Infrastructure desde Application o Domain.
- No usar datos en memoria en producción — siempre SQLite vía EF Core.
- Siempre agregar `AsNoTracking()` en queries de solo lectura.
- Nunca usar `.Result` o `.Wait()` — todo async/await.

## Comandos frecuentes

```bash
dotnet build                              # Compilar toda la solución
dotnet test                               # Correr los 7 tests
dotnet run --project src/CalSystem.Api   # Iniciar la API (Swagger en /swagger)

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
- `change-validator` — agente quality gate con 7 checks automáticos
- Hooks en `.claude/settings.json` — validación automática en edición y commits
