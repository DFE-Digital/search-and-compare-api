using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;

namespace GovUk.Education.SearchAndCompare.Geocoder
{
    public class LocationRequester
    {
        private readonly LocationRequesterConfiguration _config;
        private readonly ILogger _logger;
        private readonly IHttpClient _httpClient;
        private readonly ICourseDbContext _context;

        public LocationRequester(LocationRequesterConfiguration config, ILogger logger, IHttpClient httpClient, ICourseDbContext context)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
            this._context = context;
        }

        public async Task RequestLocations()
        {
            var locations = _context.Locations
                .Where(x => x.Latitude == null) // choose un-geocoded locations...
                .OrderBy(x => x.LastGeocodedUtc) // ... preferring those that we haven't attempted recently
                .Take(_config.BatchSize)
                .ToList();

            var locationQueries = new Dictionary<int, Task<Coordinates>>();
            var geocoder = new TECH_DEBT__TemporarilyCopied__Geocoder(_config.ApiKey, _httpClient);
            var utcNow = DateTime.UtcNow;

            foreach (var location in locations)
            {
                locationQueries.Add(
                    location.Id,
                    geocoder.ResolvePostCodeAsync(location.GeoAddress)
                );
            }

            foreach (var location in locations)
            {
                location.LastGeocodedUtc = utcNow;
                var coordinates = await locationQueries[location.Id];
                if (coordinates != null)
                {
                    location.FormattedAddress = coordinates.FormattedLocation;
                    location.Longitude = coordinates.Longitude;
                    location.Latitude = coordinates.Latitude;
                }
                else
                {
                    _logger.Information($"Unable to resolve address: {location.Id}, {location.GeoAddress}");
                }
            }

            _context.SaveChanges();
        }
    }
}
