using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Api.Services
{
    /// <summary>
    /// Result of searching for any location by distance.
    /// Contains a mix of course and campus results.
    /// If it's a course match then campus is null.
    /// If it's a campus match then both are populated.
    /// The location distance is populated.
    /// </summary>
    public class LocationResult
    {
        public Location Location { get; set; }
        public Course Course { get; set; }
        public Campus Campus { get; set; }
    }
}
