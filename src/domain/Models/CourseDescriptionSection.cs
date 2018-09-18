using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("course-description-section")]
    public class CourseDescriptionSection
    {
                [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }

        public int Ordinal { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int CourseId { get; set; }

                [Newtonsoft.Json.JsonIgnore]
        public Course Course { get; set; }
    }
}