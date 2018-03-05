using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SearchAndCompare.Migrations
{
    public partial class StructuredContactDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactDetailsId",
                table: "course",
                type: "int4",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "contact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int4", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Fax = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Address_County = table.Column<string>(type: "text", nullable: true),
                    Address_Line1 = table.Column<string>(type: "text", nullable: true),
                    Address_Line2 = table.Column<string>(type: "text", nullable: true),
                    Address_Line3 = table.Column<string>(type: "text", nullable: true),
                    Address_Line4 = table.Column<string>(type: "text", nullable: true),
                    Address_Line5 = table.Column<string>(type: "text", nullable: true),
                    Address_Line6 = table.Column<string>(type: "text", nullable: true),
                    Address_PostCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_ContactDetailsId",
                table: "course",
                column: "ContactDetailsId",
                unique: false);

            migrationBuilder.AddForeignKey(
                name: "FK_course_contact_ContactDetailsId",
                table: "course",
                column: "ContactDetailsId",
                principalTable: "contact",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_contact_ContactDetailsId",
                table: "course");

            migrationBuilder.DropTable(
                name: "contact");

            migrationBuilder.DropIndex(
                name: "IX_course_ContactDetailsId",
                table: "course");

            migrationBuilder.DropColumn(
                name: "ContactDetailsId",
                table: "course");
        }
    }
}
