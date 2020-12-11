using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddUserToTotp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTotpDevices_AspNetUsers_AppUserId",
                table: "UserTotpDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserTotpDevices_AppUserId",
                table: "UserTotpDevices");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "UserTotpDevices");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "UserTotpDevices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserTotpDevices_UserId",
                table: "UserTotpDevices",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTotpDevices_AspNetUsers_UserId",
                table: "UserTotpDevices",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTotpDevices_AspNetUsers_UserId",
                table: "UserTotpDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserTotpDevices_UserId",
                table: "UserTotpDevices");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserTotpDevices");

            migrationBuilder.AddColumn<Guid>(
                name: "AppUserId",
                table: "UserTotpDevices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTotpDevices_AppUserId",
                table: "UserTotpDevices",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTotpDevices_AspNetUsers_AppUserId",
                table: "UserTotpDevices",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
