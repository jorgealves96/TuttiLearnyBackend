using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class EditPathItemTemplateForNewForkingArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_path_id",
                table: "path_item_templates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_path_item_templates_user_path_id",
                table: "path_item_templates",
                column: "user_path_id");

            migrationBuilder.AddForeignKey(
                name: "fk_path_item_templates_user_paths_user_path_id",
                table: "path_item_templates",
                column: "user_path_id",
                principalTable: "user_paths",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_path_item_templates_user_paths_user_path_id",
                table: "path_item_templates");

            migrationBuilder.DropIndex(
                name: "ix_path_item_templates_user_path_id",
                table: "path_item_templates");

            migrationBuilder.DropColumn(
                name: "user_path_id",
                table: "path_item_templates");
        }
    }
}
