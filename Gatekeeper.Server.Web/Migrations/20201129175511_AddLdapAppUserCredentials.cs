using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddLdapAppUserCredentials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LdapAppUserCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LdapAppSettingsId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    HashedPassword = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LdapAppUserCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LdapAppUserCredentials_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LdapAppUserCredentials_LdapAppSettings_LdapAppSettingsId",
                        column: x => x.LdapAppSettingsId,
                        principalTable: "LdapAppSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LdapAppUserCredentials_LdapAppSettingsId",
                table: "LdapAppUserCredentials",
                column: "LdapAppSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_LdapAppUserCredentials_UserId",
                table: "LdapAppUserCredentials",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LdapAppUserCredentials");
        }
    }
}
