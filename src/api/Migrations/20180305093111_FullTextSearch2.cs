using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SearchAndCompare.Migrations
{
    public partial class FullTextSearch2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropIndex("idx_fts_course");
            migrationBuilder.Sql(@"DROP FUNCTION gin_fts_course_fn(id INTEGER);");

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

            migrationBuilder.Sql(@"
CREATE INDEX idx_fts_provider ON ""provider"" USING gin(to_tsvector('english', ""Name""));
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("idx_fts_provider");
            migrationBuilder.Sql(@"DROP FUNCTION course_matching_query(query TSQUERY);");

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION gin_fts_course_fn(id INTEGER)
RETURNS tsvector
AS
$BODY$
    SELECT to_tsvector(""p1"".""Name"") || to_tsvector(coalesce(""p2"".""Name"", ''))
    FROM ""course""
    LEFT OUTER JOIN ""provider"" AS ""p1"" ON ""course"".""ProviderId"" = ""p1"".""Id""
    LEFT OUTER JOIN ""provider"" AS ""p2"" ON ""course"".""AccreditingProviderId"" = ""p2"".""Id""
    WHERE ""course"".""Id"" = $1
$BODY$
LANGUAGE SQL
IMMUTABLE;
");

            migrationBuilder.Sql(@"
CREATE INDEX idx_fts_course ON ""course"" USING gin(gin_fts_course_fn(""Id""));
");

        }
    }
}
