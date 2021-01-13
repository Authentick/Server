using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthServer.Server.Migrations
{
    public partial class MakeValuesString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "SystemSettings",
                type: "text",
                nullable: false,
                oldClrType: typeof(string[]),
                oldType: "text[]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string[]>(
                name: "Value",
                table: "SystemSettings",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
