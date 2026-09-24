# Base de datos local

SyncForge usa SQL Server mediante Entity Framework Core. La conexión de desarrollo está en `API/appsettings.Development.json`, dentro de `ConnectionStrings:SyncForge`, y apunta a la instancia `localhost` y a la base `SyncForge` con autenticación integrada de Windows.

`Startup.cs` lee esa configuración y la pasa a `AddInfrastructure`. Los comandos de EF Core utilizan la API como proyecto de inicio para leer la misma configuración. Para otros entornos, define `ConnectionStrings:SyncForge` en su configuración o mediante la variable estándar `ConnectionStrings__SyncForge`; no guardes credenciales en archivos versionados.

Los comandos para crear, actualizar y revertir el esquema están en la [guía de migraciones](migrations.md).

Las migraciones crean `ImportJobs`, `ImportAttempts`, `Orders` y `StoredFiles`. Cada trabajo guarda la referencia `StoredFileKey` al archivo de origen, el estado actual, un código estable de fallo y el contador de intentos. Cada intento conserva sus fechas y su propio código de fallo. Los pedidos guardan el identificador externo junto al sistema de origen; esa pareja es única.

`StoredFiles` pertenece al flujo de archivos del frontal. Guarda el nombre, tipo, tamaño, estado, fechas, código de fallo y resultado serializado. El original se almacena en `API/data/uploads` con el GUID como nombre y esa carpeta está excluida de Git. El trabajador recupera los archivos pendientes al arrancar. Por ahora `StoredFiles` y `StoredFileKey` de los trabajos de importación son flujos separados; subir un archivo no crea pedidos.

Los mensajes de error se resuelven en inglés o español al generar la respuesta. La base de datos guarda el código de fallo, no el texto traducido.
