using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using GovUk.Education.SearchAndCompare.Domain.Filters.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models;

namespace GovUk.Education.SearchAndCompare.Domain.Filters
{
    public class QueryFilter
    { 
        public int? page { get; set; }

        public double? lng { get; set; }

        public double? lat { get; set; }

        public int? rad { get; set; }

        public string loc { get; set; }

        public string lq { get; set; }

        public string subjects { get; set; }

        public int? sortby { get; set; }

        public int? funding { get; set; }

        public string query { get; set; }

        public string display { get; set; }

        public int? zoomlevel { get; set; }

        public double? offlng { get; set; }

        public double? offlat { get; set; }

        public bool pgce { get; set; }

        public bool qts { get; set; }
        
        public bool fulltime { get; set; }
        
        public bool parttime { get; set; }

        

        [IgnoreDataMemberAttribute]
        public List<int> SelectedSubjects { 
            get {            
                List<int> subjectFilterIds = new List<int> ();
                if (!string.IsNullOrEmpty(subjects))
                {
                    subjectFilterIds = subjects.Split(',')?.Select(int.Parse).ToList();
                }
                return subjectFilterIds;
            }

            set {
                subjects = string.Join(",", value);
            }
        }

        [IgnoreDataMemberAttribute]
        public FundingOption SelectedFunding {
            get {
                if (funding.HasValue)
                {
                    return (FundingOption)funding.Value;
                }
                return FundingOption.All;
            }
            set {
                if (value == FundingOption.All)
                {
                    funding = null;
                }
                else
                {
                    funding = (int)value;
                }
            }
        }
        
        [IgnoreDataMemberAttribute]
        public Coordinates Coordinates
        {
            get {
                return lng.HasValue && lat.HasValue && rad.HasValue 
                    ? new Coordinates(lat.Value, lng.Value, null, loc) 
                    : null;
            }
        }

        [IgnoreDataMemberAttribute]
        public Coordinates OffsetCoordinates
        {
            get {
                return offlat.HasValue && offlng.HasValue
                    ? new Coordinates(offlat.Value, offlng.Value) 
                    : null;
            }
        }

        [IgnoreDataMemberAttribute]
        public RadiusOption? RadiusOption
        {
            get {
                return rad.HasValue ? (RadiusOption?) rad.Value : null;
            }
        }

        [IgnoreDataMemberAttribute]
        public SortByOption? SortBy
        {
            get {
                return sortby.HasValue ? (SortByOption?) sortby.Value : null;
            }
        }

        [IgnoreDataMemberAttribute]
        public IEnumerable<SortByOption> ValidSortings
        {
            get {
                return Enum.GetValues(typeof(SortByOption)).Cast<SortByOption>()
                           .Where(x => x != SortByOption.Distance || RadiusOption != null);
            }
        }

        public bool DisplayAsMap
        {
            get
            {
                return display == DisplayOptions.Map;
            }
        }

        public DisplayOptions DisplayOptions = new DisplayOptions();

        public string AsQueryString()
        {
            var properties = from property in this.GetType().GetProperties()
                where property.GetCustomAttribute(typeof(IgnoreDataMemberAttribute)) == null && property.GetValue(this, null) != null
                select property.Name + "=" + HttpUtility.UrlEncode(property.GetValue(this, null).ToString());

            return String.Join("&", properties.ToArray());
        }
    }
}