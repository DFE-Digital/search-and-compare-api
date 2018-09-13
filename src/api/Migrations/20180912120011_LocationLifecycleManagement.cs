using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SearchAndCompare.Migrations
{
    public partial class LocationLifecycleManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "location",
                type: "float8",
                nullable: true,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "location",
                type: "float8",
                nullable: true,
                oldClrType: typeof(double));
            
            migrationBuilder.Sql(@"
                UPDATE location SET ""Latitude"" = null, ""Longitude"" = null WHERE ""Latitude"" = 0
            ");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastGeocodedUtc",
                table: "location",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGeocodedUtc",
                table: "location");

            migrationBuilder.Sql(@"
                UPDATE location SET ""Latitude"" = 0, ""Longitude"" = 0 WHERE ""Latitude"" = null
            ");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "location",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float8",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "location",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float8",
                oldNullable: true);
        }
    }
}
