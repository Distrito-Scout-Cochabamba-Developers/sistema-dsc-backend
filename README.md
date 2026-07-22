# 🚀 DSC Backend (.NET 10) - Clean Architecture & BMAD Method

Repositorio inicial y configurado para el backend del **Developer Student Community (DSC)**. Construido sobre **.NET 10**, estructurado mediante **Clean Architecture** e integrado con el marco de trabajo **BMAD-METHOD** para maximizar la velocidad y calidad del equipo con asistencia de IA.

---

## 🏛️ Arquitectura del Proyecto (Clean Architecture)

El proyecto está organizado dentro de la carpeta `src/` y `tests/` para mantener una clara separación de responsabilidades y cero acoplamiento hacia la base de datos o infraestructura en la capa central:

```text
DSC/
├── DSC.sln                           # Solución principal de .NET
├── .editorconfig                     # Estándar de código C# para el equipo
├── .gitignore                        # Archivos ignorados por Git
├── README.md                         # Documentación de inicio
├── user_stories_sistema_dsc.md       # Historias de usuario del sistema DSC
├── src/
│   ├── DSC.Domain/                   # 🟢 Núcleo del Dominio (Entidades, Interfaces, Value Objects)
│   ├── DSC.Application/              # 🟡 Casos de Uso, DTOs, Mapeos y Validaciones
│   ├── DSC.Infrastructure/           # 🔵 Persistencia (EF Core, Dapper), Caching, Servicios Externos
│   └── DSC.Api/                      # 🔴 Punto de entrada Web API (Endpoints, Middlewares, AppSettings)
└── tests/
    └── DSC.UnitTests/                # 🧪 Pruebas Unitarias (xUnit, Moq/NSubstitute)
```

### Reglas de Dependencias
- `DSC.Domain`: **Sin dependencias externas.** Solo contiene la lógica pura del negocio.
- `DSC.Application`: Depende únicamente de `DSC.Domain`.
- `DSC.Infrastructure`: Depende de `DSC.Application` y `DSC.Domain`.
- `DSC.Api`: Depende de `DSC.Infrastructure` y `DSC.Application`.
- `DSC.UnitTests`: Depende de `DSC.Application` y `DSC.Domain`.

---

## ⚡ Metodología de Desarrollo: BMAD-METHOD

Para acelerar el desarrollo del backend manteniendo el orden y la calidad del código, el equipo de DSC adopta **[BMAD-METHOD](https://github.com/bmad-code-org/BMAD-METHOD)** (*Breakthrough Method for Agile AI-Driven Development*).

### ¿Qué es BMAD?
BMAD es un marco **Agile Nativo para IA (Agentic Agile Framework)** que transforma la interacción ad-hoc con asistentes de código (como Antigravity, Claude Code, Cursor, Copilot) en un flujo de trabajo estructurado por roles especializados de agentes (Analista, PM, Arquitecto, Desarrollador, Tester).

### Ciclo de Trabajo BMAD en DSC
1. **Breakdown (Desglose):** Se toman las historias de usuario definidas y se dividen en especificidades técnicas manejables por capa.
2. **Map Out (Diseño de Contratos):** Antes de codificar la implementación, se definen las interfaces en `DSC.Domain` y los DTOs/Casos de Uso en `DSC.Application`.
3. **Automate (Desarrollo Asistido):** El desarrollador colabora con el agente de IA para implementar la persistencia en `DSC.Infrastructure` y los endpoints en `DSC.Api`.
4. **Deliver (Entrega y Verificación):** Se ejecutan las pruebas unitarias e integración en `DSC.UnitTests` (`dotnet test`) antes de abrir el Pull Request.

> 💡 **Tip para el equipo:** Consulta la documentación oficial de BMAD en [docs.bmad-method.org](https://docs.bmad-method.org) para aprender cómo usar los roles y flujos agiles guiados por IA.

---

## 🛠️ Comandos de Inicio Rápido

### 1. Restaurar dependencias
```bash
dotnet restore
```

### 2. Compilar la solución
```bash
dotnet build --no-restore
```

### 3. Ejecutar las pruebas unitarias
```bash
dotnet test
```

### 4. Ejecutar la Web API
```bash
dotnet run --project src/DSC.Api
```

Una vez iniciada la API, podrás acceder a los endpoints e interfaz Swagger/OpenAPI en `http://localhost:5000` (o el puerto asignado en la consola).

---

## 🤝 Convenciones de Código para Devs

- **Reglas de código:** Asegúrate de tener habilitado `.editorconfig` en tu IDE (Visual Studio, VS Code, Rider).
- **Formatos de campos:** Utilizar el prefijo `_` para campos privados (ej. `private readonly ILogger _logger;`).
- **Nulabilidad estricta:** El proyecto tiene habilitado `<Nullable>enable</Nullable>`. Evita los advertencias de tipos nulos.
- **Git Branching:** Crea tus ramas como `feature/story-id-breve-descripcion` y realiza PRs hacia la rama `main`.
