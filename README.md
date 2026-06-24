# CalSystem

Sistema de Órdenes de Servicio Técnico para **Phoenix Calibration DR**. Gestiona órdenes de calibración y servicio técnico de equipos industriales, con asignación de técnicos y seguimiento de estado.

> Generado con asistencia de Claude Code — prompt: "Genera el README.md principal del proyecto CalSystem. Stack: .NET 10, Clean Architecture, EF Core SQLite, MediatR, xUnit."

---

## Stack

| Capa | Tecnología |
|------|-----------|
| API | ASP.NET Core 10, Swagger/Swashbuckle |
| Aplicación | MediatR 12 (CQRS) |
| Persistencia | EF Core 9 + SQLite |
| Tests | xUnit + Moq + FluentAssertions |
| Arquitectura | Clean Architecture |

---

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Opcional) [dotnet-ef tools](https://learn.microsoft.com/ef/core/cli/dotnet) para gestión de migraciones

```bash
dotnet tool install --global dotnet-ef
```

---

## Instalación y ejecución

```bash
# Clonar el repositorio
git clone https://github.com/TU_USUARIO/calsystem.git
cd calsystem

# Restaurar dependencias y compilar
dotnet restore
dotnet build

# Ejecutar la API (la base de datos SQLite se crea automáticamente)
dotnet run --project src/CalSystem.Api
```

La API estará disponible en `https://localhost:{puerto}`.  
Swagger UI en `https://localhost:{puerto}/swagger`.

---

## Endpoints

### POST /api/orders — Crear orden de servicio

```http
POST /api/orders
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

### PUT /api/orders/{id}/assign — Asignar técnico

```http
PUT /api/orders/3fa85f64-5717-4562-b3fc-2c963f66afa6/assign
Content-Type: application/json

{
  "technicianId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

**Respuesta:** `200 OK` | `404 Not Found` si la orden no existe.

---

### GET /api/orders?status={status} — Consultar órdenes por estado

```http
GET /api/orders?status=Pending
GET /api/orders?status=InProgress
GET /api/orders?status=Closed
```

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
    "createdAt": "2026-06-24T10:30:00Z"
  }
]
```

**Valores válidos de status:** `Pending`, `InProgress`, `Closed`  
**Error:** `400 Bad Request` si el status no es válido.

---

## Pruebas

```bash
dotnet test
```

```
Passed! - Failed: 0, Passed: 7, Skipped: 0, Total: 7
```

Los tests unitarios cubren los 3 handlers (CreateOrder, AssignTechnician, GetOrdersByStatus) usando Moq para el repositorio y FluentAssertions para los asserts.

---

## Arquitectura

```
src/
  CalSystem.Domain/          # Entidades (ServiceOrder, Technician), enums, interfaces
  CalSystem.Application/     # Casos de uso: Commands, Queries, Handlers
  CalSystem.Infrastructure/  # EF Core, SQLite, repositorios, migraciones
  CalSystem.Api/             # Controllers, Program.cs, Swagger
tests/
  CalSystem.Tests/           # Pruebas unitarias con xUnit
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

## Claude Code Tooling

Este proyecto incluye comandos y agents personalizados para validación de calidad:

| Herramienta | Uso |
|-------------|-----|
| `/validate` | Valida compilación, tests, arquitectura y migraciones |
| `/validate-agent` | Análisis profundo en subagente aislado |
| `/validate-smart` | Validación interactiva por severidad (🔴🟡🔵) |
| `/new-entity` | Genera entidades de dominio DDD |
| Hooks automáticos | Build en cada edición .cs, gates antes de commits |

Ver `.claude/GUIDE.md` para documentación completa.
