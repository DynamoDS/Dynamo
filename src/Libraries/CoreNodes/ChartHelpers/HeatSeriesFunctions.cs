using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodes.ChartHelpers
{
    public class HeatSeriesFunctions
    {
        private static List<string> defaultLabelsX = new List<string> { "Item-1", "Item-2", "Item-3", "Item-4", "Item-5" };
        private static List<string> defaultLabelsY = new List<string>
        {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"
        };

        private static List<List<double>> defaultNestedValues = new List<List<double>> {
            new List<double> { 5, 6, 6, 7, 8, 9, 3 },
            new List<double> { 5, 6, 6, 7, 8, 9, 3 },
            new List<double> { 5, 6, 6, 7, 8, 9, 3 },
            new List<double> { 5, 6, 6, 7, 8, 9, 3 },
            new List<double> { 5, 6, 6, 7, 8, 9, 3 } };

        private HeatSeriesFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<Dictionary<string, string>, List<double>> GetNodeInput(List<string> xLabels, List<string> yLabels, List<List<double>> values, List<DSCore.Color> colors)
        {
            var output = new Dictionary<Dictionary<string, string>, List<double>>();

            if (xLabels == null && yLabels == null && values == null)
            {
                for (var i = 0; i < defaultLabelsX.Count; i++)
                {
                    var labelsDict = new Dictionary<string, string>();
                    for (var j = 0; j < defaultLabelsY.Count; j++)
                    {
                        labelsDict.Add(defaultLabelsY[j], defaultLabelsX[i]);
                    }
                    output.Add(labelsDict, defaultNestedValues[i]);
                }

                return output;
            }

            if (xLabels.Count != values.Count && xLabels.Count > 0)
            {
                return null;
            }


            for (var i = 0; i < xLabels.Count; i++)
            {
                var labelsDict = new Dictionary<string, string>();
                for (var j = 0; j < yLabels.Count; j++)
                {
                    labelsDict.Add(yLabels[j], xLabels[i]);
                }
                output.Add(labelsDict, values[i]);
            }

            return output;
        }
    }
}
