using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Models;

namespace WebApplication2.Data.Seed
{
    public static class UserSeed
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            var seedPassword = Environment.GetEnvironmentVariable("SEED_DEFAULT_PASSWORD") ?? "Dev@2024Secure!";

            await userManager.InsertUserAsync("admin@usag.com", seedPassword, Rol.ADMIN, "Administrador", "Sistema");
            await userManager.InsertUserAsync("marconava@usag.com.mx", seedPassword, Rol.ADMIN, "Marco", "Nava");
            await userManager.InsertUserAsync("director@usag.com", seedPassword, Rol.DIRECTOR, "Director", "General");
            await userManager.InsertUserAsync("control@usag.com", seedPassword, Rol.CONTROL_ESCOLAR, "Control", "Escolar");
        }

        private static async Task InsertUserAsync(this UserManager<ApplicationUser> userManager, string email, string password, string rol, string nombres = "", string apellidos = "")
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Nombres = nombres,
                    Apellidos = apellidos
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, rol));
                    await userManager.AddToRoleAsync(user, rol);
                }
            }
        }
    }
}
