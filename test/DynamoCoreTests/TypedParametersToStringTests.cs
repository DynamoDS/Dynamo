using System.Collections.Generic;
using System.Linq;
using Dynamo.DSEngine;
using Dynamo.Library;
using NUnit.Framework;
using Utils = Dynamo.Nodes.Utilities;

namespace Dynamo.Tests
{
    class TypedParametersToStringTests
    {
        [Test]
        public void TypedParametersToStringTest()
        {
            //1. Foo(x: double, y : double) -> Foo.double-double
            //2. Foo(point : Point) -> Foo.Point
            //3. Foo(a : bool[][], b : var[], c : double[][]) -> Foo.bool2-var1-double2
            //4. Foo(arr : var[]..[], a : int) -> Foo.varN-int
            //5. Foo(a: Autodesk.DesignScript.Geometry.Circle, b: Xxxx.Yyy.Curve)
            //6. Empty string(a: int)

            // 1 case
            List<TypedParameter> parameters1 = new List<TypedParameter>();
            parameters1.Add(new TypedParameter("x", new ProtoCore.Type { Name = "double" }));
            parameters1.Add(new TypedParameter("y", new ProtoCore.Type { Name = "double" }));
            FunctionDescriptor functionItem1 = new FunctionDescriptor("Foo", parameters1, FunctionType.GenericFunction);

            System.Console.WriteLine(functionItem1.Parameters.Count());
            Assert.AreEqual("Foo.double-double", Utils.TypedParametersToString(functionItem1));

            //2 case
            List<TypedParameter> parameters2 = new List<TypedParameter>();
            parameters2.Add(new TypedParameter("point", new ProtoCore.Type { Name = "Point" }));
            FunctionDescriptor functionItem2 = new FunctionDescriptor("Foo", parameters2, FunctionType.GenericFunction);

            Assert.AreEqual("Foo.Point", Utils.TypedParametersToString(functionItem2));

            //3 case
            List<TypedParameter> parameters3 = new List<TypedParameter>();
            parameters3.Add(new TypedParameter("a", new ProtoCore.Type { Name = "bool[][]" }));
            parameters3.Add(new TypedParameter("b", new ProtoCore.Type { Name = "var[]" }));
            parameters3.Add(new TypedParameter("c", new ProtoCore.Type { Name = "double[][]" }));
            FunctionDescriptor functionItem3 = new FunctionDescriptor("Foo", parameters3, FunctionType.GenericFunction);

            Assert.AreEqual("Foo.bool2-var1-double2", Utils.TypedParametersToString(functionItem3));

            //4 case
            List<TypedParameter> parameters4 = new List<TypedParameter>();
            parameters4.Add(new TypedParameter("arr", new ProtoCore.Type { Name = "var[]..[]" }));
            parameters4.Add(new TypedParameter("a", new ProtoCore.Type { Name = "int" }));
            FunctionDescriptor functionItem4 = new FunctionDescriptor("Foo", parameters4, FunctionType.GenericFunction);

            Assert.AreEqual("Foo.varN-int", Utils.TypedParametersToString(functionItem4));

            //5 case
            List<TypedParameter> parameters5 = new List<TypedParameter>();
            parameters5.Add(new TypedParameter("a", new ProtoCore.Type { Name = "Autodesk.DesignScript.Geometry.Circle" }));
            parameters5.Add(new TypedParameter("b", new ProtoCore.Type { Name = "Xxxx.Yyy.Curve" }));
            FunctionDescriptor functionItem5 = new FunctionDescriptor("Foo", parameters5, FunctionType.GenericFunction);

            Assert.AreEqual("Foo.Circle-Curve", Utils.TypedParametersToString(functionItem5));

            //6 case
            List<TypedParameter> parameters6 = new List<TypedParameter>();
            parameters6.Add(new TypedParameter("a", new ProtoCore.Type { Name = "int" }));
            FunctionDescriptor functionItem6 = new FunctionDescriptor("", parameters6, FunctionType.GenericFunction);

            Assert.AreEqual(".int", Utils.TypedParametersToString(functionItem6));
        }
    }
}
