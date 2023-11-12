using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodes.ChartHelpers
{
    public class ScatterPlotFunctions
    {
        private static Random rnd = new Random();
        private static List<string> defaultTitles = new List<string> { "Plot 1", "Plot 2", "Plot 3" };

        private ScatterPlotFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, List<List<double>>> GetNodeInput(List<string> titles, List<List<double>> xValues, List<List<double>> yValues, List<DSCore.Color> colors)
        {
            var output = new Dictionary<string, List<List<double>>>();

            if (titles == null || xValues == null || yValues == null)
            {
                var listX = new List<List<double>>();
                var listY = new List<List<double>>();
                for (var i = 0; i < defaultTitles.Count; i++)
                {
                    listX.Add(GenerateRandomValuesList(20));
                    listY.Add(GenerateRandomValuesList(20));
                }

                for (var i = 0; i < defaultTitles.Count; i++)
                {
                    var combined = new List<List<double>> { listX[i], listY[i] };
                    output.Add(defaultTitles[i], combined);
                }

                return output;
            }

            if (titles.Count != xValues.Count || xValues.Count != yValues.Count)
            {
                return output;
            }

            for (var i = 0; i < titles.Count; i++)
            {
                var combined = new List<List<double>> { xValues[i], yValues[i] };

                output.Add(titles[i], combined);
            }

            return output;
        }

        private static List<double> GenerateRandomValuesList(int count)
        {
            List<double> values = new List<double>();

            for (int i = 0; i < count; i++)
            {
                values.Add(rnd.NextDouble() * 10);
            }

            return values;
        }
    }
}
