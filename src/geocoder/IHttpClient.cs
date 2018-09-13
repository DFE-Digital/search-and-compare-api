using System.Net.Http;
using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Geocoder
{
    /// <summary>
    ///    Wraps System.Net.Http.HttpClient to make it testable
    /// </summary>
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
    }
}
