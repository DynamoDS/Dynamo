using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Utilities
{
    public static class ThicknessExtensions
    {
        // SEPARATECORE: ugh!  need to get all of the thickness refs out of DynamoCore
        public static System.Windows.Thickness AsWindowsThickness(this Dynamo.Utilities.Thickness thickness)
        {
            return new System.Windows.Thickness(
                thickness.Left,
                thickness.Top,
                thickness.Right,
                thickness.Bottom);
        }
    }
}
