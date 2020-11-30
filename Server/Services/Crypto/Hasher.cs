using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AuthServer.Server.Services.Crypto
{
    public class Hasher
    {
        private string HashV1(string input, byte[] salt)
        {
            string saltString = Convert.ToBase64String(salt);

            string hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: input,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8)
                );

            return "1|" + hashed + "|" + saltString;
        }

        public string Hash(string input)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return HashV1(input, salt);
        }

        public bool VerifyHash(string hash, string plainText)
        {
            string[] hashComponents = hash.Split("|");

            if (hashComponents[0] == "1")
            {
                String calculatedHash = HashV1(plainText, Convert.FromBase64String(hashComponents[2]));
                return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(hash), Encoding.ASCII.GetBytes(calculatedHash));
            }

            return false;
        }
    }
}
