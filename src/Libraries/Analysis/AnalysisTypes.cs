using System;
using System.Collections.Generic;
using System.Linq;

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

        public SurfaceAnalysisData(IEnumerable<UV> locations, IEnumerable<string> valueDescriptions, IList<IList<double>> values)
        {
            var descriptions = valueDescriptions as string[] ?? valueDescriptions.ToArray();

            if (descriptions.Count() != values.Count)
            {
                throw new ArgumentException("There must be an equal number of value sets and value descriptions");
            }

            var valueDict = new Dictionary<string, IList<double>>();
            var count = 0;
            foreach (var descrip in descriptions)
            {
                valueDict.Add(descrip, values[count]);
                count++;
            }

            Locations = locations;
            Values = valueDict;
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

        public VectorAnalysisData(IEnumerable<Point> locations, IEnumerable<string> valueDescriptions, IList<IList<Vector>> values)
        {
            var descriptions = valueDescriptions as string[] ?? valueDescriptions.ToArray();

            if (descriptions.Count() != values.Count)
            {
                throw new ArgumentException("There must be an equal number of value sets and value descriptions");
            }

            var valueDict = new Dictionary<string, IList<Vector>>();
            var count = 0;
            foreach (var descrip in descriptions)
            {
                valueDict.Add(descrip, values[count]);
                count++;
            }

            Locations = locations;
            Values = valueDict;
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

        public PointAnalysisData(IEnumerable<Point> locations, IEnumerable<string> valueDescriptions, IList<IList<double>> values)
        {
            var descriptions = valueDescriptions as string[] ?? valueDescriptions.ToArray();

            if (descriptions.Count() != values.Count)
            {
                throw new ArgumentException("There must be an equal number of value sets and value descriptions");
            }

            var valueDict = new Dictionary<string, IList<double>>();
            var count = 0;
            foreach (var descrip in descriptions)
            {
                valueDict.Add(descrip, values[count]);
                count++;
            }

            Locations = locations;
            Values = valueDict;
        }
    }
}
