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

        public UcasController(ICourseDbContext courseDbContext, HttpClient httpClient)
        {
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
            var sessionId = GetSessionId();
            // todo: include provider
            var course = _context.Courses.FindAsync(courseId).Result;
            if (course == null){
                return NotFound();
            }

            var courseUrl = GenerateCourseUrl(course, sessionId);
            return Ok(courseUrl);
        }

        private object GenerateCourseUrl(Course course, string sessionId)
        {
            var inst = course.Provider.ProviderCode;
            var courseCode = course.ProgrammeCode;
            const string modifier = "";
            // e.g. "http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/StateId/FtZTMVzsnznHD12F9RlkEsGMDpCWs-VuyG/HAHTpage/gttr_search.HsProfile.run?inst=295&course=37T6&mod="
            var url = string.Format($"http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/StateId/{sessionId}/HAHTpage/gttr_search.HsProfile.run?inst={inst}&course={courseCode}&mod={modifier}");
            return Ok(url);
        }

        private string GetSessionId()
        {
            const string searchStartUrl = "http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/gttr_search.hjx;start=gttr_search.HsForm.run";
            var response = _httpClient.GetAsync(searchStartUrl).Result;
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
            var ucasBytes = response.Content.ReadAsByteArrayAsync().Result;
            var ucasHtml = Encoding.GetEncoding(1252).GetString(ucasBytes);
            var matchCollection = Regex.Matches(ucasHtml, @"StateId\/([^\/]*)\/");
            var stateId = matchCollection.First().Groups.Skip(1).First().Captures.First().Value;
            return stateId;
        }
    }
}