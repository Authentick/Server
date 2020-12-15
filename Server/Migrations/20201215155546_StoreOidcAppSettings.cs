using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace AuthServer.Server.Migrations
{
    public partial class StoreOidcAppSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OIDCAppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    ClientSecret = table.Column<string>(type: "text", nullable: false),
                    Audience = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OIDCAppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OIDCAppSettings_AuthApp_AuthAppId",
                        column: x => x.AuthAppId,
                        principalTable: "AuthApp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OIDCSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OIDCAppSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<Instant>(type: "timestamp", nullable: false),
                    ExpiredTime = table.Column<Instant>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OIDCSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OIDCSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OIDCSessions_OIDCAppSettings_OIDCAppSettingsId",
                        column: x => x.OIDCAppSettingsId,
                        principalTable: "OIDCAppSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OIDCAppSettings_AuthAppId",
                table: "OIDCAppSettings",
                column: "AuthAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OIDCSessions_OIDCAppSettingsId",
                table: "OIDCSessions",
                column: "OIDCAppSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_OIDCSessions_UserId",
                table: "OIDCSessions",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OIDCSessions");

            migrationBuilder.DropTable(
                name: "OIDCAppSettings");
        }
    }
}
