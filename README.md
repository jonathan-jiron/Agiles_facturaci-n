# Ágiles Facturación – Sprint 1 (Base del Sistema) ✅

Sistema de facturación electrónica con arquitectura Onion, autenticación JWT, gestión de clientes/productos y integración preparada para SRI Ecuador.

## 🎯 Objetivos Sprint 1 (Completados)

✅ Arquitectura Onion implementada (Domain, Application, Infrastructure, WebAPI, UI)  
✅ Conexión SQL Server con Entity Framework Core  
✅ Autenticación con JWT (hash BCrypt, tokens con expiración)  
✅ CRUD Clientes (Backend REST + UI Blazor)  
✅ CRUD Productos con gestión por lotes  
✅ Dashboard interactivo con métricas  
✅ Interfaz Blazor WASM responsive  
✅ Seed de datos iniciales  
✅ Migraciones de base de datos consolidadas  
✅ Registro y visualización de actividad reciente (creación, edición y eliminación de clientes/productos)
- Actividad reciente dinámica: muestra eventos reales de acciones sobre clientes y productos.

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────┐
│  UI (Blazor WebAssembly)                │  ← Frontend SPA
│  - Autenticación (JWT en localStorage)  │
│  - Páginas: Login, Dashboard, Clientes  │
│  - Productos, gestión de lotes          │
└────────────┬────────────────────────────┘
             │ HTTP/JSON
             ▼
┌─────────────────────────────────────────┐
│  WebAPI (.NET 8)                        │  ← API REST
│  - Controllers (Auth, Clientes, etc.)   │
│  - JWT Bearer Authentication            │
│  - CORS configurado                     │
└────────────┬────────────────────────────┘
             │ Entity Framework Core
             ▼
┌─────────────────────────────────────────┐
│  Infrastructure                         │  ← Capa de datos
│  - ApplicationDbContext                 │
│  - Migrations                           │
│  - Seed inicial (DbInitializer)         │
└────────────┬────────────────────────────┘
             │ ADO.NET / SQL
             ▼
┌─────────────────────────────────────────┐
│  SQL Server                             │  ← Base de datos
│  - Usuarios (BCrypt hash)               │
│  - Clientes, Productos, Lotes           │
└─────────────────────────────────────────┘
             ▲
             │
┌─────────────┴────────────────────────────┐
│  Domain                                  │  ← Entidades de negocio
│  - Cliente, Producto, Lote, Usuario      │
└──────────────────────────────────────────┘
```

---

## 🚀 Instalación y Ejecución

### Requisitos
- .NET SDK 8.0+
- SQL Server (local o remoto)
- Visual Studio 2022 o VS Code
- Git

### 1️⃣ Clonar repositorio
```bash
git clone <url-repo>
cd Agiles_facturaci-n
```

### 2️⃣ Configurar cadena de conexión
```json
// WebAPI/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=AgilesFacturacion;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 3️⃣ Aplicar migraciones
```powershell
dotnet ef database update -p Infrastructure -s WebAPI
```

### 4️⃣ Ejecutar WebAPI
```powershell
cd WebAPI
dotnet run
# Escucha en http://localhost:5240
```

### 5️⃣ Ejecutar UI (nueva terminal)
```powershell
cd UI
dotnet run
# Abre http://localhost:5241
```

---

## 🔐 Autenticación

### Login
**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuario": {
    "id": 1,
    "username": "admin",
    "rol": "ADMIN"
  }
}
```

**Errores:**
- `400 BadRequest`: Datos inválidos (falta username/password o formato incorrecto)
- `401 Unauthorized`: Credenciales incorrectas

### Configuración JWT
```json
// WebAPI/appsettings.json
{
  "Jwt": {
    "Issuer": "AgilesFacturacion",
    "Audience": "AgilesFacturacionUI",
    "Key": "CLAVE-SUPER-SECRETA-REEMPLAZAR-POR-AMBIENTE",
    "ExpiresMinutes": 60
  },
  "Security": {
    "BcryptWorkFactor": 10
  }
}
```

**Claims del token:**
- `NameIdentifier`: ID del usuario
- `Name`: Username
- `Role`: Rol (ADMIN/USER)

**Protección de rutas:**
- Backend: `[Authorize]` en controladores
- Frontend: `@attribute [Authorize]` en páginas Blazor

---

## 📊 Base de Datos

### Tablas principales

#### Usuarios
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | int PK | Autoincremental |
| Username | nvarchar(50) UNIQUE | Nombre de usuario |
| PasswordHash | nvarchar(max) | Hash BCrypt (work factor 10) |
| Rol | nvarchar(20) | ADMIN / USER |

#### Clientes
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | int PK | Autoincremental |
| Identificacion | nvarchar(20) UNIQUE | Cédula/RUC/Pasaporte |
| NombreRazonSocial | nvarchar(200) | Nombre completo o razón social |
| Telefono | nvarchar(30) | Teléfono de contacto |
| Correo | nvarchar(150) | Email (opcional) |
| Direccion | nvarchar(250) | Dirección fiscal |
| RowVersion | rowversion | Control de concurrencia |

#### Productos
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | int PK | Autoincremental |
| Codigo | nvarchar(50) | Código interno del producto |
| Nombre | nvarchar(200) | Nombre comercial |
| Descripcion | nvarchar(500) | Descripción detallada |
| RowVersion | rowversion | Control de concurrencia |

#### Lotes
| Campo | Tipo | Descripción |
|-------|------|-------------|
| Id | int PK | Autoincremental |
| NumeroLote | nvarchar(100) | Identificador del lote |
| ProductoId | int FK | Relación con Producto |
| Cantidad | int | Unidades en stock |
| PrecioUnitario | decimal(18,2) | Precio por unidad |
| FechaIngreso | datetime | Fecha de entrada al inventario |
| FechaVencimiento | datetime | Fecha de vencimiento (opcional) |

#### EventoActividad
| Campo      | Tipo           | Descripción                        |
|------------|----------------|------------------------------------|
| Id         | int PK         | Autoincremental                    |
| Titulo     | nvarchar(100)  | Tipo de evento (creado, editado...)|
| Descripcion| nvarchar(250)  | Detalle del evento                 |
| Icono      | nvarchar(50)   | Icono FontAwesome                  |
| Color      | nvarchar(20)   | Color para UI                      |
| Fecha      | datetime       | Fecha y hora del evento            |

### Datos iniciales (Seed)
- **1 usuario**: `admin` / `admin123` (ROL: ADMIN)
- **4 clientes** de prueba
- **10 productos** con códigos PROD-001 a PROD-010
- **14 lotes** distribuidos entre productos

---

## 🎨 Interfaz de Usuario

### Páginas implementadas

#### `/login` (EmptyLayout)
- Formulario centrado en lado izquierdo (40% pantalla)
- Validación en tiempo real (DataAnnotations)
- Mensajes de error específicos (400 vs 401)
- Credenciales de prueba visibles
- Diseño con gradientes y sombras modernas

#### `/` (Dashboard)
- 4 tarjetas de métricas:
  - Clientes registrados (+12% vs mes anterior)
  - Productos en inventario (+8%)
  - Facturas emitidas (+24%)
  - Ventas del mes ($45,230.50, +18%)
- Accesos rápidos a módulos principales
- Fecha/hora actual
- Usuario y rol en navbar

#### `/clientes`
- Tabla paginada con búsqueda en tiempo real
- Filtros por identificación y nombre
- Modal para crear/editar (formulario reactivo)
- Validaciones: identificación única, email válido
- Confirmación de eliminación

#### `/productos`
- Gestión de productos y lotes en misma vista
- Tabla de productos con expansión de lotes
- Agregar/editar lotes por producto
- Cálculo de stock total automático
- Alertas de vencimiento (próximos 30 días)

### Componentes compartidos
- **MainLayout**: Sidebar colapsable + navbar superior
- **NavMenu**: Links con iconos Font Awesome
- **EmptyLayout**: Solo contenido (para login)
- **AuthStateProvider**: Gestión de estado de autenticación

---

## 🔧 Configuración adicional

### CORS
```csharp
// WebAPI/Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
        policy.WithOrigins("http://localhost:5241")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
```

### Migraciones
```powershell
# Crear nueva migración
dotnet ef migrations add NombreMigracion -p Infrastructure -s WebAPI

# Aplicar cambios
dotnet ef database update -p Infrastructure -s WebAPI

# Rollback a migración anterior
dotnet ef database update NombreMigracionAnterior -p Infrastructure -s WebAPI

# Eliminar última migración (si no se aplicó)
dotnet ef migrations remove -p Infrastructure -s WebAPI
```

### Publicación
```powershell
# WebAPI
dotnet publish WebAPI -c Release -o publish/api

# UI (con tree shaking)
dotnet workload install wasm-tools
dotnet publish UI -c Release -o publish/ui
```

---

## 📦 Dependencias principales

### WebAPI
- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- Microsoft.EntityFrameworkCore.Tools (8.0.0)
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.0)
- BCrypt.Net-Next (4.0.3)
- System.IdentityModel.Tokens.Jwt (6.35.0)

### UI
- Microsoft.AspNetCore.Components.WebAssembly (8.0.0)
- Microsoft.AspNetCore.Components.Authorization (8.0.0)
- System.IdentityModel.Tokens.Jwt (6.35.0)
- Microsoft.Extensions.Http (8.0.0)

---

## 🐛 Solución de problemas comunes

### Error: "UI.styles.css 404"
**Solución:**
```powershell
Remove-Item -Recurse -Force .\UI\bin,.\UI\obj
dotnet build UI
```

### Error: "Authorization failed"
**Causa:** Token expirado o no presente.
**Solución:** Volver a hacer login. El token dura 60 minutos.

### Login no redirige
**Causa:** AuthStateProvider no notifica cambio.
**Solución:** Verificar que `MarkUserAuthenticatedAsync()` se llama después de guardar token.

### CORS bloqueado
**Causa:** Puerto de UI diferente al configurado.
**Solución:** Actualizar `AllowUI` policy en `Program.cs` con puerto correcto.

### Migraciones fallan
**Causa:** Cambios en entidades sin migración.
**Solución:**
```powershell
dotnet ef migrations add FixSchema -p Infrastructure -s WebAPI
dotnet ef database update -p Infrastructure -s WebAPI
```

---

## 📈 Métricas del Sprint 1

| Métrica | Valor |
|---------|-------|
| Horas planificadas | 64 h |
| Horas ejecutadas | ~64 h |
| Historias completadas | 5/5 (100%) |
| Bugs encontrados | 0 críticos |
| Cobertura de código | N/A (sin tests aún) |
| Tamaño UI (dev) | 9.20 MB |
| Tamaño UI (publicado) | ~3 MB (con wasm-tools) |

---

## 🚧 Limitaciones conocidas (Sprint 1)

- ❌ Solo un usuario (admin) creado por seed, no hay UI para gestión de usuarios
- ❌ No hay Refresh Token (token expira en 60 min sin renovación automática)
- ❌ Contraseñas no tienen política de complejidad
- ❌ No hay auditoría de cambios (quién/cuándo modificó registros)
- ❌ DTOs exponen entidades directamente (sin capa de abstracción)
- ❌ Sin paginación server-side (todo en memoria)
- ❌ Sin tests unitarios ni de integración
- ❌ Sin logs estructurados (Serilog pendiente)
- ❌ Sin manejo global de errores con códigos correlacionados

---

## 🎯 Roadmap Sprint 2+

### Sprint 2 (próximos pasos inmediatos)
1. **Gestión de Usuarios**
   - CRUD de usuarios (solo ADMIN)
   - Cambio de contraseña
   - Bloqueo/desbloqueo de cuentas
   
2. **Módulo de Facturación**
   - Entidades: Factura, DetalleFactura
   - Numeración secuencial (establecimiento + punto emisión)
   - Cálculo de impuestos (IVA 0%/12%/15%)
   - Estados: Borrador, Emitida, Autorizada, Anulada

3. **Mejoras de seguridad**
   - Refresh Token
   - Política de contraseñas (mínimo 8 caracteres, mayúsculas, números)
   - Registro de intentos fallidos (bloqueo después de 5 intentos)

4. **Testing**
   - Unit tests (Application layer)
   - Integration tests (API endpoints con WebApplicationFactory)

### Sprint 3+
- Integración real SRI (firma electrónica, envío XML)
- Reportes (ventas por mes, top productos, clientes frecuentes)
- Exportación PDF/Excel
- Notificaciones por email
- Modo offline (PWA)
- Internacionalización (es/en)

---

## 👥 Equipo Sprint 1

| Integrante | Rol | Responsabilidades |
|------------|-----|-------------------|
| Joseph Cháchalo | Backend/BD | Migraciones, seed, documentación |
| Andrés Paredes | Arquitectura | Estructura Onion, DI, code reviews |
| Jonathan Jirón | Backend Developer | Controladores, autenticación, lógica de negocio |
| Erick López | Frontend Developer | Blazor UI, componentes, estilos |

---

## 📞 Contacto y soporte

- **Repositorio:** [GitHub URL]
- **Documentación técnica:** Ver carpeta `/docs` (pendiente Sprint 2)
- **Issues:** Reportar bugs en GitHub Issues
- **Wiki:** Guías detalladas en GitHub Wiki

---

## 📄 Licencia

[Definir licencia según proyecto: MIT, Apache 2.0, Propietaria, etc.]

---

**Última actualización:** 8 de noviembre de 2025  
**Versión:** Sprint 1 - Base del Sistema ✅
