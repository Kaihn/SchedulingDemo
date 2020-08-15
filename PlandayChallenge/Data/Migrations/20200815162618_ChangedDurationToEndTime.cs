using Microsoft.EntityFrameworkCore.Migrations;

namespace PlandayChallenge.Migrations
{
    public partial class ChangedDurationToEndTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Shifts");

            migrationBuilder.AddColumn<int>(
                name: "EndTime",
                table: "Shifts",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Shifts");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
