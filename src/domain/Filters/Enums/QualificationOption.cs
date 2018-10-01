namespace GovUk.Education.SearchAndCompare.Domain.Filters.Enums
{
    public enum QualificationOption
    {
        QtsOnly = 1 << 0,
        
        PgdePgceWithQts = 1 << 1,

        Other = 1 << 2
    }
}