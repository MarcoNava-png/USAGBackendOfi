using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs.Dashboard;
using WebApplication2.Core.Responses.Dashboard;
using WebApplication2.Core.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(roleClaim))
                {
                    return BadRequest(new { message = "El usuario no tiene un rol asignado" });
                }

                // Si tiene superadmin y también admin, usar admin para el dashboard
                if (roleClaim == "superadmin" && User.IsInRole("admin"))
                {
                    roleClaim = "admin";
                }

                var dashboard = await _dashboardService.GetDashboardAsync(userId, roleClaim);

                var response = new Response<DashboardResponseDto>
                {
                    Data = dashboard
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }

        [HttpGet("admin")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                var dashboard = await _dashboardService.GetAdminDashboardAsync();
                var response = new Response<AdminDashboardDto> { Data = dashboard };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }

        [HttpGet("director")]
        [Authorize(Roles = "admin,director")]
        public async Task<IActionResult> GetDirectorDashboard()
        {
            try
            {
                var dashboard = await _dashboardService.GetDirectorDashboardAsync();
                var response = new Response<DirectorDashboardDto> { Data = dashboard };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }

        [HttpGet("finanzas")]
        [Authorize(Roles = "admin,finanzas,director")]
        public async Task<IActionResult> GetFinanzasDashboard()
        {
            try
            {
                var dashboard = await _dashboardService.GetFinanzasDashboardAsync();
                var response = new Response<FinanzasDashboardDto> { Data = dashboard };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }

        [HttpGet("control-escolar")]
        [Authorize(Roles = "admin,controlescolar,director")]
        public async Task<IActionResult> GetControlEscolarDashboard()
        {
            try
            {
                var dashboard = await _dashboardService.GetControlEscolarDashboardAsync();
                var response = new Response<ControlEscolarDashboardDto> { Data = dashboard };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }

        [HttpGet("admisiones")]
        [Authorize(Roles = "admin,admisiones,controlescolar,director")]
        public async Task<IActionResult> GetAdmisionesDashboard()
        {
            try
            {
                var dashboard = await _dashboardService.GetAdmisionesDashboardAsync();
                var response = new Response<AdmisionesDashboardDto> { Data = dashboard };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }

        [HttpGet("coordinador")]
        [Authorize(Roles = "admin,coordinador,director,academico")]
        public async Task<IActionResult> GetCoordinadorDashboard()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var dashboard = await _dashboardService.GetCoordinadorDashboardAsync(userId);
                var response = new Response<CoordinadorDashboardDto> { Data = dashboard };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }

        [HttpGet("docente")]
        [Authorize(Roles = "admin,docente,coordinador,director,academico")]
        public async Task<IActionResult> GetDocenteDashboard()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var dashboard = await _dashboardService.GetDocenteDashboardAsync(userId);
                var response = new Response<DocenteDashboardDto> { Data = dashboard };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }

        [HttpGet("alumno")]
        [Authorize(Roles = "admin,alumno,controlescolar,director")]
        public async Task<IActionResult> GetAlumnoDashboard()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var dashboard = await _dashboardService.GetAlumnoDashboardAsync(userId);
                var response = new Response<AlumnoDashboardDto> { Data = dashboard };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el dashboard", error = ex.Message });
            }
        }
    }
}
