using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("campus")]
    public class Campus
    {
                [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        public string CampusCode { get; set; }

        public int? LocationId { get; set; }

        public Location Location { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public Course Course { get; set; }
    }
}