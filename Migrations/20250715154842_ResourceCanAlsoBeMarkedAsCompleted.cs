using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class ResourceCanAlsoBeMarkedAsCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_resource_path_items_path_item_id",
                table: "resource");

            migrationBuilder.DropPrimaryKey(
                name: "pk_resource",
                table: "resource");

            migrationBuilder.RenameTable(
                name: "resource",
                newName: "resources");

            migrationBuilder.RenameIndex(
                name: "ix_resource_path_item_id",
                table: "resources",
                newName: "ix_resources_path_item_id");

            migrationBuilder.AddColumn<bool>(
                name: "is_completed",
                table: "resources",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "pk_resources",
                table: "resources",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_resources_path_items_path_item_id",
                table: "resources",
                column: "path_item_id",
                principalTable: "path_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_resources_path_items_path_item_id",
                table: "resources");

            migrationBuilder.DropPrimaryKey(
                name: "pk_resources",
                table: "resources");

            migrationBuilder.DropColumn(
                name: "is_completed",
                table: "resources");

            migrationBuilder.RenameTable(
                name: "resources",
                newName: "resource");

            migrationBuilder.RenameIndex(
                name: "ix_resources_path_item_id",
                table: "resource",
                newName: "ix_resource_path_item_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_resource",
                table: "resource",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_resource_path_items_path_item_id",
                table: "resource",
                column: "path_item_id",
                principalTable: "path_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
