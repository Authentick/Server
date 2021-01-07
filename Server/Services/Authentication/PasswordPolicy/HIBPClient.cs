using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Gatekeeper.Server.Services.Authentication.PasswordPolicy
{
    public class HIBPClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HIBPClient(
            IHttpClientFactory httpClientFactory
        )
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsBreachedAsync(string password)
        {
            CancellationToken token = new CancellationToken();
            return await IsBreachedAsync(password, token);
        }

        public async Task<bool> IsBreachedAsync(string password, CancellationToken cancellationToken)
        {
            // Hash the password with SHA1
            SHA1 sha1 = SHA1.Create();
            byte[] byteString = Encoding.UTF8.GetBytes(password);
            byte[] hashedBytes = sha1.ComputeHash(byteString);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashedBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            string hashString = sb.ToString();

            // Split the string
            string start = hashString.Substring(0, 5);
            string end = hashString.Substring(5);

            // Do a request to the pwnedpasswords
            HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = System.TimeSpan.FromSeconds(3);
            Uri url = new Uri("https://api.pwnedpasswords.com/range/" + start);
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("User-Agent", "gatekeeper");
            req.Headers.Add("Add-Padding", "true");

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(req, cancellationToken);
            }
            catch
            {
                return false;
            }

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            // Obtain the data from the stream
            StreamReader reader = new StreamReader(response.Content.ReadAsStream());
            string data = await reader.ReadToEndAsync();

            // Since we added the padding lets remove all the data that matches on 0
            data = Regex.Replace(data, @"[0-9A-Z]{35}:0", "");

            // No finally do the actual check (uppercase) if our string is in there fail
            if (data.Contains(end.ToUpper()))
            {
                return true;
            }

            // All good!
            return false;
        }
    }
}
