using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class SubjectAreasController : Controller
    {
        private readonly ICourseDbContext _context;

        public SubjectAreasController(ICourseDbContext courseDbContext)
        {
            _context = courseDbContext;
        }

        // GET api/subjectareas
        [HttpGet]
        public IActionResult GetAll()
        {
            var subjectAreas = _context.GetOrderedSubjectsByArea();

            return Ok(subjectAreas);
        }
    }
}