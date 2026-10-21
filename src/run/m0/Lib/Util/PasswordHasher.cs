using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace m0.Lib.Util
{    
    public static class PasswordHasher
    {
        private const int SaltSize = 16;          // 128 bit
        private const int KeySize = 32;           // 256 bit
        private const int Iterations = 600_000;   // OWASP for PBKDF2-HMAC-SHA256
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

            return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        public static bool Verify(string passwordToCheck, string passwordStored)
        {
            var parts = passwordStored.Split('$');
            if (parts.Length != 4 || parts[0] != "pbkdf2-sha256")
                return false;

            int iterations = int.Parse(parts[1]);
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);

            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(passwordToCheck, salt, iterations, Algorithm, expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}
