using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddLdapAppSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthApp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthApp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LdapAppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthAppId = table.Column<Guid>(type: "uuid", nullable: false),
                    BindUser = table.Column<string>(type: "text", nullable: false),
                    BaseDn = table.Column<string>(type: "text", nullable: false),
                    UseForAuthentication = table.Column<bool>(type: "boolean", nullable: false),
                    UseForIdentity = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LdapAppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LdapAppSettings_AuthApp_AuthAppId",
                        column: x => x.AuthAppId,
                        principalTable: "AuthApp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LdapAppSettings_AuthAppId",
                table: "LdapAppSettings",
                column: "AuthAppId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LdapAppSettings");

            migrationBuilder.DropTable(
                name: "AuthApp");
        }
    }
}
