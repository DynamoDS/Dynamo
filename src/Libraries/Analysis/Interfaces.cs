using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Analysis
{
    public interface IFieldAnalysisData<TLocation, TResult>
    {
        /// <summary>
        /// A collection of calculation locations.
        /// </summary>
        IEnumerable<TLocation> CalculationLocations { get; set; }

        /// <summary>
        /// A dictionary of calculation results for
        /// each calculation location.
        /// </summary>
        Dictionary<string, IList<TResult>> Results { get; set; }
    }

    public interface ISurfaceAnalysisData<TLocation, TResult> : IFieldAnalysisData<TLocation, TResult>
    {
        /// <summary>
        /// The analysis geometry.
        /// </summary>
        Surface Surface { get; set; }
    }
}
