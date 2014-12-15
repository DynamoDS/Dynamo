using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Imperative
{
    class Assignment : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_SampleTestUsingCodeWithinTestFunction()
        {
            String code =
             @"variable;[Imperative]
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
[Imperative]
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
[Imperative]
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
                string src = @"[Imperative]
{
    b = if (2==2) { a = 0; }
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_TestRepeatedAssignment_negative()
        {
            string src = @"a;
b;
[Imperative]
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
            string err = "1460274 - Sprint 18 : rev 1590 : Update issue : Cyclic dependency cases are going into infinite loop";
            string src = @"a;
b;
[Imperative]
{
	a = 2;
        b = a *3;
        a = 6.5;
        a = b / 3; 
}
";
            thisTest.VerifyRunScriptSource(src, err);
            thisTest.Verify("b", 6);
            thisTest.Verify("a", 2.0);
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
[Imperative]
{
	a = 4;
	b = a + 2;
    [Associative]
    {
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
    }
	f = a * 2;
    g1 = 3;
    g3 = g2;
      
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("g1").Payload == 3);
            Assert.IsTrue(mirror.GetValue("g3").DsasmValue.IsNull);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("g1").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_TestInFunctionScope()
        {
            string src = @"test;
[Imperative]
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
            string src = @"a;
b;
c;
d;
e;
[Imperative]
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
            Assert.IsTrue(Convert.ToDouble(mirror.GetValue("e").Payload) == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_TestUsingMathAndLogicalExpr()
        {
            string src = @"a;
b;
[Imperative]
{
  a = 3.5;
  b = 1.5;
  b = a + b; 
  b = a - b;
  b = a * b;
  b = a / b; 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 3.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == -0.66666666666666663);
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
[Imperative]
{
  a = 3;
  b = -4;
  b = a + b; 
  b = a - b;
  b = a * b;
  b = a / b; 
  
  c1 = 1 && 2;
  c2 = 1 && 0;
  c3 = null && true;
  
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue(Convert.ToInt64(mirror.GetValue("b").Payload) == 0);
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
                string src = @"[Imperative]
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
                string src = @"[Imperative]
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
[Imperative]
{
	   def  mul : double ( n1 : double, n2 : double )
        {
        	return = n1 * n2;
        }
        def add : double ( n1 : double, n2 : double )
        {
        	return = n1 + n2;
        }
        test0 = add (-1 , 7.5 ) ;
        test1 = add ( mul(1,2), 4.5 ) ;  
        test2 = add (mul(1,2.5), 4 ) ; 
        test3 = add (add(1.5,0.5), 4.5 ) ;  
        test4 = add (1+1, 4.5 ) ;
        test5 = add (add(1,1)+add(1,0.5), 3.0 ) ;
       
       			
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("test0").Payload == 6.5); //failing here
            Assert.IsTrue((double)mirror.GetValue("test1").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test2").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test3").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test4").Payload == 6.5);
            Assert.IsTrue((double)mirror.GetValue("test5").Payload == 6.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_TestAssignmentToCollection()
        {
            string src = @"a;
b;
[Imperative]
{
	a = {{1,2},3.5};
	c = a[1];
	d = a[0][1];
        a[0][1] = 5;
       	b = a[0][1] + a[1];
        a = 2;		
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 8.5);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_TestInvalidSyntax()
        {
            string src = @"a;
b;
[Imperative]
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
[Imperative]
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
[Imperative]
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
[Imperative]
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
            string src = @"c1;
c2;
c3;
c4;
[Imperative]
{
  a = 3;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
}
";
            thisTest.RunScriptSource(src);
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
[Imperative]
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
        [Ignore]
        public void T26_Defect_1450854()
        {
            string src = @"[Imperative]
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
            Assert.IsTrue(mirror.GetValue("d").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("e1").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("f1").DsasmValue.IsNull);


        }
        [Ignore]
        public void T29_Defect_1449887()
        {
            string src = @"[Imperative]
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
            string src = @"[Imperative]
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
[Imperative]
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
[Imperative]
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
[Imperative]
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
[Imperative]
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
[Imperative]
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
            string src = @"c;
[Imperative]
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
        }

        [Test]
        [Category("SmokeTest")]
        public void T37_TestOperationOnNullAndBool()
        {
            string src = @"b;
[Imperative]
{
	a = true;
	b = a + 1;
	c = null + 2;
 }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            //Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T38_Defect_1449928()
        {
            string src = @"c;
[Imperative]
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
[Imperative]
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
[Imperative]
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
[Imperative]
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
            string src = @"x;
[Imperative]
{
	a = { -2,3,4.5,true };
	x = 1;
	for ( y in a )
	{
		x = x *y;       //incorrect result
    }
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("x").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T43__Defect_1452423_3()
        {

            string src = @"b;
[Imperative]
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
        }

        [Test]
        [Category("SmokeTest")]
        public void T44__Defect_1452423_4()
        {
            string src = @"x;
[Imperative]
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
[Imperative]
{
	a = 4 + true;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
        }

        [Test]
        [Category("Failure")]
        [Category("SmokeTest")]
        public void T46_TestBooleanOperationOnNull()
        {
            string src = @"
a;b;c;d;
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
	if( a )
	{
	    d = d + 1;
	}
	
	
	
}";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4080
            string err = "if condition fails to evaluate a null";
            ExecutionMirror mirror = thisTest.RunScriptSource(src, err);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T47_TestBooleanOperationOnNull()
        {
            string src = @"d;
[Imperative]
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
a;
b;
x;
y;
b1;
[Imperative]
{
    def foo : int ( a : int )
	{
	    t1 = t2 = 2;
		return = t1 + t2 + a ;
	}
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
        public void T49_TestForStringObjectType()
        {
            //Assert.Fail("1455594 - Sprint15 : Rev 804: String object type is missing in the new language ");
            string code = @"
b;
[Associative]
{
    def foo : string (x : string )
	{
	   return = x; 		
	}
    a = ""sarmistha"";
    b = foo ( a );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", "sarmistha", 0);
        }
        [Ignore]
        public void T50_Defect_1449889()
        {
            string code = @"
[Imperative]
{
	a = 3;
	b = 2;
	c = a & b;
	d = 5;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T51_Assignment_Using_Negative_Index()
        {
            string code = @"
a = { 0, 1, 2, 3 };
c1 = a [-1];
c2 = a [-2];
c3 = a [-3];
c4 = a [-4];
c5 = a [-5];
c6 = a [-1.5];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c1", 3, 0);
            thisTest.Verify("c2", 2, 0);
            thisTest.Verify("c3", 1, 0);
            thisTest.Verify("c4", 0, 0);
            Assert.IsTrue(mirror.GetValue("c5").DsasmValue.IsNull);
            thisTest.Verify("c6", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1449889()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string code = @"
a;
c;
d;
[Imperative]
{
	a = b;
    c = foo();
	d = 1;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);
            thisTest.Verify("d", 1);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Defect_1454691()
        {
            string code = @"
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = add_1(a1);
	}
	
	def add_1 ( x )
	{
	    return  = x + 1;
	}
	
}
[Imperative]
{
	A1 = A.CreateA(1);
	a = A1.a;
}
a;
x;
b;
[Associative]
{
    x = 3;
	A1 = A.CreateA(x);
	a = A1.a;
	b = [Imperative]
	{
	    if ( a < 10 )
		{
		    B1 = A.CreateA(a);
			return = B1.a;
		}
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);
            thisTest.Verify("a", 4);
            thisTest.Verify("b", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T54_Defect_1454691()
        {
            object[] expectedResult = { 6, 5 };
            string code = @"
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = add_1(a1);
	}
	
	def add_1 ( x )
	{
	    return  = x + 1;
	}
	
}
class B extends A
{
	b : var;
	constructor CreateB ( a1 : int )
	{
		b = a1 + 1;
		a = b + 1 ; //add_1(b);
	}
	
}
x;
a1;
a2;
b1;
b2;
[Imperative]
{
	A1 = A.CreateA(1);
	a1 = A1.a;
	B1 = B.CreateB(1);
	a2 = B1.a;
	b2 = B1.b;
}
def foo ( x ) 
{
    return = x;
}
c=[Associative]
{
    x = 3;
	A1 = A.CreateA(x);
	a1 = A1.a;
	B1 = B.CreateB(x);
	b1 = B1.b;
	a2 = B1.a;
	
	b = [Imperative]
	{
	    if ( a1 < 10 )
		{
		    B1 = B.CreateB(a1);
			return = { B1.a, B1.b };
		}
		return = 0;
	}
	return = b;
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);
            thisTest.Verify("a1", 4);
            thisTest.Verify("a2", 6);
            thisTest.Verify("b1", 5);
            thisTest.Verify("b2", 2);
            thisTest.Verify("c", expectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void T55_Defect_1454691()
        {

            string code = @"
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = a1;
	}
	
	
}
y1;
y2;
y3;
y4;
[Imperative]
{
    y1 = 0;
	x = 1;
	while ( x != 2 )
	{
	    t1 = A.CreateA(x);
		y1 = y1 + t1.a;
		x = x + 1;	    
	}
	
	y2 = 0;
	c = { 3, 4 };
	for ( i in c )
	{
	    t1 = A.CreateA(i);
		y2 = y2 + t1.a;
	}
	
	y3 = 1;
	if( y3 < 2 )
	{
	    while ( y3 <= 2 )
		{
			t1 = A.CreateA(y3);
			y3 = y3 + t1.a;			    
		}
	}
	
	y4 = 1;
	if( y4 > 20 )
	{
	    y4 = -1;
	}
	else
	{
	    t1 = A.CreateA(y4);
		y4 = y4 + t1.a;
	}
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y1", 1);
            thisTest.Verify("y2", 7);
            thisTest.Verify("y3", 4);
            thisTest.Verify("y4", 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T56_Defect_1454691()
        {
            string code = @"
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = add_1(a1);
	}
	
	def add_1 ( x )
	{
		return  = x + 1;
	}
}
b;
[Associative]
{
    x = 3;
	A1 = A.CreateA(x);
	a1 = A1.a;
	b = [Imperative]
	{
		if ( a1 < 10 )
		{
			return = A1.a;
		}
	return = A1.a + 1;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T57_Defect_1454691_2()
        {
            string code = @"
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = a1 + 1;
	}
}
def foo ( )
{
    x = A.CreateA(1);
 	a1 = x.a; 
	
	y = [Imperative]
	{
		if ( a1  < 10 )
		{
			x1 = A.CreateA(2);
			a2 = x1.a; 
			return = a2;
		}
		return = a1; 
	}
	return = a1 + y; 
}
x2;
[Associative]
{  
	x2 = foo(); 	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x2", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T58_Defect_1454691_3()
        {
            string code = @"
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = a1;
	}
	
	def CreateNewVal ( )
	{
		y = [Imperative]
		{
			if ( a  < 10 )
			{
				x1 = A.CreateA(10);
				y1 = x1.a;
 				return = y1;
			}
			return = a;
		}
		return = a + y;
	}
}
    a1 = A.CreateA(1);
	b1 = a1.CreateNewVal(); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 11);
        }

        [Test]
        [Category("SmokeTest")]
        public void T59_Defect_1455590()
        {
            string code = @"
	a = b = 2;
c;d;e;
	[Imperative]
	{
		c = d = e = 4+1;
	}
		";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 5);
            thisTest.Verify("d", 5);
            thisTest.Verify("e", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T60_Defect_1455590_2()
        {
            string code = @"
def foo ( a )
{
	b = c = a ;
	return = a + b + c;
}
x = foo ( 3 );
y;
[Imperative]
{
	y = foo ( 4 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 9);
            thisTest.Verify("y", 12);

        }

        [Test]
        [Category("SmokeTest")]
        public void T61_TestBooleanOperationOnNull()
        {
            string code = @"
b1;
b2;
b3;
[Imperative]
{
    a = null;
	
	b1 = 0;
	b2 = 1;
	b3 = -1;
	
	if( a == b1 )
	{
	    b1 = 10;
	}
	if ( a < b2 )
	{
		b2 = 10;
	}
	if ( a > b3 )
	{
		b3 = 10;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("b1", 0, 0);
            thisTest.Verify("b2", 1, 0);
            thisTest.Verify("b3", -1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_Defect_1456721()
        {
            string code = @"
b = true;
a = 2 * b;
c = 3;
b1 = null;
a1 = 2 * b1;
c1 = 3;
a2 = 1 + true;
b2 = 2 * true;
c2 = 3  - true;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
            thisTest.Verify("c1", 3, 0);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("a1").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("a2").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("b2").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("c2").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Defect_1452643()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
    b = if (2==2) { a = 0; } // expected 'StatementUsedInAssignment' warning here
	a = 2;
	
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T64_Defect_1450715()
        {
            string code = @"
a;
[Imperative]
{
    a = { 1, 0.5, null, {2,3 } ,{{0.4, 5}, true } };
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 1, 0.5, null, new Object[] { 2, 3 }, new Object[] { new Object[] { 0.4, 5 }, true } };
            thisTest.Verify("a", a);
        }

        [Test]
        [Category("Method Resolution")]
        public void T65_Operation_On_Null()
        {
            String code =
@"
a = null + 1; 
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);

            Object n1 = null;

            thisTest.Verify("a", n1);
        }

        [Test]
        public void T66_Imperative_1467368_negative()
        {
            String code =
            @"
            [Imperative] 
            {
                [Imperative]
                {
                    a = 1; 
                }
            }
            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "1467368 - Imperative inside Imperative goes into loop ";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            });
        }

        [Test]
        public void T66_Associative_1467368_negative_2()
        {
            String code =
            @"
            [Associative] 
            {
                [Associative]
                {
                    a = 1; 
                }
            }
            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "1467368 - Imperative inside Imperative goes into loop ";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            });
        }

        [Test]
        public void T67_DNL_1467458()
        {
            String code =
            @"
            class A
{
    x:int;
    def foo(a : int)
    {
        x = a;
        return = x;
    }
}
b;
[Imperative]
{
    p = A.A();
    p.foo(9);
    b = p.x;
}
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 9);
        }


        [Test]
        public void T67_DNL_1467458_3()
        {
            String code =
            @"
            class A
{
    a;
    def foo()
    {
        a = 1;
        return = a;
    }
}
c;
class B{
    b;
    def foo()
    {
        b = 5;
        return = b;
    }
}
[Imperative]
{
    p = { A.A(),B.B() };
    {p[0].foo(),p[1].foo()};  // compilation error
    c = { p[0].a, p[1].b };
}
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", new object[] { 1, 5 });
        }

        [Test]
        public void T67_DNL_1467458_4()
        {
            String code =
            @"
 class B{ a = 1; }
class A
{
    b;
    c;
    def foo()
    {
        b = 1;
        [Imperative]
        {
            p = B.B();
            p.foo();
            c =  p.a;
        }
        return = c;
    }
}
z = A.A();
y =z.foo();
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("y", 1);
        }

        [Test]
        public void T68_DNL_1467523()
        {
            String code =
            @"
class A
{
}
[Imperative]
{
    arr1 : double[] ;
    arr2 : double[] = null;
    arr3 : double[]..[];
    arr4 : double[]..[] = null;
    arr5 : double[]  = { };
    arr6 : double[]..[]  = { };
    arr11 : int[] ;
    arr12 : int[] = null;
    arr13 : int[]..[];
    arr14 : int[]..[] = null;
    arr15 : int[]  = { };
    arr16 : int[]..[]  = { };
    arr111 : bool[] ;
    arr112 : bool[] = null;
    arr113 : bool[]..[];
    arr114 : bool[]..[] = null;
    arr115 : bool[]  = { };
    arr116 : bool[]..[]  = { };
    arr1111 : A[] ;
    arr1112 : A[] = null;
    arr1113 : A[]..[];
    arr1114 : A[]..[] = null;
    arr1115 : A[]  = { };
    arr1116 : A[]..[]  = { };
   
}
dummy = 1;
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("dummy", 1);
        }
    }


}
