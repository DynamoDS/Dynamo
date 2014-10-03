using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    internal enum AnalysisStatus
    {
        InProgress, // Analysis ongoing
        Paused,     // Analysis paused and can continue
        Stopped,    // Analysis stopped or has not started. There is no results to be shown
        Completed   // Analysis completed. There is results to be shown.
    }

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
        event EventHandler AnalysisCompleted;
        void Analyze(bool parallel);
        IEnumerable<IAnalysisData<TLocation,TResult>> Results { get; }
    }
}
