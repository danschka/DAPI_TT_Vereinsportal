using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TT_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamStatisticsUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyTischtennisStatisticsUrl",
                table: "Teams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MyTischtennisStatisticsUrl",
                table: "TeamRounds",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyTischtennisStatisticsUrl",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "MyTischtennisStatisticsUrl",
                table: "TeamRounds");
        }
    }
}
