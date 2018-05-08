using System.Net.Http;
using GovUk.Education.SearchAndCompare.Domain.Client;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using NUnit.Framework;

namespace GovUk.Education.SearchAndCompare.Api.Tests.Integration.Tests
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

        [Test]
        public void Test2()
        {            
            var api = new SearchAndCompareApi(new HttpClient(), "http://search-and-compare-api-bat-development.e4ff.pro-eu-west-1.openshiftapps.com/api/");
            var res = api.GetCourses(new QueryFilter{
                rad=5,
                lat=52.205337,
                lng=0.121817
            });
            Assert.That(res.Count > 0);
        }

    }
}
