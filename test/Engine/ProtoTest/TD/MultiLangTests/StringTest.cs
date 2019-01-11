using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoScript.Runners;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class StringTest : ProtoTestBase
    {
        ProtoScript.Runners.DebugRunner fsr;

        public override void Setup()
        {
            base.Setup();
            fsr = new ProtoScript.Runners.DebugRunner(core);
        }

        public override void TearDown()
        {
            base.TearDown();
            fsr = null;
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_1()
        {
            string code = @"
a = ""word"";
b = ""word "";
result = 
[Imperative]
{
	if(a==b)
	{
		return = true;
	}
	else return = false;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_2()
        {
            string code = @"
a = ""w ord"";
b = ""word"";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_3()
        {
            string code = @"
a = "" "";
b = """";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_4()
        {
            string code = @"
a = ""a"";
b = ""a"";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "result", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_5()
        {
            string code = @"
a = ""  "";//3 whiteSpace
b = ""	"";//tab
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_6()
        {
            string code = @"
a = """";
b = "" "";
result = 
[Imperative]
{
	if (a ==null && b!=null) return = true;
	else return = false;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "result", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_String_IfElse_7()
        {
            string code = @"
a = ""a"";
result = 
[Imperative]
{
	if (a ==true||a == false) return = true;
	else return = false;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "result", true, 0);
        }

        [Test]
        public void T02_String_Not()
        {
            string errmsg = "1467170 - Sprint 25 - Rev 3142 it must skip the else loop if the conditiona cannot be evaluate to bool it must be skip both if and else";
            string code = @"a = ""a"";
result = 
[Imperative]
{
	if(a)
	{
		return = false;
	}else if(!a)
	{
		return = false;
	}
}
";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object v1 = null;
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T03_Defect_UndefinedType()
        {
            string code = @"
def foo(x:S)
{
	return = x;
}
b = foo(1);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object v1 = null;
            TestFrameWork.Verify(mirror, "b", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_Defect_1467100_String()
        {
            string code = @"
def f(s : string)
{
    return = s;
}
x = f(""hello"");
//expected : x = ""hello""
//received : x = null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", "hello");
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_Defect_1454320_String()
        {
            string code = @"
str;
[Associative]
{
str = ""hello world!"";
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("str", "hello world!");
        }

        [Test]

        public void T05_String_ForLoop()
        {
            string errmsg = "1467197 - Sprint 25 - Rev 3211 - when using dynamic array(without init as an empty array) within a for loop in imperative block, it returns null ";
            string code = @"a = ""hello"";
b = ""world"";
c = [ a, b ];
j = 0;
s = [ ];
	def String(x : string)
	{
	    return = x;
}
r = 
[Imperative]
{
	for(i in c)
	{
	    s[j] = String(i);
	    j = j + 1;
	}
	
    return = s;
  
}
";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { "hello", "world" };
            thisTest.Verify("r", v1, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T07_String_Replication()
        {
            string code = @"
a = ""a"";
bcd = [""b"",""c"",""d""];
r = a +bcd;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { "ab", "ac", "ad" };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_String_Replication_1()
        {
            string code = @"
a = [""a""];
bc = [""b"",""c""];
str = a + bc;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { "ab" };

            thisTest.Verify("str", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_String_Replication_2()
        {
            string code = @"
a = ""a"";
b = [[""b""],[""c""]];
str = a +b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { "ab" };
            Object[] v2 = new Object[] { "ac" };
            Object[] v3 = new Object[] { v1, v2 };
            thisTest.Verify("str", v3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_String_Inline()
        {
            string errmsg = "1467248 - Sprint25 : rev 3452 : comparison with null should result to false in conditional statements";
            string code = @"a = ""a"";
b = ""b"";
r = a>b? a:b;
r1 = a==b? ""Equal"":""!Equal"";";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { "hello", "world" };
            thisTest.Verify("r", "b");
            thisTest.Verify("r1", "!Equal");
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_String_Inline_2()
        {
            string errmsg = "1467248 - Sprint25 : rev 3452 : comparison with null should result to false in conditional statements";
            string code = @"a = ""a"";
b = ""b"";
r = a>b? a:b;
r1 = a==b? ""Equal"":""!Equal"";
b = ""a"";";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { "hello", "world" };
            thisTest.Verify("r", "a");
            thisTest.Verify("r1", "Equal");
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_String_DynamicArr()
        {
            string code = @"
a[1] = foo(""1"" + 1);
a[2] = foo(""2"");
a[10] = foo(""10"");
a[ - 2] = foo("" - 2"");//smart formatting
r = 
[Imperative]
{
    i = 5;
    while(i < 7)
    {
        a[i] = foo(""whileLoop"");
        i = i + 1;
    }
    return = a;
}
def foo(x:var)
{
    return = x + ""!!"";
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v2 = null;
            Object[] v1 = new Object[] { v2, "11!!", "2!!", v2, v2, "whileLoop!!", "whileLoop!!", v2, v2, " - 2!!", "10!!" };
            thisTest.Verify("r", v1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T11_String_Imperative()
        {
            //Assert.Fail("");
            string code = @"
r =
[Imperative]
{
    a = ""a"";
    b = a;
    
}
c = b;
b = ""b1"";
a = ""a1"";
m = ""m"";
n = m;
n = ""n"";
m = m+n;
//a =""a1""
//b = ""b1""
//c = ""b1"";
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;

            thisTest.Verify("a", "a1");
            thisTest.Verify("b", "b1");
            thisTest.Verify("c", "b1");
            thisTest.Verify("m", v1);
            thisTest.Verify("n", v1);
        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringInt()
        {
            String code =
                @"
                a = ""["" + __ToStringFromObject(1)+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[1]");
        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringDouble()
        {
            String code =
                @"
                a = ""["" + 1.1+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[1.100000]");
        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringString()
        {
            String code =
                @"
                a = ""["" + ""1.0""+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[1.0]");
        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringChar()
        {
            String code =
                @"
                a = ""["" + '1'+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[1]");
        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringBool()
        {
            String code =
                @"
                a = ""["" + true+""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "[true]");
        }

        [Test]
        [Category("ConcatenationString")]
        public void TV_ADD_StringNull()
        {
            String code =
                @"
                a = ""["" + null +""]"";
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467263 - Concatenating a string with an integer throws method resolution error");
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("ConcatenationString")]
        public void TV_ADD_StringPointer_1()
        {
            String code =
                @"
import(""FFITarget.dll"");
                a  = ClassFunctionality.ClassFunctionality();
                b = ""a"" + a;
    
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", "aFFITarget.ClassFunctionality");
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("ConcatenationString")]
        public void TV_ADD_StringArr()
        {
            String code =
                @"
import(""FFITarget.dll"");
                a  = ClassFunctionality.ClassFunctionality(1);
                arr1 = [1,2];
                arr2 = [1,a];
                b1 = ""a"" + __ToStringFromArray(arr1);
                b2 = ""a"" + __ToStringFromArray(arr2);
                ";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467263 - Concatenating a string with an integer throws method resolution error");
            thisTest.Verify("b1", "a{1,2}");
            thisTest.Verify("b2", "a{1,FFITarget.ClassFunctionality}");
        }

        [Test]
        public void TestStringIndexing01()
        {
            String code =
                @"
                s = ""abc"";
                r = s[0];
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 'a');
        }

        [Test]
        public void TestStringIndexing02()
        {
            String code =
                @"
                s = ""abcdef"";
                r = s[1..3];
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "bcd");
        }

        [Test]
        public void TestStringIndexing03()
        {
            String code =
                @"
                s = ""abcdef"";
                r = s[-1];
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 'f');
        }

        [Test]
        public void TestStringIndexing04()
        {
            String code =
                @"
                s = ""abcdef"";
                r = s[(-1)..(-3)];
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "fed");
        }

        [Test]
        public void TestStringIndexing05()
        {
            String code =
                @"
                s = """";
                r = s[0];
                ";
            thisTest.RunScriptSource(code);
            // Will get an index out of range runtime warning
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.OverIndexing);
        }

        [Test]
        public void TestLocalizedStringInCode()
        {
            string code = @"x = ""中文字符"";";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", "中文字符");
        }
    }
}
