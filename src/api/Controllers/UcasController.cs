using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    /// <summary>
    /// All dealings with the ucas website
    /// </summary>
    [Route("api/[controller]")]
    public class UcasController : Controller
    {
        /// <summary>
        /// Get a valid sessionId from the ucas site and then return a url for the course.
        /// The url will only be valid while the session is valid! (5 mins at last test)
        /// </summary>
        [HttpGet("course-url")]
        public async Task<IActionResult> GetUcasCourseUrl(string programmeCode, string providerCode)
        {
            // todo: grab sessionId, build actual url for course
            return Ok("http://search.gttr.ac.uk/cgi-bin/hsrun.hse/General/2018_gttr_search/StateId/FtZTMVzsnznHD12F9RlkEsGMDpCWs-VuyG/HAHTpage/gttr_search.HsProfile.run?inst=295&course=37T6&mod=");
        }
    }
}