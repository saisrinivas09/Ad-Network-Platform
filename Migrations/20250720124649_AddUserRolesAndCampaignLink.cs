﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdCampaignTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRolesAndCampaignLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "AdCampaigns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdCampaigns_UserId",
                table: "AdCampaigns",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdCampaigns_Users_UserId",
                table: "AdCampaigns",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdCampaigns_Users_UserId",
                table: "AdCampaigns");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_AdCampaigns_UserId",
                table: "AdCampaigns");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AdCampaigns");
        }
    }
}
