using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
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
            string src = @"
i = [Imperative]
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
    return [x, z, y, w, f];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new object[] {35, 35, 5, null, null});
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_TestImpInsideAssoc()
        {
            string src = @"
a = [Associative]
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
    return [x, y, z, w, f];
};";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", new object[] {5.1, null, null, null, null});
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
            string src = @"
b = [Associative]
{ 
    a = 2;
    return [Imperative]
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
        return b;
    }
}
  ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_Defect_1449732()
        {
            string src = @"
	def fn1:int(a:int,b:int)
	{
	    return = a + b -1;
	}
    c = [Imperative]
    {
    	return fn1(3,2);
    } ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("c", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_Defect_1450174()
        {
            string src = @"
	def function1:double(a:int,b:double)
	{ 
	    return a * b;
	}	
    c = [Imperative]
    {
    	return function1(2 + 3,4.0 + 6.0 / 4.0);
    }
  ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("c", 27.5);
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
                thisTest.Verify("x", 5);
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
		def fn:int(a:int)
		{
		    return = a;
		}
[Associative]
{
	a = 4;
	b = a*2;
	x = [Imperative]
	{
		_i = fn(0);
		
		return = _i; 
	}
	a = x;
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_Defect_1452044()
        {
            string src = @"
b = [Associative]
{
	a = 2;
	return [Imperative]
	{
		return 2 * a;
	}
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16__Defect_1452588()
        {
            string src = @"
x = [Imperative]
{
	a = [ 1,2,3,4,5 ];
    x = null;
	for( y in a )
	{
		x = 5;
	}
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17__Defect_1452588_2()
        {
            string src = @"
c = [Imperative]
{
	a = 1;
	
	if( a == 1 )
	{
		if( a + 1 == 2)
			b = 2;
	}
	return a;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("c", 1);
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
