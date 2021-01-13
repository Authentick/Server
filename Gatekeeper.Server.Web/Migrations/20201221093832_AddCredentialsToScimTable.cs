using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddCredentialsToScimTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Hostname",
                table: "SCIMAppSettings",
                newName: "Endpoint");

            migrationBuilder.AddColumn<string>(
                name: "Credentials",
                table: "SCIMAppSettings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Credentials",
                table: "SCIMAppSettings");

            migrationBuilder.RenameColumn(
                name: "Endpoint",
                table: "SCIMAppSettings",
                newName: "Hostname");
        }
    }
}
