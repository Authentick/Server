using System;
using System.Security.Cryptography;

namespace AuthServer.Server.Services.Crypto
{
    public class SecureRandom
    {
        public string GetRandomString(int length)
        {
            int asciiLowerBound = 33;
            int asciiUpperBound = 126;
            string password = "";

            for (int i = 0; i < length; i++)
            {
                int charNumber = RandomNumberGenerator.GetInt32(asciiLowerBound, asciiUpperBound);
                password += Char.ConvertFromUtf32(charNumber);
            }

            return password;
        }
    }
}
