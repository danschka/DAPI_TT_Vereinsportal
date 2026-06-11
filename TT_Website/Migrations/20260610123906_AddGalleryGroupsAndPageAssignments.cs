using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TT_Website.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryGroupsAndPageAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GalleryGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GalleryGroupImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GalleryGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    GalleryImageId = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryGroupImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryGroupImages_GalleryGroups_GalleryGroupId",
                        column: x => x.GalleryGroupId,
                        principalTable: "GalleryGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryGroupImages_GalleryImages_GalleryImageId",
                        column: x => x.GalleryImageId,
                        principalTable: "GalleryImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PageGalleryAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageKey = table.Column<string>(type: "TEXT", nullable: false),
                    PageTitle = table.Column<string>(type: "TEXT", nullable: false),
                    SlideshowEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IntervalSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    GalleryGroupId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageGalleryAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageGalleryAssignments_GalleryGroups_GalleryGroupId",
                        column: x => x.GalleryGroupId,
                        principalTable: "GalleryGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryGroupImages_GalleryGroupId_GalleryImageId",
                table: "GalleryGroupImages",
                columns: new[] { "GalleryGroupId", "GalleryImageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GalleryGroupImages_GalleryImageId",
                table: "GalleryGroupImages",
                column: "GalleryImageId");

            migrationBuilder.CreateIndex(
                name: "IX_PageGalleryAssignments_GalleryGroupId",
                table: "PageGalleryAssignments",
                column: "GalleryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PageGalleryAssignments_PageKey",
                table: "PageGalleryAssignments",
                column: "PageKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryGroupImages");

            migrationBuilder.DropTable(
                name: "PageGalleryAssignments");

            migrationBuilder.DropTable(
                name: "GalleryGroups");
        }
    }
}
