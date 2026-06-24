---
id: ctx-004
title: Gestión de técnicos end-to-end (6 capas)
date: 2026-06-24
tags: [domain,app,api,frontend]
type: feature
accessed: 0
---

## Resumen
Implementación completa del CRUD de técnicos (crear + listar) atravesando todas las capas de la arquitectura. Incluye entidad `Technician` en Domain, interfaz `ITechnicianRepository`, handlers `CreateTechnicianCommand` y `GetAllTechniciansQuery`, controlador `TechniciansController`, página Blazor `/technicians` y 2 tests unitarios. El modal de asignación en el frontend ahora usa un dropdown con técnicos reales en lugar de pedir el GUID manualmente.

## Archivos clave
- `src/CalSystem.Domain/Entities/Technician.cs` — entidad con `Id`, `Name`, `Email`, `Create()`
- `src/CalSystem.Domain/Interfaces/ITechnicianRepository.cs` — `AddAsync` + `GetAllAsync`
- `src/CalSystem.Application/Technicians/Commands/CreateTechnician/` — Command + Handler
- `src/CalSystem.Application/Technicians/Queries/GetAllTechnicians/` — Query + Handler + TechnicianDto
- `src/CalSystem.Infrastructure/Persistence/Repositories/TechnicianRepository.cs` — implementación EF Core
- `src/CalSystem.Infrastructure/DependencyInjection.cs` — registro `ITechnicianRepository → TechnicianRepository`
- `src/CalSystem.Api/Controllers/TechniciansController.cs` — POST /api/technicians + GET /api/technicians
- `src/CalSystem.Web/Pages/Technicians.razor` — formulario crear + tabla listar
- `tests/CalSystem.Tests/Technicians/Commands/CreateTechnicianHandlerTests.cs` — 2 tests

## Decisiones tomadas
- `GetAllAsync` usa `AsNoTracking()` — es solo lectura
- El dropdown de asignación carga técnicos al abrir el modal (`OnParametersSetAsync`)
- Si no hay técnicos registrados: mensaje "Crea uno en la sección Técnicos" en el modal

## Problemas resueltos
- Ninguno — patrón ya establecido por UC-01..03 se replicó limpiamente

## Relacionado
- [[ctx-002]] — patrón de repositorio y handlers que se replica aquí
- [[ctx-005]] — frontend (Technicians.razor y dropdown en AssignTechnicianModal)
