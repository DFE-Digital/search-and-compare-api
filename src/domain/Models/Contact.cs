using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("contact")]
    public class Contact
    {
        public int Id { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public Address Address { get; set; }

        public Course Course { get; set; }
    }
}