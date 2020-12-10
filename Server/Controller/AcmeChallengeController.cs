using AuthServer.Server.Services.TLS;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Server.Controller
{

    [Route(".well-known/acme-challenge")]
    public class AcmeChallengeController : ControllerBase
    {
        private readonly AcmeChallengeSingleton _acmeChallengeSingleton;

        public AcmeChallengeController(AcmeChallengeSingleton acmeChallengeSingleton)
        {
            _acmeChallengeSingleton = acmeChallengeSingleton;
        }

        [HttpGet("{token}")]
        public IActionResult ShowChallenge(string token)
        {
            string? challenge = _acmeChallengeSingleton.GetChallenge(token);

            if (challenge == null)
            {
                return NotFound("Token not found");
            }

            return Ok(challenge);
        }
    }

}