using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddAlertsToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION \"hstore\"");
            migrationBuilder.CreateTable(
                name: "SystemSecurityAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertType = table.Column<int>(type: "integer", nullable: false),
                    KeyValueStore = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSecurityAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSecurityAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: true),
                    AlertType = table.Column<int>(type: "integer", nullable: false),
                    KeyValueStore = table.Column<Dictionary<string, string>>(type: "hstore", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSecurityAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSecurityAlerts_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSecurityAlerts_RecipientId",
                table: "UserSecurityAlerts",
                column: "RecipientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSecurityAlerts");

            migrationBuilder.DropTable(
                name: "UserSecurityAlerts");
            migrationBuilder.Sql("DROP EXTENSION \"hstore\"");
        }
    }
}
