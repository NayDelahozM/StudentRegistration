using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace StudentRegistration.API.Helpers
{
    /// <summary>
    /// Helper para validación de autorización por estudiante
    /// Fix Problema #2: Autorización real por estudiante
    /// </summary>
    public static class AuthorizationHelper
    {
        /// <summary>
        /// Obtiene el studentId del claim del token JWT
        /// </summary>
        public static int? GetStudentIdFromToken(HttpContext httpContext)
        {
            var studentIdClaim = httpContext.User?.FindFirst("studentId");

            if (studentIdClaim != null && int.TryParse(studentIdClaim.Value, out int studentId))
            {
                return studentId;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el rol del usuario autenticado
        /// </summary>
        public static string GetUserRole(HttpContext httpContext)
        {
            return httpContext.User?.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Valida si el usuario tiene rol Admin
        /// </summary>
        public static bool IsAdmin(HttpContext httpContext)
        {
            return GetUserRole(httpContext) == "Admin";
        }

        /// <summary>
        /// Valida si el usuario autenticado puede acceder a los datos del estudiante especificado.
        /// Los Admins pueden acceder a cualquier estudiante.
        /// Los Estudiantes solo pueden acceder a sus propios datos.
        /// </summary>
        /// <param name="httpContext">Contexto HTTP actual</param>
        /// <param name="requestedStudentId">ID del estudiante que se intenta acceder</param>
        /// <returns>True si tiene acceso, false si no</returns>
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

                // Si el estudiante no tiene studentId en el token (registro antiguo), denegar acceso
                return false;
            }

            // Otros roles no tienen acceso
            return false;
        }

        /// <summary>
        /// Valida si el usuario autenticado puede inscribir al estudiante especificado.
        /// </summary>
        public static bool CanEnrollStudent(HttpContext httpContext, int estudianteId)
        {
            return CanAccessStudentData(httpContext, estudianteId);
        }
    }
}
