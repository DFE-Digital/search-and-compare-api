using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("subject-funding")]
    public class SubjectFunding
    {
                [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }

        public int? Scholarship { get; set; }

        public int? EarlyCareerPayments { get; set; }

        public int? BursaryFirst { get; set; }

        public int? BursaryUpperSecond { get; set; }

        public int? BursaryLowerSecond { get; set; }
    }
}