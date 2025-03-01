using Microsoft.AspNetCore.Identity;

namespace UsersApp.Models.Seed
{
    public class Seed
    {
       
            public static async Task Initialize(IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
            {
                var roleNames = new[] { "Admin", "Dev", "Owner", "Guest","Customer"  };

                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        var role = new IdentityRole(roleName);
                        await roleManager.CreateAsync(role);
                    }
                }
            }
        
    }
}
