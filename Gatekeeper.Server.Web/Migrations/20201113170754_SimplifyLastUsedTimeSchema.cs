using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace AuthServer.Server.Migrations
{
    public partial class SimplifyLastUsedTimeSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthSessionUsages");

            migrationBuilder.AddColumn<Instant>(
                name: "LastUsedTime",
                table: "AuthSessions",
                type: "timestamp",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUsedTime",
                table: "AuthSessions");

            migrationBuilder.CreateTable(
                name: "AuthSessionUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<IPAddress>(type: "inet", nullable: false),
                    LastActive = table.Column<Instant>(type: "timestamp", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthSessionUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthSessionUsages_AuthSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AuthSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthSessionUsages_SessionId",
                table: "AuthSessionUsages",
                column: "SessionId");
        }
    }
}
