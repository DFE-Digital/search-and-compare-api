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
                var allCourses = context2.Courses.FromSql("SELECT *, NULL as \"Distance\", NULL as \"DistanceAddress\" FROM \"course\"");

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
                Assert.GreaterOrEqual(distance.Value, 713);
                Assert.LessOrEqual(distance.Value, 714);
            }
        }

        [Test]
        public void GetLocationFilteredCourses_GiveCorrectDistanceAddress()
        {
            Assert.AreEqual(0, context.Courses.Count());
            
            var persistedCourse = GetSimpleCourse();
            
            persistedCourse.ProviderLocation = new Location {
                Address = "London",
                Latitude = 50,
                Longitude = 0
            };

            persistedCourse.Campuses.Single().Location = new Location {
                Address = "Edinburgh",
                Latitude = 56.0,
                Longitude = -3.2
            };

            entitiesToCleanUp.Add(context.Courses.Add(persistedCourse));
            context.SaveChanges();            
            

            using (var context2 = GetContext())
            {
                Assert.AreEqual("London", context2.GetLocationFilteredCourses(50, 0, 100).Single().DistanceAddress, "Filtered to (roughly) London, DistanceAddress should be London");
            }
            using (var context3 = GetContext())
            {
                Assert.AreEqual("Edinburgh", context3.GetLocationFilteredCourses(56.0, -3.2, 100).Single().DistanceAddress, "Filtered to (roughly) Edinburgh, DistanceAddress should be Edinburgh");
            }            
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
                Assert.AreEqual(2, context2.SuggestProviders(@"provider my\").Count(), "multiwords good");
                Assert.AreEqual(2, context2.SuggestProviders(@"prov my\").Count(), "multiwords good incomplete");
            }
        }

        [Test]
        [TestCase("xyz", 1)]
        [TestCase("wxy", 1)]
        [TestCase("xxx", 0)]
        [TestCase("xy", 1)]
        [TestCase("wx", 1)]
        [TestCase("x", 1)]
        [TestCase("w", 1)]
        [TestCase("y", 0)]
        public void ProviderSuggest_ProviderCode(string query, int expectedCount)
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetSimpleCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                Assert.AreEqual(expectedCount, context2.SuggestProviders(query).Count(), "complete");
            }
        }

        [Test]
        public void ProviderSuggest_ExactMatchesPreferred()
        {
            Assert.AreEqual(0, context.Courses.Count());            

            var course1 = GetSimpleCourse();
            var course2 = GetSimpleCourse();
            var course3 = GetSimpleCourse();

            var provider1 = new Provider { Name = "Teach"};
            var provider2 = new Provider { Name = "Teach2"};

            course1.Provider = provider1;
            course2.Provider = provider2;
            course3.Provider = provider2;

            entitiesToCleanUp.Add(context.Courses.Add(course1));
            entitiesToCleanUp.Add(context.Courses.Add(course2));
            entitiesToCleanUp.Add(context.Courses.Add(course3));

            context.SaveChanges();


            using (var context2 = GetContext())
            {
                Assert.AreEqual("Teach2", context.SuggestProviders("tea").First().Name, "should be sorted by course count by default");
                Assert.AreEqual("Teach", context.SuggestProviders("teach").First().Name, "should prefer exact matches");
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

        [Test]
        public void GetSubjects_OmitEmptySubjects()
        {
            var entity1 = context.Courses.Add(GetSimpleCourse());
            var entity2 = context.Subjects.Add(new Subject { Name = "Other subject", SubjectArea = new SubjectArea { Name = "Other subject area" }});

            context.SaveChanges();

            entitiesToCleanUp.Add(entity1);
            entitiesToCleanUp.Add(entity2);

            using (var context2 = GetContext())
            {
                var subjects = context2.GetSubjects();
                subjects.Count().Should().Be(1);
                subjects.First().Name.Should().Be("My subject");

                var subjectAreas = context2.GetOrderedSubjectsByArea();
                subjectAreas.Count().Should().Be(1);
                subjectAreas.First().Name.Should().Be("My subject area");
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
                    new CourseSubject { Subject = new Subject {Name = "My subject", SubjectArea = new SubjectArea { Name = "My subject area" } } }
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
                    new CourseDescriptionSection { Name = "Title", Text = "Content" }
                }
            };
        }
    }
}
