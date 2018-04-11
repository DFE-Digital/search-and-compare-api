using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SearchAndCompare.Migrations
{
    public partial class RefactorCampuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_course_ContactDetailsId",
                table: "course");

            migrationBuilder.DropColumn(
                name: "Address_County",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Line1",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Line2",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Line3",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Line4",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Line5",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_Line6",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "Address_PostCode",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "ApplicationsAcceptedFrom",
                table: "campus");

            migrationBuilder.DropColumn(
                name: "FullTime",
                table: "campus");

            migrationBuilder.DropColumn(
                name: "PartTime",
                table: "campus");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationsAcceptedFrom",
                table: "course",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "course",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FullTime",
                table: "course",
                type: "int4",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartTime",
                table: "course",
                type: "int4",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "course",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "contact",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_ContactDetailsId",
                table: "course",
                column: "ContactDetailsId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_course_ContactDetailsId",
                table: "course");

            migrationBuilder.DropColumn(
                name: "ApplicationsAcceptedFrom",
                table: "course");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "course");

            migrationBuilder.DropColumn(
                name: "FullTime",
                table: "course");

            migrationBuilder.DropColumn(
                name: "PartTime",
                table: "course");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "course");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "contact");

            migrationBuilder.AddColumn<string>(
                name: "Address_County",
                table: "contact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Line1",
                table: "contact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Line2",
                table: "contact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Line3",
                table: "contact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Line4",
                table: "contact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Line5",
                table: "contact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Line6",
                table: "contact",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_PostCode",
                table: "contact",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationsAcceptedFrom",
                table: "campus",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FullTime",
                table: "campus",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PartTime",
                table: "campus",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_course_ContactDetailsId",
                table: "course",
                column: "ContactDetailsId");
        }
    }
}
