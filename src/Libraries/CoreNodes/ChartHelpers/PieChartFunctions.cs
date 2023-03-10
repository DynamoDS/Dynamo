using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodes.ChartHelpers
{
    public class PieChartFunctions
    {
        private static List<string> defaultLabels = new List<string> { "Item1", "Item2", "Item3" };
        private static List<double> defaultValues = new List<double> { 100, 100, 100 };
        private PieChartFunctions()
        {
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, double> GetNodeInput(List<string> labels,
            List<double> values,
            List<DSCore.Color> colors)
        {
            var output = new Dictionary<string, double>();

            if(labels == null || values == null)
            {
                for (var i = 0; i < defaultLabels.Count; i++)
                {
                    output.Add(defaultLabels[i], defaultValues[i]);
                }

                return output;
            }

            if (labels.Count != values.Count)
            {
                return output;
            }

            for (var i = 0; i < labels.Count; i++)
            {
                output.Add(labels[i], values[i]);
            }

            return output;
        }
    }
}
