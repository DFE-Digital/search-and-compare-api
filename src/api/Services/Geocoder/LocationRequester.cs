using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog;
using Serilog.Core;

namespace GovUk.Education.SearchAndCompare.Geocoder
{
    public class LocationRequester : ILocationRequester
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


        private List<Location> GetLocationsToGeocode()
        {
            var locations = _context.Locations
                .Where(x => (x.Latitude == null || x.Longitude == null) || x.LastGeocodedUtc == DateTime.MinValue)
                .OrderBy(x => x.LastGeocodedUtc)
                .Take(_config.BatchSize)
                .ToList();

            return locations;
        }

        private int GetTotalLocationsToGeocode()
        {
            var locations = _context.Locations
                .Where(x => (x.Latitude == null || x.Longitude == null) || x.LastGeocodedUtc == DateTime.MinValue)
                .OrderBy(x => x.LastGeocodedUtc);

            return locations.Count();
        }

        public async Task<int> RequestLocations()
        {
            var locations = GetLocationsToGeocode();
            var totalLocationsToGeocode = GetTotalLocationsToGeocode();
            var geocoder = new TECH_DEBT__TemporarilyCopied__Geocoder(_config.ApiKey, _httpClient);
            var utcNow = DateTime.UtcNow;

            _logger.Information($"Geocode proccessing a total of : {locations.Count()}/{totalLocationsToGeocode}");

            var failures = new Dictionary<string, string>();
            foreach (var location in locations)
            {
                // The try catch needs to be here as geocoder consume external service
                try
                {
                    var coordinates = await geocoder.ResolvePostCodeAsync(location.GeoAddress);
                    if (coordinates != null)
                    {
                        location.FormattedAddress = coordinates.FormattedLocation;
                        location.Longitude = coordinates.Longitude;
                        location.Latitude = coordinates.Latitude;
                        location.LastGeocodedUtc = utcNow;
                    }
                    else
                    {
                        failures.Add(location.Id.ToString(), location.GeoAddress);
                    }
                }
                catch (Exception)
                {
                    failures.Add(location.Id.ToString(), location.GeoAddress);
                }
            }

            if(failures.Any())
            {

                foreach(var failure in failures)
                {
                    _logger.Warning($"Geocode unable to resolve: {failure.Key}, {failure.Value}");
                }

                var msg = $"Geocode failures a total of : {failures.Count()}.{locations.Count()}";
                _logger.Warning(msg);

                new TelemetryClient().TrackTrace($"Geocode failures a total of : {failures.Count()}.{locations.Count()}", SeverityLevel.Error, failures);
            }

            _context.SaveChanges();

            return failures.Count();
        }
    }
}
