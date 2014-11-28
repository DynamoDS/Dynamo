using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Imperative
{
    class MicroFeatureTests : ProtoTestBase
    {
        readonly string testCasePath = Path.GetFullPath(@"..\..\..\Scripts\imperative\MicroFeatureTests\");

        [Test]
        public void TestAssignment01()
        {
            String code =
@"
foo;
[Imperative]
{
	foo = 5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object o = mirror.GetValue("foo").Payload;
            Assert.IsTrue((Int64)o == 5);
        }

        [Test]
        public void TestAssignment02()
        {
            String code =
@"
foo;
[Imperative]
{
	foo = 5;
    foo = 6;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object o = mirror.GetValue("foo").Payload;
            Assert.IsTrue((Int64)o == 6);
        }

        [Test]
        public void TestNull01()
        {
            String code =
                @"aa;bb;a;b;c;[Imperative]
                {
                    i = 0;
                    aa = 0;
                    bb = 0;
                    if (i == null)
                    {
                        aa = i + 10;
                    }
        
                    j = 0;
                    if (j != null)
                    {
                        bb = i + 20;
                    }
                    a = 2;
                    b = null + 2;
                    c = b * 3; 
                }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("aa").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("bb").Payload == 20);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);
        }

        [Test]
        public void TestNull02()
        {
            String code =
                @"[Imperative]
                {
                    a = b;
                }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
        }

        [Test]
        public void TestNullInForLoop01()
        {
            String code =
                @"a = [Imperative]
                {
                    def foo(i : var[]..[])
                    {
                        j = 10;
                        for(x in i)
                        {
                            j = 11;
                        }
                        return = j;
                    }
                    return = foo(null);
                }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 10);
        }


        public void Fibonacci_recusion()
        {
            Setup();
            String code =
                        @"fib10;[Imperative]
                            {
	                            def fibonacci : int( number : int)
	                            {
		                            if( number < 2)
		                            {
		                                return = 1;
		                            }
		                            return = fibonacci(number-1) + fibonacci(number -2);
		
	                            }
	                            fib10 = fibonacci(10);
                            }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            var fib10 = (Int64)mirror.GetValue("fib10").Payload;
            Assert.IsTrue(fib10 == 55);
        }

        [Test]
        public void TestFunction01()
        {
            String code =
@"
a;
[Imperative] 
{
	// An imperative function
	// Clamps 'i' between min and max ranges
	def clampRange : int(i : int, rangeMin : int, rangeMax : int)
	{
		clampedValue = i;
		if(i < rangeMin) 
		{
			clampedValue = rangeMin;
		}
		elseif( i > rangeMax ) 
		{
			clampedValue = rangeMax; 
		} 
		return = clampedValue;
	}
	a = clampRange(99, 10, 100);
}"
;
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 99);
        }

        [Test]
        public void TestFunction02()
        {
            string code =
                    @"test;[Imperative]
                    {
	                    def add:double( n1:int, n2:double )
	                    {
		                    return = n1 + n2;
	                    }
	                    test = add (3+1, 4.5 ) ;
                    }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("test").Payload == 8.5);
        }

        [Test]
        public void TestFunction03()
        {
            string code =
                    @"x;temp2;[Imperative]
                        {
	                        def fn2:int(a:int)
	                        {   
		                        if( a < 0 )
		                        {
			                        return = 0;
		                        }	
		                        return = 1;
	                        }
	                        x = fn2(4);
	                        temp2 = 56;
	                        if( fn2(4) == 1 )
	                        {
		                        temp2 = fn2 ( 5 );
	                        }
                        }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("temp2").Payload == 1);
        }

        [Test]
        public void IfStatement01()
        {
            String code =
                        @"
                        b;
                        [Imperative]
                        {
	                        a = 5;
	                        b = 0;
	                        if (a < 0)	
		                        b=2;
	                        b=1;
                        }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
        }

        [Test]
        public void IfStatement02()
        {
            String code =
                        @"
                        b;
                        [Imperative]
                        {
	                        a = 5;
	                        b = 0;
	                        if (a < 0)	
		                        b=2;
                            else
	                            b=1;
                        }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
        }

        [Test]
        public void IfStatement03()
        {
            String code =
                        @"
                        b;
                        [Imperative]
                        {
	                        a = 5;
	                        b = 0;
	                        if (a < 0)	
		                        b=2;
                            else if (a > 0)
	                            b=1;
                        }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
        }

        [Test]
        public void IfStatement04()
        {
            String code =
                        @"
b;
                        [Imperative]
                        {
	                        a = 2;
                            b = 0;
	                        if( a < 0 )
		                        b = 0;
	                        elseif ( a == 2 )
		                        b = 2;
	                        else
		                        b = 1;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        public void IfStatement05()
        {
            String code =
                        @"
b;
                        [Imperative]
                        {
	                        a = true;
                            b = 0;
	                        if(a)
		                        b = 1;	                        
	                        else
		                        b = 2;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
        }

        [Test]
        public void IfStatement06()
        {
            String code =
                        @"
b;
[Imperative]
                        {
	                        a = 1;
                            b = 0;
	                        if(a!=1); 
	                        else 
		                        b = 2; 
                        }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        public void IfStatement07()
        {
            String code =
                        @"
temp1;
[Imperative]
                        {
	                        a1=7.5;
	                        temp1=10;
	                        if(a1==7.5)	
		                        temp1=temp1+1;
	                        else
		                        temp1=temp1+2;
                        }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("temp1").Payload == 11);
        }

        [Test]
        public void IfStatement08()
        {
            String code =
                        @"
b;
[Imperative]
                          {
	                        a = 0;
	                        b = 0;
	                        if (a <= 0)	
                            {
		                        b = 2;
        
                                if (b == 0)
                                {
	                                b = 27;
                                }
                                else
                                    b = 28;
                            }
                            else 
                            {
                                if (a > 0)
                                {
	                                b = 1;
                                }
                            }
    
                        }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 28);
        }

        [Test]
        public void IfStatement09()
        {
            String code =
                    @"a;b;[Imperative]
                    {
	                    a = 4;	
	
	                    if( a == 4 )
	                    {
		                    i = 0;
                        }
                        // The unbounded warning only occurs here
	                    a = i;
                        // At this point 'i' is already allocated and assigned null
	                    b = i; 
                    }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
        }

        [Test]
        public void IfStatement10()
        {
            String code =
                    @"
a;
c;
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 0);
        }

        [Test]
        public void NestedBlocks001()
        {
            String code =
                        @"a;[Imperative]
                        {
                            a = 4;
                            b = a*2;
                
                            [Associative]
                            {
                                i=0;
                                temp=1;
                            }
                            a = temp;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
        }

        [Test]
        public void NegativeFloat001()
        {
            String code =
                        @"x;y;[Imperative]
                            {
	                            x = -2.5;
	                            y = -0.0;
                            }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("x").Payload == -2.5);
            Assert.IsTrue((Double)mirror.GetValue("y").Payload == 0.0);
        }

        [Test]
        public void ForLoop01()
        {
            String code =
                        @"x;
                        [Imperative]
                        {
                            a = {10,20,30,40};
                            x = 0;
                            for (val in a)
                            {
                                x = x + val;
                            }
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 100);
        }

        [Test]
        public void ForLoop02()
        {
            String code =
                        @"x;
                        [Imperative]
                        {
                            x = 0;
                            for (val in {100,200,300,400})
                            {
                                x = x + val;
                            }
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1000);
        }

        [Test]
        public void ForLoop03()
        {
            String code =
                        @"x;
                        [Imperative]
                        {
                            x = 0;
                            for (val in {{100,101},{200,201},{300,301},{400,401}})
                            {
                                x = x + val[1];
                            }
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1004);
        }

        [Test]
        public void ForLoop04()
        {
            String code =
                        @"x;
                        [Imperative]
                        {
                            x = 0;
                            for (val in 10)
                            {
                                x = x + val;
                            }
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 10);
        }

        [Test]
        public void ForLoop05()
        {
            String code =
                        @"y;
                        [Imperative]
                        {
                            y = 0;
                            b = 11;
                            for (val in b)
                            {
                                y = y + val;
                            }
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 11);
        }
        [Ignore]
        public void BitwiseOp001()
        {
            String code =
                        @"c;
                        [Imperative]
                        {
	                        a = 2;
	                        b = 3;
                            c = a & b;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
        }
        [Ignore]
        public void BitwiseOp002()
        {
            String code =
                        @"c;
                        [Imperative]
                        {
	                        a = 2;
	                        b = 3;
                            c = a | b;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
        }
        [Ignore]
        public void BitwiseOp003()
        {
            String code =
                        @"b;
                        [Imperative]
                        {
	                        a = 2;
	                        b = ~a;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -3);
        }
        [Ignore]
        public void BitwiseOp004()
        {
            String code =
                        @"c;
                        [Imperative]
                        {
	                        a = true;
                            b = false;
	                        c = a^b;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", null);
        }

        [Test]
        public void LogicalOp001()
        {
            String code =
                        @"e;
                        [Imperative]
                        {
	                        a = true;
	                        b = false;
                            c = 1;
                            d = a && b;
                            e = c && d;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("e", false);
        }

        [Test]
        public void LogicalOp002()
        {
            String code =
                        @"e;
                        [Imperative]
                        {
	                        a = true;
	                        b = false;
                            c = 1;
                            d = a || b;
                            e = c || d;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("e", true);
        }

        [Test]
        public void LogicalOp003()
        {
            String code =
                        @"c;
                        [Imperative]
                        {
	                        a = true;
	                        b = false;
                            c = 0;
                            
                            if ( a && b )
                                c = 1;
                            else
                                c = 2;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 2);
        }

        [Test]
        public void LogicalOp004()
        {
            String code =
                        @"c;
                        [Imperative]
                        {
	                        a = true;
	                        b = false;
                            c = 0;
                            
                            if ( a || b )
                                c = 1;
                            else
                                c = 2;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 1);
        }

        [Test]
        public void LogicalOp005()
        {
            String code =
                        @"c;
                        [Imperative]
                        {
	                        a = true;
                            c = 0;
                            
                            if ( !a )
                                c = 1;
                            else
                                c = 2;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
        }

        [Test]
        public void LogicalOp006()
        {
            String code =
                        @"c;
                        [Imperative]
                        {
	                        a = true;
	                        b = false;
                            c = !(a || !b);
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", false);
        }

        [Test]
        public void LogicalOp007()
        {
            String code =
                        @"temp;
                        [Imperative]
                        {
	                        i1 = 2.5;
	                        i2 = 3;
	                        temp = 2;
	                        if((i2==3)&&(i1==2.5))
		                        temp=temp+1;	  
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 3);
        }

        [Test]
        public void LogicalOp008()
        {
            String code =
                        @"b;c;d;e;
                        [Imperative]
                        {
	                        a = null;
	                        b = 0;
	                        c = 0;
	                        d = 0;
	                        e = 0;
	
	                        if ( a == true)
		                        b = 1;
	                        if (a != false)
		                        c = 2;	
	                        if (null)
		                        d = 3;
	                        if (!null)
		                        e = 4;	
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 0);
        }

        [Test]
        public void DoubleOp()
        {
            String code =
                        @"b;
                        [Imperative]
                        {
	                        a = 1 + 2;
                            b = 2.0;
                            b = a + b; 
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 5.0);
        }

        [Test]
        public void RangeExpr001()
        {
            String code =
                        @"a;
                        [Imperative]
                        {
	                        a = 1..1.5..0.2;                           
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            List<Object> result = new List<Object> { 1.0, 1.2, 1.4 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
        }

        [Test]
        public void RangeExpr002()
        {
            String code =
                        @"b;c;d;e;
                        [Imperative]
                        {
	                        a = 1.5..5..1.1;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3];                             
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 1.5);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == 2.6);
            Assert.IsTrue((Double)mirror.GetValue("d").Payload == 3.7);
            Assert.IsTrue((Double)mirror.GetValue("e").Payload == 4.8);

        }

        [Test]
        public void RangeExpr003()
        {
            String code =
                        @"b;c;d;e;
                        [Imperative]
                        {
	                        a = 15..10..-1.5;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3];                             
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 15.0);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == 13.5);
            Assert.IsTrue((Double)mirror.GetValue("d").Payload == 12.0);
            Assert.IsTrue((Double)mirror.GetValue("e").Payload == 10.5);
        }

        [Test]
        public void RangeExpr004()
        {
            String code =
                        @"b;c;d;e;f;
                        [Imperative]
                        {
	                        a = 0..15..#5;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3]; 
                            f = a[4];                            
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 0);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == 3.75);
            Assert.IsTrue((Double)mirror.GetValue("d").Payload == 7.5);
            Assert.IsTrue((Double)mirror.GetValue("e").Payload == 11.25);
            Assert.IsTrue((Double)mirror.GetValue("f").Payload == 15);
        }

        [Test]
        public void RangeExpr005()
        {
            String code =
                        @"b;c;d;e;f;
                        [Imperative]
                        {
	                        a = 0..15..~4;
                            b = a[0];
	                        c = a[1];
	                        d = a[2];
	                        e = a[3];  
                            f = a[4];                           
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 0);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == 3.75);
            Assert.IsTrue((Double)mirror.GetValue("d").Payload == 7.5);
            Assert.IsTrue((Double)mirror.GetValue("e").Payload == 11.25);
            Assert.IsTrue((Double)mirror.GetValue("f").Payload == 15);
        }

        [Test]
        public void RangeExpr06()
        {
            string code = @"
x1; x2; x3; x4;
[Imperative]
{
    x1 = 0..#(-1)..5;
    x2 = 0..#0..5;
    x3 = 0..#1..10;
    x4 = 0..#5..10;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", null);
            thisTest.Verify("x2", new object[] {});
            thisTest.Verify("x3", new object[] {0});
            thisTest.Verify("x4", new object[] {0, 10, 20, 30, 40});
        }

        [Test]
        public void WhileStatement01()
        {
            String code =
                        @"i;temp;
                         [Imperative]
                        {
                            i = 1;
                            a = 3;
                            temp = 0;
                            if( a == 3 )             
                            {
                                while( i <= 4)
                                {
	                                if( i > 10 )
                                    { 
		                                temp = 4;
                                    }			  
	                                else 
                                    {
		                                i = i + 1;
                                    }
                                }
                            }
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 0);
        }

        [Test]
        public void WhileStatement02()
        {
            String code =
                        @"i;
                        [Imperative]
                        {
                            i = 1;
                            temp = 0;
                            while( i <= 2 )
                            {
                                a = 1;                     
                                i = i + 1;
                            }
                        }  
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 3);
        }

        [Test]
        public void RecurringDecimal01()
        {
            String code =
                        @"
                        c;
                        [Imperative]
                        {
                         a = 3.5;
                         b = -5.25;
                         c = a/b;
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == -0.66666666666666663);
        }

        [Test]
        public void Factorial01()
        {
            String code =
                        @"
val;
                        [Imperative]
                        {	
	                        def fac : int( n : int )
	                        {
       	                        if(n == 0)                
	                                return = 1;                
                                return = n * fac (n-1 );
	                        }    
	                        val = fac(5);				
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 120);
        }

        [Test]
        public void ToleranceTest()
        {
            String code =
                        @"a;
                        [Imperative]
                        {	
	                        a = 0.3; b = 0.1;  
	                        if (a-b < 0.2) { a = 0; }			
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("a").Payload == 0.3);
        }

        [Test]
        public void InlineCondition001()
        {
            String code =
                        @"c;
                        [Imperative]
                        {	
	                        a = 10;
                            b = 20;
                            c = a < b ? a : b;			
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 10);
        }

        [Test]
        public void InlineCondition002()
        {
            String code =
                        @"c;
                        [Imperative]
                        {	
	                        a = 10;
			                b = 20;
                            c = a > b ? a : a == b ? 0 : b; 
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 20);
        }

        [Test]
        public void InlineCondition003()
        {
            String code =
                        @"c;
                        [Imperative]
                        {	
	                        a = 10;
                            b = 20;
                            c = (a > b ? a : b) > 15 ? a + (a > b ? a : b) : b + (a > b ? a : b); 
                        }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 30);
        }
        [Ignore]
        public void PrePostFix001()
        {
            String code =
                        @"
                            [Imperative]
                            {
	                            a = 5;
                                b = ++a;
                            }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 6);
        }
        [Ignore]
        public void PrePostFix002()
        {
            String code =
                        @"
                            [Imperative]
                            {
	                            a = 5;
                                b = a++;
                            }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 5);
        }
        [Ignore]
        public void PrePostFix003()
        {
            String code =
                        @"
                            [Imperative]
                            {
	                            a = 5;			//a=5;
                                b = ++a;		//b =6; a =6;
                                a++;			//a=7;
                                c = a++;		//c = 7; a = 8;
                            }
                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 7);
        }

        [Test]
        public void Modulo001()
        {
            String code =
                @"  c;
                    [Imperative]
                    {
                        a = 10 % 4; // 2
                        b = 5 % a; // 1
                        c = b + 11 % a * 3 - 4; // 0
                    }                
                    ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 0);
        }

        [Test]
        public void Modulo002()
        {
            String code =
               @"   c;
                    [Imperative]
                    {
                        a = 10 % 4; // 2
                        b = 5 % a; // 1
                        c = 11 % a == 2 ? 11 % 2 : 11 % 3; // 2
                    }
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
        }

        [Test]
        public void NegativeIndexOnCollection001()
        {
            String code =
                @"  b;[Imperative]
                    {
                        a = {1, 2, 3, 4};
                        b = a[-2]; // 3
                    }
                    ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 3);
        }

        [Test]
        public void NegativeIndexOnCollection002()
        {
            String code =
                @"  b;[Imperative]
                    {
                        a = { { 1, 2 }, { 3, 4 } };
                        b = a[-1][-2]; // 3
                    }
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 3);
        }

        [Test]
        public void NegativeIndexOnCollection003()
        {
            String code =
                @"
                    class A
                    {
	                    x : var[];
	
	                    constructor A()
	                    {
		                    x = { B.B(), B.B(), B.B() };
	                    }
                    }
                    class B
                    {
	                    x : var[]..[];
	
	                    constructor B()
	                    {
		                    x = { { 1, 2 }, { 3, 4 },  { 5, 6 } };		
	                    }
                    }
b;
                    [Imperative]
                    {
                        a = { A.A(), A.A(), A.A() };
                        b = a[-2].x[-3].x[-2][-1]; // 4 
                    }
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
        }

        [Test]
        public void PopListWithDimension()
        {
            String code =
                @"
                class A
                {
	                x : var;
	                y : var;
	                z : var[];
	
	                constructor A()
	                {
		                x = B.B(20, 30);
		                y = 10;
		                z = { B.B(40, 50), B.B(60, 70), B.B(80, 90) };
	                }
                }
                class B
                {
	                m : var;
	                n : var;
	
	                constructor B(_m : int, _n : int)
	                {
		                m = _m;
		                n = _n;
	                }
                }
watch1;
watch2;
watch3;
watch4;
                [Imperative]
                {
	                a = A.A();
	                b = B.B(1, 2);
	                c = { B.B(-1, -2), B.B(-3, -4) };
	                a.z[-2] = b;
	                watch1 = a.z[-2].n; // 2
	                a.z[-2].m = 3;
	                watch2 = a.z[-2].m; // 3
	                a.x = b;
	                watch3 = a.x.m; // 3
	                a.z = c;
	                watch4 = a.z[-1].m; // -3
                }
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("watch1", 2);
            thisTest.Verify("watch2", 3);
            thisTest.Verify("watch3", 3);
            thisTest.Verify("watch4", -3);
        }

        [Test]
        public void TestArrayOverIndexing01()
        {
            string code = @"
[Imperative]
{
    arr1 = {true, false};
    arr2 = {1, 2, 3};
    arr3 = {false, true};
    t = arr2[1][0];
}
";
            thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kOverIndexing);
        }

        [Test]
        public void TestTemporaryArrayIndexing01()
        {
            string code = @"
t;
[Imperative]
{
    t = {1,2,3,4}[3];
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing02()
        {
            string code = @"
t;
[Imperative]
{
    t = {{1,2}, {3,4}}[1][1];
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing03()
        {
            string code = @"
t;
[Imperative]
{
    t = ({{1,2}, {3,4}})[1][1];
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing04()
        {
            string code = @"
t;
[Imperative]
{
    t = ({{1,2}, {3,4}}[1])[1];
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing05()
        {
            string code = @"
t;
[Imperative]
{
    t = {1,2,3,4,5}[1..3];
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 2, 3, 4 });
        }

        [Test]
        public void TestTemporaryArrayIndexing06()
        {
            string code = @"
t;
[Imperative]
{
    t = (1..5)[1..3];
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 2, 3, 4 });
        }

        [Test]
        public void TestDynamicArray001()
        {
            String code =
@"
loc;
[Imperative]
{
    range = 1..10;
    loc = {};
    c = 0;
    for(i in range)
    {
        loc[c] = i + 1;
        c = c + 1;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            ProtoCore.Lang.Obj o = mirror.GetFirstValue("loc");
            ProtoCore.DSASM.Mirror.DsasmArray arr = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue((Int64)arr.members[0].Payload == 2);
        }
        [Test, Ignore]
        public void TestTryCatch001()
        {
            String code =
@"
x;t2;y2;y3;t3;z;
[Imperative]
{
    x = 1;
    // t2,y2,y3 shouldn't be changed!
    t2 = 1;
    y2 = 1;
    y3 = 1;
    try
    {
        y1 = 1;
        try
        {
            t1 = 100;
        }
        catch (e:var)
        {
            t2 = 200;
        }
        t3 = 300;
    }
    catch (e :int)
    {
        y2 = 2;
    }
    catch (e:boolean)
    {
        y3 = 3;
    }
    z = 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("t2").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y2").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y3").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("t3").Payload == 300);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 2);
        }
        [Test, Ignore]
        public void TestTryCatch002()
        {
            string code = @"
class MyException
{
    ex:int;
    constructor Create()
    {
        ex = 100;
    }
}
x;y1;y2;y3;y4;y5;y6;z;
[Imperative]
{
   x = 1;
   y1 = 0;
   y2 = 0;
   y3 = 0;
   y4 = 0;
   y5 = 0;
   y6 = 0;
   
   try
   {
       y1 = 1;
       throw 1 + 2;
       y2 = 2;
   }
   catch (e:boolean)
   {
       y3 = 3;
   }
   catch (e:int)
   {
       y4 = 4;
   }
   try
   {
       y5 = 5;
       throw MyException.Create();
       y6 = 6;
   }
   catch (e:MyException)
   {
   }
   z = 5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y1").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y2").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y3").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y4").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("y5").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("y6").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 5);
        }
        [Test, Ignore] // Jun: Ignore for now until we allow deeply nested function definitions
        public void TestTryCatchStackUnwinding01()
        {
            string code = @"    y0 = 0;
    y1 = 0;
    y2 = 0;
    y3 = 0;
    y4 = 0;
    y5 = 0;
    y6 = 0;
    y7 = 0;
    y8 = 0;
    y9 = 0;
    y10 = 0;
    y11 = 0;
    y12 = 0;
    y13 = 0;
[Imperative]
{
    y1 = 1;
    try
    {
        y2 = 2;
        [Associative]
        {
            [Imperative]
            {
                y3 = 3;
                def foo()
                {
                    try
                    {
                        y4 = 4;
                        throw 3;
                        y5 = 5;
                    }
                    catch (e:bool)
                    {
                        y6 = 6;
                    }
                    y7 = 7;
                }
                y8 = 8;
                try
                {
                    r = foo();
                    y9 = 9;
                }
                catch (e:char)
                {
                    y10 = 10;
                }
                y11 = 11;
            }
        }
    }
    catch (e:double)
    {
        y12 = 12;
    }
    catch (e:int)
    {
        y13 = 13;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("y1").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y2").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("y3").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("y4").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("y5").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y6").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y7").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y8").Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("y9").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y10").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y11").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y12").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("y13").Payload == 13);
        }
        [Test, Ignore]
        public void TestTryCatchGetExceptionValue01()
        {
            string code = @"y = 0;
[Imperative]
{
    try
    {
        throw 1+2;
    }
    catch (e:int)
    {
        y = e;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 3);
        }
        [Test, Ignore]
        public void TestTryCatchGetExceptionValue02()
        {
            string code = @"y = 0;
[Imperative]
{
    try
    {
        [Associative]
        {
            [Imperative]
            {
                throw 1+2;
            }
        }
    }
    catch (e:int)
    {
        y = e;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 3);
        }
        [Test, Ignore]
        public void TestTryCatchGetExceptionValue03()
        {
            string code = @"y = 0;
[Imperative]
{
    def foo()
    {
        [Associative]
        {
            [Imperative]
            {
                throw 3;
            }
        }
    }
    try
    {
        foo();
    }
    catch (e:int)
    {
        y = e;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 3);
        }

        [Test]
        public void TestAttributeOnGlobalFunction()
        {
            string code = @"class TestAttribute
{
	constructor TestAttribute()
	{}
}
class VisibilityAttribute
{
	x : var;
	constructor VisibilityAttribute(_x : var)
	{
		x = _x;
	}
}
[Imperative]
{
	[Test, Visibility(1)]
	def foo : int()
	{
		return = 10;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestAttributeOnLanguageBlock()
        {
            string code = @"class TestAttribute
{
	constructor TestAttribute()
	{}
}
class VisibilityAttribute
{
	x : var;
	constructor VisibilityAttribute(_x : var)
	{
		x = _x;
	}
}
[Imperative]
{
	[Associative, version=""###"", Visibility(10 + 1), fingerprint=""FS54"", Test] 
	{
		a = 19;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestAttributeWithLanguageBlockAndArrayExpression()
        {
            string code = @"class TestAttribute
{
	constructor TestAttribute()
	{}
}
class VisibilityAttribute
{
	x : var;
	constructor VisibilityAttribute(_x : var)
	{
		x = _x;
	}
}
[Imperative]
{
	def foo : int[]..[](p : var[]..[])
	{
		a = { 1, { 2, 3 }, 4 };
		return = a[1];
	}
	[Associative, version=""###"", Visibility(10 + 1), fingerprint=""FS54"", Test] 
	{
		a = {1, 2, 3};
		b = a[1];
		c = a[0];
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestStringConcatenation01()
        {
            string code = @"s3;s6;s9;
[Imperative]
{
	s1='a';
	s2=""bcd"";
	s3=s1+s2;
	s4=""abc"";
	s5='d';
	s6=s4+s5;
	s7=""ab"";
	s8=""cd"";
	s9=s7+s8;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("s3", "abcd");
            thisTest.Verify("s6", "abcd");
            thisTest.Verify("s9", "abcd");
        }

        [Test]
        [Category("Failure")]
        public void TestStringOperations()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4118
            string code = @"[Imperative]
{
	s = ""ab"";
	r1 = s + 3;
	r2 = s + false;
	r3 = s + null;
	r4 = !s;
	r5 = s == ""ab"";
	r6 = s == s;
	r7 = ""ab"" == ""ab"";
	ns = s;
	ns[0] = 1;
	r8 = ns == {1, 'b'};
	r9 = s != ""ab"";
    ss = ""abc"";
    ss[0] = 'x';
    r10 = """" == null;
}
";
            string err = "MAGN-4118 null upgrades to string when added to string";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("r1", "ab3");
            thisTest.Verify("r2", "abfalse");
            thisTest.Verify("r3", null);
            thisTest.Verify("r4", false);
            thisTest.Verify("r5", true);
            thisTest.Verify("r6", true);
            thisTest.Verify("r7", true);
            thisTest.Verify("r8", false);
            thisTest.Verify("r9", false);
            thisTest.Verify("ss", "xbc");
            thisTest.Verify("r10", null);
        }

        [Test]
        [Category("Failure")]
        public void TestStringTypeConversion()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4119
            string code = @"[Imperative]
{
	def foo:bool(x:bool)
	{
	    return=x;
	}
	r1 = foo('h');
	r2 = 'h' && true;
	r3 = 'h' + 1;
}";
            string err = "MAGN-4119 Char does not upgrade to 'int' in arithmetic expression";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("r1", true);
            thisTest.Verify("r2", true);
            thisTest.Verify("r3", Convert.ToInt64('h') + 1);
        }

        [Test]
        public void TestLocalKeyword01()
        {
            string code =
@"
i = [Imperative]
{
    a : local int = 1;      
    return = a;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 1);
        }

        [Test]
        public void TestLocalKeyword02()
        {
            string code =
@"
i = [Imperative]
{
    a : local int = 1;      
    b : local int = 2;       
    return = a + b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 3);
        }

        [Test]
        public void TestLocalKeyword03()
        {
            string code =
@"
a = 1;
b = [Imperative]
{
    a : local = 2;
    x : local = a;
    return = x;
}

c = a;
d = b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void TestLocalKeyword04()
        {
            string code =
@"
a = 1;
b = [Imperative]
{
    a : local = 2;
    return = a;
}

c = a;
d = b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("d", 2);
        }


        [Test]
        public void TestLocalKeyword05()
        {
            string code =
@"
a = [Imperative]
{
    a : local = 1;
    b = 0;
    if (a == 1)
    {
        a : local = 2;
        b = a;
    }
    else
    {
        a : local = 3;
        b = a;
    }
    return = b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
        }

        [Test]
        public void TestLocalKeyword06()
        {
            string code =
@"
a = [Imperative]
{
    a : local = 1;
    b = 0;
    if (a != 1)
    {
        a : local = 2;
        b = a;
    }
    else
    {
        a : local = 3;
        b = a;
    }
    return = b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

                [Test]
        public void TestLocalVariableNoUpdate01()
        {
            string code =
@"
a = 1;
b = a;
c = [Imperative]
{
    a : local = 2; // Updating local 'a' should not update global 'a'
    return = 1;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 1);
        }

        [Test]
        public void TestLocalVariableNoUpdate02()
        {
            string code =
@"
a : local = 1;      // Tagging a variable local at the global scope has no semantic effect
b : local = a;
c = [Imperative]
{
    a : local = 2; // Updating local 'a' should not update global 'a'
    return = a;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 2);
        }
    }
}
