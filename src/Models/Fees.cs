using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.SearchAndCompare.Api.Models.Enums;
using GovUk.Education.SearchAndCompare.Api.Models.Joins;

namespace GovUk.Education.SearchAndCompare.Api.Models
{
    [Table("fees")]
    public class Fees
    {
        public int Id { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        public long UkFees { get; set; }

        public long EuFees { get; set; }

        public long InternationalFees { get; set; }
    }
}