using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AuthServer.Shared;
using AuthServer.Server.Models;

namespace AuthServer.Server.Controllers
{
    [ApiController]
    [Route("/.well-known/openid-configuration")]
    public class OpenIdDiscoveryController : ControllerBase
    {

        private readonly ILogger<OpenIdDiscoveryController> _logger;

        public OpenIdDiscoveryController(ILogger<OpenIdDiscoveryController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public OpenIdDiscoveryModel Get()
        {
            string protocol = (HttpContext.Request.IsHttps) ? "https" : "http";
            string domain = protocol + "://" + HttpContext.Request.Host;
            return new OpenIdDiscoveryModel {
                issuer = domain,
                authorization_endpoint = domain + "/auth/oidc"
            };
        }
    }
}
