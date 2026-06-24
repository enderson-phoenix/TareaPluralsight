# CalSystem

Sistema de Órdenes de Servicio Técnico para **Phoenix Calibration DR**. Gestiona órdenes de calibración y servicio técnico de equipos industriales, con asignación de técnicos, seguimiento de estado y cierre con notas.

---

## Stack

| Capa | Tecnología |
|------|-----------|
| Frontend | Blazor WebAssembly 10.0.9 — Kanban interactivo (puerto 5200) |
| API | ASP.NET Core 10, Swagger/Swashbuckle (puerto 5112) |
| Aplicación | MediatR 12 (CQRS) |
| Persistencia | EF Core 10 + SQLite (migraciones formales) |
| Tests | xUnit + Moq + FluentAssertions |
| Arquitectura | Clean Architecture (4 capas) |

---

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- dotnet-ef tools para gestión de migraciones:

```bash
dotnet tool install --global dotnet-ef
```

---

## Instalación y ejecución

```bash
# Clonar el repositorio
git clone https://github.com/enderson-phoenix/TareaPluralsight.git
cd TareaPluralsight

# Restaurar dependencias y compilar
dotnet restore
dotnet build

# Terminal 1 — API (crea la BD automáticamente vía Migrate())
dotnet run --project src/CalSystem.Api --urls http://localhost:5112

# Terminal 2 — Frontend Blazor
dotnet run --project src/CalSystem.Web --urls http://localhost:5200
```

| Recurso | URL |
|---------|-----|
| **Frontend (Kanban)** | http://localhost:5200 |
| **Swagger UI** | http://localhost:5112/swagger |

---

## Flujo del sistema

```
[Crear Orden] → Pending
      ↓  (Asignar Técnico)
  InProgress
      ↓  (Cerrar Orden + Notas)
    Closed
```

---

## Endpoints — Órdenes de Servicio

### POST /api/service-orders — Crear orden

```http
POST /api/service-orders
Content-Type: application/json

{
  "customerName": "Juan Pérez",
  "equipment": "Balanza Analítica XR-200",
  "problemDescription": "No enciende después de caída de tensión"
}
```

**Respuesta:** `201 Created`
```json
{ "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

---

### PUT /api/service-orders/{id}/assign — Asignar técnico

```http
PUT /api/service-orders/3fa85f64.../assign
Content-Type: application/json

{
  "technicianId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

**Respuesta:** `200 OK` | `404 Not Found`

---

### PUT /api/service-orders/{id}/close — Cerrar orden

```http
PUT /api/service-orders/3fa85f64.../close
Content-Type: application/json

{
  "notes": "Sensor calibrado. Lectura corregida a ±0.1°C."
}
```

**Respuesta:** `200 OK` | `404 Not Found`

---

### GET /api/service-orders?status={status} — Consultar por estado

```http
GET /api/service-orders?status=Pending
GET /api/service-orders?status=InProgress
GET /api/service-orders?status=Closed
```

**Valores válidos:** `Pending`, `InProgress`, `Closed`

**Respuesta:** `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "Juan Pérez",
    "equipment": "Balanza Analítica XR-200",
    "problemDescription": "No enciende después de caída de tensión",
    "status": "Pending",
    "technicianId": null,
    "notes": null,
    "createdAt": "2026-06-24T10:30:00Z"
  }
]
```

---

## Endpoints — Técnicos

### POST /api/technicians — Registrar técnico

```http
POST /api/technicians
Content-Type: application/json

{
  "name": "Carlos López",
  "email": "carlos@phoenix.com"
}
```

**Respuesta:** `201 Created`
```json
{ "id": "a1b2c3d4-..." }
```

---

### GET /api/technicians — Listar técnicos

```http
GET /api/technicians
```

**Respuesta:** `200 OK`
```json
[
  {
    "id": "a1b2c3d4-...",
    "name": "Carlos López",
    "email": "carlos@phoenix.com"
  }
]
```

---

## Pruebas

```bash
dotnet test
```

```
Passed! - Failed: 0, Passed: 11, Skipped: 0, Total: 11
```

| Conjunto de tests | Casos |
|-------------------|-------|
| `CreateOrderHandlerTests` | 2 — id válido, estado Pending inicial |
| `AssignTechnicianHandlerTests` | 2 — asignación exitosa, orden no encontrada |
| `CloseOrderHandlerTests` | 3 — con notas, sin notas, orden inexistente |
| `GetOrdersByStatusHandlerTests` | 2 — mapeo a DTO, colección vacía |
| `CreateTechnicianHandlerTests` | 2 — id válido, llamada única a AddAsync |

---

## Arquitectura

```
src/
  CalSystem.Domain/          # Entidades (ServiceOrder, Technician), enums, interfaces
  CalSystem.Application/     # Casos de uso: Commands, Queries, Handlers, DTOs
  CalSystem.Infrastructure/  # EF Core, SQLite, repositorios, migraciones
  CalSystem.Api/             # Controllers, Program.cs, Swagger
  CalSystem.Web/             # Blazor WASM: páginas, componentes, servicios HTTP
tests/
  CalSystem.Tests/           # Pruebas unitarias con xUnit + Moq + FluentAssertions
```

**Jerarquía de dependencias:**
```
Domain ← Application ← Infrastructure ← Api
                                    ↑
                            CalSystem.Web (vía HTTP)
```

**Flujo de una petición:**
```
HTTP Request
  → Controller
  → MediatR.Send(Command/Query)
  → Handler (Application)
  → Repository Interface (Domain)
  → Repository Implementation (Infrastructure / EF Core / SQLite)
```

---

## Migraciones EF Core

El proyecto usa migraciones formales (no `EnsureCreated`):

```bash
# Aplicar migraciones pendientes
dotnet ef database update \
  --project src/CalSystem.Infrastructure \
  --startup-project src/CalSystem.Api

# Crear nueva migración tras cambios en entidades
dotnet ef migrations add NombreMigracion \
  --project src/CalSystem.Infrastructure \
  --startup-project src/CalSystem.Api
```

---

## Claude Code Tooling

### Validación de calidad

| Herramienta | Uso |
|-------------|-----|
| `/validate` | Valida compilación, tests, arquitectura y migraciones |
| `/validate-agent` | Análisis profundo en subagente aislado |
| `/validate-smart` | Validación interactiva por severidad (🔴🟡🔵) |
| `/new-entity` | Genera entidades de dominio DDD listas para usar |
| `/crear-pr [título]` | Crea rama feature, commit, push y PR en GitHub sin tocar main |
| `/ctx-save [título]` | Guarda mini-contexto del trabajo actual en el índice |
| `/ctx-search [@tag\|keyword]` | Busca contexto específico sin leer todos los archivos |
| `/ctx-cleanup` | Archiva contextos de más de 6 meses al backlog |
| Hooks automáticos | Build en cada edición .cs, gates antes de commits, índice al inicio de sesión |

### Sistema de agentes expertos

| Herramienta | Uso |
|-------------|-----|
| `/consult` | Orquestador: auto-detecta capas afectadas y enruta a los expertos |
| `/consult @domain ...` | Consulta solo al experto de dominio (DDD, entidades) |
| `/consult @app ...` | Consulta solo al experto de Application (CQRS, handlers) |
| `/consult @infra ...` | Consulta solo al experto de Infrastructure (EF Core, SQLite) |
| `/consult @api ...` | Consulta solo al experto de Api (controllers, Swagger) |
| `/consult @test ...` | Consulta solo al experto de Testing (xUnit, Moq, TDD) |
| `/consult @arch ...` | Consulta solo al experto de Arquitectura (Clean Architecture) |
| `/consult @frontend ...` | Consulta solo al experto de Frontend (Blazor WASM) |

**Atajos directos:**

| Herramienta | Especialidad |
|-------------|-------------|
| `/domain-expert` | Entidades DDD, enums, interfaces de repositorio |
| `/app-expert` | Commands, Queries, Handlers, DTOs, MediatR |
| `/infra-expert` | EF Core, AppDbContext, repositorios, migraciones |
| `/api-expert` | Controllers, endpoints HTTP, Swagger, Program.cs |
| `/test-expert` | xUnit, Moq, FluentAssertions, ciclo TDD |
| `/arch-expert` | Clean Architecture, límites de capas, SOLID |
| `/frontend-expert` | Blazor WASM, Razor, HttpClient, componentes, CSS |

Ver `.claude/GUIDE.md` para documentación completa.

### MCP Servers configurados

| Servidor | Propósito |
|----------|-----------|
| `github` | Crear issues, commits, PRs desde Claude Code |

**Configuración:** `.mcp.json` en la raíz del repo (commiteado — compartido con el equipo).  
Requiere la variable de entorno `GITHUB_PERSONAL_ACCESS_TOKEN` definida en el shell o en `.env.local`.
