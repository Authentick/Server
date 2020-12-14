using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace AuthServer.Server.Migrations
{
    public partial class AddInvalidLoginAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvalidLoginAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    AttemptTime = table.Column<Instant>(type: "timestamp", nullable: false),
                    IpAddress = table.Column<IPAddress>(type: "inet", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvalidLoginAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvalidLoginAttempts_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvalidTwoFactorAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptTime = table.Column<Instant>(type: "timestamp", nullable: false),
                    IPAddress = table.Column<IPAddress>(type: "inet", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvalidTwoFactorAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvalidTwoFactorAttempts_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvalidLoginAttempts_TargetUserId",
                table: "InvalidLoginAttempts",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvalidTwoFactorAttempts_TargetUserId",
                table: "InvalidTwoFactorAttempts",
                column: "TargetUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvalidLoginAttempts");

            migrationBuilder.DropTable(
                name: "InvalidTwoFactorAttempts");
        }
    }
}
