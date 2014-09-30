using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    public class SurfaceAnalysisData : ISurfaceAnalysisData
    {
        public Surface Surface { get; set; }
        public IEnumerable<UV> Locations { get; set; }
        public Dictionary<string, IList<double>> Values { get; set; }

        public SurfaceAnalysisData(Surface surface, IEnumerable<UV> locations, Dictionary<string, IList<double>> values)
        {
            Surface = surface;
            Locations = locations;
            Values = values;
        }
    }

    public class VectorAnalysisData : IVectorAnalysisData
    {
        public IEnumerable<Point> Locations { get; set; }
        public Dictionary<string, IList<Vector>> Values { get; set; }

        public VectorAnalysisData(IEnumerable<Point> locations, Dictionary<string, IList<Vector>> values)
        {
            Locations = locations;
            Values = values;
        }
    }

    public class PointAnalysisData : IPointAnalysisData
    {
        public IEnumerable<Point> Locations { get; set; }
        public Dictionary<string, IList<double>> Values { get; set; }

        public PointAnalysisData(IEnumerable<Point> locations, Dictionary<string, IList<double>> values)
        {
            Locations = locations;
            Values = values;
        }
    }
}
