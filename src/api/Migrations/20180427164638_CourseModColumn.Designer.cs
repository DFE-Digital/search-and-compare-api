﻿// <auto-generated />
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SearchAndCompare.Migrations
{
    [DbContext(typeof(CourseDbContext))]
    [Migration("20180427164638_CourseModColumn")]
    partial class CourseModColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Campus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CampusCode");

                    b.Property<int?>("CourseId");

                    b.Property<int?>("LocationId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("LocationId");

                    b.ToTable("campus");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Contact", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("Email");

                    b.Property<string>("Fax");

                    b.Property<string>("Phone");

                    b.Property<string>("Website");

                    b.HasKey("Id");

                    b.ToTable("contact");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AccreditingProviderId");

                    b.Property<int>("AgeRange");

                    b.Property<DateTime?>("ApplicationsAcceptedFrom");

                    b.Property<int?>("ContactDetailsId");

                    b.Property<double?>("Distance");

                    b.Property<string>("Duration");

                    b.Property<int>("FullTime");

                    b.Property<int>("IncludesPgce");

                    b.Property<bool>("IsSalaried");

                    b.Property<string>("Mod");

                    b.Property<string>("Name");

                    b.Property<int>("PartTime");

                    b.Property<string>("ProgrammeCode");

                    b.Property<string>("ProviderCodeName");

                    b.Property<int>("ProviderId");

                    b.Property<int?>("ProviderLocationId");

                    b.Property<int>("RouteId");

                    b.Property<DateTime?>("StartDate");

                    b.HasKey("Id");

                    b.HasIndex("AccreditingProviderId");

                    b.HasIndex("ContactDetailsId")
                        .IsUnique();

                    b.HasIndex("ProviderId");

                    b.HasIndex("ProviderLocationId");

                    b.HasIndex("RouteId");

                    b.ToTable("course");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.CourseDescriptionSection", b =>
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

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.DefaultCourseDescriptionSection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Name");

                    b.HasKey("Id");

                    b.ToTable("default-course-description-section");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.FeeCaps", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("EndYear");

                    b.Property<long>("EuFees");

                    b.Property<long>("InternationalFees");

                    b.Property<int>("StartYear");

                    b.Property<long>("UkFees");

                    b.HasKey("Id");

                    b.ToTable("feecaps");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Joins.CourseSubject", b =>
                {
                    b.Property<int>("CourseId");

                    b.Property<int>("SubjectId");

                    b.HasKey("CourseId", "SubjectId");

                    b.HasIndex("SubjectId");

                    b.ToTable("course_subject");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Location", b =>
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

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Provider", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("ProviderCode");

                    b.HasKey("Id");

                    b.ToTable("provider");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Route", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsSalaried");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("route");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Subject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("FundingId");

                    b.Property<bool>("IsSubjectKnowledgeEnhancementAvailable");

                    b.Property<string>("Name");

                    b.Property<int?>("SubjectAreaId");

                    b.HasKey("Id");

                    b.HasIndex("FundingId");

                    b.HasIndex("SubjectAreaId");

                    b.ToTable("subject");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.SubjectArea", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("Ordinal");

                    b.HasKey("Id");

                    b.ToTable("subject-area");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.SubjectFunding", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BursaryFirst");

                    b.Property<int?>("BursaryLowerSecond");

                    b.Property<int?>("BursaryUpperSecond");

                    b.Property<int?>("EarlyCareerPayments");

                    b.Property<int?>("Scholarship");

                    b.HasKey("Id");

                    b.ToTable("subject-funding");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Campus", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Course", "Course")
                        .WithMany("Campuses")
                        .HasForeignKey("CourseId");

                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Location", "Location")
                        .WithMany()
                        .HasForeignKey("LocationId");
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Course", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Provider", "AccreditingProvider")
                        .WithMany("AccreditedCourses")
                        .HasForeignKey("AccreditingProviderId");

                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Contact", "ContactDetails")
                        .WithOne("Course")
                        .HasForeignKey("GovUk.Education.SearchAndCompare.Domain.Models.Course", "ContactDetailsId");

                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Provider", "Provider")
                        .WithMany("Courses")
                        .HasForeignKey("ProviderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Location", "ProviderLocation")
                        .WithMany()
                        .HasForeignKey("ProviderLocationId");

                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Route", "Route")
                        .WithMany("Courses")
                        .HasForeignKey("RouteId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("GovUk.Education.SearchAndCompare.Domain.Models.Fees", "Fees", b1 =>
                        {
                            b1.Property<int>("CourseId");

                            b1.Property<int>("Eu");

                            b1.Property<int>("International");

                            b1.Property<int>("Uk");

                            b1.ToTable("course");

                            b1.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Course")
                                .WithOne("Fees")
                                .HasForeignKey("GovUk.Education.SearchAndCompare.Domain.Models.Fees", "CourseId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });

                    b.OwnsOne("GovUk.Education.SearchAndCompare.Domain.Models.Salary", "Salary", b1 =>
                        {
                            b1.Property<int>("CourseId");

                            b1.Property<int?>("Maximum");

                            b1.Property<int?>("Minimum");

                            b1.ToTable("course");

                            b1.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Course")
                                .WithOne("Salary")
                                .HasForeignKey("GovUk.Education.SearchAndCompare.Domain.Models.Salary", "CourseId")
                                .OnDelete(DeleteBehavior.Cascade);
                        });
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.CourseDescriptionSection", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Course", "Course")
                        .WithMany("DescriptionSections")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Joins.CourseSubject", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Course", "Course")
                        .WithMany("CourseSubjects")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.Subject", "Subject")
                        .WithMany("CourseSubjects")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GovUk.Education.SearchAndCompare.Domain.Models.Subject", b =>
                {
                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.SubjectFunding", "Funding")
                        .WithMany()
                        .HasForeignKey("FundingId");

                    b.HasOne("GovUk.Education.SearchAndCompare.Domain.Models.SubjectArea", "SubjectArea")
                        .WithMany("Subjects")
                        .HasForeignKey("SubjectAreaId");
                });
#pragma warning restore 612, 618
        }
    }
}
