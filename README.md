# Student Registration System

Sistema universitario de gestión de inscripciones de estudiantes con control de acceso basado en roles y validación de reglas de negocio.

**Estado:** ✅ Listo para producción
**Última actualización:** Diciembre 2025
**Versión:** 1.0.0

---

## 1. Descripción del Sistema

### Problema de Negocio Resuelto
Permite a estudiantes universitarios inscribirse en materias con las siguientes restricciones académicas:
- Máximo 3 materias por estudiante
- Máximo 9 créditos (3 materias × 3 créditos)
- No se puede repetir profesor en diferentes materias
- Visibilidad limitada de datos entre estudiantes (solo nombre y apellido)

### Usuarios y Permisos

| Rol | Permisos | Acceso |
|-----|----------|--------|
| **Estudiante** | - Inscribirse en máximo 3 materias<br>- Cancelar inscripciones propias<br>- Ver sus propias inscripciones<br>- Ver compañeros de clase (solo mismo nombre/apellido)<br>- Ver y editar su perfil | Dashboard, Mis Inscripciones, Mi Perfil, Compañeros |
| **Admin** | - Gestionar todos los estudiantes (CRUD completo)<br>- Ver todas las inscripciones<br>- Ver perfil completo de estudiantes<br>- Crear nuevos estudiantes | Dashboard, Estudiantes, Todas las Inscripciones, Perfiles completos |



## 2. Arquitectura

### Clean Architecture (.NET 8)

```
┌─────────────────────────────────────────────────────┐
│                     PRESENTATION                      │
│  ┌──────────────┐  ┌──────────────────────────────┐  │
│  │ Controllers  │  │        Angular SPA            │  │
│  │  (API Layer)  │  │  - Components                 │  │
│  │              │  │  - Guards (RBAC)               │  │
│  │              │  │  - Services                   │  │
│  └──────┬───────┘  └──────────────────────────────┘  │
└─────────┼──────────────────────────────────────────┘
          │
┌─────────┴──────────────────────────────────────────┐
│                  APPLICATION                         │
│  ┌──────────────┐  ┌──────────────┐                 │
│  │   Services   │  │    DTOs      │                 │
│  │              │  │              │                 │
│  │  Business    │  │   Mappings   │                 │
│  │  Logic       │  │              │                 │
│  └──────┬───────┘  └──────────────┘                 │
└─────────┼──────────────────────────────────────────┘
          │
┌─────────┴──────────────────────────────────────────┐
│                     DOMAIN                           │
│  ┌──────────────────────────────────────────────┐  │
│  │  Entities (Estudiante, Materia, Profesor)    │  │
│  │  Interfaces (IUnitOfWork, IRepositories)     │  │
│  │  Common (Result, BaseEntity)                │  │
│  └──────────────────────────────────────────────┘  │
└─────────┼──────────────────────────────────────────┘
          │
┌─────────┴──────────────────────────────────────────┐
│                  INFRASTRUCTURE                     │
│  ┌──────────────┐  ┌──────────────┐                 │
│  │ Repositories │  │  DbContext   │                 │
│  │ (EF Core)     │  │              │                 │
│  │              │  │  JWT Service  │                 │
│  └──────────────┘  └──────────────┘                 │
└────────────────────────────────────────────────────┘
```

### Responsabilidad por Capa

| Capa | Responsabilidad | No Dependencias De |
|------|----------------|---------------------|
| **Domain** | Entidades de negocio, interfaces, reglas | Ninguna (puro C#) |
| **Application** | Lógica de negocio, DTOs, mapeos | Domain |
| **Infrastructure** | DB (EF Core), JWT, repositorios | Domain, Application |
| **API (Controllers)** | HTTP, autorización, orquestación | Application, Infrastructure |
| **Frontend (Angular)** | UI, guards RBAC, consumo API | Ninguna del backend |

### Flujo de Petición Real

```
Usuario (Angular)                                    Database (SQL Server)
    │                                                       │
    ├─ 1. Click "Inscribir" ─────────────────────────────┐│
    │                                                   ││
    ├─ 2. authGuard: Verifica JWT ─────────────────────┐││
    │                                                   │││
    ├─ 3. POST /api/inscripciones ────────────────────────┼┼┐
    │    │                                                │││
    │    ├─ 4. InscripcionesController.Inscribir()     ││││
    │    │                                                │││
    │    ├─ 5. AuthorizationHelper.CanAccessStudentData()││││
    │    │    │  Valida: JWT studentId == dto.estudianteId││││
    │    │    │  PREVIENE: IDOR (authorization bypass)  ││││
    │    │    └─────────────────────────────────────┘ │││
    │    │                                                │││
    │    ├─ 6. InscripcionService.InscribirAsync()     ││││
    │    │    │  Validar: MAX_MATERIAS = 3               ││││
    │    │    │  Validar: MAX_CREDITOS = 9               ││││
    │    │    │  Validar: NO repetir profesor            ││││
    │    │    │  Iniciar: BeginTransactionAsync()       ││││
    │    │    └──────────────────────────────────────┐ │││
    │    │                                           │ │││
    │    ├─ 7. UnitOfWork.AddAsync(inscripcion) ─────┼─┼┼┼── INSERT
    │    │                                           │ │││
    │    ├─ 8. UnitOfWork.CommitAsync() ────────────┼─┼┼── COMMIT
    │    │    │  SaveChangesAsync + Commit           │ │││
    │    │    │  ATÓMICO: Todo o nada                │ │││
    │    │    └──────────────────────────────────────┘ │││
    │    │                                                │││
    │    └─ 9. Return 201 Created ─────────────────────┼─┼┘
    │                                                     ││
    └────────────────────────────────────────────────────┘│
                                                       │
```

---

## 3. Stack Tecnológico

### Backend (.NET 8)

| Componente | Tecnología | Versión |
|------------|------------|---------|
| **Lenguaje** | C# | .NET 8.0 |
| **Framework** | ASP.NET Core Web API | 8.0 |
| **ORM** | Entity Framework Core | 8.0 |
| **Base de Datos** | SQL Server | LocalDB / Express / Developer |
| **Autenticación** | JWT Bearer (System.IdentityModel.Tokens) | 8.0 |
| **Validación** | FluentValidation | 11.9 |
| **Mapeo** | AutoMapper | 13.0 |
| **Logging** | ILogger (Microsoft.Extensions.Logging) | 8.0 |
| **API Documentation** | Swashbuckle.AspNetCore | 6.5 |

### Frontend (Angular)

| Componente | Tecnología | Versión |
|------------|------------|---------|
| **Framework** | Angular | 17+ |
| **TypeScript** | TypeScript | 5.x |
| **HTTP Client** | HttpClientModule | @angular/common/http |
| **Forms** | Reactive Forms | @angular/forms |
| **Routing** | Angular Router | @angular/router |
| **RxJS** | Reactive Extensions | 7.x |

### Librerías Críticas

**Backend:**
- `BCrypt.Net-Next` - Hash de contraseñas
- `Microsoft.EntityFrameworkCore.SqlServer` - Provider SQL Server
- `Microsoft.IdentityModel.Tokens` - JWT tokens
- `FluentValidation.AspNetCore` - Validación de DTOs
- `AutoMapper.Extensions.Microsoft.DependencyInjection` - DI AutoMapper

**Frontend:**
- Todas son dependencias estándar de Angular (no第三方 críticos)


## 4. Seguridad

### Autenticación (JWT)

**Flujo:**
1. Usuario hace POST `/api/auth/login` con username/password
2. `AuthService` valida contra hash BCrypt en DB
3. `JwtService` genera token con claims:
   - `sub`: UsuarioId
   - `email`: Email
   - `unique_name`: Username
   - `role`: Rol ("Admin" o "Estudiante")
   - **`studentId`**: EstudianteId (si aplica)**
4. Token expira en **24 horas**

**Código:** `JwtService.cs:22-53`, `AuthService.cs:88-148`

### Autorización por Recurso

**Implementación:**
- **Admin**: Puede acceder a cualquier estudiante
- **Estudiante**: Solo puede acceder a SUS propios datos (studentId debe coincidir)
- **Validación**: `AuthorizationHelper.CanAccessStudentData(HttpContext, estudianteId)`
- **Prevención**: IDOR (Insecure Direct Object Reference) - estudiante no puede operar datos de otro

**Ejemplo:**
```csharp
// InscripcionesController.cs:48-52
if (!AuthorizationHelper.CanAccessStudentData(HttpContext, dto.EstudianteId))
{
    return StatusCode(403, new { message = "No tienes permiso..." });
}
```

**Código:** `AuthorizationHelper.cs:42-69`, `InscripcionesController.cs:48-52,83-87`

### RBAC Frontend (Angular Guards)

| Guard | Protege | Rutas |
|-------|---------|-------|
| `authGuard` | Usuarios autenticados | Dashboard, Perfil, etc. |
| `adminGuard` | Solo Admin | `/estudiantes`, `/todas-inscripciones` |
| `estudianteGuard` | Solo Estudiantes | `/companeros` |
| Redirección 403 | Unauthorized page con UX profesional | `/unauthorized` |

**Código:** `auth.guard.ts:8-79`, `app.routes.ts:27-37,57-60`

### Datos NO Visibles por Estudiantes

| Dato | Admin Ve | Estudiante Ve | Razón |
|------|----------|---------------|--------|
| **Email de otros estudiantes** | ✅ | ❌ | Privacidad (GDPR) |
| **Teléfono** | ✅ | ❌ | Privacidad |
| **Dirección** | ✅ | ❌ | Privacidad |
| **Nombre completo** | ✅ | ✅ | Necesario para identificación básica |
| **Inscripciones de otros** | ✅ | ❌ | Privacidad académica |

**DTO que implementa esto:** `CompañeroClaseDto` (solo `EstudianteNombre`)

---

## 5. Base de Datos

### Modelo Lógico

```
┌─────────────────┐
│  Usuarios        │
│  ─────────────── │
│  UsuarioId (PK)  │
│  Username        │
│  Email           │
│  PasswordHash    │
│  Rol             │
│  EstudianteId    │◄─────────┐
└─────────────────┘           │
                              │
┌─────────────────┐           │
│  Estudiantes     │           │
│  ─────────────── │           │
│  EstudianteId(PK)│           │
│  Nombre          │           │
│  Apellido        │           │
│  Email           │           │
│  Telefono        │           │
│  Activo          │           │
└─────────────────┘           │
                              │
         ┌────────────────────┴──────┬────────────────┐
         │                             │                │
┌────────▼────────┐        ┌─────────▼─────────┐  ┌───────────▼────────┐
│  Materias        │        │  Profesores         │  │  ProfesorMaterias  │
│  ────────────────│        │  ─────────────────│  │  ───────────────────│
│  MateriaId (PK)  │        │  ProfesorId (PK)    │  │  ProfesorMateriaId(PK)│
│  Codigo          │        │  Nombre             │  │  ProfesorId (FK)     │
│  Nombre          │        │  Apellido           │  │  MateriaId (FK)      │
│  Creditos (3)    │        │  Email              │  │  (1 profesor por    │
│  Descripcion     │        └────────────────────┘  │   materia)          │
└──────────────────┘                              └────────────────────┘
         ▲                                              ▲
         │                                              │
         └──────────────────────────────────────────────┘
                              │
                    ┌─────────▼──────────────┐
                    │   Inscripciones         │
                    │   ─────────────────────│
                    │   InscripcionId (PK)    │
                    │   EstudianteId (FK)     │
                    │   MateriaId (FK)        │
                    │   ProfesorId (FK)        │
                    └──────────────────────────┘
```

### Restricciones Críticas

| Tabla | Restricción | SQL Implementation |
|-------|-------------|---------------------|
| **Inscripciones** | UNIQUE(EstudianteId, MateriaId) con soft delete | `ApplicationDbContext.cs:99-100` |
| **ProfesorMaterias** | UNIQUE(ProfesorId, MateriaId) con soft delete | `ApplicationDbContext.cs:84-85` |
| **Usuarios** | UNIQUE(Username) con soft delete | `ApplicationDbContext.cs:123` |
| **Usuarios** | UNIQUE(Email) con soft delete | `ApplicationDbContext.cs:124` |
| **Estudiantes** | UNIQUE(Email) con soft delete | `ApplicationDbContext.cs:43` |

### Índices de Performance

```sql
CREATE INDEX IX_Inscripciones_EstudianteId ON Inscripciones(EstudianteId);
CREATE INDEX IX_Inscripciones_MateriaId ON Inscripciones(MateriaId);
CREATE INDEX IX_Inscripciones_ProfesorId ON Inscripciones(ProfesorId);
```

**Ubicación:** `ApplicationDbContext.cs:101-103`

---

## 6. Cómo Ejecutar el Sistema

### Clonar el Repositorio

```bash
# Clonar el proyecto desde git
git clone https://github.com/NayDelahozM/StudentRegistration.git
cd StudentRegistration
```

### Requisitos Previos

1. **SDK .NET 8**: [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **SQL Server**: LocalDB, Express o Developer
3. **Node.js 18+**: [Descargar](https://nodejs.org/)
4. **Angular CLI**: `npm install -g @angular/cli`

### Backend

```bash
# Navegar al backend
cd Backend

# Restaurar paquetes
dotnet restore

# Configurar conexión en appsettings.json
# "ConnectionStrings": {
#   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StudentRegistrationDB;Trusted_Connection=True;"
# }

# Ejecutar migraciones (automático en desarrollo)
dotnet run --project StudentRegistration.API

# API estará en:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001
# - Swagger: https://localhost:5001/swagger
```

### Frontend

```bash
# Navegar al frontend
cd frontend

# Instalar dependencias
npm install

# Ejecutar en desarrollo
ng serve

# Aplicación estará en:
# http://localhost:4200
```

### Variables de Entorno

**Backend** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=StudentRegistrationDB;Trusted_Connection=True;"
  },
  "Jwt": {
    "SecretKey": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "StudentRegistrationAPI",
    "Audience": "StudentRegistrationClient"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  }
}
```

**Frontend** (no variables - está hardcoded en `auth.service.ts:7`):
```typescript
private apiUrl = 'http://localhost:5000/api';
```

**NOTA:** Cambiar `auth.service.ts:7` antes de producción.

### Migraciones

**Automático en desarrollo** (`Program.cs:178-190`):
- Se ejecuta al iniciar la API
- Aplica migrations pendientes
- Crea seed data (admin, 5 profesores, 10 materias)

**Manual en producción:**
```bash
dotnet ef migrations add InitialCreate -p StudentRegistration.Infrastructure -s StudentRegistration.API
dotnet ef database update -p StudentRegistration.Infrastructure -s StudentRegistration.API
```

### Puertos

| Servicio | Puerto | URL |
|----------|--------|-----|
| Backend HTTP | 5000 | http://localhost:5000 |
| Backend HTTPS | 5001 | https://localhost:5001 |
| Swagger | 5001 | https://localhost:5001/swagger |
| Frontend | 4200 | http://localhost:4200 |

---

## 7. Cómo Probarlo

### Usuarios de Prueba (Seed Data)

| Rol | Username | Password | ID |
|-----|----------|----------|-----|
| **Admin** | `admin` | `Admin123*` | 1 |
| **Estudiante** | Registrar nuevo usuario en `/register` | - | - |

### Casos de Prueba Clave

#### 1. Inscripción Exitosa (Estudiante)
```bash
# 1. Login como estudiante
POST /api/auth/login
{
  "username": "estudiante_test",
  "password": "Password123*"
}

# 2. Obtener materias disponibles
GET /api/inscripciones/materias-disponibles/1
Authorization: Bearer <token>

# 3. Inscribirse en 3 materias (NO mismo profesor)
POST /api/inscripciones
Authorization: Bearer <token>
{
  "estudianteId": 1,
  "materiaIds": [1, 3, 5]
}

# Expected: 201 Created
```

#### 2. Prevención: Exceder Máximo de Materias
```bash
POST /api/inscripciones
Authorization: Bearer <token_estudiante_con_3_materias>
{
  "estudianteId": 1,
  "materiaIds": [2]
}

# Expected: 400 Bad Request
# "Ya tiene 3 materia(s). Máximo: 3"
```

#### 3. Prevención: Repetir Profesor
```bash
POST /api/inscripciones
Authorization: Bearer <token_estudiante_con_materia_1>
{
  "estudianteId": 1,
  "materiaIds": [2]
}

# Expected: 400 Bad Request
# "Ya tiene una materia con el profesor María González"
```

#### 4. IDOR Prevention (Estudiante intenta inscribir a otro)
```bash
POST /api/inscripciones
Authorization: Bearer <token_estudiante_1>
{
  "estudianteId": 2,  # OTRO estudiante
  "materiaIds": [1]
}

# Expected: 403 Forbidden
# "No tienes permiso para inscribir a este estudiante"
```

#### 5. Admin ve Todos los Estudiantes
```bash
GET /api/estudiantes
Authorization: Bearer <token_admin>

# Expected: 200 OK
# Array con EstudianteSummaryDto (solo nombre + apellido, sin email)
```

#### 6. Estudiante NO ve Email de Compañeros
```bash
GET /api/estudiantes/1/companeros
Authorization: Bearer <token_estudiante>

# Expected: 200 OK
# Array con { estudianteNombre: "Juan Perez", materiaNombre: "Programación I" }
# NOTA: NO incluye email, teléfono, dirección
```

### Endpoints Críticos

| Método | Endpoint | Autenticación | Autorización |
|--------|----------|---------------|--------------|
| `POST` | `/api/auth/login` | ❌ No | - |
| `POST` | `/api/auth/register` | ❌ No | - |
| `GET` | `/api/estudiantes` | ✅ Sí | Solo Admin |
| `GET` | `/api/estudiantes/{id}` | ✅ Sí | Admin o propio estudiante |
| `GET` | `/api/estudiantes/{id}/companeros` | ✅ Sí | Admin o propio estudiante |
| `POST` | `/api/inscripciones` | ✅ Sí | Admin o propio estudiante |
| `DELETE` | `/api/inscripciones/{id}` | ✅ Sí | Admin o propio estudiante |
| `GET` | `/api/inscripciones/todas` | ✅ Sí | Solo Admin |
| `GET` | `/api/inscripciones/materias-disponibles/{id}` | ✅ Sí | Admin o propio estudiante |

---


## Archivos de Configuración Clave

| Archivo | Propósito |
|---------|-----------|
| `Backend/StudentRegistration.API/appsettings.json` | Connection string, JWT, CORS |
| `Backend/StudentRegistration.API/Program.cs` | Startup, DI, middleware, Swagger |
| `Backend/StudentRegistration.Infrastructure/Data/ApplicationDbContext.cs` | Seed data, mappings, constraints |
| `frontend/src/app/services/auth.service.ts` | API base URL (línea 7) |
| `frontend/src/app/guards/auth.guard.ts` | RBAC logic |

---

## Soporte

- **Documentación API Swagger**: https://localhost:5001/swagger (desarrollo)
- **Base de Conocimiento**: Ver código fuente (comentarios de alto valor ingenieril)





