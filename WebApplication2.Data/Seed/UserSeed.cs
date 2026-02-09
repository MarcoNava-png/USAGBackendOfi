using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Models;

namespace WebApplication2.Data.Seed
{
    public static class UserSeed
    {
        public static void Seed(UserManager<ApplicationUser> userManager)
        {
            userManager.InsertUser("admin@usag.com", "Admin123", Rol.ADMIN, "Administrador", "Sistema");
            userManager.InsertUser("marconava@usag.com.mx", "Admin123", Rol.ADMIN, "Marco", "Nava");
            userManager.InsertUser("director@usag.com", "Director123", Rol.DIRECTOR, "Director", "General");
            userManager.InsertUser("control@usag.com", "Control123", Rol.CONTROL_ESCOLAR, "Control", "Escolar");
        }

        private static void InsertUser(this UserManager<ApplicationUser> userManager, string email, string password, string rol, string nombres = "", string apellidos = "")
        {
            if (userManager.FindByEmailAsync(email).Result == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Nombres = nombres,
                    Apellidos = apellidos
                };

                var result = userManager.CreateAsync(user, password).Result;

                if (result.Succeeded)
                {
                    userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, rol)).Wait();
                    userManager.AddToRoleAsync(user, rol).Wait();
                }
            }
        }
    }
}
