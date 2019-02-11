namespace GovUk.Education.SearchAndCompare.Domain.Models.Enums
{
    public enum IncludesPgce
    {
        /* QTS only search category */
        QtsOnly = 0,


        /* QTS + PGCE search category */
        QtsWithPgce = 1,
        QtsWithOptionalPgce = 2,
        QtsWithPgde = 3,
        

        /* "Other" search category */
        QtlsOnly = 4,
        QtlsWithPgce = 5,
        QtlsWithPgde = 6,
    }
}