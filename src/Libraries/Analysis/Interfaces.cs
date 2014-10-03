using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    public interface IAnalysisData<TLocation, TResult>
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

    public interface ISurfaceAnalysisData<TLocation, TResult> : IAnalysisData<TLocation, TResult>
    {
        /// <summary>
        /// The analysis surface.
        /// </summary>
        Surface Surface { get; set; }
    }

    public interface ICurveAnalysisData<TLocation, TResult> : IAnalysisData<TLocation, TResult>
    {
        /// <summary>
        /// The analysis curve.
        /// </summary>
        Curve Curve { get; set; }
    }

    /// <summary>
    /// An analysis model.
    /// </summary>
    /// <typeparam name="TLocation">The analysis location type. (i.e. UV, Point)</typeparam>
    /// <typeparam name="TResult">The analysis return type. (i.e. double, vector, SIUnit)</typeparam>
    public interface IAnalysisModel<TLocation,TResult>
    {
        void PreAnalysis();
        void Analyze(bool parallel);
        void PostAnalysis();
        IEnumerable<IAnalysisData<TLocation,TResult>> Results { get; set; }
    }
}
