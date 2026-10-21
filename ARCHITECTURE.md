# 🏛️ Guía de Arquitectura de Software - DSC Backend (.NET 10)

¡Bienvenido al equipo de desarrollo del backend de **Distrito Scout Cochabamba (DSC)**!

Este documento fue diseñado para que **todos los desarrolladores del equipo** comprendan la arquitectura de la solución, sus principios, la separación de responsabilidades y la guía paso a paso de cómo implementar una nueva funcionalidad.

---

## 🎯 ¿Por qué esta arquitectura?

El backend de DSC es un **producto enterprise listo para producción (no es un MVP básico)**. Buscamos un sistema **escalable, altamente mantenible, testeable y libre de acoplamiento rígido a la base de datos o marcos de trabajo**.

Para lograrlo, combinamos dos enfoques fundamentales:

1. **Clean Architecture (Arquitectura Limpia)**: Define la estructura física de capas del proyecto en C# y la **Regla de Dependencias** (el núcleo de negocio no depende de nada externo).
2. **Domain-Driven Design (DDD)**: Define cómo modelamos la complejidad del negocio Scout (reglas, entidades, agregados y eventos).

---

## 🧱 Estructura de Proyectos y Capas

La solución de .NET está dividida en 4 capas concéntricas principales ubicadas en `src/`:

```text
src/
├── DSC.Domain/         🟢 Capa Interna (Núcleo Puro de Negocio)
├── DSC.Application/    🟡 Capa de Casos de Uso y Orquestación
├── DSC.Infrastructure/ 🔵 Capa de Persistencia (EF Core) y Adaptadores Técnicos
└── DSC.Api/            🔴 Capa de Entrada HTTP REST / Web API
```

### 🚨 La Regla de Oro de Dependencias

> **Las dependencias avanzan estrictamente de afuera hacia adentro.**  
> `DSC.Domain` no conoce a NINGÚN otro proyecto. `DSC.Application` solo conoce a `DSC.Domain`. `DSC.Infrastructure` e `DSC.Api` dependen de las capas internas.

```text
  [ DSC.Api ] ──► [ DSC.Application ] ──► [ DSC.Domain ] (Capa Central)
                       ▲
  [ DSC.Infrastructure ] ┘
```

---

### 🔍 Responsabilidades por Capa

#### 1. 🟢 `DSC.Domain` (Dominio)

- **Contenido**: Entidades (`Entities`), Raíces de Agregado (`Aggregate Roots`), Objetos de Valor (`Value Objects`), Excepciones de Dominio, Eventos de Dominio (`Domain Events`) e Interfaces de Repositorios (`IRepository`).
- **Reglas**:
    - **Cero paquetes de infraestructura** (Sin Entity Framework Core, sin Newtonsoft.Json, sin Microsoft.AspNetCore).
    - Encapsulamiento estricto: Propiedades con setters `private` o `init`. Toda mutación de estado ocurre mediante métodos de negocio que validan invariantes.

#### 2. 🟡 `DSC.Application` (Aplicación)

- **Contenido**: Casos de Uso (Handlers de Comandos y Consultas / CQRS), DTOs, Contratos e Interfaces de Servicios Externos (`IEmailSender`, etc.).
- **Reglas**:
    - Orquesta la ejecución llamando a los repositorios (interfaces) y métodos del dominio.
    - Define DTOs de entrada y salida (usando C# `record`).

#### 3. 🔵 `DSC.Infrastructure` (Infraestructura)

- **Contenido**: `ScoutDbContext` de Entity Framework Core 10, configuraciones de tablas (`IEntityTypeConfiguration<T>`), Repositorios Concretos (`ScoutMemberRepository.cs`), clientes HTTP externos.
- **Reglas**:
    - Mapea los datos de la BD a las Entidades de Dominio.
    - Toda consulta de lectura debe usar `.AsNoTracking()` o `.Select()`.

#### 4. 🔴 `DSC.Api` (Presentación / Web API)

- **Contenido**: Controladores / Endpoints REST, Middlewares (Manejo de excepciones con RFC 7807 Problem Details), Swagger y configuración de Inyección de Dependencias en `Program.cs`.

---

## 🛠️ Ejemplo Práctico Completo: Implementando un Caso de Uso

Imaginemos el caso de uso: **"Registrar un nuevo Miembro Scout"**. A continuación se muestra cómo se implementa en cada capa:

### 1. Capa `DSC.Domain`

#### A. Objeto de Valor (`Value Object`): `CiNumber.cs`

Un objeto de valor es inmutable y autodocumentado:

```csharp
namespace DSC.Domain.ValueObjects;

public record class CiNumber
{
    public string Value {; }

    public CiNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 5)
            throw new DomainException("El número de CI no es válido.");

        Value = value.Trim();
    }
}
```

#### B. Raíz del Agregado (`Aggregate Root`): `ScoutMember.cs`

Entidad que protege sus propias reglas de negocio:

```csharp
namespace DSC.Domain.Entities;

using DSC.Domain.ValueObjects;

public class ScoutMember
{
    public Guid Id {; private set; }
    public string FirstName {; private set; } = string.Empty;
    public string LastName {; private set; } = string.Empty;
    public CiNumber Ci {; private set; } = null!;
    public DateTime BirthDate {; private set; }
    public bool IsActive {; private set; }

    // Constructor privado para EF Core
    private ScoutMember() { }

    // Método de fábrica con validación de negocio (Mayoría de edad / Invariantes Scouts)
    public static ScoutMember Create(string firstName, string lastName, CiNumber ci, DateTime birthDate)
    {
        var age = DateTime.UtcNow.Year - birthDate.Year;
        if (age < 5)
            throw new DomainException("Un miembro scout debe tener al menos 5 años.");

        return new ScoutMember
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Ci = ci,
            BirthDate = birthDate,
            IsActive = true
        };
    }
}
```

#### C. Interfaz del Repositorio: `IScoutMemberRepository.cs`

```csharp
namespace DSC.Domain.Repositories;

using DSC.Domain.Entities;

public interface IScoutMemberRepository
{
    Task AddAsync(ScoutMember member, CancellationToken cancellationToken);
    Task<bool> ExistsCiAsync(string ciValue, CancellationToken cancellationToken);
}
```

---

### 2. Capa `DSC.Application`

#### DTO y Caso de Uso (Handler): `RegisterScoutMemberHandler.cs`

```csharp
namespace DSC.Application.UseCases.ScoutMembers;

using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

public record RegisterScoutMemberCommand(
    string FirstName,
    string LastName,
    string Ci,
    DateTime BirthDate
);

public class RegisterScoutMemberHandler(IScoutMemberRepository repository)
{
    public async Task<Guid> HandleAsync(RegisterScoutMemberCommand command, CancellationToken cancellationToken)
    {
        var ci = new CiNumber(command.Ci);

        if (await repository.ExistsCiAsync(ci.Value, cancellationToken))
            throw new InvalidOperationException("Ya existe un miembro registrado con este CI.");

        var member = ScoutMember.Create(command.FirstName, command.LastName, ci, command.BirthDate);

        await repository.AddAsync(member, cancellationToken);
        return member.Id;
    }
}
```

---

### 3. Capa `DSC.Infrastructure`

#### Mapeo EF Core (`IEntityTypeConfiguration`): `ScoutMemberConfiguration.cs`

Separamos el mapeo de BD de la clase C#:

```csharp
namespace DSC.Infrastructure.Persistence.Configurations;

using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ScoutMemberConfiguration : IEntityTypeConfiguration<ScoutMember>
{
    public void Configure(EntityTypeBuilder<ScoutMember> builder)
    {
        builder.ToTable("ScoutMembers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();

        // Mapeo de Value Object
        builder.Property(x => x.Ci)
            .HasConversion(ci => ci.Value, v => new Domain.ValueObjects.CiNumber(v))
            .HasMaxLength(20)
            .IsRequired();
    }
}
```

#### Repositorio Concreto: `ScoutMemberRepository.cs`

```csharp
namespace DSC.Infrastructure.Persistence.Repositories;

using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public class ScoutMemberRepository(ScoutDbContext dbContext) : IScoutMemberRepository
{
    public async Task AddAsync(ScoutMember member, CancellationToken cancellationToken)
    {
        await dbContext.ScoutMembers.AddAsync(member, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsCiAsync(string ciValue, CancellationToken cancellationToken)
    {
        return await dbContext.ScoutMembers
            .AsNoTracking() // ⚡ Optimización de lectura
            .AnyAsync(x => x.Ci.Value == ciValue, cancellationToken);
    }
}
```

---

### 4. Capa `DSC.Api`

#### Endpoint / Controller: `ScoutMembersController.cs`

```csharp
namespace DSC.Api.Controllers;

using DSC.Application.UseCases.ScoutMembers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/scout-members")]
public class ScoutMembersController(RegisterScoutMemberHandler handler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterScoutMemberCommand command, CancellationToken cancellationToken)
    {
        var memberId = await handler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = memberId }, new { Id = memberId });
    }
}
```

---

## ⚡ Estándares de Código y Rendimiento en .NET 10

1. **Primary Constructors**: Inyectar dependencias directamente en la firma de la clase (`public class MyHandler(IRepository repo)`).
2. **Immutabilidad (`record`)**: Usar `record` para todos los DTOs y Objetos de Valor.
3. **Propagación de CancellationToken**: Pasar `CancellationToken` a todos los métodos asíncronos (`Async`).
4. **Optimización de Lecturas**: Toda consulta LINQ de lectura DEBE incluir `.AsNoTracking()` o `.Select()`.
5. **Inyección de Dependencias**: No inyectar `DbContext` ni repositorios en servicios `Singleton`.

---

## 📚 Documentación de features

Contratos y cambios recientes (catálogo de módulos, catálogos que exige el POST de asistencia) viven en [`docs/`](./docs/README.md). Este archivo sigue siendo la guía de capas y dependencias.

---

## 🧪 Estrategia de Testing (xUnit)

1. **Pruebas Unitarias (`tests/DSC.UnitTests`)**:
    - Probar el dominio (`ScoutMember.Create`) verificando que lance excepciones si las reglas no se cumplen.
    - Probar el Handler (`RegisterScoutMemberHandler`) utilizando Mocks de `IScoutMemberRepository`.
2. **Pruebas de Integración**:
    - Probar `ScoutMemberRepository` contra EF Core.
3. **Pruebas E2E (End-to-End)**:
    - Se crearán al completar módulos funcionales enteros.
