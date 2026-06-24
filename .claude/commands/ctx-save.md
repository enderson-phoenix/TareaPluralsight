---
description: Guarda el contexto del trabajo actual como un mini-contexto indexado en .claude/context/
argument-hint: "[título] o [@tipo título] — omitir para auto-detectar desde git"
---

Invoca al agente `context-manager` para guardar el contexto del trabajo actual.

El título e instrucciones recibidas son:

$ARGUMENTS

## Comportamiento según argumentos

**Sin argumentos (`/ctx-save`):**
El agente infiere el título desde el último commit (`git log -1 --pretty=%s`) y detecta
automáticamente el tipo y los tags según los archivos modificados.

**Con título (`/ctx-save Gestión de técnicos end-to-end`):**
Usa ese texto como título del contexto. El agente detecta tipo y tags automáticamente.

**Con tipo prefijado (`/ctx-save @tooling Nuevo agente pr-creator`):**
El `@tipo` fuerza el tipo del contexto (`@feature`, `@bugfix`, `@decision`, `@tooling`, `@setup`).
El agente deduce los tags desde los archivos afectados.

## Lo que hará el agente

1. Lee `git log` y `git diff` para entender qué cambió
2. Lee los archivos clave afectados (máx 5) para extraer decisiones y detalles
3. Determina el siguiente ID disponible en `.claude/context/INDEX.md`
4. Crea `.claude/context/active/ctx-NNN-slug.md` con resumen estructurado
5. Actualiza `INDEX.md` — agrega una fila y actualiza el contador
6. Confirma con el ID asignado y los tags detectados

## Cuándo usarlo

- Después de completar una feature, fix o cambio importante
- Cuando quieres que Claude recuerde el contexto en sesiones futuras
- Antes de cerrar una sesión larga de trabajo
- Antes de hacer `/crear-pr` para que el historial quede documentado

## Ejemplos

```
/ctx-save
/ctx-save Gestión de técnicos end-to-end
/ctx-save @tooling Sistema de agentes expertos y orquestador /consult
/ctx-save @bugfix Fix: ruta api/service-orders desalineada en frontend
/ctx-save @decision Usar Migrate() en lugar de EnsureCreated para producción
```

Procede directamente invocando al agente `context-manager` con los argumentos recibidos.
