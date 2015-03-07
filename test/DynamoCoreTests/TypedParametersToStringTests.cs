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
            //3. Foo(a : bool [ ] [ ] , b : var[], c : double[][]) -> Foo.bool2-var1-double2            
            //4. Foo(arr : var[]..[], a : int) -> Foo.varN-int
            //5. Foo(a: Autodesk.DesignScript.Geometry.Circle, b: Xxxx.Yyy.Curve)
            //6. Empty string(a: int)

            // 1 case
            var parameters1 = new List<TypedParameter>();
            parameters1.Add(new TypedParameter(new TypedParameterParams("x", new ProtoCore.Type { Name = "double" })));
            parameters1.Add(new TypedParameter(new TypedParameterParams("y", new ProtoCore.Type { Name = "double" })));
            var functionItem1 = new FunctionDescriptor(new FunctionDescriptorParams
            {
                FunctionName = "Foo", Parameters = parameters1, FunctionType = FunctionType.GenericFunction
            });

            System.Console.WriteLine(functionItem1.Parameters.Count());
            Assert.AreEqual("Foo.double-double", Utils.TypedParametersToString(functionItem1));

            //2 case
            var parameters2 = new List<TypedParameter>();
            parameters2.Add(new TypedParameter(new TypedParameterParams("point", new ProtoCore.Type { Name = "Point" })));
            var functionItem2 = new FunctionDescriptor(new FunctionDescriptorParams
            {
                FunctionName = "Foo", Parameters = parameters2, FunctionType = FunctionType.GenericFunction
            });

            Assert.AreEqual("Foo.Point", Utils.TypedParametersToString(functionItem2));

            //3 case
            var parameters3 = new List<TypedParameter>();
            parameters3.Add(new TypedParameter(new TypedParameterParams("a", new ProtoCore.Type { Name = "bool [ ] [ ] " })));
            parameters3.Add(new TypedParameter(new TypedParameterParams("b", new ProtoCore.Type { Name = "var[]" })));
            parameters3.Add(new TypedParameter(new TypedParameterParams("c", new ProtoCore.Type { Name = "double[][]" })));
            var functionItem3 = new FunctionDescriptor(new FunctionDescriptorParams
            {
                FunctionName = "Foo", Parameters = parameters3, FunctionType = FunctionType.GenericFunction
            });

            Assert.AreEqual("Foo.bool2-var1-double2", Utils.TypedParametersToString(functionItem3));

            //4 case
            var parameters4 = new List<TypedParameter>();
            parameters4.Add(new TypedParameter(new TypedParameterParams("arr", new ProtoCore.Type { Name = "var[]..[]" })));
            parameters4.Add(new TypedParameter(new TypedParameterParams("a", new ProtoCore.Type { Name = "int" })));
            var functionItem4 = new FunctionDescriptor(new FunctionDescriptorParams
            {
                FunctionName = "Foo", Parameters = parameters4, FunctionType = FunctionType.GenericFunction
            });

            Assert.AreEqual("Foo.varN-int", Utils.TypedParametersToString(functionItem4));

            //5 case
            var parameters5 = new List<TypedParameter>();
            parameters5.Add(new TypedParameter(new TypedParameterParams("a", new ProtoCore.Type { Name = "Autodesk.DesignScript.Geometry.Circle" })));
            parameters5.Add(new TypedParameter(new TypedParameterParams("b", new ProtoCore.Type { Name = "Xxxx.Yyy.Curve" })));
            var functionItem5 = new FunctionDescriptor(new FunctionDescriptorParams
            {
                FunctionName = "Foo", Parameters = parameters5, FunctionType = FunctionType.GenericFunction
            });

            Assert.AreEqual("Foo.Circle-Curve", Utils.TypedParametersToString(functionItem5));

            //6 case
            var parameters6 = new List<TypedParameter>();
            parameters6.Add(new TypedParameter(new TypedParameterParams("a", new ProtoCore.Type { Name = "int" })));
            var functionItem6 = new FunctionDescriptor(new FunctionDescriptorParams
            {
                FunctionName = "", Parameters = parameters6, FunctionType = FunctionType.GenericFunction
            });

            Assert.AreEqual(".int", Utils.TypedParametersToString(functionItem6));
        }
    }
}
