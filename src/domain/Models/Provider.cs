using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("provider")]
    public class Provider
    {
                [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        public string ProviderCode { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [InverseProperty("Provider")]
        public ICollection<Course> Courses { get; set; }

                [Newtonsoft.Json.JsonIgnore]
        [InverseProperty("AccreditingProvider")]
        public ICollection<Course> AccreditedCourses { get; set; }
    }
}