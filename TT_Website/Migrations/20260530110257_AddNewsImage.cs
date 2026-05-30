using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TT_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "NewsPosts",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "NewsPosts");
        }
    }
}
