using System;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    [TestFixture]
    class TypeSystemTests
    {
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public void ArrayConvTest()
        {
            String code =
                @"def foo:int[](){ return = [3.5]; }a=foo();";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 4 });
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RedefConvTest()
        {
            String code =
                @"    def foo()    {        x:int = 3.2;          return = x;    }v=foo();";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("v", 3);
        }


        [Test]
        public void RetArrayTest()
        {
            //DNL-1467221 Sprint 26 - Rev 3345 type conversion to array as return type does not get converted
            String code =
                @"         def foo3 : int[] ( a : double )         {            return = a;         }                 dummyArg = 1.5;                b2 = foo3 ( dummyArg ); ";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", new object[] { 2 });
        }

        [Test]
        public void RetArrayTest2()
        {
            //DNL-1467221 Sprint 26 - Rev 3345 type conversion to array as return type does not get converted
            String code =
                @"         def foo3 : double[] ( a : double )         {            return = a;         }                 dummyArg = 1.5;                b2 = foo3 ( dummyArg ); ";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", new object[] { 1.5 });
        }

        [Test]
        public void StatementArrayTest()
        {
            //DNL-1467221 Sprint 26 - Rev 3345 type conversion to array as return type does not get converted
            String code =
                @"a;[Associative]{ a : int[] = 1.5;}";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 2 });
        }

        [Test]
        public void StatementArrayTest2()
        {
            //DNL-1467221 Sprint 26 - Rev 3345 type conversion to array as return type does not get converted
            String code =
                @"a;[Associative]{ a : double[] = 1.5;}";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1.5 });
        }

        [Test]
        public void Rep1()
        {
            String code =
                @"def foo(){ return = 3.5; }a=foo();";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3.5);
        }

        [Test]
        public void Rep2()
        {
            String code =
                @"def foo(i:int){ return = 3.5; }a=foo(3);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3.5);
        }

        [Test]
        public void Rep3()
        {
            String code =
                @"def foo(i:int){ return = 3.5; }a=foo([0, 1]);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 3.5, 3.5 });
        }

        [Test]
        public void Rep4()
        {
            String code =
                @"def foo(i:int){ return = 3.5; }a=foo([[0, 1]]);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { new object[] { 3.5, 3.5 } });
        }

        [Test]
        public void Rep5()
        {
            //Assert.Fail("DNL-1467183 Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            String code =
                @"def foo(i:int){ return = 3.5; }a=foo([[0, 1], 1]);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { new object[] { 3.5, 3.5 }, 3.5 });
        }

        [Test]
        public void MinimalStringTest()
        {

            String code =
                @"a = ""Callsite is an angry bird"";b ="""";";
            var mirror = thisTest.RunScriptSource(code);
            StackValue sv = mirror.GetRawFirstValue("a");
            StackValue svb = mirror.GetRawFirstValue("b");
            thisTest.Verify("a", "Callsite is an angry bird");
        }

        [Test]
        public void SimpleUpCast()
        {
            String code =
                @"def foo(x:int[]){    return = x;}r = foo(1);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new Object[] { 1 });
        }

        [Test]
        public void TypedAssign()
        {
            String code =
                @"x : int = 2.3;";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2);
        }

        [Test]
        public void TestVarUpcast()
        {
            string code =
                @"x : var[] = 3;";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 3 });
        }

        [Test]
        public void TestVarDispatch()
        {
            string code =
                @"def foo (x : var[]){return=x;}y = foo(3);z = foo([3]);z1 = foo([[3]]);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", new object[] { 3 });
            thisTest.Verify("z", new object[] { 3 });
            thisTest.Verify("z1", new object[] { new object[] { 3 } });
        }

        [Test]
        public void TestIntDispatch()
        {
            string code =
                @"def foo (x : int[]){return=x;}y = foo(3);z = foo([3]);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", new object[] { 3 });
            thisTest.Verify("z", new object[] { 3 });
        }

        [Test]
        public void TestVarDispatchOnArrayStructure()
        {
            string code =
                @"                    def foo (x : var[][])                    {                    return=x;                    }                    y = foo(3);                    z = foo([3]);";
            string error = "1467326 - Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("y", new object[] { new object[] { 3 } });
            thisTest.Verify("z", new object[] { new object[] { 3 } });
        }

        [Test]
        public void TestVarDispatchOnArrayStructure2()
        {
            string code =
                @"                    def foo (x : var[][][])                    {                    return=x;                    }                    y = foo(3);                    z = foo([3]);                    z2 = foo([[3]]);                    z3 = foo([[[3]]]);                    ";
            string error = "1467326 - Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("y", new object[] { new object[] { new object[] { 3 } } });
            thisTest.Verify("z", new object[] { new object[] { new object[] { 3 } } });
            thisTest.Verify("z2", new object[] { new object[] { new object[] { 3 } } });
            thisTest.Verify("z3", new object[] { new object[] { new object[] { 3 } } });
        }

        [Test]
        public void TestIntDispatchOnArrayStructure()
        {
            string code =
                @"                    def foo (x : int[][])                    {                    return=x;                    }                    y = foo(3);                    z = foo([3]);";
            string error = "1467326 - Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("y", new object[] { new object[] { 3 } });
            thisTest.Verify("z", new object[] { new object[] { 3 } });
        }

        [Test]
        public void TestIntDispatchRetOnArrayStructure()
        {
            string code =
                @"                    def foo (x : int[][])                    {                    return=1;                    }                    //y = foo(3);                    z = foo([3]);                    z1 = foo([[3]]);";
            string error = "1467326 - Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            //thisTest.Verify(mirror, "y", 1);
            thisTest.Verify("z", 1);
            thisTest.Verify("z1", 1);
        }

        [Test]
        [Category("Failure")]
        public void TestIntSetOnArrayStructure()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668
            string code =
                @"                    x:int[][] = 3;                    y:int[][] = [3];                    z:int[][] = [[3]];                    z1:int[][] = [[[3]]];";
            string error = "MAGN-1668 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("x", new object[] { new object[] { 3 } });
            thisTest.Verify("y", new object[] { new object[] { 3 } });
            thisTest.Verify("z", new object[] { new object[] { 3 } });
            thisTest.Verify("z1", null);
        }

        [Test]
        public void TestIntDispatchOnArrayStructure2()
        {
            string code =
                @"                    def foo (x : int[][][])                    {                    return=x;                    }                    y = foo(3);                    z = foo([3]);                    z2 = foo([[3]]);                    z3 = foo([[[3]]]);                    ";
            string error = "1467326 - Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected ";
            var mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("y", new object[] { new object[] { new object[] { 3 } } });
            thisTest.Verify("z", new object[] { new object[] { new object[] { 3 } } });
            thisTest.Verify("z2", new object[] { new object[] { new object[] { 3 } } });
            thisTest.Verify("z3", new object[] { new object[] { new object[] { 3 } } });
        }


        [Test]
        public void TestVarReturnOnArrayStructure()
        {
            string code =
                @"def foo : var[] (x){return=x;}y = foo(3);";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", new object[] { 3 });
        }

        [Test]
        public void TestArbitraryRankArr()
        {
            string code =
                @"a:int[] =  3 ;b:int[]..[] =  3 ;y:int[] = [ 3 ];z:int[]..[] = [ 3 ];";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 3 });
            thisTest.Verify("b", 3);
            thisTest.Verify("y", new object[] { 3 });
            thisTest.Verify("z", new object[] { 3 });
        }

        [Test]
        public void TestAssignFailDueToRank()
        {
            string code =
                @"a:int = [3];";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
        }

    }
}

