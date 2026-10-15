
using Microsoft.AspNetCore.Identity;

namespace ElDarBLL.Helpers
{
    public class PasswordHelper
    {
        private static readonly PasswordHasher<string> _hasher = new PasswordHasher<string>();

        public static string HashPassword(string password)
        {
            return _hasher.HashPassword("user", password);
        }

        public static bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            var result = _hasher.VerifyHashedPassword("user", hashedPassword, inputPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
