using System;

namespace DynamoUnits
{
    public class Location
    {
        /// <summary>
        /// The Location's latitude in degrees between -90.0 and 90.0;
        /// </summary>
        public double Latitude { get; internal set; }

        /// <summary>
        /// The Location's Longitude in degrees between -180.0 and 180.0
        /// </summary>
        public double Longitude { get; internal set; }

        private Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Create a Location object by specifying a latitude and a longitude.
        /// </summary>
        /// <param name="latitude">An angle, in degrees, between -90.0 and 90.0</param>
        /// <param name="longitude">An angle, in degrees, between -180.0 and 180.0.</param>
        public static Location ByLatitudeAndLongitude(double latitude, double longitude)
        {
            if (latitude < -90.0 || latitude > 90.0)
            {
                throw new Exception("You must enter a latitude between -90.0 and 90.");
            }

            if (longitude < -180.0 || longitude > 180)
            {
                throw new Exception("You must enter a longitude between -180.0 and 180.0");
            }

            return new Location(latitude, longitude);
        }

        public override string ToString()
        {
            return string.Format("Lat: {0}, Long: {1}", Latitude.ToString(BaseUnit.NumberFormat), Longitude.ToString(BaseUnit.NumberFormat));
        }
    }
}
