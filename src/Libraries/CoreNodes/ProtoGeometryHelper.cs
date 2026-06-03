using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public class BoundaryConditionHelper
    {
        /// <summary>
        /// Converts a string representation to a PanelSurfaceBoundaryCondition enumeration value.
        /// </summary>
        /// <param name="val">The string representation of the boundary condition value.</param>
        /// <returns>The corresponding PanelSurfaceBoundaryCondition enumeration value.</returns>
        public static PanelSurfaceBoundaryCondition BoundaryConditionFromString(string val)
        {
            return Enum.Parse<PanelSurfaceBoundaryCondition>(val);
        }
    }
}
