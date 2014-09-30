using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    public class SurfaceAnalysisData : ISurfaceAnalysisData
    {
        public Surface Surface { get; set; }
        public List<UV> Locations { get; set; }
        public Dictionary<string, List<double>> Values { get; set; }

        public SurfaceAnalysisData(Surface surface, List<UV> locations, Dictionary<string, List<double>> values)
        {
            Surface = surface;
            Locations = locations;
            Values = values;
        }
    }
}
