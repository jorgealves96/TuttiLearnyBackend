using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTypeFromPathItemsAndAddItToResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "path_items");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "resource",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "resource");

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "path_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
