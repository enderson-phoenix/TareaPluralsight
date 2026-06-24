# Requirements — Sistema de Órdenes de Servicio Técnico

> **Proyecto:** Claude Code en Práctica  
> **Organización:** Phoenix Calibration DR  
> **Plazo:** 5 días después del horario laboral  
> **Entregable:** Repositorio en GitHub con evidencias de uso de Claude Code

---

## Stack Tecnológico

- .NET Core
- Clean Architecture
- Entity Framework Core
- SQLite
- xUnit

---

## Casos de Uso

### UC-01 — Crear orden de servicio
Registrar una nueva orden con datos del cliente, equipo y descripción del problema.

### UC-02 — Asignar técnico a una orden
Actualizar el estado de una orden y asignarle un técnico responsable.

### UC-03 — Consultar órdenes por estado
Listar órdenes filtradas por su estado actual: `Pendiente`, `En Progreso`, `Cerrada`.

---

## Temas de Claude Code Requeridos

Cada tema debe tener evidencia clara de su uso en el repositorio. No se evalúa el orden de aplicación.

### T-01 — Plan Mode + Ask Mode _(Curso 1)_
- Usar Plan Mode antes de implementar alguna funcionalidad no trivial.
- La conversación o el plan generado debe quedar documentado.

### T-02 — /init y CLAUDE.md _(Curso 1)_
- El repositorio debe tener un `CLAUDE.md` configurado con contexto del proyecto.
- Debe incluir: stack, convenciones, rutas importantes y restricciones.

### T-03 — Test-driven Iteration _(Curso 1)_
- Documentar al menos un ciclo TDD completo con Claude Code.
- Secuencia requerida: escribir prueba → fallar → implementar → pasar.
- Incluir evidencia en el archivo `EVIDENCE.md`.

### T-04 — Documentation Guidelines _(Curso 1)_
- Generar con Claude el `README.md` principal del proyecto.
- Generar XML comments de al menos una clase del dominio con Claude.

### T-05 — Security _(Curso 1)_
- Solicitar a Claude una revisión de seguridad del endpoint más crítico.
- Incluir los hallazgos o confirmaciones en las evidencias.

### T-06 — GitHub MCP Integration _(Curso 2)_
- Usar la integración de GitHub MCP para al menos una acción real desde Claude Code.
- Acciones válidas: crear un issue, hacer un commit, abrir o revisar un PR.

### T-07 — Custom Skill _(Curso 2)_
- Crear un skill propio útil para el proyecto.
- Ejemplos: generador de entidades DDD, scaffold de handler CQRS, generador de migraciones EF.
- Incluir el archivo del skill en el repositorio.

### T-08 — Custom Hook _(Curso 2)_
- Implementar al menos un hook básico que agregue valor al flujo de trabajo.
- Ejemplos: hook de validación antes de ejecutar un comando, hook de logging de operaciones.

---

## Entregables del Repositorio

- [ ] Código fuente completo
- [ ] Estructura de Clean Architecture respetada
- [ ] Los 3 casos de uso funcionales con endpoints, handlers y tests
- [ ] Base de datos SQLite configurada vía EF Core con migraciones
- [ ] `CLAUDE.md` en la raíz del repositorio
- [ ] `README.md` con instrucciones claras de cómo ejecutar el proyecto
- [ ] `EVIDENCE.md` (o carpeta `/evidence`) con capturas o logs de los 8 temas
- [ ] Ciclo TDD documentado: prueba fallida → implementación → prueba pasando
- [ ] Archivo del Custom Skill (`.md` o el formato que corresponda)
- [ ] Archivo del Hook implementado con comentarios que expliquen su propósito

---

## Criterios de Evaluación

| Criterio | Descripción | Peso |
|----------|-------------|------|
| Funcionalidad | Los 3 endpoints responden correctamente y los tests pasan. | 30% |
| Uso de Claude Code | Evidencia clara y completa de los 8 temas requeridos. | 40% |
| Arquitectura y calidad | Clean Architecture respetada, código legible, tests significativos. | 20% |
| Skill y Hook | El skill y hook creados son prácticos, funcionales y documentados. | 10% |

---

## Nota

El objetivo no es construir un sistema perfecto ni completo. **El objetivo es aprender a trabajar con Claude Code de forma efectiva.** Un proyecto con alcance reducido pero con evidencia sólida vale más que un sistema completo donde Claude fue usado solo como autocompletado.

Se espera que el código sea mayoritariamente generado o guiado por Claude Code. El rol del developer es supervisar, corregir, iterar y tomar decisiones.
