using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class AddUserGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserUserGroup",
                columns: table => new
                {
                    GroupsId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserUserGroup", x => new { x.GroupsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_AppUserUserGroup_AspNetUsers_MembersId",
                        column: x => x.MembersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserUserGroup_UserGroup_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "UserGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthAppUserGroup",
                columns: table => new
                {
                    AuthAppsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGroupsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthAppUserGroup", x => new { x.AuthAppsId, x.UserGroupsId });
                    table.ForeignKey(
                        name: "FK_AuthAppUserGroup_AuthApp_AuthAppsId",
                        column: x => x.AuthAppsId,
                        principalTable: "AuthApp",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthAppUserGroup_UserGroup_UserGroupsId",
                        column: x => x.UserGroupsId,
                        principalTable: "UserGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserUserGroup_MembersId",
                table: "AppUserUserGroup",
                column: "MembersId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthAppUserGroup_UserGroupsId",
                table: "AuthAppUserGroup",
                column: "UserGroupsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserUserGroup");

            migrationBuilder.DropTable(
                name: "AuthAppUserGroup");

            migrationBuilder.DropTable(
                name: "UserGroup");
        }
    }
}
