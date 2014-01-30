using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class PrintValueTests
    {

        [Test]
        public void BuildValueStringIsCorrectForString()
        {
            var val = FScheme.Value.NewString("Lorem ipsum dolor sit amet, consectetur adipiscing elit. In sed nisl eget ante ullamcorper pellentesque.");
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("\"Lorem ipsum dolor si...\"", res);
        }

        [Test]
        public void BuildValueStringIsCorrectForEmtpyList()
        {
            var val = FScheme.Value.NewList(new FSharpList<FScheme.Value>(null, null));
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("List (empty)", res);
        }

        [Test]
        public void BuildValueStringIsCorrectForVeryLongString()
        {
            var val = FScheme.Value.NewString("Cool string");
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("\"Cool string\"", res);
        }

        [Test]
        public void BuildValueStringIsCorrectForDouble()
        {
            var val = FScheme.Value.NewNumber(-1.2328);
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("-1.2328", res);
        }

        [Test]
        public void BuildValueStringIsCorrectForDoubleWithManyDecimalPoints()
        {
            var val = FScheme.Value.NewNumber(-1.232812983923);
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("-1.232813", res);
        }

        [Test]
        public void BuildValueStringIsCorrectForFunction()
        {
            var val = FScheme.Value.NewFunction(null);
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("<function>", res);
        }

        [Test]
        public void BuildValueStringIsCorrectForListOfDoubles()
        {
            var list = new List<double>() {1, 2, 3, 4};
            var val = FScheme.Value.NewList(Utils.ToFSharpList(list.Select(FScheme.Value.NewNumber)));
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("List\n  1\n  2\n  ...", res);
        }

        [Test]
        public void BuildValueStringIsCorrectForListOfStrings()
        {
            var list = new List<string>() { "This", "is a", "list", "of strings" };
            var val = FScheme.Value.NewList(Utils.ToFSharpList(list.Select(FScheme.Value.NewString)));
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("List\n  \"This\"\n  \"is a\"\n  ...", res);
        }

        [Test]
        public void BuildValueStringIsCorrectFor2DListOfStringsWithArray1Recurse3()
        {

            var nestedList1 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() {"This", "is a", "list", "of strings"}).Select(FScheme.Value.NewString)));
            var nestedList2 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() {"Another", "list", "of strings"}).Select(FScheme.Value.NewString)));
            var nestedList3 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() {"This", "is a", "list", "of strings"}).Select(FScheme.Value.NewString)));

            var list = new List<FScheme.Value>() { nestedList1, nestedList2, nestedList3 };

            var val = FScheme.Value.NewList(Utils.ToFSharpList(list));
            var res = NodeModel.PrintValue(val, 0, 1, 0, 3);
            Assert.AreEqual("List\n" +
                            "  List\n"+
                            "    \"This\"\n"+
                            "    ...\n"+
                            "  ..."
                            , res);

        }

        [Test]
        public void BuildValueStringIsCorrectFor2DListOfStringsWithArray2Recurse3()
        {

            var nestedList1 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "This", "is a", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList2 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Another", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList3 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "This", "is a", "list", "of strings" }).Select(FScheme.Value.NewString)));

            var list = new List<FScheme.Value>() { nestedList1, nestedList2, nestedList3 };

            var val = FScheme.Value.NewList(Utils.ToFSharpList(list));
            var res = NodeModel.PrintValue(val, 0, 2, 0, 3);
            Assert.AreEqual("List\n" +
                            "  List\n" +
                            "    \"This\"\n" +
                            "    \"is a\"\n" +
                            "    ...\n" +
                            "  List\n" +
                            "    \"Another\"\n" +
                            "    \"list\"\n" +
                            "    ...\n" +
                            "  ..."
                            , res);

        }

        [Test]
        public void BuildValueStringIsCorrectFor2DListOfStringsWithArray3Recurse3()
        {
            var nestedList1 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "This", "is a", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList2 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Another", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList3 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Finally", "a", "list", "of strings" }).Select(FScheme.Value.NewString)));

            var list = new List<FScheme.Value>() { nestedList1, nestedList2, nestedList3 };

            var val = FScheme.Value.NewList(Utils.ToFSharpList(list));
            var res = NodeModel.PrintValue(val, 0, 3, 0, 3);
            Assert.AreEqual("List\n" +
                            "  List\n" +
                            "    \"This\"\n" +
                            "    \"is a\"\n" +
                            "    \"list\"\n" +
                            "    ...\n" +
                            "  List\n" +
                            "    \"Another\"\n" +
                            "    \"list\"\n" +
                            "    \"of strings\"\n" +
                            "  List\n" +
                            "    \"Finally\"\n" +
                            "    \"a\"\n" +
                            "    \"list\"\n" +
                            "    ..."
                            , res);
        }

        [Test]
        public void BuildValueStringIsCorrectFor2DListOfStringsWithArray3Recurse2()
        {
            var nestedList1 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "This", "is a", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList2 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Another", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList3 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Finally", "a", "list", "of strings" }).Select(FScheme.Value.NewString)));

            var list = new List<FScheme.Value>() { nestedList1, nestedList2, nestedList3 };

            var val = FScheme.Value.NewList(Utils.ToFSharpList(list));
            var res = NodeModel.PrintValue(val, 0, 3, 0, 2);
            Assert.AreEqual("List\n" +
                            "  List\n" +
                            "    ...\n" +
                            "  List\n" +
                            "    ...\n" +
                            "  List\n" +
                            "    ..."
                            , res);
        }

        [Test]
        public void BuildValueStringIsCorrectFor2DListOfStringsWithArray2Recurse2()
        {
            var nestedList1 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "This", "is a", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList2 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Another", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList3 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Finally", "a", "list", "of strings" }).Select(FScheme.Value.NewString)));

            var list = new List<FScheme.Value>() { nestedList1, nestedList2, nestedList3 };

            var val = FScheme.Value.NewList(Utils.ToFSharpList(list));
            var res = NodeModel.PrintValue(val, 0, 2, 0, 2);
            Assert.AreEqual("List\n" +
                            "  List\n" +
                            "    ...\n" +
                            "  List\n" +
                            "    ...\n" +
                            "  ..."
                            , res);
        }

        [Test]
        public void BuildValueStringIsCorrectFor3DListOfStringsWithArray3Recurse3SimpleInteriorArray()
        {
            var nestedList1 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "This", "is a", "list", "of strings" }).Select(FScheme.Value.NewString)));

            var list = new List<FScheme.Value>() { nestedList1 };
            var innerFList = FScheme.Value.NewList(Utils.ToFSharpList(list));
            var innerList = new List<FScheme.Value>() { innerFList };
            var val = FScheme.Value.NewList(Utils.ToFSharpList(innerList));

            var res = NodeModel.PrintValue(val, 0, 3, 0, 4);
            Assert.AreEqual("List\n" +
                            "  List\n" +
                            "    List\n" +
                            "      \"This\"\n" +
                            "      \"is a\"\n" +
                            "      \"list\"\n" +
                            "      ..."
                            , res);
        }

        [Test]
        public void BuildValueStringIsCorrectFor3DListOfStringsWithArray3Recurse4()
        {
            var nestedList1 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "This", "is a", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList2 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Another", "list", "of strings" }).Select(FScheme.Value.NewString)));
            var nestedList3 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<string>() { "Finally", "a", "list", "of strings" }).Select(FScheme.Value.NewString)));


            var list = new List<FScheme.Value>() { nestedList1, nestedList2, nestedList3 };
            var innerFList = FScheme.Value.NewList(Utils.ToFSharpList(list));
            var innerList = new List<FScheme.Value>() { innerFList };
            var val = FScheme.Value.NewList(Utils.ToFSharpList(innerList));

            var res = NodeModel.PrintValue(val, 0, 3, 0, 4);
            Assert.AreEqual("List\n" +
                            "  List\n" +
                            "    List\n"+
                            "      \"This\"\n" +
                            "      \"is a\"\n" +
                            "      \"list\"\n" +
                            "      ...\n" +
                            "    List\n" +
                            "      \"Another\"\n" +
                            "      \"list\"\n" +
                            "      \"of strings\"\n" +
                            "    List\n" +
                            "      \"Finally\"\n" +
                            "      \"a\"\n" +
                            "      \"list\"\n" +
                            "      ..."
                            , res);
        }

        [Test]
        public void BuildValueStringIsCorrectFor3DListOfDoublesWithArray3Recurse4()
        {
            var nestedList1 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<double>() { 1, 5.2, 2, 3910 }).Select(FScheme.Value.NewNumber)));
            var nestedList2 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<double>() { 1, -3e5, 2, -5, 1 }).Select(FScheme.Value.NewNumber)));
            var nestedList3 =
                FScheme.Value.NewList(
                    Utils.ToFSharpList(
                        (new List<double>() { 1, 2 }).Select(FScheme.Value.NewNumber)));


            var list = new List<FScheme.Value>() { nestedList1, nestedList2, nestedList3 };
            var innerFList = FScheme.Value.NewList(Utils.ToFSharpList(list));
            var innerList = new List<FScheme.Value>() { innerFList };
            var val = FScheme.Value.NewList(Utils.ToFSharpList(innerList));

            var res = NodeModel.PrintValue(val, 0, 3, 0, 4);
            Assert.AreEqual("List\n" +
                            "  List\n" +
                            "    List\n" +
                            "      1\n" +
                            "      5.2\n" +
                            "      2\n" +
                            "      ...\n" +
                            "    List\n" +
                            "      1\n" +
                            "      -300000\n" +
                            "      2\n" +
                            "      ...\n" +
                            "    List\n" +
                            "      1\n" +
                            "      2"
                            , res);
        }

        [Test]
        public void BuildValueStringAcceptsNullInput()
        {
            FScheme.Value value = null;
            var res = NodeModel.PrintValue(value, 0, 3, 0, 4);
            Assert.AreEqual("<null>",res);
        }

        [Test]
        public void BuildValueStringAcceptsEmptyContainerInput()
        {
            var res = NodeModel.PrintValue(FScheme.Value.NewContainer(null), 0, 3, 0, 4);
            Assert.AreEqual("<empty>", res);
        }

    }
}
