using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddSystemSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
