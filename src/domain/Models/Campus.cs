using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("campus")]
    public class Campus
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CampusCode { get; set; }

        public int? LocationId { get; set; }

        public Location Location { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }
    }
}