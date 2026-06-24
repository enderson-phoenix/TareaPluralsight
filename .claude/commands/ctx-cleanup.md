---
description: Archiva contextos de más de 6 meses al backlog y genera reporte de limpieza
argument-hint: "(sin argumentos) — revisa todos los contextos activos automáticamente"
---

Ejecuta la limpieza del índice de contexto de CalSystem.

## Proceso de limpieza

### PASO 1 — Leer el estado actual

Lee `.claude/context/INDEX.md` y lista todos los contextos en la sección "Activos".
Extrae para cada uno: ID, título, fecha (formato YYYY-MM).

Calcula la fecha límite: hoy - 6 meses.

### PASO 2 — Evaluar cada contexto

Para cada contexto activo, lee su archivo para obtener el campo `accessed` del frontmatter.

Aplica la regla de decisión:

| Condición | Acción |
|-----------|--------|
| `date` >= límite (< 6 meses) | ✅ Mantener activo |
| `date` < límite Y `accessed` < 3 | 📦 Archivar |
| `date` < límite Y `accessed` >= 3 | 🔒 Mantener (grace period por uso frecuente) |

**Grace period:** Un contexto con `accessed >= 3` se usa regularmente — conservarlo aunque sea viejo evita perder conocimiento valioso que el equipo consulta frecuentemente.

### PASO 3 — Ejecutar el archivado

Para cada contexto a archivar:
1. Copia el archivo de `active/` a `archive/` usando Write
2. Actualiza el campo `status` del frontmatter a `archived`

### PASO 4 — Actualizar INDEX.md

1. Mueve las filas de contextos archivados de la tabla "Activos" a "Archivados"
2. Actualiza el contador: `Activos: N | Archivados: N`
3. Actualiza la fecha de actualización

### PASO 5 — Reporte final

```
╔══════════════════════════════════════╗
║    CTX-CLEANUP — CalSystem           ║
╚══════════════════════════════════════╝

Fecha límite: YYYY-MM-DD (6 meses atrás)

| Contexto | Fecha | Accesos | Acción |
|----------|-------|---------|--------|
| ctx-001  | 2025-01 | 1 | 📦 Archivado |
| ctx-002  | 2025-01 | 5 | 🔒 Mantenido (uso frecuente) |
| ctx-003  | 2026-06 | 0 | ✅ Activo |

Archivados:              N contextos
Mantenidos (frecuentes): N contextos  
Activos restantes:       N contextos
```

## Cuándo ejecutarlo

- Una vez al mes como mantenimiento del índice
- Cuando el INDEX.md tiene más de 15 contextos activos
- Antes de onboardear un nuevo miembro al equipo (índice limpio)

## Ejemplo

```
/ctx-cleanup
```
