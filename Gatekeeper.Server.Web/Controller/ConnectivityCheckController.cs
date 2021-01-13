using System;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Server.Controller
{

    [Route("api/connectivity_check")]
    public class ConnectivityCheckController : ControllerBase
    {
        [HttpGet]
        public ChallengeResponse ShowChallenge([FromQuery] Guid challenge)
        {
            return new ChallengeResponse
            {
                Challenge = challenge,
            };
        }

        public class ChallengeResponse
        {
            public Guid Challenge { get; set; }
        }
    }
}
