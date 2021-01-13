using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class ConfigureLdapCredentialsRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LdapAppUserCredentials_LdapAppSettings_LdapAppSettingsId",
                table: "LdapAppUserCredentials");

            migrationBuilder.AlterColumn<Guid>(
                name: "LdapAppSettingsId",
                table: "LdapAppUserCredentials",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LdapAppUserCredentials_LdapAppSettings_LdapAppSettingsId",
                table: "LdapAppUserCredentials",
                column: "LdapAppSettingsId",
                principalTable: "LdapAppSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LdapAppUserCredentials_LdapAppSettings_LdapAppSettingsId",
                table: "LdapAppUserCredentials");

            migrationBuilder.AlterColumn<Guid>(
                name: "LdapAppSettingsId",
                table: "LdapAppUserCredentials",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_LdapAppUserCredentials_LdapAppSettings_LdapAppSettingsId",
                table: "LdapAppUserCredentials",
                column: "LdapAppSettingsId",
                principalTable: "LdapAppSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
