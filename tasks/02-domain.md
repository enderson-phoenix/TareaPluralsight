# Fase 2 — Capa de Dominio

**Prerequisito:** Fase 1 completada.  
**Resultado esperado:** Entidades, enums e interfaces de repositorio en `CalSystem.Domain`. Sin referencias a EF Core ni a ningún framework externo.

> **Regla clave de Clean Architecture:** el Domain no depende de nada. Es el núcleo del sistema.

---

## 2.1 Crear el enum `OrderStatus`

**Archivo:** `src/CalSystem.Domain/Enums/OrderStatus.cs`

```csharp
namespace CalSystem.Domain.Enums;

public enum OrderStatus
{
    Pending,
    InProgress,
    Closed
}
```

---

## 2.2 Crear la entidad `Technician`

**Archivo:** `src/CalSystem.Domain/Entities/Technician.cs`

```csharp
namespace CalSystem.Domain.Entities;

public class Technician
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    private Technician() { }  // requerido por EF Core

    public static Technician Create(string name, string email)
    {
        return new Technician
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email
        };
    }
}
```

---

## 2.3 Crear la entidad `ServiceOrder`

Esta es la entidad central del sistema. Contiene toda la lógica de negocio de una orden.

**Archivo:** `src/CalSystem.Domain/Entities/ServiceOrder.cs`

```csharp
using CalSystem.Domain.Enums;

namespace CalSystem.Domain.Entities;

public class ServiceOrder
{
    public Guid Id { get; private set; }
    public string CustomerName { get; private set; }
    public string Equipment { get; private set; }
    public string ProblemDescription { get; private set; }
    public OrderStatus Status { get; private set; }
    public Guid? TechnicianId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ServiceOrder() { }  // requerido por EF Core

    public static ServiceOrder Create(string customerName, string equipment, string problemDescription)
    {
        return new ServiceOrder
        {
            Id = Guid.NewGuid(),
            CustomerName = customerName,
            Equipment = equipment,
            ProblemDescription = problemDescription,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AssignTechnician(Guid technicianId)
    {
        TechnicianId = technicianId;
        Status = OrderStatus.InProgress;
    }

    public void Close()
    {
        Status = OrderStatus.Closed;
    }
}
```

> **Por qué `private set`?** Previene que código externo modifique el estado directamente.
> Solo los métodos de la entidad (`AssignTechnician`, `Close`) pueden cambiar el estado.
> Esto es encapsulamiento de dominio.

---

## 2.4 Definir la interfaz del repositorio

Las interfaces van en el Domain para que Application pueda usarlas sin conocer la implementación.

**Archivo:** `src/CalSystem.Domain/Interfaces/IServiceOrderRepository.cs`

```csharp
using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;

namespace CalSystem.Domain.Interfaces;

public interface IServiceOrderRepository
{
    Task AddAsync(ServiceOrder order);
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceOrder>> GetByStatusAsync(OrderStatus status);
    Task UpdateAsync(ServiceOrder order);
}
```

---

## 2.5 Verificar estructura final

```
src/CalSystem.Domain/
  Entities/
    ServiceOrder.cs
    Technician.cs
  Enums/
    OrderStatus.cs
  Interfaces/
    IServiceOrderRepository.cs
```

```bash
dotnet build src/CalSystem.Domain
```

No debe haber errores ni warnings. El proyecto Domain tampoco debe tener referencias en su `.csproj` (es la capa más interna).

---

## Checklist

- [ ] `OrderStatus` con 3 valores: `Pending`, `InProgress`, `Closed`
- [ ] `ServiceOrder` con método `Create(...)` y `AssignTechnician(...)`
- [ ] `Technician` con método `Create(...)`
- [ ] `IServiceOrderRepository` con 4 métodos
- [ ] `dotnet build` sin errores

**Siguiente:** [`03-application.md`](03-application.md)
