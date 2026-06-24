---
id: ctx-001
title: Setup inicial: Clean Architecture + EF Core + Hooks
date: 2026-06-24
tags: [infra,arch,tooling]
type: setup
accessed: 0
---

## Resumen
Configuración inicial completa del proyecto CalSystem para Phoenix Calibration DR. Se estructuró en 5 proyectos siguiendo Clean Architecture (Domain → Application → Infrastructure → Api → Web). La base de datos usa EF Core 10 + SQLite con migraciones formales (`Migrate()`, no `EnsureCreated`). Se configuraron 4 hooks automáticos en `.claude/settings.json` para validación continua de calidad.

## Archivos clave
- `src/CalSystem.Api/Program.cs` — configuración de DI, CORS (5200), migración automática, Swagger
- `src/CalSystem.Infrastructure/Persistence/AppDbContext.cs` — DbContext con mapping de entidades
- `src/CalSystem.Infrastructure/Migrations/20260624140627_InitialCreate.cs` — migración inicial
- `.claude/settings.json` — 4 hooks: PostToolUse build, PreToolUse pre-commit, Stop background
- `.claude/hooks/build-validator.ps1` — script PowerShell del hook de build automático
- `Directory.Build.props` — supresión de advisory SQLitePCLRaw GHSA-2m69-gcr7-jv3q (sin fix upstream)

## Decisiones tomadas
- `Migrate()` en lugar de `EnsureCreated()` para soportar migraciones en producción correctamente
- 4 proyectos .NET + 1 test project separado: Domain, Application, Infrastructure, Api, Web
- CORS configurado solo para http://localhost:5200 (frontend Blazor WASM)
- SQLitePCLRaw advisory suprimido en `Directory.Build.props` porque no hay versión corregida disponible

## Problemas resueltos
- `DatabaseFacade.Migrate()` requería `using Microsoft.EntityFrameworkCore;` en Program.cs
- Hook inline en JSON causaba errores de parsing — se extrajo a `.claude/hooks/build-validator.ps1`
- Corchetes `[]` en PowerShell string causaban error de array — se escaparon con backtick

## Relacionado
- [[ctx-002]] — casos de uso (usa esta infraestructura)
- [[ctx-006]] — tooling de Claude Code (hooks documentados aquí)
