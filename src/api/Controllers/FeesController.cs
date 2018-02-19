using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Api.ListExtensions;
using GovUk.Education.SearchAndCompare.Domain.Filters;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class FeesController : Controller
    {
        private readonly ICourseDbContext _context;

        public FeesController(ICourseDbContext courseDbContext)
        {
            _context = courseDbContext;
        }

        // GET api/fees
        [HttpGet]
        public IActionResult GetAll()
        {
            var fees = _context.GetLatestFees();

            return Ok(fees);
        }
    }
}