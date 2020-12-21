using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddAppSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthMethod",
                table: "AuthApp",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DirectoryMethod",
                table: "AuthApp",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProxyAppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    InternalHostname = table.Column<string>(type: "text", nullable: false),
                    PublicHostname = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProxyAppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProxyAppSettings_AuthApp_AuthAppId",
                        column: x => x.AuthAppId,
                        principalTable: "AuthApp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SCIMAppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    Hostname = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SCIMAppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SCIMAppSettings_AuthApp_AuthAppId",
                        column: x => x.AuthAppId,
                        principalTable: "AuthApp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProxyAppSettings_AuthAppId",
                table: "ProxyAppSettings",
                column: "AuthAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SCIMAppSettings_AuthAppId",
                table: "SCIMAppSettings",
                column: "AuthAppId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProxyAppSettings");

            migrationBuilder.DropTable(
                name: "SCIMAppSettings");

            migrationBuilder.DropColumn(
                name: "AuthMethod",
                table: "AuthApp");

            migrationBuilder.DropColumn(
                name: "DirectoryMethod",
                table: "AuthApp");
        }
    }
}
