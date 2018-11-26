using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GovUk.Education.SearchAndCompare.Domain.Models.Enums;
using GovUk.Education.SearchAndCompare.Domain.Models.Joins;

namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    [Table("course")]
    public class Course
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ProgrammeCode { get; set; }

        public string ProviderCodeName { get; set; }

        public int ProviderId { get; set; }

        public Provider Provider { get; set; }

        public int? AccreditingProviderId { get; set; }

        public Provider AccreditingProvider { get; set; }

        public AgeRange AgeRange { get; set; }

        public int RouteId { get; set; }

        public Route Route { get; set; }

        public IncludesPgce IncludesPgce { get; set; }

        public ICollection<CourseDescriptionSection> DescriptionSections { get; set; }

        public ICollection<Campus> Campuses { get; set; }

        public ICollection<CourseSubject> CourseSubjects { get; set; }

        public Fees Fees { get; set; }

        public bool IsSalaried { get; set; }

        public Salary Salary { get; set; }

        public int? ProviderLocationId { get; set; }

        public Location ProviderLocation { get; set; }

        public double? Distance { get; set; }

        public string DistanceAddress { get; set; }

        public int? ContactDetailsId { get; set; }

        public Contact ContactDetails { get; set; }

        public VacancyStatus FullTime { get; set; }

        public VacancyStatus PartTime { get; set; }

        public DateTime? ApplicationsAcceptedFrom { get; set; }

        public DateTime? StartDate { get; set; }

        public string Duration { get; set; }

        public string Mod { get; set; }

        /// <summary>
        /// Aggregate of vacancy status for each campus.
        /// </summary>
        /// <value></value>
        public bool HasVacancies { get; set; }
        /// <summary>
        /// Flags course as Special Education Needs and Disabilities
        /// </summary>
        public bool IsSen { get; set; }

        /// <summary>
        /// A course is consider valid if it has:
        ///     ProgrammeCode
        ///     Provider.ProviderCode
        ///     Route.Name
        ///     At least a Subject in CourseSubjects
        ///     An AccreditingProvider.ProviderCode if AccreditingProvider is provided
        ///     A empty list of Campuses or a valid Location in Campuses if provided
        ///     At least a Fee or a Salary
        /// </summary>
        /// <returns>
        /// True, if it is valid, else false.
        /// </returns>
        public bool IsValid(bool throwException = false)
        {
            var noProgrammeCode = string.IsNullOrWhiteSpace(this.ProgrammeCode);
            var noProvider = this.Provider == null || string.IsNullOrWhiteSpace(this.Provider.ProviderCode);
            var noRoute = this.Route == null || string.IsNullOrWhiteSpace(this.Route.Name);

            var badSubject =  this.CourseSubjects != null ? (this.CourseSubjects.Count() > 0 ?
                this.CourseSubjects.Any(cs => cs.Subject == null || string.IsNullOrWhiteSpace(cs.Subject.Name) ) :
                true) : true;

            var badAccreditingProvider = this.AccreditingProvider != null ? string.IsNullOrWhiteSpace(this.AccreditingProvider.ProviderCode) : false;

            var badCampus = this.Campuses == null ? true : this.Campuses.Any(x => x.Location == null);

            var badFeesOrSalary = this.Fees == null && this.Salary == null;

            var badProviderLocation = false; //this.ProviderLocation == null || string.IsNullOrWhiteSpace(this.ProviderLocation.Address) );

            var badContactDetails = false;//courses.Any(x => {
            //     var cd = this.ContactDetails;
            //     return cd == null;
            // });

            // If this is true then its a no ops, as it will either throw DbUpdateException or InvalidOperationException or NullReferenceException.
            if(noProgrammeCode || noProvider || noRoute || badSubject || badAccreditingProvider || badCampus || badFeesOrSalary || badProviderLocation || badContactDetails)
            {
                if (throwException)
                {
                    var reason = $"noProgrammeCode: {noProgrammeCode}, noProvider: {noProvider}, noRoute: {noRoute},  badSubject: {badSubject}, badAccreditingProvider: {badAccreditingProvider}, badCampus: {badCampus}, badFeesOrSalary: {badFeesOrSalary}, badProviderLocation: {badProviderLocation}, badContactDetails: {badContactDetails}";

                    throw new InvalidOperationException($"Failed precondition reason: [{reason}] ");
                }
            }

            return !(noProvider || noRoute || badSubject || badAccreditingProvider || badCampus || badFeesOrSalary || badProviderLocation || badContactDetails);
        }
    }
}
