# API HTTP

Los controladores llaman a los servicios de `Application`; estos coordinan los handlers, que acceden a SQL Server mediante los repositorios de `Infrastructure`. Desde la raíz del repositorio puedes iniciar la API con `dotnet run --project API/API.csproj --launch-profile http`. La conexión de desarrollo está en `API/appsettings.Development.json`, bajo `ConnectionStrings:SyncForge`, y usa autenticación de Windows. Para otro entorno, configura esa misma clave o la variable estándar `ConnectionStrings__SyncForge`.

Todas las rutas admiten `?language=es` o `?language=en`. También se acepta `Accept-Language`; el parámetro de consulta tiene prioridad. Si no se indica idioma, se usa inglés. Los errores se devuelven como `ProblemDetails` con mensajes de los recursos localizados.

| Método | Ruta | Acción |
| --- | --- | --- |
| `GET` | `/api/health` | Comprobar la conexión con SQL Server y obtener la versión |
| `POST` | `/api/files` | Subir un archivo multipart en el campo `file` (máximo 50 MB) |
| `GET` | `/api/files` | Listar archivos del más reciente al más antiguo |
| `GET` | `/api/files/{id}` | Consultar estado y metadatos de un archivo |
| `GET` | `/api/files/{id}/result` | Obtener el resumen y la vista previa cuando haya terminado |
| `POST` | `/api/files/{id}/reprocess` | Volver a procesar un archivo completado o fallido |
| `DELETE` | `/api/files/{id}` | Borrar los metadatos y el archivo original |
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

El flujo de pedidos sigue siendo independiente: crear un trabajo, iniciarlo, guardar los pedidos y completarlo o marcarlo como fallido. `StoredFileKey` de los trabajos de importación todavía no se vincula automáticamente con `/api/files`. El archivo [Requests.http](../API/Requests.http) contiene ejemplos ejecutables; cambia los identificadores por los devueltos al crear los recursos.
