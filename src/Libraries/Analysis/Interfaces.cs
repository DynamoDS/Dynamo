using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    public interface ISurfaceAnalysisData
    {
        Surface Surface { get; set; }
        IEnumerable<UV> Locations { get; set; }
        Dictionary<string, IList<double>> Values { get; set; }
    }

    public interface IPointAnalysisData
    {
        IEnumerable<Point> Locations { get; set; }
        Dictionary<string, IList<double>> Values { get; set; }
    }

    public interface IVectorAnalysisData
    {
        IEnumerable<Point> Locations { get; set; }
        Dictionary<string, IList<Vector>> Values { get; set; }
    }
}
