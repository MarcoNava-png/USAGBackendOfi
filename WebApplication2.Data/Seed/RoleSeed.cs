using Microsoft.AspNetCore.Identity;
using System.Reflection;
using WebApplication2.Configuration.Constants;

namespace WebApplication2.Data.Seed
{
    public static class RoleSeed
    {
        public static void Seed(RoleManager<IdentityRole> roleManager)
        {

            var fields = typeof(Rol).GetFields();

            foreach (FieldInfo field in fields)
            {
                var value = field.GetValue(null)!.ToString();

                if (!roleManager.RoleExistsAsync(value).Result)
                {
                    var role = new IdentityRole();
                    role.Name = value;
                    var roleResult = roleManager.CreateAsync(role).Result;
                }
            }
        }
    }
}
