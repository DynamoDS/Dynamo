using NUnit.Framework;
using ProtoCore.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System;

namespace CodeGenILTests
{
    public class MicroTests
    {
        protected EmitMSIL.CodeGenIL codeGen;
        protected Dictionary<string, IList> inputs = new Dictionary<string, IList>();

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
    }

    [TestFixture]
    public class RangeMicroTests : MicroTests
    {
    #region constants

        [Test]
        public void RangeTestInts_nullstep()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..10;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10 }, output.Values.ToList().First() as long[]);
        }
        [Test]
        public void RangeTestIntStep_ints()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..10..2;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 2, 4, 6, 8, 10 }, output.Values.ToList().First() as long[]);
        }
        [Test]
        public void RangeTestDoubleStep_doubles()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..10..2.0;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 2, 4,6,8,10 }, output.Values.ToList().First() as double[]);
        }

        [Test]
        public void RangeTestAmountByStep_DoubleStep_IntRange()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..#10..5.0;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 5, 10, 15, 20, 25,30,35, 40,45 }, output.Values.ToList().First() as long[]);
        }
        [Test]
        public void RangeTestAmountByStep_IntStep_IntRange()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..#10..5;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45 }, output.Values.ToList().First() as long[]);
        }

        [Test]
        public void RangeTestDouble()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..1.1..0.1;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, .1, .2, .3, .4, .5, .6, .7, .8, .9, 1.0,1.1 }, output.Values.ToList().First() as double[]);
        }

        [Test]
        public void DoubleAmountThrows()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..10..#5.0;";

            var ast = ParserUtils.Parse(dscode).Body;
            Assert.Throws<ArgumentException>(() =>
            {

                var output = codeGen.EmitAndExecute(ast);
                Assert.IsNotEmpty(output);
                Assert.AreEqual(null, output.Values.ToList().First());

            });
            
           
        }

        [Test]
        public void RangeAmountDoubles()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0.0..10.0..#5;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0,2.5,5,7.5,10 }, output.Values.ToList().First() as double[]);
        }

        [Test]
        public void RangeAmountInts_returnsDoubles()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..10..#5;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 2.5, 5, 7.5, 10 }, output.Values.ToList().First() as double[]);
        }

        [Test]
        public void RangeAmountInts_returnsInts()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..10..#2;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 10 }, output.Values.ToList().First() as long[]);
        }

       
        [Test]
        public void RangeApproximateStep_Doubles()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
0..10..~3;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            //irrational numbers! 0,3.33333,6.66666,10
            CollectionAssert.AreEqual(new object[] { 0, 10d/3d, (10d / 3d)*2, 10 }, output.Values.ToList().First() as double[]);
        }


        [Test]
        //'Object of type 'System.int64[]' can be converted to type 'System.Collections.Generic.IEnumerable`1[System.Double]'.'
        public void SumIntRange_TypeCoerNeeded()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
x = 0..10;
y = DSCore.Math.Sum(x);";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(55, output.Values.ToList()[1][0]);
        }

        [Test]
        //'Object of type 'System.int64[]' can be converted to type 'System.Collections.Generic.IEnumerable`1[System.Double]'.'
        public void SumIntArray_TypeCoerNeeded()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
y = DSCore.Math.Sum([1,2,3]);";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(6, output["y"][0]);
        }

        [Test]
        //'Object of type 'IList' can be converted to type 'System.Collections.Generic.IEnumerable`1[System.Double]'.'
        public void SumIntTempRange_TypeCoerNeeded()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
y = DSCore.Math.Sum(0..10);";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(55, output["y"][0]);
        }

        [Test]
        public void Coerce_MultipleCalls()
        {
            var code = @"
import(""DesignScriptBuiltin.dll"");
import(""DSCoreNodes.dll"");
x = [0,1,2,3];
y = [0,1];
test2 = DSCore.List.RemoveItemAtIndex (x, y);
test3 = DSCore.List.RemoveItemAtIndex (x, y);";

            var ast = ParserUtils.Parse(code).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(new long[] { 2, 3 }, output["test2"]);
            Assert.AreEqual(new long[] { 2, 3 }, output["test3"]);
        }

        #endregion
        #region identifers
        [Test]
        public void Range_step_Ints_Idents()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
fr = 0;
to = 10;
step = 1;
fr..to..step;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0,1,2,3,4,5,6,7,8,9,10 }, output.Values.ToList()[3] as long[]);
        }
        [Test]
        public void Range_step_Doubles_Idents()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
fr = 0.0;
to = 10.0;
step = 1.0;
fr..to..step;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, output.Values.ToList()[3] as double[]);
        }

        [Test]
        public void Range_step_Doubles_DifferentTypes_idents()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
fr = 0.0;
to = 10;
step = 1.0;
fr..to..step;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, output.Values.ToList()[3] as double[]);
        }
        [Test]
        public void Range_amount_Doubles_idents()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
fr = 0;
to = 10;
step = 5;
fr..to..#step;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 2.5, 5, 7.5, 10 }, output.Values.ToList()[3] as double[]);
        }
        //NOTE that is test illustrates the difference between the from..#amount..step range form where step is a double that 
        //can be represnted by an int - in the old vm and if we know the types we can create an int range - but when using identifers, we
        //just return a double range because we can't figure it out ahead of time without knowing the input values.
        [Test]
        public void Range_Number_Step_IntAndDouble_DoubleRange_Idents()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
fr = 0;
to = 10;
step = 5.0;
fr..#to..step;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45 }, output.Values.ToList()[3] as double[]);
        }
        #endregion
    }
}
