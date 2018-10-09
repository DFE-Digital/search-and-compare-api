using System;
using GovUk.Education.SearchAndCompare.Api;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.SearchAndCompare.Geocoder
{
    public class LocationRequesterConfiguration
    {
        public string ApiKey { get; }
        public int BatchSize { get; }

        public LocationRequesterConfiguration(string apiKey, int batchSize)
        {
            ApiKey = apiKey;
            BatchSize = batchSize;
        }

        public static LocationRequesterConfiguration FromConfiguration(IConfiguration configuration)
        {
            return new LocationRequesterConfiguration(
                configuration["google_cloud_platform_key"],
                int.TryParse(configuration["geocoder_batch_size"], out int batchSize) ? batchSize : 10 
            );
        }
    }
}