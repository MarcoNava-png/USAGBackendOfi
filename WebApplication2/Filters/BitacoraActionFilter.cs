using System.Security.Claims;
using System.Text.Json;
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
            ["Planes"] = "Finanzas",
            ["Departamento"] = "Catalogos",
            ["ReporteAcademico"] = "Academico",
            ["DocumentosSolicitudes"] = "Documentos",
        };

        // Controllers to skip logging (internal/read-only)
        private static readonly HashSet<string> SkipControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Dashboard", "Diagnostico", "ReportesGlobales", "BitacoraAccion",
            "NotificacionUsuario", "Notificaciones", "Bitacora", "Ubicacion",
            "SuperAdminAuth", "TenantAdmin", "Email", "Catalogos"
        };

        // Auth actions that need special handling (no JWT yet)
        private static readonly HashSet<string> AuthActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Login", "ForgotPassword", "ResetPassword"
        };

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;

            // Skip non-relevant methods
            if (method == "OPTIONS" || method == "HEAD")
            {
                await next();
                return;
            }

            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            var action = context.RouteData.Values["action"]?.ToString() ?? "";

            // Skip internal controllers
            if (SkipControllers.Contains(controller))
            {
                await next();
                return;
            }

            var executedContext = await next();

            // Only log successful operations (2xx status)
            if (executedContext.Exception != null) return;
            if (executedContext.Result is ObjectResult objResult && (objResult.StatusCode < 200 || objResult.StatusCode >= 300)) return;

            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var descripcion = $"{method} /api/{controller}/{action}";

            // Handle auth actions (Login, ForgotPassword, ResetPassword) - no JWT available
            if (AuthActions.Contains(action))
            {
                var email = ExtractEmailFromArgs(context);
                var accion = action switch
                {
                    "Login" => "INICIAR_SESION",
                    "ForgotPassword" => "SOLICITAR_RESET_PASSWORD",
                    "ResetPassword" => "RESTABLECER_PASSWORD",
                    _ => FormatAction(method, action)
                };

                try
                {
                    var bitacora = context.HttpContext.RequestServices.GetService<IBitacoraAccionService>();
                    if (bitacora != null)
                    {
                        await bitacora.RegistrarAsync(
                            email ?? "anonimo", email ?? "anonimo", accion, "Autenticacion",
                            controller, "", descripcion,
                            ip: ip);
                    }
                }
                catch { }
                return;
            }

            var userId = context.HttpContext.User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            var nombreUsuario = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? userId;
            var modulo = ControllerModuleMap.GetValueOrDefault(controller, "General");
            var entidadId = context.RouteData.Values.TryGetValue("id", out var idVal) ? idVal?.ToString() : null;
            var accionFinal = FormatAction(method, action);

            try
            {
                var bitacoraService = context.HttpContext.RequestServices.GetService<IBitacoraAccionService>();
                if (bitacoraService != null)
                {
                    await bitacoraService.RegistrarAsync(
                        userId, nombreUsuario, accionFinal, modulo,
                        controller, entidadId ?? "", descripcion,
                        ip: ip);
                }
            }
            catch
            {
                // Never let logging fail the request
            }
        }

        private static string? ExtractEmailFromArgs(ActionExecutingContext context)
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null) continue;

                var emailProp = arg.GetType().GetProperty("Email");
                if (emailProp != null)
                {
                    return emailProp.GetValue(arg)?.ToString();
                }
            }
            return null;
        }

        private static string FormatAction(string method, string actionName)
        {
            var prefix = method switch
            {
                "GET" => "CONSULTAR",
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
