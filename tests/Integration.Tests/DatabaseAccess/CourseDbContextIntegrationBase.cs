using System.Collections.Generic;
using System.IO;
using System.Linq;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests.DatabaseAccess
{
    public class CourseDbContextIntegrationBase
    {
        protected CourseDbContext context;

        protected IList<EntityEntry> entitiesToCleanUp = new List<EntityEntry>();

        protected CourseDbContext GetContext()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("integration-tests.json")
                .AddUserSecrets<CourseDbContextIntegrationBase>()
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
    }
}
