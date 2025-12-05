# Sistema de Logging

> **Nivel:** Todos  
> **Objetivo:** Entender cómo funciona el sistema de logging y cómo usarlo

---

## 📋 Índice

1. [¿Qué es el logging?](#-qué-es-el-logging)
2. [Ubicación de los logs](#-ubicación-de-los-logs)
3. [Cómo acceder a los logs](#-cómo-acceder-a-los-logs)
4. [Niveles de log](#-niveles-de-log)
5. [Qué se registra](#-qué-se-registra)
6. [Para el usuario final](#-para-el-usuario-final)
7. [Para desarrolladores](#-para-desarrolladores)

---

## 🎯 ¿Qué es el logging?

El sistema de logging registra automáticamente:
- ✅ Operaciones importantes (crear cliente, guardar cita, etc.)
- ✅ Errores y excepciones
- ✅ Advertencias (intentos de duplicados, etc.)
- ✅ Información de depuración

**¿Para qué sirve?**
- Diagnosticar problemas cuando un cliente reporta un error
- Entender qué estaba haciendo el usuario cuando ocurrió el error
- Rastrear el flujo de operaciones
- Auditoría de cambios

---

## 📁 Ubicación de los logs

Los logs se guardan automáticamente en:

```
%LOCALAPPDATA%\InkStudio\logs\
```

**En Windows:**
```
C:\Users\[TU_USUARIO]\AppData\Local\InkStudio\logs\
```

### Formato de archivos

- **Nombre:** `inkstudio-YYYYMMDD.log`
- **Ejemplo:** `inkstudio-20241205.log`
- **Retención:** Se mantienen los últimos 30 días

---

## 🔍 Cómo acceder a los logs

### Opción 1: Desde la aplicación

1. Abre InkStudio CRM
2. En el menú lateral, haz clic en **"📋 Logs"**
3. Se abrirá automáticamente la carpeta de logs en el explorador

### Opción 2: Manualmente

1. Presiona `Win + R`
2. Escribe: `%LOCALAPPDATA%\InkStudio\logs`
3. Presiona Enter

### Opción 3: Desde código

```csharp
using InkStudio.Services;

// Abrir carpeta de logs
LoggingService.AbrirCarpetaLogs();

// Obtener ruta del log más reciente
var logMasReciente = LoggingService.ObtenerLogMasReciente();
```

---

## 📊 Niveles de log

| Nivel | Cuándo se usa | Ejemplo |
|-------|---------------|---------|
| **Debug** | Información detallada para desarrollo | "Cargando lista de clientes" |
| **Information** | Operaciones normales exitosas | "Cliente guardado: Juan Pérez" |
| **Warning** | Situaciones anómalas pero no críticas | "Teléfono duplicado detectado" |
| **Error** | Errores que se pueden manejar | "Error al cargar clientes" |
| **Fatal** | Errores críticos que cierran la app | "Error fatal al iniciar" |

---

## 📝 Qué se registra

### Operaciones normales

```
[2024-12-05 14:30:15.123 +01:00] [INF] Clientes cargados: 25 clientes activos
[2024-12-05 14:30:20.456 +01:00] [INF] Creando nuevo cliente: Juan Pérez, Tel: 612345678
[2024-12-05 14:30:21.789 +01:00] [INF] Cliente guardado exitosamente
```

### Búsquedas

```
[2024-12-05 14:35:10.123 +01:00] [DBG] Buscando clientes con término: juan
[2024-12-05 14:35:10.456 +01:00] [INF] Búsqueda completada: 3 resultados para 'juan'
```

### Errores

```
[2024-12-05 14:40:15.123 +01:00] [ERR] Error al guardar cliente. Nombre: Juan, Tel: 612345678
System.Data.Sqlite.SqliteException: UNIQUE constraint failed: Clientes.Telefono
   at Microsoft.EntityFrameworkCore...
```

### Advertencias

```
[2024-12-05 14:45:20.123 +01:00] [WRN] Intento de crear cliente con teléfono duplicado: 612345678
[2024-12-05 14:50:10.456 +01:00] [WRN] Eliminando (desactivando) cliente ID: 5, Nombre: Juan Pérez
```

---

## 👤 Para el usuario final

### Si la aplicación tiene un error

1. **No cierres la aplicación inmediatamente**
   - El error se guarda automáticamente en el log

2. **Abre la carpeta de logs**
   - Haz clic en "📋 Logs" en el menú lateral
   - O navega manualmente a `%LOCALAPPDATA%\InkStudio\logs`

3. **Encuentra el archivo más reciente**
   - Busca el archivo con la fecha de hoy: `inkstudio-YYYYMMDD.log`

4. **Copia el contenido del error**
   - Abre el archivo con el Bloc de notas
   - Busca las líneas que empiezan con `[ERR]` o `[FAT]`
   - Copia desde el error hasta el final del stack trace

5. **Envía el log al soporte**
   - Puedes enviar solo las líneas del error
   - O el archivo completo si es necesario

### Ejemplo de error en el log

```
[2024-12-05 15:30:15.123 +01:00] [ERR] Error al guardar cliente. Nombre: Juan, Tel: 612345678
System.Data.Sqlite.SqliteException: UNIQUE constraint failed: Clientes.Telefono
   at Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.RelationalShapedQueryCompilingExpressionVisitor.VisitShapedQueryExpression(ShapedQueryExpression shapedQueryExpression)
   at InkStudio.ViewModels.ClientesViewModel.GuardarCliente() in ClientesViewModel.cs:line 276
```

---

## 👨‍💻 Para desarrolladores

### Cómo usar logging en tu código

#### 1. Importar Serilog

```csharp
using Serilog;
```

#### 2. Logging básico

```csharp
// Información
Log.Information("Cliente guardado: {Nombre}", cliente.Nombre);

// Debug
Log.Debug("Cargando lista de clientes");

// Advertencia
Log.Warning("Teléfono duplicado: {Telefono}", telefono);

// Error con excepción
try
{
    await _db.SaveChangesAsync();
}
catch (Exception ex)
{
    Log.Error(ex, "Error al guardar cliente. ID: {ClienteId}", clienteId);
}
```

#### 3. Logging con contexto

```csharp
// Con múltiples parámetros
Log.Information("Actualizando cliente ID: {ClienteId}, Nombre: {Nombre}, Tel: {Telefono}", 
    clienteId, nombre, telefono);

// Con objetos
Log.Debug("Datos del cliente: {@Cliente}", cliente);
```

### Buenas prácticas

#### ✅ Hacer

- Loggear operaciones importantes (crear, editar, eliminar)
- Loggear todos los errores con `Log.Error(ex, ...)`
- Incluir contexto relevante (IDs, nombres, etc.)
- Usar niveles apropiados

#### ❌ No hacer

- Loggear información sensible (contraseñas, datos personales completos)
- Loggear en bucles muy frecuentes (puede llenar el disco)
- Loggear información obvia sin valor

### Ejemplo completo

```csharp
[RelayCommand]
private async Task GuardarCliente()
{
    try
    {
        Log.Debug("Iniciando guardado de cliente: {Nombre}", Nombre);
        
        // Validación
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            Log.Warning("Intento de guardar cliente sin nombre");
            MensajeError = "El nombre es obligatorio";
            return;
        }

        // Guardar
        if (EsEdicion)
        {
            Log.Information("Actualizando cliente ID: {Id}, Nombre: {Nombre}", 
                ClienteSeleccionado.Id, Nombre);
        }
        else
        {
            Log.Information("Creando nuevo cliente: {Nombre}, Tel: {Telefono}", 
                Nombre, Telefono);
        }

        await _db.SaveChangesAsync();
        Log.Information("Cliente guardado exitosamente");
    }
    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
    {
        Log.Warning("Teléfono duplicado: {Telefono}", Telefono);
        MensajeError = "Ya existe un cliente con ese teléfono";
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error al guardar cliente. Nombre: {Nombre}, Tel: {Telefono}", 
            Nombre, Telefono);
        MensajeError = $"Error al guardar: {ex.Message}";
    }
}
```

---

## 🔧 Configuración

El logging se configura en `Services/LoggingService.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()  // Nivel mínimo
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  // Reducir ruido de EF
    .WriteTo.Console()  // También en consola (debug)
    .WriteTo.File(
        path: logFile,
        rollingInterval: RollingInterval.Day,  // Un archivo por día
        retainedFileCountLimit: 30)  // Mantener 30 días
    .CreateLogger();
```

---

## 📦 Dependencias

- **Serilog** (4.1.0): Framework de logging
- **Serilog.Sinks.File** (6.0.0): Escribir a archivos
- **Serilog.Sinks.Console** (6.0.0): Escribir a consola (debug)

---

## 🚨 Troubleshooting

### Los logs no se crean

1. Verifica permisos de escritura en `%LOCALAPPDATA%\InkStudio\`
2. Revisa que `LoggingService.Inicializar()` se llame en `Program.cs`

### Los logs son muy grandes

- Los logs se rotan diariamente
- Se mantienen solo 30 días
- Puedes cambiar `retainedFileCountLimit` en `LoggingService.cs`

### No veo errores en el log

- Verifica que el nivel mínimo sea `Debug` o `Information`
- Algunos errores pueden estar en `Warning` en lugar de `Error`

---

> **Recuerda:** Los logs son tu mejor amigo para diagnosticar problemas. Siempre pide el log cuando un usuario reporte un error.

