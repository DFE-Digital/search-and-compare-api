using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class ExtendCourseDistanceToCampus : Migration
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
                    SELECT course.""Id"",                        
                        MIN(LEAST(earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")), 
			                earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc2.""Latitude"",loc2.""Longitude"")))) AS ""Distance""
                    FROM ""course""
                    JOIN location loc ON course.""ProviderLocationId"" = loc.""Id""
                    JOIN campus on campus.""CourseId"" = course.""Id""
                    JOIN location loc2 ON campus.""LocationId"" = loc2.""Id""
                    WHERE (""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"") OR
		                    ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc2.""Latitude"",loc2.""Longitude""))
                    AND LEAST(earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")), 
			                earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc2.""Latitude"",loc2.""Longitude""))) <= rad
			        GROUP BY course.""Id""
                $$ LANGUAGE SQL;");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION course_distance(double precision,
                                                                 double precision,
                                                                 double precision);");

            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION course_distance( lat DOUBLE PRECISION, 
                                                            lon DOUBLE PRECISION, 
                                                            rad DOUBLE PRECISION) 
                                            RETURNS TABLE (""Id"" integer,
                                                           ""Distance"" double precision) AS $$
                    SELECT course.""Id"",                        
                        earth_distance(ll_to_earth(lat, lon), ll_to_earth(""Latitude"",""Longitude"")) AS ""Distance""
                    FROM ""course""
                    JOIN location ON course.""ProviderLocationId"" = location.""Id""
                    WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(location.""Latitude"",location.""Longitude"") 
                    AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(location.""Latitude"",location.""Longitude"")) <= rad
                $$ LANGUAGE SQL;");

        }
    }
}
