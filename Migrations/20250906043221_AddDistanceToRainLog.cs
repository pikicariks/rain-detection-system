using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainDetectionApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDistanceToRainLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Distance",
                table: "RainLogs",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "RainLogs");
        }
    }
}
