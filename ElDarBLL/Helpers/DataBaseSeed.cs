using ElDarDal.Context;
using ElDarDal.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ElDarBLL.Helpers
{
    public static class DataBaseSeed
    {
        private static readonly Guid DefaultAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public static async Task SeedDataAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<ElDarDbContext>();

            var existingUser = dbContext.User.FirstOrDefault(u => u.Email == "Nabil2388@gmail.com");

            if (existingUser == null)
            {
                dbContext.User.Add(new User
                {
                    Id = DefaultAdminId,
                    UserName = "Test",
                    Email = "Test@gmail.com",
                    PasswordHash = PasswordHelper.HashPassword("123456789"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsSynced = false
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}

