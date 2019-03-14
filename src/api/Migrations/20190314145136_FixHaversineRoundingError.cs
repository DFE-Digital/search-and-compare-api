using Microsoft.EntityFrameworkCore.Migrations;

namespace SearchAndCompare.Migrations
{
    public partial class FixHaversineRoundingError : Migration
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
                    WITH matching_locations as (
                        -- The Haversine formula for estimating distances on a spherical earth.
                        -- Calculates distance in degrees and multiplies by 111045 metres per degree.
                        SELECT ""Id"", ""Address"",
                            111045.0 *
                            DEGREES(
                                ACOS(
                                    LEAST(
                                        COS(RADIANS(lat))
                                        * COS(RADIANS(""Latitude""))
                                        * COS(RADIANS(lon) - RADIANS(""Longitude""))
                                        + SIN(RADIANS(lat))
                                        * SIN(RADIANS(""Latitude""))
                                    ,1)
                                )
                            ) AS ""Distance""
                        from location
                        where
                            111045.0 *
                            DEGREES(
                                ACOS(
                                    LEAST(
                                        COS(RADIANS(lat))
                                        * COS(RADIANS(""Latitude""))
                                        * COS(RADIANS(lon) - RADIANS(""Longitude""))
                                        + SIN(RADIANS(lat))
                                        * SIN(RADIANS(""Latitude""))
                                    ,1)
                                )
                            ) <  rad
                    ), matching_campuses AS (
                        select campus.""CourseId"" as ""Id"", ""Distance"", ""Address"" as ""DistanceAddress""
                        from campus
                        JOIN matching_locations on matching_locations.""Id"" = ""LocationId""
                    ), matching_courses AS (
                        select course.""Id"", ""Distance"", ""Address"" as ""DistanceAddress""
                        from course
                        JOIN matching_locations on matching_locations.""Id"" = ""ProviderLocationId""
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
                    WITH matching_locations as (
                        -- The Haversine formula for estimating distances on a spherical earth.
                        -- Calculates distance in degrees and multiplies by 111045 metres per degree.
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
                        JOIN matching_locations on matching_locations.""Id"" = ""LocationId""
                    ), matching_courses AS (
                        select course.""Id"", ""Distance"", ""Address"" as ""DistanceAddress""
                        from course
                        JOIN matching_locations on matching_locations.""Id"" = ""ProviderLocationId""
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
    }
}
