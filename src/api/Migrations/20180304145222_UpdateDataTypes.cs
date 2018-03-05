using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SearchAndCompare.Migrations
{
    public partial class UpdateDataTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Salary_Minimum",
                table: "course",
                type: "int4",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Salary_Maximum",
                table: "course",
                type: "int4",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Fees_Uk",
                table: "course",
                type: "int4",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "Fees_International",
                table: "course",
                type: "int4",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<int>(
                name: "Fees_Eu",
                table: "course",
                type: "int4",
                nullable: false,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Salary_Minimum",
                table: "course",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int4",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Salary_Maximum",
                table: "course",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int4",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Fees_Uk",
                table: "course",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int4");

            migrationBuilder.AlterColumn<long>(
                name: "Fees_International",
                table: "course",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int4");

            migrationBuilder.AlterColumn<long>(
                name: "Fees_Eu",
                table: "course",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int4");
        }
    }
}
