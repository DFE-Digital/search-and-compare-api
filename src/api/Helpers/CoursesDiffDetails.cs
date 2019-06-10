using GovUk.Education.SearchAndCompare.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.SearchAndCompare.Api.Helpers
{

    public class CoursesDiffDetails
    {

        public CoursesDiffDetails(IList<string> currentCourses, IList<string> recievedCourses)
        {
            Removed = currentCourses.Except(recievedCourses).ToList();
            Added = recievedCourses.Except(currentCourses).ToList();
        }

        public IList<string> Added {get;}
        public IList<string> Removed {get;}
    }
}
