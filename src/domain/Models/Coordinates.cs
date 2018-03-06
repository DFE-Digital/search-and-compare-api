namespace GovUk.Education.SearchAndCompare.Domain.Models
{
    public class Coordinates 
    {        
        public double Latitude { get; }

        public double Longitude { get; }

        public string RawInput { get; }

        public string FormattedLocation { get; }

        public string FormattedCoordinates {
            get {
                return string.Format("{0},{1}", Latitude, Longitude);
            }
        }

        public Coordinates(double latitude, double longitude, string rawInput, string formattedLocation)
        {
            Latitude = latitude;
            Longitude = longitude;
            RawInput = rawInput;
            FormattedLocation = formattedLocation;
        }

        public Coordinates(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            RawInput = string.Empty;
            FormattedLocation = string.Empty;
        }

        public static Coordinates operator +(Coordinates c1, Coordinates c2)
        {
            // This is very simplified and won't work at the edges of the world!
            return new Coordinates(c1.Latitude + c2.Latitude, c1.Longitude + c2.Longitude);
        }

        public static bool operator ==(Coordinates c1, Coordinates c2)
        {
            if (ReferenceEquals(c1, c2)) { return true; }
            if (ReferenceEquals(c1, null)) { return false; }
            if (ReferenceEquals(c2, null)) { return false; }
            return c1.Latitude == c2.Latitude && c1.Longitude == c2.Longitude;
        }

        public static bool operator !=(Coordinates c1, Coordinates c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var c2 = (Coordinates)obj;
            return Latitude == c2.Latitude && Longitude == c2.Longitude;
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }
    }
}