using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthServer.Shared;

namespace AuthServer.Server.Controllers
{
    [ApiController]
    [Route("/auth/oidc")]
    public class OpenIdConnectController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<OpenIdConnectController> _logger;

        public OpenIdConnectController(ILogger<OpenIdConnectController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get(string clientId, string redirectUri, string scope, string responseCode, string state)
        {
            return clientId;
        }
    }
}
