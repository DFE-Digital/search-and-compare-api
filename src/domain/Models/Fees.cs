using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    public class Fees
    {
        public long Uk { get; set; }

        public long Eu { get; set; }

        public long International { get; set; }
    }
}