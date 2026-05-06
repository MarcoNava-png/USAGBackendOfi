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
        public static async Task InsertInitialDataAsync(this IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var service = scope.ServiceProvider;

                try
                {
                    Console.WriteLine("=== INICIANDO DbInitializer ===");
                    var context = service.GetRequiredService<ApplicationDbContext>();
                    var masterContext = service.GetRequiredService<MasterDbContext>();
                    var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
                    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    var isDevelopment = environment == Environments.Development;
                    Console.WriteLine($"Ambiente: {environment}, IsDevelopment: {isDevelopment}");

                    try
                    {
                        Console.WriteLine("Ejecutando migraciones Master...");
                        if (!await masterContext.Database.CanConnectAsync() ||
                            (await masterContext.Database.GetAppliedMigrationsAsync()).Count() == 0)
                        {
                            await masterContext.Database.MigrateAsync();
                            Console.WriteLine("Migraciones Master completadas.");
                        }
                        else
                        {
                            Console.WriteLine("Master DB ya inicializada, omitiendo migraciones.");
                        }
                    }
                    catch (Exception exMaster)
                    {
                        Console.WriteLine($"Advertencia migraciones Master (omitido): {exMaster.Message}");
                    }

                    try
                    {
                        Console.WriteLine("Ejecutando migraciones...");
                        if (!await context.Database.CanConnectAsync() ||
                            (await context.Database.GetAppliedMigrationsAsync()).Count() == 0)
                        {
                            await context.Database.MigrateAsync();
                            Console.WriteLine("Migraciones completadas.");
                        }
                        else
                        {
                            Console.WriteLine("App DB ya inicializada, omitiendo migraciones.");
                        }
                    }
                    catch (Exception exApp)
                    {
                        Console.WriteLine($"Advertencia migraciones App (omitido): {exApp.Message}");
                    }

                    Console.WriteLine("Ejecutando RoleSeed...");
                    await RoleSeed.SeedAsync(roleManager);

                    if (isDevelopment)
                    {
                        Console.WriteLine("Ejecutando UserSeed...");
                        await UserSeed.SeedAsync(userManager);
                    }

                    Console.WriteLine("Ejecutando CatalogosSeed...");
                    await CatalogosSeed.SeedAsync(context, isDevelopment, userManager);

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
