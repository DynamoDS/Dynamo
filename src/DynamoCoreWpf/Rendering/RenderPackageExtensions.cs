using System.Linq;
using System.Windows.Media;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Wpf.Rendering
{
    public static class RenderPackageExtensions
    {
        public static bool AllLineStripVerticesHaveColor(this IRenderPackage package, Color color)
        {
            if (!package.LineStripVertices.Any())
            {
                return false;
            }

            for (var i = 0; i < package.LineStripVertexColors.Count(); i += 4)
            {
                if (color.R != package.LineStripVertexColors.ElementAt(i) ||
                    color.G != package.LineStripVertexColors.ElementAt(i + 1) ||
                    color.B != package.LineStripVertexColors.ElementAt(i + 2) ||
                    color.A != package.LineStripVertexColors.ElementAt(i + 3))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
