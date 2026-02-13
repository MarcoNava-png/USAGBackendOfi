using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApplication2.Configuration.Constants;
using WebApplication2.Configuration.CustomExceptions;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebApplication2.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<ApplicationUser> Signup(ApplicationUser user, string password, List<string> roles)
        {
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(error => error.Description));

                throw new ValidationException(errors);
            }

            foreach (var role in roles)
            {
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, role));
                await _userManager.AddToRoleAsync(user, role);
            }

            return user;

        }

        public async Task<UserLoginInfoDto> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) throw new ValidationException(ErrorConstants.INVALID_CREDENTIALS);

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);

            if (!result.Succeeded)
            {
                throw new ValidationException(ErrorConstants.INVALID_CREDENTIALS);
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Priorizar admin como rol principal para el frontend
            var role = roles.Contains("admin") ? "admin" : roles.FirstOrDefault() ?? "Usuario";

            var userLoginTokenDto = GetUserLoginToken(user, role, roles.ToList());

            return userLoginTokenDto;

        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            return user;
        }

        public async Task<ApplicationUser?> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user;
        }

        public async Task RequestPasswordReset(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return;
            }

            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
            var encodedToken = Uri.EscapeDataString(passwordResetToken);
            var encodedEmail = Uri.EscapeDataString(email);

            var resetUrl = $"{frontendUrl}/auth/reset-password?token={encodedToken}&email={encodedEmail}";

            await _emailService.SendPasswordResetEmailAsync(email, passwordResetToken, resetUrl);
        }

        public async Task ResetPassword(string email, string newPassword, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new Exception($"Error al restablecer contraseña: {errors}");
            }
        }

        public async Task AdminResetPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new Exception($"Error al restablecer contraseña: {errors}");
            }
        }

        public async Task<ApplicationUser> UpdateUserProfile(ApplicationUser newUser)
        {
            var user = await _userManager.FindByEmailAsync(newUser.Email);

            user.Nombres = newUser.Nombres;
            user.Apellidos = newUser.Apellidos;
            user.Telefono = newUser.Telefono;
            user.Biografia = newUser.Biografia;

            if (!string.IsNullOrEmpty(newUser.PhotoUrl))
            {
                user.PhotoUrl = newUser.PhotoUrl;
            }

            await _userManager.UpdateAsync(user);

            return user;
        }

        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            return await Task.FromResult(users);
        }

        public async Task<IList<string>> GetUserRoles(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task UpdateUserEmailAsync(string userId, string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("Usuario no encontrado.");

            // Verificar que el nuevo email no esté en uso
            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new Exception("El correo electrónico ya está en uso por otro usuario.");
            }

            user.Email = newEmail;
            user.NormalizedEmail = newEmail.ToUpperInvariant();
            user.UserName = newEmail;
            user.NormalizedUserName = newEmail.ToUpperInvariant();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new Exception($"Error al actualizar email: {errors}");
            }
        }

        public async Task UpdateUserRolesAsync(string userId, List<string> newRoles)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("Usuario no encontrado.");

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove old roles
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                var existingClaims = await _userManager.GetClaimsAsync(user);
                foreach (var claim in existingClaims.Where(c => c.Type == ClaimTypes.Role))
                {
                    await _userManager.RemoveClaimAsync(user, claim);
                }
            }

            // Add new roles
            foreach (var role in newRoles)
            {
                await _userManager.AddToRoleAsync(user, role);
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, role));
            }
        }

        public async Task DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            await _userManager.DeleteAsync(user);
        }

        public async Task DeleteUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            await _userManager.DeleteAsync(user);
        }

        public async Task<UserLoginInfoDto> RefreshToken(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("Usuario no encontrado.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains("admin") ? "admin" : roles.FirstOrDefault() ?? "alumno";

            return GetUserLoginToken(user, role, roles.ToList());
        }

        private UserLoginInfoDto GetUserLoginToken(ApplicationUser user, string primaryRole, List<string> allRoles)
        {
            var claims = new List<Claim>
            {
                new Claim("userId", user.Id),
                new Claim(ClaimTypes.Name, $"{user.Nombres} {user.Apellidos}".Trim()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Agregar TODOS los roles al JWT
            foreach (var r in allRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }

            var expiration = DateTime.UtcNow.AddHours(1);

            var token = BuildToken(claims, expiration);

            return new UserLoginInfoDto()
            {
                UserId = user.Id,
                Email = user.Email,
                Nombres = user.Nombres,
                Apellidos = user.Apellidos,
                Telefono = user.Telefono,
                Biografia = user.Biografia,
                Role = primaryRole,
                Token = token,
                Expiration = expiration,
                PhotoUrl = user.PhotoUrl,
            };
        }

        private string BuildToken(List<Claim> claims, DateTime expiration)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
