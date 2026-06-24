---
id: ctx-005
title: Frontend Blazor WASM: Kanban + Técnicos + navegación
date: 2026-06-24
tags: [frontend]
type: feature
accessed: 0
---

## Resumen
Interfaz visual completa en Blazor WebAssembly 10.0.9 corriendo en puerto 5200. El frontend consume la API REST en puerto 5112 via `OrderApiService` (HttpClient centralizado). Incluye Kanban de 3 columnas (Pending/InProgress/Closed), página de gestión de técnicos, barra de navegación y 4 modales. No tiene referencias al backend — es puramente un cliente HTTP.

## Archivos clave
- `src/CalSystem.Web/Pages/Home.razor` — Kanban con estadísticas y 3 columnas de órdenes
- `src/CalSystem.Web/Pages/Technicians.razor` — formulario crear técnico + tabla listado
- `src/CalSystem.Web/Components/OrderCard.razor` — tarjeta de orden con acciones contextuales por estado
- `src/CalSystem.Web/Components/AssignTechnicianModal.razor` — dropdown de técnicos reales (no GUID manual)
- `src/CalSystem.Web/Components/CreateOrderModal.razor` — formulario nueva orden
- `src/CalSystem.Web/Components/CloseOrderModal.razor` — campo de notas al cerrar
- `src/CalSystem.Web/Services/OrderApiService.cs` — todos los métodos HTTP centralizados
- `src/CalSystem.Web/Layouts/MainLayout.razor` — nav con NavLink (Órdenes / Técnicos)
- `src/CalSystem.Web/wwwroot/css/app.css` — estilos completos (dark mode, kanban, modales)

## Decisiones tomadas
- Sin CDN externos — CSS inline en app.css (restricción de seguridad Blazor WASM)
- `OrderApiService` centraliza todas las llamadas HTTP (no dispersas en componentes)
- Estado de los modales se maneja con `@bind` + `EventCallback<bool>` — patrón estándar Blazor
- `StateHasChanged()` llamado explícitamente después de operaciones async que modifican UI

## Problemas resueltos
- Rutas de API originalmente usaban `/api/orders` — corregidas a `/api/service-orders` en todo el servicio para alinear con el assignment

## Relacionado
- [[ctx-004]] — gestión de técnicos (Technicians.razor y dropdown)
- [[ctx-001]] — CORS en Program.cs configurado para localhost:5200
