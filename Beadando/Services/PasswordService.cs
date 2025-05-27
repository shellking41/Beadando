using System;
using System.Security.Cryptography;
using System.Text;

namespace Beadando.Services
{
    public class PasswordService
    {
        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public string HashPassword(string password)
        {
            var salt = GenerateSalt();
            return HashPassword(password, salt);
        }

        public string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                string saltedPassword = password + salt;
                byte[] bytes = Encoding.UTF8.GetBytes(saltedPassword);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes) + ":" + salt;
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            var hash = parts[0];
            var salt = parts[1];

            var newHashedPassword = HashPassword(password, salt);
            return hashedPassword == newHashedPassword;
        }
    }
}
