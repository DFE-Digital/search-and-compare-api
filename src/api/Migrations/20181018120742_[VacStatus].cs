using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class VacStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasVacancies",
                table: "course",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VacStatus",
                table: "campus",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasVacancies",
                table: "course");

            migrationBuilder.DropColumn(
                name: "VacStatus",
                table: "campus");
        }
    }
}
