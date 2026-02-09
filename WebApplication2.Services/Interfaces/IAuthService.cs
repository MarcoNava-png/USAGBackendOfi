using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;


namespace WebApplication2.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApplicationUser> Signup(ApplicationUser user, string password, List<string> roles);
        Task<UserLoginInfoDto> Login(string username, string password);
        Task<ApplicationUser> GetUserByEmail(string email);
        Task<ApplicationUser?> GetUserById(string id);
        Task<List<ApplicationUser>> GetAllUsers();
        Task<IList<string>> GetUserRoles(ApplicationUser user);
        Task RequestPasswordReset(string email);
        Task ResetPassword(string email, string newPassword, string token);
        Task AdminResetPassword(string userId, string newPassword);
        Task<ApplicationUser> UpdateUserProfile(ApplicationUser newUser);
        Task UpdateUserRolesAsync(string userId, List<string> newRoles);
        Task DeleteUser(string email);
        Task DeleteUserById(string id);
        Task<UserLoginInfoDto> RefreshToken(string userId);
    }
}
