using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Api.Models
{
    [Table("default-course-description-section")]
    public class DefaultCourseDescriptionSection
    {
        public int Id { get; set; }

        public int Name { get; set; }
    }
}