using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RainDetectionApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommandType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CommandData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExecuted = table.Column<bool>(type: "bit", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RainLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AnalogValue = table.Column<int>(type: "int", nullable: false),
                    DigitalValue = table.Column<int>(type: "int", nullable: false),
                    IsRaining = table.Column<bool>(type: "bit", nullable: false),
                    ServoPosition = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RainLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_CreatedAt",
                table: "DeviceCommands",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCommands_IsExecuted",
                table: "DeviceCommands",
                column: "IsExecuted");

            migrationBuilder.CreateIndex(
                name: "IX_RainLogs_EventType",
                table: "RainLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_RainLogs_Timestamp",
                table: "RainLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_SettingName",
                table: "SystemSettings",
                column: "SettingName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceCommands");

            migrationBuilder.DropTable(
                name: "RainLogs");

            migrationBuilder.DropTable(
                name: "SystemSettings");
        }
    }
}
