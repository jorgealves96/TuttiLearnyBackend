using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quiz_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    path_template_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_templates_path_templates_path_template_id",
                        column: x => x.path_template_id,
                        principalTable: "path_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_feedbacks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    was_helpful = table.Column<bool>(type: "boolean", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    quiz_template_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_feedbacks", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_feedbacks_quiz_templates_quiz_template_id",
                        column: x => x.quiz_template_id,
                        principalTable: "quiz_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_quiz_feedbacks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_question_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    options = table.Column<List<string>>(type: "jsonb", nullable: false),
                    correct_answer_index = table.Column<int>(type: "integer", nullable: false),
                    quiz_template_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_question_template", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_question_template_quiz_templates_quiz_template_id",
                        column: x => x.quiz_template_id,
                        principalTable: "quiz_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_results",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    score = table.Column<int>(type: "integer", nullable: false),
                    total_questions = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    quiz_template_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_results_quiz_templates_quiz_template_id",
                        column: x => x.quiz_template_id,
                        principalTable: "quiz_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_quiz_results_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_quiz_feedbacks_quiz_template_id",
                table: "quiz_feedbacks",
                column: "quiz_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_feedbacks_user_id",
                table: "quiz_feedbacks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_question_template_quiz_template_id",
                table: "quiz_question_template",
                column: "quiz_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_results_quiz_template_id",
                table: "quiz_results",
                column: "quiz_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_results_user_id",
                table: "quiz_results",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_templates_path_template_id",
                table: "quiz_templates",
                column: "path_template_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quiz_feedbacks");

            migrationBuilder.DropTable(
                name: "quiz_question_template");

            migrationBuilder.DropTable(
                name: "quiz_results");

            migrationBuilder.DropTable(
                name: "quiz_templates");
        }
    }
}
