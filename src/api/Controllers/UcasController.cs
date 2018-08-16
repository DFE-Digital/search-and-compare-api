using System.Threading.Tasks;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.Ucas;
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
        private readonly IUcasUrlBuilder _ucasUrlBuilder;

        public UcasController(ICourseDbContext courseDbContext, IUcasUrlBuilder ucasUrlBuilder)
        {
            _context = courseDbContext;
            _ucasUrlBuilder = ucasUrlBuilder;
        }

        /// <summary>
        /// Get a valid sessionId from the ucas site and then return a url for the course.
        /// The url will only be valid while the session is valid! (5 mins at last test)
        /// </summary>
        [HttpGet("course-url/{courseCode:string}")]
        public async Task<IActionResult> GetUcasCourseUrl(string courseCode)
        {
            var course =  await _context.GetCourseWithProviderSubjectsRouteCampusesAndDescriptions(courseCode);

            if (course == null){
                return NotFound();
            }

            const string modifier = ""; // todo: modifier support - https://dfedigital.atlassian.net/browse/BATSA-257

            string courseUrl = await _ucasUrlBuilder.GenerateCourseUrl(course.Provider.ProviderCode, course.ProgrammeCode, modifier);

            return Ok(new {courseUrl});
        }
    }
}
