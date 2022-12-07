using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodeModels.Charts.ChartHelpers
{
    public class BarChartFunctions
    {
        private BarChartFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, List<double>> GetNodeInput(List<string> labels, List<List<double>> values, List<DSCore.Color> colors)
        {
            // TODO - just pass input data unmodified instead?
            var output = new Dictionary<string, List<double>>();

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
