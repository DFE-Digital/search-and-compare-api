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
using FluentAssertions;

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
        public void Constructor_Exception()
        {
            Action act = () => new HttpClientWrapper(null);
            var msg = $"Failed to instantiate due to HttpClient = null";
            act.Should().Throw<SearchAndCompareApiException>().WithMessage(msg);
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

        [Test]
        public void SendAsync_SearchAndCompareApiException()
        {
            var ub = new UriBuilder("test");
            var uri = ub.Uri;
            var sc = new StringContent("tested");

            mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => throw new Exception());

            // Func<Task<HttpResponseMessage>> func = async () => await sut.PostAsync(uri, sc);
            Func<Task> act = async () => await sut.PostAsync(uri, sc);

            var msg = $"API POST Failed uri {uri}";
            act.Should().Throw<SearchAndCompareApiException>().WithMessage(msg);
        }

        [Test]
        public void GetAsync_SearchAndCompareApiException()
        {
            var ub = new UriBuilder("test");
            var uri = ub.Uri;

            mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => throw new Exception());

            Func<Task> act = async () => await sut.GetAsync(uri);

            var msg = $"API GET Failed uri {uri}";
            act.Should().Throw<SearchAndCompareApiException>().WithMessage(msg);
        }

        [Test]
        public void PutAsync_SearchAndCompareApiException()
        {
            var ub = new UriBuilder("test");
            var uri = ub.Uri;
            var sc = new StringContent("tested");

            mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => throw new Exception());

            Func<Task> act = async () => await sut.PutAsync(uri, sc);

            var msg = $"API Put Failed uri {uri}";
            act.Should().Throw<SearchAndCompareApiException>().WithMessage(msg);
        }
    }
}