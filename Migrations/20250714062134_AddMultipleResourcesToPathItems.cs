using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleResourcesToPathItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "url",
                table: "path_items");

            migrationBuilder.CreateTable(
                name: "resource",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    path_item_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_path_items_path_item_id",
                        column: x => x.path_item_id,
                        principalTable: "path_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_resource_path_item_id",
                table: "resource",
                column: "path_item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resource");

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "path_items",
                type: "text",
                nullable: true);
        }
    }
}
