using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class StoreScimStateInDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScimGroupSyncStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SCIMAppSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScimGroupSyncStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScimGroupSyncStates_SCIMAppSettings_SCIMAppSettingsId",
                        column: x => x.SCIMAppSettingsId,
                        principalTable: "SCIMAppSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScimGroupSyncStates_UserGroup_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScimUserSyncStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SCIMAppSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScimUserSyncStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScimUserSyncStates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScimUserSyncStates_SCIMAppSettings_SCIMAppSettingsId",
                        column: x => x.SCIMAppSettingsId,
                        principalTable: "SCIMAppSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScimGroupSyncStates_SCIMAppSettingsId",
                table: "ScimGroupSyncStates",
                column: "SCIMAppSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_ScimGroupSyncStates_UserGroupId",
                table: "ScimGroupSyncStates",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScimUserSyncStates_SCIMAppSettingsId",
                table: "ScimUserSyncStates",
                column: "SCIMAppSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_ScimUserSyncStates_UserId",
                table: "ScimUserSyncStates",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScimGroupSyncStates");

            migrationBuilder.DropTable(
                name: "ScimUserSyncStates");
        }
    }
}
