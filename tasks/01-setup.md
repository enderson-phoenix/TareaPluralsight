# Fase 1 — Configuración del Proyecto

**Prerequisito:** Ninguno — este es el punto de partida.  
**Resultado esperado:** Solución .NET que compila, con estructura Clean Architecture y repositorio en GitHub.

---

## 1.1 Crear la estructura de la solución

Clean Architecture divide el código en capas con dependencias en una sola dirección:
`Api → Application → Domain` (Infrastructure implementa las interfaces definidas en Application).

Ejecuta estos comandos en la carpeta raíz del proyecto:

```bash
# Crear la solución
dotnet new sln -n CalSystem

# Crear los proyectos de cada capa
dotnet new classlib -n CalSystem.Domain         -o src/CalSystem.Domain
dotnet new classlib -n CalSystem.Application    -o src/CalSystem.Application
dotnet new classlib -n CalSystem.Infrastructure -o src/CalSystem.Infrastructure
dotnet new webapi   -n CalSystem.Api            -o src/CalSystem.Api

# Crear el proyecto de pruebas
dotnet new xunit    -n CalSystem.Tests          -o tests/CalSystem.Tests

# Agregar todos a la solución
dotnet sln add src/CalSystem.Domain/CalSystem.Domain.csproj
dotnet sln add src/CalSystem.Application/CalSystem.Application.csproj
dotnet sln add src/CalSystem.Infrastructure/CalSystem.Infrastructure.csproj
dotnet sln add src/CalSystem.Api/CalSystem.Api.csproj
dotnet sln add tests/CalSystem.Tests/CalSystem.Tests.csproj
```

**Estructura esperada:**
```
CalSystem/
  src/
    CalSystem.Domain/
    CalSystem.Application/
    CalSystem.Infrastructure/
    CalSystem.Api/
  tests/
    CalSystem.Tests/
  CalSystem.sln
```

---

## 1.2 Configurar referencias entre proyectos

```bash
# Application conoce el Domain
dotnet add src/CalSystem.Application reference src/CalSystem.Domain

# Infrastructure implementa interfaces de Application y Domain
dotnet add src/CalSystem.Infrastructure reference src/CalSystem.Application
dotnet add src/CalSystem.Infrastructure reference src/CalSystem.Domain

# Api orquesta todo
dotnet add src/CalSystem.Api reference src/CalSystem.Application
dotnet add src/CalSystem.Api reference src/CalSystem.Infrastructure

# Tests necesita Application y Domain para probar handlers
dotnet add tests/CalSystem.Tests reference src/CalSystem.Application
dotnet add tests/CalSystem.Tests reference src/CalSystem.Domain
```

---

## 1.3 Instalar paquetes NuGet

```bash
# EF Core con SQLite (en Infrastructure)
dotnet add src/CalSystem.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/CalSystem.Infrastructure package Microsoft.EntityFrameworkCore.Design

# MediatR para CQRS (en Application)
dotnet add src/CalSystem.Application package MediatR
dotnet add src/CalSystem.Application package MediatR.Extensions.Microsoft.DependencyInjection

# EF Core tools en la API (necesario para correr migraciones)
dotnet add src/CalSystem.Api package Microsoft.EntityFrameworkCore.Design

# Herramientas de prueba
dotnet add tests/CalSystem.Tests package Moq
dotnet add tests/CalSystem.Tests package FluentAssertions
```

> `FluentAssertions` hace los asserts más legibles:
> `result.Should().NotBeNull()` en lugar de `Assert.NotNull(result)`.

---

## 1.4 Inicializar el repositorio Git y GitHub

Crea primero el `.gitignore` para .NET. Contenido mínimo:

```
bin/
obj/
*.user
.vs/
.idea/
*.db
*.db-shm
*.db-wal
appsettings.Development.json
```

Luego:

```bash
git init
git add .
git commit -m "chore: initial project structure with Clean Architecture"
```

Crea el repositorio en GitHub y enlázalo:

```bash
git remote add origin https://github.com/TU_USUARIO/calsystem.git
git push -u origin main
```

---

## 1.5 Verificar que todo compila

```bash
dotnet build
dotnet test   # 0 pruebas encontradas es exitoso en este punto
```

Si hay errores, casi siempre son de paquetes no restaurados. Ejecuta `dotnet restore` primero.

---

## Checklist

- [ ] `dotnet build` sin errores
- [ ] `dotnet test` sin errores
- [ ] 5 proyectos visibles en la solución
- [ ] Repositorio en GitHub con primer commit
- [ ] `.gitignore` cubre `bin/`, `obj/`, `*.db`

**Siguiente:** [`02-domain.md`](02-domain.md)
