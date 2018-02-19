using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class SubjectsController : Controller
    {
        private readonly ICourseDbContext _context;

        public SubjectsController(ICourseDbContext courseDbContext)
        {
            _context = courseDbContext;
        }

        // GET api/subjects
        [HttpGet]
        public IActionResult GetFiltered(QueryFilter filter)
        {
            var filteredSubjects = _context.GetSubjects().ToFilteredList<Subject>(
                subject => filter.SelectedSubjects.Contains(subject.Id));

            return Ok(filteredSubjects);
        }
    }
}