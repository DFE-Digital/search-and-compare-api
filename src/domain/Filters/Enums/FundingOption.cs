namespace GovUk.Education.SearchAndCompare.Domain.Filters.Enums
{
    public enum FundingOption
    {
        Unfunded = 1,

        Bursary = 1 << 1,

        Scholarship = 1 << 2,

        Salary = 1 << 3,

        NoSalary = Bursary | Scholarship,

        NoBursary = Salary | Scholarship,

        NoScholarship = Salary | Bursary,

        AnyFunding = Salary | NoSalary, 

        All = Unfunded | AnyFunding
    }
}