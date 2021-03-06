using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("subject-funding")]
    public class SubjectFunding 
    {
        public int Id { get; set; }

        public int? Scholarship { get; set; }

        public int? EarlyCareerPayments { get; set; }

        public int? BursaryFirst { get; set; }
    }
}