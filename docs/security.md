# Seguridad y acceso

## Estado actual

La demo local funciona con ASP.NET Core Identity y SQL Server. `SecurityDbContext` usa `SyncForgeAuth`, separada de `SyncForge` (negocio). Identity guarda hashes de contraseña, usuarios y roles; `AuthSessions` guarda el identificador y vigencia de cada sesión. No hay contraseñas de Microsoft en la aplicación.

Las altas y la asignación de la contraseña inicial usan `UserManager<AppUser>` y el `PasswordHasher<AppUser>` predeterminado de Identity. En la versión 10.0.12 usada por el proyecto, el formato Identity V3 aplica PBKDF2 con HMAC-SHA512, una sal aleatoria de 128 bits, 100 000 iteraciones y una clave derivada de 256 bits. `Users.PasswordHash` contiene el resultado codificado con sus parámetros, nunca la contraseña. Identity verifica el hash al iniciar sesión y puede actualizar formatos anteriores cuando corresponde; consulta el [código oficial de PasswordHasher](https://github.com/dotnet/aspnetcore/blob/v10.0.12/src/Identity/Extensions.Core/src/PasswordHasher.cs).

`GET /api/health`, `POST /api/auth/login`, `POST /api/auth/register`, `POST /api/auth/resend-confirmation` y `GET /api/auth/confirm-email` son anónimos. El registro y la confirmación se limitan por IP. Cada inicio de sesión correcto crea una fila `AuthSessions` ligada a `Users` y emite un JWT nuevo, firmado con HMAC SHA-256 y válido durante 24 horas. `Users.LastSignedInAt` registra la fecha del último acceso correcto y se devuelve en `/api/auth/me`. La respuesta del login incluye `Cache-Control: no-store` para que cachés HTTP no guarden el token. La firma, emisor, destinatario y caducidad se validan en cada petición. Además, la API consulta la sesión, el correo confirmado, el estado del usuario y sus roles actuales en `SyncForgeAuth`: cerrar sesión, desactivar una cuenta o cambiar su rol invalida los tokens emitidos. Las sesiones vencidas se limpian durante nuevos inicios de sesión. Tras cinco contraseñas fallidas, Identity bloquea temporalmente la cuenta durante 15 minutos. El login también se limita a cinco peticiones por minuto y dirección IP.

La clave `Security:SigningKey` se guarda en `API/appsettings.Development.local.json` (ignorado por Git). Debe tener al menos 32 bytes aleatorios codificados en Base64. En esta instalación ya existe. Para generar una nueva en otra instalación:

```powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
@{ Security = @{ SigningKey = $key } } | ConvertTo-Json -Depth 3 | Set-Content API/appsettings.Development.local.json
```

Si cambias esa clave, los tokens existentes dejan de ser válidos. Para otro entorno, proporciona la clave y las cadenas de conexión mediante un gestor de secretos o variables de entorno, por ejemplo `Security__SigningKey`, `ConnectionStrings__SyncForge` y `ConnectionStrings__SyncForgeAuth`. Publica la API únicamente por HTTPS.

## SuperAdmin inicial

La migración `SeedInitialSuperAdmin` crea el usuario `DiegoEspina` con correo `diegoespinarodriguez@gmail.com` y rol `SuperAdmin`. `EmailConfirmed` es `false` y `PasswordHash` es `NULL`. Una contraseña básica incluida en una migración pública permitiría que cualquiera entrase en cuanto se verificara el correo. Por ello, la migración deja la cuenta sin acceso y el propietario establece una contraseña privada, una sola vez, desde una terminal interactiva. El comando `--SuperAdminCreation` recoge la contraseña en la API y delega su validación y guardado al servicio de seguridad; `Program.cs` solo inicia el host. Detén la API antes de compilar y ejecuta:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project API/API.csproj --launch-profile https -- --SuperAdminCreation
```

El asistente solicita contraseña y confirmación. Exige 12 caracteres o más, mayúscula, minúscula, dígito y símbolo. Después configura SMTP y solicita `POST /api/auth/resend-confirmation` con `{"email":"diegoespinarodriguez@gmail.com"}`. Abre el enlace recibido; solo tras confirmar el correo podrás iniciar sesión. No pongas la contraseña en la línea de comandos, en `appsettings` ni en el repositorio.

## Registro y verificación del email

`POST /api/auth/register` acepta `email`, `displayName` y `password`. Crea exclusivamente un usuario `User`; no hay alta pública de administradores. La respuesta `202` no revela si la dirección ya existe. El servidor envía un enlace con un token de confirmación válido durante 24 horas. `GET /api/auth/confirm-email` consume el token y Identity establece `Users.EmailConfirmed = true`. Hasta entonces, el login está bloqueado. `POST /api/auth/resend-confirmation` acepta `{"email":"..."}` y devuelve `202` sin revelar si existe la cuenta. Si falla el envío después de crearla, queda pendiente y se puede pedir el reenvío.

El transporte usa MailKit por SMTP con STARTTLS (o TLS directo en el puerto 465). En desarrollo, `API/appsettings.Development.json` propone Gmail como servidor y `diegoespinarodriguez@gmail.com` como remitente. Falta una credencial SMTP: configúrala como `Email__Smtp__Password` en el entorno o como `Email:Smtp:Password` en `API/appsettings.Development.local.json`, que Git ignora. Para introducirla sin dejarla en el historial de PowerShell antes de iniciar la API:

```powershell
$smtpSecret = Read-Host 'Contraseña de aplicación SMTP' -AsSecureString
$env:Email__Smtp__Password = [System.Net.NetworkCredential]::new('', $smtpSecret).Password
```

No uses la contraseña normal de Gmail ni la compartas en el chat. [Google explica](https://support.google.com/mail/answer/185833) que la contraseña de aplicación exige verificación en dos pasos; si tu cuenta no ofrece esta opción, usa otro proveedor SMTP y cambia los valores `Email:Smtp`. Sin una credencial configurada, registro y reenvío responden `503` y no se activa ninguna cuenta. El enlace de desarrollo apunta a `https://localhost:7233/api/auth/confirm-email`; cambia `Email:Smtp:ConfirmationBaseUrl` al desplegar.

## Tokens y roles

Inicia la API normalmente y solicita el token:

```http
POST /api/auth/login
Content-Type: application/json

{"email":"tu@correo.com","password":"tu-contraseña"}
```

La respuesta incluye `accessToken`, `tokenType`, `expiresAt`, `isFirstLogin` y `shouldShowOnboarding`. `isFirstLogin` solo es verdadero en el primer inicio de sesión correcto de esa cuenta; se calcula antes de actualizar `Users.LastSignedInAt`. `shouldShowOnboarding` sigue siendo verdadero hasta que el usuario termine la guía, aunque cierre el navegador y vuelva a entrar. Cuando la termine, el frontal debe llamar con su token a `POST /api/auth/onboarding/complete`: devuelve `204`, acepta llamadas repetidas y guarda la fecha en `Users.OnboardingCompletedAt`. En los siguientes logins, `shouldShowOnboarding` será falso; `GET /api/auth/me` también incluye este indicador para restaurar el estado de la interfaz después de recargar. No marques la guía como completada al iniciar sesión, porque se perdería si se interrumpe.

El frontal puede mantener `accessToken` y `expiresAt` en `sessionStorage` y enviarlo en `Authorization: Bearer <accessToken>` mientras siga vigente; debe eliminarlo al cerrar sesión y tratar `401` como sesión terminada. `sessionStorage` pertenece al navegador: el backend no guarda allí el token ni controla el cierre de la pestaña. El backend conserva solo la fila de sesión necesaria para validar o revocar el token. `GET /api/auth/me` devuelve el usuario actual y `POST /api/auth/logout` revoca solo la sesión del token utilizado. Una petición sin token o con token inválido devuelve `401`; una acción sin el rol necesario devuelve `403`. Los errores de identidad se localizan con `?language=es`, `?language=en` o `Accept-Language`.

| Rol | Acceso actual |
| --- | --- |
| `User` | Endpoints de negocio, archivos, papelera, importaciones y pedidos |
| `Admin` | Los mismos endpoints de negocio |
| `SuperAdmin` | Endpoints de negocio y administración de usuarios |

El modelo **todavía no aísla los datos de negocio por usuario**: cualquier cuenta activa con uno de estos roles puede acceder a los mismos archivos y pedidos. Antes de abrir el servicio a usuarios distintos, habrá que añadir propiedad/tenant a los registros de negocio y políticas específicas por acción.

Solo `SuperAdmin` puede llamar a `GET /api/users`, `GET /api/users/{id}`, `POST /api/users`, `PATCH /api/users/{id}/role` y `PATCH /api/users/{id}/status`. `POST /api/users` acepta `email`, `displayName`, `password` y `role` (`User` o `Admin`), envía confirmación y no permite iniciar sesión hasta verificar el correo. El cambio de rol acepta `{"role":"Admin"}` y el cambio de estado `{"isActive":false}`. Estas acciones no pueden modificar la cuenta `SuperAdmin`; cambiar rol o estado revoca sus sesiones existentes. El registro público solo crea `User`.

La interfaz Scalar está disponible en desarrollo en `/api-docs`; su documento OpenAPI declara el esquema Bearer para autorizar peticiones de prueba. La página y `/openapi/v1.json` son anónimas solo en desarrollo.

## Preparación para Microsoft Entra ID

El tenant `ab9d8530-f78b-472a-b807-04804330194a` y su dominio `diegoespinarodriguezgmail.onmicrosoft.com` están en la sección `AzureAd` de `API/appsettings.Development.json`. Todavía no hay una app registration ni Client ID (la captura del tenant muestra cero aplicaciones), por lo que **Entra ID no autentica peticiones todavía**. El emisor `SyncForge.Local` es independiente de Entra; los JWT actuales son propios de esta demo. `AppUser` reserva `EntraTenantId` y `EntraObjectId` con índice único para vincular cuentas más adelante. Nunca se debe enlazar una cuenta únicamente por correo: el identificador estable es la pareja tenant/objeto, tras validar un token emitido para esta API.

Cuando dispongas de tenant y app registration, registra la API, define sus scopes o app roles, configura el frontend como cliente público con Authorization Code + PKCE mediante MSAL y añade validación de tokens Entra con `Microsoft.Identity.Web`. Mapea los identificadores `tid`/`oid` a usuarios locales y define políticas para `User`, `Admin` y `SuperAdmin`. El token de acceso emitido por Entra tendrá la duración que establezca Entra; el plazo local de 24 horas no se trasladará a esos tokens. Se puede explorar un tenant de desarrollo con una cuenta personal de Microsoft, sujeto a la elegibilidad actual del programa de Microsoft. Esta integración requiere configuración y pruebas reales antes de habilitarse.
