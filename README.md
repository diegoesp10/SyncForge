# SyncForge

## Reiniciar la base de datos local (PowerShell)

Ejecuta estos comandos **uno por uno desde la raíz del repositorio**, con la API detenida. La conexión de desarrollo apunta a `localhost`, base `SyncForge`, con autenticación de Windows. El borrado elimina **la base completa y todos sus datos**.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet tool restore
dotnet build API/API.csproj -m:1 /nodeReuse:false
dotnet ef database drop --dry-run --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --no-build
dotnet ef database drop --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --no-build
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --no-build
```

`drop` muestra la base que va a borrar y solicita confirmación. `update` vuelve a crearla y aplica todas las migraciones. `--no-build` usa la compilación explícita anterior y evita el error de metadatos de proyecto que puede aparecer al compilar dentro de `dotnet ef`. Si PowerShell muestra `>>`, pulsa `Ctrl+C` y vuelve a introducir el comando completo en una sola línea. El borrado de SQL Server no elimina los archivos originales de `API/data/uploads`.

## Documentación

- [Base de datos y modelo de datos](docs/database.md)
- [Comandos de migraciones de EF Core](docs/migrations.md)
- [Rutas y ejemplos de la API](docs/api.md)
