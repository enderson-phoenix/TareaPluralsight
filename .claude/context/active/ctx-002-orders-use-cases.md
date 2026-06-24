---
id: ctx-002
title: UC-01..03: CreateOrder, AssignTechnician, GetOrdersByStatus
date: 2026-06-24
tags: [domain,app,api,test]
type: feature
accessed: 0
---

## Resumen
Implementación de los tres casos de uso base del sistema de órdenes de servicio usando CQRS con MediatR 12. Cada caso de uso sigue el patrón: `record` Command/Query implementando `IRequest<T>` + Handler en el mismo directorio. Los repositorios están definidos como interfaces en Domain e implementados en Infrastructure. 7 tests unitarios con xUnit + Moq + FluentAssertions.

## Archivos clave
- `src/CalSystem.Domain/Entities/ServiceOrder.cs` — entidad con constructor privado, `Create()` factory, `private set`
- `src/CalSystem.Domain/Interfaces/IServiceOrderRepository.cs` — contrato del repositorio
- `src/CalSystem.Application/Orders/Commands/CreateOrder/` — CreateOrderCommand + Handler
- `src/CalSystem.Application/Orders/Commands/AssignTechnician/` — AssignTechnicianCommand + Handler
- `src/CalSystem.Application/Orders/Queries/GetOrdersByStatus/` — Query + Handler + OrderDto
- `src/CalSystem.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs` — implementación EF Core
- `tests/CalSystem.Tests/Orders/` — 7 tests cubriendo happy path + edge cases

## Decisiones tomadas
- `AssignTechnicianCommand` retorna `bool` (no `Unit`) para dar feedback al controller si la orden existía
- `GetOrdersByStatus` usa `AsNoTracking()` — es read-only
- `GetByIdAsync` NO usa `AsNoTracking()` — precede a `UpdateAsync`
- Constructor privado en `ServiceOrder` requerido tanto por DDD como por EF Core

## Problemas resueltos
- Ninguno — implementación directa siguiendo el patrón Clean Architecture

## Relacionado
- [[ctx-001]] — setup inicial (infraestructura que estos casos de uso usan)
- [[ctx-003]] — UC-04 CloseOrder (extiende este contexto)
