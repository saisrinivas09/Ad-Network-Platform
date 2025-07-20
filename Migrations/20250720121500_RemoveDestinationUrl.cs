using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdCampaignTracker.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDestinationUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationUrl",
                table: "Ads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationUrl",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
