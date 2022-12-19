using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodeModelsWpf.Charts.ChartHelpers
{
    public class ScatterPlotFunctions
    {
        private ScatterPlotFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, Dictionary<string, List<double>>> GetNodeInput(List<string> titles, List<List<double>> xValues, List<List<double>> yValues, List<DSCore.Color> colors)
        {
            // TODO - just pass input data unmodified instead?
            var output = new Dictionary<string, Dictionary<string, List<double>>>();

            if (titles.Count != xValues.Count || xValues.Count != yValues.Count)
            {
                return output;
            }

            for (var i = 0; i < titles.Count; i++)
            {
                var coordinates = new Dictionary<string, List<double>>();
                coordinates.Add("X", xValues[i]);
                coordinates.Add("Y", yValues[i]);
                output.Add(titles[i], coordinates);
            }

            return output;
        }
    }
}
