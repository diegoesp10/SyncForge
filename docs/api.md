# API HTTP

La API usa los handlers de `Application`; estos acceden a SQL Server mediante los repositorios de `Infrastructure`. Desde la raíz del repositorio puedes iniciarla con `dotnet run --project API/API.csproj --launch-profile http`. La conexión de desarrollo está en `API/appsettings.Development.json`, bajo `ConnectionStrings:SyncForge`, y usa autenticación de Windows. Para otro entorno, configura esa misma clave o la variable estándar `ConnectionStrings__SyncForge`.

Todas las rutas admiten `?language=es` o `?language=en`. También se acepta `Accept-Language`; el parámetro de consulta tiene prioridad. Si no se indica idioma, se usa inglés. Los errores se devuelven como `ProblemDetails` con mensajes de los recursos localizados.

| Método | Ruta | Acción |
| --- | --- | --- |
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

El flujo actual es crear el trabajo, iniciarlo, guardar los pedidos y completarlo o marcarlo como fallido. `StoredFileKey` es la referencia al archivo original; la API todavía no recibe ni procesa archivos automáticamente. El archivo [Requests.http](../API/Requests.http) contiene ejemplos ejecutables; cambia `jobId` y `orderId` por los identificadores devueltos al crear los recursos.
