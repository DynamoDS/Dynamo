using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    public interface IAnalysisData<TLocation, TValue>
    {
        /// <summary>
        /// A collection of calculation locations.
        /// </summary>
        IEnumerable<TLocation> CalculationLocations { get; }

        /// <summary>
        /// A dictionary of calculation results for
        /// each calculation location.
        /// </summary>
        Dictionary<string, IList<TValue>> Values { get; }

        IList<TValue> GetValuesByKey(string key);
    }

    public interface ISurfaceAnalysisData<TLocation, TValue> : IAnalysisData<TLocation, TValue>
    {
        /// <summary>
        /// The analysis surface.
        /// </summary>
        Surface Surface { get; }
    }

    public interface ICurveAnalysisData<TLocation, TValue> : IAnalysisData<TLocation, TValue>
    {
        /// <summary>
        /// The analysis curve.
        /// </summary>
        Curve Curve { get; }
    }

    /// <summary>
    /// An analysis model.
    /// </summary>
    /// <typeparam name="TLocation">The analysis location type. (i.e. UV, Point)</typeparam>
    /// <typeparam name="TResult">The analysis return type. (i.e. double, vector, SIUnit)</typeparam>
    public interface IAnalysisModel<TLocation, TValue>
    {
        event EventHandler AnalysisCompleted;
        void Analyze(bool parallel);
        IEnumerable<IAnalysisData<TLocation, TValue>> Values { get; }
    }
}
