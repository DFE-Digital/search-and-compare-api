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
            if(wrapped == null)
            {
                throw new SearchAndCompareApiException($"Failed to instantiate due to HttpClient = null");
            }

            this.wrapped = wrapped;
        }

        public async Task<HttpResponseMessage> GetAsync(Uri queryUri)
        {
            try
            {
                return await wrapped.GetAsync(queryUri);
            }
            catch(Exception ex)
            {
                var msg = $"API GET Failed uri {queryUri}";
                throw new SearchAndCompareApiException(msg, ex);
            }
        }

        public async Task<HttpResponseMessage> PostAsync(Uri queryUri, StringContent content)
        {
            try
            {
                return await wrapped.PostAsync(queryUri, content);
            }
            catch(Exception ex)
            {
                var msg = $"API POST Failed uri {queryUri}";
                throw new SearchAndCompareApiException(msg, ex);
            }
        }

        public async Task<HttpResponseMessage> PutAsync(Uri queryUri, StringContent content)
        {
            try
            {
                return await wrapped.PutAsync(queryUri, content);
            }
            catch(Exception ex)
            {
                var msg = $"API PUT Failed uri {queryUri}";
                throw new SearchAndCompareApiException(msg, ex);
            }
        }
    }
}
