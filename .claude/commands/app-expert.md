---
description: Invoca directamente al Experto en Application Layer de CalSystem (CQRS, MediatR, Commands, Queries, Handlers, DTOs)
argument-hint: "descripción del caso de uso o handler"
---

Consulta al agente `application-expert` con la siguiente tarea:

$ARGUMENTS

El agente application-expert es el especialista en:
- Commands y Queries (records con IRequest<T>)
- Handlers (IRequestHandler<TRequest, TResponse>)
- DTOs para respuestas de Queries
- MediatR 12 y registro en DependencyInjection.cs
- Decisión: cuándo es Command vs Query, qué retorna cada uno

Procede directamente con la consulta al experto application-expert.
