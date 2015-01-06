using System.Collections.Generic;

using NUnit.Framework;

using Dynamo.DSEngine;
using Dynamo.Search;
using Dynamo.Library;
using Dynamo.Nodes;
using Dynamo.Search.SearchElements;
using Utils = Dynamo.Nodes.Utilities;


namespace Dynamo
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
            parameters1.Add(new TypedParameter("x", "double"));
            parameters1.Add(new TypedParameter("y", "double"));
            FunctionDescriptor functionItem1 = new FunctionDescriptor("Foo", parameters1, FunctionType.GenericFunction);

            var dsFunctionNodeSearchElement1 =
                new DSFunctionNodeSearchElement("", functionItem1, SearchElementGroup.Create);

            Assert.AreEqual(
                "Foo.double-double",
                Utils.TypedParametersToString(dsFunctionNodeSearchElement1.FunctionDescriptor));

            //2 case
            List<TypedParameter> parameters2 = new List<TypedParameter>();
            parameters2.Add(new TypedParameter("point", "Point"));
            FunctionDescriptor functionItem2 = new FunctionDescriptor("Foo", parameters2, FunctionType.GenericFunction);

            var dsFunctionNodeSearchElement2 =
                new DSFunctionNodeSearchElement("", functionItem2, SearchElementGroup.Create);

            Assert.AreEqual(
                "Foo.Point",
                Utils.TypedParametersToString(dsFunctionNodeSearchElement2.FunctionDescriptor));

            //3 case
            List<TypedParameter> parameters3 = new List<TypedParameter>();
            parameters3.Add(new TypedParameter("a", "bool[][]"));
            parameters3.Add(new TypedParameter("b", "var[]"));
            parameters3.Add(new TypedParameter("c", "double[][]"));
            FunctionDescriptor functionItem3 = new FunctionDescriptor("Foo", parameters3, FunctionType.GenericFunction);

            var dsFunctionNodeSearchElement3 =
                new DSFunctionNodeSearchElement("", functionItem3, SearchElementGroup.Create);

            Assert.AreEqual(
                "Foo.bool2-var1-double2",
                Utils.TypedParametersToString(dsFunctionNodeSearchElement3.FunctionDescriptor));

            //4 case
            List<TypedParameter> parameters4 = new List<TypedParameter>();
            parameters4.Add(new TypedParameter("arr", "var[]..[]"));
            parameters4.Add(new TypedParameter("a", "int"));
            FunctionDescriptor functionItem4 = new FunctionDescriptor("Foo", parameters4, FunctionType.GenericFunction);

            var dsFunctionNodeSearchElement4 =
                new DSFunctionNodeSearchElement("", functionItem4, SearchElementGroup.Create);

            Assert.AreEqual(
                "Foo.varN-int",
                Utils.TypedParametersToString(dsFunctionNodeSearchElement4.FunctionDescriptor));

            //5 case
            List<TypedParameter> parameters5 = new List<TypedParameter>();
            parameters5.Add(new TypedParameter("a", "Autodesk.DesignScript.Geometry.Circle"));
            parameters5.Add(new TypedParameter("b", "Xxxx.Yyy.Curve"));
            FunctionDescriptor functionItem5 = new FunctionDescriptor("Foo", parameters5, FunctionType.GenericFunction);

            var dsFunctionNodeSearchElement5 =
                new DSFunctionNodeSearchElement("", functionItem5, SearchElementGroup.Create);

            Assert.AreEqual(
                "Foo.Circle-Curve",
                Utils.TypedParametersToString(dsFunctionNodeSearchElement5.FunctionDescriptor));

            //6 case
            List<TypedParameter> parameters6 = new List<TypedParameter>();
            parameters6.Add(new TypedParameter("a", "int"));
            FunctionDescriptor functionItem6 = new FunctionDescriptor("", parameters6, FunctionType.GenericFunction);

            var dsFunctionNodeSearchElement6 =
                new DSFunctionNodeSearchElement("", functionItem6, SearchElementGroup.Create);

            Assert.AreEqual(
                ".int",
                Utils.TypedParametersToString(dsFunctionNodeSearchElement6.FunctionDescriptor));

        }
    }
}
