# Seguridad y acceso

## Estado actual

La demo local funciona con ASP.NET Core Identity y SQL Server. `SecurityDbContext` usa `SyncForgeAuth`, separada de `SyncForge` (negocio). Identity guarda hashes de contraseña, usuarios y roles; `AuthSessions` guarda el identificador y vigencia de cada sesión. No hay contraseñas de Microsoft en la aplicación.

`POST /api/auth/login` es la única acción anónima de la API. Cada inicio de sesión correcto crea una fila `AuthSessions` ligada a `Users` y emite un JWT nuevo, firmado con HMAC SHA-256 y válido durante 24 horas. `Users.LastSignedInAt` registra la fecha del último acceso correcto y se devuelve en `/api/auth/me`. La respuesta del login incluye `Cache-Control: no-store` para que cachés HTTP no guarden el token. La firma, emisor, destinatario y caducidad se validan en cada petición. Además, la API consulta la sesión, el estado del usuario y sus roles actuales en `SyncForgeAuth`: cerrar sesión, desactivar una cuenta o cambiar su rol invalida los tokens emitidos. Las sesiones vencidas se limpian durante nuevos inicios de sesión. Tras cinco contraseñas fallidas, Identity bloquea temporalmente la cuenta durante 15 minutos. El login también se limita a cinco peticiones por minuto y dirección IP.

La clave `Security:SigningKey` se guarda en `API/appsettings.Development.local.json` (ignorado por Git). Debe tener al menos 32 bytes aleatorios codificados en Base64. En esta instalación ya existe. Para generar una nueva en otra instalación:

```powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
@{ Security = @{ SigningKey = $key } } | ConvertTo-Json -Depth 3 | Set-Content API/appsettings.Development.local.json
```

Si cambias esa clave, los tokens existentes dejan de ser válidos. Para otro entorno, proporciona la clave y las cadenas de conexión mediante un gestor de secretos o variables de entorno, por ejemplo `Security__SigningKey`, `ConnectionStrings__SyncForge` y `ConnectionStrings__SyncForgeAuth`. Publica la API únicamente por HTTPS.

## Primer SuperAdmin

Aplica primero la migración de seguridad según [migrations.md](migrations.md). Después ejecuta desde la raíz del repositorio en una terminal interactiva:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project API/API.csproj --launch-profile https -- --bootstrap-superadmin
```

El asistente solicita correo, nombre y contraseña con confirmación. La contraseña exige 12 caracteres o más, mayúscula, minúscula, dígito y símbolo. El proceso termina al crear la cuenta y rechaza un segundo SuperAdmin. No pongas la contraseña en la línea de comandos, en `appsettings` ni en el repositorio.

## Tokens y roles

Inicia la API normalmente y solicita el token:

```http
POST /api/auth/login
Content-Type: application/json

{"email":"tu@correo.com","password":"tu-contraseña"}
```

La respuesta incluye `accessToken`, `tokenType` y `expiresAt`. El frontal puede mantener `accessToken` y `expiresAt` en `sessionStorage` y enviarlo en `Authorization: Bearer <accessToken>` mientras siga vigente; debe eliminarlo al cerrar sesión y tratar `401` como sesión terminada. `sessionStorage` pertenece al navegador: el backend no guarda allí el token ni controla el cierre de la pestaña. El backend conserva solo la fila de sesión necesaria para validar o revocar el token. `GET /api/auth/me` devuelve el usuario actual y `POST /api/auth/logout` revoca solo la sesión del token utilizado. Una petición sin token o con token inválido devuelve `401`; una acción sin el rol necesario devuelve `403`. Los errores de identidad se localizan con `?language=es`, `?language=en` o `Accept-Language`.

| Rol | Acceso actual |
| --- | --- |
| `User` | Endpoints de negocio, archivos, papelera, importaciones y pedidos |
| `Admin` | Los mismos endpoints de negocio |
| `SuperAdmin` | Endpoints de negocio y administración de usuarios |

El modelo **todavía no aísla los datos de negocio por usuario**: cualquier cuenta activa con uno de estos roles puede acceder a los mismos archivos y pedidos. Antes de abrir el servicio a usuarios distintos, habrá que añadir propiedad/tenant a los registros de negocio y políticas específicas por acción.

Solo `SuperAdmin` puede llamar a `GET /api/users`, `GET /api/users/{id}`, `POST /api/users`, `PATCH /api/users/{id}/role` y `PATCH /api/users/{id}/status`. `POST /api/users` acepta `email`, `displayName`, `password` y `role` (`User` o `Admin`). El cambio de rol acepta `{"role":"Admin"}` y el cambio de estado `{"isActive":false}`. Estas acciones no pueden modificar la cuenta `SuperAdmin`; cambiar rol o estado revoca sus sesiones existentes. No existe registro público.

La interfaz Scalar está disponible en desarrollo en `/api-docs`; su documento OpenAPI declara el esquema Bearer para autorizar peticiones de prueba. La página y `/openapi/v1.json` son anónimas solo en desarrollo.

## Preparación para Microsoft Entra ID

El tenant `ab9d8530-f78b-472a-b807-04804330194a` y su dominio `diegoespinarodriguezgmail.onmicrosoft.com` están en la sección `AzureAd` de `API/appsettings.Development.json`. Todavía no hay una app registration ni Client ID (la captura del tenant muestra cero aplicaciones), por lo que **Entra ID no autentica peticiones todavía**. El emisor `SyncForge.Local` es independiente de Entra; los JWT actuales son propios de esta demo. `AppUser` reserva `EntraTenantId` y `EntraObjectId` con índice único para vincular cuentas más adelante. Nunca se debe enlazar una cuenta únicamente por correo: el identificador estable es la pareja tenant/objeto, tras validar un token emitido para esta API.

Cuando dispongas de tenant y app registration, registra la API, define sus scopes o app roles, configura el frontend como cliente público con Authorization Code + PKCE mediante MSAL y añade validación de tokens Entra con `Microsoft.Identity.Web`. Mapea los identificadores `tid`/`oid` a usuarios locales y define políticas para `User`, `Admin` y `SuperAdmin`. El token de acceso emitido por Entra tendrá la duración que establezca Entra; el plazo local de 24 horas no se trasladará a esos tokens. Se puede explorar un tenant de desarrollo con una cuenta personal de Microsoft, sujeto a la elegibilidad actual del programa de Microsoft. Esta integración requiere configuración y pruebas reales antes de habilitarse.
