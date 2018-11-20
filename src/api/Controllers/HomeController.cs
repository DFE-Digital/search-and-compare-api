using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GovUk.Education.SearchAndCompare.Api.ViewModels;

namespace GovUk.Education.SearchAndCompare.Api.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("")]
        public RedirectResult Index()
        {
            return Redirect("swagger");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
