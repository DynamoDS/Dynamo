using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Imperative
{
    class WhileTest : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_NegativeSyntax_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
{
    i = 0;
    temp = 1;
    while ( i < 5 ]
	{
	    temp=temp+1;
        i=i+1;
    }
}	";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_AssociativeBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
    i = 0;
    temp = 1;
    while ( i <= 5)
	{
	    temp = temp + 1;
         i = i + 1;
    }
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T03_UnnamedBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"{
    i = 0;
    temp = 1;
    while ( i < 5 )
	{
	    temp = temp + 1;
        i = i + 1;
    }
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T04_OutsideBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"i = 0;    
temp = 1;
while( i < 5 )
{
    temp = temp + 1;
    i = i + 1;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T05_WithinFunction()
        {
            string src = @"testvar;
[Imperative]
{
	def fn1 : int (a : int)
	{   
		i = 0;
		temp = 1;
		while ( i < a )
		{
		    temp = temp + 1;
		    i = i + 1;
		}
		return = temp;
	}
	testvar = fn1(5);
} 
	
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("testvar").Payload == 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T06_InsideNestedBlock()
        {
            string src = @"temp;
a;
b;
[Associative]
{
	a = 4;
	b = a*2;
	temp = 0;
	[Imperative]
	{
		i=0;
		temp=1;
		while(i<=5)
		{
	      i=i+1;
		  temp=temp+1;
		}
    }
	a = temp;
      
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 14);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_BreakStatement()
        {
            string src = @"temp;
i;
[Imperative]
{
		i=0;
		temp=0;
		while( i <= 5 )
		{ 
	      i = i + 1;
		  if ( i == 3 )
		      break;
		  temp=temp+1;
		}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_ContinueStatement()
        {
            string src = @"temp;
i;
[Imperative]
{
		i = 0;
		temp = 0;
		while ( i <= 5 )
		{
		  i = i + 1;
		  if( i <= 3 )
		  {
		      continue;
	      }
		  temp=temp+1;
		 
		}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_NestedWhileStatement()
        {
            string src = @"temp;
i;
a;
p;
[Imperative]
{
	i = 1;
	a = 0;
	p = 0;
	
	temp = 0;
	
	while( i <= 5 )
	{
		a = 1;
		while( a <= 5 )
		{
			p = 1;
			while( p <= 5 )
			{
				temp = temp + 1;
				p = p + 1;
			}
			a = a + 1;
		}
		i = i + 1;
	}
}  ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 125);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("p").Payload == 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_WhilewithAssgnmtStatement_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
{
		i = 2;
		temp = 1;
		while( i = 2 )
		{ 
	        i = i + 1 ;
		    temp = temp + 1 ;
		}
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_WhilewithLogicalOperators()
        {
            string src = @"temp1;temp2;temp3;temp4;
[Imperative]
{
		i1 = 5;
		temp1 = 1;
		while( i >= 2) 
		{ 
	        i1=i1-1;
		    temp1=temp1+1;
		}
		
		i2 = 5;
		temp2 = 1;
		while ( i2 != 1 )
		{
		    i2 = i2 - 1;
		    temp2 = temp2 + 1;
		}
         
		temp3 = 2;
        while( i2 == 1 )
		{
		     temp3 = temp3 + 1;
		     i2 = i2 - 1;
		} 
		while( ( i2 == 1 ) && ( i1 == 1 ) )  
        {
             temp3=temp3+1;
		     i2=i2-1;
        }
		temp4 = 3;
		while( ( i2 == 1 ) || ( i1 == 5 ) )
        {
            i1 = i1 - 1;		
            temp4 = 4;
        }       
 
}		
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("temp2").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("temp3").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("temp4").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_WhileWithFunctionCall()
        {
            string src = @"testvar;
[Imperative]
{ 
	def fn1 :int ( a : int )
	{   
		i = 0;
		temp = 1;
		while ( i < a )
		{
			temp = temp + 1;
			i = i + 1;
		}
		return = temp;
	}
	testvar = 8;
	
	while ( testvar != fn1(6) )
	{ 
		testvar=testvar-1;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("testvar").Payload == 7);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_DoWhileStatment_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
{
	 a = 1;
	 temp = 1;
	 do
	 {  
	    temp = temp + 1;
	    a = a + 1;
	  
	 } while(a<3);
} 
  ";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_TestFactorialUsingWhileStmt()
        {
            string src = @"a;
factorial_a;
[Imperative]
{
    a = 1;
	b = 1;
    while( a <= 5 )
	{
		a = a + 1;
		b = b * (a-1) ;		
	}
	factorial_a = b * a;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("factorial_a").Payload == 720);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_TestWhileWithDecimalvalues()
        {
            string src = @"a;b;
[Imperative]
{
    a = 1.5;
	b = 1;
    while(a <= 5.5)
	{
		a = a + 1;
		b = b * (a-1) ;		
	}
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 324.84375);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_TestWhileWithLogicalOperators()
        {
            string src = @"a;b;
[Imperative]
{
    a = 1.5;
	b = 1;
    while(a <= 5.5 && b < 20)
	{
		a = a + 1;
		b = b * (a-1) ;		
	}	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 5.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 59.0625);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_TestWhileWithBool()
        {
            string src = @"a;
[Imperative]
{
    a = 0;	
    while(a == false)
	{
		a = 1;	
	}	
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T18_TestWhileWithNull()
        {
            string src = @"a;b;c;
[Imperative]
{
    a = null;
    c = null;
	
    while(a == 0)
	{
		a = 1;	
	}
    while(null == c)
	{
		c = 1;	
	}
    while(a == b)
	{
		a = 2;	
	}	
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_TestWhileWithIf()
        {
            string src = @"a;b;
[Imperative]
{
    a = 2;
	b = a;
	while ( a <= 4)
	{
		if(a < 4)
		{
			b = b + a;
		}
		else
		{
			b = b + 2*a;
		}
		a = a + 1;
	}
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 15);

        }

        [Test]
        [Category("SmokeTest")]
        public void T20_Test()
        {
            string src = @"a = 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

        }

        [Test]

        public void T20_TestWhileToCreate2DimArray()
        {
            // Assert.Fail("1463672 - Sprint 20 : rev 2140 : 'array' seems to be reserved as a keyword in a specific case ! ");
            string code = @"
def Create2DArray( col : int)
{
	result = [Imperative]
    {
		array = { 1, 2 };
		counter = 0;
		while( counter < col)
		{
			array[counter] = { 1, 2};
			counter = counter + 1;
		}
		return = array;
	}
    return = result;
}
x = Create2DArray( 2) ;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 1, 2 }, new Object[] { 1, 2 } };
            thisTest.Verify("x", v1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T21_TestWhileToCallFunctionWithNoReturnType()
        {
            string code = @"
def foo ()
{
	return = 0;
}
def test ()
{
	temp = [Imperative]
	{
		t1 = foo();
		t2 = 2;
		while ( t2 > ( t1 + 1 ) )
		{
		    t1 = t1 + 1;
		}
		return = t1;		
	}
	return = temp;
}
x = test();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_Defect_1463683()
        {
            string code = @"
def foo ()
{
	return = 1;
}
def test ()
{
	temp = [Imperative]
	{
		t1 = foo();
		t2 = 3;
		if ( t2 < ( t1 + 1 ) )
		{
		    t1 = t1 + 2;
		}
		else
		{
		    t1 = t1 ;
		}
		return = t1;		
	}
	return = temp;
}
x = test();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_Defect_1463683_2()
        {
            string code = @"
def foo ()
{
	return = 1;
}
class A 
{
    t1 : int;
	t2 : int;
	
	def test ()
	{
		temp = [Imperative]
		{
			t1 = foo();
			t2 = 3;
			if ( t2 < ( t1 + 1 ) )
			{
				t1 = t1 + 2;
			}
			else
			{
				t1 = t1 ;
			}
			return = t1;		
		}
		return = temp;
	}
}
a = A.A();
x = a.test();
x1 = a.t1;
x2 = a.t2;
y;y1;y2;
[Imperative]
{
	y = a.test();
	y1 = a.t1;
	y2 = a.t2;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 3);
            thisTest.Verify("y", 1);
            thisTest.Verify("y1", 1);
            thisTest.Verify("y2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_Defect_1463683_3()
        {
            string errmsg = "1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2)";
            string src = @"def foo ()
{
	return = { 0, 1, 2 };
}
class A 
{
    t1;
	t2;	
	def test ()
	{
		c = 0;
		temp = [Imperative]
		{
			t1 = foo();
			t2 = 0;
			for ( i in t1 )
			{
				if (i < ( t2 + 1 ) )
				{
					t1[c] = i + 1;
				}
				else
				{
					t1[c] = i +2 ;
				}
				c = c + 1 ;
			}
			return = t1;		
		}
		return = temp;
	}
}
a = A.A();
x = a.test();
x1 = a.t1;
x2 = a.t2;
y;y1;y2;
[Imperative]
{
	y = a.test();
	y1 = a.t1;
	y2 = a.t2;
}";
            thisTest.VerifyRunScriptSource(src, errmsg);
            Object[] v1 = new Object[] { 1, 3, 4 };

            thisTest.Verify("x", v1);
            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", 0);
            thisTest.Verify("y", v1);
            thisTest.Verify("y1", v1);
            thisTest.Verify("y2", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_Defect_1463683_4()
        {
            string code = @"
def foo ()
{
	return = 1;
}
def test (t2)
{
	temp = [Imperative]
	{
		t1 = foo();
		if ( (t2 > ( t1 + 1 )) && (t2 >=3)  )
		{
		    t1 = t1 + 2;
		}
		else
		{
		    t1 = t1 ;
		}
		return = t1;		
	}
	return = temp;
}
x1 = test(3);
x2 = test(0);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 3);
            thisTest.Verify("x2", 1);
        }




    }
}
