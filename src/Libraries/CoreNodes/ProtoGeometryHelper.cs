using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public class BoundaryConditionHelper
    {
        public static PanelSurfaceBoundaryCondition BoundaryConditionFromString(string val)
        {
            return Enum.Parse<PanelSurfaceBoundaryCondition>(val);
        }
    }
}
