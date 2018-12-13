using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class ClearOrphanedCampusRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from campus where \"CourseId\" is null;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //do nuffink
        }
    }
}
