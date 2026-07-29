# CitasMedicas API

API REST para gestionar doctores, pacientes y citas médicas.

## Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- FluentValidation
- Swagger UI
- xUnit y FluentAssertions

## Arquitectura

```text
Api            → Controllers, Swagger y manejo de errores
Application    → DTOs, validadores, mappers y servicios
Domain         → Entidades, enums e interfaces
Infrastructure → EF Core, SQLite y repositorios
Tests          → Tests unitarios y de integración
```

Los controllers delegan la lógica a los servicios. Los servicios usan interfaces de repositorio y `Infrastructure` contiene las implementaciones con EF Core.

## Requisitos

- .NET SDK 10

Restaurar dependencias y herramientas:

```powershell
dotnet restore
dotnet tool restore
```

## Base de datos

La API usa SQLite con la base de datos `Api/Data/citasmedicas.db`.

Crear y aplicar migraciones:

```powershell
dotnet tool run dotnet-ef migrations add NombreMigracion `
  --project .\Infrastructure\Infrastructure.csproj `
  --startup-project .\Api\Api.csproj `
  --output-dir Migrations

dotnet tool run dotnet-ef database update `
  --project .\Infrastructure\Infrastructure.csproj `
  --startup-project .\Api\Api.csproj
```

## Ejecutar

```powershell
dotnet run --project .\Api\Api.csproj
```

En Development, Swagger UI está disponible en:

```text
https://localhost:<puerto>/swagger
```

## Endpoints principales

| Recurso | Endpoints |
|---|---|
| Doctores | `POST /api/doctores`, `GET /api/doctores`, `GET /api/doctores/{id}`, `PATCH /api/doctores/{id}/desactivar` |
| Pacientes | `POST /api/pacientes`, `GET /api/pacientes`, `GET /api/pacientes/{id}` |
| Citas | `POST /api/citas`, `GET /api/citas`, `GET /api/citas/{id}`, `PATCH /api/citas/{id}/cancelar` |

`GET /api/citas` admite los filtros `doctorId`, `pacienteId` y `estado`.

## Reglas principales

- El documento del paciente es único.
- Las citas requieren doctor y paciente activos.
- Las citas deben tener fecha futura y no pueden solaparse.
- Un paciente puede tener como máximo tres citas programadas.
- Un doctor con citas futuras no puede desactivarse.
- Solo se pueden cancelar citas programadas antes de su inicio.

## Tests

Ejecutar toda la suite:

```powershell
dotnet test CitasMedicas.slnx
```

Incluye tests unitarios de servicios, tests de repositorios con SQLite en memoria y tests HTTP de la API con `WebApplicationFactory`.
