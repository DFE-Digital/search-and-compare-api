﻿// <auto-generated />
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace SearchAndCompare.Migrations
{
    [DbContext(typeof(CourseDbContext))]
    [Migration("20180109143630_LocationIndex")]
    partial class LocationIndex
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Campus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<DateTime?>("ApplicationsAcceptedFrom");

                    b.Property<string>("CampusCode");

                    b.Property<int>("FullTime");

                    b.Property<string>("Name");

                    b.Property<int>("PartTime");

                    b.HasKey("Id");

                    b.ToTable("campus");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AccreditingProviderId");

                    b.Property<int>("AgeRange");

                    b.Property<int>("IncludesPgce");

                    b.Property<string>("Name");

                    b.Property<string>("ProgrammeCode");

                    b.Property<string>("ProviderCodeName");

                    b.Property<int>("ProviderId");

                    b.Property<int?>("ProviderLocationId");

                    b.Property<int>("RouteId");

                    b.HasKey("Id");

                    b.HasIndex("AccreditingProviderId");

                    b.HasIndex("ProviderId");

                    b.HasIndex("ProviderLocationId");

                    b.HasIndex("RouteId");

                    b.ToTable("course");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.CourseDescriptionSection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CourseId");

                    b.Property<string>("Name");

                    b.Property<int>("Ordinal");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.ToTable("course-description-section");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.DefaultCourseDescriptionSection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Name");

                    b.HasKey("Id");

                    b.ToTable("default-course-description-section");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Joins.CourseCampus", b =>
                {
                    b.Property<int>("CourseId");

                    b.Property<int>("CampusId");

                    b.HasKey("CourseId", "CampusId");

                    b.HasIndex("CampusId");

                    b.ToTable("course_campus");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Joins.CourseSubject", b =>
                {
                    b.Property<int>("CourseId");

                    b.Property<int>("SubjectId");

                    b.HasKey("CourseId", "SubjectId");

                    b.HasIndex("SubjectId");

                    b.ToTable("course_subject");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.HasKey("Id");

                    b.HasIndex("Longitude", "Latitude");

                    b.ToTable("location");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Provider", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("ProviderCode");

                    b.HasKey("Id");

                    b.ToTable("provider");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Route", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("route");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Subject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int?>("SubjectAreaId");

                    b.HasKey("Id");

                    b.HasIndex("SubjectAreaId");

                    b.ToTable("subject");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.SubjectArea", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("Ordinal");

                    b.HasKey("Id");

                    b.ToTable("subject-area");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Course", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Provider", "AccreditingProvider")
                        .WithMany("AccreditedCourses")
                        .HasForeignKey("AccreditingProviderId");

                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Provider", "Provider")
                        .WithMany("Courses")
                        .HasForeignKey("ProviderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Location", "ProviderLocation")
                        .WithMany()
                        .HasForeignKey("ProviderLocationId");

                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Route", "Route")
                        .WithMany("Courses")
                        .HasForeignKey("RouteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.CourseDescriptionSection", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Course", "Course")
                        .WithMany("DescriptionSections")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Joins.CourseCampus", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Campus", "Campus")
                        .WithMany("CourseCampuses")
                        .HasForeignKey("CampusId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Course", "Course")
                        .WithMany("CourseCampuses")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Joins.CourseSubject", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Course", "Course")
                        .WithMany("CourseSubjects")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GovUk.Education.SearchAndCompare.Models.Subject", "Subject")
                        .WithMany("CourseSubjects")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Models.Subject", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Models.SubjectArea", "SubjectArea")
                        .WithMany()
                        .HasForeignKey("SubjectAreaId");
                });
#pragma warning restore 612, 618
        }
    }
}
