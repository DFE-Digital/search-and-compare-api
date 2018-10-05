using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class CampusDistance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION course_distance(double precision,
                                                                 double precision,
                                                                 double precision);");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE
                FUNCTION location_distance( lat DOUBLE PRECISION, lon DOUBLE PRECISION, radius DOUBLE PRECISION) 
                RETURNS TABLE (""LocationId"" integer, ""Distance"" double precision) AS
                $$

                SELECT loc.""Id"" as ""LocationId"", earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) AS ""Distance""
                FROM location loc
                WHERE ""earth_box""(ll_to_earth(lat, lon), radius) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"")
                    AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) <= radius

                $$ LANGUAGE SQL;
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION location_distance(double precision,
                                                                 double precision,
                                                                 double precision);");

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION course_distance( lat DOUBLE PRECISION, 
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
                $$ LANGUAGE SQL;
");

        }
    }
}
