using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Domain.Client
{
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

        public Task<HttpResponseMessage> PostAsync(Uri queryUri, StringContent content)
        {
            return wrapped.PostAsync(queryUri, content);
        }

        public Task<HttpResponseMessage> PutAsync(Uri queryUri, StringContent content)
        {
            return wrapped.PutAsync(queryUri, content);
        }
    }
}
