using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdCampaignTracker.Data
{
    public class AdDbContext : DbContext
    {
        public AdDbContext(DbContextOptions<AdDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<AdCampaign> AdCampaigns { get; set; }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<AdPerformance> AdPerformances { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) { base.OnModelCreating(modelBuilder); }
    }

    // --- MODELS ---
    public class User { public int Id { get; set; } [Required] public string Username { get; set; } = string.Empty; [Required] public string PasswordHash { get; set; } = string.Empty; [Required] public string Role { get; set; } = string.Empty; public ICollection<AdCampaign> Campaigns { get; set; } = new List<AdCampaign>(); }
    public class AdCampaign { public int Id { get; set; } [Required] public string Name { get; set; } = string.Empty; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public int UserId { get; set; } [JsonIgnore] public User? User { get; set; } public ICollection<Ad> Ads { get; set; } = new List<Ad>(); }
    public class AdPerformance { public int Id { get; set; } public int AdId { get; set; } [JsonIgnore] public Ad? Ad { get; set; } public int Clicks { get; set; } public int Impressions { get; set; } public decimal Cost { get; set; } }

    public class Ad
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        [Required]
        public string AdType { get; set; } = string.Empty; // "Image" or "Video"
        [Required, StringLength(100)]
        public string Headline { get; set; } = string.Empty;
        [StringLength(250)]
        public string BodyText { get; set; } = string.Empty;
        [Required]
        public string MediaUrl { get; set; } = string.Empty;
        [JsonIgnore]
        public AdCampaign? Campaign { get; set; }
        public ICollection<AdPerformance> Performances { get; set; } = new List<AdPerformance>();
    }
}