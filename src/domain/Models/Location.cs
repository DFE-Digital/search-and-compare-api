using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("location")]
    public class Location
    {
        public Location()
        {
            LastGeocodedUtc = DateTime.MinValue;
        }

        public int Id { get; set; }

        public string Address { get; set; }

        public string FormattedAddress { get; set; }
        public string GeoAddress { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime LastGeocodedUtc { get; set; }
    }
}