using AuthServer.Server.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddDeviceInfoToAuthSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "AuthSessions");

            migrationBuilder.AddColumn<DeviceInformation>(
                name: "DeviceInfo",
                table: "AuthSessions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "AuthSessions",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceInfo",
                table: "AuthSessions");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "AuthSessions");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AuthSessions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
