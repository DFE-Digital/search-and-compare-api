using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class DropCourseMatchingQuery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            migrationBuilder.Sql(@"DROP FUNCTION course_matching_query(query TSQUERY);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION course_matching_query(query TSQUERY)
RETURNS TABLE(""Id"" INTEGER)
AS
$BODY$
    SELECT ""course"".""Id""
    FROM ""course""
    LEFT OUTER JOIN ""provider"" AS ""p1"" ON ""course"".""ProviderId"" = ""p1"".""Id""
    LEFT OUTER JOIN ""provider"" AS ""p2"" ON ""course"".""AccreditingProviderId"" = ""p2"".""Id""
    WHERE ((to_tsvector('english', ""p1"".""Name"") || to_tsvector('english', coalesce(""p2"".""Name"", ''))) @@ query) IS TRUE
$BODY$
LANGUAGE SQL;
");
        }
    }
}
