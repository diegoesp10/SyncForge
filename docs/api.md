# API HTTP

## Ver los endpoints en el navegador

La API genera el documento OpenAPI con `Microsoft.AspNetCore.OpenApi` y lo muestra con Scalar. Al iniciar el proyecto `API` en entorno `Development`, el perfil de Visual Studio abre automáticamente la interfaz. También puedes entrar manualmente:

Salvo `POST /api/auth/login`, las acciones HTTP de la API requieren `Authorization: Bearer <accessToken>`. Obtén el token tras crear el primer `SuperAdmin` con el comando interactivo de [seguridad](security.md). En Scalar, usa la opción de autorización Bearer para probar rutas protegidas. La propia documentación permanece accesible sin token solo en desarrollo.

| Perfil | Interfaz interactiva | Documento JSON |
| --- | --- | --- |
| `http` | `http://localhost:5254/api-docs` | `http://localhost:5254/openapi/v1.json` |
| `https` | `https://localhost:7233/api-docs` | `https://localhost:7233/openapi/v1.json` |

Desde la raíz del repositorio: `dotnet run --project API/API.csproj --launch-profile https`. La interfaz lee el documento que ASP.NET Core genera de los controladores y permite inspeccionar las rutas, sus parámetros, los esquemas y probar peticiones. Cuando añadas o cambies un endpoint, recompila y reinicia la API, y actualiza la página; no hay una lista manual de rutas que mantener. La interfaz y el JSON solo se exponen en `Development`.

Los controladores de negocio llaman a los servicios de `Application`; estos coordinan los handlers, que acceden a SQL Server mediante los repositorios de `Infrastructure`. Los controladores de autenticación y usuarios llaman a `Security`, con su propio `SecurityDbContext`. Desde la raíz del repositorio puedes iniciar la API con `dotnet run --project API/API.csproj --launch-profile http`. Las conexiones de desarrollo están en `API/appsettings.Development.json`, bajo `ConnectionStrings:SyncForge` y `ConnectionStrings:SyncForgeAuth`, y usan autenticación de Windows. Para otro entorno, configura esas claves o las variables `ConnectionStrings__SyncForge` y `ConnectionStrings__SyncForgeAuth`.

Todas las rutas admiten `?language=es` o `?language=en`. También se acepta `Accept-Language`; el parámetro de consulta tiene prioridad. Si no se indica idioma, se usa inglés. Los errores se devuelven como `ProblemDetails` con mensajes de los recursos localizados.

| Método | Ruta | Acción |
| --- | --- | --- |
| `POST` | `/api/auth/login` | Iniciar sesión con correo y contraseña; devuelve un token de 24 horas (anónimo) |
| `GET` | `/api/auth/me` | Consultar la cuenta del token |
| `POST` | `/api/auth/logout` | Revocar la sesión actual (`204`) |
| `GET` | `/api/users` | Listar usuarios (`SuperAdmin`) |
| `GET` | `/api/users/{id}` | Consultar usuario (`SuperAdmin`) |
| `POST` | `/api/users` | Crear usuario `User` o `Admin` (`SuperAdmin`) |
| `PATCH` | `/api/users/{id}/role` | Cambiar rol (`SuperAdmin`) |
| `PATCH` | `/api/users/{id}/status` | Activar o desactivar usuario (`SuperAdmin`) |
| `GET` | `/api/health` | Comprobar la conexión con SQL Server y obtener la versión |
| `POST` | `/api/files` | Subir un archivo multipart en el campo `file` (máximo 50 MB) |
| `GET` | `/api/files` | Listar archivos del más reciente al más antiguo |
| `GET` | `/api/files/{id}` | Consultar estado y metadatos de un archivo |
| `GET` | `/api/files/{id}/result` | Obtener el resumen y la vista previa cuando haya terminado |
| `POST` | `/api/files/{id}/reprocess` | Volver a procesar un archivo completado o fallido |
| `PATCH` | `/api/files/{id}/name` | Cambiar el nombre visible del archivo y devolver su ficha actualizada |
| `DELETE` | `/api/files/{id}` | Mover el archivo a la papelera (conserva `204`) |
| `POST` | `/api/trash-can/{id}` | Mover un archivo a la papelera y devolver su ficha (`201`) |
| `GET` | `/api/trash-can` | Listar archivos de la papelera, del más reciente al más antiguo |
| `GET` | `/api/trash-can/{id}` | Consultar un archivo de la papelera |
| `GET` | `/api/trash-can/{id}/result` | Consultar su resultado si terminó el procesamiento |
| `POST` | `/api/trash-can/{id}/restore` | Restaurar un archivo antes de que venza su plazo |
| `DELETE` | `/api/trash-can/{id}` | Borrar definitivamente el original y sus datos (`204`) |
| `POST` | `/api/import-jobs` | Crear un trabajo pendiente |
| `GET` | `/api/import-jobs?skip=0&take=50` | Listar trabajos |
| `GET` | `/api/import-jobs/{id}` | Consultar trabajo e intentos |
| `POST` | `/api/import-jobs/{id}/start` | Iniciar un intento |
| `POST` | `/api/import-jobs/{id}/complete` | Completar el intento actual |
| `POST` | `/api/import-jobs/{id}/fail` | Fallar el intento actual con un código |
| `POST` | `/api/import-jobs/{id}/retry` | Preparar un trabajo fallido para otro intento |
| `POST` | `/api/orders` | Guardar un pedido en un trabajo en proceso |
| `GET` | `/api/orders/{id}` | Consultar un pedido |
| `GET` | `/api/orders/by-source?sourceSystem={sourceSystem}&externalId={externalId}` | Consultar por identidad de origen |
| `GET` | `/api/orders?importJobId={id}&skip=0&take=50` | Listar pedidos de un trabajo |

El flujo de archivos responde al contrato de `SyncForge Front/API_CONTRACT.md`. La API guarda el original en `API/data/uploads` y sus metadatos, estado y resultado en SQL Server. Solo admite CSV, TSV, JSON, TXT, LOG, XML y MD con contenido de texto UTF-8. Antes de registrar la subida comprueba la extensión, el tipo MIME declarado y los bytes del archivo; fotos, vídeos, audio, documentos binarios y XLSX reciben `415 Unsupported Media Type` y no se conservan. El tipo MIME por sí solo no determina la aceptación. Un trabajador en segundo plano entrega hasta 200 filas de tabla o una vista previa limitada de texto/JSON. Un JSON inválido se muestra como texto con una advertencia localizada. Si se reinicia la API, los archivos pendientes o interrumpidos vuelven a la cola.

Para renombrar, envía `PATCH /api/files/{id}/name` con `Content-Type: application/json` y `{"fileName":"nuevo.csv"}`. La respuesta es `FileItemResponse` con el nuevo `fileName`. Se cambia `StoredFiles.FileName`; el original en disco conserva su GUID y el resultado ya procesado permanece intacto. El nombre debe tener entre 1 y 260 caracteres, ser un nombre simple sin separadores ni caracteres de ruta, y conservar la extensión original. Un nombre inválido devuelve `400`, un identificador inexistente o en la papelera `404`, y un archivo en procesamiento `409`. Los mensajes se localizan igual que el resto de errores.

La papelera es lógica: la tabla `TrashCan` enlaza por `FileId` con `StoredFiles` y conserva `movedAt` y `purgeAt`. Al moverlo, desaparece de `/api/files` y permanece disponible en `/api/trash-can` con su resultado; el original físico sigue disponible para restaurarlo. `purgeAt` se fija 30 días después del movimiento. Un trabajador revisa los vencimientos al iniciar la API y cada cinco minutos; elimina el archivo físico y luego el registro SQL. Si falla, deja la entrada marcada y vuelve a intentar el borrado en el siguiente ciclo. La eliminación definitiva manual usa el mismo proceso. Los errores de papelera se localizan con los recursos de `Domain`.

El flujo de pedidos sigue siendo independiente: crear un trabajo, iniciarlo, guardar los pedidos y completarlo o marcarlo como fallido. `StoredFileKey` de los trabajos de importación todavía no se vincula automáticamente con `/api/files`. El archivo [Requests.http](../API/Requests.http) contiene ejemplos ejecutables; cambia los identificadores por los devueltos al crear los recursos.
