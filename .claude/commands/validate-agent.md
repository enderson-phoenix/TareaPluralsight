---
description: Delega la validación completa al agente change-validator en un subcontexto aislado. Usar cuando quieres análisis profundo sin ensuciar el contexto principal de la conversación.
argument-hint: "qué cambió (opcional) — si se omite, auto-detecta desde git"
---

Eres el orquestador de validación de CalSystem. Tu único trabajo en este comando es:
1. Recopilar el contexto del cambio
2. Delegarlo al agente `change-validator`
3. Presentar el resultado limpio al developer

---

## PASO 1 — Recopilar contexto del cambio

**Si "$ARGUMENTS" tiene contenido:** úsalo como contexto. Pasa al PASO 2.

**Si "$ARGUMENTS" está vacío:** detecta qué cambió ejecutando:

```bash
git status --short 2>&1
git diff --name-only 2>&1
git diff --cached --name-only 2>&1
git show --name-only --format="Commit: %s" HEAD 2>&1 | head -15
```

Construye un resumen del cambio: qué archivos, qué tipo (entidad/handler/repositorio/endpoint/migración).

---

## PASO 2 — Delegar al agente change-validator

Invoca el agente `change-validator` usando la herramienta Agent con:
- `subagent_type`: "change-validator"
- `prompt`: el contexto que construiste en el PASO 1, más la instrucción de ejecutar todos los checks relevantes y retornar el reporte completo con tabla PASS/FAIL/WARN y plan de corrección si aplica.

El agente corre en un contexto aislado: sus herramientas y output no contaminan esta conversación.

---

## PASO 3 — Presentar resultado y guardar FIX-PLAN.md si hay fallos

Cuando el agente retorne su resultado:

1. Muestra el reporte completo (tabla de checks + veredicto).

2. **Si el veredicto es BLOQUEADO o ADVERTENCIAS:** usa la herramienta Write para guardar `FIX-PLAN.md` en la raíz del proyecto con el plan de corrección del agente, en formato:

```markdown
# FIX-PLAN — [descripción del problema]

> Generado por `/validate-agent` | Agente: change-validator
> Veredicto: [BLOQUEADO / ADVERTENCIAS]
> Cambio: [qué cambió]

## Pasos para corregir

- [ ] 1. [acción exacta con archivo y comando]
- [ ] 2. ...
- [ ] N. Ejecutar `/validate-agent` para confirmar resolución.

## Notas del desarrollador
<!-- espacio libre -->
```

3. **Si el veredicto es APROBADO** y existe un `FIX-PLAN.md` previo: agrega la línea `✅ Resuelto — todos los checks pasaron.` al final del archivo.

---

**Cuándo usar este comando vs `/validate`:**

| Situación | Usar |
|-----------|------|
| Cambio rápido, quiero feedback inline | `/validate` |
| Cambio grande o crítico, quiero análisis aislado | `/validate-agent` |
| Quiero mantener el contexto de conversación limpio | `/validate-agent` |
| Estoy en una sesión larga y el contexto está lleno | `/validate-agent` |
