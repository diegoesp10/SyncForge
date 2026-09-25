# Migraciones de EF Core

Ejecuta estos comandos desde la raíz del repositorio. EF Core usa la API como proyecto de inicio. `Infrastructure` y `SyncForgeDbContext` migran la base de negocio `SyncForge`; `Security` y `SecurityDbContext` migran por separado `SyncForgeAuth`. Ambas conexiones están en `API/appsettings.Development.json`. En PowerShell, selecciona el entorno de desarrollo antes de trabajar con las bases locales:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
```

Para otro servidor, cambia la configuración del entorno correspondiente o define `ConnectionStrings__SyncForge` y `ConnectionStrings__SyncForgeAuth` en la sesión. La clave privada de firma local se carga desde `API/appsettings.Development.local.json`; si clonas el repositorio, créala como explica [security.md](security.md) antes de ejecutar EF, porque la API valida su configuración al arrancar.

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

## Migraciones de identidad (`SyncForgeAuth`)

Estos comandos solo afectan a la base de usuarios. No los confundas con los de `Infrastructure`; las dos bases tienen historiales `__EFMigrationsHistory` independientes. Al borrar o revertir `SyncForgeAuth` se pierden las cuentas, roles asignados y sesiones, incluido el `SuperAdmin`.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet build API/API.csproj -m:1 /nodeReuse:false
dotnet ef migrations list --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
dotnet ef migrations has-pending-model-changes --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
dotnet ef database update --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
```

Para volver a crear **solo** la base de identidad, detén la API, revisa la base indicada por `--dry-run` y ejecuta cada línea por separado:

```powershell
dotnet ef database drop --dry-run --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
dotnet ef database drop --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
dotnet ef database update --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
```

Después del borrado, `database update` vuelve a insertar a `DiegoEspina` sin contraseña y sin correo confirmado. Tendrás que fijar de nuevo su contraseña con `dotnet run --project API/API.csproj --launch-profile https -- --SuperAdminCreation` y verificar el email. Para revertir todas las tablas de identidad conservando la base vacía, usa:

```powershell
dotnet ef database update 0 --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
```

Tras modificar las entidades de seguridad, crea y aplica una nueva migración únicamente en `Security`:

```powershell
dotnet build API/API.csproj -m:1 /nodeReuse:false
dotnet ef migrations add NombreDelCambio --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build --output-dir Migrations
dotnet build API/API.csproj -m:1 /nodeReuse:false
dotnet ef database update --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
```

La compilación después de `migrations add` es necesaria cuando se usa `--no-build`: permite a EF cargar la nueva migración antes de ejecutar `database update`.
