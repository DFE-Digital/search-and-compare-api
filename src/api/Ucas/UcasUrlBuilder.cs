using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GovUk.Education.SearchAndCompare.Api.Ucas
{
    /// <summary>
    /// It seems the only way to deep-link to a course on the UCAS site is to have a valid sessionId embedded in the url.
    /// These expire after ~5 mins so have to be rengenerated every time.
    /// This class handles obtaining a current sessionId and constructing a valid url to a course.
    /// </summary>
    public class UcasUrlBuilder : IUcasUrlBuilder
    {
        private readonly HttpClient _httpClient;

        public UcasUrlBuilder(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get a url with a new sessionId from the UCAS website.
        /// Note this involves making a GET request to the UCAS site.
        /// </summary>
        public async Task<string> GenerateCourseUrl(string providerCode, string programmeCode, string modifier)
        {
            var sessionId = await GetSessionId();
            return GenerateCourseUrl(providerCode, programmeCode, modifier, sessionId);
        }

        /// <summary>
        /// Get a new sessionId from the UCAS website.
        /// Note this involves making a GET request to the UCAS site.
        /// </summary>
        public async Task<string> GetSessionId()
        {
            var ucasHtml = await GetUcasHtml();
            var stateId = ExtractStateId(ucasHtml);
            return stateId;
        }

        public static string ExtractStateId(string ucasHtml)
        {
            var matchCollection = Regex.Matches(ucasHtml, "StateId\\/([^\\/]*)\\/");
            var stateId = matchCollection.First().Groups.Skip(1).First().Captures.First().Value;
            return stateId;
        }

        private async Task<string> GetUcasHtml()
        {
            const string searchStartUrl =
                "http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/gttr_search.hjx;start=gttr_search.HsForm.run";
            var response = await _httpClient.GetAsync(searchStartUrl);

            if (!response.IsSuccessStatusCode)
            {
                // todo: improve handling
                throw new Exception("UCAS website GET request failed.");
            }

            /*
                // This commented out code would allow us to actually look at the content type in the response and respect it should we wish to.
                const string contentTypeHeader = "Content-Type";
                if (!response.Content.Headers.Contains(contentTypeHeader)) { throw new Exception("Content type header missing in UCAS site response"); }
                var contentType = response.Content.Headers.FirstOrDefault(header => header.Key == contentTypeHeader).Value.FirstOrDefault();
            */

            // We are assuming the response is Windows-1252 encoded because that's what we've seen when working on this.
            var ucasBytes = await response.Content.ReadAsByteArrayAsync();
            var enc1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
            var ucasHtml = enc1252.GetString(ucasBytes);
            return ucasHtml;
        }

        private static string GenerateCourseUrl(string providerCode, string programmeCode, string modifier, string sessionId)
        {
            var url = string.Format(
                "http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/StateId/{3}/HAHTpage/gttr_search.HsProfile.run?inst={0}&course={1}&mod={2}",
                providerCode, programmeCode, modifier, sessionId);
            return url;
        }
    }
}
