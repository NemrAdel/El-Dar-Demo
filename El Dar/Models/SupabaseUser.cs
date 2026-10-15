using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace El_Dar.Models
{
    [Table("Users")]
    public class SupabaseUser:BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("UserName")]
        public string UserName { get; set; } = null!;
        [Column("Email")]
        public string Email { get; set; } = null!;
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = null!;
        [Column("ResetPasswordToken")]
        public string? ResetPasswordToken { get; set; }
        [Column("ResetTokenExpires")]
        public DateTime? ResetTokenExpires { get; set; }
        [Column("IsSynced")]
        public bool IsSynced { get; set; } = false;
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }

    }
}
