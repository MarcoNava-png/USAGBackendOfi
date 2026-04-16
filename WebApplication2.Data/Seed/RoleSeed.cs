using Microsoft.AspNetCore.Identity;
using System.Reflection;
using WebApplication2.Configuration.Constants;

namespace WebApplication2.Data.Seed
{
    public static class RoleSeed
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            var fields = typeof(Rol).GetFields();

            foreach (FieldInfo field in fields)
            {
                var value = field.GetValue(null)!.ToString();

                if (value.Contains(','))
                    continue;

                if (!await roleManager.RoleExistsAsync(value))
                {
                    var role = new IdentityRole();
                    role.Name = value;
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}
