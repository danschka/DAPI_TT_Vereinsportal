using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TT_Website.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeamsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyTischtennisTeamId",
                table: "Teams",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Season",
                table: "Teams",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyTischtennisTeamId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Season",
                table: "Teams");
        }
    }
}
