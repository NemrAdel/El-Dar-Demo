
namespace ElDarDal.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public bool IsSynced { get; set; } = false;
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }=DateTime.UtcNow;
    }
}
