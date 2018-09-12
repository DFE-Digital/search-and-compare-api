using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using GovUk.Education.SearchAndCompare.Domain.Client;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Unit.Tests.Client
{
    [TestFixture]
    public class HttpClientWrapperTests
    {
        private HttpClientWrapper sut;
        private  Mock<HttpMessageHandler> mockMessageHandler;
        [SetUp]
        public void SetUp()
        {
            mockMessageHandler = new Mock<HttpMessageHandler>();
            sut = new HttpClientWrapper(new HttpClient(mockMessageHandler.Object));
        }

        [Test]
        public async Task PostAsync()
        {
            var ub = new UriBuilder("test");
            var uri = ub.Uri;
            var sc = new StringContent("tested");

            mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage());

            var result = await sut.PostAsync(uri, sc);
            mockMessageHandler.VerifyAll();

            Assert.Pass("Verified underlaying http client was called");
        }
    }
}