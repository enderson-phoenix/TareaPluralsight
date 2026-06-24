---
description: Crea una rama feature, commit, push y abre un Pull Request en GitHub (no commitea a main)
argument-hint: "título del PR (opcional — si se omite, se auto-genera desde los cambios)"
---

Invoca al agente `pr-creator` para crear un Pull Request con los cambios actuales.

El título del PR es:

$ARGUMENTS

Si el argumento está vacío, el agente auto-detecta un título descriptivo desde `git diff`.

## Lo que hará el agente

1. Verifica que hay cambios pendientes (si no los hay, avisa y para)
2. Valida que el build y los tests pasen — si fallan, **no crea el PR**
3. Genera el nombre de rama `feature/<slug-del-titulo>` (o usa la rama actual si ya es feature)
4. Hace commit de todos los cambios pendientes con el título como mensaje
5. Hace push de la rama al remoto
6. Crea el PR en GitHub via `gh pr create` con body estructurado
7. Devuelve la URL del PR creado

## Cuándo usarlo

- Tienes cambios listos y quieres que vayan a revisión antes de mergear a `main`
- Quieres que el historial de `main` solo tenga merges de PRs aprobados
- Trabajas en equipo y necesitas code review antes de integrar

## Ejemplos

```
/crear-pr
/crear-pr Agregar campo Notes a ServiceOrder
/crear-pr Configurar MCP GitHub en el proyecto
/crear-pr Fix: alinear ruta api/service-orders en frontend
```

Procede directamente con la ejecución del agente `pr-creator`.
