using System.Net.Http;
using GovUk.Education.SearchAndCompare.Domain.Client;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration
{
    [TestFixture]
    [Category("SacApi")]
    [Explicit]
    public class SearchAndCompareApiTests
    {
        [Test]
        public void Test1()
        {            
            var api = new SearchAndCompareApi(new HttpClient(), "http://search-and-compare-api-bat-development.e4ff.pro-eu-west-1.openshiftapps.com/api/");
            var res = api.GetProviderSuggestions("yorks");
            Assert.That(res.Count > 0);
        }

    }
}
