/*
================================================================================
FILE: /Controllers/HostController.cs
DESC: This controller is now synchronized with the latest data models.
================================================================================
*/
using AdCampaignTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdCampaignTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Host,Admin")]
    public class HostController : ControllerBase
    {
        private readonly AdDbContext _context;

        public HostController(AdDbContext context)
        {
            _context = context;
        }

        [HttpGet("ads")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllAdsForHost()
        {
            var ads = await _context.Ads
                .Select(a => new 
                {
                    Id = a.Id,
                    Headline = a.Headline,
                    BodyText = a.BodyText,
                    AdType = a.AdType
                })
                .ToListAsync();

            return Ok(ads);
        }
    }
}
