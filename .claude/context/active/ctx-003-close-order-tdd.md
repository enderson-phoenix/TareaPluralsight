---
id: ctx-003
title: UC-04: CloseOrder con Notes + ciclo TDD rojo-verde
date: 2026-06-24
tags: [domain,app,api,test]
type: feature
accessed: 0
---

## Resumen
Implementación de cierre de órdenes de servicio incluyendo campo `Notes` opcional para observaciones del técnico. Desarrollado con ciclo TDD completo: test escrito primero (rojo: build falló), luego implementación mínima (verde: 10 tests pasando). Requirió cambios en Domain, Application, Infrastructure (mapping), Api (endpoint) y Tests (3 nuevos casos).

## Archivos clave
- `src/CalSystem.Domain/Entities/ServiceOrder.cs` — agregado `Notes { get; private set; }` + método `Close(string? notes)`
- `src/CalSystem.Application/Orders/Commands/CloseOrder/CloseOrderCommand.cs` — record `(Guid OrderId, string? Notes) : IRequest<bool>`
- `src/CalSystem.Application/Orders/Commands/CloseOrder/CloseOrderHandler.cs` — handler retorna bool
- `src/CalSystem.Application/Orders/Queries/GetOrdersByStatus/OrderDto.cs` — campo `Notes` agregado al DTO
- `src/CalSystem.Infrastructure/Persistence/AppDbContext.cs` — `.HasMaxLength(2000)` para Notes
- `src/CalSystem.Api/Controllers/ServiceOrdersController.cs` — endpoint `PUT {id}/close`
- `tests/CalSystem.Tests/Orders/Commands/CloseOrderHandlerTests.cs` — 3 tests: con notes, sin notes, inexistente

## Decisiones tomadas
- `Notes` es `string?` (nullable) — cerrar sin notas es válido para órdenes simples
- `CloseOrderHandler` retorna `bool`: `true` si cerró, `false` si la orden no existe
- `HasMaxLength(2000)` en EF Core para limitar tamaño sin validation formal (MVP)
- Ruta: `PUT /api/service-orders/{id}/close` (alineada con spec de evaluación)

## Problemas resueltos
- TDD real: test escrito antes del handler causó `error CS0246: CloseOrderHandler not found` — comportamiento esperado en ciclo rojo
- La ruta inicial era `/api/orders/{id}/close` — se corrigió a `/api/service-orders/{id}/close` para alinear con el assignment

## Relacionado
- [[ctx-002]] — UC-01..03 (base sobre la que se construye este UC-04)
- [[ctx-004]] — gestión de técnicos (el técnico asignado es quien cierra con notes)
