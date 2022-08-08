using NUnit.Framework;
using ProtoCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenILTests
{
    class ReplicationTests
    {
        private EmitMSIL.CodeGenIL codeGen;
        private Dictionary<string, IList> inputs = new Dictionary<string, IList>();

        [SetUp]
        public void Setup()
        {
            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            codeGen = new EmitMSIL.CodeGenIL(inputs, Path.Combine(assemblyPath, "OpCodesTEST.txt"));
        }

        [TearDown]
        public void TearDown()
        {
            codeGen.Dispose();
        }

        [Test]
        public void MSIL_Arithmatic_List_And_List_Same_Length()
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

        [Test]
        public void MSIL_Arithmatic_No_Replication()
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

            var result = (double)output["list"][0];
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
            var bR = a as FFITarget.ReplicationTestA;
            if (aR != null && bR != null)
            {
                return aR.X == bR.X &&
                       aR.Y == bR.Y &&
                       aR.Z == bR.Z;
            }
            return false;
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

            Assert.IsTrue(AreEqual(
                new Object[] { new FFITarget.ReplicationTestA() { X = 0, Y = 0, Z = 0 } },
                output["test2"]));

            Assert.IsTrue(AreEqual(
                new Object[] {
                    new Object[] {
                        new Object[] {
                            new FFITarget.ReplicationTestA() { X = 0, Y = 2, Z = 4 },
                            new FFITarget.ReplicationTestA() { X = 1, Y = 2, Z = 5 }
                        },
                        new Object[] {
                            new FFITarget.ReplicationTestA() { X = 0, Y = 3, Z = 4 },
                            new FFITarget.ReplicationTestA() { X = 1, Y = 3, Z = 5 }
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
