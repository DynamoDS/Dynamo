using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Imperative
{
    public class MicroFeatureTests
    {
        public TestFrameWork thisTest = new TestFrameWork();
        readonly string testCasePath = Path.GetFullPath(@"..\..\..\Scripts\imperative\MicroFeatureTests\");
        [SetUp]
        public void Setup()
        {
        }

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
            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", 5);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", 6);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("aa",0);
            thisTest.Verify("bb",20);
            thisTest.Verify("a",2);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
        }

        [Test]
        public void TestNull02()
        {
            String code =
                @"[Imperative]
                {
                    a = b;
                }";
            thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.IdUnboundIdentifier);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("fib10", 55);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",1);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",1);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",1);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",2);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",1);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",2);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("temp1",11);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",28);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",0);
            thisTest.Verify("c",0);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",-2.5);
            thisTest.Verify("y",0.0);
        }

        [Test]
        public void ForLoop01()
        {
            String code =
                        @"x;
                        [Imperative]
                        {
                            a = [10,20,30,40];
                            x = 0;
                            for (val in a)
                            {
                                x = x + val;
                            }
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",100);
        }

        [Test]
        public void ForLoop02()
        {
            String code =
                        @"x;
                        [Imperative]
                        {
                            x = 0;
                            for (val in [100,200,300,400])
                            {
                                x = x + val;
                            }
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",1000);
        }

        [Test]
        public void ForLoop03()
        {
            String code =
                        @"x;
                        [Imperative]
                        {
                            x = 0;
                            for (val in [[100,101],[200,201],[300,301],[400,401]])
                            {
                                x = x + val[1];
                            }
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",1004);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("x",10);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("y",11);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",2);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",3);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",-3);
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
            thisTest.RunScriptSource(code);
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
            thisTest.RunScriptSource(code);
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
            thisTest.RunScriptSource(code);
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
            thisTest.RunScriptSource(code);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",1);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",2);
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
            thisTest.RunScriptSource(code);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("temp",3);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",0);
            thisTest.Verify("c",0);
            thisTest.Verify("d",0);
            thisTest.Verify("e",0);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",5.0);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] {1.0, 1.2, 1.4});
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",1.5);
            thisTest.Verify("c",2.6);
            thisTest.Verify("d",3.7);
            thisTest.Verify("e",4.8);

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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",15.0);
            thisTest.Verify("c",13.5);
            thisTest.Verify("d",12.0);
            thisTest.Verify("e",10.5);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",0);
            thisTest.Verify("c",3.75);
            thisTest.Verify("d",7.5);
            thisTest.Verify("e",11.25);
            thisTest.Verify("f",15);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",0);
            thisTest.Verify("c",3.75);
            thisTest.Verify("d",7.5);
            thisTest.Verify("e",11.25);
            thisTest.Verify("f",15);
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
            thisTest.RunScriptSource(code);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("i",5);
            thisTest.Verify("temp",0);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("i",3);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",-0.66666666666666663);
        }

        [Test]
        public void Factorial01()
        {
            String code =
                        @"
val;
def fac : int( n : int )
{
    return = [Imperative]
    {
        if(n == 0)                
	        return = 1;                
        else
            return = n * fac (n-1 );
    }
}
                        [Imperative]
                        {	
	                        val = fac(5);				
                        }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("val",120);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",0.3);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",10);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",20);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",30);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 6);
            thisTest.Verify("b", 6);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",6);
            thisTest.Verify("b",5);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("a",8);
            thisTest.Verify("b",6);
            thisTest.Verify("c",7);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",0);
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("c",2);
        }

        [Test]
        public void NegativeIndexOnCollection001()
        {
            String code =
                @"  b;[Imperative]
                    {
                        a = [1, 2, 3, 4];
                        b = a[-2]; // 3
                    }
                    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",3);
        }

        [Test]
        public void NegativeIndexOnCollection002()
        {
            String code =
                @"  b;[Imperative]
                    {
                        a = [ [ 1, 2 ], [ 3, 4 ] ];
                        b = a[-1][-2]; // 3
                    }
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",3);
        }

        [Test]
        public void ListMethod_Imperative()
        {
            var code =
@"
import(""DSCoreNodes.dll"");
import(""BuiltIn.ds"");
a = [Imperative]
{
counter = 0;
lst = [];
while(counter < 10)
{
lst = List.AddItemToEnd(counter, lst);
counter = counter + 1;
}
return = lst;
};
  ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 0,1,2,3,4,5,6,7,8,9 }
);
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
		                    x = [ B.B(), B.B(), B.B() ];
	                    }
                    }
                    class B
                    {
	                    x : var[]..[];
	
	                    constructor B()
	                    {
		                    x = [ [ 1, 2 ], [ 3, 4 ],  [ 5, 6 ] ];		
	                    }
                    }
b;
                    [Imperative]
                    {
                        a = [ A.A(), A.A(), A.A() ];
                        b = a[-2].x[-3].x[-2][-1]; // 4 
                    }
                ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b",4);
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
		                z = [ B.B(40, 50), B.B(60, 70), B.B(80, 90) ];
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
	                c = [ B.B(-1, -2), B.B(-3, -4) ];
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
            thisTest.RunScriptSource(code);
            thisTest.Verify("watch1", 2);
            thisTest.Verify("watch2", 3);
            thisTest.Verify("watch3", 3);
            thisTest.Verify("watch4", -3);
        }

        [Test, Category("Failure")]
        public void TestArrayOverIndexing01()
        {
            // TODO pratapa: Zero sub-indexing of array now works due to array promotion 
            // after introducing Builtin.Get.ValueAtIndex for indexing operator
            string code = @"
[Imperative]
{
    arr1 = [true, false];
    arr2 = [1, 2, 3];
    arr3 = [false, true];
    t = arr2[1][0];
}
";
            thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.OverIndexing);
        }

        [Test]
        public void TestTemporaryArrayIndexing01()
        {
            string code = @"
t;
[Imperative]
{
    t = [1,2,3,4][3];
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
    t = [[1,2], [3,4]][1][1];
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
    t = ([[1,2], [3,4]])[1][1];
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
    t = ([[1,2], [3,4]][1])[1];
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
    t = [1,2,3,4,5][1..3];
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
    loc = [];
    c = 0;
    for(i in range)
    {
        loc[c] = i + 1;
        c = c + 1;
    }
}
v = loc[0];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("v", 2);
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
            thisTest.RunScriptSource(code);
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
	r8 = ns == [1, 'b'];
	r9 = s != ""ab"";
    ss = ""abc"";
    ss[0] = 'x';
    r10 = """" == null;
}
";
            string err = "MAGN-4118 null upgrades to string when added to string";
            thisTest.RunScriptSource(code, err);
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
            thisTest.RunScriptSource(code, err);
            thisTest.Verify("r1", true);
            thisTest.Verify("r2", true);
            thisTest.Verify("r3", Convert.ToInt64('h') + 1);
        }

        [Test, Category("Failure")]
        public void TestStringForloop()
        {
            // TODO pratapa: Regression post Dictionary changes. Now returns null!
            string code = 
@"
r = [Imperative]
{
    s = ""foo"";
    for (x in ""bar"")
    {
         s  = s + x;
    }
    return = s;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", "foobar");
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
