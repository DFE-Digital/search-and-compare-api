using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("route")]
    public class Route
    {
                [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsSalaried { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public ICollection<Course> Courses { get; set; }
    }
}