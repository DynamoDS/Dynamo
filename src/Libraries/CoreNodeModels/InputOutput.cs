using Autodesk.DesignScript.Geometry;
using DynamoUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes;

namespace CoreNodeModels
{
    [NodeName("Inputs Outputs")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [OutPortTypes("inputOutput")]
    public class InputOutput : DSDropDownBase
    {
        private Dictionary<string, Type> typeLUT = new Dictionary<string, Type>
        {
            { "Boolean", typeof(bool) },
            { "BoundingBox", typeof(BoundingBox) },
            { "CoordinateSystem", typeof(CoordinateSystem) },
            { "Curve", typeof(Curve) },
            { "Arc", typeof(Arc) },
            { "Circle", typeof(Circle) },
            { "Ellipse", typeof(Ellipse) },
            { "EllipseArc", typeof(EllipseArc) },
            { "Helix", typeof(Helix) },
            { "Line", typeof(Line) },
            { "NurbsCurve", typeof(NurbsCurve) },
            { "PolyCurve", typeof(PolyCurve) },
            { "Polygon", typeof(Polygon) },
            { "Rectangle", typeof(Rectangle) },
            { "DateTime", typeof(DateTime) },
            { "Double", typeof(Double) },
            { "Integer", typeof(int) },
            { "Location", typeof(Location) },
            { "Mesh", typeof(Mesh) },
            { "Plane", typeof(Plane) },
            { "Point", typeof(Point) },
            { "Solid", typeof(Solid) },
            { "Cone", typeof(Cone) },
            { "Cylinder", typeof(Cylinder) },
            { "Cuboid", typeof(Cuboid) },
            { "Sphere", typeof(Sphere) },
            { "String", typeof(string) },
            { "Surface", typeof(Surface) },
            { "NurbsSurface", typeof(NurbsSurface) },
            { "PolySurface", typeof(PolySurface) },
            { "TimeSpan", typeof(TimeSpan) },
            { "UV", typeof(UV) },
            { "Vector", typeof(Vector) },
        };

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            foreach (string typeName in typeLUT.Keys)
            {
                Items.Add(new DynamoDropDownItem(typeName, typeLUT[typeName]));
            }

            return SelectionState.Restore;
        }

    }
}
