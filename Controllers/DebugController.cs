using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace AdCampaignTracker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DebugController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This endpoint is public so we can test it easily
        [HttpGet("config")]
        [AllowAnonymous]
        public IActionResult GetConfigValues()
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                return BadRequest(new {
                    Message = "ERROR: The server could not find the JWT settings in appsettings.json.",
                    Note = "Please ensure your appsettings.json file has a 'Jwt' section with 'Key', 'Issuer', and 'Audience'."
                });
            }

            // Let's add a partial view of the key to be 100% sure.
            var partialKey = jwtKey.Length > 5 ? jwtKey.Substring(0, 5) : jwtKey;

            return Ok(new
            {
                JwtKey_Is_Set = !string.IsNullOrEmpty(jwtKey),
                JwtKey_StartsWith = partialKey, // <-- ADDED THIS LINE
                JwtIssuer_From_Config = jwtIssuer,
                JwtAudience_From_Config = jwtAudience
            });
        }
    }
}
