using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    public class Salary
    {
        public int? Minimum { get; set; }

        public int? Maximum { get; set; }
    }
}