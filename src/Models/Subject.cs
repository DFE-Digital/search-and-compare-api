using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.SearchAndCompare.Api.Models.Joins;

namespace GovUk.Education.SearchAndCompare.Api.Models
{
    [Table("subject")]
    public class Subject
    {
        public int Id { get; set; }

        public SubjectArea SubjectArea { get; set; }

        public int? FundingId { get; set; }
        public SubjectFunding Funding { get; set; }

        public string Name { get; set; }

        public ICollection<CourseSubject> CourseSubjects { get; set; }
    }
}