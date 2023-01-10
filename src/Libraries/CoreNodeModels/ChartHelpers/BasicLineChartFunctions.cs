using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace CoreNodeModels.ChartHelpers
{
    public class BasicLineChartFunctions
    {
        private BasicLineChartFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, List<double>> GetNodeInput(List<string> titles, List<List<double>> values, List<DSCore.Color> colors)
        {
            // TODO - just pass input data unmodified instead?
            var output = new Dictionary<string, List<double>>();

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
