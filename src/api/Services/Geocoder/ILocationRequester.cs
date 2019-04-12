using System.Net.Http;
using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Geocoder
{
    public interface ILocationRequester
    {
         Task<int> RequestLocations();
    }
}
