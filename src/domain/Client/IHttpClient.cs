using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> GetAsync(Uri queryUri);
    }

    public class HttpClientWrapper : IHttpClient
    {
        private readonly HttpClient wrapped;

        public HttpClientWrapper(HttpClient wrapped)
        {
            this.wrapped = wrapped;
        }

        public Task<HttpResponseMessage> GetAsync(Uri queryUri)
        {
            return wrapped.GetAsync(queryUri);
        }
    }
}
