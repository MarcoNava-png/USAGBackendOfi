using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Data.Seed
{
    public static class DbInitializer
    {
        public static void InsertInitialData(this IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var service = scope.ServiceProvider;

                try
                {
                    Console.WriteLine("=== INICIANDO DbInitializer ===");
                    var context = service.GetRequiredService<ApplicationDbContext>();
                    var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    var isDevelopment = environment == Environments.Development;
                    Console.WriteLine($"Ambiente: {environment}, IsDevelopment: {isDevelopment}");

                    Console.WriteLine("Ejecutando migraciones...");
                    context.Database.Migrate();
                    Console.WriteLine("Migraciones completadas.");

                    Console.WriteLine("Ejecutando RoleSeed...");
                    RoleSeed.Seed(roleManager);

                    if (isDevelopment)
                    {
                        Console.WriteLine("Ejecutando UserSeed...");
                        UserSeed.Seed(userManager);
                    }

                    Console.WriteLine("Ejecutando CatalogosSeed...");
                    CatalogosSeed.Seed(context, isDevelopment, userManager);

                    Console.WriteLine("Ejecutando PermissionSeed...");
                    PermissionSeed.Seed(context, roleManager);

                    Console.WriteLine("Ejecutando TipoDocumentoSeed...");
                    TipoDocumentoSeed.Seed(context);

                    Console.WriteLine("=== DbInitializer COMPLETADO ===");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en DbInitializer: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
    }
}
