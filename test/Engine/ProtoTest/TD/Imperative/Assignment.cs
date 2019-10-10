using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
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
            thisTest.Verify("variable", null);
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
            thisTest.Verify("a", null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.IdUnboundIdentifier);
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
b = [Imperative]
{
    b = a = 2;
    return b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b", 2);
            thisTest.Verify("a", null);

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
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("Update")]
        public void T08_TestCyclicReference()
        {
            string err = "1460274 - Sprint 18 : rev 1590 : Update issue : Cyclic dependency cases are going into infinite loop";
            string src = @"a;
b = [Imperative]
{
	a = 2;
        b = a *3;
        a = 6.5;
        a = b / 3; 
    return b;
}
";
            thisTest.VerifyRunScriptSource(src, err);
            thisTest.Verify("b", 6);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_TestInNestedBlock()
        {
            string src = @"
i = [Imperative]
{
	a = 4;
	b = a + 2;
    i = [Associative]
    {
        return [Imperative]
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
            return [a, b, g1, g2, d, c, e];
        }

    }
    a = i[0];
    b = i[1];
    g2 = i[3];
    d = i[4];
    c = i[5];
    e = i[6];
	f = a * 2;
    g1 = 3;
    g3 = g2;
    return [a, b, f, g1, g3, d, c, e];
};
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            var arr = new object[] {4, 4, 8, 3, null, 4, 0, 0};
            thisTest.Verify("i", arr);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_TestInFunctionScope()
        {
            string src = @"
	 def add:double( n1:int, n2:double )
	 {
		  
		  return = n1 + n2;
	 }
test = [Imperative]
{
	 test = add(2,2.5);
     return test;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("test",4.5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T11_TestInClassScope()
        {
            string src = @"
                                import (""FFITarget.dll"");
                                 a1 = ClassFunctionality.ClassFunctionality(2);
                                 b1 = a1.IntVal;
                            
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b1", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_TestUsingMathAndLogicalExpr()
        {
            string src = @"
i = [Imperative]
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
  return [a, b, c, d, e];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            var arr = new[] {3, 5, 1, 7, 4};
            thisTest.Verify("i", arr);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_TestUsingMathAndLogicalExpr()
        {
            string src = @"
i = [Imperative]
{
  a = 3.5;
  b = 1.5;
  b = a + b; 
  b = a - b;
  b = a * b;
  b = a / b; 
  return [a, b];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {3.5, -0.66666666666666663});
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_TestUsingMathAndLogicalExpr()
        {
            string src = @"
i = [Imperative]
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
  return [a, b, c1, c2, c3];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            var arr = new object[] {3, 0, true, false, null};
            thisTest.Verify("i", arr);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_TestInRecursiveFunctionScope()
        {
            string src = @"
	def fac : int ( n : int )
	{
        return = [Imperative]
        {
	        if(n == 0 )
            {
		        return = 1;
            }
		    return = n * fac (n-1 );
	    }
    }
val = [Imperative]
{
    val = fac(5);
    return val;				
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("val", 120);
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
            string src = @"
	   def  mul : double ( n1 : double, n2 : double )
        {
        	return = n1 * n2;
        }
        def add : double ( n1 : double, n2 : double )
        {
        	return = n1 + n2;
        }
i = [Imperative]
{
        test0 = add (-1 , 7.5 ) ;
        test1 = add ( mul(1,2), 4.5 ) ;  
        test2 = add (mul(1,2.5), 4 ) ; 
        test3 = add (add(1.5,0.5), 4.5 ) ;  
        test4 = add (1+1, 4.5 ) ;
        test5 = add (add(1,1)+add(1,0.5), 3.0 ) ;
       return [test0, test1, test2, test3, test4, test5];
       			
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {6.5, 6.5, 6.5, 6.5, 6.5, 6.5});
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_TestAssignmentToCollection()
        {
            string src = @"
i=
[Imperative]
{
	a = [[1,2],3.5];
	c = a[1];
	d = a[0][1];
    a[0][1] = 5;
    b = a[0][1] + a[1];
    a = 2;	
    return [b, a];	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {8.5, 2});
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_TestInvalidSyntax()
        {
            string src = @"
i = [Imperative]
{
	a = 2;;;;;
    b = 3;
    return [a, b];		
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {2, 3});
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_TestAssignmentToBool()
        {
            string src = @"
i = [Imperative]
{
	a = true;
    b = false;  
    return [a, b];    			
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // need to capture the warning
            thisTest.Verify("i", new[] {true, false});
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_TestAssignmentToNegativeNumbers()
        {
            string src = @"
i = [Imperative]
{
	a = -1;
	b = -111;
	c = -0.1;
	d = -1.99;
	e = 1.99;
    return [a, b, c, d, e];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] { -1, -111, -0.1, -1.99, 1.99 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_TestUsingMathAndLogicalExpr()
        {
            string src = @"
i = [Imperative]
{
  a = -3.5;
  b = -4;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
  return [c1, c2, c3, c4];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {-7.5, 0.5, 14.0, 0.875});
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_TestUsingMathematicalExpr()
        {
            string src = @"
i = [Imperative]
{
  a = 3;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
  return [c1, c2, c3, c4];
}
";
            thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {5, 1, 6, 1.5});
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_TestUsingMathematicalExpr()
        {
            string src = @"
i = [Imperative]
{
  a = 3.0;
  b = 2;
  c1 = a + b; 
  c2 = a - b;
  c3 = a * b;
  c4 = a / b; 
  return [c1, c2, c3, c4];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] { 5, 1, 6, 1.5 });
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
            thisTest.Verify("c", 3);
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

            thisTest.Verify("a1", -3);
            thisTest.Verify("b1", -1);
            thisTest.Verify("c1", 0);
            thisTest.Verify("d", null);
            thisTest.Verify("e1", null);
            thisTest.Verify("f1", null);


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
            thisTest.Verify("c", 6);
            thisTest.Verify("d", 15);
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
            thisTest.Verify("c", 6);
            thisTest.Verify("d", 15);
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Defect_1449877()
        {
            string src = @"
i = [Imperative]
{
	a = -1;
	b = -2;
	c = -3;
	c = a * b * c;
	d = c * b - a;
	e = b + c / a;
	f = a * b + c;
    return [c, d, e];
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {-6, 13, 4});
        }

        [Test]
        [Category("SmokeTest")]
        public void T32_Defect_1449877_2()
        {
            string src = @"
	def func:int(a:int,b:int)
	{
	    return = b + a;
	}
    d = [Imperative]
    {
    	a = 3;
    	b = -1;
    	d = func(a,b);
        return d;
    } ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T33_Defect_1450003()
        {
            string src = @"
	def check:double( _a:double, _b:int )
	{
	    _c = _a * _b;
	    return = _c;
	} 
    i = [Imperative]
    {
    	_a_test = check(2.5,5);
    	_b = 4.5;
    	_c = true;
        return [_a_test, _b, _c];
    }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new object[] {12.5, 4.5, true});
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Defect_1450727()
        {
            string src = @"
z = [Imperative]
{
	x = -5.5;
	y = -4.2;
 
	z = x + y;
    return z;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z",-9.7);
        }

        [Test]
        [Category("SmokeTest")]
        public void T35_Defect_1450727_2()
        {
            string src = @"
	def neg_float:double(x:double,y:double)
	{
	    a = x;
	    b = y;
	    return = a + b;
	}
    z = [Imperative]
    {
    	z = neg_float(-2.3, -5.8);
        return z;
    }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z",-8.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T36_Defect_1450555()
        {
            string src = @"
c = [Imperative]
{
	a = true;
	b = 2;
	c = 2;
 
	if( a )
	    b = false;
 
	if( b==0 )
	    c = 1;

    return c;
 }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("c", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T37_TestOperationOnNullAndBool()
        {
            string src = @"
i = [Imperative]
{
	a = true;
	b = a + 1;
	c = null + 2;
    return [b, c];
 }";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new object[] {null, null});
        }

        [Test]
        [Category("SmokeTest")]
        public void T38_Defect_1449928()
        {
            string src = @"
c = [Imperative]
{
 a = 2.3;
 b = -6.9;
 c = a / b;
 return c;
} 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("c",-0.33333333333333331);
        }

        [Test]
        [Category("SmokeTest")]
        public void T39_Defect_1449704()
        {
            string src = @"
a = [Imperative]
{
 a = b;
 return a;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
        }
        
        [Test]
        [Category("SmokeTest")]
        public void T41__Defect_1452423()
        {
            string src = @"
d = [Imperative]
{
	b = true;
	c = 4.5;
	d = c * b;
    return d;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("d", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T42__Defect_1452423_2()
        {
            string src = @"
x = [Imperative]
{
	a = [ -2,3,4.5,true ];
	x = 1;
	for ( y in a )
	{
		x = x *y;       //incorrect result
    }
	return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T43__Defect_1452423_3()
        {

            string src = @"
b = [Imperative]
{
	a = 0;
	while ( a == false )
	{
		a = 1;
	}
	
	b = a;
    return b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T44__Defect_1452423_4()
        {
            string src = @"
x = [Imperative]
{
	y = true;
	x = 1 + y;
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T45__Defect_1452423_5()
        {
            string src = @"
a = [Imperative]
{
	a = 4 + true;
    return a;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
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
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            thisTest.Verify("d", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T47_TestBooleanOperationOnNull()
        {
            string src = @"
d = [Imperative]
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
    return d;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("d", 0);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T48_MultipleAssignments()
        {
            string code = @"

    def foo : int ( a : int )
	{
	    t1 = t2 = 2;
		return = t1 + t2 + a ;
	}
i = [Imperative]
{
	a = b = 4;
    x = y = foo(1);
	b1 = a+x;
    return [a, b, x, y, b1];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] {4, 4, 5, 5, 9});
        }

        [Test]
        public void T49_TestForStringObjectType()
        {
            //Assert.Fail("1455594 - Sprint15 : Rev 804: String object type is missing in the new language ");
            string code = @"
b;
def foo : string (x : string )
{
   return = x; 		
}
[Associative]
{
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
a = [ 0, 1, 2, 3 ];
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
            thisTest.Verify("c5", 3, 0);
            thisTest.Verify("c6", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1449889()
        {
            string code = @"
i = [Imperative]
{
	a = b;
    c = foo();
	d = 1;
    return [a, c, d];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new object[] {null, null, 1});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T53_Defect_1454691()
        {
            string code = @"
import (""FFITarget.dll"");
[Imperative]
{
	A1 = ClassFunctionality.ClassFunctionality(1);
	a = A1.IntVal;
}
a;
x;
b;
[Associative]
{
    x = 3;
	A1 = ClassFunctionality.ClassFunctionality(x);
	a = A1.IntVal+1;
	b = [Imperative]
	{
	    if ( a < 10 )
		{
		    B1 = ClassFunctionality.ClassFunctionality(a);
			return = B1.IntVal+1;
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T55_Defect_1454691()
        {

            string code = @"
import(""FFITarget.dll"");
i = [Imperative]
{
    y1 = 0;
	x = 1;
	while ( x != 2 )
	{
	    t1 = ClassFunctionality.ClassFunctionality(x);
		y1 = y1 + t1.IntVal;
		x = x + 1;	    
	}
	
	y2 = 0;
	c = [ 3, 4 ];
	for ( i in c )
	{
	    t1 = ClassFunctionality.ClassFunctionality(i);
		y2 = y2 + t1.IntVal;
	}
	
	y3 = 1;
	if( y3 < 2 )
	{
	    while ( y3 <= 2 )
		{
			t1 = ClassFunctionality.ClassFunctionality(y3);
			y3 = y3 + t1.IntVal;			    
		}
	}
	
	y4 = 1;
	if( y4 > 20 )
	{
	    y4 = -1;
	}
	else
	{
	    t1 = ClassFunctionality.ClassFunctionality(y4);
		y4 = y4 + t1.IntVal;
	}
	return [y1, y2, y3, y4];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] {1, 7, 4, 2});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T56_Defect_1454691()
        {
            string code = @"
import(""FFITarget.dll"");
b = [Associative]
{
    x = 3;
	A1 = ClassFunctionality.ClassFunctionality(x);
	a1 = A1.IntVal;
	return [Imperative]
	{
		if ( a1 < 10 )
		{
			return = A1.IntVal;
		}
	    return = A1.IntVal + 1;
	}
};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T57_Defect_1454691_2()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( )
{
    x = ClassFunctionality.ClassFunctionality(1);
 	a1 = x.IntVal; 
	
	y = [Imperative]
	{
		if ( a1  < 10 )
		{
			x1 = ClassFunctionality.ClassFunctionality(2);
			a2 = x1.IntVal; 
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
            thisTest.Verify("x2", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T58_Defect_1454691_3()
        {
            string code = @"
import (""FFITarget.dll"");
	def CreateNewVal ( )
	{
	    a=1;
		y = [Imperative]
		{
			if ( a  < 10 )
			{
				x1 = ClassFunctionality.ClassFunctionality(10);
				y1 = x1.IntVal;
 				return = y1;
			}
			return = a;
		}
		return = a + y;
	}

	b1 = CreateNewVal(); 
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
	i = [Imperative]
	{
		c = d = e = 4+1;
        return [c, d, e];
	}
		";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("i", new[] {5, 5, 5});
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
y = [Imperative]
{
	return foo ( 4 );
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
i = [Imperative]
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
    return [b1, b2, b3];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("i", new[] {0, 1, -1});
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
            thisTest.Verify("a", null);
            thisTest.Verify("a1", null);
            thisTest.Verify("a2", null);
            thisTest.Verify("b2", null);
            thisTest.Verify("c2", null);
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
a = [Imperative]
{
    return [ 1, 0.5, null, [2,3 ] ,[[0.4, 5], true ] ];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            var a = new Object[] { 1, 0.5, null, new Object[] { 2, 3 }, new Object[] { new Object[] { 0.4, 5 }, true } };
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            string err = "1467368 - Imperative inside Imperative goes into loop ";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T68_DNL_1467523()
        {
            String code =
            @"
import(""FFITarget.dll"");
[Imperative]

{

    arr1 : double[] ;

    arr2 : double[] = null;

    arr3 : double[]..[];

    arr4 : double[]..[] = null;

    arr5 : double[]  = [ ];

    arr6 : double[]..[]  = [ ];

    arr11 : int[] ;

    arr12 : int[] = null;

    arr13 : int[]..[];

    arr14 : int[]..[] = null;

    arr15 : int[]  = [ ];

    arr16 : int[]..[]  = [ ];

    arr111 : bool[] ;

    arr112 : bool[] = null;

    arr113 : bool[]..[];

    arr114 : bool[]..[] = null;

    arr115 : bool[]  = [ ];

    arr116 : bool[]..[]  = [ ];

    arr1111 : ClassFunctionality[] ;

    arr1112 : ClassFunctionality[] = null;

    arr1113 : ClassFunctionality[]..[];

    arr1114 : ClassFunctionality[]..[] = null;

    arr1115 : ClassFunctionality[]  = [ ];

    arr1116 : ClassFunctionality[]..[]  = [ ];

}

dummy = 1;

            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("dummy", 1);
        }
    }


}
