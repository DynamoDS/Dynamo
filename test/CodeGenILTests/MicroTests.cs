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
    [TestFixture]
    public class MicroTests
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
            CollectionAssert.AreEqual(new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10 }, output.Values.ToList().First()[0] as long[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 2, 4, 6, 8, 10 }, output.Values.ToList().First()[0] as long[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 2, 4,6,8,10 }, output.Values.ToList().First()[0] as double[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 5, 10, 15, 20, 25,30,35, 40,45 }, output.Values.ToList().First()[0] as long[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45 }, output.Values.ToList().First()[0] as long[]);
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
            CollectionAssert.AreEqual(new object[] { 0, .1, .2, .3, .4, .5, .6, .7, .8, .9, 1.0,1.1 }, output.Values.ToList().First()[0] as double[]);
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
                Assert.AreEqual(null, output.Values.ToList().First()[0]);

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
            CollectionAssert.AreEqual(new object[] { 0,2.5,5,7.5,10 }, output.Values.ToList().First()[0] as double[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 2.5, 5, 7.5, 10 }, output.Values.ToList().First()[0] as double[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 10 }, output.Values.ToList().First()[0] as long[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 10d/3d, (10d / 3d)*2, 10 }, output.Values.ToList().First()[0] as double[]);
        }


        [Category("Failure")]
        [Test]
        //'Object of type 'System.int64[]' cannot be converted to type 'System.Collections.Generic.IEnumerable`1[System.Double]'.'
        public void SumIntRange_TypeCoerNeeded()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
x = 0..10;
y = DSCore.Math.Sum(x);";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(55, output.Values.ToList().First()[0]);
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
            CollectionAssert.AreEqual(new object[] { 0,1,2,3,4,5,6,7,8,9,10 }, output.Values.ToList()[3][0] as long[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, output.Values.ToList()[3][0] as double[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, output.Values.ToList()[3][0] as double[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 2.5, 5, 7.5, 10 }, output.Values.ToList()[3][0] as double[]);
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
            CollectionAssert.AreEqual(new object[] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45 }, output.Values.ToList()[3][0] as double[]);
        }

        // Operator tests
        [Test]
        public void Add_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1+2;
b=1.5+2;
c=1+2.5;
d=1.3+2.5;
e=""abc""+""def"";
f=""abc""+2.5;
g='a'+5;
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(7, results.Count);
            Assert.IsInstanceOf<long>(results[0][0]);
            Assert.AreEqual(3, results[0][0]);
            Assert.IsInstanceOf<double>(results[1][0]);
            Assert.AreEqual(3.5, results[1][0]);
            Assert.IsInstanceOf<double>(results[2][0]);
            Assert.AreEqual(3.5, results[2][0]);
            Assert.IsInstanceOf<double>(results[3][0]);
            Assert.AreEqual(3.8, results[3][0]);
            Assert.IsInstanceOf<string>(results[4][0]);
            Assert.AreEqual("abcdef", results[4][0]);
            Assert.IsInstanceOf<string>(results[5][0]);
            Assert.AreEqual("abc2.5", results[5][0]);
            Assert.IsNull(results[6][0]);
        }
        [Test]
        public void Sub_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1-2;
b=1.5-2;
c=1-2.5;
d=1.3-2.5;
g='a'-5;
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsInstanceOf<long>(results[0][0]);
            Assert.AreEqual(-1, results[0][0]);
            Assert.IsInstanceOf<double>(results[1][0]);
            Assert.AreEqual(-0.5, results[1][0]);
            Assert.IsInstanceOf<double>(results[2][0]);
            Assert.AreEqual(-1.5, results[2][0]);
            Assert.IsInstanceOf<double>(results[3][0]);
            Assert.AreEqual(-1.2, results[3][0]);
            Assert.IsNull(results[4][0]);
        }
        [Test]
        public void Mul_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1*2;
b=1.5*2;
c=1*2.5;
d=1.3*2.5;
e=1.5+'c';
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsInstanceOf<long>(results[0][0]);
            Assert.AreEqual(2, results[0][0]);
            Assert.IsInstanceOf<double>(results[1][0]);
            Assert.AreEqual(3.0, results[1][0]);
            Assert.IsInstanceOf<double>(results[2][0]);
            Assert.AreEqual(2.5, results[2][0]);
            Assert.IsInstanceOf<double>(results[3][0]);
            Assert.AreEqual(3.25, results[3][0]);
            Assert.IsNull(results[4][0]);
        }
        [Test]
        public void Div_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1/2;
b=1.5/2;
c=1/2.5;
d=1.3/2.5;
e=1.3/'c';
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsInstanceOf<double>(results[0][0]);
            Assert.AreEqual(0.5, results[0][0]);
            Assert.IsInstanceOf<double>(results[1][0]);
            Assert.AreEqual(0.75, results[1][0]);
            Assert.IsInstanceOf<double>(results[2][0]);
            Assert.AreEqual(0.4, results[2][0]);
            Assert.IsInstanceOf<double>(results[3][0]);
            Assert.AreEqual(0.52, results[3][0]);
            Assert.IsNull(results[4][0]);
        }
        [Test]
        public void Mod_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1%2;
b=-1.5%2;
c=1%-2.5;
d=1.3%2.5;
e=1.5%'a';
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsInstanceOf<long>(results[0][0]);
            Assert.AreEqual(1, results[0][0]);
            Assert.IsInstanceOf<double>(results[1][0]);
            Assert.AreEqual(0.5, results[1][0]);
            Assert.IsInstanceOf<double>(results[2][0]);
            Assert.AreEqual(-1.5, results[2][0]);
            Assert.IsInstanceOf<double>(results[3][0]);
            Assert.AreEqual(1.3, results[3][0]);
            Assert.IsNull(results[4][0]);
        }
        [Test]
        public void Neg_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1;
b=-1.0;
c=-a;
d=-b;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(4, results.Count);
            Assert.IsInstanceOf<long>(results[2][0]);
            Assert.AreEqual(-1, results[2][0]);
            Assert.IsInstanceOf<double>(results[3][0]);
            Assert.AreEqual(1.0, results[3][0]);
        }
        [Test]
        public void ToBooleanConversions()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=true==true;
b=true==2;
c=true==null;
d=true==2.5;
e=""true""==true;
f=""""==true;
g='t'==true;
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(7, results.Count);
            Assert.IsInstanceOf<bool>(results[0][0]);
            Assert.AreEqual(true, results[0][0]);
            Assert.IsInstanceOf<bool>(results[1][0]);
            Assert.AreEqual(true, results[1][0]);
            Assert.IsInstanceOf<bool>(results[2][0]);
            Assert.AreEqual(false, results[2][0]);
            Assert.IsInstanceOf<bool>(results[3][0]);
            Assert.AreEqual(true, results[3][0]);
            Assert.IsInstanceOf<bool>(results[4][0]);
            Assert.AreEqual(true, results[4][0]);
            Assert.IsInstanceOf<bool>(results[5][0]);
            Assert.AreEqual(false, results[5][0]);
            Assert.IsInstanceOf<bool>(results[6][0]);
            Assert.AreEqual(true, results[6][0]);
        }
        [Test]
        public void Eq_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=true==false;
b=3==3;
c=3.5==3.5;
d=3.5==3;
e=""abc""==""def"";
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsInstanceOf<bool>(results[0][0]);
            Assert.AreEqual(false, results[0][0]);
            Assert.IsInstanceOf<bool>(results[1][0]);
            Assert.AreEqual(true, results[1][0]);
            Assert.IsInstanceOf<bool>(results[2][0]);
            Assert.AreEqual(true, results[2][0]);
            Assert.IsInstanceOf<bool>(results[3][0]);
            Assert.AreEqual(false, results[3][0]);
            Assert.IsInstanceOf<bool>(results[4][0]);
            Assert.AreEqual(false, results[4][0]);
        }
        [Test]
        public void Nq_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=true!=false;
b=3!=3;
c=3.5!=3.5;
d=3.5!=3;
e=""abc""!=""def"";
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsInstanceOf<bool>(results[0][0]);
            Assert.AreEqual(true, results[0][0]);
            Assert.IsInstanceOf<bool>(results[1][0]);
            Assert.AreEqual(false, results[1][0]);
            Assert.IsInstanceOf<bool>(results[2][0]);
            Assert.AreEqual(false, results[2][0]);
            Assert.IsInstanceOf<bool>(results[3][0]);
            Assert.AreEqual(true, results[3][0]);
            Assert.IsInstanceOf<bool>(results[4][0]);
            Assert.AreEqual(true, results[4][0]);
        }
        [Test]
        public void Gt_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1>2;
b=1.0>2;
c=1.0>2.0;
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(3, results.Count);
            Assert.IsInstanceOf<bool>(results[0][0]);
            Assert.AreEqual(false, results[0][0]);
            Assert.IsInstanceOf<bool>(results[1][0]);
            Assert.AreEqual(false, results[1][0]);
            Assert.IsInstanceOf<bool>(results[2][0]);
            Assert.AreEqual(false, results[2][0]);
        }
        [Test]
        public void Lt_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1<2;
b=1.0<2;
c=1.0<2.0;
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(3, results.Count);
            Assert.IsInstanceOf<bool>(results[0][0]);
            Assert.AreEqual(true, results[0][0]);
            Assert.IsInstanceOf<bool>(results[1][0]);
            Assert.AreEqual(true, results[1][0]);
            Assert.IsInstanceOf<bool>(results[2][0]);
            Assert.AreEqual(true, results[2][0]);
        }
        [Test]
        public void Ge_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1>=2;
b=1.0>=2;
c=1.0>=1.0;
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(3, results.Count);
            Assert.IsInstanceOf<bool>(results[0][0]);
            Assert.AreEqual(false, results[0][0]);
            Assert.IsInstanceOf<bool>(results[1][0]);
            Assert.AreEqual(false, results[1][0]);
            Assert.IsInstanceOf<bool>(results[2][0]);
            Assert.AreEqual(true, results[2][0]);
        }
        [Test]
        public void Le_Operator()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=1<=2;
b=1.0<=2;
c=1.0<=1.0;
";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            var results = output.Values.ToList();
            Assert.AreEqual(3, results.Count);
            Assert.IsInstanceOf<bool>(results[0][0]);
            Assert.AreEqual(true, results[0][0]);
            Assert.IsInstanceOf<bool>(results[1][0]);
            Assert.AreEqual(true, results[1][0]);
            Assert.IsInstanceOf<bool>(results[2][0]);
            Assert.AreEqual(true, results[2][0]);
        }
        #endregion
    }
}
