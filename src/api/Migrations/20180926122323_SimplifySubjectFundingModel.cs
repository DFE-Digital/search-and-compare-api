using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class SimplifySubjectFundingModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BursaryLowerSecond",
                table: "subject-funding");

            migrationBuilder.DropColumn(
                name: "BursaryUpperSecond",
                table: "subject-funding");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BursaryLowerSecond",
                table: "subject-funding",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BursaryUpperSecond",
                table: "subject-funding",
                nullable: true);
        }
    }
}
