using System.Security.Cryptography;
using System.Text;

namespace com.MirenlightStudio.NovaSandbox.MasterServer.Static
{
    public static class Argon2PasswordHasher
    {
        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        private static byte[] HashPassword(byte[] password, byte[] salt)
        {
            var argon2 = new Argon2id(password)
            {
                Salt = salt,
                DegreeOfParallelism = 1,
                MemorySize = 25565,
                Iterations = 2
            };
            return argon2.GetBytes(32);
        }

        public static bool VerifyPassword(string password, byte[] salt, byte[] storedHash)
        {
            var hash = HashPassword(Encoding.UTF8.GetBytes(password), salt);
            return CryptographicOperations.FixedTimeEquals(hash, storedHash);
        }
    }
}
