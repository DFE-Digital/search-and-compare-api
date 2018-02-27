using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SearchAndCompare.Migrations
{
    public partial class AddFeesAndSalary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "fees",
                newName: "feecaps"
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsSalaried",
                table: "course",
                type: "bool",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                @"UPDATE course
                  SET ""IsSalaried"" = route.""IsSalaried""
                  FROM route
                  WHERE course.""RouteId"" = route.""Id"""
            );

            migrationBuilder.AddColumn<int>(
                name: "Salary_Maximum",
                table: "course",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Salary_Minimum",
                table: "course",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Fees_Eu",
                table: "course",
                type: "integer",
                nullable: false,
                defaultValue: 9250);

            migrationBuilder.AddColumn<int>(
                name: "Fees_International",
                table: "course",
                type: "integer",
                nullable: false,
                defaultValue: 16340);

            migrationBuilder.AddColumn<int>(
                name: "Fees_Uk",
                table: "course",
                type: "integer",
                nullable: false,
                defaultValue: 9250);

            migrationBuilder.Sql(@"DROP FUNCTION course_distance;");
            
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSalaried",
                table: "course");

            migrationBuilder.DropColumn(
                name: "Fees_Eu",
                table: "course");

            migrationBuilder.DropColumn(
                name: "Fees_International",
                table: "course");

            migrationBuilder.DropColumn(
                name: "Fees_Uk",
                table: "course");

            migrationBuilder.DropColumn(
                name: "Salary_Maximum",
                table: "course");

            migrationBuilder.DropColumn(
                name: "Salary_Minimum",
                table: "course");

            migrationBuilder.RenameTable(
                name: "feecaps",
                newName: "fees");

            migrationBuilder.Sql(@"DROP FUNCTION course_distance;");

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
                        earth_distance(ll_to_earth(lat, lon), ll_to_earth(""Latitude"",""Longitude"")) AS ""Distance""
                    FROM ""course""
                    JOIN location ON course.""ProviderLocationId"" = location.""Id""
                    WHERE ""earth_box""(ll_to_earth(lat, lon), rad) @> ll_to_earth(location.""Latitude"",location.""Longitude"") 
                    AND earth_distance(ll_to_earth(lat, lon), ll_to_earth(location.""Latitude"",location.""Longitude"")) <= rad
                $$ LANGUAGE SQL;");
        }
    }
}
