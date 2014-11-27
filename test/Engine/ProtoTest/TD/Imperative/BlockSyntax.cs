using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Imperative
{
    class BlockSyntax : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_TestImpInsideImp()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
    x = 5;
    [Imperative]
    {
        y = 5;
    }
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
                // thisTest.Verify("x", 5);
                // thisTest.Verify("y", 5);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_TestAssocInsideImp()
        {
            string src = @"x;
y;
z;
w;
f;
[Imperative]
{
    x = 5.1;
    z = y;
    w = z * 2;
    [Associative]
    {
        y = 5;
        z = x;
        x = 35;
        i = 3;
    }
    f = i;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 35);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 35);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 5);
            Assert.IsTrue(mirror.GetValue("w").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("f").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_TestImpInsideAssoc()
        {
            string src = @"x;
y;
z;
w;
f;
[Associative]
{
    x = 5.1;
    z = y;
    w = z * 2;
    [Imperative]
    {
        y = 5;
        z = x;
        x = 35;
        i = 3;
    }
    f = i;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 35);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("w").Payload == 10);
            Assert.IsTrue(mirror.GetValue("f").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_TestImperativeBlockWithMissingBracket_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
{
    x = 5.1;
    
";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_TestImperativeBlockWithMissingBracket_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
    x = 5.1;
    
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T06_TestNestedImpBlockWithMissingBracket_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
{
    x = 5.1;
    [Associative]
    {
    
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_TestBlockWithIncorrectBlockName_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[imperitive]
{
    x = 5.1;    
    
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_TestBlockWithIncorrectBlockName_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"[Imperative]
{
    x = 5.1; 
    [assoc]
    {
        y = 2;
    }
    
}";
                ExecutionMirror mirro = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_Defect_1449829()
        {
            string src = @"b;
[Associative]
{ 
 a = 2;
[Imperative]
{   
	b = 1;
    if(a == 2 )
	{
	b = 2;
    }
    else 
    {
	b = 4;
    }
}
}
  ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_Defect_1449732()
        {
            string src = @"c;
[Imperative]
{
	def fn1:int(a:int,b:int)
	{
	return = a + b -1;
	}
 
	c = fn1(3,2);
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_Defect_1450174()
        {
            string src = @"c;
[Imperative]
{
	def function1:double(a:int,b:double)
	{ 
	return = a * b;
	}	
 
	c = function1(2 + 3,4.0 + 6.0 / 4.0);
}
  ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("c").Payload == 27.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_Defect_1450599()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
	x = 5;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 5);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_Defect_1450527()
        {
            string src = @"temp;
[Associative]
{
	a = 1;
	temp=0;
	[Imperative]
	{
	    i = 0;
	    if(i <= a)
	    {
	        temp = temp + 1;
	    }
	}
	a = 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("temp", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_Defect_1450550()
        {
            string src = @"a;
[Associative]
{
	a = 4;
	b = a*2;
	x = [Imperative]
	{
		def fn:int(a:int)
		{
		    return = a;
		}
		
		_i = fn(0);
		
		return = _i; 
	}
	a = x;
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_Defect_1452044()
        {
            string src = @"b;
[Associative]
{
	a = 2;
	[Imperative]
	{
		b = 2 * a;
	}
		
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16__Defect_1452588()
        {
            string src = @"x;
[Imperative]
{
	a = { 1,2,3,4,5 };
	for( y in a )
	{
		x = 5;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17__Defect_1452588_2()
        {
            string src = @"c;
[Imperative]
{
	a = 1;
	
	if( a == 1 )
	{
		if( a + 1 == 2)
			b = 2;
	}
	
	c = a;
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T18__Negative_Block_Syntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"x = 1;
y = {Imperative]
{
   return = x + 1;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T19_Imperative_Nested()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
   a=1;
   [Imperative]
    {
    b=a+1;
    }
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }
    }
}
