using System;
using System.Threading.Tasks;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.Controllers
{
    [TestFixture]
    public class SaveSingleCourse_CoursesControllerTests : CourseDbContextIntegrationBase
    {
        CoursesController subject;

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<CoursesController>>();
            subject = new CoursesController(context, loggerMock.Object, new Mock<IConfiguration>().Object);
        }

        [TearDown]
        public new async Task TearDown()
        {
            await context.GetCoursesWithProviderSubjectsRouteAndCampuses().ForEachAsync(x =>
            {
                entitiesToCleanUp.Add(context.Entry(x));
            });

            await context.Campuses.ForEachAsync(x =>
            {
                entitiesToCleanUp.Add(context.Entry(x));
            });


            await context.Contacts.ForEachAsync(x =>
            {
                entitiesToCleanUp.Add(context.Entry(x));
            });
        }

        [Test]
        public async Task ImportNullCourse()
        {
            var result = await subject.SaveCourses(null);

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_CourseSubjects()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.CourseSubjects = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_Fees()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Fees = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);
        }

        [Test]
        public async Task ImportCourse_Null_Salary()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Salary = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);
        }

        [Test]
        public async Task ImportCourse_Null_CourseSubjects_Subject()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.CourseSubjects = new List<CourseSubject>(){
                    new CourseSubject(){Subject = null}};
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_Route()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Route = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_Provider()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Provider = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> {course});

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_AccreditingProvider_ProviderCode()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.AccreditingProvider.ProviderCode = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_AccreditingProvider()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.AccreditingProvider = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);
        }

        [Test]
        [Ignore("Need Clarification")]
        public async Task ImportCourse_Null_ContactDetails()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.ContactDetails = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_Campuses()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Campuses = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_Campuses_Location()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.Campuses = new List<Campus>()
                {
                    new Campus {Location = null}
                };
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertBad(result);
        }

        [Test]
        public async Task ImportCourse_Null_ProviderLocation()
        {
            var courses = GetCourses(1);
            foreach (var item in courses)
            {
                item.ProviderLocation = null;
            }

            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);
        }

        [Test]
        public async Task ImportOneCourse()
        {
            var courses = GetCourses(1);
            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);
        }

        [Test]
        public async Task ImportTwoCourses()
        {
            var courses = GetCourses(2);
            var course = courses.First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);

            var course1 = courses[1];
            var result1 = await subject.SaveCourses(new List<Course>{course1});

            AssertOkay(result1);
            var resultingCourses = context.GetCoursesWithProviderSubjectsRouteAndCampuses()
                .Include(x => x.DescriptionSections)
                .ToList();

            var resultingProviders = context.Providers.ToList();

            // deduplicated
            resultingProviders.Count.Should().Be(1);
            context.Routes.Count().Should().Be(1);
            context.Subjects.Count().Should().Be(1);

            var expectedLocations = GetExpectedLocations(courses);
            context.Locations.Count().Should().Be(expectedLocations.Count());

            // non-deduplicated
            resultingCourses.Count.Should().Be(2);
            resultingCourses.SelectMany(x => x.DescriptionSections).Distinct().Count().Should().Be(2);
            context.Campuses.Count().Should().Be(8);
            context.CourseSubjects.Count().Should().Be(2);
            context.Contacts.Count().Should().Be(2);

            object.ReferenceEquals(resultingCourses[0].Provider, resultingCourses[1].Provider).Should().BeTrue();
        }

        [Test]
        public void ImportSameCoursesTwice()
        {
            var course = GetCourses(1).First();
            var result1 = subject.SaveCourses(new List<Course> { course }).Result;
            AssertOkay(result1);

            var course2 = GetCourses(1).First();
            var result2 = subject.SaveCourses(new List<Course>{course2}).Result;
            AssertOkay(result2);

            var resultingCourses = context.GetCoursesWithProviderSubjectsRouteAndCampuses().ToList();
            var resultingProviders = context.Providers.ToList();

            resultingCourses.Count().Should().Be(1);
            var savedCourse = resultingCourses.First();
            savedCourse.Campuses.Count().Should().Be(course.Campuses.Count());
            savedCourse.DescriptionSections.Count().Should().Be(course.DescriptionSections.Count());
            resultingCourses.SelectMany(x => x.DescriptionSections).Count().Should().Be(course.DescriptionSections.Count());

            // deduplicated
            var courses = new List<Course> {course, course2};
            resultingProviders.Count.Should().Be(1);
            context.Routes.Count().Should().Be(1);
            context.Subjects.Count().Should().Be(1);
            var expectedLocations = GetExpectedLocations(courses);
            context.Locations.Count().Should().Be(expectedLocations.Count());
            context.Campuses.Count().Should().Be(4); //definately right

            // non-deduplicated            
            context.Contacts.Count().Should().Be(2); //probably wrong
            context.CourseSubjects.Count().Should().Be(1);            
        }

        [Test]
        public async Task GeocodingIsPreservedAcrossImports()
        {
            // initial import
            var course = GetCourses(1).First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);
            // set some coordinates
            var location = context.Locations.First(l => l.Latitude == null && l.Longitude == null);
            location.Latitude = 51.0;
            location.Longitude = 13.7;
            context.SaveChanges();

            // second import, with equivalent addresses
            var course2 = GetCourses(1).First();
            await subject.SaveCourses(new List<Course>{course2});

            // coordinates previously set are still there
            var loc = context.Locations.FirstOrDefault(x => x.Latitude == 51.0 && x.Longitude == 13.7);
            loc.Should().NotBeNull();
        }

        [Test]
        public async Task SubjectsArePreservedAcrossImports()
        {
            // initial import
            var course = GetCourses(1).First();
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);

            // associate with a subject area
            context.Subjects.Single().SubjectArea.Should().BeNull();
            context.Subjects.Single().SubjectArea = new SubjectArea {
                Name = "Primary"
            };
            context.SaveChanges();

            // second import, with equivalent subject
            var course2 = GetCourses(1).First();
            await subject.SaveCourses(new List<Course>{course2});

            // subject area previously set is still there
            context.Subjects.Single().SubjectArea.Should().NotBeNull();
            context.Subjects.Single().SubjectArea.Name.Should().Be("Primary");
        }
        [Test]
        public async Task SenCoursesArePreservedAcrossImports()
        {
            // initial import
            var course = GetCourses(1).First();
            course.IsSen = true;
            var result = await subject.SaveCourses(new List<Course> { course });

            AssertOkay(result);

            var savedCourse = context.GetCoursesWithProviderSubjectsRouteAndCampuses().Single();
            savedCourse.IsSen.Should().BeTrue();
            // next import
            var course2 = GetCourses(1).First();
            course2.IsSen = false;
            var result2 = await subject.SaveCourses(new List<Course> { course2 });

            AssertOkay(result2);

            savedCourse = context.GetCoursesWithProviderSubjectsRouteAndCampuses().Single();
            savedCourse.IsSen.Should().BeFalse();
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
        internal static List<Location> GetExpectedLocations(IList<Course> courses)
        {
            var provider = courses.Where(x => x.ProviderLocation != null).Select(x => x.ProviderLocation);
            var contactDetailsAddresses = courses.Where(x => x.ContactDetails?.Address != null).Select(x => new Location {Address = x.ContactDetails.Address});
            var campuses = courses.SelectMany(x => x.Campuses).Where(x => !x.Name.Equals("Main site", StringComparison.InvariantCultureIgnoreCase)).Select(x => new Location {Address = $"{x.Name}, {x.Location.Address}"});

            var locations = new List<Location>();
            locations.AddRange(provider);
            locations.AddRange(contactDetailsAddresses);
            locations.AddRange(campuses);

            var expectedLocation = new List<Location>();
            foreach(var l in locations) {
                if(!expectedLocation.Any(x => x.Address == l.Address)) {
                    expectedLocation.Add(new Location {Address = l.Address, GeoAddress = l.Address});
                }
            }
            return expectedLocation;
        }

        internal static List<Campus> GetCampuses()
        {
            var campuses = new List<Campus>{
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
                        },
                        new Campus
                        {
                            CampusCode = "C",
                            Name = "CampusC",
                            Location = new Location
                            {
                                Address = "Common location"
                            }
                        },
                        new Campus
                        {
                            CampusCode = "D",
                            Name = "Main site",
                            Location = new Location
                            {
                                Address = "Common location"
                            }
                        }
                    };
                return campuses;
        }

        internal static List<Course> GetCourses(int count)
        {
            var courses = new List<Course>();

            for (int i = 0; i < count; i++)
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
                    Campuses = new Collection<Campus>(GetCampuses()),

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

                    IncludesPgce = IncludesPgce.QtsOnly,

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

                courses.Add(course);
            }

            return courses;
        }
    }
}