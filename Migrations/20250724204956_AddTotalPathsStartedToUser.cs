using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalPathsStartedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "total_paths_started",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "total_paths_started",
                table: "users");
        }
    }
}
