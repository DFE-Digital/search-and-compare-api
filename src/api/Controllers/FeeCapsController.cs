using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class FeeCapsController : Controller
    {
        private readonly ICourseDbContext _context;

        public FeeCapsController(ICourseDbContext courseDbContext)
        {
            _context = courseDbContext;
        }

        // GET api/fees
        [HttpGet]
        public IActionResult GetAll()
        {
            var feeCaps = _context.GetFeeCaps();

            return Ok(feeCaps);
        }
    }
}