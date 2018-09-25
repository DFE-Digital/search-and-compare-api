using System.Collections.Generic;
using System.Linq;
using GovUk.Education.SearchAndCompare.Api.Controllers;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using NUnit;
using GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.DatabaseAccess;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.Controllers
{
    [TestFixture]
    public class CoursesControllerTests : CourseDbContextIntegrationBase
    {
        CoursesController subject;

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<CoursesController>>();
            subject = new CoursesController(context, loggerMock.Object);
        }

        [Test]
        public void ImportNullCourse()
        {
            var result = subject.Index(null);

            AssertBad(result);
        }

        [Test]
        public void ImportCourse_Null_CourseSubjects()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.CourseSubjects = null;
            }

            var result = subject.Index(courses);
            courses.Any(x => x.IsValid(false)).Should().BeFalse();
            AssertBad(result);
        }

        [Test]
        public void ImportCourse_Null_CourseSubjects_Subject()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.CourseSubjects = new List<CourseSubject>(){
                    new CourseSubject(){Subject = null}};
            }

            var result = subject.Index(courses);
            courses.Any(x => x.IsValid(false)).Should().BeFalse();
            AssertBad(result);
        }

        [Test]
        public void ImportCourse_Null_Route()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Route = null;
            }

            var result = subject.Index(courses);
            courses.Any(x => x.IsValid(false)).Should().BeFalse();
            AssertBad(result);
        }

        [Test]
        public void ImportCourse_Null_Provider()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Provider = null;
            }

            var result = subject.Index(courses);
            courses.Any(x => x.IsValid(false)).Should().BeFalse();

            AssertBad(result);
        }

        [Test]
        public void ImportCourse_Null_AccreditingProvider_ProviderCode()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.AccreditingProvider.ProviderCode = null;
            }

            var result = subject.Index(courses);

            courses.Any(x => x.IsValid(false)).Should().BeFalse();

            AssertBad(result);
        }

        [Test]
        public void ImportCourse_Null_AccreditingProvider()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.AccreditingProvider = null;
            }

            var result = subject.Index(courses);
            courses.All(x => x.IsValid(false)).Should().BeTrue();


            AssertOkay(result);
        }

        [Test]
        [Ignore("Need Clarification")]
        public void ImportCourse_Null_ContactDetails()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.ContactDetails = null;
            }

            var result = subject.Index(courses);

            courses.Any(x => x.IsValid(false)).Should().BeFalse();

            AssertBad(result);
        }

        [Test]
        public void ImportCourse_Null_Campuses()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Campuses = null;
            }

            var result = subject.Index(courses);


            AssertBad(result);
            courses.Any(x => x.IsValid(false)).Should().BeFalse();

        }

        [Test]
        public void ImportCourse_Null_Campuses_Location()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Campuses = new List<Campus>()
                {
                    new Campus {Location = null}
                };
            }

            var result = subject.Index(courses);

            courses.Any(x => x.IsValid(false)).Should().BeFalse();

            AssertBad(result);
        }

        [Test]
        public void ImportCourse_Null_ProviderLocation()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.ProviderLocation = null;
            }

            var result = subject.Index(courses);

            courses.All(x => x.IsValid(false)).Should().BeTrue();

            AssertOkay(result);
        }

        [Test]
        public void ImportOneCourse()
        {
            var courses = GetCourses(1);
            var result = subject.Index(courses);

            courses.All(x => x.IsValid(false)).Should().BeTrue();
            AssertOkay(result);
        }

        [Test]
        public void ImportTwoCourses()
        {
            var courses = GetCourses(2);
            var result = subject.Index(courses);

            AssertOkay(result);

            var resultingCourses = context.GetCoursesWithProviderSubjectsRouteAndCampuses()
                .Include(x => x.DescriptionSections)
                .ToList();

            var resultingProviders = context.Providers.ToList();

            // deduplicated
            resultingProviders.Count.Should().Be(1);
            context.Routes.Count().Should().Be(1);
            context.Subjects.Count().Should().Be(1);
            context.Locations.Count().Should().Be(1);

            // non-deduplicated
            resultingCourses.Count.Should().Be(2);
            resultingCourses.SelectMany(x => x.DescriptionSections).Distinct().Count().Should().Be(2);
            context.Campuses.Count().Should().Be(4);
            context.CourseSubjects.Count().Should().Be(2);
            context.Contacts.Count().Should().Be(2);

            object.ReferenceEquals(resultingCourses[0].Provider, resultingCourses[1].Provider).Should().BeTrue();
        }

        [Test]
        public void ImportTwoCoursesTwice()
        {
            var result1 = subject.Index(GetCourses(20));
            AssertOkay(result1);

            var result2 = subject.Index(GetCourses(2));
            AssertOkay(result2);

            var resultingCourses = context.GetCoursesWithProviderSubjectsRouteAndCampuses().ToList();
            var resultingProviders = context.Providers.ToList();

            // deduplicated
            resultingProviders.Count.Should().Be(1);
            context.Routes.Count().Should().Be(1);
            context.Subjects.Count().Should().Be(1);
            context.Locations.Count().Should().Be(1);
            object.ReferenceEquals(resultingCourses[0].Provider, resultingCourses[1].Provider).Should().BeTrue();

            // non-deduplicated
            resultingCourses.Count.Should().Be(2);
            resultingCourses.SelectMany(x => x.DescriptionSections).Distinct().Count().Should().Be(2);
            context.Campuses.Count().Should().Be(4);
            context.CourseSubjects.Count().Should().Be(2);
            context.Contacts.Count().Should().Be(2);
        }

        [Test]
        public void GeocodingIsPreservedAcrossImports()
        {
            // initial import
            var result = subject.Index(GetCourses(1));

            AssertOkay(result);

            context.Locations.Single().Latitude.Should().NotBe(51.0);
            context.Locations.Single().Longitude.Should().NotBe(13.7);

            // set some coordinates
            context.Locations.Single().Latitude = 51.0;
            context.Locations.Single().Longitude = 13.7;
            context.SaveChanges();

            // second import, with equivalent addresses
            subject.Index(GetCourses(1));

            // coordinates previously set are still there
            context.Locations.Single().Latitude.Should().Be(51.0);
            context.Locations.Single().Longitude.Should().Be(13.7);
        }

        [Test]
        public void SubjectsArePreservedAcrossImports()
        {
            // initial import
            var result = subject.Index(GetCourses(1));

            AssertOkay(result);

            // associate with a subject area
            context.Subjects.Single().SubjectArea.Should().BeNull();
            context.Subjects.Single().SubjectArea = new SubjectArea {
                Name = "Primary"
            };
            context.SaveChanges();

            // second import, with equivalent subject
            subject.Index(GetCourses(1));

            // subject area previously set is still there
            context.Subjects.Single().SubjectArea.Should().NotBeNull();
            context.Subjects.Single().SubjectArea.Name.Should().Be("Primary");
        }

        internal void AssertBad(IActionResult result)
        {
            AssertStatusCode(result, 400);
        }

        internal static void AssertOkay(IActionResult result)
        {
            AssertStatusCode(result, 200);
        }

        internal static void AssertStatusCode(IActionResult result, int statusCode)
        {
            result.Should().NotBeNull();

            var statusCodeResult  = result as StatusCodeResult;

            statusCodeResult.Should().NotBeNull();
            statusCodeResult.StatusCode.Should().Be(statusCode);
        }

        internal static List<Course> GetCourses(int count)
        {
            var courses = new List<Course>();

            for (int i = 0; i < count; i++)
            {
                var course = GetCourse(i);

                courses.Add(course);
            }

            return courses;
        }

        internal static Course GetCourse(int i)
        {
            var course = new Course
            {
                ProgrammeCode = "ProgrammeCode" + i,
                Name = "Name" + i,
                // UnNeed
                ProviderId = 42,
                Provider = new Provider
                {
                    // UnNeed
                    Id = 24 + i,
                    Name = "Name",
                    ProviderCode = "ProviderCode"
                },

                // UnNeed
                AccreditingProvider = new Provider
                {
                    // UnNeed
                    Id = 124 + i,
                    Name = "Name (accrediting)",
                    ProviderCode = "ProviderCode"
                },

                AgeRange = AgeRange.Secondary,
                // Needed
                Route = new Route
                {
                    Name = "Scitt",
                    IsSalaried = true
                },

                // need instance of at least zero unenriched
                DescriptionSections = new Collection<CourseDescriptionSection>
                {
                    new CourseDescriptionSection
                    {
                        Name = "section",
                        Text = "Section text"
                    }
                },

                // need instance of at least one
                Campuses = new Collection<Campus>
                {
                    new Campus
                    {
                        CampusCode = "A",
                        Name = "CampusA",
                        Location = new Location
                        {
                            Address = "Common location"
                        }
                    },
                    new Campus
                    {
                        CampusCode = "B",
                        Name = "CampusB",
                        Location = new Location
                        {
                            Address = "Common location"
                        }
                    }
                },

                // need instance of at least one

                CourseSubjects = new Collection<CourseSubject>
                {
                    new CourseSubject
                    {
                        Subject = new Subject
                        {
                            Name = "Physics"
                        }
                    }
                },

                IncludesPgce = IncludesPgce.No,

                // need fee or salary
                Fees = new Fees(),
                Salary = new Salary(),

                // need the full object
                ContactDetails = new Contact(),

                // need the address part
                ProviderLocation = new Location
                {
                    Address = "Common location"
                }
            };

            return course;
        }

    }
}