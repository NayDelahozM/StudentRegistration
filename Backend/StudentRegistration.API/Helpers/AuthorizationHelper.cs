using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace StudentRegistration.API.Helpers
{
    public static class AuthorizationHelper
    {
        // OBTIENE studentId del claim "studentId" del JWT (agregado en JwtService)
        // RETURNS: null si el claim no existe (tokens antiguos sin studentId)
        public static int? GetStudentIdFromToken(HttpContext httpContext)
        {
            var studentIdClaim = httpContext.User?.FindFirst("studentId");

            if (studentIdClaim != null && int.TryParse(studentIdClaim.Value, out int studentId))
            {
                return studentId;
            }

            return null;
        }

        public static string GetUserRole(HttpContext httpContext)
        {
            return httpContext.User?.FindFirst(ClaimTypes.Role)?.Value;
        }

        public static bool IsAdmin(HttpContext httpContext)
        {
            return GetUserRole(httpContext) == "Admin";
        }

        // AUTORIZACIÓN POR RECURSO: Valida si el usuario puede acceder a datos del estudiante
        // ADMIN: Acceso total a cualquier estudiante
        // ESTUDIANTE: Solo puede acceder a sus propios datos (studentId debe coincidir)
        //
        // PREVIENE: IDOR (Insecure Direct Object Reference) - estudiante operando datos de otro
        //
        // RIESGO: Si studentId es null (token antiguo), deniega acceso por seguridad
        // SOLUTION: Usuario debe re-loguearse para obtener token con studentId
        public static bool CanAccessStudentData(HttpContext httpContext, int requestedStudentId)
        {
            var userRole = GetUserRole(httpContext);

            // Los admins pueden acceder a cualquier estudiante
            if (userRole == "Admin")
            {
                return true;
            }

            // Los estudiantes solo pueden acceder a sus propios datos
            if (userRole == "Estudiante")
            {
                var studentId = GetStudentIdFromToken(httpContext);

                if (studentId.HasValue)
                {
                    return studentId.Value == requestedStudentId;
                }

                // Token antiguo sin studentId - denegar por seguridad
                // Usuario debe re-loguearse para obtener token actualizado con studentId
                return false;
            }

            // Otros roles no tienen acceso
            return false;
        }

        public static bool CanEnrollStudent(HttpContext httpContext, int estudianteId)
        {
            return CanAccessStudentData(httpContext, estudianteId);
        }
    }
}
