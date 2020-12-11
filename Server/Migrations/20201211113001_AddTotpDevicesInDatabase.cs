using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace AuthServer.Server.Migrations
{
    public partial class AddTotpDevicesInDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTotpDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SharedSecret = table.Column<string>(type: "text", nullable: false),
                    CreationTime = table.Column<Instant>(type: "timestamp", nullable: false),
                    LastUsedTime = table.Column<Instant>(type: "timestamp", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTotpDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTotpDevices_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTotpDevices_AppUserId",
                table: "UserTotpDevices",
                column: "AppUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTotpDevices");
        }
    }
}
