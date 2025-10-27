using System.Security.Cryptography;
using UserManagementService.Services.Interfaces;

namespace UserManagementService.Services.Implementations
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; 
        private const int HashSize = 32; 
        private const int Iterations = 100000; 

        public (string hash, string salt) HashPassword(string password)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: saltBytes,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA512,
                outputLength: HashSize
            );

            return (
                hash: Convert.ToBase64String(hashBytes),
                salt: Convert.ToBase64String(saltBytes)
            );
        }

        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            // Hash the input password with the stored salt
            byte[] inputHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: saltBytes,
                iterations: Iterations,
                hashAlgorithm: HashAlgorithmName.SHA512,
                outputLength: HashSize
            );

            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            // Compare hashes in constant time (prevents timing attacks)
            return CryptographicOperations.FixedTimeEquals(storedHashBytes, inputHashBytes);
        }
    }
}