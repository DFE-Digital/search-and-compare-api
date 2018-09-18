using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("contact")]
    public class Contact
    {
                [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public string Address { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Course Course { get; set; }
    }
}