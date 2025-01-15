using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Backup.Infrasctructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NextBackupDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BackupIntervalInMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    WordDaysId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkHoursId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsMondayWorking = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTuesdayWorking = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsWednesdayWorking = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsThursdayWorking = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFridayWorking = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSaturdayWorking = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSundayWorking = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartHour = table.Column<byte>(type: "INTEGER", nullable: false),
                    EndHour = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkHours", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "WorkDays");

            migrationBuilder.DropTable(
                name: "WorkHours");
        }
    }
}
