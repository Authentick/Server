using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddDeviceCookieToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"AuthSessions\"");

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceCookieId",
                table: "AuthSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DeviceCookies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCookies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthSessions_DeviceCookieId",
                table: "AuthSessions",
                column: "DeviceCookieId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthSessions_DeviceCookies_DeviceCookieId",
                table: "AuthSessions",
                column: "DeviceCookieId",
                principalTable: "DeviceCookies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthSessions_DeviceCookies_DeviceCookieId",
                table: "AuthSessions");

            migrationBuilder.DropTable(
                name: "DeviceCookies");

            migrationBuilder.DropIndex(
                name: "IX_AuthSessions_DeviceCookieId",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "DeviceCookieId",
                table: "AuthSessions");
        }
    }
}
