using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Dynamo.DSEngine;


namespace Dynamo
{
    class ShortenParameterTypeTest
    {
        [Test]
        public void TestShortenParameterType()
        {
            // Dummy variables to create dsFunctionNodeSearchElement.
            IEnumerable<string> tags = new List<string>();
            List<Dynamo.Library.TypedParameter> parameters1 = new List<Dynamo.Library.TypedParameter>();
            FunctionDescriptor functionItem = new FunctionDescriptor("", parameters1, FunctionType.GenericFunction);

            var dsFunctionNodeSearchElement =
                new Dynamo.Search.SearchElements.
                    DSFunctionNodeSearchElement("", functionItem,
                                                 Dynamo.Search.SearchElementGroup.Create);
            //1. Foo(x: double, y : double) -> Foo.double-double
            //2. Foo(point : Point) -> Foo.Point
            //3. Foo(a : bool[][], b : var[], c : double[][]) -> Foo.bool2-var1-double2
            //4. Foo(arr : var[]..[], a : int) -> Foo.varN-int


            // 1 case
            List<Tuple<string, string>> inputParameters1 = new List<Tuple<string, string>>();
            inputParameters1.Add(Tuple.Create<string, string>("x", "double"));
            inputParameters1.Add(Tuple.Create<string, string>("y", "double"));
            Assert.AreEqual(
                "Foo.double-double",
                dsFunctionNodeSearchElement.GetFullIconName("Foo", inputParameters1));

            //2 case
            List<Tuple<string, string>> inputParameters2 = new List<Tuple<string, string>>();
            inputParameters2.Add(Tuple.Create<string, string>("point", "Point"));
            Assert.AreEqual(
                "Foo.Point",
                dsFunctionNodeSearchElement.GetFullIconName("Foo", inputParameters2));

            //3 case
            List<Tuple<string, string>> inputParameters3 = new List<Tuple<string, string>>();
            inputParameters3.Add(Tuple.Create<string, string>("a", "bool[][]"));
            inputParameters3.Add(Tuple.Create<string, string>("b", "var[]"));
            inputParameters3.Add(Tuple.Create<string, string>("c", "double[][]"));
            Assert.AreEqual(
                "Foo.bool2-var1-double2",
                dsFunctionNodeSearchElement.GetFullIconName("Foo", inputParameters3));

            //4 case
            List<Tuple<string, string>> inputParameters4 = new List<Tuple<string, string>>();
            inputParameters4.Add(Tuple.Create<string, string>("arr", "var[]..[]"));
            inputParameters4.Add(Tuple.Create<string, string>("a", "int"));
            Assert.AreEqual(
                "Foo.varN-int",
                dsFunctionNodeSearchElement.GetFullIconName("Foo", inputParameters4));

        }
    }
}
