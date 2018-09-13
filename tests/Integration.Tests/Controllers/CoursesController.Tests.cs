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

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.Controllers
{
    [TestFixture]
    public class CoursesControllerTests : CourseDbContextIntegrationBase
    {
        CoursesController subject;

        [SetUp]
        public void Setup()
        {
            subject = new CoursesController(context);
        }

        [Test]
        public void ImportNullCourse()
        {
            var result = subject.Index(null);

            AssertBad(result);
        }

        [Test]
        public void ImportOneCourse()
        {
            var courses = GetCourses(1);
            var result = subject.Index(courses);

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
            Assert.AreEqual(1, resultingProviders.Count);
            Assert.AreEqual(1, context.Routes.Count());
            Assert.AreEqual(1, context.Subjects.Count());
            Assert.AreEqual(1, context.Locations.Count());

            // non-deduplicated
            Assert.AreEqual(2, resultingCourses.Count);
            Assert.AreEqual(2, resultingCourses.SelectMany(x => x.DescriptionSections).Distinct().Count());
            Assert.AreEqual(4, context.Campuses.Count());
            Assert.AreEqual(2, context.CourseSubjects.Count());
            Assert.AreEqual(2, context.Contacts.Count());

            Assert.True(object.ReferenceEquals(resultingCourses[0].Provider, resultingCourses[1].Provider));
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
            Assert.AreEqual(1, resultingProviders.Count);
            Assert.True(object.ReferenceEquals(resultingCourses[0].Provider, resultingCourses[1].Provider));
            Assert.AreEqual(1, context.Routes.Count());
            Assert.AreEqual(1, context.Subjects.Count());
            Assert.AreEqual(1, context.Locations.Count());

            // non-deduplicated
            Assert.AreEqual(2, resultingCourses.Count);
            Assert.AreEqual(2, resultingCourses.SelectMany(x => x.DescriptionSections).Distinct().Count());
            Assert.AreEqual(4, context.Campuses.Count());
            Assert.AreEqual(2, context.CourseSubjects.Count());
            Assert.AreEqual(2, context.Contacts.Count());
        }

        [Test]
        public void GeocodingIsPreservedAcrossImports()
        {
            // initial import
            var result = subject.Index(GetCourses(1));

            AssertOkay(result);

            Assert.AreNotEqual(51.0, context.Locations.Single().Latitude);
            Assert.AreNotEqual(13.7, context.Locations.Single().Longitude);

            // set some coordinates
            context.Locations.Single().Latitude = 51.0;
            context.Locations.Single().Longitude = 13.7;
            context.Save();

            // second import, with equivalent addresses
            subject.Index(GetCourses(1));

            // coordinates previously set are still there
            Assert.AreEqual(51.0, context.Locations.Single().Latitude);
            Assert.AreEqual(13.7, context.Locations.Single().Longitude);
        }

        private void AssertBad(IActionResult result)
        {
            AssertStatusCode(result, 400);
        }

        private void AssertOkay(IActionResult result)
        {
            AssertStatusCode(result, 200);
        }

        private void AssertStatusCode(IActionResult result, int statusCode)
        {
            Assert.IsNotNull(result);

            var statusCodeResult  = result as StatusCodeResult;

            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(statusCode, statusCodeResult.StatusCode);
        }

        private List<Course> GetCourses(int count)
        {
            var courses = new List<Course>();

            for (int i = 0; i < count; i++)
            {
                var course = new Course
                {
                    Name = "Name" + i,
                    ProviderId = 42,
                    Provider = new Provider
                    {
                        Id = 24 + i,
                        Name = "Name",
                        ProviderCode = "ProviderCode"
                    },

                    AccreditingProvider = new Provider
                    {
                        Id = 124 + i,
                        Name = "Name (accrediting)",
                        ProviderCode = "ProviderCode"
                    },

                    AgeRange = AgeRange.Secondary,
                    Route = new Route
                    {
                        Name = "Scitt",
                        IsSalaried = true
                    },

                    DescriptionSections = new Collection<CourseDescriptionSection>
                    {
                        new CourseDescriptionSection
                        {
                            Name = "section",
                            Text = "Section text"
                        }
                    },

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
                    Fees = new Fees(),
                    Salary = new Salary(),
                    ContactDetails = new Contact(),

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