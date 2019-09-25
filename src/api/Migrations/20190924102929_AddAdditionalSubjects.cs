using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class AddAdditionalSubjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: subject table will retain old list of subject as well
            var insertScience = @"
                INSERT INTO subject (""Name"", ""SubjectAreaId"")
                SELECT 'Science', ""Id"" from ""subject-area""
                where ""Name"" = 'Secondary';
            ";
            migrationBuilder.Sql(insertScience);

            var insertPhilosophy = @"
                INSERT INTO subject (""Name"", ""SubjectAreaId"")
                SELECT 'Philosophy', ""Id"" from ""subject-area""
                where ""Name"" = 'Secondary';
            ";

            migrationBuilder.Sql(insertPhilosophy);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to go back
        }
    }
}
