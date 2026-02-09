using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Filters
{
    public class BitacoraActionFilter : IAsyncActionFilter
    {
        private static readonly Dictionary<string, string> ControllerModuleMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Aspirante"] = "Admisiones",
            ["DocumentoEstudiante"] = "Documentos",
            ["EstudiantePanel"] = "Estudiantes",
            ["Estudiante"] = "Estudiantes",
            ["Pagos"] = "Pagos",
            ["Caja"] = "Pagos",
            ["Recibos"] = "Finanzas",
            ["Plantillas"] = "Finanzas",
            ["Conceptos"] = "Finanzas",
            ["Beca"] = "Finanzas",
            ["BecaCatalogo"] = "Finanzas",
            ["Grupo"] = "Academico",
            ["Profesor"] = "Academico",
            ["Calificaciones"] = "Calificaciones",
            ["Asistencia"] = "Academico",
            ["Inscripcion"] = "Inscripciones",
            ["PeriodoAcademico"] = "Catalogos",
            ["PlanEstudios"] = "Catalogos",
            ["MateriaPlan"] = "Catalogos",
            ["Campus"] = "Catalogos",
            ["Catalogos"] = "Catalogos",
            ["Parciales"] = "Calificaciones",
            ["Auth"] = "Autenticacion",
            ["Permission"] = "Configuracion",
            ["Convenio"] = "Admisiones",
            ["Importacion"] = "Estudiantes",
            ["Director"] = "Academico",
            ["Coordinador"] = "Academico",
        };

        // Controllers to skip logging
        private static readonly HashSet<string> SkipControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Dashboard", "Diagnostico", "ReportesGlobales", "BitacoraAccion",
            "NotificacionUsuario", "Notificaciones", "Bitacora", "Ubicacion",
            "SuperAdminAuth", "TenantAdmin", "Email"
        };

        // Actions to skip (GET-like POSTs that are read-only)
        private static readonly HashSet<string> SkipActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Login", "Register", "RefreshToken", "ForgotPassword", "ResetPassword"
        };

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;

            // Only log modifying operations
            if (method == "GET" || method == "OPTIONS" || method == "HEAD")
            {
                await next();
                return;
            }

            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            var action = context.RouteData.Values["action"]?.ToString() ?? "";

            // Skip certain controllers and actions
            if (SkipControllers.Contains(controller) || SkipActions.Contains(action))
            {
                await next();
                return;
            }

            var executedContext = await next();

            // Only log successful operations (2xx status)
            if (executedContext.Exception != null) return;
            if (executedContext.Result is ObjectResult objResult && (objResult.StatusCode < 200 || objResult.StatusCode >= 300)) return;

            var userId = context.HttpContext.User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            var nombreUsuario = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? userId;
            var modulo = ControllerModuleMap.GetValueOrDefault(controller, "General");
            var entidadId = context.RouteData.Values.TryGetValue("id", out var idVal) ? idVal?.ToString() : null;
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();

            var descripcion = $"{method} /api/{controller}/{action}";
            var accion = FormatAction(method, action);

            try
            {
                var bitacora = context.HttpContext.RequestServices.GetService<IBitacoraAccionService>();
                if (bitacora != null)
                {
                    await bitacora.RegistrarAsync(
                        userId, nombreUsuario, accion, modulo,
                        controller, entidadId ?? "", descripcion,
                        ip: ip);
                }
            }
            catch
            {
                // Never let logging fail the request
            }
        }

        private static string FormatAction(string method, string actionName)
        {
            var prefix = method switch
            {
                "POST" => "CREAR",
                "PUT" => "ACTUALIZAR",
                "PATCH" => "ACTUALIZAR",
                "DELETE" => "ELIMINAR",
                _ => method
            };

            // Convert PascalCase action to UPPER_SNAKE_CASE
            var snake = System.Text.RegularExpressions.Regex.Replace(actionName, "([a-z])([A-Z])", "$1_$2");
            return $"{prefix}_{snake}".ToUpperInvariant();
        }
    }
}
