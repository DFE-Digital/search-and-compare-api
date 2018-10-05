using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.DatabaseAccess
{
    [TestFixture]
    [Category("Integration")]
    [Category("Integration_DB")]
    [Explicit]
    public class CourseDbContextIntegrationTests : CourseDbContextIntegrationBase
    {
        [Test]
        public void InsertCourse()
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetSimpleCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                var allCourses = context2.Courses.FromSql("SELECT *, NULL as \"Distance\" FROM \"course\"");

                Assert.AreEqual(1, allCourses.Count());
                Assert.AreEqual(GetSimpleCourse().Name, allCourses.First().Name);
            }

        }

        [Test]
        public void GetLocationFilteredCourses()
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetSimpleCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                Assert.AreEqual(1, context2.GetLocationFilteredCourses(50, 0, 1000).Count(), "Filtered to (roughly) London, the course should be found");
                Assert.AreEqual(0, context2.GetLocationFilteredCourses(56.0, -3.2, 1000).Count(), "Filtered to (roughly) Edinburgh, the course shouldn't be found");

                var distance = context2.GetLocationFilteredCourses(50, 0.01, 1000).Single().Distance;
                Assert.NotNull(distance);
                Assert.LessOrEqual(715, distance.Value);
                Assert.GreaterOrEqual(716, distance.Value);
            }
        }

        [Test]
        public void LocationFiltering()
        {
            context.Locations.Count().Should().Be(0, "database should be clean before test runs");
            var emptyDbResult = context.LocationsInRadius(1, 2, 3);
            emptyDbResult.Count().Should().Be(0, "There are no locations");
            context.Locations.AddRange(
            new List<Location>
            {
                new Location { Address = "near", Latitude = 51.5073509, Longitude = -0.1277583}, // centre of london
                new Location { Address = "far", Latitude = 51.4996109, Longitude = -0.1310529}, // ~800m away
                new Location { Address = "where-ever you are", Latitude = 52.2213067, Longitude = -0.2578839}, // ~80km away
            });
            context.SaveChanges();
            var singleMatch = context.LocationsInRadius(51.5073509, -0.1277583, 100); // centre of london
            singleMatch.Count().Should().Be(1, "searched ten metres around location");
            var match = singleMatch.Single();
            match.Distance.Should().Be(4);
            match.Location.Address.Should().Be("near", "related entity should be loaded");
            var doubleMatch = context.LocationsInRadius(51.5073509, -0.1277583, 10000); // centre of london
            doubleMatch.Count().Should().Be(2, "searched ten km around location");
        }

        [Test]
        public void InsertCourseNeedsProvider()
        {
            var c = GetSimpleCourse();
            c.Provider = null;
            context.Courses.Add(c);

            Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        }

        [Test]
        public void InsertCourseNeedsRoute()
        {
            var c = GetSimpleCourse();
            c.Route = null;
            context.Courses.Add(c);

            Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        }

        [Test]
        public void TextSearch()
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetSimpleCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                Assert.AreEqual(1, context2.GetTextFilteredCourses("My provider").Count(), "exact match");
                Assert.AreEqual(1, context2.GetTextFilteredCourses("my Provider").Count(), "exact match, case insensitive");
                Assert.AreEqual(0, context2.GetTextFilteredCourses("provider").Count(), "partial match, don't return");
            }
        }

        [Test]
        public void TextSearch_NullAndEmpty()
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetSimpleCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                Assert.Throws(typeof(ArgumentException), () => context2.GetTextFilteredCourses(""), "Empty");
                Assert.Throws(typeof(ArgumentException), () => context2.GetTextFilteredCourses("  "), "Whitespace");
                Assert.Throws(typeof(ArgumentException), () => context2.GetTextFilteredCourses(null), "Null");
                Assert.Throws(typeof(ArgumentException), () => context2.GetTextAndLocationFilteredCourses("", 50, 0, 1000), "Empty (with location)");
                Assert.Throws(typeof(ArgumentException), () => context2.GetTextAndLocationFilteredCourses("  ", 50, 0, 1000), "Whitespace (with location)");
                Assert.Throws(typeof(ArgumentException), () => context2.GetTextAndLocationFilteredCourses(null, 50, 0, 1000), "Null (with location)");
            }
        }

        [Test]
        public void ProviderSuggest()
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetSimpleCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                Assert.AreEqual(2, context2.SuggestProviders("prov").Count(), "incomplete");
                Assert.AreEqual(2, context2.SuggestProviders("provider").Count(), "complete");
                Assert.AreEqual(2, context2.SuggestProviders("providers").Count(), "plural");
                Assert.AreEqual(2, context2.SuggestProviders("provider' &!").Count(), "garbage");
                Assert.AreEqual(0, context2.SuggestProviders("providerrr").Count(), "overshoot");
                Assert.AreEqual(0, context2.SuggestProviders("provider foobar").Count(), "multiwords bad");
                Assert.AreEqual(2, context2.SuggestProviders("provider my").Count(), "multiwords good");
                Assert.AreEqual(2, context2.SuggestProviders("prov my").Count(), "multiwords good incomplete");
            }
        }

        [Test]
        public void RetrieveByIdCaseInsenstive()
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetSimpleCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                var course = context2.GetCourseWithProviderSubjectsRouteCampusesAndDescriptions("xYz", "1aB").Result;
                Assert.AreEqual("1AB", course.ProgrammeCode, "course code is retrieved");
                Assert.AreEqual("My first course", course.Name, "course name is retrieved");

                Assert.AreEqual("My Campus", course.Campuses.Single().Name, "campus data is retrieved");
                Assert.AreEqual("SCITT", course.Route.Name, "route data is retrieved");
                Assert.AreEqual("My subject", course.CourseSubjects.Single().Subject.Name, "subject data is retrieved");
                Assert.AreEqual("My Campus", course.Campuses.Single().Name, "campus data is retrieved");

                Assert.AreEqual("Title", course.DescriptionSections.Single().Name, "description title is retrieved");
                Assert.AreEqual("Content", course.DescriptionSections.Single().Text, "description content is retrieved");

                Assert.AreEqual("My accrediting provider", course.AccreditingProvider.Name, "accrediting provider name is retrieved");
                Assert.AreEqual("WXY", course.AccreditingProvider.ProviderCode, "accrediting provider code is retrieved");
            }
        }

        private static Course GetSimpleCourse()
        {
            return new Course()
            {
                Name = "My first course",

                ProgrammeCode = "1AB",

                Provider = new Provider
                {
                    ProviderCode = "XYZ",
                    Name = "My provider"
                },
                ProviderLocation = new Location
                {
                    Address = "123 Fake Street",
                    Latitude = 50.0,
                    Longitude = 0
                },
                AccreditingProvider = new Provider
                {
                    ProviderCode = "WXY",
                    Name = "My accrediting provider"
                },
                Campuses = new HashSet<Campus>
                {
                    new Campus { Name = "My Campus" }
                },
                CourseSubjects = new HashSet<CourseSubject>
                {
                    new CourseSubject { Subject = new Subject {Name = "My subject"} }
                },
                Route = new Route
                {
                    Name = "SCITT"
                },
                IsSalaried = false,
                Fees = new Fees { Eu = 9250, Uk = 9250, International = 16340 },
                Salary = new Salary(),
                DescriptionSections = new HashSet<CourseDescriptionSection>
                {
                    new CourseDescriptionSection { Id = 1, Name = "Title", Text = "Content" }
                }
            };
        }
    }
}
