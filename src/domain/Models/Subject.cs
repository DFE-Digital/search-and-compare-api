using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("subject")]
    public class Subject
    {
                [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }

        public SubjectArea SubjectArea { get; set; }

        public int? FundingId { get; set; }
        public SubjectFunding Funding { get; set; }

        public string Name { get; set; }

        public bool IsSubjectKnowledgeEnhancementAvailable { get; set; }

        public ICollection<CourseSubject> CourseSubjects { get; set; }
    }
}