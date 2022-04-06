using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    public class CustomLabel : IGraphicItem
    {
        private readonly double x;
        private readonly double y;
        private readonly double z;
        private readonly string label;

        private CustomLabel(double x, double y, double z, string label)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.label = label;
        }

        public static CustomLabel ByCoordinatesAndString(double x = 0, double y = 0, double z = 0, string label = "EMPTY_LABEL")
        {
            return new CustomLabel(x, y, z, label);
        }

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            package.AddPointVertex(x, y, z);
            package.Description = label;
        }
    }
}
