# SyncForge

## Explorar la API

Arranca `API` con el perfil `https` de Visual Studio o con `dotnet run --project API/API.csproj --launch-profile https`. Visual Studio abre automáticamente la referencia interactiva en `https://localhost:7233/api-docs`; desde la terminal puedes abrir esa URL en el navegador. El documento OpenAPI generado a partir de los endpoints activos está en `https://localhost:7233/openapi/v1.json`. Consulta [la guía de la API](docs/api.md) para el perfil HTTP y más detalles.

## Funcionamiento del backend

- `API` expone los controladores HTTP y la documentación OpenAPI. Los controladores delegan en `Application`, que ejecuta las acciones y utiliza los repositorios de `Infrastructure`; `Domain` contiene las reglas y los errores localizados, y `Contracts` define las peticiones y respuestas.
- `POST /api/files` admite archivos de texto CSV, TSV, JSON, TXT, LOG, XML y MD de hasta 50 MB. El original se guarda en `API/data/uploads` con su GUID; SQL Server conserva el nombre visible, el tipo, el estado y el resultado. Un trabajador procesa los archivos y recupera pendientes tras reiniciar la API.
- `PATCH /api/files/{id}/name` cambia el nombre visible con `{"fileName":"nuevo.csv"}` y devuelve la ficha actualizada. Conserva la extensión y el archivo físico; no permite renombrar mientras se procesa. `GET /api/files` y `GET /api/files/{id}` muestran el nuevo nombre.
- `DELETE /api/files/{id}` mueve el archivo a `TrashCan`. `GET /api/trash-can` lista la papelera, `POST /api/trash-can/{id}/restore` lo restaura y `DELETE /api/trash-can/{id}` lo elimina definitivamente. El borrado automático se ejecuta 30 días después del movimiento.
- Los trabajos de importación y pedidos tienen sus propios endpoints y tablas. Su flujo todavía es independiente del procesamiento de `/api/files`. Los mensajes de error se devuelven en inglés o español mediante `?language=en`, `?language=es` o `Accept-Language`.
- `Security` guarda usuarios, roles y sesiones en la base SQL Server independiente `SyncForgeAuth`. La API exige `Authorization: Bearer <token>` en las rutas de negocio. El token local dura 24 horas y se comprueba contra una sesión revocable en cada petición.
- Las contraseñas se guardan con el hasher estándar de ASP.NET Core Identity V3 (PBKDF2, HMAC-SHA512, sal aleatoria y 100 000 iteraciones); nunca se almacenan en texto plano. La preparación inicial de `DiegoEspina` se ejecuta con `--SuperAdminCreation` desde una terminal interactiva.
- El login devuelve `isFirstLogin` y `shouldShowOnboarding`. El frontal puede mostrar su guía mientras `shouldShowOnboarding` sea `true` y, al completarla, llamar a `POST /api/auth/onboarding/complete`. Ese estado se guarda por usuario y también se consulta en `GET /api/auth/me`.

La conexión de desarrollo usa SQL Server en `localhost` con autenticación de Windows. Las rutas y los cuerpos de petición están detallados en [la guía de la API](docs/api.md) y en [Requests.http](API/Requests.http).

## Primer acceso a la demo local

La autenticación local usa ASP.NET Core Identity. Necesitas una clave de firma privada en `API/appsettings.Development.local.json`, excluido de Git. En esta instalación ya está creada. Para preparar otra instalación, ejecuta una vez desde la raíz del repositorio:

```powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
@{ Security = @{ SigningKey = $key } } | ConvertTo-Json -Depth 3 | Set-Content API/appsettings.Development.local.json
```

La migración de identidad ya inserta `DiegoEspina` (`diegoespinarodriguez@gmail.com`) con rol `SuperAdmin`, sin contraseña y con el correo pendiente de confirmar. Después de aplicar las migraciones, establece una contraseña privada mediante el asistente interactivo:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet tool restore
dotnet build API/API.csproj -m:1 /nodeReuse:false
dotnet ef database update --context SecurityDbContext --project Security/Security.csproj --startup-project API/API.csproj --no-build
dotnet run --project API/API.csproj --launch-profile https -- --SuperAdminCreation
```

El último comando pide una contraseña y su confirmación; exige al menos 12 caracteres, mayúscula, minúscula, número y símbolo. Para activar la cuenta, configura la credencial SMTP privada y solicita un enlace con `POST /api/auth/resend-confirmation`; confirma el correo recibido. Sin SMTP, el registro y el reenvío responden `503`. Solo entonces podrás iniciar sesión en `POST /api/auth/login`. El registro público `POST /api/auth/register` crea cuentas `User` pendientes de verificación; el `SuperAdmin` puede crear cuentas `User` y `Admin` en `/api/users`. Consulta [seguridad, correo y futura integración Entra ID](docs/security.md).

**Entra ID aún no está activo:** falta registrar la aplicación. La cuenta local funciona para esta demo; más adelante se podrá vincular a Entra mediante identificadores de tenant y objeto, sin almacenar contraseñas de Microsoft.
El tenant de desarrollo `ab9d8530-f78b-472a-b807-04804330194a` ya figura en `API/appsettings.Development.json`. Falta registrar la aplicación y obtener su Client ID; el login actual sigue emitiendo tokens propios de SyncForge.

## Reiniciar la base de datos local (PowerShell)

Ejecuta estos comandos **uno por uno desde la raíz del repositorio**, con la API detenida. La conexión de desarrollo apunta a `localhost`, base `SyncForge`, con autenticación de Windows. El borrado elimina **la base completa y todos sus datos**.

Esta secuencia afecta solo a `SyncForge`, la base de negocio. `SyncForgeAuth` y sus usuarios se conservan. La [guía de migraciones](docs/migrations.md) incluye los comandos separados para ambas bases.

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
- [Autenticación, roles y Entra ID](docs/security.md)
