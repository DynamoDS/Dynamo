using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Imperative
{
    class IfElseTest : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_TestAllPassCondition()
        {
            string src = @"x;
y;
z;
[Imperative]
{
 a1 = 2 ;
 a2 = -1;
 a3 = 101;
 a4 = 0;
 
 b1 = 1.0;
 b2 = 0.0;
 b3 = 0.1;
 b4 = -101.99;
 b5 = 10.0009;
 
 c1 = { 0, 1, 2, 3};
 c2 = { 1, 0.2};
 c3 = { 0, 1.4, true };
 c4 = {{0,1}, {2,3 } };
 
 x = {0, 0, 0, 0};
 if(a1 == 2 ) // pass condition
 {
     x[0] = 1;
 }  
 if(a2 <= -1 )  // pass condition
 {
     x[1] = 1;
 }
 if(a3 >= 101 )  // pass condition
 {
     x[2] = 1;
 }
 if(a4 == 0 )  // pass condition
 {
     x[3] = 1;
 }
 
 
 y = {0, 0, 0, 0, 0};
 if(b1 == 1.0 ) // pass condition
 {
     y[0] = 1;
 }  
 if(b2 <= 0.0 )  // pass condition
 {
     y[1] = 1;
 }
 if(b3 >= 0.1 )  // pass condition
 {
     y[2] = 1;
 }
 if(b4 == -101.99 )  // pass condition
 {
     y[3] = 1;
 }
 if(b5 == 10.0009 )  // pass condition
 {
     y[4] = 1;
 }
 
 
 z = {0, 0, 0, 0};
 if(c1[0] == 0 ) // pass condition
 {
     z[0] = 1;
 }  
 if(c2[1] <= 0.2 )  // pass condition
 {
     z[1] = 1;
 }
 if(c3[2] == true )  // pass condition
 {
     z[2] = 1;
 }
  if(c4[0][0] == 0 )  // pass condition
 {
     z[3] = 1;
 }
 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Object o = mirror.GetValue("x").Payload;
            ProtoCore.DSASM.Mirror.DsasmArray x = (ProtoCore.DSASM.Mirror.DsasmArray)o;
            Assert.IsTrue((Int64)x.members[0].Payload == 1);
            Assert.IsTrue((Int64)x.members[1].Payload == 1);
            Assert.IsTrue((Int64)x.members[2].Payload == 1);
            Assert.IsTrue((Int64)x.members[3].Payload == 1);
            Object o1 = mirror.GetValue("y").Payload;
            ProtoCore.DSASM.Mirror.DsasmArray y = (ProtoCore.DSASM.Mirror.DsasmArray)o1;
            Assert.IsTrue((Int64)y.members[0].Payload == 1);
            Assert.IsTrue((Int64)y.members[1].Payload == 1);
            Assert.IsTrue((Int64)y.members[2].Payload == 1);
            Assert.IsTrue((Int64)y.members[3].Payload == 1);
            Assert.IsTrue((Int64)y.members[4].Payload == 1);
            Object o2 = mirror.GetValue("z").Payload;
            ProtoCore.DSASM.Mirror.DsasmArray z = (ProtoCore.DSASM.Mirror.DsasmArray)o2;
            Assert.IsTrue((Int64)z.members[0].Payload == 1);
            Assert.IsTrue((Int64)z.members[1].Payload == 1);
            Assert.IsTrue((Int64)z.members[2].Payload == 1);
            Assert.IsTrue((Int64)z.members[3].Payload == 1);
        }

        [Test]
        //[Category ("SmokeTest")]
        [Category("Warnings and Exceptions")]
        public void T02_IfElseIf()
        {
            string err = "1467181 - Sprint25 : rev 3152: Warning message of 'lack of returning statement' should display when not all paths returns value at compiling time ";
            string src = @"temp1;
[Imperative]
{
 a1 = 7.5;
 
 temp1 = 10;
 
 if( a1>=10 )
 {
 temp1 = temp1 + 1;
 }
 
 elseif( a1<2 )
 {
 temp1 = temp1 + 2;
 }
 elseif(a1<10)
 {
 temp1 = temp1 + 3;
 }
 
  
 }";
            thisTest.VerifyRunScriptSource(src, err);
            thisTest.Verify("temp1", 13);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_MultipleIfStatement()
        {
            string src = @"temp;
[Imperative]
{
 a=1;
 b=2;
 temp=1;
 
 if(a==1)
 {temp=temp+1;}
 
 if(b==2)  //this if statement is ignored
 {temp=4;}
 
 }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_IfStatementExpressions()
        {
            string src = @"temp1;
[Imperative]
{
 a=1;
 b=2;
 temp1=1;
 if((a/b)==0)
 {
  temp1=0;
  if((a*b)==2)
  { temp1=2;
  }
 } 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_InsideFunction()
        {
            string src = @"temp;
temp2;
[Imperative]
{
	def fn1:int(a:int)
	{   
		if(a>=0)
			return = 1;
		else 
			return = 0;
	}
    def fn2:int(a:int)
	{   
	   
		if( a < 0 )
		{
			return = 0;
		}
		elseif	( a == 2 )
		{
			return = 2;
		}
		else
		{
			return = 1;
		}
	}
	
    temp = 0;
    temp2 = 0;
	 if(fn1(-1)==0)
		 temp=fn1(2);	 
		 
	
	if(fn2(2)==2)
	   temp2=fn2(1);
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("temp2").Payload == 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T06_NestedIfElse()
        {
            string src = @"temp1;
[Imperative]
{
def fn1:int(a:int)
{   
     if( a >= 0 )
		return = 1;
	else
		return = 0;
}
 a = 1;
 b = 2;
 temp1 = 1;
 
 if( a/b == 0 )
 {
  temp1=0;
  if( a*b == 1 )
  { 
	temp1=2;
  }  
  else if( a*b == 4 )
  { 
	temp1=5;
  }  
  else
  {
	temp1=3;
	if( fn1(-1)>-1 )
	{
		temp1=4;
	}
  }
 } 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_ScopeVariableInBlocks()
        {
            string src = @"temp;
a;
[Imperative]
{
	a = 4;
	b = a*2;
	temp = 0;
	if(b==8)
	{
		i=0;
		temp=1;
		if(i<=a)
		{
		  temp=temp+1;
		}
    }
	a = temp;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_NestedBlocks()
        {
            string src = @"a;
b;
temp;
[Associative]
{
	a = 4;
	
	[Imperative]
	{
		i=10;
		temp=1;
		if(i>=-2)
		{
		  temp=2;
		}
    }
	b=2*a;
	a=2;
              
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_NestedIfElseInsideWhileStatement()
        {
            string src = @"temp;
[Imperative]
{
		i=0;
		temp=0;
		while(i<=5)
		{ 
			i=i+1;
			if(i<=3)
			{
				temp=temp+1;
			}		  
			elseif(i==4)
			{
				temp = temp+1;
				if(temp==i) 
				{
					temp=temp+1;
				}			
			}
			else 
			{
				if (i==5)
				{ temp=temp+1;
				}
			}
		}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 6);

        }

        [Test]
        [Category("SmokeTest")]
        public void T10_TypeConversion()
        {
            string src = @"temp;
[Imperative]
{
    temp = 0;
    a=4.0;
    if(a==4)
        temp=1;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_TestIfElseUsingFunctionCall()
        {
            string src = @"a;
b;
[Imperative]
{
 def add : double (a :double, b:double)
 {
     return  = a + b;
 }
 
 a=4.0;
 b = 4.0;
 if(a<add(1.0,2.0))
 {
     a = 1;
 }
 else
 {
     a = 0;
 }
 
 if(add(1.5,2.0) >= a)
 {
     b = 1;
 }
 else
 {
     b = 0;
 }
 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_TestIfElseUsingClassProperty()
        {
            string src = @"x;
y;
	class A 
    {                                      
		P1:var;
        constructor A(p1:int)
        {
            P1 = p1;
        }
          
    }
    
	[Imperative]
	{
	    a1 = A.A(2);
        b1 = a1.P1; 
		x = 2;
		y = 2;
		if(a1.P1 == 2 )
		{
		    x = 1;
		}
		else
		{
			x = 0;
		}
		
		if(3 < a1.P1  )
		{
		    y = 1;
		}
		else
		{
			y = 0;
		}
	}
                              
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_IfElseIf()
        {
            string src = @"temp1;
[Imperative]
{
 a1 = -7.5;
 
 temp1 = 10.5;
 
 if( a1>=10.5 )
 {
 temp1 = temp1 + 1;
 }
 
 elseif( a1<2 )
 {
 temp1 = temp1 + 2;
 }
 
  
 }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("temp1").Payload == 12.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_IfElseStatementExpressions()
        {
            string src = @"temp1;
[Imperative]
{
 a=1;
 b=2;
 temp1=1;
 if((a/b)==1)
 {
  temp1=0;
 }
 elseif ((a*b)==2)
 { temp1=2;
 }
 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T15_TestEmptyIfStmt()
        {
            string src = @"a;
b;
[Imperative]
{
 a = 0;
 b = 1;
 if(a == b);
 else a = 1;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T16_TestIfConditionWithNegation_Negative()
        {
            string src = @"a;
b;
[Imperative]
{
    a = 3;
    b = -3;
	if ( a == !b )
	{
	    a = 4;
	}
	
}
 
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_WhileInsideElse()
        {
            string src = @"i;
[Imperative]
{
	i=1;
	a=3;
    temp=0;
	if(a==4)             
	{
		 i = 4;
	}
	else
	{
		while(i<=4)
		 {
			  if(i>10) 
				temp=4;			  
			  else 
				i=i+1;
		 }
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 5);

        }

        [Test]
        [Category("SmokeTest")]
        public void T18_WhileInsideIf()
        {
            string src = @"i;
[Imperative]
{
	i=1;
	a=3;
    temp=0;
	if(a==3)             //when the if statement is removed, while loop works fine, otherwise runs only once
	{
		 while(i<=4)
		 {
			  if(i>10) 
				temp=4;			  
			  else 
				i=i+1;
		 }
	}
}
 
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_BasicIfElseTestingWithNumbers()
        {
            string src = @"a;
b;
c;
d;
[Imperative]
{
    a = 0;
    b = 0;
    c = 0;
    d = 0;
    if(1)
	{
		a = 1;
	}
	else
	{
		a = 2;
	}
	
	
	if(0)
	{
		b = 1;
	}
	else
	{
		b = 2;
	}
	
	if(0)
	{
		c = 1;
	}
	elseif(1)
	{
		c = 3;
	}
	
	if(0)
	{
		d = 1;
	}
	elseif(0)
	{
		d = 2;
	}
	else
	{
		d = 4;
	}
		
} 
 
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_BasicIfElseTestingWithNumbers()
        {
            string src = @"a;
b;
c;
d;
e;
f;
[Imperative]
{
    a = 0;
    b = 0;
    c = 0;
    d = 0;
    e = 0;
    f = 0;
    if(1.5)
	{
		a = 1;
	}
	else
	{
		a = 2;
	}
	
	
	if(-1)
	{
		b = 1;
	}
	else
	{
		b = 2;
	}
	
	if(0)
	{
		c = 1;
	}
	elseif(20)
	{
		c = 3;
	}
	
	if(0)
	{
		d = 1;
	}
	elseif(0)
	{
		d = 2;
	}
	else
	{
		d = 4;
	}
	
	if(true)
	{
		e = 5;
	}
	
	if(false)
	{
		f = 1;
	}
	else
	{
		f = 6;
	}
		
} 
 
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_IfElseWithArray_negative()
        {
            string src = @"c;
[Imperative]
{
    a = { 0, 4, 2, 3 };
	b = 1;
    c = 0;
	if(a > b)
	{
		c = 0;
	}
	else
	{
		c = 1;
	}
} 
 
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_IfElseWithArrayElements()
        {
            string src = @"c;
[Imperative]
{
    a = { 0, 4, 2, 3 };
	b = 1;
    c = 0;
	if(a[0] > b)
	{
		c = 0;
	}
	elseif( b  < a[1] )
	{
		c = 1;
	}
} 
 
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    if(1.5
	{
		b = 1;
	}
		
}
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    if1.5
	{
		b = 1;
	}
		
}
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    if1.5)
	{
		b = 1;
	}
		
} 
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_IfElseWithNegatedCondition()
        {
            string src = @"c;
[Imperative]
{
    a = 1;
	b = 1;
    c = 0;
	if( !(a == b) )
	{
		c = 1;
	}
	else
	{
		c = 2;
	}
		
} 
 
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T27_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    if(1)
	
		b = 1;
	}
		
} 
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T28_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    if(1)
	{
		{
		    b = 1;
	
		
} 
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T29_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    b = 0;
    if(1)
	{
		b = 1
	}
	
		
} 
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T30_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    if(1)
	{
		b = 1;
	}
	else c = 2 } 
	
		
} 
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    if(1)
	{
		b = 1;
	}
	elsee  { c = 2 } 
	
		
} 
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T32_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    string src = @"[Imperative]
{
    if(1)
	{
		b = 1;
	}
	elseif  { c = 2 ;
	
		
} 
 
 ";
                    ExecutionMirror mirror = thisTest.RunScriptSource(src);
                });
        }

        [Test]
        [Category("SmokeTest")]
        public void T33_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    string src = @"[Imperative]
{
    if(0)
	{
		b = 1;
	}
	elseif (0) { c = 2; }
	else { c = 3;}
	else {c = 4};
	
		
} 
 
 ";
                    ExecutionMirror mirror = thisTest.RunScriptSource(src);
                });
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_IfElseSyntax_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
    {
        string src = @"[Imperative]
{
    if(0)
	{
		b = 1;
	}
	elseif { c = 2; }
	else { c = 3;}
	else {c = 4};
	
		
} 
 
 ";
        ExecutionMirror mirror = thisTest.RunScriptSource(src);
    });
        }

        [Test]
        [Category("SmokeTest")]
        public void T35_IfElseWithEmptyBody()
        {
            string src = @"c;
[Imperative]
{
    c = 0;
    if(0)
	{
		
	}
	elseif (1) { c = 2; }
	else { }
	
	
		
} 
 
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T36_IfElseInsideFunctionScope()
        {
            string src = @"temp;
[Imperative]
{ 
 def crushcode:int (a:int, b:int)
 {
  if(a<=b)
      return = a+b;  
  else 
     return = 0;           
 }                                
 temp=crushcode(2,3);  
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            //defect 1451089
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T37_Defect_1450920()
        {
            string src = @"a;
b;
c;
[Imperative]
{
    a = 0;
    b = 0;
    c = 0;
    d = 0;
    if(true)
	{
		a = 1;
	}
	
	if(false)
	{
		b = 1;
	}
	elseif(true)
	{
		b = 2;
	}
	
	if(false)
	{
		c = 1;
	}
	elseif(false)
	{
		c = 2;
	}
	else
	{
		c =  3;
	}		
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T38_Defect_1450939()
        {
            string src = @"c;
d;
[Imperative]
{
   	def test:int( a:int, b:int )
	{
		c = 0;
	    if( !(a == b) ) 
		{
			c = 0;
		}
		elseif ( !(a==b) )
		{
			c = 1;
		}
		else
		{
			c = 2;
		}
		
		return = c;
	}
	
	
	a = 1;
	b = 1;
    c = 0;
    d = 0;
	if( !(a == b) ) 
	{
		d = 0;
	}
	elseif ( !(a==b) )
	{
		d = 1;
	}
	else
	{
		d = 2;
	}
	
	
	if( ! (test ( a, b ) == 2 ) )
	{
		c = 3;
	}
	else
	{
		c = 2;
	}
		
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T39_Defect_1450920_2()
        {
            string src = @"a;
b;
c;
d;
[Imperative]
{
a=0;
b=0;
c=0;
d=0;
    if(0.4)
	{
		d = 4;
	}
	
	if(1.4)
	{
		a = 1;
	}
	
	if(0)
	{
		b = 1;
	}
	elseif(-1)
	{
		b = 2;
	}
	
	if(0)
	{
		c = 1;
	}
	elseif(0)
	{
		c = 2;
	}
	else
	{
		c =  3;
	}		
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T40_Defect_1450843()
        {
            string src = @"b2;
b3;
[Imperative]
{
 a = null;
 b1 = 0;
 b2 = 0;
 b3 = 0;
 if(a!=1); 
 else 
   b1 = 2; 
   
 if(a==1); 
 else 
   b2 = 2;
   
 if(a==1); 
 elseif(a ==3);
 else b3 = 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("b2").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b3").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T41_Defect_1450778()
        {
            string src = @"c;
d;
[Imperative]
{
 a=1;
 b=2;
 c=2;
 d = 2;
 
 if(a==1)
 {
    c = 1;
 }
 
 if(b==2)  
 {
     d = 1;
 }
 
 }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T42_Defect_1449707()
        {
            string src = @"c;
[Imperative]
{
 a = 1;
 b = 1;
 c = 1;
 if( a < 1 )
	c = 6;
 
 else if( b >= 2 )
	c = 5;
 
 else
	c = 4;
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T43_Defect_1450706()
        {
            string src = @"temp1;
temp2;
[Imperative]
{
 a1 = 7.3;
 a2 = -6.5 ;
 
 temp1 = 10;
 temp2 = 10;
 
 if( a1 <= 7.5 )
	temp1 = temp1 + 2;
 
 if( a2 >= -9.5 )
	temp2 = temp2 + 2;
 }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 12);
            Assert.IsTrue((Int64)mirror.GetValue("temp2").Payload == 12);
        }

        [Test]
        [Category("SmokeTest")]
        public void T44_Defect_1450706_2()
        {
            string src = @"x;
[Imperative]
{
	def float_fn:int(a:int)
	{
		if( a < 2 )
			return = 0;
		else
			return = 1;
	}
	 
	x = float_fn(1);
 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Defect_1450506()
        {
            string src = @"temp;
[Imperative]
{
    i1 = 2;
    i2 = 3;
	i3 = 4.5;
	
    temp = 2;
    
	while(( i2 == 3 ) && ( i1 == 2 )) 
	{
	temp = temp + 1;
	i2 = i2 - 1;
    }
	
	if(( i2 == 3 ) || ( i3 == 4.5 )) 
	{
	temp = temp + 1;
    }
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T46_TestIfWithNull()
        {
            string src = @"a;
b;
c;
[Imperative]
{
    a = null;
    c = null;
	
    if(a == 0)
	{
		a = 1;	
	}
    if(null == c)
	{
		c = 1;	
	}
    if(a == b)
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
        public void T47_Defect_1450858()
        {
            string src = @"i;
[Imperative]
{	
	i = 1;
	a = 3;
	if( a==3 )             	
	{		 
		while( i <= 4 )
		{
		if( i > 10 )
		temp = 4;
		else
		i = i + 1;
		}
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T48_Defect_1450858_2()
        {
            string src = @"test;
[Imperative]
{	
	def factorial:int(a:int)
	{
		 fact = 1;
		 
		 if( a != 0)
		 {
			 while( a > 0 )
			 { 
				fact = fact * a;
				a = a - 1;
			 }
		}	 
		
		return = fact;
	}
	
	test = factorial(4);
}	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("test").Payload == 24);
        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1450783()
        {
            string src = @"a;
[Imperative]
{
	a = 4;
	if( a == 4 )
	{
	    i = 0;
	}
	a = i;
	b = i;
} 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
        }

        [Test]
        [Category("SmokeTest")]
        public void T50_Defect_1450817()
        {
            string src = @"temp;
[Imperative]
{ 
	def fn:int(a:int)
	{
		if( a < 0 )
		if( a < -1 )
		return = 0;
		else
		return = -1;
		
		return = 1;
	}
	
	x = fn(-1);
	
	temp = 1;
	
	if (fn(2))
	{
		temp = fn(5);
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T51_Defect_1452588()
        {
            string src = @"a;
[Imperative]
{
    a = 0;
    
    if ( a == 0 )
    {
	    b = 2;
    }
    c = a;
} 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1452588_2()
        {
            string src = @"g1;
[Associative]
{ 
	[Imperative]
	{
            g2 = g1;	
	}	
	g1 = 3;      
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("g1").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Defect_1452575()
        {
            string src = @"x;
[Imperative]
{ 
	def float_fn:int(a:double)
	{
		if( a < 2.0 )
			return = 0;
		else
			return = 1;
	}
	 
	x = float_fn(-1.5);
     
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T54_Defect_1451089()
        {
            string src = @"temp;
[Imperative]
{ 
 def foo:double (a:int, b:int, c : double)
 {
  if(a<=b && b > c)
      return = a+b+c;  
  else 
     return = 0;           
 }                                
 temp=foo(2,3,2.5);  
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("temp").Payload == 7.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T55_Defect_1450506()
        {
            string src = @"temp;
[Imperative]
{
    i1 = 1.5;
    i2 = 3;
    temp = 2;
    while( ( i2==3 ) && ( i1 <= 2.5 )) 
    {
        temp = temp + 1;
	    i2 = i2 - 1;
    }     
 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T56_Defect_1460162()
        {
            string code = @"
class A
{
    X : int;
    
    
    constructor A(x : int)
    {
        X = x;        
    }
}
def length:int  (pts : A[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }  
        if( counter > 1 )
        {
            return = counter;
        }			
    //return = null;
    }
   return = numPts;
}
pt1 = A.A( 0 );
pt2 = A.A( 10 );
pts1 = {pt1, pt2};
pts2 = {pt1};
numpts1 = length(pts1);
numpts2 = length(pts2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object v1 = null;
        }

        [Test]
        [Category("SmokeTest")]
        public void T57_Function_With_If_Else_But_No_Default_Return_Statement()
        {
            string code = @"
x;
y;
[Imperative]
{
	def even : int (a : int) 
	{	
		if(( a % 2 ) > 0 )
			return = a + 1;
		
		else 
			return = a;
	}
	x = even(1);
	y = even(2);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Verification 
            thisTest.Verify("x", 2);
            thisTest.Verify("y", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T58_Defect_1450932_comparing_collection_with_singleton_Imperative()
        {
            string code = @"
c;
d;
f;
[Imperative]
{
    a = { 0, 1 };
	b = 1;
	c = -1;
	if(a > b)
	{
		c = 0;
	}
	else
	{
		c = 1;
	}
    d = a > b ? true : { false, false};
    f = a > b;
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { false, false };
            Object v2 = null;

            //Verification 
            thisTest.Verify("c", 1);
            thisTest.Verify("d", v1);
            thisTest.Verify("f", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T58_Defect_1450932_comparing_collection_with_singleton_Associative()
        {
            string code = @"
d2;
[Associative]
{
    a2 = { 0, 1 };
	b2 = 1;
	d2 = a2 > b2 ? true : { false, false};
    //f2 = a2 > b2;	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { false, false }, new Object[] { false, false } };
            //Verification 
            thisTest.Verify("d2", v1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T58_Defect_1450932_comparing_collection_with_singleton_Associative_2()
        {
            string code = @"
f2;
[Associative]
{
    a2 = { 0, 1 };
    b2 = 1;	
    f2 = a2 > b2;	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            thisTest.Verify("f2", new Object[] { false, false });
        }

        [Test]
        [Category("Inline Conditionals")]
        public void T58_Defect_1450932_comparing_collection_with_singleton_Associative_3()
        {
            string err = "";//1467192 - sprint24: rev 3199 : REGRESSION:Inline Condition with Replication is giving wrong output when multiple inline statements are used";
            string src = @"f2;
[Associative]
{
    a2 = { 0, 1 };
    b2 = 1;
    d2 = a2 > b2 ? true : { false, false};
    f2 = a2 > b2;	
}";
            thisTest.VerifyRunScriptSource(src, err);
            //Verification 
            thisTest.Verify("f2", new Object[] { false, false });
        }

        [Test]
        [Category("SmokeTest")]
        public void T59_Defect_1453881()
        {
            string err = "1467248 - Sprint25 : rev 3452 : comparison with null should result to false in conditional statements";
            string src = @"b;
d;
d2 = ( null == 0 ) ? 1 : 0; 
[Imperative]
{
	a = false;
    b = 0.5;
	d = 0;
	if( a == null)
	{
	    d = d + 1;
	}
	else
	{
	   d = d + 2;
	}
    if( b == null)
	{
	    b = b + 1;
	}
	else
	{
	   b = b + 2;
	}
	
	if( b != null)
	{
	    b = b + 3;
	}
	else
	{
	    b = b + 4;
	}
	
	
}	
";
            thisTest.VerifyRunScriptSource(src, err);

            //Verification 
            thisTest.Verify("b", 5.5);
            thisTest.Verify("d", 2);
            thisTest.Verify("d2", 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T59_Defect_1453881_2()
        {
            string code = @"
def foo ()
{
    c = 
	[Imperative]
	{
		a = false;
		b = 0.5;
		d = 0;
		if( a == null)
		{
			d = d + 1;
		}
		else
		{
		   d = d + 2;
		}
		if( b == null)
		{
			b = b + 1;
		}
		else
		{
		   b = b + 2;
		}
		
		if( b != null)
		{
			b = b + 3;
		}
		else
		{
			b = b + 4;
		}
        return = { b, d };		
	}	
	return = c;
}
test = foo();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 5.5, 2 };
            //Verification 
            thisTest.Verify("test", v1);
        }



        [Test]
        [Category("SmokeTest")]
        public void T60_Comparing_Class_Properties()
        {
            string code = @"
class B
{
    b : var;
    constructor B ( y )
    {
        b = y;
    }
}
class A
{ 
    a : var;
    constructor A ( x : var )
    {
        a = x;
    }
}
a1 = A.A(10);
b1 = B.B(10);
x1 = a1.a == B.B(10).b ? true : false ;
x2 = [Imperative]
{
    b = 0;
    if ( a1.a == B.B(10).b ) 
        b = true;
    else
        b = false;
    return = b;
}
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            thisTest.Verify("x1", true);
            thisTest.Verify("x2", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void T60_Comparing_Class_Properties_With_Null()
        {
            string code = @"
class B
{
    b : var;
    constructor B ( y )
    {
        b = y;
    }
}
class A
{ 
    a : var;
    
}
a1 = A.A(10);
b1 = B.B(10);
x1 = a1.a == B.B(10).a ? true : false ;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            thisTest.Verify("x1", true);
        }

        [Test]
        public void T61_Accessing_non_existent_properties_of_array_elements()
        {
            // Assert.Fail("");
            string code = @"
class A  
{
    x : var;
    constructor A ( y : var )
    {
        x = y;
    }
}
c = { A.A(0), A.A(1) };
p = {};
d = [Imperative]
{
    if(c[0].x == 0 )
    {
        c[0] = 0;
	p[0] = 0;
    }
    if(c[0].x == 0 )
    {
        p[1] = 1;
    }
    return = 0;
}
t1 = c[0];
t2 = c[1].x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            thisTest.Verify("t1", 0);
            thisTest.Verify("t2", 1);
        }

        [Test]
        [Category("Statements")]
        public void T62_Condition_Not_Evaluate_ToBool()
        {
            string err = "1467170 Sprint 25 - Rev 3142 it must skip the else loop if the conditional cannot be evaluate to bool it must be skip both if and else";
            string src = @"
A;
[Imperative]
{
    A = 1;
    if (0)       
 	   A = 2; 
    else 
	  A= 3;
}";
            thisTest.VerifyRunScriptSource(src, err);
            //Verification 
            thisTest.Verify("A", 3);
        }


        [Test]
        public void T63_return_in_if_1467073()
        {
            string err = "1467073 - sprint 23 rev 2651-328756 throws warning missing return statement ";
            string src = @"c;
[Imperative]
{
def even : int (a : int)
 { 
   if( ( a % 2 ) > 0 )
        return = a + 1;
   else 
           return = a;
}
c = even(1);
 }
";
            thisTest.VerifyRunScriptSource(src, err);
            thisTest.Verify("c", 2);
        }
        //TDD for if condition/inline condition

        [Test]
        [Category("TDDIfInline")]
        public void TDD_SimpleIfCondition_1()
        {
            String code =
@"
r = [Imperative]
{
    //b = 1;
    if (null==false)
    {
        return = ""null==true"";
    }
    else if(!null)
    {
        return = ""!null==true"";
    }
    return = ""expected"";
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "expected");
        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_SimpleIfCondition_2()
        {
            String code =
@"
r = [Imperative]
{
    if (!null)
    {
        return = ""!null==true"";
    }
    else if(!(true||null))
    {
        return = ""true||null==false"";
    }else
    {
        return =""expected""; 
    }
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "expected");
        }

        [Test]
        [Category("TDDIfInline")]
        // r = true!?
        public void TDD_NullAsArgs()
        {
            String code =
@"
def foo(x:string)
{
    return = 1;
}
r:bool = foo(null);
";
            thisTest.RunScriptSource(code);

            thisTest.Verify("r", true);
        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_UserDefinedTypeConvertedToBool_NotNull_defect()
        {
            String code =
@"
        class A
{
        a:int;
        constructor A (b:int)
        {
                a=b;
    }
}
d:bool=A.A(5);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("d", true);
        }

        [Test]
        [Category("TDDIfInline")]
        //not null user defined var is not evaluated as true
        public void TDD_UserDefinedTypeConvertedToBool()
        {
            String code =
@"
        class A{}
r = 
[Imperative]
{
a = A.A();
if(a)
{
    return = true;
}
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", true);
        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_UserDefinedTypeConvertedToBool_Null()
        {
            String code =
@"
n;
r = 
[Imperative]
{
a = A.A();
b:bool = A.A();
def foo(x:bool)
{
    return = ""true"";
}
m = b;
n = foo(a);
return = foo(b);
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "true");
            thisTest.Verify("n", "true");
        }
        //inline

        [Test]
        [Category("TDDIfInline")]
        public void TDD_SimpleInlineCondition_1()
        {
            String code =
@"
r = [Imperative]
{
return = null==false?""null==false"":""null==false is false"";
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "null==false is false");
        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_SimpleInlineCondition_2()
        {
            String code =
@"
r = [Imperative]
{
    return = !null==false?""!null==false"":""expected"";
   
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "expected");
        }

        [Test]
        [Category("TDDIfInline")]
        public void TDD_UserDefinedTypeConvertedToBool_Inline()
        {
            String code =
@"
        class A{}
r = 
[Imperative]
{
a = A.A();
return = a==true?true:""A.A()!=true"";
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", true);
        }
    }
}
