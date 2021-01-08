using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddAuthSessionIpsForGeolocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthSessionIps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<IPAddress>(type: "inet", nullable: false),
                    AuthSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    City = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthSessionIps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthSessionIps_AuthSessions_AuthSessionId",
                        column: x => x.AuthSessionId,
                        principalTable: "AuthSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthSessionIps_AuthSessionId",
                table: "AuthSessionIps",
                column: "AuthSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthSessionIps_IpAddress",
                table: "AuthSessionIps",
                column: "IpAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthSessionIps");
        }
    }
}
