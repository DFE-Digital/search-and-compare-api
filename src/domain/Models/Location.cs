using System;
using System.Collections.Generic;
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

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public DateTime LastGeocodedUtc { get; set; }

        public ICollection<Course> Coursees { get; set; }

        public ICollection<Campus> Campuses { get; set; }

        /// <summary>
        /// How far from search location this is.
        /// To populate this use LocationsWithDistance() in the db context.
        /// </summary>
        public double? Distance { get; set; }
    }
}