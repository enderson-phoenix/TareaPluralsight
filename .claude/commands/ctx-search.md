---
description: Busca en el índice de contexto por keyword o @tag y carga solo los archivos relevantes
argument-hint: "keyword o @tag — ej: 'technician', '@tooling', 'EF Core'"
---

Busca en el índice de contexto de CalSystem sin cargar todos los archivos.

Término de búsqueda:

$ARGUMENTS

## Proceso de búsqueda

### PASO 1 — Leer solo el INDEX.md

Lee `.claude/context/INDEX.md` (ultra-compacto, < 50 tokens).

### PASO 2 — Filtrar

Si `$ARGUMENTS` empieza con `@`:
- Es una búsqueda por tag — filtra filas cuya columna "Tags" contenga ese tag
- Ejemplo: `@tooling` → solo filas con `tooling` en tags

Si `$ARGUMENTS` es texto libre:
- Filtra filas cuyo Título, Tags o Tipo contengan el texto (case-insensitive)
- Ejemplo: `EF Core` → filas que mencionen EF Core en cualquier columna

Si `$ARGUMENTS` está vacío:
- Muestra el INDEX.md completo para navegación manual

### PASO 3 — Cargar contextos que matchearon

Para cada fila que pasó el filtro:
1. Lee el archivo de contexto completo
2. Incrementa el campo `accessed` en el frontmatter del archivo (+1)
3. Muestra el contenido con un encabezado separador

### PASO 4 — Reporte

```
Búsqueda: "<término>"
Encontrados: N de M contextos

--- ctx-NNN: Título ---
[contenido del archivo de contexto]

--- ctx-NNN: Título ---
[contenido del archivo de contexto]

---
Para ver todos: /ctx-search (sin argumentos)
Para guardar nuevo: /ctx-save [título]
```

Si no hay matches → muestra el INDEX.md completo y sugiere términos alternativos.

## Ahorro de tokens

Sin este comando, Claude leería 10-20 archivos del proyecto para entender el contexto.
Con este comando lee solo INDEX.md (1 archivo) y luego solo el contexto específico.

## Ejemplos

```
/ctx-search technician
/ctx-search @tooling
/ctx-search @domain
/ctx-search EF Core migrations
/ctx-search @frontend
/ctx-search @bugfix
/ctx-search orders
```
