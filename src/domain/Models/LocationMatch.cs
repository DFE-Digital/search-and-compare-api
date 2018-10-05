using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Api.DatabaseAccess
{
    public class LocationMatch
    {
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public double Distance { get; set; }
    }
}
