using System;

namespace DynamoUnits
{
    public class Location
    {
        /// <summary>
        /// Gets the name value from a location
        /// </summary>
        /// <returns name="string">Name of location</returns>
        public string Name { get; set; }

        /// <summary>
        /// The Location's latitude in degrees between -90.0 and 90.0
        /// </summary>
        /// <returns name="double">Latitude geographic coordinate of location</returns>
        public double Latitude { get; set; }

        /// <summary>
        /// The Location's Longitude in degrees between -180.0 and 180.0
        /// </summary>
        /// <returns name="double">Longitude geographic coordinate of location</returns>
        public double Longitude { get; set; }

        private Location(double latitude, double longitude, string name)
        {
            Latitude = latitude;
            Longitude = longitude;
            Name = name;
        }

        /// <summary>
        /// Create a Location object by specifying a latitude and a longitude.
        /// </summary>
        /// <param name="latitude">Angle, in degrees, between -90.0 and 90.0</param>
        /// <param name="longitude">Angle, in degrees, between -180.0 and 180.0</param>
        /// <param name="name">A name for the location</param>
        /// <returns name="location">Location object</returns>
        public static Location ByLatitudeAndLongitude(double latitude, double longitude, string name = null)
        {
            if (latitude < -90.0 || latitude > 90.0)
            {
                throw new Exception("You must enter a latitude between -90.0 and 90.");
            }

            if (longitude < -180.0 || longitude > 180)
            {
                throw new Exception("You must enter a longitude between -180.0 and 180.0");
            }

            return new Location(latitude, longitude, name);
        }
        
        public override string ToString()
        {
            return string.Format("Name: {0}, Lat: {1}, Long: {2}",Name, Latitude.ToString(Display.PrecisionFormat), Longitude.ToString(Display.PrecisionFormat));
        }
    }
}
