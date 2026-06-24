---
id: ctx-006
title: Sistema de agentes expertos + /consult + /crear-pr + MCP
date: 2026-06-24
tags: [tooling,git]
type: tooling
accessed: 0
---

## Resumen
Sistema completo de Claude Code tooling para CalSystem: 9 agentes especializados por capa, orquestador `/consult` con routing automático por keywords/git-diff, agente `pr-creator` para automatizar Pull Requests, configuración GitHub MCP en `.mcp.json`, y sistema de gestión de contexto (`context-manager` + `/ctx-save` + `/ctx-search` + `/ctx-cleanup`). Todo documentado en `.claude/GUIDE.md`.

## Archivos clave
- `.claude/agents/` — 9 agentes: domain, application, infrastructure, api, test, architecture, frontend, change-validator, pr-creator, context-manager
- `.claude/commands/consult.md` — orquestador: routing automático por keywords o flags `@experto`
- `.claude/commands/crear-pr.md` — delegador al agente `pr-creator`
- `.claude/agents/pr-creator.md` — flujo de 7 pasos: validar→rama→commit→push→PR en GitHub
- `.claude/agents/context-manager.md` — crea y mantiene mini-contextos indexados
- `.claude/commands/ctx-save.md` — `/ctx-save [título]` para guardar contexto actual
- `.claude/commands/ctx-search.md` — `/ctx-search [@tag | keyword]` para buscar sin leer todo
- `.claude/commands/ctx-cleanup.md` — archiva contextos > 6 meses (grace period por uso frecuente)
- `.claude/context/INDEX.md` — índice maestro compacto de todos los contextos
- `.mcp.json` — GitHub MCP server (`@modelcontextprotocol/server-github`) para el equipo
- `.claude/settings.json` — 4 hooks + SessionStart para auto-cargar INDEX.md

## Decisiones tomadas
- `pr-creator` bloquea si build o tests fallan antes de crear la rama (no PRs rotos en GitHub)
- INDEX.md diseñado para ser < 50 tokens — siempre cargado en SessionStart sin costo significativo
- Grace period en `ctx-cleanup`: contextos con `accessed >= 3` no se archivan aunque sean viejos
- `.mcp.json` commiteado al repo — toda la equipo hereda la misma config MCP al clonar

## Problemas resueltos
- Hook inline en JSON de settings.json no era mantenible — extraído a `.claude/hooks/build-validator.ps1`
- `pr-creator` detecta correctamente "no hay cambios" sin crear rama vacía

## Relacionado
- [[ctx-001]] — hooks base (Op1-Op5) que este sistema extiende con SessionStart
- [[ctx-002]] — agentes expertos usan el conocimiento de los casos de uso en sus prompts
