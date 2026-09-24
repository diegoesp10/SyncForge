# Base de datos local

SyncForge usa SQL Server mediante Entity Framework Core. La conexión de desarrollo está en `API/appsettings.Development.json`, dentro de `ConnectionStrings:SyncForge`, y apunta a la instancia `localhost` y a la base `SyncForge` con autenticación integrada de Windows.

`Startup.cs` lee esa configuración y la pasa a `AddInfrastructure`. Los comandos de EF Core utilizan la API como proyecto de inicio para leer la misma configuración. Para otros entornos, define `ConnectionStrings:SyncForge` en su configuración o mediante la variable estándar `ConnectionStrings__SyncForge`; no guardes credenciales en archivos versionados.

Los comandos para crear, actualizar y revertir el esquema están en la [guía de migraciones](migrations.md).

Las migraciones existentes crean `ImportJobs`, `ImportAttempts` y `Orders`. Cada trabajo guarda la referencia `StoredFileKey` al archivo de origen, el estado actual, un código estable de fallo y el contador de intentos. Cada intento conserva sus fechas y su propio código de fallo. Los pedidos guardan el identificador externo junto al sistema de origen; esa pareja es única. `StoredFileKey` es una referencia: la lectura y el almacenamiento físico del archivo se incorporarán cuando se implemente el flujo de importación.

Los mensajes de error se resuelven en inglés o español al generar la respuesta. La base de datos guarda el código de fallo, no el texto traducido.
