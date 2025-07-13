using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableAndColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningPaths_Users_UserId",
                table: "LearningPaths");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_PathItems_PathItemId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_PathItems_LearningPaths_LearningPathId",
                table: "PathItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notes",
                table: "Notes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PathItems",
                table: "PathItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LearningPaths",
                table: "LearningPaths");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Notes",
                newName: "notes");

            migrationBuilder.RenameTable(
                name: "PathItems",
                newName: "path_items");

            migrationBuilder.RenameTable(
                name: "LearningPaths",
                newName: "learning_paths");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "users",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Auth0Id",
                table: "users",
                newName: "auth0id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "notes",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "notes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PathItemId",
                table: "notes",
                newName: "path_item_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "notes",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Notes_PathItemId",
                table: "notes",
                newName: "ix_notes_path_item_id");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "path_items",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "path_items",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "path_items",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "path_items",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "path_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LearningPathId",
                table: "path_items",
                newName: "learning_path_id");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "path_items",
                newName: "is_completed");

            migrationBuilder.RenameIndex(
                name: "IX_PathItems_LearningPathId",
                table: "path_items",
                newName: "ix_path_items_learning_path_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "learning_paths",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "learning_paths",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "learning_paths",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "learning_paths",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "GeneratedFromPrompt",
                table: "learning_paths",
                newName: "generated_from_prompt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "learning_paths",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_LearningPaths_UserId",
                table: "learning_paths",
                newName: "ix_learning_paths_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notes",
                table: "notes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_path_items",
                table: "path_items",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_learning_paths",
                table: "learning_paths",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_learning_paths_users_user_id",
                table: "learning_paths",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_notes_path_items_path_item_id",
                table: "notes",
                column: "path_item_id",
                principalTable: "path_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_path_items_learning_paths_learning_path_id",
                table: "path_items",
                column: "learning_path_id",
                principalTable: "learning_paths",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_learning_paths_users_user_id",
                table: "learning_paths");

            migrationBuilder.DropForeignKey(
                name: "fk_notes_path_items_path_item_id",
                table: "notes");

            migrationBuilder.DropForeignKey(
                name: "fk_path_items_learning_paths_learning_path_id",
                table: "path_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notes",
                table: "notes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_path_items",
                table: "path_items");

            migrationBuilder.DropPrimaryKey(
                name: "pk_learning_paths",
                table: "learning_paths");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "notes",
                newName: "Notes");

            migrationBuilder.RenameTable(
                name: "path_items",
                newName: "PathItems");

            migrationBuilder.RenameTable(
                name: "learning_paths",
                newName: "LearningPaths");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "auth0id",
                table: "Users",
                newName: "Auth0Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Notes",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Notes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "path_item_id",
                table: "Notes",
                newName: "PathItemId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Notes",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_notes_path_item_id",
                table: "Notes",
                newName: "IX_Notes_PathItemId");

            migrationBuilder.RenameColumn(
                name: "url",
                table: "PathItems",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "PathItems",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "PathItems",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "PathItems",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PathItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "learning_path_id",
                table: "PathItems",
                newName: "LearningPathId");

            migrationBuilder.RenameColumn(
                name: "is_completed",
                table: "PathItems",
                newName: "IsCompleted");

            migrationBuilder.RenameIndex(
                name: "ix_path_items_learning_path_id",
                table: "PathItems",
                newName: "IX_PathItems_LearningPathId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "LearningPaths",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "LearningPaths",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "LearningPaths",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "LearningPaths",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "generated_from_prompt",
                table: "LearningPaths",
                newName: "GeneratedFromPrompt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "LearningPaths",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_learning_paths_user_id",
                table: "LearningPaths",
                newName: "IX_LearningPaths_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notes",
                table: "Notes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PathItems",
                table: "PathItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LearningPaths",
                table: "LearningPaths",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPaths_Users_UserId",
                table: "LearningPaths",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_PathItems_PathItemId",
                table: "Notes",
                column: "PathItemId",
                principalTable: "PathItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PathItems_LearningPaths_LearningPathId",
                table: "PathItems",
                column: "LearningPathId",
                principalTable: "LearningPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
