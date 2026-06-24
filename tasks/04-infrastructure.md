# Fase 4 — Capa de Infraestructura

**Prerequisito:** Fases 2 y 3 completadas.  
**Resultado esperado:** Base de datos SQLite configurada, `AppDbContext` funcionando y repositorio implementado con EF Core.

---

## 4.1 Crear el `AppDbContext`

El contexto de EF Core mapea las entidades del dominio a tablas de la base de datos.

**Archivo:** `src/CalSystem.Infrastructure/Persistence/AppDbContext.cs`

```csharp
using CalSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<Technician> Technicians => Set<Technician>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Equipment).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ProblemDescription).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
        });
    }
}
```

> `HasConversion<string>()` guarda el enum `OrderStatus` como texto en SQLite ("Pending", "InProgress", "Closed") en lugar de un número. Más legible si abres la base de datos con un visor.

---

## 4.2 Implementar el repositorio

**Archivo:** `src/CalSystem.Infrastructure/Persistence/Repositories/ServiceOrderRepository.cs`

```csharp
using CalSystem.Domain.Entities;
using CalSystem.Domain.Enums;
using CalSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CalSystem.Infrastructure.Persistence.Repositories;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly AppDbContext _context;

    public ServiceOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ServiceOrder order)
    {
        await _context.ServiceOrders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id)
    {
        return await _context.ServiceOrders.FindAsync(id);
    }

    public async Task<IEnumerable<ServiceOrder>> GetByStatusAsync(OrderStatus status)
    {
        return await _context.ServiceOrders
            .Where(o => o.Status == status)
            .ToListAsync();
    }

    public async Task UpdateAsync(ServiceOrder order)
    {
        _context.ServiceOrders.Update(order);
        await _context.SaveChangesAsync();
    }
}
```

---

## 4.3 Configurar la inyección de dependencias de Infrastructure

**Archivo:** `src/CalSystem.Infrastructure/DependencyInjection.cs`

```csharp
using CalSystem.Domain.Interfaces;
using CalSystem.Infrastructure.Persistence;
using CalSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CalSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();

        return services;
    }
}
```

---

## 4.4 Configurar la Api para usar Infrastructure

### Agregar la connection string
**Archivo:** `src/CalSystem.Api/appsettings.json` — agregar:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=calsystem.db"
  }
}
```

### Registrar en Program.cs
**Archivo:** `src/CalSystem.Api/Program.cs` — agregar antes de `builder.Build()`:

```csharp
using CalSystem.Application;
using CalSystem.Infrastructure;

builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!
);
```

---

## 4.5 Crear y aplicar la migración inicial

EF Core necesita las herramientas globales instaladas. Si no las tienes:

```bash
dotnet tool install --global dotnet-ef
```

Luego, desde la raíz de la solución:

```bash
# Crear la migración (el --project es Infrastructure, el --startup-project es la Api)
dotnet ef migrations add InitialCreate \
  --project src/CalSystem.Infrastructure \
  --startup-project src/CalSystem.Api

# Aplicar la migración (crea el archivo .db)
dotnet ef database update \
  --project src/CalSystem.Infrastructure \
  --startup-project src/CalSystem.Api
```

Esto crea la carpeta `Migrations/` dentro de Infrastructure y el archivo `calsystem.db` en la Api.

> Agrega `calsystem.db` al `.gitignore` si no lo hiciste en la Fase 1.

---

## 4.6 Verificar

```bash
dotnet build
```

Si compila sin errores, la configuración de EF Core es correcta. Puedes verificar la base de datos con cualquier visor de SQLite (como DB Browser for SQLite).

---

## Checklist

- [ ] `AppDbContext` con `DbSet<ServiceOrder>` y `DbSet<Technician>`
- [ ] `ServiceOrderRepository` implementa `IServiceOrderRepository`
- [ ] `DependencyInjection.cs` en Infrastructure registra el contexto y el repositorio
- [ ] `appsettings.json` tiene la connection string de SQLite
- [ ] `Program.cs` llama `AddApplication()` y `AddInfrastructure(...)`
- [ ] Migración `InitialCreate` creada en `Migrations/`
- [ ] `dotnet build` sin errores

**Siguiente:** [`05-api.md`](05-api.md)
