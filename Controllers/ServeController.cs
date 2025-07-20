/*
================================================================================
FILE: /Controllers/ServeController.cs
DESC: Rewritten to be much smarter. It now checks the AdType and generates
      different HTML for image ads vs. video ads.
================================================================================
*/
using AdCampaignTracker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AdCampaignTracker.Controllers
{
    [ApiController]
    [Route("serve")]
    public class ServeController : ControllerBase
    {
        private readonly AdDbContext _context;
        public ServeController(AdDbContext context) { _context = context; }

        [HttpGet("ad/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ServeAd(int id)
        {
            var ad = await _context.Ads.FindAsync(id);
            if (ad == null) return NotFound();

            string mediaElement;
            if (ad.AdType == "Video")
            {
                // Autoplay, muted, and loop are standard for video ads
                mediaElement = $@"<video autoplay muted loop playsinline src=""{ad.MediaUrl}"" style=""width: 100%; height: 100%; object-fit: cover;""></video>";
            }
            else // Default to Image
            {
                mediaElement = $@"<img src=""{ad.MediaUrl}"" alt=""{ad.Headline}"" style=""width: 100%; height: 100%; object-fit: cover;""/>";
            }

            var adHtml = $@"
                <html>
                <head>
                    <style>
                        body, html {{ margin: 0; padding: 0; font-family: sans-serif; box-sizing: border-box; }}
                        .ad-container {{
                            position: relative;
                            width: 100vw;
                            height: 100vh;
                            overflow: hidden;
                        }}
                        .ad-text-overlay {{
                            position: absolute;
                            bottom: 0;
                            left: 0;
                            width: 100%;
                            background: rgba(0, 0, 0, 0.7);
                            color: white;
                            padding: 10px;
                            box-sizing: border-box;
                        }}
                        .ad-headline {{ font-size: 1.1em; font-weight: bold; margin: 0 0 5px 0; }}
                        .ad-body {{ font-size: 0.9em; margin: 0; }}
                    </style>
                </head>
                <body>
                    <div class=""ad-container"">
                        {mediaElement}
                        <div class=""ad-text-overlay"">
                            <div class=""ad-headline"">{ad.Headline}</div>
                            <div class=""ad-body"">{ad.BodyText}</div>
                        </div>
                    </div>
                    <script>navigator.sendBeacon('/track/impression/{id}');</script>
                </body>
                </html>";

            return Content(adHtml, "text/html");
        }
    }
}
