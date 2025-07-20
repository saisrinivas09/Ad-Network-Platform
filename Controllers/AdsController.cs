/*
================================================================================
FILE: /Controllers/AdsController.cs
DESC: Updated to include a new [HttpDelete] endpoint that allows
      users with the "Admin" role to delete any ad from the system.
================================================================================
*/
using AdCampaignTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AdCampaignTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // General authorization for the controller
    public class AdsController : ControllerBase
    {
        private readonly AdDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdsController(AdDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost]
        [Authorize(Roles = "Client,Admin")] // Clients and Admins can create ads
        public async Task<ActionResult<Ad>> CreateAd([FromForm] AdCreateModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();
            
            // Security check: Ensure the campaign belongs to the user if they are a Client
            var campaignQuery = _context.AdCampaigns.AsQueryable();
            if (User.IsInRole("Client"))
            {
                campaignQuery = campaignQuery.Where(c => c.UserId == userId);
            }

            var campaign = await campaignQuery.FirstOrDefaultAsync(c => c.Id == model.CampaignId);
            if (campaign == null) return BadRequest("Invalid Campaign ID or you do not have permission for this campaign.");

            if (model.MediaFile == null || model.MediaFile.Length == 0) return BadRequest("No media file uploaded.");

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.MediaFile.FileName)}";
            var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads", "ads");
            if (!Directory.Exists(uploadsFolderPath)) Directory.CreateDirectory(uploadsFolderPath);

            var filePath = Path.Combine(uploadsFolderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.MediaFile.CopyToAsync(stream);
            }

            var mediaUrl = $"{Request.Scheme}://{Request.Host}/uploads/ads/{fileName}";

            var ad = new Ad
            {
                CampaignId = model.CampaignId,
                AdType = model.AdType,
                Headline = model.Headline,
                BodyText = model.BodyText,
                MediaUrl = mediaUrl
            };

            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();

            return Ok(ad);
        }

        // --- NEW DELETE ENDPOINT ---
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // IMPORTANT: Only Admins can delete ads
        public async Task<IActionResult> DeleteAd(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null)
            {
                return NotFound();
            }

            // Optional: Delete the associated media file from wwwroot/uploads/ads
            // This prevents orphaned files on your server.
            try
            {
                var fileName = Path.GetFileName(ad.MediaUrl);
                var filePath = Path.Combine(_env.WebRootPath, "uploads", "ads", fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't block the database deletion
                Console.WriteLine($"Error deleting ad media file: {ex.Message}");
            }

            _context.Ads.Remove(ad);
            await _context.SaveChangesAsync();

            return NoContent(); // Standard successful response for a DELETE request
        }
    }

    // DTO for creating a new Ad (remains the same)
    public class AdCreateModel { [Required] public int CampaignId { get; set; } [Required] public string AdType { get; set; } = string.Empty; [Required] public string Headline { get; set; } = string.Empty; public string BodyText { get; set; } = string.Empty; [Required] public IFormFile MediaFile { get; set; } = null!; }
}
