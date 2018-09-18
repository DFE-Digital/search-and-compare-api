using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models.Joins
{
    [Table("course_subject")]
    public class CourseSubject
    {
        public int CourseId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
                public Course Course { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int SubjectId { get; set; }

        public Subject Subject { get; set; }
    }
}
