using NUnit.Framework;
using ProtoCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenILTests
{
    class ReplicationTests : MicroTests
    {
        [Test]
        public void MSIL_Arithmetic_List_And_List_Same_Length()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([ 1, 4, 7, 2], [ 5, 8, 3, 6 ]);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[] { 5, 8, 7, 6 };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        #region longest
        [Test]
        public void ZTLongestLacing_ShouldReturn4Results()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max(10, [ 5, 8, 3, 6 ]<1L>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[] { 10,10,10,10};
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void ZTLongestLacing_ShouldReturn4NestedResults_WithGuide()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10], [ 5, 8, 3, 6 ]<1L>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new object[] { new[] { 10 }, new[] { 10 }, new[] { 10 }, new[] { 10 } };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void ZTLongestLacing_ShouldReturn4NestedResults_WithHigherGuideOnListOfSingleItem1()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10]<2>, [ 5, 8, 3, 6 ]<1L>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new object[] { new[] { 10 }, new[] { 10 }, new[] { 10 }, new[] { 10 } };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void ZTLongestLacing_ShouldReturn4NestedResults_WithHigherGuideOnListOfSingleItem2()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10]<2>, [ 5, 8, 3, 6 ]<1>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new object[] { new[] { 10 }, new[] { 10 }, new[] { 10 }, new[] { 10 } };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void ZTLongestLacing_ShouldReturn4Results_WithoutGuide()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max(10, [ 5, 8, 3, 6 ]);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[] { 10, 10, 10, 10 };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        [Category("Failure")]
        public void ZTLongestLacing_ShouldReturn4Results_WithHigherGuideOnList()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10]<1>, [ 5, 8, 3, 6 ]<2>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[] { 10, 10, 10, 10 };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        [Category("Failure")]
        public void ZTLongestLacing_ShouldReturn3Lists_2Guides()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10,3]<1L>, [[1,1],[2,2,2],[4,4,4,4]]<1>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[][] {
                new[] { 10, 10 },
                new int[] { 3, 3, 3 },
                new int[] { 4, 4, 4, 4 } };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        [Category("Failure")]
        public void ZTLongestLacing_ShouldReturn3Lists_2Guides2()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10,3]<1>, [[1,1],[2,2,2],[4,4,4,4]]<1L>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[][] {
                new[] { 10, 10 },
                new int[] { 3, 3, 3 },
                new int[] { 4, 4, 4, 4 } };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void ZTLongestLacing_ShouldReturn6Lists_2Guides()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10,3]<2>, [[1,1],[2,2,2],[4,4,4,4]]<1>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[][][] {
                new []{new[] { 10, 10 },
                new int[] { 3, 3 } },

                 new []{new[] { 10, 10,10 },
                new int[] { 3, 3, 3 } },

                  new []{new[] { 10, 10,10,10 },
                new int[] { 4, 4, 4,4 } } };

            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        #endregion
        #region shortest
        [Test]
        public void ZTShortestLacing_ShouldReturnSingleItem_AndBeDefault()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10], [ 5, 8, 3, 6 ]);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[] { 10 };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void ZTShortestLacing_ShouldReturnSingleItem_WithSameGuide()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10]<1>, [ 5, 8, 3, 6 ]<1>);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[] { 10 };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void ZTShortestLacing_ShouldReturn2Results_NoGuides()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10,3], [ 1,1,1,1]);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[] { 10,3 };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void ZTShortestLacing_ShouldReturn2Lists_NoGuides()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list3 = DSCore.Math.Max([10,3], [[1,1],[2,2,2],[4,4,4,4]]);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list3"));

            var expectedResult = new int[][] { new[] { 10,10 }, new int[] { 3, 3, 3 } };
            var result = output["list3"];
            Assert.AreEqual(expectedResult, result);
        }
        #endregion
        [Test]
        public void MSIL_Arithmetic_No_Replication()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list = DSCore.Math.Sum([ 1, 2, 3 ]);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list"));

            var result = (double)output["list"];
            Assert.AreEqual(6, result);
        }

        [Test]
        public void MSIL_List_No_Replication()
        {
            string dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
list = DSCore.List.Reverse([ 1, 2, 3 ]);
";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(output.ContainsKey("list"));

            var result = output["list"];
            Assert.AreEqual(new object[] { 3, 2, 1 }, result);
        }

        [Test]
        public void MSIL_Replication_BuiltinMethods()
        {
            string code =
            @" 
                import(""DesignScriptBuiltin.dll"");
                import(""DSCoreNodes.dll"");
                x = [0,1,2,3];
                y = [0,1];
                z = [ ""int"", ""double"" ];
                test1 = DSCore.List.Contains (x, y);
                test2 = DSCore.List.IndexOf (x, y);
                test3 = DSCore.List.RemoveItemAtIndex (x, y);
                test4 = DSCore.List.NormalizeDepth (x, y);
            ";

            var ast = ParserUtils.Parse(code).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.AreEqual(output["test1"], new Object[] { true, true });
            Assert.AreEqual(output["test2"], new Object[] { 0, 1 });
            Assert.AreEqual(output["test3"], new Object[] { 2, 3 });
            Assert.AreEqual(output["test4"], new Object[] { new Object[] { 0, 1, 2, 3 }, new Object[] { 0, 1, 2, 3 } });
        }

        [Test]
        public void MSIL_ReplicationGuides_BuiltinMethods()
        {
            string code =
            @" 
                import(""DesignScriptBuiltin.dll"");
                import(""DSCoreNodes.dll"");
                x = [[0,1],[2,3]];
                y = [0,1];
                z = [ ""int"", ""double"" ];
                test1 = DSCore.List.Contains(x<1>, y<2>);
                test2 = DSCore.List.IndexOf(x<1>, y<2>);
                test3 = DSCore.List.RemoveItemAtIndex(x<1>, y<2>);
                test4 = DSCore.List.Insert(x<1>, y<2>, y<2>);
                test5 = DSCore.List.NormalizeDepth(x<1>, y<2>);
            ";

            var ast = ParserUtils.Parse(code).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.AreEqual(output["test1"], new Object[] { new Object[] { true, true }, new Object[] { false, false } });
            Assert.AreEqual(output["test2"], new Object[] { new Object[] { 0, 1 }, new Object[] { -1, -1 } });
            Assert.AreEqual(output["test3"], new Object[] { new Object[] { new Object[] { 1 }, new Object[] { 0 } }, new Object[] { new Object[] { 3 }, new Object[] { 2 } } });
            Assert.AreEqual(output["test4"], new Object[] { new Object[] { new Object[] { 0, 0, 1 }, new Object[] { 0, 1, 1 } }, new Object[] { new Object[] { 0, 2, 3 }, new Object[] { 2, 1, 3 } } });
            Assert.AreEqual(output["test5"], new Object[] { new Object[] { new Object[] { 0, 1 }, new Object[] { 0, 1 } }, new Object[] { new Object[] { 2, 3 }, new Object[] { 2, 3 } } });
        }


        private bool AreEqual(object a, object b)
        {
            if (a == null && b == null) return true;

            if (a == null || b == null) return false;

            if (a.GetType() == typeof(int) && b.GetType() == typeof(int))
            {
                return (int)a == (int)b;
            }

            bool isAIEnum = typeof(IEnumerable<object>).IsAssignableFrom(a.GetType());
            bool isBIEnum = typeof(IEnumerable<object>).IsAssignableFrom(b.GetType());
            if (isAIEnum && isBIEnum)
            {
                var aIEnum = (a as IEnumerable<object>).ToList();
                var bIEnum = (b as IEnumerable<object>).ToList();

                if (aIEnum.Count() != bIEnum.Count()) 
                { 
                    return false; 
                }

                for (int ii=0; ii < aIEnum.Count(); ii++)
                {
                    if (!AreEqual(aIEnum[ii], bIEnum[ii]))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (isAIEnum || isBIEnum)
            {
                return false;
            }

            var aR = a as FFITarget.ReplicationTestA;
            var bR = b as FFITarget.ReplicationTestA;
            if (aR != null && bR != null)
            {
                return aR.X == bR.X &&
                       aR.Y == bR.Y &&
                       aR.Z == bR.Z;
            }
            return false;
        }

        [Test]
        public void CrossProductReplication_ResultPassedAs2DArray()
        {
            string code =
            @"
                import(""ProtoGeometry.dll"");
                import(""DesignScriptBuiltin.dll"");
                import(""DSCoreNodes.dll"");
                t1 = (1..3);
                point1 = Autodesk.DesignScript.Geometry.Point.ByCoordinates(t1<1>, t1<2>, 0);
                nurbsCurve1 = Autodesk.DesignScript.Geometry.NurbsCurve.ByPoints(point1);
            ";
            var ast = ParserUtils.Parse(code).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.AreEqual(new long[] { 1, 2, 3 }, output["t1"]);
            var pointGrid = output["point1"] as object[];
            Assert.NotNull(pointGrid);
            Assert.AreEqual(3, pointGrid.Length);
            foreach (var row in pointGrid)
            {
                var ptRow = row as object[];
                Assert.NotNull(ptRow);

                Assert.AreEqual(3, ptRow.Length);
                Assert.True(ptRow[0] is Autodesk.DesignScript.Geometry.Point);
                Assert.True(ptRow[1] is Autodesk.DesignScript.Geometry.Point);
                Assert.True(ptRow[2] is Autodesk.DesignScript.Geometry.Point);
            }
            var nurbs = output["nurbsCurve1"] as object[];
            Assert.NotNull(nurbs);

            Assert.AreEqual(3, nurbs.Length);
            foreach (var obj in nurbs)
            {
                Assert.True(obj is Autodesk.DesignScript.Geometry.NurbsCurve);
            }
        }

        [Test]
        public void MSIL_FuncCall_Double_SomeGuides()
        {
            string code =
            @"
            import(""DesignScriptBuiltin.dll"");
            import (""FFITarget.dll"");
            x = [0.0,1.0];
            y = [2.0,3.0];
            z = [4.0,5.0 ];
            test1 = FFITarget.ReplicationTestA.ReplicationTestA(x, y, z);  
            test2 = FFITarget.ReplicationTestA.ReplicationTestA(0,0,0);
            test3 = FFITarget.ReplicationTestA.ReplicationTestA(x<1>, y<2>, z); 
            test4 = FFITarget.ReplicationTestA.foo(x, y<2>, z);      
            ";

            var ast = ParserUtils.Parse(code).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);

            Assert.IsTrue(AreEqual(
                new Object[] {
                    new FFITarget.ReplicationTestA() { X = 0, Y = 2, Z = 4 },
                    new FFITarget.ReplicationTestA() { X = 1, Y = 3, Z = 5 }
                },
                output["test1"]));

            Assert.IsTrue(AreEqual(new FFITarget.ReplicationTestA() { X = 0, Y = 0, Z = 0 },
                output["test2"]));

            Assert.IsTrue(AreEqual(
                new Object[] {
                    new Object[] {
                        new Object[] {
                            new FFITarget.ReplicationTestA() { X = 0, Y = 2, Z = 4 },
                            new FFITarget.ReplicationTestA() { X = 0, Y = 2, Z = 5 }
                        },
                        new Object[] {
                            new FFITarget.ReplicationTestA() { X = 0, Y = 3, Z = 4 },
                            new FFITarget.ReplicationTestA() { X = 0, Y = 3, Z = 5 }
                        }
                    },
                    new Object[] {
                        new Object[] {
                            new FFITarget.ReplicationTestA() { X = 1, Y = 2, Z = 4 },
                            new FFITarget.ReplicationTestA() { X = 1, Y = 2, Z = 5 }
                        },
                        new Object[] {
                            new FFITarget.ReplicationTestA() { X = 1, Y = 3, Z = 4 },
                            new FFITarget.ReplicationTestA() { X = 1, Y = 3, Z = 5 }
                        }
                    },
                },
                output["test3"]));

            Assert.IsTrue(AreEqual(
                new Object[] {
                    new Object[] { 6, 8 },
                    new Object[] { 7, 9 }
                },
                output["test4"]));
        }
    }
}
