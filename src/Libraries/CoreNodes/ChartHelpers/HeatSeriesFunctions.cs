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
        private static Random rnd = new Random();
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


        private HeatSeriesFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, Dictionary<string, double>> GetNodeInput(List<string> xLabels, List<string> yLabels, List<List<double>> values, List<DSCore.Color> colors)
        {
            var output = new Dictionary<string, Dictionary<string, double>>();

            if (xLabels == null || yLabels == null || values == null)
            {
                for (var i = 0; i < defaultLabelsX.Count; i++)
                {
                    var inner = new Dictionary<string, double>();
                    for (var j = 0; j < defaultLabelsY.Count; j++)
                    {
                        inner.Add(defaultLabelsY[j], rnd.Next(0, 10));
                    }
                    output.Add(defaultLabelsX[i], inner);

                }

                return output;
            }

            if (xLabels.Count != values.Count && xLabels.Count > 0)
            {
                return null;
            }

            for (var i = 0; i < xLabels.Count; i++)
            {
                var inner = new Dictionary<string, double>();
                for (var j = 0; j < yLabels.Count; j++)
                {
                    inner.Add(yLabels[j], values[i][j]);
                }
                output.Add(xLabels[i], inner);
            }

            return output;
        }
    }
}
