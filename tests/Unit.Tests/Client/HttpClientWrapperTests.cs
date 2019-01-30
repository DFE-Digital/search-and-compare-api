using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
            SetupMockMessageHandler(false, new HttpResponseMessage());

            var result = await sut.PostAsync(new UriBuilder("test").Uri, new StringContent("tested"));
            mockMessageHandler.VerifyAll();

            Assert.Pass("Verified underlaying http client was called");
        }

        [Test]
        public void SendAsync_SearchAndCompareApiException()
        {
            var uri = new UriBuilder("test").Uri;

            SetupMockMessageHandler(true, null);

            Func<Task> act = async () => await sut.PostAsync(uri, new StringContent("tested"));

            var msg = $"API POST Failed uri {uri}";
            act.Should().Throw<SearchAndCompareApiException>().WithMessage(msg);
        }

        [Test]
        public void GetAsync_SearchAndCompareApiException()
        {
            var uri = new UriBuilder("test").Uri;

            SetupMockMessageHandler(true, null);

            Func<Task> act = async () => await sut.GetAsync(uri);

            var msg = $"API GET Failed uri {uri}";
            act.Should().Throw<SearchAndCompareApiException>().WithMessage(msg);
        }

        [Test]
        public void GetAsync_SearchAndCompareApiEnsure404HasNoException()
        {
            SetupMockMessageHandler(false, new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

            Func<Task> act = async () => await sut.GetAsync(new UriBuilder("test").Uri);

            act.Should().NotThrow<SearchAndCompareApiException>();

            Assert.Pass("Verified underlaying http client was called with no exception");
        }

        [Test]
        public void GetAsync_SearchAndCompareApiEnsure404HappyPath()
        {
            SetupMockMessageHandler(false, new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound });

            var result = sut.GetAsync(new UriBuilder("test").Uri).Result;

            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public void PutAsync_SearchAndCompareApiException()
        {
            var uri = new UriBuilder("test").Uri;

            SetupMockMessageHandler(true, null);

            Func<Task> act = async () => await sut.PutAsync(uri, new StringContent("tested"));

            var msg = $"API Put Failed uri {uri}";
            act.Should().Throw<SearchAndCompareApiException>().WithMessage(msg);
        }

        private void SetupMockMessageHandler(bool throwException, HttpResponseMessage response)
        {
            mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() => throwException ? throw new Exception() : response);
        }
    }
}