using Autodesk.DesignScript.Geometry;

namespace Revit.Geometry
{
    /// <summary>
    /// A two dimensional UV domain specified by minimum and maximum UVs.
    /// </summary>
    public class Domain2D
    {
        /// <summary>
        /// The minimum of the domain.
        /// </summary>
        public Vector Min { get; set; }

        /// <summary>
        /// The maximum of the domain.
        /// </summary>
        public Vector Max { get; set; }

        /// <summary>
        /// The u dimension span of the domain.
        /// </summary>
        public double USpan
        {
            get { return Max.X - Min.X; }
        }

        /// <summary>
        /// The v dimension span of the domain.
        /// </summary>
        public double VSpan
        {
            get { return Max.Y - Min.Y; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="min">The minimum UV.</param>
        /// <param name="max">The maximum UV.</param>
        public Domain2D(Vector min, Vector max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Construct a domain by minimum and maximum.
        /// </summary>
        /// <param name="min">The minimum UV.</param>
        /// <param name="max">The maximum UV.</param>
        /// <returns></returns>
        public static Domain2D ByMinimumAndMaximum(Vector min, Vector max)
        {
            return new Domain2D(min, max);
        }

        public override string ToString()
        {
            return string.Format("Min:{0},Max:{1}", Min.ToString(), Max.ToString());
        }

    }

    /// <summary>
    /// A one dimensional domain specified by minimum and maximum.
    /// </summary>
    public class Domain
    {
        /// <summary>
        /// The minimum of the domain.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// The maximum of the domain.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// The span of the domain.
        /// </summary>
        public double Span
        {
            get { return Max - Min; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public Domain(double min, double max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Construct a domain by minimum and maximum.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public static Domain ByMinimumAndMaximum(double min, double max)
        {
            return new Domain(min, max);
        }

        public override string ToString()
        {
            return string.Format("Min:{0},Max:{1}", Min.ToString(), Max.ToString());
        }
    }
}