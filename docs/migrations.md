# Migraciones de EF Core

Ejecuta estos comandos desde la raíz del repositorio. EF Core usa la API como proyecto de inicio y lee `ConnectionStrings:SyncForge` de `API/appsettings.Development.json`. En PowerShell, selecciona el entorno de desarrollo antes de trabajar con la base local:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
```

Para otro servidor, cambia la configuración del entorno correspondiente o define `ConnectionStrings__SyncForge` en la sesión.

El repositorio fija `dotnet-ef` en la versión `10.0.12` mediante `dotnet-tools.json`. En una instalación nueva, restaura la herramienta:

```powershell
dotnet tool restore
dotnet ef --version
```

## Ver el estado

```powershell
dotnet ef migrations list --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
dotnet ef migrations has-pending-model-changes --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
```

## Montar o actualizar el esquema

Aplica todas las migraciones pendientes. Si la base aún no existe, EF Core la crea:

```powershell
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
```

## Revertir el esquema

Revierte todas las migraciones y deja la base sin las tablas de la aplicación:

```powershell
dotnet ef database update 0 --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
```

Este comando **elimina las tablas y sus datos**, pero conserva la base `SyncForge`. Para reconstruir el esquema, ejecuta de nuevo `dotnet ef database update` como aparece arriba.

Para volver a una migración concreta sin revertirlas todas, sustituye `0` por su identificador. Por ejemplo, para conservar solo la migración inicial:

```powershell
dotnet ef database update 20260924094252_InitialCreate --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
```

La reversión de una migración puede eliminar los datos de las tablas o columnas que esa migración creó.

## Crear una nueva migración

Después de modificar el modelo de EF Core:

```powershell
dotnet ef migrations add NombreDelCambio --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --output-dir Persistence/Migrations
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj
```

`database update 0` revierte el esquema; si necesitas borrar también la base de SQL Server, `dotnet ef database drop --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj` solicita confirmación antes de hacerlo.
