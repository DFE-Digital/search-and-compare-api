using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SearchAndCompare.Migrations
{
    public partial class DistanceFunctionFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION course_distance(double precision,
                                                                 double precision,
                                                                 double precision);");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION course_distance( lat DOUBLE PRECISION, 
                                                            lon DOUBLE PRECISION, 
                                                            rad DOUBLE PRECISION) 
                                            RETURNS TABLE ( ""Id"" integer,  
                                                            ""AccreditingProviderId"" integer, 
                                                            ""AgeRange"" integer, 
                                                            ""IncludesPgce"" integer, 
                                                            ""Name"" text, 
                                                            ""ProgrammeCode"" text, 
                                                            ""ProviderCodeName"" text, 
                                                            ""ProviderId"" integer, 
                                                            ""ProviderLocationId"" integer, 
                                                            ""RouteId"" integer,
                                                            ""IsSalaried"" bool,
                                                            ""Salary_Maximum"" integer,
                                                            ""Salary_Minimum"" integer,
                                                            ""Fees_Eu"" integer,
                                                            ""Fees_International"" integer,
                                                            ""Fees_Uk"" integer,
                                                            ""Distance"" double precision) AS $$
                    SELECT course.""Id"",
                        course.""AccreditingProviderId"",
                        course.""AgeRange"",
                        course.""IncludesPgce"",
                        course.""Name"",
                        course.""ProgrammeCode"",
                        course.""ProviderCodeName"",
                        course.""ProviderId"",
                        course.""ProviderLocationId"",
                        course.""RouteId"",
                        course.""IsSalaried"",
                        course.""Salary_Maximum"",
                        course.""Salary_Minimum"",
                        course.""Fees_Eu"",
                        course.""Fees_International"",
                        course.""Fees_Uk"",
                        earth_distance(ll_to_earth(lat, lon), ll_to_earth(""Latitude"",""Longitude"")) AS ""Distance""
                    FROM ""course""
                    JOIN location ON course.""ProviderLocationId"" = location.""Id""
                    WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(location.""Latitude"",location.""Longitude"") 
                    AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(location.""Latitude"",location.""Longitude"")) <= rad
                $$ LANGUAGE SQL;");
        }
    }
}
