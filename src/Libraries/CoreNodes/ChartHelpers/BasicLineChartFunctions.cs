using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodes.ChartHelpers
{
    public class BasicLineChartFunctions
    {
        private static List<string> defaultLabels = new List<string> { "Series 1", "Series 2", "Series 3" };

        private static List<List<double>> defaultNestedValues = new List<List<double>> {
            new List<double> { 4, 6, 5, 2, 4 },
            new List<double> { 6, 7, 3, 4, 6 },
            new List<double> { 4, 2, 7, 2, 7 } };

        private BasicLineChartFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, List<double>> GetNodeInput(List<string> titles, List<List<double>> values, List<DSCore.Color> colors)
        {
            var output = new Dictionary<string, List<double>>();

            if (titles == null || values == null)
            {
                for (var i = 0; i < defaultLabels.Count; i++)
                {
                    output.Add(defaultLabels[i], defaultNestedValues[i]);
                }

                return output;
            }

            if (titles.Count != values.Count)
            {
                return output;
            }

            for (var i = 0; i < titles.Count; i++)
            {
                output.Add(titles[i], values[i]);
            }

            return output;
        }
    }
}
