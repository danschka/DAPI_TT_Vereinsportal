using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TT_Website.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTeamModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MyTischtennisUrl",
                table: "Teams",
                newName: "TableDataJson");

            migrationBuilder.RenameColumn(
                name: "MyTischtennisTeamId",
                table: "Teams",
                newName: "StatisticsDataJson");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Teams",
                newName: "MyTischtennisLeagueUrl");

            migrationBuilder.AlterColumn<string>(
                name: "League",
                table: "Teams",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Teams",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "TableDataJson",
                table: "Teams",
                newName: "MyTischtennisUrl");

            migrationBuilder.RenameColumn(
                name: "StatisticsDataJson",
                table: "Teams",
                newName: "MyTischtennisTeamId");

            migrationBuilder.RenameColumn(
                name: "MyTischtennisLeagueUrl",
                table: "Teams",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "League",
                table: "Teams",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
