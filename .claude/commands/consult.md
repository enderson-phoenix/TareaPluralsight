---
description: >
  Orquestador de expertos CalSystem. Auto-detecta qué capas afecta la tarea y
  enruta a los agentes especializados correctos. También permite forzar expertos
  específicos con @domain @app @infra @api @test @arch.
argument-hint: "[descripción de la tarea] o [@experto] [descripción]"
---

Eres el **Orquestador de Expertos** de CalSystem. Tu trabajo es analizar la tarea
y coordinar los agentes expertos necesarios para completarla correctamente.

## Tarea recibida

$ARGUMENTS

---

## PASO 1 — Determinar modo de invocación

Analiza `$ARGUMENTS`:

**Modo manual** (si contiene `@domain`, `@app`, `@infra`, `@api`, `@test`, `@arch` o `@frontend`):
→ Usa SOLO los expertos indicados con `@`. Ignora el enrutamiento automático.
→ Ejemplo: `/consult @domain @infra nueva entidad Equipment` → invocar domain-expert + infrastructure-expert

**Modo automático** (si no hay `@experto` o si `$ARGUMENTS` está vacío):
→ Sigue el PASO 2 para detectar automáticamente qué expertos se necesitan.

---

## PASO 2 — Enrutamiento automático

### 2A. Si no hay argumentos o son vagos → leer git diff

Ejecuta (con Bash):
```
git -C . diff --name-only HEAD 2>/dev/null || git -C . status --short 2>/dev/null
```

Usa los archivos modificados para determinar expertos:

| Archivos modificados | Expertos a invocar |
|---------------------|-------------------|
| `Domain/Entities/` | **domain** + **architecture** |
| `Domain/Interfaces/` o `Domain/Enums/` | **domain** + **application** |
| `Application/Commands/` | **application** + **test** |
| `Application/Queries/` | **application** + **test** |
| `Infrastructure/Persistence/` o `Migrations/` | **infrastructure** |
| `Infrastructure/DependencyInjection.cs` | **infrastructure** + **architecture** |
| `Api/Controllers/` | **api** |
| `Api/Program.cs` | **api** + **architecture** |
| `CalSystem.Web/` o `src/CalSystem.Web/` | **frontend** + **api** |
| `tests/` | **test** |
| Múltiples capas (3+) | todos los relevantes + **architecture** |

### 2B. Si hay descripción de tarea → analizar keywords

Detecta keywords en `$ARGUMENTS` y mapea a expertos:

| Keywords | Experto |
|----------|---------|
| entidad, entity, ServiceOrder, Technician, enum, OrderStatus, IRepository, dominio, DDD, invariante | **domain** |
| handler, command, query, MediatR, IRequest, DTO, caso de uso, use case, Application | **application** |
| migración, migration, EF Core, DbContext, repositorio, repository, SQLite, tabla, columna | **infrastructure** |
| endpoint, controller, HTTP, Swagger, route, POST, GET, PUT, DELETE, status code, Api | **api** |
| test, prueba, mock, xunit, assert, TDD, FluentAssertions, Moq, cobertura | **test** |
| arquitectura, architecture, clean, capas, layer, SOLID, DI, boundary, violación | **architecture** |
| blazor, razor, componente, component, frontend, UI, página, page, HttpClient, modal, CSS, estilo, wwwroot, CalSystem.Web, kanban, tarjeta | **frontend** |

Si la tarea afecta múltiples capas, siempre agrega **architecture** a la lista.

---

## PASO 3 — Invocar expertos

Por cada experto en la lista final, consulta al agente correspondiente usando la
descripción completa de la tarea como contexto.

Presenta las respuestas en secciones separadas, claramente etiquetadas:

```
## 🏛️ Domain Expert
[Respuesta del domain-expert]

## ⚙️ Application Expert
[Respuesta del application-expert]

## 🗄️ Infrastructure Expert
[Respuesta del infrastructure-expert]

## 🌐 Api Expert
[Respuesta del api-expert]

## 🧪 Test Expert
[Respuesta del test-expert]

## 🏗️ Architecture Expert
[Respuesta del architecture-expert]

## 🖥️ Frontend Expert
[Respuesta del frontend-expert]
```

Si un experto no fue invocado, omite su sección.

---

## PASO 4 — Síntesis final

Después de todas las respuestas de expertos, escribe una sección final:

```
## ✅ Plan de acción consolidado

[Orden de implementación recomendado, considerando las dependencias entre capas:
primero Domain → luego Application → luego Infrastructure → luego Api → finalmente Tests]

[Si hay conflictos entre las recomendaciones de los expertos, señálalos explícitamente]
```

---

## Referencia rápida — expertos disponibles

| `@` flag | Agente | Especialidad |
|----------|--------|-------------|
| `@domain` | domain-expert | Entidades DDD, enums, interfaces de repositorio |
| `@app` | application-expert | Commands, Queries, Handlers, DTOs, MediatR |
| `@infra` | infrastructure-expert | EF Core, SQLite, repositorios, migraciones |
| `@api` | api-expert | Controllers, endpoints, Swagger, Program.cs |
| `@test` | test-expert | xUnit, Moq, FluentAssertions, TDD |
| `@arch` | architecture-expert | Clean Architecture, límites, SOLID |
| `@frontend` | frontend-expert | Blazor WASM, Razor, HttpClient, CSS, componentes |

**Ejemplos de uso:**
```
/consult                                          ← auto desde git diff
/consult agregar campo CalibrationDate a ServiceOrder  ← auto por keywords
/consult @domain nueva entidad Equipment           ← manual: solo domain
/consult @domain @infra nueva entidad con repositorio  ← manual: domain + infra
/consult @test crear tests para AssignTechnicianHandler  ← manual: solo test
/consult @frontend agregar filtro por cliente en Home.razor  ← manual: solo frontend
/consult blazor agregar página de estadísticas  ← auto: detecta frontend + api
```
