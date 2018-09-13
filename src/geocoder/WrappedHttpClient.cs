using System.Net.Http;
using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Geocoder
{
    public class WrappedHttpClient : IHttpClient
    {
        HttpClient inner = new HttpClient();

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await inner.GetAsync(url);
        }
    }
}
