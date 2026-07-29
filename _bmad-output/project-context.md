---
project_name: 'Distrito Scout Cochabamba - Sistema Backend'
user_name: 'DSC-Software-Team'
date: '2026-07-28'
sections_completed: ['technology_stack', 'architecture_rules', 'code_quality_rules', 'efcore_rules', 'testing_rules']
existing_patterns_found: 12
---

# Project Context & AI Implementation Rules

_Este archivo contiene las reglas críticas y patrones de arquitectura que los agentes de IA deben seguir estrictamente al generar o modificar código en este proyecto._

---

## 1. Stack Tecnológico y Versiones

- **Lenguaje**: C# 14 (.NET 10 SDK)
- **Web Framework**: ASP.NET Core Web API 10.0
- **ORM / Persistencia**: Entity Framework Core 10.0 (EF Core)
- **Testing**: xUnit 2.9.3, Microsoft.NET.Test.Sdk 17.14.1, Coverlet 6.0.4
- **Documentación API**: Microsoft.AspNetCore.OpenApi 10.0.5

---

## 2. Arquitectura del Sistema (Clean Architecture + Domain-Driven Design)

- **Objetivo**: Producto enterprise escalable, mantenible y robusto listo para producción (NO es un MVP).
- **Regla Estricta de Capas**:
  - `DSC.Domain`: Capa interna pura sin dependencias externas ni de frameworks.
    - Contiene Entidades (`Entities`), Raíces de Agregado (`Aggregate Roots`), Objetos de Valor (`Value Objects`), Excepciones de Dominio, Eventos de Dominio (`DomainEvents`) y las Interfaces de Repositorio (`IRepository`).
    - Las propiedades de las entidades deben tener setters privados/protegidos, exponiendo métodos de negocio que validen invariantes.
  - `DSC.Application`: Capa de casos de uso y lógica de aplicación.
    - Contiene Commands, Queries, Handlers, DTOs, Validadores e interfaces de servicios técnicos (`IEmailSender`, etc.).
  - `DSC.Infrastructure`: Implementaciones técnicas y acceso a datos.
    - Contiene el `DbContext` de EF Core, clases de configuración `IEntityTypeConfiguration<T>`, repositorios concretos y adaptadores externos.
  - `DSC.Api`: Punto de entrada HTTP REST.
    - Controllers / Minimal APIs, Middlewares (Manejo global de errores con RFC 7807 Problem Details), Swagger y registro de Inyección de Dependencias.

---

## 3. Principios de Código Limpio y Buenas Prácticas (.NET 10 / C# 14)

- **Tipos e Inmutabilidad**:
  - Utilizar `record class` y `record struct` para DTOs, comandos, respuestas y Objetos de Valor de Dominio.
  - Usar expresiones `with` para clonación y mutaciones no destructivas en Objetos de Valor.
  - Aprovechar **Primary Constructors** (`public class MyService(IDbContext db)`) para inyección de dependencias limpia y concisa.
- **Programación Asíncrona Eficiente**:
  - Propagar siempre `CancellationToken` a lo largo de toda la cadena de llamadas asíncronas.
  - Usar `ValueTask<T>` en métodos que frecuentemente completan de forma síncrona o provienen de caché.
  - Utilizar `IAsyncEnumerable<T>` (`await foreach`) para streaming asíncrono de grandes colecciones.
- **Inyección de Dependencias Limpia**:
  - Jamás inyectar un servicio `Scoped` (como `DbContext`) dentro de un servicio `Singleton` (evitar *Captive Dependencies*).

---

## 4. Patrones de Persistencia con EF Core 10

- **Consultas de Solo Lectura**:
  - Aplicar siempre `.AsNoTracking()` o proyecciones explícitas `.Select()` en consultas de lectura para eliminar la sobrecarga de seguimiento de cambios.
- **Prevención de Explosión Cartesiana**:
  - Utilizar `.AsSplitQuery()` cuando una consulta incluya múltiples relaciones de colección (`.Include()`).
- **Operaciones Masivas / Lotes**:
  - Utilizar `ExecuteUpdateAsync` y `ExecuteDeleteAsync` para actualizaciones masivas directas en la base de datos sin cargar grafos de entidades en memoria.
- **Encapsulamiento del Esquema**:
  - Definir las configuraciones de tablas y relaciones exclusivamente en clases `IEntityTypeConfiguration<T>` en `DSC.Infrastructure`. Las entidades de `DSC.Domain` no deben tener atributos de persistenica (`[Key]`, `[Table]`).

---

## 5. Estrategia de Testing (xUnit)

- **Unidades e Invariantes de Dominio**:
  - Cobertura exhaustiva con `xUnit` en la capa `DSC.Domain` probando reglas de negocio, validaciones y cambios de estado en Agregados.
- **Casos de Uso (Application)**:
  - Pruebas unitarias de Handlers y Servicios simulando repositorios mediante Mocks/Stubs.
- **Integración (Infrastructure)**:
  - Pruebas de repositorios y mapeos de EF Core contra base de datos en memoria / contenedores de prueba.
- **Pruebas End-to-End (E2E)**:
  - Se incorporarán de manera progresiva únicamente cuando un módulo funcional esté completo y estabilizado.
