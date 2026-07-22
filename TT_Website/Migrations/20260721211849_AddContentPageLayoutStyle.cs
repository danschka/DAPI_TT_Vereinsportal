using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TT_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddContentPageLayoutStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LayoutStyle",
                table: "ContentPages",
                type: "TEXT",
                nullable: false,
                defaultValue: "cards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LayoutStyle",
                table: "ContentPages");
        }
    }
}
