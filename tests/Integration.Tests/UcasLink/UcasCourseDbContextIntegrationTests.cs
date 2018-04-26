using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using GovUk.Education.SearchAndCompare.Api.Controllers;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.UcasLink
{
    [TestFixture]
    [Category("Integration")]
    [Category("Integration_Ucas")]
    [Explicit]
    public class UcasCourseDbContextIntegrationTests
    {
        // CourseDbContextIntegrationTests extract to base class [start]
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
        // CourseDbContextIntegrationTests extract to base class [end]


        [Test]
        public void InsertCourse()
        {
            Assert.AreEqual(0, context.Courses.Count());

            var entity = context.Courses.Add(GetMinimalCourse());
            context.SaveChanges();
            entitiesToCleanUp.Add(entity);

            using (var context2 = GetContext())
            {
                var allCourses = context2.Courses.FromSql("SELECT *, NULL as \"Distance\" FROM \"course\"");

                Assert.AreEqual(1, allCourses.Count());
                Assert.AreEqual(GetMinimalCourse().Name, allCourses.First().Name);
            }
        }

        [Test]
        public void UcasController_GetUcasCourseUrl()
        {

            InsertCourse();

            var courseId = context.Courses.FromSql("SELECT *, NULL as \"Distance\" FROM \"course\"").First().Id;


            var subject = new UcasController(GetContext(), new HttpClient());

            var actual = subject.GetUcasCourseUrl(courseId).Result as OkObjectResult;

            Assert.True(actual.StatusCode == 200);

            var expectedCourse = GetMinimalCourse();
            var url = actual.Value as string;
            Assert.NotNull(url);

            StringAssert.Contains(expectedCourse.ProgrammeCode, url);
            StringAssert.Contains(expectedCourse.Provider.ProviderCode, url);
        }


        private static Course GetMinimalCourse()
        {
            var campuses = new HashSet<Campus>
            {
                new Campus { Name = "My Campus" }

            };

            var courseSubjects = new HashSet<CourseSubject>
            {
                new CourseSubject { Subject = new Subject {Name = "My subject"} }
            };
            return new Course()
            {
                Name = "My minimal course",
                ProgrammeCode = "ProgrammeCode",
                Provider = new Provider
                {
                    Name = "My provider",
                    ProviderCode = "ProviderCode"
                },
                ProviderLocation = new Location
                {
                    Address = "123 Fake Street",
                    Latitude = 50.0,
                    Longitude = 0
                },
                Campuses = campuses,
                CourseSubjects = courseSubjects,
                Route = new Route
                {
                    Name = "SCITT"
                },
                IsSalaried = false,
                Fees = new Fees { Eu = 9250, Uk = 9250, International = 16340 },
                Salary = new Salary(),
                DescriptionSections = new HashSet<CourseDescriptionSection>
                    {
                        new CourseDescriptionSection { Name= "CourseDescriptionSection"}
                    }
            };
        }
    }
}
