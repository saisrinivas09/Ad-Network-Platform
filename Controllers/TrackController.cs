using AdCampaignTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AdCampaignTracker.Controllers
{
    [ApiController]
    [Route("track")]
    public class TrackController : ControllerBase
    {
        private readonly AdDbContext _context;
        public TrackController(AdDbContext context) { _context = context; }

        [HttpGet("impression/{adId}")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackImpression(int adId)
        {
            var performance = await _context.AdPerformances.FirstOrDefaultAsync(p => p.AdId == adId);
            if (performance == null)
            {
                _context.AdPerformances.Add(new AdPerformance { AdId = adId, Impressions = 1, Clicks = 0 });
            }
            else
            {
                performance.Impressions++;
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("click/{adId}")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackClick(int adId, [FromQuery] string dest)
        {
            var performance = await _context.AdPerformances.FirstOrDefaultAsync(p => p.AdId == adId);
            if (performance == null)
            {
                _context.AdPerformances.Add(new AdPerformance { AdId = adId, Impressions = 1, Clicks = 1 });
            }
            else
            {
                performance.Clicks++;
            }
            await _context.SaveChangesAsync();
            
            if (string.IsNullOrWhiteSpace(dest) || !Uri.IsWellFormedUriString(dest, UriKind.Absolute))
            {
                return BadRequest("Invalid destination URL provided.");
            }
            return Redirect(dest);
        }
    }
}