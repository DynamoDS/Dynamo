using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodeModels.ChartHelpers
{
    public class PieChartFunctions
    {
        private PieChartFunctions()
        {
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, double> GetNodeInput(List<string> labels, List<double> values,
            List<DSCore.Color> colors)
        {
            // TODO - just pass input data unmodified instead?
            var output = new Dictionary<string, double>();

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
