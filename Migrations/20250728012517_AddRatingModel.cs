using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "path_template_ratings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    path_template_id = table.Column<int>(type: "integer", nullable: false),
                    firebase_uid = table.Column<string>(type: "text", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    rated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_path_template_ratings", x => x.id);
                    table.ForeignKey(
                        name: "fk_path_template_ratings_path_templates_path_template_id",
                        column: x => x.path_template_id,
                        principalTable: "path_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_path_template_ratings_path_template_id",
                table: "path_template_ratings",
                column: "path_template_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "path_template_ratings");
        }
    }
}
