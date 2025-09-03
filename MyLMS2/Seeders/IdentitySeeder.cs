using Microsoft.AspNetCore.Identity;
using MyLMS2.Models;   
namespace MyLMS2.Seeders
{
    public class IdentitySeeder
    {
        public static async Task seedasync(IServiceProvider service)
        {
            using var scope = service.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            string[] roles = { "Admin", "Student", "Instructor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }


            var adminEmail = "Admin@123.com";   
            var adminPass = "Admin@123";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                    
                };

                var result = await userManager.CreateAsync(admin, adminPass);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
