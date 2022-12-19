using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodeModelsWpf.Charts.ChartHelpers
{
    public class HeatSeriesFunctions
    {
        private HeatSeriesFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, List<double>> GetNodeInput(List<string> xLabels, List<string> yLabels, List<List<double>> values, List<DSCore.Color> colors)
        {
            // TODO - just pass input data unmodified instead?
            if (xLabels.Count != values.Count && xLabels.Count > 0)
            {
                return null;
            }

            var output = new Dictionary<string, List<double>>();

            for (var i = 0; i < xLabels.Count; i++)
            {
                output.Add(xLabels[i], values[i]);
            }

            return output;
        }
    }
}
