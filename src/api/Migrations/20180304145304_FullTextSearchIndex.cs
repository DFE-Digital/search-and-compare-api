using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SearchAndCompare.Migrations
{
    public partial class FullTextSearchIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("idx_fts_course");
            migrationBuilder.Sql(@"DROP FUNCTION gin_fts_course_fn(id INTEGER);");
        }
    }
}
