using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreQuizModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_question_template_quiz_templates_quiz_template_id",
                table: "quiz_question_template");

            migrationBuilder.DropPrimaryKey(
                name: "pk_quiz_question_template",
                table: "quiz_question_template");

            migrationBuilder.RenameTable(
                name: "quiz_question_template",
                newName: "quiz_question_templates");

            migrationBuilder.RenameIndex(
                name: "ix_quiz_question_template_quiz_template_id",
                table: "quiz_question_templates",
                newName: "ix_quiz_question_templates_quiz_template_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_quiz_question_templates",
                table: "quiz_question_templates",
                column: "id");

            migrationBuilder.CreateTable(
                name: "user_quiz_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    selected_answer_index = table.Column<int>(type: "integer", nullable: false),
                    was_correct = table.Column<bool>(type: "boolean", nullable: false),
                    quiz_result_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_quiz_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_quiz_answers_quiz_results_quiz_result_id",
                        column: x => x.quiz_result_id,
                        principalTable: "quiz_results",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_quiz_answers_quiz_result_id",
                table: "user_quiz_answers",
                column: "quiz_result_id");

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_question_templates_quiz_templates_quiz_template_id",
                table: "quiz_question_templates",
                column: "quiz_template_id",
                principalTable: "quiz_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_quiz_question_templates_quiz_templates_quiz_template_id",
                table: "quiz_question_templates");

            migrationBuilder.DropTable(
                name: "user_quiz_answers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_quiz_question_templates",
                table: "quiz_question_templates");

            migrationBuilder.RenameTable(
                name: "quiz_question_templates",
                newName: "quiz_question_template");

            migrationBuilder.RenameIndex(
                name: "ix_quiz_question_templates_quiz_template_id",
                table: "quiz_question_template",
                newName: "ix_quiz_question_template_quiz_template_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_quiz_question_template",
                table: "quiz_question_template",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_question_template_quiz_templates_quiz_template_id",
                table: "quiz_question_template",
                column: "quiz_template_id",
                principalTable: "quiz_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
