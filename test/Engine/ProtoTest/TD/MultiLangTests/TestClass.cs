using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.MultiLangTests
{
    class TestClass : ProtoTestBase
    {
        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T01_Class_In_Various_Scopes()
        {
            string code = @"
import(""FFITarget.dll"");
obj1 = [Imperative]
{
	a = ClassFunctionality.ClassFunctionality(2);
	a1 = a.IntVal;
	return = a;
}
getX1 = obj1.IntVal;	
obj2 = [Associative]
{
	b = ClassFunctionality.ClassFunctionality(2);
	b1 = b.IntVal;
	return = b1;
}
getX2 = obj2.IntVal;	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T02_Class_In_Various_Nested_Scopes()
        {
            string code = @"
import(""FFITarget.dll"");
c1 = [Imperative]
{
	a = ClassFunctionality.ClassFunctionality(2);
	b = [Associative]
	{
	    return = ClassFunctionality.ClassFunctionality(2);
	}
	c = a.IntVal + b.IntVal;
	return = c;
}
c2 = [Associative]
{
	a = ClassFunctionality.ClassFunctionality(2);
	b = [Imperative]
	{
	    return = ClassFunctionality.ClassFunctionality(2);
	}
	c = a.IntVal + b.IntVal;
	return = c;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T03_Class_In_Various_Scopes()
        {
            string code = @"
import(""FFITarget.dll"");
a = ClassFunctionality.ClassFunctionality(2);
c1 = [Imperative]
{
	b = [Associative]
	{
	    return = a;
	}
	c = a.IntVal + b.IntVal;
	return = c;
}
c2 = [Associative]
{
	b = [Imperative]
	{
	    return = a;
	}
	c = a.IntVal + b.IntVal;
	return = c;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test][Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T12_Class_CheckPropertyWhenCreatedInImperativeCode()
        {
            String code =
            @"
import(""FFITarget.dll"");
obj = [Imperative]{
return = ClassFunctionality.ClassFunctionality(2);
}
getX = obj.IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object getX = 2;
            thisTest.Verify("getX", getX, 0);
        }
    }
}
