using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class AddedSenFlagtoCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSen",
                table: "course",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSen",
                table: "course");
        }
    }
}
