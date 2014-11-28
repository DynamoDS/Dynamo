using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Associative
{
    class Function : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T001_Associative_Function_Simple()
        {
            string code = @"
a;
b;
sum;
[Associative]
{
	def Sum : int(a : int, b : int)
	{
	
		return = a + b;
	}
	
	a = 1;
	b = 10;
	
	sum = Sum (a, b);
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 11);

        }

        [Test]
        //Function does not accept single line function / Direct Assignment
        [Category("SmokeTest")]
        public void T002_Associative_Function_SinglelineFunction()
        {
            string code = @"
d;
[Associative]
{
	def singleLine : int(a:int, b:int) = 10;
	d = singleLine(1,3);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 10);

        }

        [Test]
        [Category("SmokeTest")]
        public void T003_Associative_Function_MultilineFunction()
        {
            string code = @"
[Associative]
{
	def Divide : int(a:int, b:int)
	{
		return = a/b;
	}
	d = Divide (1,3);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("d").Payload) == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T004_Associative_Function_SpecifyReturnType()
        {
            string code = @"
d;
[Associative]
{
	def Divide : double (a:int, b:int)
	{
		return = a/b;
	}
	d = Divide (1,3);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 0.33333333333333333);
        }

        [Test]
        [Category("Type System")]
        public void T005_Associative_Function_SpecifyArgumentType()
        {
            string code = @"
result;
[Associative]
{
	def myFunction : int (a:int, b:int)
	{
		return = a + b;
	}
	d1 = 1.12;
	d2 = 0.5;
	
	result = myFunction (d1, d2);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 2);
        }

        [Test]
        //Function takes null as argument
        [Category("SmokeTest")]
        public void T006_Associative_Function_PassingNullAsArgument()
        {
            string code = @"
[Associative]
{
	def myFunction : double (a: double, b: double)
	{
		return = a + b;
	}
	d1 = null;
	d2 = 0.5;
	
	result = myFunction (d1, d2);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.IsTrue((Double)mirror.GetValue("d").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T007_Associative_Function_NestedFunction()
        {
            string code = @"
result;
[Associative]
{
	def ChildFunction : double (r1 : double)
	{
	return = r1;
	
	}
	def ParentFunction : double (r1 : double)
	{
		return = ChildFunction (r1)*2;
	}
	d1 = 1.05;
	
	result = ParentFunction (d1);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("result").Payload == 2.1);
        }

        [Test]
        //Function does not work if the argument variable is declared before function declaration
        [Category("SmokeTest")]
        public void T008_Associative_Function_DeclareVariableBeforeFunctionDeclaration()
        {
            string code = @"
sum;
[Associative]
{
    a = 1;
	b = 10;
	def Sum : int(a : int, b : int)
	{
	
		return = a + b;
	}
	
	sum = Sum (a, b);
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 11);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_Associative_Function_DeclareVariableInsideFunction()
        {
            string code = @"
[Associative]
{
	def Foo : int(input : int)
	{
		multiply = 5;
		divide = 10;
	
		return = {input*multiply, input/divide};
	}
	
	input = 20;
	sum = Foo (input);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verifiction should be done after collection is ready. 
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_Associative_Function_PassAndReturnBooleanValue()
        {
            string code = @"
result1;
result2;
[Associative]
{
	def Foo : bool (input : bool)
	{
		return = input;
	}
	
	input = false;
	result1 = Foo (input);
	result2 = Foo (true);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("result1").Payload) == false);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("result2").Payload));
        }

        [Test]
        [Category("SmokeTest")]
        public void T011_Associative_Function_FunctionWithoutArgument()
        {
            string code = @"
result1;
[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	result1 = Foo1 ();
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("result1").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_Associative_Function_MultipleFunctions()
        {
            string code = @"
result1;
result2;
[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	
	def Foo2 : int ()
	{
		return = 6;
	}
	
	
	result1 = Foo1 ();
	result2 = Foo2 ();
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("result1").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("result2").Payload == 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T013_Associative_Function_FunctionWithSameName_Negative()
        {
            string code = @"
[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	
	
	def Foo1 : int ()
	{
		return = 6;
	}
	
	
	
	result2 = Foo2 ();
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        //Should return compilation error if a variable has the same name as a function?
        [Category("SmokeTest")]
        public void T014_Associative_Function_DuplicateVariableAndFunctionName_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Associative]
{
	def Foo : int ()
	{
		return = 4;
	}
	Foo = 5;
	
	
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        //Incorrect error message when the argument number is not matching with function declaration. 
        [Category("SmokeTest")]
        public void T015_Associative_Function_UnmatchFunctionArgument_Negative()
        {

            string code = @"
[Associative]
{
	def Foo : int (a : int)
	{
		return = 5;
	}
	
	result = Foo(1,2); 
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("SmokeTest")]
        public void T016_Associative_Function_ModifyArgumentInsideFunctionDoesNotAffectItsValue()
        {
            string code = @"
input;
result;
originalInput;
[Associative]
{
	def Foo : int (a : int)
	{
		a = a + 1;
		return = a;
	}
	input = 3;
	result = Foo(input); 
	originalInput = input;
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("input").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("result").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("originalInput").Payload == 3);
        }

        [Test]
        //Calling a function before its declaration causes compilation failure
        [Category("SmokeTest")]
        public void T017_Associative_Function_CallingAFunctionBeforeItsDeclaration()
        {
            string code = @"
input;
result;
[Associative]
{
	def Level1 : int (a : int)
	{
		return = Level2(a+1);
	}
	
	def Level2 : int (a : int)
	{
		return = a + 1;
	}
	input = 3;
	result = Level1(input);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("input").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("result").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z001_Associative_Function_Regress_1454696()
        {
            string src = @"    def Twice : double(array : double[])
    {
        return = array[0];
    }
    
    arr = {1.0,2.0,3.0};
    arr2 = Twice(arr);";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("arr2", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z002_Defect_1461399()
        {
            string src = @"class Arc
{
	constructor Arc()
	{
		
	}
	def get_StartPoint()
	{
         return = 1;
	}
}
def CurveProperties(curve : Arc)
{
 return = {
	curve.get_StartPoint(),
	curve.get_StartPoint(),
	curve.get_StartPoint()	
 };
}
test=CurveProperties(null);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Object[] v1 = new Object[] { null, null, null };
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z002_Defect_1461399_2()
        {
            string src = @"class Arc
{
	constructor Arc()
	{
		
	}
	def get_StartPoint()
	{
         return = 1;
	}
}
def CurveProperties(curve : Arc)
{
 return = {
	curve[0].get_StartPoint(),
	curve[0].get_StartPoint(),
	curve[0].get_StartPoint()	
 };
}
test=CurveProperties(null);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Object[] v1 = new Object[] { null, null, null };
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z003_Defect_1456728()
        {
            string src = @"def function1 (arr :  double[] )
{
    return = { arr[0], arr [1] };
}
a = function1({null,null});
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Object[] v1 = { null, null };
            thisTest.Verify("a", v1);
        }

        [Test]
        public void TestCallFunctionReturningObjectMultipleTimes()
        {
            string code = @"

class Obj
{
    constructor Obj(){}
    def func()
    {
        return = 1;
    }
}

def f()
{
    p = Obj.Obj();
    p = p.func();
    return = p;
}

x = f();
y = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 1);
        }
    }
}
