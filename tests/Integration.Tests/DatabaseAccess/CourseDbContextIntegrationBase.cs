using System;
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

            var connectionString = new EnvConfigConnectionStringBuilder().GetConnectionString(config);

            const int maxRetryCount = 1; // don't actually allow retry for tests
            const int maxRetryDelaySeconds = 1;

            var postgresErrorCodesToConsiderTransient = new List<string>(); // ref: https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL/blob/16c8d07368cb92e10010b646098b562ecd5815d6/src/EFCore.PG/NpgsqlRetryingExecutionStrategy.cs#L99

            // Configured to be similar to context setup in real app
            // Importantly the retry is enabled as that is what the production code uses and that is incompatible with the normal transaction pattern.
            // This will allow us to catch any re-introduction of following error before the code ships: "The configured execution strategy 'NpgsqlRetryingExecutionStrategy' does not support user initiated transactions. Use the execution strategy returned by 'DbContext.Database.CreateExecutionStrategy()' to execute all the operations in the transaction as a retriable unit."
            var options = new DbContextOptionsBuilder<CourseDbContext>()
                .UseNpgsql(connectionString, b =>
                            {
                                b.MigrationsAssembly((typeof(CourseDbContext).Assembly).ToString());
                                b.EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(maxRetryDelaySeconds), postgresErrorCodesToConsiderTransient);
                            })
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
