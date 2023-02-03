using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodes.ChartHelpers
{
    public class XYLineChartFunctions
    {
        private XYLineChartFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, Dictionary<string, List<double>>> GetNodeInput(List<string> titles, List<List<double>> xValues, List<List<double>> yValues, List<DSCore.Color> colors)
        {
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
