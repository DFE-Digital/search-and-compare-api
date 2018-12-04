using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class FasterLocationQuery : Migration
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
                                                           ""Distance"" double precision,
                                                           ""DistanceAddress"" text) AS $$
                    WITH distances as (
                        SELECT ""Id"", ""Address"",
                            111045.0* DEGREES(ACOS(COS(RADIANS(lat))
                                    * COS(RADIANS(""Latitude""))
                                    * COS(RADIANS(lon) - RADIANS(""Longitude""))
                                    + SIN(RADIANS(lat))
                                    * SIN(RADIANS(""Latitude"")))) AS ""Distance""
                        from location
                        where 111045.0* DEGREES(ACOS(COS(RADIANS(lat))
                                    * COS(RADIANS(""Latitude""))
                                    * COS(RADIANS(lon) - RADIANS(""Longitude""))
                                    + SIN(RADIANS(lat))
                                    * SIN(RADIANS(""Latitude"")))) <  rad
                    ), matching_campuses AS (
                        select campus.""CourseId"" as ""Id"", ""Distance"", ""Address"" as ""DistanceAddress""
                        from campus
                        JOIN distances on distances.""Id"" = ""LocationId""    
                    ), matching_courses AS (
                        select course.""Id"", ""Distance"", ""Address"" as ""DistanceAddress""
                        from course
                        JOIN distances on distances.""Id"" = ""ProviderLocationId""
                    ), matching_all AS (
                        SELECT ""Id"", ""Distance"", ""DistanceAddress"" from matching_campuses
                        
                        UNION ALL

                        SELECT ""Id"", ""Distance"", ""DistanceAddress"" from matching_courses

                    ), matching_all_with_rows AS (
                        SELECT ""Id"", ""Distance"", ""DistanceAddress"", ROW_NUMBER() OVER(PARTITION BY ""Id"" ORDER BY ""Distance"" ASC) as ordinal
                        FROM matching_all
                    )
                    SELECT ""Id"", ""Distance"", ""DistanceAddress""
                    FROM matching_all_with_rows
                    where ordinal = 1      
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
                                                           ""Distance"" double precision,
                                                           ""DistanceAddress"" text) AS $$
                    WITH all_locs AS (
                        SELECT course.""Id"", earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) AS ""Distance"", loc.""Address""
                        FROM ""course""
                        JOIN location loc ON course.""ProviderLocationId"" = loc.""Id""
                        WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"")
                            AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) <= rad
                        UNION ALL
                        SELECT course.""Id"", earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) AS ""Distance"", loc.""Address"" 
                        FROM ""course""
                        JOIN campus on campus.""CourseId"" = course.""Id""
                        JOIN location loc ON campus.""LocationId"" = loc.""Id""			
                        WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(loc.""Latitude"",loc.""Longitude"")
                            AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(loc.""Latitude"",loc.""Longitude"")) <= rad
                    ), all_locs_rows AS (
                        SELECT *, ROW_NUMBER() OVER(PARTITION BY ""Id"" ORDER BY ""Distance"" ASC) as ordinal
                        FROM all_locs
                    )
                    SELECT ""Id"", ""Distance"", ""Address"" AS ""DistanceAddress""
                    FROM all_locs_rows WHERE ordinal = 1      
                $$ LANGUAGE SQL;");   

        }
    }
}
