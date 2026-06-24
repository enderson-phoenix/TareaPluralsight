---
description: Genera una entidad de dominio DDD completa con propiedades, método Create estático y XML comments
argument-hint: "NombreEntidad prop1:tipo prop2:tipo ..."
---

Genera una entidad de dominio DDD en C# para el proyecto CalSystem con las siguientes especificaciones:

Entidad y propiedades: $ARGUMENTS

Reglas de generación:
- Namespace: `CalSystem.Domain.Entities`
- Constructor privado (requerido por EF Core): `private NombreEntidad() { }`
- Propiedades con `private set` — nunca `public set`
- Usar `= default!;` para propiedades de tipo string requeridas (evita warnings de nullable)
- Método estático `Create(...)` que recibe los campos como parámetros y retorna la entidad
- Usar `Guid.NewGuid()` para el campo Id
- Incluir `CreatedAt = DateTime.UtcNow` si tiene sentido semánticamente
- XML documentation comments en: clase, todas las propiedades públicas, y el método Create
- El archivo va en: `src/CalSystem.Domain/Entities/{NombreEntidad}.cs`

Formato del output:
1. El código C# completo listo para copiar/usar
2. El path exacto donde va el archivo
3. Si la entidad necesita una migración EF Core, el comando exacto a ejecutar

No agregues explicaciones largas — entrega el código y los pasos concretos.

---

**Ejemplos de uso:**
```
/new-entity Equipment serialNumber:string brand:string calibrationDate:DateTime
/new-entity CalibrationRecord equipmentId:Guid result:string performedAt:DateTime
/new-entity Customer companyName:string contactEmail:string phone:string
```
