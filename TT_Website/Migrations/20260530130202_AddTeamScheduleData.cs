using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TT_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamScheduleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScheduleDataJson",
                table: "Teams",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleDataJson",
                table: "Teams");
        }
    }
}
