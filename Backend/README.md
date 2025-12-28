# StudentRegistration (SQL Server) - Clean Architecture (.NET 8)

## Requisitos
- .NET SDK 8.x
- SQL Server (LocalDB / Express / Developer)
- Visual Studio 2022 (opcional)

## Configuración rápida
1. Edita `StudentRegistration.API/appsettings.json` y ajusta `ConnectionStrings:DefaultConnection`.
2. Restaurar paquetes:
   ```bash
   dotnet restore
   ```
3. Crear y aplicar migraciones (si no usas auto-migrate):
   ```bash
   dotnet ef migrations add InitialCreate -p StudentRegistration.Infrastructure -s StudentRegistration.API
   dotnet ef database update -p StudentRegistration.Infrastructure -s StudentRegistration.API
   ```
4. Ejecutar:
   ```bash
   dotnet run --project StudentRegistration.API
   ```

## Swagger
- https://localhost:5001 (en desarrollo abre swagger por defecto)

## Auth
- POST `/api/auth/register`
- POST `/api/auth/login`
- Copia el token y úsalo en Swagger: `Authorize` -> `Bearer <token>`

## Credenciales (seed)

Al aplicar migraciones, se crea un usuario administrador por defecto:

- **Username:** `admin`
- **Password:** `Admin123*`
- **Rol:** `Admin`
