using System.Security.Cryptography;
using System.Text;

namespace RentalHub.Helper
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
