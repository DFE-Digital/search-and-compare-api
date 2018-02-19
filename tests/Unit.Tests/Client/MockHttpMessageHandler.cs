using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Domain.Unit.Tests.Client
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage Response { get; set; }

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            Response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Response);
        }
    }
}