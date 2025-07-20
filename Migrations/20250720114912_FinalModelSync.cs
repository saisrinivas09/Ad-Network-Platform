using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdCampaignTracker.Migrations
{
    /// <inheritdoc />
    public partial class FinalModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdPerformances_AdCampaigns_CampaignId",
                table: "AdPerformances");

            migrationBuilder.RenameColumn(
                name: "CampaignId",
                table: "AdPerformances",
                newName: "AdId");

            migrationBuilder.RenameIndex(
                name: "IX_AdPerformances_CampaignId",
                table: "AdPerformances",
                newName: "IX_AdPerformances_AdId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AdCampaigns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Ads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ads_AdCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "AdCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ads_CampaignId",
                table: "Ads",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdPerformances_Ads_AdId",
                table: "AdPerformances",
                column: "AdId",
                principalTable: "Ads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdPerformances_Ads_AdId",
                table: "AdPerformances");

            migrationBuilder.DropTable(
                name: "Ads");

            migrationBuilder.RenameColumn(
                name: "AdId",
                table: "AdPerformances",
                newName: "CampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_AdPerformances_AdId",
                table: "AdPerformances",
                newName: "IX_AdPerformances_CampaignId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AdCampaigns",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_AdPerformances_AdCampaigns_CampaignId",
                table: "AdPerformances",
                column: "CampaignId",
                principalTable: "AdCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
