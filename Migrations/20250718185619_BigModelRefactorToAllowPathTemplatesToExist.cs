using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class BigModelRefactorToAllowPathTemplatesToExist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "path_items");

            migrationBuilder.DropTable(
                name: "learning_paths");

            migrationBuilder.CreateTable(
                name: "path_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    generated_from_prompt = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_path_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "path_item_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    path_template_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_path_item_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_path_item_templates_path_templates_path_template_id",
                        column: x => x.path_template_id,
                        principalTable: "path_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_paths",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    path_template_id = table.Column<int>(type: "integer", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_paths", x => new { x.user_id, x.path_template_id });
                    table.ForeignKey(
                        name: "fk_user_paths_path_templates_path_template_id",
                        column: x => x.path_template_id,
                        principalTable: "path_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_paths_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    path_item_template_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_templates_path_item_templates_path_item_template_id",
                        column: x => x.path_item_template_id,
                        principalTable: "path_item_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_resource_progress",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    resource_template_id = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_resource_progress", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_resource_progress_resource_templates_resource_template",
                        column: x => x.resource_template_id,
                        principalTable: "resource_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_resource_progress_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_path_item_templates_path_template_id",
                table: "path_item_templates",
                column: "path_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_templates_path_item_template_id",
                table: "resource_templates",
                column: "path_item_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_paths_path_template_id",
                table: "user_paths",
                column: "path_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_resource_progress_resource_template_id",
                table: "user_resource_progress",
                column: "resource_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_resource_progress_user_id",
                table: "user_resource_progress",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_paths");

            migrationBuilder.DropTable(
                name: "user_resource_progress");

            migrationBuilder.DropTable(
                name: "resource_templates");

            migrationBuilder.DropTable(
                name: "path_item_templates");

            migrationBuilder.DropTable(
                name: "path_templates");

            migrationBuilder.CreateTable(
                name: "learning_paths",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    generated_from_prompt = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_learning_paths", x => x.id);
                    table.ForeignKey(
                        name: "fk_learning_paths_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "path_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    learning_path_id = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_path_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_path_items_learning_paths_learning_path_id",
                        column: x => x.learning_path_id,
                        principalTable: "learning_paths",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    path_item_id = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_notes_path_items_path_item_id",
                        column: x => x.path_item_id,
                        principalTable: "path_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    path_item_id = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_resources_path_items_path_item_id",
                        column: x => x.path_item_id,
                        principalTable: "path_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_learning_paths_user_id",
                table: "learning_paths",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notes_path_item_id",
                table: "notes",
                column: "path_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_path_items_learning_path_id",
                table: "path_items",
                column: "learning_path_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_path_item_id",
                table: "resources",
                column: "path_item_id");
        }
    }
}
