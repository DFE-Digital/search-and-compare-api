using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo
                .ApplicationInsightsTraces(configuration["APPINSIGHTS_INSTRUMENTATIONKEY"])
                .CreateLogger();

            logger.Information("Geocoder started.");

            var res = RequestLocations(configuration, logger).Result;

            logger.Information("Geocoder finished.");
        }

        private static async Task<int> RequestLocations(IConfiguration configuration, ILogger logger)
        {
            var geocoderBatchSize = 10;
            if (int.TryParse(configuration["geocoder_batch_size"], out int configuredBatchSize))
            {
                geocoderBatchSize = configuredBatchSize;
            }

            var options = new DbContextOptionsBuilder<CourseDbContext>()
                .UseNpgsql(new EnvConfigConnectionStringBuilder().GetConnectionString(configuration))
                .Options;

            var context = new CourseDbContext(options);

            var locations = context.Locations
                .Where(x => x.Latitude == null) // choose un-geocoded locations...
                .OrderBy(x => x.LastGeocodedUtc) // ... preferring those that we haven't attempted recently
                .Take(geocoderBatchSize)
                .ToList();

            var locationQueries = new Dictionary<int, Task<Coordinates>>();
            var geocoder = new TECH_DEBT__TemporarilyCopied__Geocoder(configuration["ApiKeys__GoogleMaps"]);
            var utcNow = DateTime.UtcNow;

            foreach (var location in locations)
            {
                locationQueries.Add(
                    location.Id,
                    geocoder.ResolvePostCodeAsync(location.Address)
                );
            }

            foreach (var location in locations)
            {
                location.LastGeocodedUtc = utcNow;
                var coordinates = await locationQueries[location.Id];
                if (coordinates != null)
                {
                    location.Address = coordinates.FormattedLocation;
                    location.Longitude = coordinates.Longitude;
                    location.Latitude = coordinates.Latitude;
                }
                else
                {
                    logger.Information($"Unable to resolve address: {location.Id}, {location.Address}");
                }
            }

            context.SaveChanges();
            return 0;
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
