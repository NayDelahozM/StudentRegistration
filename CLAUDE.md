# CLAUDE.md

Este archivo proporciona guía a Claude Code (claude.ai/code) para trabajar en este repositorio.

## Visión General del Proyecto

StudentRegistration es una **solución de prueba técnica** que implementa un sistema de inscripción de estudiantes usando **Clean Architecture** con .NET 8, SQL Server y autenticación JWT.

### Requisitos del Negocio
- Los estudiantes pueden registrarse en línea e inscribirse en máximo 3 materias (3 créditos cada una)
- 10 materias disponibles, cada una vale 3 créditos
- 5 profesores, cada uno dicta 2 materias
- Los estudiantes no pueden tener múltiples materias con el mismo profesor
- Los estudiantes pueden ver los registros de otros estudiantes y los compañeros de clase solo por nombre

## Comandos

### Compilar y Ejecutar
```bash
# Compilar toda la solución
dotnet build

# Ejecutar API (inicia en puerto http 5000, https 5001)
dotnet run --project StudentRegistration.API

# Ejecutar con hot reload en desarrollo
dotnet watch --project StudentRegistration.API
```

### Migraciones de Base de Datos
```bash
# Agregar migración
dotnet ef migrations add <NombreMigracion> -p StudentRegistration.Infrastructure -s StudentRegistration.API

# Aplicar migraciones a la base de datos
dotnet ef database update -p StudentRegistration.Infrastructure -s StudentRegistration.API

# Remover última migración (si no fue aplicada)
dotnet ef migrations remove -p StudentRegistration.Infrastructure -s StudentRegistration.API
```

### Pruebas
```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### Credenciales de Seed Data
```
Username: admin
Password: Admin123*
Rol: Admin
```

## Arquitectura

### Capas de Clean Architecture

```
Backend/
├── StudentRegistration.Domain/          # Lógica del núcleo, entidades, interfaces
│   ├── Entities/                        # Estudiante, Profesor, Materia, Inscripcion, Usuario
│   ├── Common/                          # Patrón Result, BaseEntity
│   └── Interfaces/                      # IUnitOfWork, interfaces de repositorios
│
├── StudentRegistration.Application/     # Lógica de negocio, DTOs, validadores, servicios
│   ├── Services/                        # AuthService, EstudianteService, InscripcionService
│   ├── DTOs/                            # Objetos de transferencia de datos
│   ├── Validators/                      # Reglas de FluentValidation
│   └── Mappings/                        # Perfiles de AutoMapper
│
├── StudentRegistration.Infrastructure/  # Acceso a datos, servicios externos
│   ├── Data/                            # ApplicationDbContext con seed data
│   ├── Repositories/                    # Implementaciones de repositorios + UnitOfWork
│   └── Services/                        # JwtService
│
└── StudentRegistration.API/             # Capa de presentación
    ├── Controllers/                     # Endpoints de API
    ├── Middleware/                      # ExceptionHandlingMiddleware
    └── Helpers/                         # AuthorizationHelper para control de acceso por estudiante
```

### Patrones de Diseño Clave

**UnitOfWork Pattern**: `IUnitOfWork` gestiona transacciones sobre repositorios. **Importante**: `CommitAsync()` internamente llama a `SaveChangesAsync()`, evita llamar a ambos en los servicios.

**Query Filters para Soft Delete**: Todas las entidades heredan de `BaseEntity` con propiedad `IsDeleted`. `ApplicationDbContext` aplica filtros globales para excluir automáticamente registros con soft-delete:

```csharp
modelBuilder.Entity<Estudiante>().HasQueryFilter(e => !e.IsDeleted);
```

**Patrón Result**: Los servicios retornan objetos `Result<T>` o `Result` con `IsSuccess`, `Message`, `Errors` y `Data`.

**Claims JWT**: Los tokens incluyen el claim personalizado `studentId` cuando `Usuario.EstudiantId` existe. Se usa para validación de autorización.

### Esquema de Base de Datos

**Entidades Core**:
- `Estudiante` → `Inscripcion` (1:N)
- `Materia` ← `ProfesorMateria` → `Profesor` (N:N via tabla puente)
- `Inscripcion` → vincula `Estudiante`, `Materia`, `Profesor`
- `Usuario` → enlace opcional a `Estudiante` (1:1, nullable)

**Seed Data** (auto-aplicado en `OnModelCreating`):
- 5 Profesores (ID 1-5)
- 10 Materias (ID 1-10, cada una 3 créditos)
- 10 relaciones ProfesorMateria (2 por profesor)
- 1 usuario Admin

### Lógica de Negocio Crítica

**InscripcionService** (`MAX_MATERIAS = 3`):
- Valida máximo 3 materias por estudiante
- Previene asignaciones duplicadas de profesor
- Usa transacciones para inscripción atómica
- `ValidateInscripcionAsync()` ejecuta todas las reglas de negocio antes de `InscribirAsync()`

**Autorización**:
- `AuthorizationHelper.CanAccessStudentData()` verifica si el estudiante puede acceder a datos de otro estudiante
- **Admins**: acceso completo a todos los estudiantes
- **Estudiantes**: solo pueden acceder a sus propios datos (el ID debe coincidir con el claim `studentId` del token)
- Aplicado en endpoints de `EstudiantesController` y `InscripcionesController`

**Importante**: Esta autorización restringe que los estudiantes vean datos de otros estudiantes. Sin embargo, **el requisito de negocio #8 indica que los estudiantes DEBERÍAN ver registros de otros estudiantes**. Este es un conflicto intencional que debe resolverse según los requisitos.

### Configuración

**Connection String**: `appsettings.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StudentRegistrationDB;Integrated Security=true;TrustServerCertificate=True"
}
```

**Configuración JWT**: Configurar en `appsettings.json`
- `SecretKey`: Debe ser ≥32 caracteres para HS256
- `ExpirationInHours`: 24 (por defecto)

**CORS**: Configurado para Angular (localhost:4200) y React (localhost:3000)

### Problemas Conocidos

1. **Typo en nombres de propiedades**: `EstudiantId` en lugar de `EstudianteId` en todo el codebase (aceptado para la evaluación)
2. **Gestión de transacciones**: Algunos servicios llaman a `SaveChangesAsync()` antes de `CommitAsync()`, creando guardados duplicados (problema #4)
3. **Problema N+1**: `EstudianteService.GetCompañerosAsync()` tiene queries N+1 al obtener compañeros (problema #5)
4. **Hashing de password**: Usa SHA256 (aceptable como demo, no es production-ready)

### Notas de Desarrollo

**Auto-migración**: En desarrollo, `Program.cs` aplica automáticamente las migraciones EF pendientes al inicio:

```csharp
await context.Database.MigrateAsync();
```

**Autenticación en Swagger**:
1. POST `/api/auth/login` o `/api/auth/register` para obtener token
2. Clic en botón "Authorize" en Swagger UI
3. Pegar token SIN el prefijo "Bearer " (Swagger lo agrega automáticamente)

**FluentValidation**: Validadores auto-registrados desde el assembly que contiene `CreateEstudianteValidator`

**Implementación de Soft Delete**: Para eliminar registros, usa `DeleteAsync()` del repositorio que establece `IsDeleted = true`. Los deletes directos en SQL omiten los query filters.
