using System;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

using Revit.GeometryConversion;

using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.Elements
{
    public class SunSettings : Element
    {
        [IsVisibleInDynamoLibrary(false)]
        public static SunSettings Current()
        {
            return new SunSettings(Document.ActiveView.SunAndShadowSettings);
        }

        private SunSettings(SunAndShadowSettings settings)
        {
            InternalSunAndShadowSettings = settings;
            InternalElementId = settings.Id;
            InternalUniqueId = settings.UniqueId;
            IsRevitOwned = true;
        }

        internal SunAndShadowSettings InternalSunAndShadowSettings { get; private set; }

        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalSunAndShadowSettings; }
        }

        /// <summary>
        ///     Calculates the direction of the sun.
        /// </summary>
        public Vector SunDirection
        {
            get
            {
                // Rotations are + CW from Y (North)
                // This contradicts the Revit API documentation
                // but matches the Sun Path diagram in Revit.
                var initialDirection = Vector.YAxis();

                var azimuth =
                    InternalSunAndShadowSettings.GetFrameAzimuth(
                        InternalSunAndShadowSettings.ActiveFrame);

                var altitude =
                    InternalSunAndShadowSettings.GetFrameAltitude(
                        InternalSunAndShadowSettings.ActiveFrame);

                var cs = CoordinateSystem.Identity()
                    .Rotate(Point.Origin(), Vector.ZAxis(), -azimuth.ToDegrees());

                var sunDirection =
                    initialDirection.Transform(cs.Rotate(cs.Origin, cs.XAxis, altitude.ToDegrees()));

                return sunDirection.Scale(100);
            }
        }

        /// <summary>
        ///     Extracts the Altitude.
        /// </summary>
        public double Altitude
        {
            get { return InternalSunAndShadowSettings.GetFrameAltitude(InternalSunAndShadowSettings.ActiveFrame).ToDegrees(); }
        }

        /// <summary>
        ///     Extracts the Azimuth.
        /// </summary>
        public double Azimuth
        {
            get { return InternalSunAndShadowSettings.GetFrameAzimuth(InternalSunAndShadowSettings.ActiveFrame).ToDegrees(); }
        }

        /// <summary>
        ///     Gets the Start Date and Time of the solar study.
        /// </summary>
        public DateTime StartDateTime
        {
            get { return InternalSunAndShadowSettings.StartDateAndTime; }
        }
        
        /// <summary>
        ///     Gets the End Date and Time of the solar study.
        /// </summary>
        public DateTime EndDateTime
        {
            get { return InternalSunAndShadowSettings.EndDateAndTime; }
        }

        /// <summary>
        ///     Gets the Date and Time for the current frame of the solar study.
        /// </summary>
        public DateTime CurrentDateTime
        {
            get { return InternalSunAndShadowSettings.ActiveFrameTime; }
        }

    }
}
