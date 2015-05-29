using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Associative
{
    class Assignment : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_SampleTestUsingCodeWithinTestFunction()
        {
            String code =
                @"variable;[Associative]
                {
	                variable = 5;
                }
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("variable");
            Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_SampleTestUsingCodeFromExternalFile()
        {
            string src = @"variable;
[Associative]
{
    variable = 5;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("variable").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_TestAssignmentToUndefinedVariables_negative()
        {
            string src = @"a;
[Associative]
{
    a = b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_TestAssignmentStmtInExpression_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
    b = if (2==2) { a = 0; }
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                // expected "StatementUsedInAssignment" warning
                Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
                Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_TestRepeatedAssignment()
        {
            string src = @"a;
b;
[Associative]
{
    b = a = 2;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T06_TestInUnnamedBlock_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"{
    b = 2;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_TestOutsideBlock()
        {
            string src = @"b = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category("Update")]
        public void T08_TestCyclicReference()
        {
            String errmsg = "1460274 - Sprint 18 : rev 1590 : Update issue : Cyclic dependency cases are going into infinite loop";
            string src = @"a;
b;
[Associative]
{
	a = 2;
        b = a *3;
        a = 6.5;
        a = b / 3; 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.VerifyRunScriptSource(src, errmsg);
            thisTest.Verify("b", null);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_TestInNestedBlock()
        {
            string src = @"a;
b;
f;
g1;
g3;
d;
c;
e;
[Associative]
{
	a = 4;
	b = a + 2;
	[Imperative]
	{
		b = 0;
		c = 0;
		if ( a == 4 )
		{
			b = 4;
		}			
		else
		{
			c = 5;
		}
		d = b;
		e = c;	
                g2 = g1;	
	}
	f = a * 2;
        g1 = 3;
        g3 = g2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", 4);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("g1").Payload == 3);
            Assert.IsTrue(mirror.GetValue("g3").DsasmValue.IsNull);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_TestInFunctionScope()
        {
            string src = @"test;
[Associative]
{
	 def add:double( n1:int, n2:double )
	 {
		  
		  return = n1 + n2;
	 }
	 test = add(2,2.5);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("test").Payload == 4.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_TestInClassScope()
        {
            string src = @"
                                 class A 
                                 {
                                      
                                      P1:int;
                                      constructor A(p1:int)
                                      {
                                          P1 = p1;
                                      }
          
                                 }
                                 a1 = A.A(2);
                                 b1 = a1.P1;
                            
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("b1").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_TestUsingMathAndLogicalExpr()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
              {
                  string src = @"[Associative]
{
  e = 0;
  a = 1 + 2;
  b = 0.1 + 1.9;
  b = a + b;
  c = b - a - 1;
  d = a + b -c;
  if( c < a )
  {
     e = 1;
  }
  else
  {
    e = 2;
  }
  if( c < a || b > d)
  {
     e = 3;
  }
  else
  {
    e = 4;
  }
  if( c < a && b > d)
  {
     e = 3;
  }
  else
  {
    e = 4;
  }
}
";
                  ExecutionMirror mirror = thisTest.RunScriptSource(src);
                  Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
                  Assert.IsTrue(Convert.ToDouble(mirror.GetValue("b").Payload) == 5);
                  Assert.IsTrue(Convert.ToDouble(mirror.GetValue("c").Payload) == 1);
                  Assert.IsTrue(Convert.ToDouble(mirror.GetValue("d").Payload) == 7);
                  Assert.IsTrue(Convert.ToDouble(mirror.GetValue("e").Payload) == 3);
              });
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_TestUsingMathAndLogicalExpr()
        {
            string src = @"a;
f;
[Associative]
{
  a = 3.5;
  b = 1.5;
  c = a + b; 
  d = a - c;
  e = a * d;
  f = a / e; 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 3.5);
            Assert.IsTrue((double)mirror.GetValue("f").Payload == -0.66666666666666663);
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_TestUsingMathAndLogicalExpr()
        {
            string src = @"a;
f;
c1;
c2;
c3;
[Associative]
{
  a = 3;
  b = -4;
  c = a + b; 
  d = a - c;
  e = a * d;
  f = a / e; 
  
  c1 = 1 && 2;
  c2 = 1 && 0;
  c3 = null && true;
  
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("f").Payload) == 0);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c1").Payload));
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c2").Payload) == false);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c3").Payload) == false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_TestInRecursiveFunctionScope()
        {
            string src = @"val;
[Imperative]
{
	
	def fac : int ( n : int )
    {
        if(n == 0 )
        {
			return = 1;
        }
		//return = 2;
		return = n * fac (n-1 );
	}
    val = fac(5);				
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 120);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_TestInvalidSyntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
	x = ;
        y = x;
        z == 4;
       			
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_TestInvalidSyntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
	_rt = 3;
       ""dg"" = 4;
       w = 2
       f = 3;
       v = v;
       			
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T18_TestMethodCallInExpr()
        {
            string src = @"test0;
test1;
test2;
test3;
test4;
test5;
[Associative]
{
	def mul : double ( n1 : int, n2 : int )
    {
      	return = n1 * n2;
    }
    def add : double( n1 : int, n2 : double )
    {
       	return = n1 + n2;
    }
    test0 = add (-1 , 7.5 ) ;
    test1 = add (mul(1,2), 4.5 ) ;  
    test2 = add (mul(1,2.5), 4 ) ; 
    test3 = add (add(1.5,0.5), 4.5 ) ;  
    test4 = add (1+1, 4.5 ) ;
    test5 = add ( add(1,1) + add(1,0.5), 3.0 ) ;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("test0").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test1").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test2").Payload == 7.0);
            Assert.IsTrue((double)mirror.GetValue("test3").Payload == 7.5);
            Assert.IsTrue((double)mirror.GetValue("test4").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test5").Payload == 7.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_TestAssignmentToCollection()
        {
            string src = @"a;
b;
[Associative]
{
	a = {{1,2},3.5};
	c = a[1];
	d = a[0][1];
        a[0][1] = 5;
       	b = a[0][1] + a[1];	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 8.5);
            //Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_TestInvalidSyntax()
        {
            string src = @"a;
b;
[Associative]
{
	a = 2;;;;;
    b = 3;
       			
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_TestAssignmentToBool()
        {
            string src = @"a;
b;
[Associative]
{
	a = true;
    b = false;      			
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // need to capture the warning
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("a").Payload));
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b").Payload) == false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_TestAssignmentToNegativeNumbers()
        {
            string src = @"a;
b;
c;
d;
e;
[Associative]
{
	a = -1;
	b = -111;
	c = -0.1;
	d = -1.99;
	e = 1.99;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -111);
            Assert.IsTrue((double)mirror.GetValue("c").Payload == -0.1);
            Assert.IsTrue((double)mirror.GetValue("d").Payload == -1.99);
            Assert.IsTrue((double)mirror.GetValue("e").Payload == 1.99);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_TestUsingMathAndLogicalExpr()
        {
            string src = @"c1;
c2;
c3;
c4;
[Associative]
{
  a = -3.5;
  b = -4;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("c1").Payload == -7.5);
            Assert.IsTrue((double)mirror.GetValue("c2").Payload == 0.5);
            Assert.IsTrue((double)mirror.GetValue("c3").Payload == 14.0);
            Assert.IsTrue((double)mirror.GetValue("c4").Payload == 0.875);
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_TestUsingMathematicalExpr()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string src = @"c1;
c2;
c3;
c4;
[Associative]
{
  a = 3;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("c1", 5);
            thisTest.Verify("c2", 1);
            thisTest.Verify("c3", 6);
            thisTest.Verify("c4", 1.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_TestUsingMathematicalExpr()
        {
            string src = @"c1;
c2;
c3;
c4;
[Associative]
{
  a = 3.0;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("c1").Payload == 5);
            Assert.IsTrue((double)mirror.GetValue("c2").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("c3").Payload == 6);
            Assert.IsTrue((double)mirror.GetValue("c4").Payload == 1.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_Negative_TestPropertyAccessOnPrimitive()
        {
            string src = @"x = 1;
y = x.a;
x1;
y1;
[Imperative]
{
    x1 = 1;
    y1 = x1.a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue(mirror.GetValue("y").DsasmValue.IsNull);
            Assert.IsTrue((Int64)mirror.GetValue("x1").Payload == 1);
            Assert.IsTrue(mirror.GetValue("y1").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_Defect_1450854()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
    a = 1;
    b = 2;
    c = 0;
	
    if (3 == a ^ b )
    {
        c = 3;
    }
    else
    {
        c = 0;
    }	
		
} 
";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            });
        }
        [Ignore]
        public void T27_Defect_1450847()
        {
            string src = @"[Imperative]
{
	 a = 2;
	 b = 0;
	 c = -1;
	 d = null;
	 
	 a1 = ~a; 
	 b1 = ~b;
	 c1 = ~c;
	 d1 = ~d;
	 
	 e = -0.5;
	 e1 = ~e1;
	 
	 f1 = ~e + ~a;
 
 }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a1").Payload == -3);
            Assert.IsTrue((Int64)mirror.GetValue("b1").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("c1").Payload == 0);
            Assert.IsTrue(mirror.GetValue("d1").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("e1").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("f1").DsasmValue.IsNull);
        }
        [Ignore]
        public void T29_Defect_1449887()
        {
            string src = @"[Associative]
{  
	a = 14;  
	b = 7;  
	c = a & b; 
	d = a|b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 15);
        }
        [Ignore]
        public void T30_Defect_1449887_2()
        {
            string src = @"[Associative]
{   
 def ANDfunc:int(a:int,b:int)
 { 
  
  return = a & b; 
 
 }
 def ORfunc:int(a:int,b:int)
 {
   
   return = a|b;
 
 }
 
 e = 14;
 
 f = 7;
 
 c = ANDfunc(e,f); 
 
 d = ORfunc(e,f);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 15);
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Defect_1449877()
        {
            string src = @"c;
d;
e;
[Associative]
{
	a = -1;
	b = -2;
	c = -3;
	c = a * b * c;
	d = c * b - a;
	e = b + c / a;
	f = a * b + c;
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == -6);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 13);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("e").Payload) == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T32_Defect_1449877_2()
        {
            string src = @"d;
[Associative]
{
	def func:int(a:int,b:int)
	{
	return = b + a;
	}
	a = 3;
	b = -1;
	d = func(a,b);
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T33_Defect_1450003()
        {
            string src = @"_a_test;
_b;
_c;
[Associative]
{
	def check:double( _a:double, _b:int )
	{
	_c = _a * _b;
	return = _c;
	} 
	_a_test = check(2.5,5);
	_b = 4.5;
	_c = true;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("_a_test", 12.5);
            thisTest.Verify("_b", 4.5);
            thisTest.Verify("_c", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Defect_1450727()
        {
            string src = @"z;
[Associative]
{
	x = -5.5;
	y = -4.2;
 
	z = x + y;
 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("z").Payload == -9.7);
        }

        [Test]
        [Category("SmokeTest")]
        public void T35_Defect_1450727_2()
        {
            string src = @"z;
[Associative]
{
	def neg_float:double(x:double,y:double)
	{
	a = x;
	b = y;
	return = a + b;
	}
	z = neg_float(-2.3,-5.8);
 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("z").Payload == -8.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T36_Defect_1450555()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
	a = true;
	b = 2;
	c = 2;
 
	if( a )
	b = false;
 
	if( b==0 )
	c = 1;
 }";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T37_TestOperationOnNullAndBool()
        {
            string src = @"b;
[Associative]
{
	a = true;
	b = a + 1;
	c = null + 2;
 }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T38_Defect_1449928()
        {
            string src = @"c;
[Associative]
{
 a = 2.3;
 b = -6.9;
 c = a / b;
} 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("c").Payload == -0.33333333333333331);
        }

        [Test]
        [Category("SmokeTest")]
        public void T39_Defect_1449704()
        {
            string src = @"a;
[Associative]
{
 a = b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T40_Defect_1450552()
        {
            string src = @"a;
[Associative]
{
 a = b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
        }

        [Test]
        [Category("SmokeTest")]
        public void T41__Defect_1452423()
        {
            string src = @"d;
[Associative]
{
	b = true;
	c = 4.5;
	d = c * b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("d").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T42__Defect_1452423_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
	a = { -2,3,4.5,true };
	x = 1;
	for ( y in a )
	{
		x = x *y;       //incorrect result
    }
	
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T43__Defect_1452423_3()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
	a = 0;
	while ( a == false )
	{
		a = 1;
	}
	
	b = a;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T44__Defect_1452423_4()
        {
            string src = @"x;
[Associative]
{
	y = true;
	x = 1 + y;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("x").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T45__Defect_1452423_5()
        {
            string src = @"a;
[Associative]
{
	a = 4 + true;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T46_TestBooleanOperationOnNull()
        {
            string src = @"a;
b;
c;
d;
[Imperative]
{
	a = null;
	b = a * 2;
	c = a && 2;	
	d = 0;
	if ( a && 2 == 0)
	{
        d = 1;
	}
	else
	{
	    d = 2;
	}
	
	if( !a )
	{
	    d = d + 2;
	}
	else
	{
	    d = d + 1;
	}
	if( a )
	{
	    d = d + 3;
	}
	
	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 3);

        }

        [Test]
        [Category("SmokeTest")]
        public void T47_TestBooleanOperationOnNull()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
	a = false;
        b = 0;
	d = 0;
	if( a == null)
	{
	    d = d + 1;
	}
    if( b == null)
	{
	    d = d + 1;
	}	
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 0);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T48_MultipleAssignments()
        {
            string code = @"
class A
{
    i : int;
	constructor A ( a : int)
	{
	     t1 = t2 = 2;
		 i = t1 + t2 + a ;
	}
	
	def foo : int ( )
	{
	    t1 = t2 = 2;
		t3 = t1 + t2 + i;
        return  = t3;		
	}
	
}
def foo : int ( a : int )
{
    t1 = t2 = 2;
	return = t1 + t2 + a ;
}
a;
b;
x;
y;
a1;
b1;
[Associative]
{
    
	a = b = 4;
    x = y = foo(1);
	a1 = A.A(1);
	b1 = a1.foo();
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
            thisTest.Verify("b", 4);
            thisTest.Verify("x", 5);
            thisTest.Verify("y", 5);
            thisTest.Verify("b1", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1455264()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[associative]
{
	a = 1;
	
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T50_Defect_1456713()
        {
            string code = @"
a = 2.3;
b = a * 3;
//Expected : b = 6.9;
//Recieved : b = 6.8999999999999995;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 6.9, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T51_Using_Special_Characters_In_Identifiers()
        {
            // Assert.Fail("1465125 - Sprint 21 : rev 2294 : @ is not allowed as a starting symbol for identifiers ");
            string code = @"
@a = 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("@a", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Negative_Associative_Syntax()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
	x = 1;
	y = {Associative]
	{
	   return = x + 1;
	}
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Collection_Indexing_On_LHS_Using_Function_Call()
        {
            // Assert.Fail("DNL-1467064 - Sprint 23 : rev 2607 : array element cannot be indexed using function on the LHS of an assignment statement");
            string code = @"
def foo()
{
    return = 0;
}
x = { 1, 2 };
x[foo()] = 3;
y = x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = { 3, 2 };
            thisTest.Verify("y", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T033_Wrong_Spelling_Of_Language_Block()
        {
            // Assert.Fail("DNL-1467065 - Sprint23 : rev :2610 : 'invalid hydrogen' error message coming from wrong spelling of language scope name");
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    string code = @"
[associative]
{
    a= 1;
}
";
                    ExecutionMirror mirror = thisTest.RunScriptSource(code);
                });
        }

        [Test]
        [Category("SmokeTest")]
        public void T54_Associative_Nested_deffect_1467063()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Associative]
{        a = 4;
         b = 2;
     [Associative]
	 {
       b = a + 4;
     }
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_Language_specifier_invalid_1467065_1()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[associative]
{    a= 1;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Language_specifier_invalid_1467065_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[imperative]
{    a= 1;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("Negative")]
        [Category("Failure")]
        public void T55_Associative_assign_If_condition_1467002()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3941
            String errmsg = "[Design Issue] conditionals with empty arrays and ararys with different ranks";
            string src = @"[Associative]
{
	x = {} == null;
}
";
            thisTest.VerifyRunScriptSource(src, errmsg);

            Object n1 = null;
            thisTest.Verify("x", n1);

        }

        [Test]
        [Category("Negative")]
        public void T56_Defect_1467242()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
@"
a = 2;
b = 4;
if(a == 2)
    b = 3;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("Negative")]
        public void T56_Defect_1467242_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
@"
a = {0, 1};
for(i in a)
{
    a[i] = 0;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("Negative")]
        public void T56_Defect_1467242_3()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
@"
i = 0;
while(i < 3)
{
    i = i + 1;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("Negative")]
        public void T57_Defect_1467255()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
@"
a = 0;
b = 10;
c = 2
y1 = a..b..2;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T58_Modifier_Block_On_User_Defined_Classes()
        {
            String code =
            @"
class B
{
    x:int;
	constructor B ( x1)
	{
	    x = x1;
	}
}
class A
{
    b : B;
	d : bool;
	constructor A ( bb:B, d1:bool)
	{
	    b = bb;
		d = d1;
	}
	def foo ( x1,x2,x3)
	{
	    b1 = B.B(x1+x2+x3);
		return = A.A(b1, d);
	}
	def Scale ( xx )
	{
	    b2 = B.B(xx+b.x);
		return = A.A(b2, d);
	}
}
bb = B.B(1);
a =
{
    A.A(bb,false).foo(10,0,0) => a1;
    a1.Scale(2);
}
test = a.b.x;
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("test", 12);
        }

        [Test]
        public void T59_Defect_1467540()
        {
            String code =
            @"
c1 = 0;
x = 0..1;
y = x[c];
";
            Object n1 = null;
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", n1);
        }

        [Test]
        public void T59_Defect_1467540_2()
        {
            String code =
            @"
y;
[Imperative]
{
    c1 = 0;
    x = 0..1;
    y = x[c];
}
";
            Object n1 = null;
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", n1, 0);
        }

        [Test]
        public void T59_Defect_1467540_3()
        {
            String code =
            @"
def foo ()
{
    c1 = 0;
    x = 0..1;
    y = x[c];
    return = y;
}
test = y;
";
            Object n1 = null;
            thisTest.RunScriptSource(code);
            thisTest.Verify("test", n1);
        }

        [Test]
        public void T60_Defect_1467525()
        {
            String code =
            @"
def Unflatten : var[][](input1Darray : var[], length : int)
{
    return2Darray = { };
    
    iCount = Count(input1Darray) / length;
    index = 0;
  
    for(i in 0..iCount)
    {
        for(j in 0..length)
        {
            returm2Darray[i][j] = input1Darray[index];
            
            index = index + 1;
        }    
    }
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T60_Defect_1467525_2()
        {
            String code =
            @"
c = 0;
a = 2;
for(i in 0..a)
{
    c = c +1;
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T60_Defect_1467525_3()
        {
            String code =
            @"
c = 0;
a = 2;
while(c < 1)
{
    c = c +1;
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T60_Defect_1467525_4()
        {
            String code =
            @"
c = 0;
a = 2;
if(c < 1)
{
    c = c +1;
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T60_Defect_1467525_5()
        {
            String code =
            @"
class A
{
    c : int;
    constrcutor A ()
    {
        c = 0;
        a = 2;
        if(c < 1)
        {
            c = c +1;
        }
    }
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T60_Defect_1467525_6()
        {
            String code =
            @"
class A
{
    
    def foo ()
    {
        c = 0;
        a = 2;
        if(c < 1)
        {
            c = c +1;
        }
    }
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T61_Defect_1467546_1()
        {
            String code =
            @"
a = 10
b = a + 1;
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("b", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_2()
        {
            String code =
            @"
[Imperative]
{
a = 10
b = a + 1;
}
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("b", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_3()
        {
            String code =
            @"
def foo ()
{
  a = 10
  b = a + 1;
}
test = foo();
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_4()
        {
            String code =
            @"
class A
{
    static def foo ()
    {
      a = 10
      b = a + 1;
    }
}
test = A.foo();
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_5()
        {
            String code =
            @"
class A
{
    constructor A ()
    {
      a = 10
      b = a + 1;
    }
}
test = A.A();
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_6()
        {
            String code =
            @"
x = 1;
a = 10
b = a + 1;
 
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_7()
        {
            String code =
            @"
def foo()
{
    return = 1;
}
x = 1;
a = foo()
b = a + 1;
 
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T61_Defect_1467546_8()
        {
            String code =
            @"
class A
{
    static def foo()
    {
        return = 1;
    }
}
x = 1;
a = A.foo()
b = a + 1;
 
";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                Object n1 = null;
                thisTest.RunScriptSource(code);
                thisTest.Verify("test", n1);
            });
        }

        [Test]
        public void T64_Defect_1467588()
        {
            String code =
            @"
class A
{
    static def foo()
    {
        return = ""Hello \""DesignScript\""!"";
    }
}
def foo()
{
    return = ""Hello \""DesignScript\""!"";
}
def foo2(s : string)
{
    return = ""New Hello \""DesignScript\""!"";
}
a = A.foo();
b = foo();
c = [Associative]
{
    return = [Imperative]
    {
        return = ""Hello \""DesignScript\""!"";
    }
}
d = foo2(""Hello \""DesignScript\""!"");
 
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", "Hello \"DesignScript\"!");
            thisTest.Verify("b", "Hello \"DesignScript\"!");
            thisTest.Verify("c", "Hello \"DesignScript\"!");
            thisTest.Verify("d", "New Hello \"DesignScript\"!");
        }

        [Test]
        public void T65_Defect_1467597()
        {
            String code =
            @"
class A 
{
    a;;
    constructor A ( x )
    {
        a = x;;
    }
    static def foo()
    {
        returnValue = 0;
        [Imperative]
        {
            for(i in { 1, 2 })
            {
                returnValue = returnValue + i;; 
            }
        }
        return = returnValue;
    }  
}
x = A.foo(); 
y = A.A(1).a;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);
            thisTest.Verify("y", 1);

        }

        [Test]
        public void T66_Defect_1467597()
        {
            String code =
            @"
class A 
{
    a = { }; ;
    b : int[]; ;
    c = 4..6; ;
    d = { 0, 2 }; ;
    f : var[]; ;
    constructor A ( x )
    {
        a = { x }; ;
        i = 2..3; ;
        b[i] = i; ;
        c[i] = i; ;
        d[i] = i; ;
        f[i] = d[i]; ;
    }
    def foo()
    {
        returnValue = 0;
        [Imperative]
        {
            for(i in { 0,1 })
            {
                returnValue = returnValue + i; ;
                b[i] = i;;
                c[i] = i;;
                d[i] = i;;
                f[i] = d[i]; ;
                
            }
        }
        return = returnValue;
    }  
}
x = A.A(1); ;
y1 = x.a; ;
y2 = x.b; ;
y3 = x.c; ;
y4 = x.d; ;
y5 = x.f; ;
y6 = x.foo(); ;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y1", new Object[] { 1 });
            thisTest.Verify("y2", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("y3", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("y4", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("y5", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("y6", 1);
        }

    }
}

