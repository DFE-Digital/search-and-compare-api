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

            migrationBuilder.AddColumn<long>(
                name: "Salary_Maximum",
                table: "course",
                type: "int8",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Salary_Minimum",
                table: "course",
                type: "int8",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Fees_Eu",
                table: "course",
                type: "int8",
                nullable: false,
                defaultValue: 9250L);

            migrationBuilder.AddColumn<long>(
                name: "Fees_International",
                table: "course",
                type: "int8",
                nullable: false,
                defaultValue: 16340L);

            migrationBuilder.AddColumn<long>(
                name: "Fees_Uk",
                table: "course",
                type: "int8",
                nullable: false,
                defaultValue: 9250L);
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

        }
    }
}
