using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
//using FFITarget.Dynamo;

namespace EmitMSIL
{
    // This class will be emitted from the AST/Dynamo graph
    class ExecuteIL
    {
        //public Point PointWrapper(double x, double y, double z)
        //{
        //    return Point.XYZ(x, y, z);
        //}

        //public Circle CircleWrapper(Point pt, double rad)
        //{
        //    return Circle.ByPointRadius(pt, rad);
        //}

        public static void Execute(IDictionary<string, IList> input, IDictionary<string, IList> output)
        {
            // Sample script for which below code is emitted:
            // x = 12;
            // y = 13;
            // z = 14;
            // c = Point.ByCoordinates(x<1>, y<2>, z<3>);
            // d = Sphere.ByCenterPointRadius(c, x);

            // 1. "x", "y", "z", identifier names are read from lhs of assignment expression
            // 2. check if each ident exists in input and output dictionaries and emit assignments accordingly
            var x = 12;
            output.Add("x", new int[] { x });
            var y = 13;
            output.Add("y", new int[] { y });
            var z = 14;
            output.Add("z", new int[] { z });
            var args = new List<object> { x, y, z };

            // 3. replication guides are read as strings from arguments in Function call AST
            var guides = new string[][] { new string[] { "1" }, new string[] { "2" }, new string[] { "3" } };
            // 4. class name is read from identifierlistnode for function call
            var className = "Autodesk.DesignScript.Geometry.Point";

            // 5. method name is read from function call AST
            //string methodName = $"PointWrapper";
            var methodName = "ByCoordinates";

            var mi = CodeGenIL.FunctionLookup(className, methodName, args);
            // 6. Emit call to ReplicationLogic by passing input args from previous steps
            IList c = Replication.ReplicationLogic(mi, args, guides);

            // 7. read output identifier from lhs of AST and emit assignment
            output.Add("c", c);

            // 1. read identifiers used in function call AST
            // 2. check if each ident exists in input and output dictionaries and emit assignments accordingly
            c = output["c"];
            //x = input["x"];
            args = new List<object> { c, x };
            // 3. read guides as strings from args in Function call AST
            guides = new string[][] { new string[] { } };
            // 4. read classname from ident list node for function call 
            className = "Autodesk.DesignScript.Geometry.Sphere";
            // 5. read methodname from function call AST
            //methodName = "CircleWrapper";
            methodName = "ByCenterPointRadius";
            mi = CodeGenIL.FunctionLookup(className, methodName, args);
            // 6. Emit call to ReplicationLogic by passing input args from previous steps
            IList d = Replication.ReplicationLogic(mi, args, guides);
            // 7. read output identifier from lhs of AST and emit assignment
            output.Add("d", d);

            // Repeat

            //IList e = Replication.ReplicationLogic(Conditional, d);
            //output.Add("e", e);
        }
    }
}
