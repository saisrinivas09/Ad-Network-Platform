/*
================================================================================
FILE: /Controllers/CampaignsController.cs
DESC: This controller has been corrected to use the modern Ad model properties
      (Headline, MediaUrl), which resolves the runtime error.
================================================================================
*/
using AdCampaignTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace AdCampaignTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Client,Admin")]
    public class CampaignsController : ControllerBase
    {
        private readonly AdDbContext _context;

        public CampaignsController(AdDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetCampaigns()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var campaigns = await _context.AdCampaigns
                .Where(c => c.UserId == userId)
                .Include(c => c.Ads)
                .Select(c => new CampaignDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Ads = c.Ads.Select(a => new AdDto
                    {
                        Id = a.Id,
                        Headline = a.Headline, // <-- CORRECTED
                        AdType = a.AdType      // <-- CORRECTED
                    }).ToList()
                })
                .ToListAsync();

            return Ok(campaigns);
        }
        
        [HttpPost]
        public async Task<ActionResult<AdCampaign>> CreateCampaign([FromBody] CampaignCreateModel model)
        {
             if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var campaign = new AdCampaign { Name = model.Name, StartDate = model.StartDate, EndDate = model.EndDate, UserId = userId };

            _context.AdCampaigns.Add(campaign);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCampaigns), new { id = campaign.Id }, campaign);
        }
    }

    // --- DTOs for Campaigns ---
    public class CampaignCreateModel { [Required] public string Name { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } }

    public class AdDto
    {
        public int Id { get; set; }
        public string Headline { get; set; } = string.Empty; // <-- CORRECTED
        public string AdType { get; set; } = string.Empty;   // <-- CORRECTED
    }

    public class CampaignDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<AdDto> Ads { get; set; } = new();
    }
}
