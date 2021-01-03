using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddHostingType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AuthApp",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HostingType",
                table: "AuthApp",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.Sql("UPDATE \"AuthApp\" SET \"HostingType\"=4 WHERE \"AuthMethod\"=1");
            migrationBuilder.Sql("UPDATE \"AuthApp\" SET \"HostingType\"=1 WHERE \"AuthMethod\"=2");
            migrationBuilder.Sql("UPDATE \"AuthApp\" SET \"HostingType\"=3 WHERE \"AuthMethod\"=3");
            migrationBuilder.Sql("UPDATE \"AuthApp\" SET \"AuthMethod\"=0 WHERE \"AuthMethod\"=3");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "AuthApp",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AuthApp");

            migrationBuilder.DropColumn(
                name: "HostingType",
                table: "AuthApp");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "AuthApp");
        }
    }
}
