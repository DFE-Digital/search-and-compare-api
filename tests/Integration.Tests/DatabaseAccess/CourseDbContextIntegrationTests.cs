using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.DatabaseAccess 
{
    [TestFixture]
    [Category("Integration")]
    [Explicit]
    public class CourseDbContextIntegrationTests
    {
        private CourseDbContext context;

        private IList<EntityEntry> entitiesToCleanUp = new List<EntityEntry>();

        public CourseDbContext GetContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("integration-tests.json")
                .Build();            

            var options = new DbContextOptionsBuilder<CourseDbContext>()
                .UseNpgsql(new EnvConfigConnectionStringBuilder().GetConnectionString(config))
                .Options;                

            return new CourseDbContext(options);             
        }

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            context = GetContext();
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        [SetUp]
        public void SetUp()
        {
            context = GetContext();
        }

        [TearDown]
        public void TearDown()
        {
            if (entitiesToCleanUp.Any())
            {
                foreach (var e in entitiesToCleanUp)
                {
                    e.State = EntityState.Deleted;
                }
                entitiesToCleanUp.Clear();
                context.SaveChanges();
            }
        }

        [OneTimeTearDown]
        public void TearDownFixture()
        {
            context = GetContext();
            context.Database.EnsureDeleted();
        }

        [Test]
        public void EnsureCreated()
        {
            Assert.False(context.Database.EnsureCreated());
        }
        
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
                Assert.AreEqual(1, context2.GetTextFilteredCourses("Provider").Count(), "Filtered to a text string that exists, the course should be found");
                Assert.AreEqual(1, context2.GetTextFilteredCourses("My Provider").Count(), "Filter with multiple words");
                Assert.AreEqual(0, context2.GetTextFilteredCourses("FooBar").Count(), "Filtered to a text string that doensn't exist , the course should not be found");
                Assert.AreEqual(1, context2.GetTextAndLocationFilteredCourses("Provider", 50, 0, 1000).Count(), "Combining text search with location search should work");
                Assert.AreEqual(0, context2.GetTextAndLocationFilteredCourses("FooBar", 50, 0, 1000).Count(), "Combining bad text search with location search should work");
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
        
        private static Course GetSimpleCourse()
        {
            return new Course()
            {
                Name = "My first course",
                Provider = new Provider
                {
                    Name = "My provider"
                },
                ProviderLocation = new Location
                {
                    Address = "123 Fake Street",
                    Latitude = 50.0,
                    Longitude = 0
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
                Salary = new Salary()
            };
        }
    }
}