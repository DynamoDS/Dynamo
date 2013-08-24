using Autodesk.LibG;

namespace DSRevitNodes
{
    /// <summary>
    /// A UV domain specified by a minimum and maximum UVs.
    /// </summary>
    public class Domain:ObjectWithPropertyDictionary
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
            get { return Max.x() - Min.x(); }
        }
        
        /// <summary>
        /// The v dimension span of the domain.
        /// </summary>
        public double VSpan
        {
            get { return Max.y() - Min.y(); }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="min">The minimum UV.</param>
        /// <param name="max">The maximum UV.</param>
        public Domain(Vector min, Vector max)
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
        public static Domain ByMinimumAndMaximum(Vector min, Vector max)
        {
            return new Domain(min, max);
        }

        public override string ToString()
        {
            return string.Format("Min:{0},Max:{1}", Min.ToString(), Max.ToString());
        }

        public override void Dispose()
        {
            //delete the elements

            //unregisters with the persistence manager
        }
    }
}
