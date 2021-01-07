using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Server.Services.Authentication.PasswordPolicy
{
    public class HIBP : IPasswordValidator<AppUser> 
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HIBP(
            IHttpClientFactory httpClientFactory
        ) {
            _httpClientFactory = httpClientFactory;
        }

        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            // Hash the password with SHA1
            SHA1 sha1 = SHA1.Create();
            byte[] byteString = Encoding.UTF8.GetBytes(password);
            byte[] hashedBytes = sha1.ComputeHash(byteString);

            StringBuilder sb = new StringBuilder();
            foreach(byte b in hashedBytes) {
                sb.Append(b.ToString("X2"));
            }
            string hashString = sb.ToString();

            // Split the string
            string start = hashString.Substring(0, 5);
            string end = hashString.Substring(5);

            // Do a request to the pwnedpasswords
            HttpClient client = _httpClientFactory.CreateClient();
            string url = "https://api.pwnedpasswords.com/range/" + start;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("User-Agent", "gatekeeper");
            req.Headers.Add("Add-Padding", "true");

            //TODO add gzip/brotli support for the client
            //TODO add timeout?
            HttpResponseMessage response = client.Send(req);

            // This API should always succeed but in case it does not something bad is going on there
            if (!response.IsSuccessStatusCode) {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "HIBPMatch",
                    Description = "Failed to fetch"
                }));
            }
            
            // Obtain the data from the stream
            StreamReader reader = new StreamReader(response.Content.ReadAsStream());
            string data = reader.ReadToEnd();

            // Since we added the padding lets remove all the data that matches on 0
            data = Regex.Replace(data, @"[0-9A-Z]{35}:0", "");

            // No finally do the actual check (uppercase) if our string is in there fail
            if (data.Contains(end.ToUpper())) {
                return Task.FromResult(IdentityResult.Failed(new IdentityError
                {
                    Code = "HIBPMatch",
                    Description = "Password appears in breached password lists"
                }));
            }

            // All good!
            return Task.FromResult(IdentityResult.Success);
        }
    }
}