using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
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
def Sum : int(a : int, b : int)
{

    return = a + b;
}
[Associative]
{
	a = 1;
	b = 10;
	
	sum = Sum (a, b);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 10);
            thisTest.Verify("sum", 11);

        }

        [Test]
        //Function does not accept single line function / Direct Assignment
        [Category("SmokeTest")]
        public void T002_Associative_Function_SinglelineFunction()
        {
            string code = @"
def singleLine : int(a:int, b:int) 
{
    return = 10;
}
d;
[Associative]
{
	d = singleLine(1,3);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 10);

        }

        [Test]
        [Category("SmokeTest")]
        public void T003_Associative_Function_MultilineFunction()
        {
            string code = @"
def Divide : int(a:int, b:int)
{
    return = a/b;
}
d;
[Associative]
{
	d = Divide (1,3);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T004_Associative_Function_SpecifyReturnType()
        {
            string code = @"
def Divide : double (a:int, b:int)
{
    return = a/b;
}
d;
[Associative]
{
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
def myFunction : int (a:int, b:int)
{
    return = a + b;
}
result;
[Associative]
{
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
def myFunction : double (a: double, b: double)
{
    return = a + b;
}
d1 = null;
d2 = 0.5;
[Associative]
{
	result = myFunction (d1, d2);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //thisTest.Verify("d", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T007_Associative_Function_NestedFunction()
        {
            string code = @"
result;
def ChildFunction : double (r1 : double)
{
return = r1;

}
def ParentFunction : double (r1 : double)
{
    return = ChildFunction (r1)*2;
}
[Associative]
{
	d1 = 1.05;
	result = ParentFunction (d1);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 2.1);
        }

        [Test]
        //Function does not work if the argument variable is declared before function declaration
        [Category("SmokeTest")]
        public void T008_Associative_Function_DeclareVariableBeforeFunctionDeclaration()
        {
            string code = @"
sum;
def Sum : int(a : int, b : int)
{

    return = a + b;
}
[Associative]
{
    a = 1;
    b = 10;
	sum = Sum (a, b);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 11);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_Associative_Function_DeclareVariableInsideFunction()
        {
            string code = @"
def Foo : int(input : int)
{
    multiply = 5;
    divide = 10;

    return = [input*multiply, input/divide];
}
[Associative]
{
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
def Foo : bool (input : bool)
{
    return = input;
}
[Associative]
{
	input = false;
	result1 = Foo (input);
	result2 = Foo (true);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result1", false);
            thisTest.Verify("result2", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void T011_Associative_Function_FunctionWithoutArgument()
        {
            string code = @"
result1;
def Foo1 : int ()
{
    return = 5;
}
[Associative]
{
	result1 = Foo1 ();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result1", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_Associative_Function_MultipleFunctions()
        {
            string code = @"
result1;
result2;
def Foo1 : int ()
{
    return = 5;
}

def Foo2 : int ()
{
    return = 6;
}
[Associative]
{
	result1 = Foo1 ();
	result2 = Foo2 ();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result1", 5);
            thisTest.Verify("result2", 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T013_Associative_Function_FunctionWithSameName_Negative()
        {
            string code = @"
def Foo1 : int ()
{
    return = 5;
}

def Foo1 : int ()
{
    return = 6;
}

[Associative]
{
	result2 = Foo2 ();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        //Should return compilation error if a variable has the same name as a function?
        [Category("SmokeTest")]
        public void T014_Associative_Function_DuplicateVariableAndFunctionName_Negative()
        {
            string code = @"
def Foo : int ()
{
    return = 4;
}
Foo = 5;
[Associative]
{
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        //Incorrect error message when the argument number is not matching with function declaration. 
        [Category("SmokeTest")]
        public void T015_Associative_Function_UnmatchFunctionArgument_Negative()
        {

            string code = @"
	def Foo : int (a : int)
	{
		return = 5;
	}
	
[Associative]
{
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
def Foo : int (a : int)
{
    a = a + 1;
    return = a;
}
[Associative]
{
	input = 3;
	result = Foo(input); 
	originalInput = input;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("input", 3);
            thisTest.Verify("result", 4);
            thisTest.Verify("originalInput", 3);
        }

        [Test]
        //Calling a function before its declaration causes compilation failure
        [Category("SmokeTest")]
        public void T017_Associative_Function_CallingAFunctionBeforeItsDeclaration()
        {
            string code = @"
input;
result;
def Level1 : int (a : int)
{
    return = Level2(a+1);
}

def Level2 : int (a : int)
{
    return = a + 1;
}
[Associative]
{
	input = 3;
	result = Level1(input);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("input", 3);
            thisTest.Verify("result", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void Z001_Associative_Function_Regress_1454696()
        {
            string src = @"    def Twice : double(array : double[])
    {
        return = array[0];
    }
    
    arr = [1.0,2.0,3.0];
    arr2 = Twice(arr);";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("arr2", 1.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Z002_Defect_1461399()
        {
            string src = @"
import(""FFITarget.dll"");
def VectorProperties(v : DummyVector)
{
 return = [
	v.X(),
	v.X(),
	v.X()	
 ];
}
test=VectorProperties(null);
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
    return = [ arr[0], arr [1] ];
}
a = function1([null,null]);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Object[] v1 = { null, null };
            thisTest.Verify("a", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestCallFunctionReturningObjectMultipleTimes()
        {
            string code = @"
import(""FFITarget.dll"");
def f()
{
    p = ClassFunctionality.ClassFunctionality(1);
    p = p.IntVal;
    return = p;
}

x = f();
y = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 1);
        }

        [Test]
        public void TestDefaultArgumentPrimitive01()
        {
            string code = @"    
def f(a : int = 1)
{
    return = a;
}
x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestDefaultArgumentPrimitive02()
        {
            string code = @"    
def f(a : int = null)
{
    return = a;
}
x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", null);
        }

        [Test]
        public void TestDefaultArgumentPrimitive03()
        {
            string code = @"    
def f(a : bool = true)
{
    return = a;
}
x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", true);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestDefaultArgumentPointer01()
        {
            string code = @"    
import(""FFITarget.dll"");
def f(p : ClassFunctionality = ClassFunctionality.ClassFunctionality(1))
{
    return = p.IntVal;
}

x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }


        [Test]
        public void TestDefaultArgumentPointer02()
        {
            string code = @"    
import(""FFITarget.dll"");

def f (a : DummyPoint = DummyPoint.ByCoordinates(1,2,3))
{
    return = a.X;
}

x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestDefaultArgumentUntyped01()
        {
            string code = @"    
def f(a = 1)
{
    return = a;
}
x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestDefaultArgumentUntyped02()
        {
            string code = @"    
def f(a = true)
{
    return = a;
}
x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", true);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestDefaultArgumentUntyped03()
        {
            string code = @"    
import(""FFITarget.dll"");
def f(p = ClassFunctionality.ClassFunctionality(1))
{
    return = p.IntVal;
}

x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestDefaultArgumentUntyped04()
        {
            string code = @"    
import(""FFITarget.dll"");

def f (a = DummyPoint.ByCoordinates(1,2,3))
{
    return = a.X;
}

x = f();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }
    }
}
