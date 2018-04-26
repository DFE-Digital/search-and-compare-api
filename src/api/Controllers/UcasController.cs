using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    /// <summary>
    /// All dealings with the ucas website
    /// </summary>
    [Route("api/[controller]")]
    public class UcasController : Controller
    {
        private readonly ICourseDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly UcasSettings _ucasSettings;

        public UcasController(UcasSettings ucasSettings, ICourseDbContext courseDbContext, HttpClient httpClient)
        {
            _ucasSettings = ucasSettings;
            _context = courseDbContext;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Get a valid sessionId from the ucas site and then return a url for the course.
        /// The url will only be valid while the session is valid! (5 mins at last test)
        /// </summary>
        [HttpGet("course-url")]
        public async Task<IActionResult> GetUcasCourseUrl(int courseId)
        {
            var sessionId = await GetSessionId();

            var course =  await _context.GetCourseWithProviderSubjectsRouteCampusesAndDescriptions(courseId);

            if (course == null){
                return NotFound();
            }

            var courseUrl = GenerateCourseUrl(course, sessionId);

            return Ok(courseUrl);
        }

        // extract to its own class
        public string GenerateCourseUrl(Course course, string sessionId)
        {
            var inst = course.Provider.ProviderCode;
            var courseCode = course.ProgrammeCode;
            const string modifier = "";
            // e.g. "http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/StateId/FtZTMVzsnznHD12F9RlkEsGMDpCWs-VuyG/HAHTpage/gttr_search.HsProfile.run?inst=295&course=37T6&mod="
            var url = string.Format(_ucasSettings.GenerateCourseUrlFormat, inst, courseCode, modifier, sessionId);

            return url;
        }

        private async Task<string> GetSessionId()
        {
            var searchStartUrl = _ucasSettings.SearchStartUrl;
            var response = await _httpClient.GetAsync(searchStartUrl);

            if (!response.IsSuccessStatusCode)
            {
                // todo: improve handling
                throw new Exception("ucas request failed");
            }

            const string contentTypeHeader = "Content-Type";
            if (!response.Content.Headers.Contains(contentTypeHeader))
            {
                // todo: be more tolerant
                throw new Exception("content type header missing in ucas site response");
            }
            var contentType = response.Content.Headers.FirstOrDefault(header => header.Key == contentTypeHeader).Value.FirstOrDefault();
            var ucasBytes = await response.Content.ReadAsByteArrayAsync();

            var enc1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252);
            var ucasHtml = enc1252.GetString(ucasBytes);
            var stateId = ExtractStateId(ucasHtml);

            return stateId;
        }

        // Extract to its own class
        public string ExtractStateId(string ucasHtml)
        {
            var matchCollection = Regex.Matches(ucasHtml, _ucasSettings.ExtractStateIdRegex);

            var stateId = matchCollection.First().Groups.Skip(1).First().Captures.First().Value;

            return stateId;
        }
    }
}
