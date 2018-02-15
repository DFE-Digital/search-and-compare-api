using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ICourseDbContext _context;

        public SubjectController(ICourseDbContext courseDbContext)
        {
            _context = courseDbContext;
        }

        [HttpGet("subjects")]
        public IActionResult Index(ResultsFilter filter)
        {
            var filteredSubjects = _context.GetSubjects().ToFilteredList<Subject>(
                subject => filter.SelectedSubjects.Contains(subject.Id));

            return View(filteredSubjects);
        }

        [HttpGet("subjectAreas")]
        public IActionResult SubjectAreas()
        {
            var subjectAreas = _context.GetOrderedSubjectsByArea();

            return View(subjectAreas);
        }
    }
}