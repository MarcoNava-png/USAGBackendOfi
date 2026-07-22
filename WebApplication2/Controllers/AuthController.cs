using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Auth;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;

namespace WebApplication2
{
    [Route("api/auth")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IMicrosoftGraphService _graphService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPlanLimiteService _planLimiteService;

        public AuthController(IAuthService authService, IBlobStorageService blobStorageService, IConfiguration configuration, IMapper mapper, IMicrosoftGraphService graphService, UserManager<ApplicationUser> userManager, IPlanLimiteService planLimiteService)
        {
            _authService = authService;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
            _mapper = mapper;
            _graphService = graphService;
            _userManager = userManager;
            _planLimiteService = planLimiteService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var userLoginInfoDto = await _authService.Login(request.Email, request.Password);

            var response = new Response<UserLoginInfoDto>
            {
                Data = userLoginInfoDto
            };

            return Ok(response);
        }

        [HttpPost("unlock/{email}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<IActionResult> UnlockUser(string email)
        {
            var user = await _authService.GetUserByEmail(email);
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            return Ok(new { success = true, message = $"Usuario {email} desbloqueado exitosamente." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _authService.RequestPasswordReset(request.Email);

                return Ok(new {
                    success = true,
                    message = "Si el correo existe en nuestro sistema, recibirás un enlace para restablecer tu contraseña."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error al procesar la solicitud", error = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPassword(request.Email, request.NewPassword, request.Token);

                return Ok(new {
                    success = true,
                    message = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión con tu nueva contraseña."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("admin-reset-password")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordRequest request)
        {
            try
            {
                await _authService.AdminResetPassword(request.UserId, request.NewPassword);

                return Ok(new {
                    success = true,
                    message = "Contraseña restablecida exitosamente."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("create-user")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<IActionResult> CreateUser(CreateUserRequest request)
        {
            var limiteError = await _planLimiteService.ValidarPuedeCrearUsuarioAsync();
            if (limiteError != null)
            {
                return BadRequest(new { message = limiteError });
            }

            // 1. Crear correo en Azure AD si se solicita
            string? azureUserId = null;
            if (request.CrearCorreoAzure)
            {
                var rolLabel = request.Roles.FirstOrDefault() ?? "Staff";
                var mailNickname = request.Email.Contains("@")
                    ? request.Email.Substring(0, request.Email.IndexOf("@"))
                    : request.Email;

                var graphRequest = new Core.Requests.MicrosoftGraph.CreateUserRequest
                {
                    DisplayName = $"{request.Nombres} {request.Apellidos}".Trim(),
                    UserPrincipalName = request.Email,
                    MailNickname = mailNickname,
                    GivenName = request.Nombres,
                    Surname = request.Apellidos,
                    Password = request.Password,
                    ForceChangePasswordNextSignIn = true,
                    JobTitle = rolLabel
                };

                var graphResult = await _graphService.CreateUserAsync(graphRequest);
                if (graphResult.Success)
                {
                    azureUserId = graphResult.UserId;
                }
                else if ((graphResult.Message ?? "").Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    var existente = await _graphService.GetUserByIdAsync(request.Email);
                    if (existente != null)
                        azureUserId = existente.Id;
                    else
                        return BadRequest(new { message = $"El correo ya existe en Microsoft 365 pero no se pudo vincular: {graphResult.Message}" });
                }
                else
                {
                    return BadRequest(new { message = $"Error al crear correo en Microsoft 365: {graphResult.Message}" });
                }
            }

            try
            {
                // 2. Crear usuario local
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    Nombres = request.Nombres,
                    Apellidos = request.Apellidos,
                    Telefono = request.Telefono,
                    Biografia = request.Biografia,
                    PhotoUrl = request.PhotoUrl,
                    IdCampusAsignado = request.IdCampusAsignado,
                    MustChangePassword = true
                };

                var createdUser = await _authService.Signup(user, request.Password, request.Roles);

                var userLoginInfoDto = await _authService.Login(request.Email, request.Password);

                var response = new Response<UserLoginInfoDto>
                {
                    Data = userLoginInfoDto
                };

                return Created($"/api/auth/user/{createdUser.Id}", response);
            }
            catch (Exception)
            {
                // Rollback: eliminar usuario de Azure AD si se creó
                if (!string.IsNullOrEmpty(azureUserId))
                {
                    await _graphService.DeleteUserAsync(azureUserId);
                }
                throw;
            }
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var userLoginInfoDto = await _authService.RefreshToken(userId);

                var response = new Response<UserLoginInfoDto>
                {
                    Data = userLoginInfoDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Usuario no autenticado" });

                var user = await _authService.GetUserById(userId);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                var userDto = _mapper.Map<ApplicationUserDto>(user);
                var roles = await _authService.GetUserRoles(user);
                userDto.Roles = roles.ToList();

                var response = new Response<ApplicationUserDto>
                {
                    Data = userDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener perfil", error = ex.Message });
            }
        }

        [HttpGet("users")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsers();
                var usersDto = _mapper.Map<List<ApplicationUserDto>>(users);

                foreach (var userDto in usersDto)
                {
                    var user = users.FirstOrDefault(u => u.Id == userDto.Id);
                    if (user != null)
                    {
                        var roles = await _authService.GetUserRoles(user);
                        userDto.Roles = roles.ToList();
                    }
                }

                var response = new Response<List<ApplicationUserDto>>
                {
                    Data = usersDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener usuarios", error = ex.Message });
            }
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _authService.GetUserById(id);

                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                var userDto = _mapper.Map<ApplicationUserDto>(user);

                var response = new Response<ApplicationUserDto>
                {
                    Data = userDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener usuario", error = ex.Message });
            }
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request, IFormFile? photoFile)
        {
            var currentUserId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Forbid();
            }

            var oldUser = await _authService.GetUserByEmail(request.Email);
            if (oldUser == null || oldUser.Id != currentUserId)
            {
                return Forbid();
            }

            var newUser = new ApplicationUser
            {
                Email = request.Email,
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Telefono = request.Telefono,
                Biografia = request.Biografia
            };

            if (photoFile != null && photoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var photoFileExtension = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(photoFileExtension))
                {
                    return BadRequest(new { message = "Solo se permiten imágenes JPG o PNG" });
                }

                if (photoFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "La imagen no puede ser mayor a 5MB" });
                }

                var photoFileName = $"{oldUser.Id}/profile{photoFileExtension}";
                var photoUrl = await _blobStorageService.UploadFile(photoFile, photoFileName, "photos");
                newUser.PhotoUrl = photoUrl;
            }

            try
            {
                var user = await _authService.UpdateUserProfile(newUser);
                var userDto = _mapper.Map<ApplicationUserDto>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el usuario. " + ex.Message);
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeOwnPasswordRequest request)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _authService.ChangeOwnPasswordAsync(userId, request.CurrentPassword, request.NewPassword);
                return Ok(new { message = "Contraseña actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("users/{id}")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserProfileRequest request)
        {
            try
            {
                var existingUser = await _authService.GetUserById(id);

                if (existingUser == null)
                    return NotFound(new { message = "Usuario no encontrado" });

                existingUser.Nombres = request.Nombres;
                existingUser.Apellidos = request.Apellidos;
                existingUser.Telefono = request.Telefono;
                existingUser.Biografia = request.Biografia;
                existingUser.IdCampusAsignado = request.IdCampusAsignado;

                // Actualizar email si cambió
                if (!string.IsNullOrEmpty(request.Email) && request.Email != existingUser.Email)
                {
                    await _authService.UpdateUserEmailAsync(id, request.Email);
                    existingUser.Email = request.Email;
                }

                var updatedUser = await _authService.UpdateUserProfile(existingUser);

                if (request.Roles != null && request.Roles.Count > 0)
                {
                    await _authService.UpdateUserRolesAsync(id, request.Roles);
                }

                var userDto = _mapper.Map<ApplicationUserDto>(updatedUser);
                var roles = await _authService.GetUserRoles(updatedUser);
                userDto.Roles = roles.ToList();

                var response = new Response<ApplicationUserDto>
                {
                    Data = userDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar usuario", error = ex.Message });
            }
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                await _authService.DeleteUserById(id);

                return Ok(new { message = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar usuario", error = ex.Message });
            }
        }

        [HttpGet("microsoft/licenses")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<IActionResult> GetLicenses()
        {
            try
            {
                var licenses = await _graphService.GetSubscribedSkusAsync();
                return Ok(new Response<List<Core.DTOs.MicrosoftGraph.LicenseInfoDto>> { Data = licenses });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("microsoft/licenses/{userId}")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<IActionResult> AssignLicense(string userId, [FromBody] AssignLicenseRequest request)
        {
            try
            {
                var result = await _graphService.AssignLicenseAsync(userId, request.SkuId);
                if (!result)
                    return BadRequest(new { message = "No se pudo asignar la licencia." });

                return Ok(new { message = "Licencia asignada exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class AssignLicenseRequest
    {
        public string SkuId { get; set; } = string.Empty;
    }
}
