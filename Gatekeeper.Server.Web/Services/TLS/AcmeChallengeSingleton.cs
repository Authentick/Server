using System.Collections.Generic;

namespace AuthServer.Server.Services.TLS
{
    public class AcmeChallengeSingleton
    {
        public Dictionary<string, string> Challenges { get; set; }


        public AcmeChallengeSingleton()
        {
            Challenges = new Dictionary<string, string>();
        }


        public void AddChallenge(string token, string challenge)
        {
            Challenges[token] = challenge;
        }

        public string? GetChallenge(string token)
        {
            string? value;
            Challenges.TryGetValue(token, out value);

            return value;
        }
    }
}
