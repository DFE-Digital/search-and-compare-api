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
        [HttpGet("home")]
        [HttpGet("home/index")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
