using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningAppNetCoreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionAndUsageTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_usage_reset_date",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "paths_extended_this_month",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "paths_generated_this_month",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "subscription_expiry_date",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tier",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_usage_reset_date",
                table: "users");

            migrationBuilder.DropColumn(
                name: "paths_extended_this_month",
                table: "users");

            migrationBuilder.DropColumn(
                name: "paths_generated_this_month",
                table: "users");

            migrationBuilder.DropColumn(
                name: "subscription_expiry_date",
                table: "users");

            migrationBuilder.DropColumn(
                name: "tier",
                table: "users");
        }
    }
}
