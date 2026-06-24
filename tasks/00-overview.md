# Sistema de Órdenes de Servicio Técnico — Checklist Maestro

**Proyecto:** Phoenix Calibration DR  
**Stack:** .NET Core · Clean Architecture · EF Core · SQLite · xUnit  
**Plazo:** 5 días después del horario laboral

---

## Mapa de dependencias

```
01-setup  →  02-domain  →  03-application  →  04-infrastructure  →  05-api  →  06-tests
                                                                                   ↑
                                                        07-claude-topics  (se intercala con todo)
```

Sigue el orden numérico. Los temas de Claude Code (archivo `07`) se van completando a medida que avanzas — no los dejes todos para el final.

---

## Progreso general

### Fase 1 — Configuración del proyecto
- [ ] Solución .NET creada con estructura Clean Architecture
- [ ] Repositorio en GitHub inicializado y enlazado
- [ ] Paquetes NuGet instalados
- [ ] Proyecto compila sin errores

**Detalles:** [`01-setup.md`](01-setup.md)

---

### Fase 2 — Capa de Dominio
- [ ] Entidad `ServiceOrder` creada
- [ ] Entidad `Technician` creada
- [ ] Enum `OrderStatus` definido
- [ ] Interfaz `IServiceOrderRepository` definida

**Detalles:** [`02-domain.md`](02-domain.md)

---

### Fase 3 — Capa de Aplicación (Casos de Uso)
- [ ] UC-01: `CreateOrderCommand` + handler implementados
- [ ] UC-02: `AssignTechnicianCommand` + handler implementados
- [ ] UC-03: `GetOrdersByStatusQuery` + handler implementados
- [ ] DTOs de request/response creados
- [ ] MediatR configurado en DI

**Detalles:** [`03-application.md`](03-application.md)

---

### Fase 4 — Capa de Infraestructura
- [ ] `AppDbContext` creado y configurado
- [ ] `ServiceOrderRepository` implementado
- [ ] Migración inicial creada y aplicada
- [ ] SQLite connection string configurada

**Detalles:** [`04-infrastructure.md`](04-infrastructure.md)

---

### Fase 5 — Capa de API
- [ ] `POST /api/orders` funciona y retorna 201
- [ ] `PUT /api/orders/{id}/assign` funciona y retorna 200
- [ ] `GET /api/orders?status={status}` funciona y retorna 200
- [ ] Swagger disponible en `/swagger`

**Detalles:** [`05-api.md`](05-api.md)

---

### Fase 6 — Pruebas
- [ ] Ciclo TDD documentado para UC-01 (prueba roja → verde)
- [ ] Tests de UC-02 y UC-03 escritos y pasando
- [ ] Test de integración básico pasando
- [ ] `dotnet test` retorna 0 errores

**Detalles:** [`06-tests.md`](06-tests.md)

---

### Fase 7 — Temas de Claude Code (evaluados al 40%)
- [ ] T-01: Plan Mode documentado
- [ ] T-02: CLAUDE.md configurado
- [ ] T-03: Ciclo TDD evidenciado en `EVIDENCE.md`
- [ ] T-04: README.md y XML comments generados con Claude
- [ ] T-05: Revisión de seguridad realizada
- [ ] T-06: Acción real con GitHub MCP completada
- [ ] T-07: Custom Skill creado y funcional
- [ ] T-08: Custom Hook implementado

**Detalles:** [`07-claude-topics.md`](07-claude-topics.md)

---

## Criterios de evaluación

| Criterio | Peso | Estado |
|----------|------|--------|
| Funcionalidad (3 endpoints + tests pasando) | 30% | [ ] |
| Uso de Claude Code (8 temas con evidencia) | 40% | [ ] |
| Arquitectura y calidad del código | 20% | [ ] |
| Skill y Hook funcionales y documentados | 10% | [ ] |

---

## Verificación final

```bash
dotnet build                          # Sin errores
dotnet test                           # Todos los tests en verde
dotnet run --project src/Api          # Swagger en http://localhost:5000/swagger
```

Confirmar que `EVIDENCE.md` tiene evidencia de los 8 temas antes de entregar.
