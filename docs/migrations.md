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

## Borrar la base y volver a crearla

Detén la API y ejecuta desde la raíz del repositorio, en PowerShell, cada comando completo por separado:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet tool restore
dotnet build API/API.csproj -m:1 /nodeReuse:false
dotnet ef database drop --dry-run --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --no-build
dotnet ef database drop --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --no-build
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --no-build
```

`--dry-run` muestra qué base se borraría sin tocarla. `drop` solicita confirmación y elimina la base **con todos sus datos**; `update` crea de nuevo `SyncForge` y aplica las migraciones disponibles. Comprueba que el primer comando señala `SyncForge` en `localhost` antes de confirmar. La carpeta `API/data/uploads` no forma parte de SQL Server y conserva los archivos originales aunque borres la base.

Si aparece `Unable to retrieve project metadata`, ejecuta la compilación explícita indicada arriba y repite el comando de EF con `--no-build`. Si el terminal muestra `>>`, PowerShell está esperando que completes una instrucción; pulsa `Ctrl+C` y escribe el comando de nuevo en una sola línea. Si SQL Server no acepta la autenticación integrada, ejecuta los comandos en tu sesión normal de Windows.

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

`database update 0` revierte el esquema sin borrar la base. Para eliminar también la base, utiliza la secuencia completa de «Borrar la base y volver a crearla».
