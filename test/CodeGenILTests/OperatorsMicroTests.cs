using System.Linq;
using NUnit.Framework;
using ProtoCore.Utils;

namespace CodeGenILTests
{
    [TestFixture]
    public class OperatorsMicroTests : MicroTests
    {
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
            Assert.IsInstanceOf<long>(results[0]);
            Assert.AreEqual(3, results[0]);
            Assert.IsInstanceOf<double>(results[1]);
            Assert.AreEqual(3.5, results[1]);
            Assert.IsInstanceOf<double>(results[2]);
            Assert.AreEqual(3.5, results[2]);
            Assert.IsInstanceOf<double>(results[3]);
            Assert.AreEqual(3.8, results[3]);
            Assert.IsInstanceOf<string>(results[4]);
            Assert.AreEqual("abcdef", results[4]);
            Assert.IsInstanceOf<string>(results[5]);
            Assert.AreEqual("abc2.5", results[5]);
            Assert.IsNull(results[6]);
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
            Assert.IsInstanceOf<long>(results[0]);
            Assert.AreEqual(-1, results[0]);
            Assert.IsInstanceOf<double>(results[1]);
            Assert.AreEqual(-0.5, results[1]);
            Assert.IsInstanceOf<double>(results[2]);
            Assert.AreEqual(-1.5, results[2]);
            Assert.IsInstanceOf<double>(results[3]);
            Assert.AreEqual(-1.2, results[3]);
            Assert.IsNull(results[4]);
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
            Assert.IsInstanceOf<long>(results[0]);
            Assert.AreEqual(2, results[0]);
            Assert.IsInstanceOf<double>(results[1]);
            Assert.AreEqual(3.0, results[1]);
            Assert.IsInstanceOf<double>(results[2]);
            Assert.AreEqual(2.5, results[2]);
            Assert.IsInstanceOf<double>(results[3]);
            Assert.AreEqual(3.25, results[3]);
            Assert.IsNull(results[4]);
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
            Assert.IsInstanceOf<double>(results[0]);
            Assert.AreEqual(0.5, results[0]);
            Assert.IsInstanceOf<double>(results[1]);
            Assert.AreEqual(0.75, results[1]);
            Assert.IsInstanceOf<double>(results[2]);
            Assert.AreEqual(0.4, results[2]);
            Assert.IsInstanceOf<double>(results[3]);
            Assert.AreEqual(0.52, results[3]);
            Assert.IsNull(results[4]);
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
            Assert.IsInstanceOf<long>(results[0]);
            Assert.AreEqual(1, results[0]);
            Assert.IsInstanceOf<double>(results[1]);
            Assert.AreEqual(0.5, results[1]);
            Assert.IsInstanceOf<double>(results[2]);
            Assert.AreEqual(-1.5, results[2]);
            Assert.IsInstanceOf<double>(results[3]);
            Assert.AreEqual(1.3, results[3]);
            Assert.IsNull(results[4]);
        }
        [Test]
        //TODO_MSIL - this test fails when enabling direct function call validation in ReplicationLogic
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
            Assert.IsInstanceOf<long>(results[2]);
            Assert.AreEqual(-1, results[2]);
            Assert.IsInstanceOf<double>(results[3]);
            Assert.AreEqual(1.0, results[3]);
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
            Assert.IsInstanceOf<bool>(results[0]);
            Assert.AreEqual(true, results[0]);
            Assert.IsInstanceOf<bool>(results[1]);
            Assert.AreEqual(true, results[1]);
            Assert.IsInstanceOf<bool>(results[2]);
            Assert.AreEqual(false, results[2]);
            Assert.IsInstanceOf<bool>(results[3]);
            Assert.AreEqual(true, results[3]);
            Assert.IsInstanceOf<bool>(results[4]);
            Assert.AreEqual(true, results[4]);
            Assert.IsInstanceOf<bool>(results[5]);
            Assert.AreEqual(false, results[5]);
            Assert.IsInstanceOf<bool>(results[6]);
            Assert.AreEqual(true, results[6]);
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
            Assert.IsInstanceOf<bool>(results[0]);
            Assert.AreEqual(false, results[0]);
            Assert.IsInstanceOf<bool>(results[1]);
            Assert.AreEqual(true, results[1]);
            Assert.IsInstanceOf<bool>(results[2]);
            Assert.AreEqual(true, results[2]);
            Assert.IsInstanceOf<bool>(results[3]);
            Assert.AreEqual(false, results[3]);
            Assert.IsInstanceOf<bool>(results[4]);
            Assert.AreEqual(false, results[4]);
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
            Assert.IsInstanceOf<bool>(results[0]);
            Assert.AreEqual(true, results[0]);
            Assert.IsInstanceOf<bool>(results[1]);
            Assert.AreEqual(false, results[1]);
            Assert.IsInstanceOf<bool>(results[2]);
            Assert.AreEqual(false, results[2]);
            Assert.IsInstanceOf<bool>(results[3]);
            Assert.AreEqual(true, results[3]);
            Assert.IsInstanceOf<bool>(results[4]);
            Assert.AreEqual(true, results[4]);
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
            Assert.IsInstanceOf<bool>(results[0]);
            Assert.AreEqual(false, results[0]);
            Assert.IsInstanceOf<bool>(results[1]);
            Assert.AreEqual(false, results[1]);
            Assert.IsInstanceOf<bool>(results[2]);
            Assert.AreEqual(false, results[2]);
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
            Assert.IsInstanceOf<bool>(results[0]);
            Assert.AreEqual(true, results[0]);
            Assert.IsInstanceOf<bool>(results[1]);
            Assert.AreEqual(true, results[1]);
            Assert.IsInstanceOf<bool>(results[2]);
            Assert.AreEqual(true, results[2]);
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
            Assert.IsInstanceOf<bool>(results[0]);
            Assert.AreEqual(false, results[0]);
            Assert.IsInstanceOf<bool>(results[1]);
            Assert.AreEqual(false, results[1]);
            Assert.IsInstanceOf<bool>(results[2]);
            Assert.AreEqual(true, results[2]);
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
            Assert.IsInstanceOf<bool>(results[0]);
            Assert.AreEqual(true, results[0]);
            Assert.IsInstanceOf<bool>(results[1]);
            Assert.AreEqual(true, results[1]);
            Assert.IsInstanceOf<bool>(results[2]);
            Assert.AreEqual(true, results[2]);
        }
    }
}
