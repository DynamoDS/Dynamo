namespace Dynamo.Nodes
{
    /// <summary>
    /// Built-in Dynamo Categories. If you want your node to appear in one of the existing Dynamo
    /// categories, then use these constants. This ensures that if the names of the categories
    /// change down the road, your node will still be placed there.
    /// </summary>
    public static partial class BuiltinNodeCategories
    {
        public const string CORE = "Core";
        public const string CORE_INPUT = "Core.Input";
        public const string CORE_STRINGS = "Core.Strings";
        public const string CORE_LISTS_CREATE = "Core.Lists.Create";
        public const string CORE_LISTS_MODIFY = "Core.Lists.Modify";
        public const string CORE_LISTS_EVALUATE = "Core.Lists.Evaluate";
        public const string CORE_LISTS_QUERY = "Core.Lists.Query";
        public const string CORE_VIEW = "Core.View";
        public const string CORE_ANNOTATE = "Core.Annotate";
        public const string CORE_EVALUATE = "Core.Evaluate";
        public const string CORE_TIME = "Core.Time";
        public const string CORE_SCRIPTING = "Core.Scripting";
        public const string CORE_FUNCTIONS = "Core.Functions";

        public const string LOGIC = "Logic";
        public const string LOGIC_MATH_ARITHMETIC = "Logic.Math.Arithmetic";
        public const string LOGIC_MATH_ROUNDING = "Logic.Math.Rounding";
        public const string LOGIC_MATH_CONSTANTS = "Logic.Math.Constants";
        public const string LOGIC_MATH_TRIGONOMETRY = "Logic.Math.Trigonometry";
        public const string LOGIC_MATH_RANDOM = "Logic.Math.Random";
        public const string LOGIC_MATH_OPTIMIZE = "Logic.Math.Optimize";
        public const string LOGIC_EFFECT = "Logic.Effect";
        public const string LOGIC_COMPARISON = "Logic.Comparison";
        public const string LOGIC_CONDITIONAL = "Logic.Conditional";
        public const string LOGIC_LOOP = "Logic.Loop";


        public const string GEOMETRY = "Geometry";

        public const string GEOMETRY_CURVE_CREATE = "Geometry.Curve.Create";
        public const string GEOMETRY_CURVE_DIVIDE = "Geometry.Curve.Divide";
        public const string GEOMETRY_CURVE_PRIMITIVES = "Geometry.Curve.Primitives";
        public const string GEOMETRY_CURVE_QUERY = "Geometry.Curve.Query";
        public const string GEOMETRY_CURVE_FIT = "Geometry.Curve.Fit";

        public const string GEOMETRY_POINT_CREATE = "Geometry.Point.Create";
        public const string GEOMETRY_POINT_MODIFY = "Geometry.Point.Modify";
        public const string GEOMETRY_POINT_QUERY = "Geometry.Point.Query";
        public const string GEOMETRY_POINT_GRID = "Geometry.Point.Grid";
        public const string GEOMETRY_POINT_TESSELATE = "Geometry.Point.Tesselate";

        public const string GEOMETRY_SOLID_BOOLEAN = "Geometry.Solid.Boolean";
        public const string GEOMETRY_SOLID_CREATE = "Geometry.Solid.Create";
        public const string GEOMETRY_SOLID_MODIFY = "Geometry.Solid.Modify";
        public const string GEOMETRY_SOLID_PRIMITIVES = "Geometry.Solid.Primitives";
        public const string GEOMETRY_SOLID_QUERY = "Geometry.Solid.Extract";
        public const string GEOMETRY_SOLID_REPAIR = "Geometry.Solid.Repair";

        public const string GEOMETRY_SURFACE_CREATE = "Geometry.Surface.Create";
        public const string GEOMETRY_SURFACE_QUERY = "Geometry.Surface.Query";
        public const string GEOMETRY_SURFACE_UV = "Geometry.Surface.UV";
        public const string GEOMETRY_SURFACE_DIVIDE = "Geometry.Surface.Divide";

        public const string GEOMETRY_TRANSFORM_APPLY = "Geometry.Transform.Apply";
        public const string GEOMETRY_TRANSFORM_MODIFY = "Geometry.Transform.Modify";
        public const string GEOMETRY_TRANSFORM_CREATE = "Geometry.Transform.Create";

        public const string GEOMETRY_INTERSECT = "Geometry.Intersect";

        public const string GEOMETRY_EXPERIMENTAL_PRIMITIVES = "Geometry.Experimental.Primitives";
        public const string GEOMETRY_EXPERIMENTAL_SURFACE = "Geometry.Experimental.Surface";
        public const string GEOMETRY_EXPERIMENTAL_CURVE = "Geometry.Experimental.Curve";
        public const string GEOMETRY_EXPERIMENTAL_SOLID = "Geometry.Experimental.Solid";
        public const string GEOMETRY_EXPERIMENTAL_MODIFY = "Geometry.Experimental.Modify";
        public const string GEOMETRY_EXPERIMENTAL_VIEW = "Geometry.Experimental.View";

        public const string REVIT = "Revit";
        public const string REVIT_DOCUMENT = "Revit.Document";
        public const string REVIT_DATUMS = "Revit.Datums";
        public const string REVIT_FAMILIES = "Revit.Families";
        public const string REVIT_SELECTION = "Revit.Selection";
        public const string REVIT_VIEW = "Revit.View";
        public const string REVIT_REFERENCE = "Revit.Reference";
        public const string REVIT_PARAMETERS = "Revit.Parameters";
        public const string REVIT_BAKE = "Revit.Bake";
        public const string REVIT_API = "Revit.API";

        public const string ANALYZE = "Analyze";
        public const string ANALYZE_MEASURE = "Analyze.Measure";
        public const string ANALYZE_DISPLAY = "Analyze.Display";
        public const string ANALYZE_COLOR = "Analyze.Color";
        public const string ANALYZE_STRUCTURE = "Analyze.Structure";
        public const string ANALYZE_CLIMATE = "Analyze.Climate";
        public const string ANALYZE_ACOUSTIC = "Analyze.Acoustic";
        public const string ANALYZE_SOLAR = "Analyze.Solar";

        public const string IO = "Input/Output";
        public const string IO_FILE = "Input/Output.File";
        public const string IO_NETWORK = "Input/Output.Network";
        public const string IO_HARDWARE = "Input/Output.Hardware";
    }
}