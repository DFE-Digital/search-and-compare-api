using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class SpeedUpLocationSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION course_distance(double precision,
                                                                 double precision,
                                                                 double precision);");

            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION course_distance( lat DOUBLE PRECISION, 
                                                            lon DOUBLE PRECISION, 
                                                            rad DOUBLE PRECISION) 
                                            RETURNS TABLE (""Id"" integer,
                                                           ""Distance"" double precision) AS $$
                    SELECT ""Id"", MIN(""Distance"") AS ""Distance""
                    FROM (
                        SELECT course.""Id"", earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) AS ""Distance""
                        FROM ""course""
                        JOIN location loc ON course.""ProviderLocationId"" = loc.""Id""
                        WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"")
                            AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) <= rad
                        UNION ALL
                        SELECT course.""Id"", earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) AS ""Distance"" 
                        FROM ""course""
                        JOIN campus on campus.""CourseId"" = course.""Id""
                        JOIN location loc ON campus.""LocationId"" = loc.""Id""			
                        WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"")
                            AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) <= rad ) x
                    GROUP BY ""Id""       
                $$ LANGUAGE SQL;");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION course_distance(double precision,
                                                                 double precision,
                                                                 double precision);");

            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION course_distance( lat DOUBLE PRECISION, 
                                                            lon DOUBLE PRECISION, 
                                                            rad DOUBLE PRECISION) 
                                            RETURNS TABLE (""Id"" integer,
                                                           ""Distance"" double precision) AS $$
                    SELECT course.""Id"",                        
                        MIN(LEAST(earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")), 
			                earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc2.""Latitude"",loc2.""Longitude"")))) AS ""Distance""
                    FROM ""course""
                    JOIN location loc ON course.""ProviderLocationId"" = loc.""Id""
                    LEFT OUTER JOIN campus on campus.""CourseId"" = course.""Id""
                    LEFT OUTER JOIN location loc2 ON campus.""LocationId"" = loc2.""Id""
                    WHERE (loc.""Id"" IS NOT NULL OR loc2.""Id"" IS NOT NULL) AND
                          (""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"") OR
		                    ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc2.""Latitude"",loc2.""Longitude""))
                    AND LEAST(earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")), 
			                earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc2.""Latitude"",loc2.""Longitude""))) <= rad
			        GROUP BY course.""Id""
                $$ LANGUAGE SQL;");

        }
    }
}
