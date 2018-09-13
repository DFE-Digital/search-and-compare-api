using System;
using System.Collections.ObjectModel;
using System.IO;
using GovUk.Education.SearchAndCompare.Api;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace GovUk.Education.SearchAndCompare.Geocoder
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = GetConfiguration();
            var requesterConfig = LocationRequesterConfiguration.FromConfiguration(configuration);
            
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo
                .ApplicationInsightsTraces(configuration["APPINSIGHTS_INSTRUMENTATIONKEY"])
                .CreateLogger();

            var options = new DbContextOptionsBuilder<CourseDbContext>()
                .UseNpgsql(new EnvConfigConnectionStringBuilder().GetConnectionString(configuration))
                .Options;

            var context = new CourseDbContext(options);

            logger.Information("Geocoder started.");
            
            // Wait() because async Mains are not supported
            var locationRequester = new LocationRequester(requesterConfig, logger, new WrappedHttpClient(), context);
            locationRequester.RequestLocations().Wait();

            logger.Information("Geocoder finished.");
        }
        
        private static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
