using Autodesk.Revit.DB;

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
        public UV Min { get; set; }
        
        /// <summary>
        /// The maximum of the domain.
        /// </summary>
        public UV Max { get; set; }
        
        /// <summary>
        /// The u dimension span of the domain.
        /// </summary>
        public double USpan
        {
            get { return Max.U - Min.U; }
        }
        
        /// <summary>
        /// The v dimension span of the domain.
        /// </summary>
        public double VSpan
        {
            get { return Max.V - Min.V; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="min">The minimum UV.</param>
        /// <param name="max">The maximum UV.</param>
        public Domain(UV min, UV max)
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
        public static Domain ByMinimumAndMaximum(UV min, UV max)
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
