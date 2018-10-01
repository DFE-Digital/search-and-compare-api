namespace GovUk.Education.SearchAndCompare.Domain.Models.Enums
{
    public enum IncludesPgce
    {
        /* QTS only search category */
        No = 0, // QtsOnly


        /* QTS + PGCE search category */
        Yes = 1, // QtsWithPgce
        QtsWithOptionalPgce = 2,
        QtsWithPgde = 3,
        

        /* "Other" search category */
        QtlsOnly = 4,
        QtlsWithPgce = 5,
        QtlsWithPgde = 6,
    }
}