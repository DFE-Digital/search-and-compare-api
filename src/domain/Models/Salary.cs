using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    public class Salary
    {
        public long? Minimum { get; set; }

        public long? Maximum { get; set; }
    }
}