using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    public interface ISurfaceAnalysisData
    {
        Surface Surface { get; set; }
        List<UV> Locations { get; set; }
        Dictionary<string, List<double>> Values { get; set; }
    }

    public interface IPointAnalysisData
    {
        List<Point> Locations { get; set; }
        Dictionary<string, List<double>> Values { get; set; }
    }
}
