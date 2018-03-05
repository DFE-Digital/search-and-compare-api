
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    [Route("api/[controller]")]
    public class ProviderController : Controller
    {
        private readonly ICourseDbContext _context;
        public ProviderController(ICourseDbContext courseDbContext)
        {
            _context = courseDbContext;
        }

        

        [HttpGet("suggest")]
        public JsonResult Suggest(string query)
        {
            return string.IsNullOrWhiteSpace(query)
                ? Json(new List<Provider>())
                : Json(_context.SuggestProviders(query));
        }
    }
}