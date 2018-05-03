using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GovUk.Education.SearchAndCompare.Api.Ucas;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Integration.Tests.UcasLink
{
    /// <summary>
    /// Set of tests for interacting with the live UCAS website,
    /// in increasing order of pedanticness.
    /// </summary>

    [Category("Integration")]
    [Category("Integration_Ucas")]
    [Explicit]
    [TestFixture]
    public class UcasUrlBuilderIntegrationTests
    {
        private UcasUrlBuilder _ucasUrlBuilder;

        [SetUp]
        public void SetUp()
        {
            _ucasUrlBuilder = new UcasUrlBuilder(new HttpClient());
        }

        [Test]
        public async Task GetSessionId_ReturnsSomething()
        {
            var sessionId = await _ucasUrlBuilder.GetSessionId();
            sessionId.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public async Task GetSessionId_LooksLikeItDidBefore()
        {
            const string exampleSessionId = "FtxLmNzhBaBHCN1U9RNvnbHODpDM_-Ud9V";
            // strict regex, provide example in error
            var sessionId = await _ucasUrlBuilder.GetSessionId();
            sessionId.Should().MatchRegex("[A-Za-z0-9_-]{29}-[A-Za-z0-9]{4}", "sessionIds we've seen before looked like this: " + exampleSessionId);
        }

        [Test]
        public async Task GeneratesUrl()
        {
            const string providerCode = "19E"; // aka institution
            const string programmeCode = "2YZZ";
            var url = await _ucasUrlBuilder.GenerateCourseUrl(providerCode, programmeCode, "");
            url.Should().StartWith("http://search.gttr.ac.uk/");
            url.Should().Contain("inst=" + providerCode);
            url.Should().Contain("course=" + programmeCode);
        }


        [Test]
        public async Task UcasReturnsHttpOkAtGeneratedUrl()
        {
            const string providerCode = "19E"; // aka institution
            const string programmeCode = "2YZZ";
            var url = await _ucasUrlBuilder.GenerateCourseUrl(providerCode, programmeCode, "");
            Console.Out.WriteLine(url);
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.IsSuccessStatusCode.Should()
                .BeTrue("Expected success HTTP status code, got " + response.StatusCode);
            var ucasBytes = await response.Content.ReadAsByteArrayAsync();
            var enc1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
            var body = enc1252.GetString(ucasBytes);
            body.Should().Contain("The 3 Rivers Teaching School Alliance");
            body.Should().Contain("Business Studies");
        }
    }
}
