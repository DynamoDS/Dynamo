using System.Collections.Generic;
using NUnit.Framework;
using ProtoTestFx;
namespace ProtoTest.DebugTests
{
    [TestFixture, Category("WatchFx Tests")]
    public class RunWatchTests
    {

        string importpath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch0_array()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Associative]
{
/*
	a = {1001,1002};
	a[0] = 1234;
	a[1] = 5678;
	x = a[1];
	y = a[0];
	
	
	b = {101, 102, {103, 104}, 105};
	b[2][1] = 100001;
	
	c = {
			101,    
			102, 
			{103, 104}, 
			{{1001, 2002}, 1},
			5
		};
	c[2][1]		= 111111;
	c[3][0][1]	= 222222;
	c[3][0][0]	= 333333;
	
	d = {
			{1, 0, 0, 0}, 
			{0, 1, 0, 0}, 
			{0, 0, 1, 0},
			{0, 0, 0, 1}
		};
	d[0][0] = c[2][1];
	d[1][1] = 2;
	d[2][2] = 2;
	d[3][3] = x;
	*/
	e = [10,[20,30]];
	e[1][1] = 40;
	dd = e[0];
	dd = e[1][0];
	dd = e[1][1];
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);

            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1_arrayargs()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Associative]
{
	//def inc : int( s : int )	
	//{
	//	return = s + 1;
	//}
	def scale2 : int( s : int )	
	{
		i = 2;
		return = s * i;
	}
	a = scale2(20);
	//b = scale2(20) + inc(2);
	//c = scale2(20) + inc(inc(2));
	//d = scale2(20) + inc(inc(inc(inc(inc(inc(inc(inc(inc(inc(inc(inc(2))))))))))));
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch2_blockassign_associative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Associative]
{
    def DoSomthing : int(p : int)
    {
        ret = p;       
        d = [Imperative]
        {
            loc = 20;
            return = loc;
        }
        return = ret * 100 + d;
    }
    a = DoSomthing(10);   
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch3_blockassign_imperative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{ 
    d = [Associative]
    {
        aa = 20;
        return = aa;
    }
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch16_demo()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{
    a = 10;
    b = 20;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch18_forloop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Imperative]
{
    a = [10,20,30,40];
    x = 0;
    for (val in a)
    {
        x = x + val;
    }
    x = 0;
    for (val in [100,200,300,400])
    {
        x = x + val;
    }
    x = 0;
    for (val in [[100,101],[200,201],[300,301],[400,401]])
    {
        x = x + val[1];
    }
    x = 0;
    for (val in 10)
    {
        x = x + val;
    }
    
    y = 0;
    b = 11;
    for (val in b)
    {
        y = y + val;
    }
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch19_functionoverload()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Associative]
{
    def f : int( p1 : int )
    {
	    x = p1 * 10;
	    return = x;
    }
    def f : int( p1 : int, p2 : int )
    {
	    return = p1 + p2;
    }
    a = 2;
    b = 20;
    i = f(a + 10);
    j = f(a, b);
}   ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch20_fusionarray()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{
	x = 0;
	y = 0;
	xSize = 2;
	ySize = 3;
	result = 0;
    
	somelist = [11,102,1003,1004];
	somelist2 = [x, y, xSize * 4, 1004 * ySize];
	// Populate a multi-dimensional array
	list2d = [[10,20,30],[40,50,60]];
	// do somthing with those values
	while( x < xSize )
	{
		while( y < ySize )
		{
			result = result + list2d[x][y];
			y = y + 1;
		}
		x = x + 1;
		y = 0;
	}
	result = result * 10;
    
	// Populate an array of ints
	list = [10, 20, 30, 40, 50];
    
	// Declare counters and result storage
	n = 0;
	size = 5;
	result = 0;
    
    
	// Summation of elements in 'list' and storing them in 'result'
	while( n < size )
	{
		result = result + list[n];
		n = n + 1;
	}
	// Get the average
	result = result / size;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch21_header1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"// import other module
import (""./include/header2.ds"");
x = 100;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch22_importtest()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""./header1.ds"");
import (""./include/header2.ds"");
a = 1;
b = 2;
[Associative]
{
    c = 3;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch25_libmath()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Associative]
{
	def add : int(p1 : int, p2 : int)
	{
		return = p1 + p2;
	}	
    
    def sub : int(p1 : int, p2 : int)
	{
		return = p1 - p2;
	}    
    
    def mul : int(p1 : int, p2 : int)
	{
		return = p1 * p2;
	}
    def div : int(p1 : int, p2 : int)
	{
		return = p1 / p2;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch26_nesting()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Imperative]
{
	a = 10;
	if(a >= 10)
	{
		x = a * 2;
		[Associative]
		{
			loc = x * a;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch27_null()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Associative]
{
	x = 1;
    a1 = null;
    b1 = a1 + 2;
    c1 = 2 + a1 * x;
    [Imperative]
    {
        a = 2;
        b = null + 2;
        c = b * 3; 
    }
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch28_replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	def sum : int(p1 : int, p2 : int)
	{
		return = p1 + p2;
	}
	//a = {1,2,3};
	//b = {4,5,6};
	//c = sum(a<1>, b<2>);
    c = sum(5, 2);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch29_simple()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Imperative]
{
    b = 2 + 10.12345;
	c = 2 + 10;
	d = 2.12 + 10.12345 * 2;
    
	e = 2.000001 == 2;
	f = 2 == 2.000001;
	g = 2.000001 == 2.000001;
	h = 2.000001 != 2;
	i = 2 != 2.000001;
	j = 2.000001 != 2.000001;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch30_simple2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Imperative]
{
	a = 2.12 + 100;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch32_update()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
/*
a = 1;
b = a;
a = 10;
*/
/*
a = 1;
a = a + 1;
a = 10;
*/
/*
a = 1;
a = 10 + 20 * a;
a = 10;
*/
/*
a = 2;
x = 20;
y = 30;
a = x + y * a;
*/
/*
a = 2;
x = 20;
y = 30;
a = x * y + a;
*/
/*
def f : int(p : int)
{
    return = p + 1;
}
a = 10;
b = f(a);
*/
/*
def doo : int()
{
    d = 12;
    return = d;
}
def f : int(p : int)
{
    a = 10;
    b = a;
    a = p;
    return = b;
}
x = 20;
y = f(x);
x = 40;
*/
/*
a = 10;
b = 20;
c = a < b ? a : b;
*/
/*
a = 5;
b = ++a;
*/";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch54_TestBasicArrayMethods()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [ 1, 2, [ 3, 4, 5, [ 6, 7, [ 8, 9, [ [ 11 ] ] ] ] ], [ 12, 13 ] ];
c = Count(a);
r = Rank(a);
a2 = Flatten(a);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch55_TestStringConcatenation01()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"s1='a';
s2=""bcd"";
s3=s1+s2;
s4=""abc"";
s5='d';
s6=s4+s5;
s7=""ab"";
s8=""cd"";
s9=s7+s8;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch57_TestStringTypeConversion()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:bool(x:bool)
{
    return=x;
}
r1 = foo('h');
r2 = 'h' && true;
r3 = 'h' + 1;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch58_import001()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def add : int(i : int, j : int)
{
	return = i + j;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch59_import002()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def mul : int(i : int, j : int)
{
	return = i * j;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch60_ImportTest001()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import(""import001.ds"");
import(""import002.ds"");
a = 10;
b = 20;
c = add(a, b);
d = mul(a, b);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch70_TestHostentityType()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	def factorial_local : hostentityid()
    {
        return = 11;
    }	
	x = factorial_local();
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch75_multilang()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
[Associative]
{
	a = 100;
	b = 200;
	[Imperative] 
	{
		n = 300;
		n = n + 2;
	} 
	c = b + b;
}
[Imperative]
{
	n = 32;
	b = 64;
	if( n < b )
	{
		n = 1; 		
	}
	n = n + b;
} 
[Associative]
{
	a = 80;
	b = 160;
	
	[Imperative] 
	{
		n = 320;
		z = 640;
		if( n < z )
		{
			n = 1; 		
		}
		[Associative]
		{
			x = 10;
			y = 20;
			z = x + y * 2;
		}
		n = 20000;
	} 
	c = b + 2;
} 
[Associative]
{
	a = 80;
	b = 160;
	
	[Imperative] 
	{
		n = 320;
		z = 640;
		if( n < z )
		{
			n = 1; 		
		}
		[Associative]
		{
			xx = 1010;
			yy = xx + 2;
			[Imperative] 
			{
				n = 3200;
				z = 6400;
				if( n < z )
				{
					n = 1000000; 		
				}
				[Associative]
				{
					x = 1111;
					y = 2222;
					z = x + y * 2;
				}
				n = 12345;
			} 
		}
		n = n + 1;
	} 
	c = b + 2;
} 

[Associative]
{
	n : int;
    x = v;
} 
[Imperative]
{
	n : int;
    x = v;
} 


// TODO Jun: Debug this - double update issue
[Associative]
{
	a = 80;
	b = 160;
	
	[Imperative] 
	{
		n = 320;
		z = 640;
		if( n < z )
		{
			n = 1; 		
		}
		[Associative]
		{
			x = 10;
			y = 20;
			z = x + y * 2;
		}	
		
		[Associative]
		{
			xx = 1000;
			yy = 2000;
			zz = xx + yy * 2000;
		}
		n = 20000;
	} 
	c = b + 2;
} 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch82_relational()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"

n : int;
n = 100 > 10;  	// 1
n = 100 < 10;	// 0
n = 100 == 10;	// 0
n = 100 != 10;	// 1
n = 100 >= 10;	// 1
n = 100 <= 10;	// 0
a : int;
b : int;
i : int;
a = 1000;
b = 1000;
i = a > b;  	// 0
i = a < b;		// 0
i = a == b;		// 1
i = a != b;		// 0
i = a >= b;		// 1
i = a <= b;		// 1
j : int;
j = n > 100 + 1;
j = n < 100 + 1;
j = n == 100 + 1;
j = n != 100 + 1;
j = n >= 100 + 1;
j = n <= 100 + 1;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch86_TestStringConcatenation01()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"s3;s6;s9;
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch87_TestStringOperations()
        {
            string defectID = "MAGN-3988 Defects with Expression Interpreter Test Framework";
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
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
	//r9 = """" == """";
	//r10 = ("""" == null);
    r9 = s != ""ab"";
    ss = ""abc"";
    ss[0] = 'x';
    r10 = """" == null;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map, defectID: defectID);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch93_header2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z = 200;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch95_T01_BasicGlobalFunction()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int(x:int)
{
	return = x;
}
a = foo;
b = foo(3); //b=3;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch96_T02_GlobalFunctionWithDefaultArg()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = foo;
b = foo(3); //b=5.0;
c = foo(2, 4.0); //c = 6.0";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch97_T03_GlobalFunctionInAssocBlk()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
c;
[Associative]
{
	def foo:double(x:int, y:double = 2.0)
	{
		return = x + y;
	}
	a = foo;
	b = foo(3); //b=5.0;
	c = foo(2, 4.0); //c = 6.0
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch103_T08_FunctionPointerUpdateTest()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1:int(x:int)
{
	return = x;
}
def foo2:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = foo1;
b = a(3);
a = foo2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch104_T09_NegativeTest_Non_FunctionPointer()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int(x:int)
{
	return = x;
}
a = 2;
b = a();";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }



        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch109_T14_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int(x:int)
{
	return = x;
}
a = foo + 2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch112_T17_PassFunctionPointerAsArg()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int(x:int)
{
	return = x;
}
def foo1:int(f:function, x:int)
{
	return = f(x);
}
a = foo1(foo, 2);
b = foo;
c = foo1(b, 3);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch113_T18_FunctionPointerAsReturnVal()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int(x:int)
{
	return = x;
}
def foo1:int(f : function, x:int)
{
	return = f(x);
}
def foo2:function()
{
	return = foo;
}
a = foo2();
b = a(2);
c = foo1(a, 3);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch133_T02_SampleTestUsingCodeFromExternalFile()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"variable;
[Associative]
{
    variable = 5;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch135_T03_TestAssignmentToUndefinedVariables_negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Associative]
{
    a = b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch137_T05_TestRepeatedAssignment()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Associative]
{
    b = a = 2;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch138_T05_TestRepeatedAssignment_negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
    b = a = 2;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch140_T07_TestOutsideBlock()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b = 2;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch141_T08_TestCyclicReference()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch142_T09_TestInNestedBlock()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch143_T10_TestInFunctionScope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch146_T13_TestUsingMathAndLogicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch147_T14_TestUsingMathAndLogicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch151_T18_TestMethodCallInExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch152_T19_TestAssignmentToCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Associative]
{
	a = [[1,2],3.5];
	c = a[1];
	d = a[0][1];
        a[0][1] = 5;
       	b = a[0][1] + a[1];	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch153_T20_TestInvalidSyntax()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Associative]
{
	a = 2;;;;;
    b = 3;
       			
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch154_T21_TestAssignmentToBool()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Associative]
{
	a = true;
    b = false;      			
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch155_T22_TestAssignmentToNegativeNumbers()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch156_T23_TestUsingMathAndLogicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch157_T24_TestUsingMathematicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch158_T25_TestUsingMathematicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch160_T26_Negative_TestPropertyAccessOnPrimitive()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        
        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch164_T31_Defect_1449877()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch165_T32_Defect_1449877_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch166_T33_Defect_1450003()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch167_T34_Defect_1450727()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z;
[Associative]
{
	x = -5.5;
	y = -4.2;
 
	z = x + y;
 
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch168_T35_Defect_1450727_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        
        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch170_T37_TestOperationOnNullAndBool()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
[Associative]
{
	a = true;
	b = a + 1;
	c = null + 2;
 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch171_T38_Defect_1449928()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Associative]
{
 a = 2.3;
 b = -6.9;
 c = a / b;
} 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch172_T39_Defect_1449704()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Associative]
{
 a = b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch173_T40_Defect_1450552()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Associative]
{
 a = b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch174_T41__Defect_1452423()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d;
[Associative]
{
	b = true;
	c = 4.5;
	d = c * b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

      
        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch177_T44__Defect_1452423_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Associative]
{
	y = true;
	x = 1 + y;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch178_T45__Defect_1452423_5()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Associative]
{
	a = 4 + true;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch179_T46_TestBooleanOperationOnNull()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch183_T50_Defect_1456713()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 2.3;
b = a * 3;
//Expected : b = 6.9;
//Recieved : b = 6.8999999999999995;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch184_T51_Using_Special_Characters_In_Identifiers()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        
        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch186_T53_Collection_Indexing_On_LHS_Using_Function_Call()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo()
{
    return = 0;
}
x = [ 1, 2 ];
x[foo()] = 3;
y = x;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch188_T55_Associative_assign_If_condition_1467002()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	x = [] == null;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch265_Z017_Defect_1456898_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a : int[] )
{
	y = [Associative]
    {
		return = a[0];
    }
	x = [Imperative]
    {
		if ( a[0] == 0 ) 
		{
		    return = 0;
		}
		else
		{
			return = a[0];
		}
    }
    return = x + y;
}
a1;
b1;
[Imperative]
{
    a1 = foo( [ 0, 1 ] );
    b1 = foo( [ 1, 2 ] );
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch292_T001_Associative_Function_DeclareAfterAssignment()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	a = 1;
	b = 10;
	
	sum = Sum (a, b);
	
	def Sum : int(a : int, b : int)
	{
	
	return = a + b;
	}
	
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch293_T001_Associative_Function_Simple()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
sum;
[Associative]
{
	def Sum : int(a : int, b : int)
	{
	
		return = a + b;
	}
	
	a = 1;
	b = 10;
	
	sum = Sum (a, b);
	
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch295_T003_Associative_Function_MultilineFunction()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	def Divide : int(a:int, b:int)
	{
		return = a/b;
	}
	d = Divide (1,3);
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch296_T004_Associative_Function_SpecifyReturnType()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d;
[Associative]
{
	def Divide : double (a:int, b:int)
	{
		return = a/b;
	}
	d = Divide (1,3);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch297_T005_Associative_Function_SpecifyArgumentType()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result;
[Associative]
{
	def myFunction : int (a:int, b:int)
	{
		return = a + b;
	}
	d1 = 1.12;
	d2 = 0.5;
	
	result = myFunction (d1, d2);
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch298_T006_Associative_Function_PassingNullAsArgument()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	def myFunction : double (a: double, b: double)
	{
		return = a + b;
	}
	d1 = null;
	d2 = 0.5;
	
	result = myFunction (d1, d2);
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch299_T007_Associative_Function_NestedFunction()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result;
[Associative]
{
	def ChildFunction : double (r1 : double)
	{
	return = r1;
	
	}
	def ParentFunction : double (r1 : double)
	{
		return = ChildFunction (r1)*2;
	}
	d1 = 1.05;
	
	result = ParentFunction (d1);
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch300_T008_Associative_Function_DeclareVariableBeforeFunctionDeclaration()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"sum;
[Associative]
{
    a = 1;
	b = 10;
	def Sum : int(a : int, b : int)
	{
	
		return = a + b;
	}
	
	sum = Sum (a, b);
	
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch301_T009_Associative_Function_DeclareVariableInsideFunction()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	def Foo : int(input : int)
	{
		multiply = 5;
		divide = 10;
	
		return = [input*multiply, input/divide];
	}
	
	input = 20;
	sum = Foo (input);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch302_T010_Associative_Function_PassAndReturnBooleanValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result1;
result2;
[Associative]
{
	def Foo : bool (input : bool)
	{
		return = input;
	}
	
	input = false;
	result1 = Foo (input);
	result2 = Foo (true);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch303_T011_Associative_Function_FunctionWithoutArgument()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result1;
[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	result1 = Foo1 ();
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch304_T012_Associative_Function_MultipleFunctions()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result1;
result2;
[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	
	def Foo2 : int ()
	{
		return = 6;
	}
	
	
	result1 = Foo1 ();
	result2 = Foo2 ();
	
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch305_T013_Associative_Function_FunctionWithSameName_Negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	def Foo1 : int ()
	{
		return = 5;
	}
	
	
	
	def Foo1 : int ()
	{
		return = 6;
	}
	
	
	
	result2 = Foo2 ();
	
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch307_T015_Associative_Function_UnmatchFunctionArgument_Negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	def Foo : int (a : int)
	{
		return = 5;
	}
	
	result = Foo(1,2); 
	
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch308_T016_Associative_Function_ModifyArgumentInsideFunctionDoesNotAffectItsValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"input;
result;
originalInput;
[Associative]
{
	def Foo : int (a : int)
	{
		a = a + 1;
		return = a;
	}
	input = 3;
	result = Foo(input); 
	originalInput = input;
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch309_T017_Associative_Function_CallingAFunctionBeforeItsDeclaration()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"input;
result;
[Associative]
{
	def Level1 : int (a : int)
	{
		return = Level2(a+1);
	}
	
	def Level2 : int (a : int)
	{
		return = a + 1;
	}
	input = 3;
	result = Level1(input);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch310_temp()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	def Level1 : int (a : int)
	{
		return = Level2(a+1);
	}
	
	def Level2 : int (a : int)
	{
		return = a + 1;
	}
	input = 3;
	result = Level1(input); 
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch311_Z001_Associative_Function_Regress_1454696()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"    def Twice : double(array : double[])
    {
        return = array[0];
    }
    
    arr = [1.0,2.0,3.0];
    arr2 = Twice(arr);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch314_Z003_Defect_1456728()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def function1 (arr :  double[] )
{
    return = [ arr[0], arr [1] ];
}
a = function1([null,null]);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch315_T001_Inline_Using_Function_Call()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def fo1 : int(a1 : int)
	{
		return = a1 * a1;
	}
	a	=	10;				
	b	=	20;
				
	smallest1   =   a	<   b   ?   a	:	b;
	largest1	=   a	>   b   ?   a	:	b;
	d = fo1(a);
	smallest2   =   (fo1(a))	<   (fo1(b))  ?   (fo1(a))	:	(fo1(a));	//100
	largest2	=   (fo1(a)) >   (fo1(b))  ?   (fo1(a))	:	(fo1(b)); //400
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch317_T003_Inline_Using_Collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	Passed = 1;
	Failed = 0;
	Einstein = 56;
	BenBarnes = 90;
	BenGoh = 5;
	Rameshwar = 80;
	Jun = 68;
	Roham = 50;
	Smartness = [ BenBarnes, BenGoh, Jun, Rameshwar, Roham ]; // { 1, 0, 1, 1, 0 }
	Results = Smartness > Einstein ? Passed : Failed;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch319_T005_Inline_Using_2_Collections_In_Condition()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	a1 	=  1..3..1; 
	b1 	=  4..6..1; 
	a2 	=  1..3..1; 
	b2 	=  4..7..1; 
	a3 	=  1..4..1; 
	b3 	=  4..6..1; 
	c1 = a1 > b1 ? true : false; // { false, false, false }
	c2 = a2 > b2 ? true : false; // { false, false, false }
	c3 = a3 > b3 ? true : false; // { false, false, false, null }
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch320_T006_Inline_Using_Different_Sized_1_Dim_Collections()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	a = 10 ;
	b = ((a - a / 2 * 2) > 0)? a : a+1 ; //11
	c = 5; 
	d = ((c - c / 2 * 2) > 0)? c : c+1 ; //5 
	e1 = ((b>(d-b+d))) ? d : (d+1); //5
	//inline conditional, returning different sized collections
	c1 = [1,2,3];
	c2 = [1,2];
	a1 = [1, 2, 3, 4];
	b1 = a1>3?true:a1; // expected : {1, 2, 3, true}
	b2 = a1>3?true:c1; // expected : {1, 2, 3}
	b3 = a1>3?c1:c2;   // expected : {1, 2}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch321_T007_Inline_Using_Collections_And_ReplicationCollectionFunctionCall()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def even : int(a : int)
	{
		return = a * 2;
	}
	a =1..10..1 ; //{1,2,3,4,5,6,7,8,9,10}
	i = 1..5; 
	b = ((a[i] % 2) > 0)? even(a[i]) : a ;  // { 1, 6, 3, 10, 5 }	
	c = ((a[0] % 2) > 0)? even(a[i]) : a ; // { 4, 6, 8, 10, 12 }
	d = ((a[-2] % 2) == 0)? even(a[i]) : a ; // { 1, 2,..10}
	e1 = (a[-2] == d[9])? 9 : a[1..2]; // { 2, 3 }
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch322_T008_Inline_Returing_Different_Ranks()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	a = [ 0, 1, 2, 4];
	x = a > 1 ? 0 : [1,1]; // { 1, 1} ? 
	x_0 = x[0];
	x_1 = x[1];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch323_T009_Inline_Using_Function_Call_And_Collection_And_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def even(a : int)
	{
		return = a * 2;
	}
	def odd(a : int ) 
	{
	return = a* 2 + 1;
	}
	x = 1..3;
	a = ((even(5) > odd(3)))? even(5) : even(3); //10
	b = ((even(x) > odd(x+1)))?odd(x+1):even(x) ; // {2,4,6}
	c = odd(even(3)); // 13
	d = ((a > c))?even(odd(c)) : odd(even(c)); //53
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch324_T010_Defect_1456751_execution_on_both_true_and_false_path_issue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
a = 0;
def foo ( )
{
    a = a + 1;
    return = a;
}
x = 1 > 2 ? foo() + 1 : foo() + 2;
	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch325_T010_Defect_1456751_replication_issue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"xx;
x1;
x2;
x3;
x4;
[Imperative]
{
	a = [ 0, 1, 2];
	b = [ 3, 11 ];
	c = 5;
	d = [ 6, 7, 8, 9];
	e = [ 10 ];
	xx = 1 < a ? a : 5; // expected:5 
        yy = 0;
	if( 1 < a )
	    yy = a;
	else
	    yy = 5;
	x1 = a < 5 ? b : 5; // expected:5 
	t1 = x1[0];
	t2 = x1[1];
	c1 = 0;
	for (i in x1)
	{
		c1 = c1 + 1;
	}
	x2 = 5 > b ? b : 5; // expected:f
	t3 = x2[0];
	t4 = x2[1];
	c2 = 0;
	for (i in x2)
	{
		c2 = c2 + 1;
	}
	x3 = b < d ? b : e; // expected: {10}
	t5 = x3[0];
	c3 = 0;
	for (i in x3)
	{
		c3 = c3 + 1;
	}
	x4 = b > e ? d : [ 0, 1]; // expected {0,1}
	t7 = x4[0]; 
	c4 = 0;
	for (i in x4)
	{
		c4 = c4 + 1;
	}
}
/*
Expected : 
result1 = { 5, 5, 2 };
thisTest.Verification(mirror, ""xx"", result1, 1);
thisTest.Verification(mirror, ""t1"", 3, 1);
thisTest.Verification(mirror, ""t2"", 11, 1);
thisTest.Verification(mirror, ""c1"", 2, 1);
thisTest.Verification(mirror, ""t3"", 3, 1);
thisTest.Verification(mirror, ""t4"", 5, 1);
thisTest.Verification(mirror, ""c2"", 2, 1);
thisTest.Verification(mirror, ""t5"", 3, 1); 
thisTest.Verification(mirror, ""c3"", 1, 1);
thisTest.Verification(mirror, ""t7"", 0, 1);
thisTest.Verification(mirror, ""c4"", 1, 1);*/";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch326_T011_Defect_1467281_conditionals()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @" x = 2 == [ ]; 
 y = []==null;
 z = [[1]]==[1];
 z2 = [[1]]==1;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch328_T01_Arithmatic_List_And_List_Different_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 4, 7, 2];
list2 = [ 5, 8, 3, 6, 7, 9 ];
list3 = list1 + list2; // { 6, 12, 10, 8 }
list4 = list1 - list2; // { -4, -4, 4, -4}
list5 = list1 * list2; // { 5, 32, 21, 12 }
list6 = list2 / list1; // { 5, 2, 0, 3 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch329_T02_Arithmatic_List_And_List_Same_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 4, 7, 2];
list2 = [ 5, 8, 3, 6 ];
list3 = list1 + list2; // { 6, 12, 10, 8 }
list4 = list1 - list2; // { -4, -4, 4, -4}
list5 = list1 * list2; // { 5, 32, 21, 12 }
list6 = list2 / list1; // { 5, 2, 0, 3 }
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch330_T03_Arithmatic_Mixed()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 13, 23, 42, 65, 23 ];
list2 = [ 12, 8, 45, 64 ];
list3 = 3 * 6 + 3 * (list1 + 10) - list2 + list1 * list2 / 3 + list1 / list2; // { 128, 172, 759, 1566 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch331_T04_Arithmatic_Single_List_And_Integer()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 2, 3, 4, 5 ];
a = 5;
list2 = a + list1; // { 6, 7, 8, 9, 10 }
list3 = list1 + a; // { 6, 7, 8, 9, 10 }
list4 = a - list1; // { 4, 3, 2, 1, 0 }
list5 = list1 - a; // { -4, -3, -2, -1, 0 }
list6 = a * list1; // { 5, 10, 15, 20, 25 }
list7 = list1 * a; // { 5, 10, 15, 20, 25 }
list8 = a / list1; 
list9 = list1 / a; ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch332_T05_Logic_List_And_List_Different_Value()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 8, 10, 4, 7 ];
list2 = [ 2, 6, 10, 3, 5, 20 ];
list3 = list1 > list2; // { false, true, false, true, true }
list4 = list1 < list2;	// { true, false, false, false, false }
list5 = list1 >= list2; // { false, true, true, true, true }
list6 = list1 <= list2; // { true, false, true, false, false }
list9 = [ true, false, true ];
list7 = list9 && list5; // { false, false, true }
list8 = list9 || list6; // { true, false, true }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch333_T06_Logic_List_And_List_Same_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 8, 10, 4, 7 ];
list2 = [ 2, 6, 10, 3, 5 ];
list3 = list1 > list2; // { false, true, false, true, true }
list4 = list1 < list2;	// { true, false, false, false, false }
list5 = list1 >= list2; // { false, true, true, true, true }
list6 = list1 <= list2; // { true, false, true, false, false }
list7 = list3 && list5; // { false, true, false, true, true }
list8 = list4 || list6; // { true, false, true, false, false }
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch334_T07_Logic_Mixed()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 5, 8, 3, 6 ];
list2 = [ 4, 1, 6, 3 ];
list3 = (list1 > 1) && (list2 > list1) || (list2 < 5); // { true, true, false , true }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch335_T08_Logic_Single_List_And_Value()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 2, 3, 4, 5 ];
a = 3;
list2 = a > list1; // { true, true, false, false, false }
list3 = list1 > a; // { false, false, false, true, true }
list4 = a >= list1; // { true, true, true, false, false }
list5 = list1 >= a; // { false, false, true, true, true }
list6 = a < list1; // { false, false, false, true, true }
list7 = list1 < a; // { true, true, false, false, false }
list8 = a <= list1; // { false, false, true, true, true }
list9 = list1 <= a; // { true, true, true, false, false }
list10 = list2 && true; // { true, true, false, false, false }
list11 = false && list2; // { false, false, false, false, false }
list12 = list2 || true; // { true, true, true, true, true }
list13 = false || list2; // { true, true, false, false, false }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch336_T09_Replication_On_Operators_In_Range_Expr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{
	z5 = 4..1; // { 4, 3, 2, 1 }
	z2 = 1..8; // { 1, 2, 3, ... , 6, 7, 8 }
	z6 = z5 - z2 + 0.3;  // { 3.3, 1.3, -1.7, -2.7 }
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch369_T41_Pass_3x3_List_And_2x4_List()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(a : int, b : int)
{
	return = a * b;
}
list1 = [ [ 1, 2, 3 ], [ 4, 5, 6 ], [ 7, 8, 9 ] ];
list2 = [ [ 1, 2, 3, 4 ], [ 5, 6, 7, 8 ] ];
list3 = foo(list1, list2); // { { 1, 4, 9 }, { 20, 30, 42 } }
x = list3[0];
y = list3[1];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch370_T42_Pass_3_List_Different_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(a : int, b : int, c : int)
{
	return = a * b - c;
}
list1 = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
list2 = [ 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, -1, -2, -3 ];
list3 = [1, 4, 7, 2, 5, 8, 3 ];
list4 = foo(list1, list2, list3); // { 9, 14, 17, 26, 25, 22, 25 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch371_T43_Pass_3_List_Different_Length_2_Integers()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(a : int, b : int, c : int, d : int, e : int)
{
	return = a * b - c / d + e;
}
list1 = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
list2 = [ 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, -1, -2, -3 ];
list3 = [1, 4, 7, 2, 5, 8, 3 ];
list4 = foo(list1, list2, list3, 4, 23);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch372_T44_Pass_3_List_Same_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(a : int, b : int, c : int)
{
	return = a * b - c;
}
list1 = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
list2 = [ 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 ];
list3 = [1, 4, 7, 2, 5, 8, 3, 6, 9, 0 ];
list4 = foo(list1, list2, list3); // { 9, 14, 17, 26, 25, 22, 25, 18, 9, 10 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch373_T45_Pass_3_List_Same_Length_2_Integers()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(a : int, b : int, c : int, d : int, e : int)
{
	return = a * b - c * d + e;
}
list1 = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
list2 = [ 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 ];
list3 = [1, 4, 7, 2, 5, 8, 3, 6, 9, 0 ];
list4 = foo(list1, list2, list3, 26, 43); // { 27, -43, -115, 19, -57, -135, -7, -89, -173, 53 }  ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch374_T46_Pass_FunctionCall_Reutrn_List()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(a : int)
{
	return = a * a;
}
list1 = [ 1, 2, 3, 4, 5 ];
list3 = foo(foo(foo(list1))); // { 1, 256, 6561, 65536, 390625 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch375_T47_Pass_Single_3x3_List()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(a : int)
{
	return = a * a;
}
list1 = [ [ 1, 2, 3 ], [ 4, 5, 6 ], [ 7, 8, 9 ] ];
list2 = foo(list1); // { { 1, 4, 9 }, { 16, 25, 36 }, { 49, 64, 81 } }
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch376_T48_Pass_Single_List()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(num : int)
{
	return = num * num;
}
list1 = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
list2 = foo(list1);  // { 1, 4, 9, 16, 25, 36, 49, 64, 81, 100 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch377_T49_Pass_Single_List_2_Integers()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int(num : int, num2 : int, num3 : int)
{
	return = num * num2 - num3;
}
list1 = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
list2 = foo(list1, 34, 18); // { 16, 50, 84, 118, 152, 186, 220, 254, 288, 322 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch380_T50_1_of_3_Exprs_is_List()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ true, false, true, false, true ];
list2 = list1 ? 1 : 0; // { 1, 0, 1, 0, 1 }
list3 = true ? 10 : list2; // { 10, 10, 10, 10, 10 }
list4 = false ? 10 : list2; // { 1, 0, 1, 0, 1 }
a = [ 1, 2, 3, 4, 5 ];
b = [5, 4, 3, 2, 1 ];
c = [ 4, 3, 2, 1 ];
list5 = a > b ? 1 : 0; // { 0, 0, 0, 1, 1 }
list6 = c > a ? 1 : 0; // { 1, 1, 0, 0 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch381_T51_2_of_3_Exprs_are_Lists_Different_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 2, 3, 4, 5 ];
list2 = [ true, false, true, false ];
list3 = list2 ? list1 : 0; // { 1, 0, 3, 0 }
list4 = list2 ? 0 : list1; // { 0, 2, 0, 4 }
list5 = [ -1, -2, -3, -4, -5, -6 ];
list6 = true ? list1 : list5; // { 1, 2, 3, 4, 5 }
list7 = false ? list1 : list5; // { -1, -2, -3, -4, -5 }  
a = [ 1, 2, 3, 4 ];
b = [ 5, 4, 3, 2, 1 ];
c = [ 1, 4, 7 ];
list8 = a >= b ? a + c : 10; // { 10, 10, 10 }
list9 = a < b ? 10 : a + c; // { 10, 10, 10 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch382_T52_2_of_3_Exprs_are_Lists_Same_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ 1, 2, 3, 4, 5 ];
list2 = [ true, false, true, false, true ];
list3 = list2 ? list1 : 0; // { 1, 0, 3, 0, 5 }
list4 = list2 ? 0 : list1; // { 0, 2, 0, 4, 0 }
list5 = true ? list3 : list4; // { 1, 0, 3, 0, 5 }
list6 = true ? list4 : list3; // {0, 2, 0, 4, 0 }
a = [ 1, 2, 3, 4, 5 ];
b = [ 5, 4, 3, 2 ];
list7 = a > b ? a + b : 10; // { 10, 10, 10, 6 }
list8 = a <= b ? 10 : a + b; // { 10, 10, 10, 6 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch383_T53_3_of_3_Exprs_are_different_dimension_list()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [ [ 1, 2, 3 ], [ 4, 5, 6 ] ];
b = [ [ 1, 2 ],  [ 3, 4 ], [ 5, 6 ] ];
c = [ [ 1, 2, 3, 4 ], [ 5, 6, 7, 8 ], [ 9, 10, 11, 12 ] ];
list = a > b ? b + c : a + c; // { { 2, 4, }, { 8, 10 } } ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch384_T54_3_of_3_Exprs_are_Lists_Different_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ true, false, true, true, false ];
list2 = [ 1, 2, 3, 4 ];
list3 = [ -1, -2, -3, -4, -5, -6 ];
list4 = list1 ? list2 : list3; // { 1, -2, 3, 4 }
list5 = !list1 ? list2 : list4; // { 1, 2, 3, 4 }
list6 = [ -1, -2, -3, -4, -5 ];
list7 = list1 ? list2 : list6; // { 1, -2, 3, 4 }
a = [ 3, 0, -1 ];
b = [ 2, 1, 0, 3 ];
c = [ -2, 4, 1, 2, 0 ];
list8 = a < c ? b + c : a + c; // { 1, 4, 1 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch385_T55_3_of_3_Exprs_are_Lists_Same_Length()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ true, false, false, true ];
list2 = [ 1, 2, 3, 4 ];
list3 = [ -1, -2, -3, -4 ];
list4 = list1 ? list2 : list3; // { 1, -2, -3, 4 }
list5 = !list1 ? list2 : list3; // { -1, 2, 3, -4 }
a = [ 1, 4, 7 ];
b = [ 2, 8, 5 ];
c = [ 6, 9, 3 ];
list6 = a > b ? b + c : b - c; // { -4, -1, 8 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch386_T56_UnaryOperator()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"list1 = [ true, true, false, false, true, false ];
list2 = !list1; // { false, false, true, true, false, true }
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch387_T001_Simple_Update()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
b = a + 1;
a = 2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch388_T002_Update_Collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0..4..1;
b = a;
c = b[2];
a = 10..14..1;
b[2] = b[2] + 1;
a[2] = a[2] + 1;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch389_T003_Update_In_Function_Call()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1 ( a : int ) 
{
    return = a + 1;
}
def foo2 ( a : int[] ) 
{
    a[0] = a[1] + 1;
	return = a;
}
def foo3 ( a : int[] ) 
{
    b = a;
	b[0] = b[1] + 1;
	return = b;
}
a = 0..4..1;
b = a[0];
c = foo1(b);
d = foo2(a);
e1 = foo3(a);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch390_T004_Update_In_Function_Call_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1 ( a : int ) 
{
    return = a + 1;
}
def foo3 ( a : int[] ) 
{
    b = a;
	b[0] = b[1] + 1;
	return = b;
}
a = 0..4..1;
b = a[0];
c = foo1(b);
e1 = foo3(a);
a = 10..14..1;
a[1] = a[1] + 1;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch391_T005_Update_In_collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=1;
b=2;
c=4;
collection = [a,b,c];
collection[1] = collection[1] + 0.5;
d = collection[1];
d = d + 0.1; // updates the result of accessing the collection
b = b + 0.1; // updates the source member of the collection";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch395_T009_Update_Of_Undefined_Variables()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"u1 = u2;
u2 = 3;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch396_T010_Update_Of_Singleton_To_Collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"s1 = 3;
s2 = s1 -1;
s1 = [ 3, 4 ] ;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch397_T011_Update_Of_Variable_To_Null()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x = 1;
y = 2/x;
x = 0;
v1 = 2;
v2 = v1 * 3;
v1 = null;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch398_T012_Update_Of_Variables_To_Bool()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"p1 = 1;
p2 = p1 * 2;
p1 = false;
q1 = -3.5;
q2 = q1 * 2;
q1 = true;
s1 = 1.0;
s2 = s1 * 2;
s1 = false;
t1 = -1;
t2 = t1 * 2;
t1 = true;
r1 = 1;
r2 = r1 * 2;
r1 = true;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch406_T019_Update_General()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"X = 1;
Y = X + 1;
X = X + 1;
X = X + 1;
//Y = X + 1;
//X  = X + 1;
test = X + Y;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch423_T024_Defect_1459470()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0..4..1;
b = a;
c = b[2];
a = 10..14..1;
b[2] = b[2] + 1;
a[2] = a[2] + 1;
x = a;
					        
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch425_T024_Defect_1459470_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ()
{
	a = 0..4..1;
	b = a;
	c = b[2];
	a = 10..14..1;
	b[2] = b[2] + 1;
	a[2] = a[2] + 1;
	return = true;
	
}
a :int[];
b : int[];
c : int;
test = foo();
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch426_T024_Defect_1459470_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1,2,3,4];
b = a;
c = b[2];
d = a[2];
a[0..1] = [1, 2];
b[2..3] = 5;
	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch427_T025_Defect_1459704()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = b;
b = 3;
c = a;
					        
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch435_T029_Defect_1460139_Update_In_Class()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"X = 1;
Y = X + 1;
X = X + 1;
X = X + 1; // this line causing the problem
test = X + Y;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch438_T031_Defect_1467491_ImportUpdate_Main()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import(""T031_Defect_1467491_ImportUpdate_Sub.ds"");
t = 5;
z = a.x;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch462_T022_Array_Marshal()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (Dummy from ""FFITarget.dll"");
dummy = Dummy.Dummy();
arr = [0.0,1.0,2.0,3.0,4.0,5.0,6.0,7.0,8.0,9.0,10.0];
sum_1_10 = dummy.SumAll1D(arr);
twice_arr = dummy.Twice(arr);
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch465_T02_SampleTestUsingCodeFromExternalFile()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"variable;
[Imperative]
{
    variable = 5;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch466_T03_TestAssignmentToUndefinedVariables_negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
    a = b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch468_T05_TestRepeatedAssignment_negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Imperative]
{
    b = a = 2;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch470_T07_TestOutsideBlock()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b = 2;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch471_T08_TestCyclicReference()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch472_T09_TestInNestedBlock()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch475_T12_TestUsingMathAndLogicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch476_T13_TestUsingMathAndLogicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch477_T14_TestUsingMathAndLogicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch482_T19_TestAssignmentToCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Imperative]
{
	a = [[1,2],3.5];
	c = a[1];
	d = a[0][1];
        a[0][1] = 5;
       	b = a[0][1] + a[1];
        a = 2;		
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch483_T20_TestInvalidSyntax()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Imperative]
{
	a = 2;;;;;
    b = 3;
       			
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch484_T21_TestAssignmentToBool()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Imperative]
{
	a = true;
    b = false;      			
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch485_T22_TestAssignmentToNegativeNumbers()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch486_T23_TestUsingMathAndLogicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch487_T24_TestUsingMathematicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch488_T25_TestUsingMathematicalExpr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch493_T31_Defect_1449877()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch496_T34_Defect_1450727()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z;
[Imperative]
{
	x = -5.5;
	y = -4.2;
 
	z = x + y;
 
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch498_T36_Defect_1450555()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch499_T37_TestOperationOnNullAndBool()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
[Imperative]
{
	a = true;
	b = a + 1;
	c = null + 2;
 }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch500_T38_Defect_1449928()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Imperative]
{
 a = 2.3;
 b = -6.9;
 c = a / b;
} 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch501_T39_Defect_1449704()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
 a = b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch502_T40_Defect_1450552()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
 a = b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch503_T41__Defect_1452423()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d;
[Imperative]
{
	b = true;
	c = 4.5;
	d = c * b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch504_T42__Defect_1452423_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ -2,3,4.5,true ];
	x = 1;
	for ( y in a )
	{
		x = x *y;       //incorrect result
    }
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch505_T43__Defect_1452423_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch506_T44__Defect_1452423_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	y = true;
	x = 1 + y;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch507_T45__Defect_1452423_5()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = 4 + true;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch508_T46_TestBooleanOperationOnNull()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch509_T47_TestBooleanOperationOnNull()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch511_T49_TestForStringObjectType()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch513_T51_Assignment_Using_Negative_Index()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [ 0, 1, 2, 3 ];
c1 = a [-1];
c2 = a [-2];
c3 = a [-3];
c4 = a [-4];
c5 = a [-5];
c6 = a [-1.5];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch514_T52_Defect_1449889()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
c;
d;
[Imperative]
{
	a = b;
    c = foo();
	d = 1;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch521_T59_Defect_1455590()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	a = b = 2;
c;d;e;
	[Imperative]
	{
		c = d = e = 4+1;
	}
		";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch522_T60_Defect_1455590_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a )
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch523_T61_TestBooleanOperationOnNull()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b1;
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch524_T62_Defect_1456721()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b = true;
a = 2 * b;
c = 3;
b1 = null;
a1 = 2 * b1;
c1 = 3;
a2 = 1 + true;
b2 = 2 * true;
c2 = 3  - true;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch526_T64_Defect_1450715()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
    a = [ 1, 0.5, null, [2,3 ] ,[[0.4, 5], true ] ];
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch528_T02_TestAssocInsideImp()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch529_T03_TestImpInsideAssoc()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch535_T09_Defect_1449829()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch539_T13_Defect_1450527()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch541_T15_Defect_1452044()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
[Associative]
{
	a = 2;
	[Imperative]
	{
		b = 2 * a;
	}
		
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch542_T16__Defect_1452588()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1,2,3,4,5 ];
	for( y in a )
	{
		x = 5;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch543_T17__Defect_1452588_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch546_T01_WhileBreakContinue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
y;
[Imperative]
{
    x = 0;
    y = 0;
    while (true) 
    {
        x = x + 1;
        if (x > 10)
            break;
        
        if ((x == 1) || (x == 3) || (x == 5) || (x == 7) || (x == 9))
            continue;
        
        y = y + 1;
    }
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch547_T02_WhileBreakContinue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"sum;
[Imperative]
{
    x = 0;
    sum = 0;
    while (x <= 10) 
    {
        x = x + 1;
        if (x >= 5)
            break;
        
        y = 0;
        while (true) 
        {
            y = y + 1;
            if (y >= 10)
                break;
        }
        // y == 10 
        sum = sum + y;
    }
    // sum == 40 
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch548_T03_ForLoopBreakContinue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"sum;
[Imperative]
{
    sum = 0;
    for (x in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13])
    {
        if (x >= 11)
            break;
        sum = sum + x;
    }
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch549_T04_ForLoopBreakContinue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"sum;
[Imperative]
{
    sum = 0;
    for (x in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10])
    {
        sum = sum + x;
        if (x <= 5)
            continue;
        sum = sum + 1;
    }
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch555_T05_TestForLoopInsideNestedBlocks()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Associative]
{
	a = [ 4, 5 ];
	[Imperative]
	{
		x = 0;
		b = [ 2,3 ];
		for( y in b )
		{
			x = y + x;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch556_T06_TestInsideNestedBlocksUsingCollectionFromAssociativeBlock()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
[Associative]
{
	a = [ 4,5 ];
	b =[Imperative]
	{
	
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
		return = x;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch557_T07_TestForLoopUsingLocalVariable()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1, 2, 3, 4, 5 ];
	x = 0;
	for( y in a )
	{
		local_var = y + x;	
        x = local_var + y;		
	}
	z = local_var;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch559_T09_TestForLoopWithBreakStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1,2,3 ];
	x = 0;
	for( i in a )
	{
		x = x + 1;
		break;
	}	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch560_T10_TestNestedForLoops()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1,2,3 ];
	x = 0;
	for ( i in a )
	{
		for ( j in a )
        {
			x = x + j;
		}
	}
}	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch561_T11_TestForLoopWithSingleton()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [1];
	b = 1;
	x = 0;
 
	for ( y in a )
	{
		x = x + 1;
	}
 
	for ( y in b )
	{
		x = x + 1;
	}
} ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch562_T12_TestForLoopWith2DCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
z;
[Imperative]
{
	a = [[1],[2,3],[4,5,6]];
	x = 0;
	i = 0;
    for (y in a)
	{
		x = x + y[i];
	    i = i + 1;	
	}
	z = 0;
    for (i1 in a)
	{
		for(j1 in i1)
		{
		    z = z + j1;
		}
	}
			
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch563_T13_TestForLoopWithNegativeAndDecimalCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
y;
[Imperative]
{
	a = [ -1,-3,-5 ];
	b = [ 2.5,3.5,4.2 ];
	x = 0;
	y = 0;
    for ( i in a )
	{
		x = x + i;
	}
	
	for ( i in b )
	{
		y = y + i;
	}
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch564_T14_TestForLoopWithBooleanCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{ 
	a = [ true, false, true, true ];
	x = false;
	
	for( i in a )
	{
	    x = x + i;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch565_T15_TestForLoopWithMixedCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
y;
[Imperative]
{
	a = [ -2, 3, 4.5 ];
	x = 1;
	for ( y in a )
	{
		x = x * y;       
    }
	
	a = [ -2, 3, 4.5, true ];
	y = 1;
	for ( i in a )
	{
		y = i * y;       
    }
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch566_T16_TestForLoopInsideIfElseStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = 1;
	b = [ 2,3,4 ];
	if( a == 1 )
	{
		for( y in b )
		{
			a = a + y;
		}
	}
	
	else if( a !=1)
	{
		for( y in b )
		{
			a = a + 1;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch567_T17_TestForLoopInsideNestedIfElseStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = 1;
	b = [ 2,3,4 ];
	c = 1;
	if( a == 1 )
	{
		if(c ==1)
		{
			for( y in b )
			{
				a = a + y;
			}
		}	
	}
	
	else if( a !=1)
	{
		for( y in b )
		{
			a = a + 1;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch568_T18_TestForLoopInsideWhileStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = 1;
	b = [ 1,1,1 ];
	x = 0;
	
	if( a == 1 )
	{
		while( a <= 5 )
		{
			for( i in b )
			{
				x = x + 1;
			}
			a = a + 1;
		}
	}
}
			";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch569_T19_TestForLoopInsideNestedWhileStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	i = 1;
	a = [1,2,3,4,5];
	x = 0;
	
	while( i <= 5 )
	{
		j = 1;
		while( j <= 5 )
		{
			for( y in a )
			{
			x = x + 1;
			}
			j = j + 1;
		}
		i = i + 1;
	}
}	
		
			";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch570_T20_TestForLoopWithoutBracket()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1, 2, 3 ];
    x = 0;
	
	for( y in a )
	    x = y;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch571_T21_TestIfElseStatementInsideForLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1,2,3,4,5 ];
	x = 0;
	
	for ( i in a )
	{
		if( i >=4 )
			x = x + 3;
			
		else if ( i ==1 )
			x = x + 2;
		
		else
			x = x + 1;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch572_T22_TestWhileStatementInsideForLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1,2,3 ];
	x = 0;
	
	for( y in a )
	{
		i = 1;
		while( i <= 5 )
		{
			j = 1;
			while( j <= 5 )
			{
				x = x + 1;
				j = j + 1;
			}
		i = i + 1;
		}
	}
}
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch573_T23_TestForLoopWithDummyCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1;
a2;
a3;
a4;
a5;
a6;
[Imperative]
{
	a = [0, 0, 0, 0, 0, 0];
	b = [5, 4, 3, 2, 1, 0, -1, -2];
	i = 5;
	for( x in b )
	{
		if(i >= 0)
		{
			a[i] = x;
			i = i - 1;
		}
	}
	a1 = a[0];
	a2 = a[1];
	a3 = a[2];
	a4 = a[3];
	a5 = a[4];
	a6 = a[5];
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch574_T24_TestForLoopToModifyCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a6;
a7;
[Imperative]
{
	a = [1,2,3,4,5,6,7];
	i = 0;
	for( x in a )
	{
	
		a[i] = a[i] + 1;
		i = i + 1;
		
	}
	a1 = a[0];
	a2 = a[1];
	a3 = a[2];
	a4 = a[3];
	a5 = a[4];
	a6 = a[5];
	a7 = a[6];
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch575_T25_TestForLoopEmptyCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [];
	x = 0;
	for( i in a )
	{
		x = x + 1;
	}
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch576_T26_TestForLoopOnNullObject()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	x = 0;
	
	for ( i in b )
	{
		x = x + 1;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch578_T28_Defect_1452966()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1,2,3 ];
	x = 0;
	for ( i in a )
	{
		for ( j in a )
        {
			x = x + j;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch579_T29_Defect_1452966_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [[6],[5,4],[3,2,1]];
	x = 0;
	
    for ( i in a )
	{
		for ( j in i )
		{
			x = x + j;
		}
	}		
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch580_T30_ForLoopNull()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 1,null,null ];
	x = 1;
	
	for( i in a )
	{
		x = x + 1;
	}
}
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch583_T33_ForLoopToReplaceReplicationGuides()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [ 1, 2 ];
b = [ 3, 4 ];
//c = a<1> + b <2>;
dummyArray = [ [ 0, 0 ], [ 0, 0 ] ];
counter1 = 0;
counter2 = 0;
[Imperative]
{
	for ( i in a )
	{
		counter2 = 0;
		
		for ( j in b )
		{	    
			dummyArray[ counter1 ][ counter2 ] = i + j;
			
			counter2 = counter2 + 1;
		}
		counter1 = counter1 + 1;
	}
	
}
a1 = dummyArray[0][0];
a2 = dummyArray[0][1];
a3 = dummyArray[1][0];
a4 = dummyArray[1][1];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch584_T34_Defect_1452966()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"sum;
[Imperative]
{
	a = [ 1, 2, 3, 4 ];
	sum = 0;
	
	for(i in a )
	{
		for ( i in a )
		{
			sum = sum + i;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch585_T35_Defect_1452966_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"sum;
[Imperative]
{
	a = [ [1, 2, 3], [4], [5,6] ];
	sum = 0;
	
	for(i in a )
	{
		for (  j in i )
		{
			sum = sum + j;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch586_T36_Defect_1452966_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
[Associative]
{
	a = [ [1, 2, 3], [4], [5,6] ];
	
	def forloop :int ( a: int[]..[] )
	{
		sum = 0;
		sum = [Imperative]
		{
			for(i in a )
			{
				for (  j in i )
				{
					sum = sum + j;
				}
			}
			return = sum;
		}
		return = sum;
	}
	
	b =forloop(a);
	
	
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch587_T37_Defect_1454517()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"	a = [ 4,5 ];
	
	b =[Imperative]
	{
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
		
		return = x;
	}
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch588_T38_Defect_1454517_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"	a = [ 4,5 ];
	x = 0;
	
	[Imperative]
	{
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
	}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch589_T38_Defect_1454517_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a : int [] )
{
    x = 0;
	x = [Imperative]
	{	
		for( y in a )
		{
			x = x + y;
		}
		return =x;
	}
	return = x;
}
a = [ 4,5 ];	
b;
[Imperative]
{
	b = foo(a);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch590_T39_Defect_1452951()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Associative]
{
	a = [ 4,5 ];
   
	[Imperative]
	{
	       //a = { 4,5 }; // works fine
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch594_T40_Create_3_Dim_Collection_Using_For_Loop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x = [ [ [ 0, 0] , [ 0, 0] ], [ [ 0, 0 ], [ 0, 0] ]];
a = [ 0, 1 ];
b = [ 2, 3];
c = [ 4, 5 ];
y = [Imperative]
{
	c1 = 0;
	for ( i in a)
	{
	    c2 = 0;
		for ( j in b )
		{
		    c3 = 0;
			for ( k in c )
			{
			    x[c1][c2][c3] = i + j + k;
				c3 = c3 + 1;
			}
			c2 = c2+ 1;
		}
		c1 = c1 + 1;
	}
	
	return = x;
			
}
p1 = y[0][0][0];
p2 = y[0][0][1];
p3 = y[0][1][0];
p4 = y[0][1][1];
p5 = y[1][0][0];
p6 = y[1][0][1];
p7 = y[1][1][0];
p8 = y[1][1][1];
p9 = x [1][1][1];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch595_T41_Create_3_Dim_Collection_Using_For_Loop_In_Func_Call()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def foo :int[]..[]( a : int[], b:int[], c :int[])
{
	y = [Imperative]
	{
		x = [ [ [ 0, 0] , [ 0, 0] ], [ [ 0, 0 ], [ 0, 0] ]];
		c1 = 0;
		for ( i in a)
		{
			c2 = 0;
			for ( j in b )
			{
				c3 = 0;
				for ( k in c )
				{
					x[c1][c2][c3] = i + j + k;
					c3 = c3 + 1;
				}
				c2 = c2+ 1;
			}
			c1 = c1 + 1;
		}		
		return = x;				
	}
	return = y;
}
a = [ 0, 1 ];
b = [ 2, 3];
c = [ 4, 5 ];
y = foo ( a, b, c );
p1 = y[0][0][0];
p2 = y[0][0][1];
p3 = y[0][1][0];
p4 = y[0][1][1];
p5 = y[1][0][0];
p6 = y[1][0][1];
p7 = y[1][1][0];
p8 = y[1][1][1];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch600_T44_Use_Bracket_Around_Range_Expr_In_For_Loop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"s;
[Imperative] {
s = 0;
for (i in (0..10)) {
	s = s + i;
}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch602_T01_TestAllPassCondition()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
 
 c1 = [ 0, 1, 2, 3];
 c2 = [ 1, 0.2];
 c3 = [ 0, 1.4, true ];
 c4 = [[0,1], [2,3 ] ];
 
 x = [0, 0, 0, 0];
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
 
 
 y = [0, 0, 0, 0, 0];
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
 
 
 z = [0, 0, 0, 0];
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch603_T02_IfElseIf()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch604_T03_MultipleIfStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch605_T04_IfStatementExpressions()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch608_T07_ScopeVariableInBlocks()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch609_T08_NestedBlocks()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch610_T09_NestedIfElseInsideWhileStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch611_T10_TypeConversion()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"temp;
[Imperative]
{
    temp = 0;
    a=4.0;
    if(a==4)
        temp=1;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch614_T13_IfElseIf()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch615_T14_IfElseStatementExpressions()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch616_T15_TestEmptyIfStmt()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
[Imperative]
{
 a = 0;
 b = 1;
 if(a == b);
 else a = 1;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch617_T16_TestIfConditionWithNegation_Negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch618_T17_WhileInsideElse()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch619_T18_WhileInsideIf()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch620_T19_BasicIfElseTestingWithNumbers()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch621_T20_BasicIfElseTestingWithNumbers()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch622_T21_IfElseWithArray_negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Imperative]
{
    a = [ 0, 4, 2, 3 ];
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch623_T22_IfElseWithArrayElements()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Imperative]
{
    a = [ 0, 4, 2, 3 ];
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch627_T26_IfElseWithNegatedCondition()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch636_T35_IfElseWithEmptyBody()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch638_T37_Defect_1450920()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch640_T39_Defect_1450920_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch641_T40_Defect_1450843()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch642_T41_Defect_1450778()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch643_T42_Defect_1449707()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch644_T43_Defect_1450706()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch646_T45_Defect_1450506()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch647_T46_TestIfWithNull()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch648_T47_Defect_1450858()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch650_T49_Defect_1450783()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch652_T51_Defect_1452588()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch653_T52_Defect_1452588_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch656_T55_Defect_1450506()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch659_T58_Defect_1450932_comparing_collection_with_singleton_Associative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d2;
[Associative]
{
    a2 = [ 0, 1 ];
	b2 = 1;
	d2 = a2 > b2 ? true : [ false, false];
    //f2 = a2 > b2;	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch660_T58_Defect_1450932_comparing_collection_with_singleton_Associative_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"f2;
[Associative]
{
    a2 = [ 0, 1 ];
    b2 = 1;	
    f2 = a2 > b2;	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch661_T58_Defect_1450932_comparing_collection_with_singleton_Associative_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"f2;
[Associative]
{
    a2 = [ 0, 1 ];
    b2 = 1;
    d2 = a2 > b2 ? true : [ false, false];
    f2 = a2 > b2;	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch662_T58_Defect_1450932_comparing_collection_with_singleton_Imperative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
d;
f;
[Imperative]
{
    a = [ 0, 1 ];
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
    d = a > b ? true : [ false, false];
    f = a > b;
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch663_T59_Defect_1453881()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch664_T59_Defect_1453881_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ()
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
        return = [ b, d ];		
	}	
	return = c;
}
test = foo();
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch668_T62_Condition_Not_Evaluate_ToBool()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
A;
[Imperative]
{
    A = 1;
    if (0)       
 	   A = 2; 
    else 
	  A= 3;
}
//expected A=1;//Received A=3;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch672_T003_Inline_Using_Collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{
	Passed = 1;
	Failed = 0;
	Einstein = 56;
	BenBarnes = 90;
	BenGoh = 5;
	Rameshwar = 80;
	Jun = 68;
	Roham = 50;
	Smartness = [ BenBarnes, BenGoh, Jun, Rameshwar, Roham ]; // { 1, 0, 1, 1, 0 }
	Results = Smartness > Einstein ? Passed : Failed;
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch673_T005_Inline_Using_2_Collections_In_Condition()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{
	a1 	=  1..3..1; 
	b1 	=  4..6..1; 
	a2 	=  1..3..1; 
	b2 	=  4..7..1; 
	a3 	=  1..4..1; 
	b3 	=  4..6..1; 
	c1 = a1 > b1 ? true : false; // { false, false, false }
	c2 = a2 > b2 ? true : false; // { false, false, false }
	c3 = a3 > b3 ? true : false; // { false, false, false, null }
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch674_T006_Inline_Using_Different_Sized_1_Dim_Collections()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{
	a = 10 ;
	b = ((a - a / 2 * 2) > 0)? a : a+1 ; //11
	c = 5; 
	d = ((c - c / 2 * 2) > 0)? c : c+1 ; //5 
	e1 = ((b>(d-b+d))) ? d : (d+1); //5
	//inline conditional, returning different sized collections
	c1 = [1,2,3];
	c2 = [1,2];
	a1 = [1, 2, 3, 4];
	b1 = a1>3?true:a1; // expected : {1, 2, 3, true}
	b2 = a1>3?true:c1; // expected : {1, 2, 3}
	b3 = a1>3?c1:c2;   // expected : {1, 2}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch676_T008_Inline_Returing_Different_Ranks()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	a = [ 0, 1, 2, 4];
	x = a > 1 ? 0 : [1,1]; // { 1, 1} ? 
	x_0 = x[0];
	x_1 = x[1];
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch678_T010_Inline_Using_Literal_Values()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
c;
d;
e;
f;
g;
h;
[Imperative]
{
	a = 1 > 2.5 ? false: 1;
	b = 0.55 == 1 ? true : false;
	c = (( 1 + 0.5 ) / 2 ) <= (200/10) ? (8/2) : (6/3);
	d = true ? true : false;
	e = false ? true : false;
	f = true == true ? 1 : 0.5;
	g = (1/3.0) > 0 ? (1/3.0) : (4/3);
	h = (1/3.0) < 0 ? (1/3.0) : (4/3);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch682_T014_Inline_Using_Collections()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"t1;t2;t3;t4;t5;t7;
c1;c2;c3;c4;
[Imperative]
{
	a = [ 0, 1, 2];
	b = [ 3, 11 ];
	c = 5;
	d = [ 6, 7, 8, 9];
	e = [ 10 ];
	x1 = a < 5 ? b : 5;
	t1 = x1[0];
	t2 = x1[1];
	c1 = 0;
	for (i in x1)
	{
		c1 = c1 + 1;
	}
	
	x2 = 5 > b ? b : 5;
	t3 = x2[0];
	t4 = x2[1];
	c2 = 0;
	for (i in x2)
	{
		c2 = c2 + 1;
	}
	
	x3 = b < d ? b : e;
	t5 = x3[0];
	c3 = 0;
	for (i in x3)
	{
		c3 = c3 + 1;
	}
	
	x4 = b > e ? d : [ 0, 1];
	t7 = x4[0];	
	c4 = 0;
	for (i in x4)
	{
		c4 = c4 + 1;
	}
	
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch684_T016_Inline_Using_Operators()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo (a:int )
{
	 return = a;   
}
a = 1+2 > 3*4 ? 5-9 : 10/2;
b = a > -a ? 1 : 0;
c = 2> 1 && 4>3 ? 1 : 0;
d = 1 == 1 || (1 == 0) ? 1 : 0;
e1 = a > b && c > d ? 1 : 0;
f = a <= b || c <= d ? 1 : 0;
g = foo([ 1, 2 ]) > 3+ foo([4,5,6]) ?  1 : 3+ foo([4,5,6]);
i = [1,3] > 2 ? 1: 0;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch685_T017_Inline_In_Function_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1 ( b )
{
	return = b == 0 ? b : b+1;
	
}
def foo2 ( x )
{
	y = [Imperative]
	{
	    if(x > 0)
		{
		   return = x >=foo1(x) ? x : foo1(x);
		}
		return = x >=2 ? x : 2;
	}
	x1 = y == 0 ? 0 : y;
	return = y + x1;
}
a1 = foo1(4);
a2 = foo2(3);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch687_T019_Defect_1456758()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b = true;
a1 = b && true ? -1 : 1;
a2;
[Imperative]
{
	a2 = b && true ? -1 : 1;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch688_T020_Nested_And_With_Range_Expr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
a1 =  1 > 2 ? true : 2 > 1 ? 2 : 1;
a2 =  1 > 2 ? true : 0..3;
b = [0,1,2,3];
a3 = 1 > 2 ? true : b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch689_T021_Defect_1467166_array_comparison_issue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative] 
{
    a = [ 0, 1, 2]; 
    xx = a < 1 ? 1 : 0;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch696_T06_InsideNestedBlock()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch697_T07_BreakStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch698_T08_ContinueStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch699_T09_NestedWhileStatement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch701_T11_WhilewithLogicalOperators()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch704_T14_TestFactorialUsingWhileStmt()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch705_T15_TestWhileWithDecimalvalues()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch706_T16_TestWhileWithLogicalOperators()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch707_T17_TestWhileWithBool()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
    a = 0;	
    while(a == false)
	{
		a = 1;	
	}	
	
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch708_T18_TestWhileWithNull()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch709_T19_TestWhileWithIf()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch710_T20_TestWhileToCreate2DimArray()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def Create2DArray( col : int)
{
	result = [Imperative]
    {
		array = [ 1, 2 ];
		counter = 0;
		while( counter < col)
		{
			array[counter] = [ 1, 2];
			counter = counter + 1;
		}
		return = array;
	}
    return = result;
}
x = Create2DArray( 2) ;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch711_T21_TestWhileToCallFunctionWithNoReturnType()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ()
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch712_T22_Defect_1463683()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ()
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch715_T22_Defect_1463683_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ()
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch716_test()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch719_T03_Assignment_Slicing_With_Collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a:int[] )
{
	a[0] = 0;
	return = a;
}
	a = [1,2,3];
	c = foo ( a  );
	d = c[0];
	e = c[1];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch721_80574Transpose()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing Transpose
x = [[1,2],[3,4]];
y = Transpose(x) ;
a=[1,2];
b = Transpose(a);
a1 = [[[1,2]],[[3,4]]];
b1 = Transpose(a1);
x1 = [[1,2],[3,4,5]];
y1 = Transpose(x1);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch723_80576allFalse()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing AllFalse
w = false ;
x = [1,2,3,false];
y = [-1,-2,3,4];
z = [w,w,w,w];
a = AllFalse(a);//false 
b = AllFalse(y); //false
c = AllFalse(z) ; //true 
w1 = [P1, ""s"", [1,2,3],true,false];
a1 = AllFalse(w1);
a2 = AllFalse(3);
w2 = 5.0;
a3 = AllFalse(w2);
a4 = AllFalse(null);
w3 = [];
a5 = AllFalse(w3);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch732_80586countFalse()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing CountFalse
w = false ;
x = [1,2,3,false];
y = [-1,-2,3,4];
z = [w,w,w,true];
a = CountFalse(x);//1 
b = CountFalse(y); //0
c = CountFalse(z) ; //3
g = [false];
h = CountFalse(g); //1 
x1 = [[true,false],true,false,[[[false]]],[false,false]];
y1 = CountFalse(x1);
//negative testing
a1 = ""s"";
b1 = CountFalse(a1); //0
b2 = CountFalse(a2); //0
b3 = CountFalse(null); //0
a4 = [];
b4 = CountFalse(a4); //0";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch733_80587countTrue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing countTrue
w = true ;
x = [1,2,3,true];
y = [-1,-2,3,4];
z = [w,w,w,w];
a = countTrue(x);//1 
b = countTrue(y); //0
c = countTrue(z) ; //4 
x1 = [[true,false],true,false,[[[false]]],[false,false]];
y1 = countTrue(x1);
//negative testing
a1 = ""s"";
b1 = countTrue(a1); //0
b2 = countTrue(a2); //0
b3 = countTrue(null); //0
a4 = [];
b4 = countTrue(a4); //0";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch735_80589flatten()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing flatten()
x = [[1.1],[2.3,3]];
y = [[[-1,2,-3]],6];
a = Flatten(x);
b = Flatten(y); 
c = Flatten(y * 0.1);
d = Flatten(2);
e1 = Flatten(-0.2);
//negative testing
a1 = ""s"";
b1 = Flatten(a1); //b1 = ""s""
b3 = Flatten(null); // b3 = null
a4 = [];
b4 = Flatten(a4); //b4 = null
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch736_80590floor()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing floor()
x = 1.5 ; 
y = 0.01 ; 
z = -0.1 ; 
a = floor(x) ;//1
b = floor(y) ;//0 
c = floor(z) ; //-1
d = floor(-1.5) ;//-2
e1 = floor(-2);//-2
g = floor(2.1);//2
h = floor([0.2*0.2 , 1.2 , -1.2 , 3.4 , 3.6 , -3.6]);//{0,1,-2,3,2,-4}
//negative testing
a1 = ""s"";
b1 = floor(a1); //null
b2 = floor(a2); //null
b3 = floor(null); //null
a4 = [];
b4 = floor(a4); //null";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch738_80592log10()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing log1010()
x =1 ; 
y = 10000;
a = log10(x) ;//0
b = log10(10) ; //1
c = log10([0.1,-1,-0.9,0.9,y]);//{ -1, -1.#IO, -1.#IO, -0.105, 4.000 }
d = log10(a); // expected -1.#IO(invalid output) but then d = -2147483648
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch739_80593map()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing map(rangeMin: double, rangeMax: double, inputValue: double)
a = map(3,8,12); //1 
b = map(4,5,12); //1
c = map(2.3,5,9); //1
d = map(2.3,5,0.2); //0
e1 = map(2.3,5.2,0.2);  //0 
f = map([1,2.2,3,4],[2,4.2,3.4,5],1.2);//{0.200, 0, 0, 0 }
g = map([1,2],[3.4,4.5],[0,1]); // {0,0}
g1 = map([1,2,3],[4,5],[0,1]); // 
g2 = map(4,4,2); // 
g3 = map(4,4,4); // 
//negative testing
g4 = map([[1,2],[2,3]],[1,2],3);
b1 = map(""s"",8,12); //1 
b3 = map(null,4,5); //null
b4 = map(null,null,null); //null
b5 = map(4,null,5); //null
b6 = map(5,4,null); //null
b7 = map([],5,4); //null
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch740_80594mapto()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing mapTo( rangeMin: double, rangeMax: double,  inputValue: double,  targeRangeMin: double,  targetRangeMax: double)
a = mapTo(3,8,12,4,5); //5
b = mapTo(4,5,12,3,4); //4
c = mapTo(2.3,5,9,10,12); //12
d = mapTo(2.3,5,0.2,[1,2,3,4],5); //{1,2,3,4}
e1 = mapTo(-2.3,5.2,0.2,[-2.3,3.4],[4.5,5.6]);  //{-0.033,4.133}
f = mapTo([1,2],[3.4,4.5],[0,1],2,3); // {2,2}
//negative testing
g4 = map([[1,2],[2,3]],[1,2],3,4,5);
b1 = map(""s"",8,12,14,15); //1 
b3 = map(null,4,5,5,6); //null
b4 = map(null,null,null,null,null); //null
b5 = map(4,null,5,6,7); //null
b6 = map(5,4,null,7,8); //null
b7 = map([],5,4,8,9); //null
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch741_80595max()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing max()
a = max(4.5,6.5); //6.5
b = 10;
c = 3 ; 
d = max(b,c); //10
e1 = [a,b,c];//{ 6.5, 10, 3 }
f = max(e,[2,3.5,4]); //{ 6.5, 10, 4 }
g = max(e<2>,f<1>); //{ { 6.5, 10, 6.5 }, { 10, 10, 10 }, { 6.500, 10, 4 } }
e = max(-1,-2); //-1 , g = { { 2 }, { 3.5 }, { 4 } } and f = {2,3.5,4}
a1 = max(4/5,4.0/5.0);
//negative testing
a1 = ""s"";
b1 = max(a1,4); //null
b2 = max(a2,5); //null
b3 = max(null,5); //null
a4 = [];
b4 = max(a4,[1,2]); //null";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch742_80596min()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing min()
a = min(5,6); //5
b = 10.5;
c = 3.5 ; 
d = min(b,c); //3.5
e1 = [a,b,c];//{ 5, 10.500, 3.500 }
f = min(e,[2,3.5,4]); //{ 2, 3.500, 3.500 }
g = min(e<2>,f<1>); //{ { 2, 2, 2 }, { 3.500, 3.500, 3.500 }, { 3.500, 3.500, 3.500 } }
e = min(-1,-2); //-2 , g = { { -2 }, { -2 }, { -2 } } and f = { -2, -2, -2 }
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch743_80597rand()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing rand()
x = 1 ;
y = [1,3,5];
z = [8,7,6]; 
a = rand(x,10) ;
b = rand(2,10);
c = rand(9,9.5);//No method found matching the given argument(s) in global context
                 //Updated variable c = null*/
d = rand([1,2,3,4,5],10);
e1 = rand([1,2,3],[4,5,6]);
f = rand(y<1> ,z<2>);
//negative testing
a1 = ""s"";
b1 = rand(a1,3); //null
b2 = rand(5,a2); //null
b3 = rand(3,null); //null
a4 = [];
b4 = rand(a4,6); //null
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch744_80598sin()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"// testing using sin()
a = 90 ; 
b = sin(a);
c = a * 3 ;
d = sin(c);
e1 = [0,30,45,60,90];
f = sin(e);
/*
testBSplineCurve = BSplineCurve.ByPoint(points);
tControlVertices = testBSplineCurve.ControlVertices;
update the parameter
Ondemand: property will not be shown unless we query it. 
Gurantee: should never be null
Super class should be included
    Updated variable a = 90
    Updated variable b = 0.894
*/
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch745_80599someFalse()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing someFalse
w = false ;
x = [1,2,3,false];
y = [-1,-2,3,4];
z = [w,w,w,w];
a = someFalse(x);//true 
b = someFalse(y); //false
c = someFalse(z) ; //true ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch746_80600someNulls()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing someNulls 
w = null ;
x = [1,2,3,null];
y = [-1,-2,3,4];
z = [w,w,w,w];
a = someNulls(x);//false 
b = someNulls(y); //false
c = someNulls(z) ; //true ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch747_80601someTrue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing someTrue
w = true ;
x = [1,2,3,true];
y = [-1,-2,3,4];
z = [w,w,w,w];
a = someTrue(x);//true 
b = someTrue(y); //false
c = someTrue(z) ; //true ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch749_80603tan()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//testing tan()
a = 90 ; 
b = tan(a);
c = a * 3 ;
d = tan(c);
e = [0,30,45,60,90];
f = tan(e);
g = tan([180,360]); //{-0,-0}
h = tan(-90);
i = tan(10);
j = tan(135); //-1
k = tan(-45); //-1
l = tan(0); //0";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch759_T001_SomeNulls_IfElse_01()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result =
[Imperative]
{
	arr1 = [1,null];
	arr2 = [1,2];
	if(SomeNulls(arr1))
	{
		arr2 = arr1;
	}
	return = SomeNulls(arr2);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch760_T001_SomeNulls_IfElse_02()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result =
[Imperative]
{
	arr1 = [];
	arr2 = [1,2];
	if(SomeNulls(arr1))
	{
		arr2 = arr1;
	}
	return = SomeNulls(arr2);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch761_T002_SomeNulls_ForLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a = [1,3,5,7,[]];
	b = [null,null,true];
	c = [SomeNulls([1,null])];
	d = [a,b,c];
	j = 0;
	e = [];
	
	for(i in d)
	{
		
		e[j]= SomeNulls(i);
		j = j+1;
	}
	return  = e;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch762_T003_SomeNulls_WhileLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a = [1,3,5,7,[]];
	b = [null,null,true];
	c = [[]];
	
	d = [a,b,c];
	
	i = 0;
	j = 0;
	e = [];
	
	while(i<Count(d))
	{
	
		e[j]= SomeNulls(d[i]);
		i = i+1;
		j = j+1;
	}
	return = e ;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch763_T004_SomeNulls_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(x:var[]..[])
{
	a = [];
	i = 0;
	[Imperative]
	{
		for(j in x)
		{
			if(SomeNulls(j))
			{
				a[i] = j;
				i = i+1;
			}
		}
	}
	return  = Count(a);
}
b = [
[null],
[1,2,3,[]],
[0]
];
result = foo(b);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch765_T006_SomeNulls_Inline()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result;
[Imperative]
{
a = [null,1];
b = [];
c = [1,2,3];
result = SomeNulls(c)?SomeNulls(b):SomeNulls(a);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch766_T007_SomeNulls_RangeExpression()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result =
[Imperative]
{
i = 0;
arr = [[1,1.2] , [null,0], [true, false] ];
a1 = 0;
a2 = 2;
d = 1;
a = a1..a2..d;
for(i in a)
{
	if(SomeNulls(arr[i])) 
	return = i;
	
}
return = -1;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch767_T008_SomeNulls_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"/*
[Imerative]
{
	a = 1..5;
	i = 0..3;
	x = a[i];
}
*/
a = [
[[null, 1],1],
[null],
[1,2,false]
];
i = 0..2;
j = 0;
[Imperative]
{
		if(SomeNulls(a[i]))
		{
			j = j+1;
		}
		
} 
//Note : the following works fine : 
/*
[Imperative]
{
	for ( x in  i) 
	{		
	    if(SomeNulls(a[x]))
	    {
                j = j+1;
	    }
	}
}
*/";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch769_T010_SomeNulls_AssociativeImperative_01()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"m;
n;
[Imperative]
{
	a = [1,2,null];
	b = [null, null];
	
	[Associative]
	{
		a = [1];
		b = a;
		m = SomeNulls(b);
		a = [1,null,[]];
		n = m;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch770_T010_SomeNulls_AssociativeImperative_02()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{
	a = [false];
	if(!SomeNulls(a))
	{
	[Associative]
	{
		
		b = a;
		a = [null];
		
		m = SomeNulls(b);//true,false
		[Imperative]
		{
			c = a;
			a = [2];
			n = SomeNulls(c);//true
		}
		
	}
	}else
	{
	m = false;
	n = false;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch771_T010_SomeNulls_AssociativeImperative_03()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	
	a = [[]];
	b = a;
	
	m = SomeNulls(b);//false
	[Imperative]
	{
		c = a;
		a = [null,[]];
		m = SomeNulls(c);//false
	}
	a = [null];
	n = SomeNulls(b);//true;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch774_T012_CountTrue_IfElse()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result =
[Imperative]
{
	arr1 = [true,[[[[true]]]],null];
	arr2 = [[true],[false],null];
	if(CountTrue(arr1) > 1)
	{
		arr2 = arr1;
	}
	return = CountTrue(arr2);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch775_T013_CountTrue_ForLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a = [1,3,5,7,[]];
	b = [null,null,[1,true]];
	c = [CountTrue([1,null])];
	
	d = [a,b,c];
	j = 0;
	e = [];
	
	for(i in d)
	{
		e[j]= CountTrue(i);
		j = j+1;
	}
	return  = e;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch776_T014_CountTrue_WhileLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a = [1,3,5,7,[1]];//0
	b = [1,null,true];//1
	c = [[false]];//0
	
	d = [a,b,c];
	
	i = 0;
	j = 0;
	e = [];
	
	while(i<Count(d))
	{
		e[j]= CountTrue(d[i]);
		i = i+1;
		j = j+1;
	}
	return = e ;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch777_T015_CountTrue_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(x:var[]..[])
{
	a = [];
	i = 0;
	[Imperative]
	{
		for(j in x)
		{
			a[i] = CountTrue(j);
			i = i+1;
		}
	}
	return  = a;
}
b = [
[null],//0
[1,2,3,[true]],//1
[0],//0
[true, true,1,true, null],//3
[x, null]//0
];
result = foo(b);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch780_T018_CountTrue_RangeExpression_01()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
result = 
[Imperative]
{
	a1 = [1,true, null];//1
	a2 = 8;
	a3 = [2,[true,[true,1]],[false,x, true]];//3
	a = CountTrue(a1)..a2..CountTrue(a3);//{1,4,7}
	
	return = CountTrue(a);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch781_T018_CountTrue_RangeExpression_02()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a1 = [1,true, null];//1
	a2 = 8;
	a3 = [2,[true,[true,1]],[false,x, true]];//3
	a = CountTrue(a1)..a2..~CountTrue(a3);//{}
	return = CountTrue(a);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch782_T018_CountTrue_RangeExpression_03()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
result = 
[Imperative]
{
	a1 = [1,true, null];//1
	a2 = 8;
	a3 = [2,[true,[true,1]],[false,x, true]];//3
	a = [1.0,4.0,7.0];
	//a = CountTrue(a1)..a2..#CountTrue(a3);//{}
	return = CountTrue(a);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch783_T019_CountTrue_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def foo(x:int)
{
	return = x +1;
}
a = [true,[true],1];//2
b = [null];
c = [[[true]]];//1
d = [[true],[false,[true,true]]];//3
arr = [CountTrue(a),CountTrue(b),CountTrue(c),CountTrue(d)];
result = foo(arr);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch786_T022_CountTrue_ImperativeAssociative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
c;
[Imperative]
{
	a1 = [true,0,1,1.0,null];
	a2 = [false, CountTrue(a1),0.0];
	a3 = a1;
	[Associative]
	{
		a1 = [true,[true]];
		a4 = a2;
		a2 = [true];
		b = CountTrue(a4);//1
	}
	
	c = CountTrue(a3);//1
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch787_T023_CountFalse_IfElse()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result =
[Imperative]
{
	arr1 = [false,[[[[false]]]],null,0];
	arr2 = [[true],[false],null,null];
	if(CountFalse(arr1) > 1)
	{
		arr2 = arr1;
	}
	return = CountFalse(arr2);//2
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch788_T024_CountFalse_ForLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a = [1,3,5,7,[]];
	b = [null,null,[0,false]];
	c = [CountFalse([[false],null])];
	
	d = [a,b,c];
	j = 0;
	e = [];
	
	for(i in d)
	{
		e[j]= CountFalse(i);
		j = j+1;
	}
	return  = e;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch789_T025_CountFalse_WhileLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a = [1,3,5,7,[0]];//0
	b = [1,null,false];//1
	c = [[true]];//0
	
	d = [a,b,c];
	
	i = 0;
	j = 0;
	e = [];
	
	while(i<Count(d))
	{
		e[j]= CountFalse(d[i]);
		i = i+1;
		j = j+1;
	}
	return = e ;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch790_T026_CountFalse_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(x:var[]..[])
{
	a = [];
	i = 0;
	[Imperative]
	{
		for(j in x)
		{
			a[i] = CountFalse(j);
			i = i+1;
		}
	}
	return  = a;
}
b = [
[null],//0
[1,2,3,[false]],//1
[0],//0
[false, false,0,false, null],//3
[x, null]//0
];
result = foo(b);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch793_T029_CountFalse_RangeExpression_01()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
result = 
[Imperative]
{
	a1 = [0,false, null];//1
	a2 = 8;
	a3 = [2,[false,[false,1]],[false,x, true]];//3
	a = CountFalse(a1)..a2..CountFalse(a3);//{1,4,7}
	
	return = CountFalse(a);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch794_T029_CountFalse_RangeExpression_02()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
result = 
[Imperative]
{
	a1 = [1,false, null];//1
	a2 = 8;
	a3 = [2,[false,[false,1]],[false,x, true]];//3
	a = [1.0,4.0,7.0];
	//a = CountFalse(a1)..a2..#CountFalse(a3);//{}
	return = CountFalse(a);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch795_T030_CountFalse_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def foo(x:int)
{
	return = x +1;
}
a = [false,[false],0];//2
b = [CountFalse([a[2]])];
c = [[[false]]];//1
d = [[false],[false,[true,false,0]]];//3
arr = [CountFalse(a),CountFalse(b),CountFalse(c),CountFalse(d)];
result = foo(arr);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch798_T033_CountFalse_ImperativeAssociative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
c;
[Imperative]
{
	a1 = [false,0,1,1.0,null];
	a2 = [true, CountFalse(a1),0.0];
	a3 = a1;
	[Associative]
	{
		a1 = [false,[false]];
		a4 = a2;
		a2 = [false];
		b = CountFalse(a4);//1
	}
	
	c = CountFalse(a3);//1
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch799_T034_AllFalse_IfElse()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [false, false];//true
b = [[false]];//true
c = [false, 0];//false
result = [];
[Imperative]
{
	if(AllFalse(a)){
		a[2] = 0;
		result[0] = AllFalse(a);//false
	} 
	if(!AllFalse(b)){
		
		result[1] = AllFalse(b);//false
	}else
	{result[1]= null;}
	if(!AllFalse(c)){
		result[2] = AllFalse(c);
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch800_T035_AllFalse_ForLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
result = 
[Imperative]
{
	a = [false,false0,0,null,x];//false
	b = [false,false0,x];//false
	c = [];//false
	d = [[]];//false
	
	h = [
	[[0]],
	[false]
];
	e = [a,b ,c ,d,h];
	f = [];
	j = 0;
	for(i in e)
	{	
		if(AllFalse(i)!=true){
			f[j] = AllFalse(i);
			j = j+1;
		}
		
	}
return = f;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch801_T036_1_Null_Check()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = null;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch802_T036_AllFalse_WhileLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
result = 
[Imperative]
{
	a = [false,false0,0,null,x];//false
	b = [false,false0,x];//false
	c = [];//false
	d = [[]];//false
	e = [a,b ,c ,d];
	i = 0;
	f = [];
	j = 0;
	while(!AllFalse(e[i])&& i < Count(e))
	{	
		if(AllFalse(e[i])!=true){
			f[j] = AllFalse(e[i]);
			j = j+1;
		}
		i = i+1;
		
	}
return = f;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch803_T037_AllFalse_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo( x : bool)
{	
	return = !x;
}
a1 = [0];
a2 = [null];
a3 = [!true];
b = [a1,a2,a3];
result = [foo(AllFalse(a1)),foo(AllFalse(a2)),foo(AllFalse(a3))];//true,true,false
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch805_T039_AllFalse_Inline()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1 = [false,[false]];
a = AllFalse(a1);//true
b1 = [null,null];
b = AllFalse(b1);//false
c = AllFalse([b]);//t
result = a? c:b;//t";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch807_T040_AllFalse_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [
	[[0]],
	[false]
];
c = AllFalse(a);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch808_T042_AllFalse_DynamicArray()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b = [];
a = [[true],[false],[false],
	[false,[true,false]]];
	
	i = 0;
	result2 = 
	[Imperative]
	{
		while(i<Count(a))
		{
			b[i] = AllFalse(a[i]);
			i = i+1;
		}
		return = b;
	}
	result = AllFalse(a);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch810_T044_AllFalse_ImperativeAssociative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"m;
n;
[Imperative]
{
	a = [false||true];
	b = [""false""];
	c = a;
	a = [false];
	[Associative]
	{
		
		d = b;
		
		b = [false];
		
		m = AllFalse(c);//f
		n = AllFalse(d);//t
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch811_T045_Defect_CountArray_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [0,1,null];
b = [m,[],a];
c=[];
c[0] = 1;
c[1] = true;
c[2] = 0;
c[3] = 0;
a1 = Count(a);
b1 = Count(b);
c1 = Count(c);
result = [a1,b1,c1];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch812_T045_Defect_CountArray_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result=
[Imperative]
{
a = [];
b = a;
a[0] = b;
a[1] = ""true"";
c = Count(a);
a[2] = c;
return = Count(a);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch813_T045_Defect_CountArray_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [];
b = [null,1+2];
a[0] = b;
a[1] = b[1];
result = Count(a);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch814_T046_Sum_IfElse()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a = [1,2,3,4];
	b = [1.0,2.0,3.0,4.0];
	c = [1.0,2,3,4.0];
	d = [];
	e = [[1,2,3,4]];
	f = [true,1,2,3,4];
	g = [null];
	
	m= [-1,-1,-1,-1,-1,-1,-1];
	
	if(Sum(a)>=0) m[0] = Sum(a);	
	if(Sum(b)>=0) m[1] = Sum(b);
	if(Sum(c)>=0) m[2] = Sum(c);
	if(Sum(d)>=0) m[3] = Sum(d); 
	if(Sum(e)>=0) m[4] = Sum(e);
	if(Sum(f)>=0) m[5] = Sum(f);
	if(Sum(g)>=0) m[6] = Sum(g);
	
	return = m;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch815_T047_Sum_ForLoop()
        {
            string defectID = "MAGN-3988 Defects with Expression Interpreter Test Framework";
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import(""FFITarget.dll"");
result = 
[Imperative]
{
	a = [0,0.0];
	b = [[]];
	c = [m, DummyMath.Sum(a), b, 10.0];
	
	d = [a,b,c];
	j = 0;
	
	for(i in d)
	{
		d[j] = DummyMath.Sum(i);
		j = j+1;
	}
	
	return = d; 
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map, defectID: defectID);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch816_T048_Sum_WhileLoop()
        {
            string defectID = "MAGN-3988 Defects with Expression Interpreter Test Framework";
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import(""FFITarget.dll"");
result = 
[Imperative]
{
	a = [-2,0.0];
	b = [[]];
	c = [m, DummyMath.Sum(a), b, 10.0];
	
	d = [a,b,c];
	j = 0;
	k = 0;
	e = [];
	
	while(j<Count(d))
	{
		if(DummyMath.Sum(d[j])!=0)
		{
			e[k] = DummyMath.Sum(d[j]);
			k = k+1;
		}
		j = j+1;
	}
	
	return = e; 
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map, defectID: defectID);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch817_T049_Sum_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(x:var[])
{
	return =
	[Imperative]
	{
		return = Sum(x);
	}
}
a = [-0.1,true,[],null,1];
b = [m+n,[[[1]]]];
c = [Sum(a),Sum(b)];
result = foo(c);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch819_T051_Sum_Inline()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1,[2,-3.00]];//0.0
sum = Sum(a);
b = Sum(a) -1;//-1.0
c = Sum([a,b,-1]);//-2.0;
result = Sum(a)==0&& b==-1.00? b :c;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch820_T052_Sum_RangeExpression()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a1 = [1,true, null];//1
	a2 = 8;
	a3 = [2,[true,[true,1.0]],[false,x, true]];//3.0
	a = Sum(a1)..a2..Sum(a3);//{1,4,7}
	
	return = Sum(a);//12.0
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch821_T053_Sum_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1,2,3];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch822_T054_Sum_DynamicArr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [];
b = [1.0,2,3.0];
c = [null,m,""1""];
a[0]=Sum(b);//6.0
a[1] = Sum(c);//0
a[2] = Sum([a[0],a[1]]);//6.0
result = Sum(a);//12.0";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch824_T056_Sum_AssociativeImperative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1,0,0.0];
b = [2.0,0,true];
b1 = [b,1];
[Imperative]
{
	c = a[2];
	a[1] = 1;
	m = a;
	sum1 = Sum([c]);//0.0
	[Associative]
	{
		 b[1] = 1;
		 sum2 = Sum( b1);////4.0
	}
	
	a[2]  =1;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch825_T057_Average_DataType_01()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [];
b = [1,2,3];
c = [0.1,0.2,0.3,1];
d = [true, false, 1];
a1 = Average(a);
b1 = Average(b);
c1 = Average(c);
d1 = Average(d);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch827_T059_Defect_Flatten_RangeExpression()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0..10..5;
b = 20..30..2;
c = [a, b];
d = Flatten([a,b]);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch828_T059_Defect_Flatten_RangeExpression_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [[null]];
b = [1,2,[3]];
c = [a,b];
d = Flatten(c);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch829_T060_Average_ForLoop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result = 
[Imperative]
{
	a = [];
	b = [1,[2],[[2],1]];
	c = [true, false, null, 10];
	d = [a,b,c];
	
	e = [];
	j = 0;
	
	for(i in d)
	{
		e[j] = Average(i);
		 j = j+1;
		
	}
	return = e;
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch830_T061_Average_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : double (x :var[]..[])
{
	
	return = Average(x);
}
a = [1,2,2,1];
b = [1,[]];
c = Average(a);
result = [foo(a),foo(b)];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch832_T063_Average_Inline()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1.0,2];
b = [[0],1.0,[2]];
result = Average(a)>Average(b)?true:false;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch833_T064_Average_RangeExpression()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0..6..3;//0,3,6
b = 0..10..~3;//0,3.3,6.6,10
m = Average(a);//3
n = Average(b);//5.0
c = Average([m])..Average([n]);//3.0,4.0,5.0";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch835_T066_Print_String()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"r1 = Print(""Hello World"");
str = ""Hello World!!"";
r2 = Print(str);
str = ""Hello + World"";";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch836_T067_Print_Arr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"arr = [ 0, 1 ,2];
r1 = Print(arr);
arr2 = [0,[1],[[]]];
r2 = Print(arr2);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch848_test()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [];
b = Average(a);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch927_T53_Undefined_Class_negative_1467107_10()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(x:int)
{
return = x + 1;
}
//y1 = test.foo(2);
m=null;
y2 = m.foo(2);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch928_T53_Undefined_Class_negative_associative_1467091_13()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : int)
{
return = x + 1;
}
y = test.foo (1);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch960_T77_Defect_1460274_Class_Update()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b = 1;
a = b + 1;
b = a;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch963_T77_Defect_1460274_Class_Update_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Imperative]
{
	a = [];
	b = a;
	a[0] = b;
	c = Count(a);
}
[Associative]
{
	a1 = [0];
	b1 = a1;
	a1[0] = b1;
	c1 = Count(a1);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch976_Collection_Assignment_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
d;
e;
[Imperative]
{
	a = [ [1,2], [3,4] ];
	
	a[1] = [-1,-2,3];
	
	c = a[1][1];
	
	d = a[0];
	
	b = [ 1, 2 ];
	
	b[0] = [2,2];
	e = b[0];
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch977_Collection_Assignment_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo: int[]( a: int,b: int )
{
	return = [ a,b ];
}
	c = foo( 1, 2 );
d;	
[Imperative]
{
	d = foo( 3 , -4 );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch978_Collection_Assignment_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo: int[]( a: int,b: int )
{
	return = [ a+1,b-2 ];
}
	c = foo( 1, 2 );
	d;
[Imperative]
{
	d = foo( 2+1 , -3-1 );
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch980_Collection_Assignment_5()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo: int[] ( a : int[], b: int, c:int )
{
	a[b] = c;
	return = a;
}
d = [ 1,2,2 ];
b = foo( d,2,3 );
e;
c;
[Imperative]
{
	e = [ -2,1,2 ];
	c = foo( e,0,0 );
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch981_T01_Simple_1D_Collection_Assignment()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
c;
d;
e;
[Imperative]
{
	a = [ [1,2], [3,4] ];
	
	a[1] = [-1,-2,3];
	
	c = a[1][1];
	
	d = a[0];
	
	b = [ 1, 2 ];
	
	b[0] = [2,2];
	e = b[0];
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch982_T02_Collection_Assignment_Associative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
b;
c;
d;
e;
[Associative]
{
	a = [ [1,2], [3,4] ];
	
	a[1] = [-1,-2,3];
	
	c = a[1][1];
	
	d = a[0];
	
	b = [ 1, 2 ];
	
	b[0] = [2,2];
	e = b[0];
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch983_T03_Collection_Assignment_Nested_Block()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
d;
e;
[Associative]
{
	a = [ [1,2,3],[4,5,6] ];
	
	[Imperative]
	{
		c = a[0];
		d = a[1][2];
	}
	
	e = c;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch984_T04_Collection_Assignment_Using_Indexed_Values()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
d;
e;
[Associative]
{
	a = [ [1,2,3],[4,5,6] ];
	
	b = [ a[0], 4 ];
	
	c = b[0];
	
	d = b[1];
	
	e = [ a[0][0], a[0][1], a[1][0] ];
	
}
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch987_T07_Collection_Assignment_In_Function_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def collection :int[] ( a :int[] , b:int , c:int )
{
	a[1] = b;
	a[2] = c;
	return= a;
}
	a = [ 1,0,0 ];
	[Imperative]
	{
		a = collection( a, 2, 3 );
	}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch988_T08_Collection_Assignment_In_Function_Scope_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a )
{
	return= a;
}
	a = [ 1, foo( 2 ) , 3 ];
	b;
	[Imperative]
	{
		b = [ foo( 4 ), 5, 6 ];
	}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch990_T10_2D_Collection_Assignment_In_Function_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"	def foo( a:int[] )
	{
		a[0][0] = 1;
		return= a;
	}
	b = [ [0,2,3], [4,5,6] ];
	d = foo( b );
	c = d[0];
		
		
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch991_T11_2D_Collection_Assignment_Heterogeneous()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
c;
d;
[Imperative]
{
	a = [ [1,2,3], [4], [5,6] ];
	b = a[1];
	a[1] = 2;
	a[1] = a[1] + 1;
	a[2] = [7,8];
	c = a[1];
	d = a[2][1];
}	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch992_T12_Collection_Assignment_Block_Return_Statement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c1;
c2;
[Associative]
{
	a = 3;
	
	b = [Imperative]
	{
		c = [ 1,2,3 ];
		if( c[1] <= 3 )
		return= c;
	}
	
	b[2] = 4;
	a = b;
	c1 = a[1];
	c2 = a[2];
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch993_T13_2D_Collection_Assignment_Block_Return_Statement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c1;
c2;
[Associative]
{
	a = 3;
	
	b = [Imperative]
	{
		c = [ [ 1,2,3 ] , [ 4,5,6 ] ] ;
		return= c;
	}
	
	b[0][0] = 0;
	a = b;
	c1 = a[0];
	c2 = a[1][2];
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch994_T14_2D_Collection_Assignment_Using_For_Loop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"pts = [[0,1,2],[0,1,2]];
x = [1,2];
y = [1,2,3];
[Imperative]
{
    c1 = 0;
	for ( i in x )
	{
		c2 = 0;
		for ( j in y )
		{
		    pts[c1][c2] = i+j;
			c2 = c2+1;
		}
		c1 = c1 + 1;
	}
	
}
p1 = pts[1][1];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch995_T15_2D_Collection_Assignment_Using_While_Loop()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"p1;
[Imperative]
{
	pts = [[0,1,2],[0,1,2]];
	x = [1,2,3];
	y = [1,2,3];
    i = 0;
	while ( i < 2 )
	{		
		j = 0;
		while ( j  < 3 )
		{
		    pts[i][j] = i+j;
			j = j + 1;
		}
		i = i + 1;
	}
	p1 = pts[1][1];
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch997_T17_Assigning_Collection_And_Updating()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1, 2, 3];
b = a;
b[0] = 100;
t = a[0];       // t = 100, as expected
      
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch998_T18_Assigning_Collection_In_Function_And_Updating()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def A (a: int [])
{
    return = a;
}
val = [1,2,3];
b = A(val);
t = b;
t[0] = 100;    // 
y = b[0];
z = val[0];    // val[0] is still 1
      
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch999_T19_Assigning_Collection_In_Function_And_Updating()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def A (a: int [])
{
    return = a;
}
val = [1,2,3];
b = A(val);
b[0] = 100;     
z = val[0];     
      
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1000_T20_Defect_1458567()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
b = a[1];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1002_T21_Defect_1460891()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
c;
[Imperative]
{
    b = [ ];
    count = 0;
    a = 1..5..2;
    for ( i in a )
    {
        b[count] = i + 1;
        count = count + 1;
    }
	c = b ;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1003_T21_Defect_1460891_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def CreateArray ( x : var[] , i )
{
    x[i] = i;
	return = x;
}
b = [0, 1];
count = 0..1;
b = CreateArray ( b, count );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1004_T22_Create_Multi_Dim_Dynamic_Array()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"test;
[Imperative]
{
    d = [[]];
    r = c = 0;
    a = [ 0, 1, 2 ];
	b = [ 3, 4, 5 ];
    for ( i in a )
    {
        c = 0;
		for ( j in b)
		{
		    d[r][c] = i + j;
			c = c + 1;
        }
		r = r + 1;
    }
	test = d;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1007_T24_Dynamic_Array_Argument_Function_1465802_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def foo : int(i:int[])
	{
		return = 1;
	}
b1;
[Associative]
{
cy=[];
cy[0]=10;
cy[1]=12;
b1=foo(cy);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1008_T24_Dynamic_Array_Argument_Function_1465802_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def foo : int(i:int[])
	{
		return = 1;
	}
b1;
[Associative]
{
cy=[];
cy[0]=10;
cy[1]=null;
b1=foo(cy);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1012_T24_Dynamic_Array_Imperative_Function_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def createArray( p : int[] )
{  
    a = [Imperative]  
    {    
        collection = [];	
	lineCnt = 0;
	while ( lineCnt < 2 )
	{
            collection [ lineCnt ] = p [ lineCnt ] * -1;
	    lineCnt = lineCnt + 1;      
	}
	return = collection;
    }
    return = a;
}
x = createArray ( [ 1, 2, 3, 4 ] );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1013_T24_Dynamic_Array_Imperative_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"t = [Imperative]
{
    d = [ [ ] ];
    r = c = 0;
    a = [ 0, 1, 2 ];
    b = [ 3, 4, 5 ];
    for ( i in a )
    {
        c = 0;
	for ( j in b)
	{
	    d[r][c] = i + j;
	    c = c + 1;
        }
	r = r + 1;
    }
    test = d;
    return = test;
}
// expected : test = { { 3, 4, 5 }, {4, 5, 6}, {5, 6, 7} }
// received : test = { { 3, 4, 5 }, , }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1014_T24_Dynamic_Array_Inside_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def foo ( d : var[] )
{
    [Imperative]
    {
	r = c = 0;
	a = [ 0, 1, 2 ];
	b1 = [ 3, 4, 5 ];
	for ( i in a )
	{
	    c = 0;
	    for ( j in b1)
	    {
		d[r][c] = i + j;
		c = c + 1;
	    }
	    r = r + 1;
	}	
    }
    return = d;
}
b = [];
b = foo ( b ) ;     
a = b;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1015_T24_Dynamic_Array_Inside_Function_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def foo ( d : var[]..[] )
{
    [Imperative]
    {
	r = c = 0;
	a = [ 0, 1, 2 ];
	b1 = [ 3, 4, 5 ];
	for ( i in a )
	{
	    c = 0;
	    for ( j in b1)
	    {
		d[r][c] = i + j;
		c = c + 1;
	    }
	    r = r + 1;
	}	
    }
    return = d;
}
b = [ [] ];
b = foo ( b ) ;     
a = b;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1028_T25_Adding_Elements_To_Array()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0..2;
a[3] = 3;
b = a;
x = [ [ 0, 0 ] , [ 1, 1 ] ];
x[1][2] = 1;
x[2] = [2,2,2,2];
y = x;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1030_T25_Adding_Elements_To_Array_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def add ( x : var[]..[] ) 
{
    x[1][2] = 1;
    x[2] = [ 2, 2, 2, 2 ];
    return = x;
}
x = [ [ 0, 0 ] , [ 1, 1 ] ];
x = add(x);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1032_T26_Defct_DNL_1459616()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=1;
a=[a,2];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1033_T26_Defct_DNL_1459616_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=[1,2];
[Imperative]
{
    a=[a,2];
}
b = a;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1034_T26_Defct_DNL_1459616_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=[1,2];
[Imperative]
{
    a=[a,2];
}
b = [ 1, 2 ];
def foo ( )
{
    b =  [ b[1], b[1] ];
    return = null;
}
dummy = foo ();
c = b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1037_T26_defect_1464429_DynamicArray()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
b = [ ]; // Note : b = { 0, 0} works fine
count = 0..1;
t2 = CreateArray ( b, count );
t1=b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1039_T27_defect_1464429_DynamicArray()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
b = [ ]; // Note : b = { 0, 0} works fine
count = 0..1;
t2 = CreateArray ( b, count );
t1=b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1044_T27_defect_1464429_DynamicArray_update()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
b = [ ]; // Note : b = { 0, 0} works fine
count = 0..1;
t2 = CreateArray ( b, count );
t1=b;
count = -2..-1;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1048_T27_DynamicArray_Invalid_Index_1465614_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=[];
b=a[2];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1055_T29_DynamicArray_Using_Out_Of_Bound_Indices()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"   
    basePoint = [  ];
    
    basePoint [ 4 ] =3;
    test = basePoint;
    
    a = basePoint[0] + 1;
    b = basePoint[ 4] + 1;
    c = basePoint [ 8 ] + 1;
    
    d = [ 0,1 ];
    e1 = d [ 8] + 1;
    
    x = [ ];
    y = [ ];    
    t = [Imperative]
    {
        k = [ ];
	for ( i in 0..1 )
	{
	    x[i] = i;
	}
	k[0] = 0;
	for ( i in x )
	{
	    y[i] = x[i] + x[i+1];
	    k[i+1] = x[i] + x[i+1];
	
	}
	return = k;
    }
    z = y;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1060_T40_Index_usingFunction_1467064()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo()
{    
return = 0;
}
x = [ 1, 2 ];
x[foo()] = 3;
y = x;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1071_T62_Change_Avariable_To_Dynamic_Array_OnTheFly()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def func(a:int)
{
a=5;
return = a;
}
c=1;
b= func(c[0]);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1072_T62_Create_Dynamic_Array_OnTheFly()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"i;
b;
z=[Imperative]
{
for (i in 0..7)
{
b[i] = i;
}
return=b;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1073_T62_Create_Dynamic_Array_OnTheFly_AsFunctionArgument()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def func(a:int)
{
a=5;
return = a;
}
b= func(c[0]);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1077_T62_Create_Dynamic_Array_OnTheFly_passargument_function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	
  def test(a:int[])
	{
	b=1;
	return=b;
	}
d[0]=5;
a=test(d);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1078_T63_Create_MultiDimension_Dynamic_Array_OnTheFly()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def func(a:int[]..[])
{
a[0][1]=5;
a[2][3]=6;
return = a;
}
c=1;
b= func(c);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1079_T63_Dynamic_array_onthefly_1467066()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
z=[Imperative]
{
b[5]=0;
for (i in 0..7)
{
b[i] = i;
}
return=b;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1080_T63_Dynamic_array_onthefly_aliastoanother()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=5;
b=a;
a[2]=3;
b[2]=-5;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1082_T63_Dynamic_array_onthefly_function_1467139()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(a:int[])
{
}
x[0]=5;
a = foo(x);
c = [100];
t = x;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1084_T63_Dynamic_array_onthefly_update()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z=true;
b=z;
z[0]=[1];
z=5;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1085_T64_Modify_itemInAnArray_1467093()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1, 2, 3];
a[1] = a; 
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3988
            string defectID = "MAGN-3988 Defects with Expression Interpreter Test Framework";

            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map, defectID: defectID);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1086_T64_Modify_itemInAnArray_1467093_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;
[Imperative]
{
a = [];
b = a;
a[0] = b;
//hangs here
c = a;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1087_T65_Array_Alias_ByVal_1467165()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [0,1,2,3];
b=a;
a[0]=9;
b[0]=10;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1089_T65_Array_Alias_ByVal_1467165_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [0,1,2,3];
b=a;
a[0]=9;
b[0]=false;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1092_T65_Array_Alias_ByVal_1467165_6()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [0,1,2,3];
b=a;
a[0]=null;
b[0]=false;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1093_T66_Array_CannotBeUsedToIndex1467069()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3988
            string defectID = "MAGN-3988 Defects with Expression Interpreter Test Framework";

            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
    a = [3,1,2]; 
    x = [10,11,12,13,14,15]; 
    x[a] = 2;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map, defectID: defectID);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1095_T67_Array_Remove_Item()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=[1,2,3,4,5,6,7];
a=Remove(a,0);// expected :{2,3,4,5,6,7}
a=Remove(a,4);//expected {1,2,3,4,6,7}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1096_T67_Array_Remove_Item_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=[1,2,3,4,5,6,7];
a=Remove(a,0);// expected :{2,3,4,5,6,7}
a=Insert(a,4,6);//expected {1,2,3,4,6,7}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1097_T68_copy_byreference_1467105()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"test;
[Associative]
{
a = [ 1, 2, 3];
b = a;
b[0] = 10;
test = a[0]; //= 10� i.e. a change in �b� causes a change to �a�
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1098_T01_Function_In_Assoc_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Associative]
{
    def foo : int( a:int )
    {
	   return = a * 10;
	}
	
    a = foo( 2 );
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1101_T04_Function_In_Nested_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Associative]
{
    def foo : int( a:int, b : int )
    {
	   return = a * b;
	}
	a = 3.5;
	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		a = foo( 2, 1 );
		return = a;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1102_T05_Function_outside_Any_Block()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int( a:int, b : int )
{
    return = a * b;
}
a = 3.5;
b = 3.5;
[Associative]
{
	a = 3.5;
	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		a = foo( 2, 1 );
		return = a;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1103_T06_Function_Imp_Inside_Assoc()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Associative]
{
	def foo : int( a:int, b : int )
	{
		return = a * b;
	}
	a = 3.5;
	b = 3.5;
	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		c = foo( 2, 1 );
		return = c;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1111_T12_Function_From_Inside_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def add_1 : double( a:double )
{
	return = a + 1;
}
a;b;
[Associative]
{
	def add_2 : double( a:double )
	{
		return = add_1( a ) + 1;
	}
	
	a = 1.5;
	b = add_2 (a );
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1116_T17_Function_From_Parallel_Blocks()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Associative]
{
	def foo : int( n : int )
	{
		return = n * n;	
	}
	
	
	
}
[Associative]
{
	a = 3;
	b = foo (a );
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1119_T20_Function_From_Imperative_If_Block()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;y;z;
[Associative]
{
	def foo : int( n : int )
	{
		return = n * n;	
	}
	
	[Imperative]
	{
	
		a = [ 0, 1, 2, 3, 4, 5 ];
		x = 0;
		for ( i in a )
		{
			x = x + foo ( i );
		}
		
		y = 0;
		j = 0;
		while ( a[j] <= 4 )
		{
			y = y + foo ( a[j] );
			j = j + 1;
		}
		
		z = 0;
		
		if( x == 55 )
		{
		    x = foo (x);
		}
		
		if ( x == 50 )
		{
		    x = 2;
		}
		elseif ( y == 30 )
		{
		    y = foo ( y );
		}
		
		if ( x == 50 )
		{
		    x = 2;
		}
		elseif ( y == 35 )
		{
		    x = 3; 
		}
		else
		{
		    z = foo (5);
		}
	}
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1122_T23_Function_Call_As_Function_Call_Arguments()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c1;
[Associative]
{
	def foo : double ( a : double , b :double )
	{
		return = a + b ;	
	}
	
	def foo2 : double ( a : double , b :double )
	{
		return = foo ( a , b ) + foo ( a, b );	
	}
	
	a1 = 2;
	b1 = 4;
	c1 = foo2( foo (a1, b1 ), foo ( a1, foo (a1, b1 ) ) );
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1133_T34_Function_With_Mismatching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b1;
[Associative]
{ 
	 def foo1 : double ( a : double )
	 {
	    return = true;
	 }
	 
	 dummyArg = 1.5;
	
	b1 = foo1 ( dummyArg );
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1134_T35_Function_With_Mismatching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b2;
[Associative]
{ 
	 def foo2 : double ( a : double )
	 {
	    return = 5;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo2 ( dummyArg );
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1135_T36_Function_With_Mismatching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b2;
[Associative]
{ 
	 def foo3 : int ( a : double )
	 {
	    return = 5.5;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1137_T38_Function_With_Mismatching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b2;
[Associative]
{ 
	 def foo3 : int[] ( a : double )
	 {
	    return = a;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1138_T39_Function_With_Mismatching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b2;
[Associative]
{ 
	 def foo3 : int ( a : double )
	 {
	    return = [1, 2];
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1139_T40_Function_With_Mismatching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{ 
	 def foo3 : int[][] ( a : double )
	 {
	    return = [ [2.5], [3.5]];
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1140_T41_Function_With_Mismatching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{ 
	 def foo3 : int[][] ( a : double )
	 {
	    return = [ [2.5], 3];
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1141_T42_Function_With_Mismatching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b2;
[Associative]
{ 
	 def foo3 : bool[]..[] ( a : double )
	 {
	    return = [ [2], 3];
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1142_T43_Function_With_Matching_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b2;
[Associative]
{ 
	 def foo3 : int[]..[] ( a : double )
	 {
	    return = [ [ 0, 2 ], [ 1 ] ];
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg )[0][0];	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1144_T45_Function_With_Mismatching_Argument_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b2;
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
	 b2 = foo ( 1 );	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1146_T47_Function_With_Mismatching_Argument_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
	 b2 = foo ( true);	
	 c = 3;	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1149_T50_Function_With_Mismatching_Argument_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Associative]
{ 
	 def foo : double ( a : int[] )
	 {
	    return = 1.5;
     }
	 aa = [ ];
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1150_T51_Function_With_Mismatching_Argument_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Associative]
{ 
	 def foo : double ( a : double[] )
	 {
	    return = 1.5;
     }
	 aa = [1, 2 ];
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1151_T52_Function_With_Mismatching_Argument_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Associative]
{ 
	 def foo : double ( a : double[] )
	 {
	    return = 1.5;
     }
	 aa = 1.5;
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1152_T53_Function_Updating_Argument_Values()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aa;b2;c;
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    a = 4.5;
		return = a;
     }
	 aa = 1.5;
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1155_T56_Function_Updating_Argument_Values()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aa;bb;c;
[Associative]
{ 
	 def foo : int[] ( a : int[] )
	 {
	    a[0] = 0;
		return = a;
     }
	 aa = [ 1, 2 ];
	 bb = foo ( aa );	
	 
	 c = 3;	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1156_T57_Function_Using_Local_Var_As_Same_Name_As_Arg()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aa;bb;c;
[Associative]
{ 
	 def foo : int ( a : int )
	 {
	    a = 3;
		b = a + 1;
		return = b;
     }
	 
	 aa = 1;
	 bb = foo ( aa );
     c = 3;	 
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1157_T58_Function_With_No_Argument()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c1;c2;
     def foo : int (  )
	 {
	    return = 3;
     }
	 
	 [Associative]
	 { 
		c1 = foo();	
     }
	 
	 [Imperative]
	 { 
		c2 = foo();	
     }
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1158_T59_Function_With_No_Return_Stmt()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
     def foo : int ( a : int )
	 {
	    b = a + 1;
     }
	 
	 [Associative]
	 { 
		c = foo(1);	
     }
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1159_T60_Function_With_No_Return_Stmt()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
     def foo : int ( a : int )
	 {
	    b = a + 1;
     }
	 
	 [Imperative]
	 { 
		c = foo(1);	
     }
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1173_T74_Function_With_Simple_Replication_Associative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;y;
[Associative]
{
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	x = [ 1, 2, 3 ];
	y = foo(x);
	
}
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1175_T75_Function_With_Replication_In_Two_Args()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"y;
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = [ 1, 2, 3 ];
	x2 = [ 1, 2, 3 ];
	
	y = foo ( x1, x2 );
	
}
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1176_T76_Function_With_Replication_In_One_Arg()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"y;
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = [ 1, 2, 3 ];
	x2 = 1;
	
	y = foo ( x1, x2 );
	
}
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1177_T77_Function_With_Simple_Replication_Guide()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1;a2;a3;a4;
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = [ 1, 2 ];
	x2 = [ 1, 2 ];
	y = foo( x1<1> , x2<2> );
	a1 = y[0][0];
	a2 = y[0][1];
	a3 = y[1][0];
	a4 = y[1][1];
	
}
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1178_T78_Function_call_By_Reference()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;d;
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		a = a + b;
		b = 2;
		return  = a + b;
	}
	
	a = 1;
	b = 2;
	c = foo (a, b );
	d = a + b;
	
}
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1180_T80_Function_call_By_Reference()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
    def foo : int ( a : int, b : int )
	{
		c = [Imperative]
		{
		    d = 0;
			if( a > b )
				d = 1;
			return = d;	
		}
		a = a + c;
		b = b + c;
		return  = a + b;
	}
	
	a = 2;
	b = 1;
	c = foo (a, b );
	d = a + b;
	
}
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1181_T81_Function_Calling_Imp_From_Assoc()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
[Associative]
{
    def foo : int ( a : int )
	{
		c = [Imperative]
		{
		    d = 0;
			if( a > 1 )
			{
				d = 1;
			}
			return = d;	
		}
		return  = a + c;
	}
	
	a = 2;
	b = foo (a );	
}
	 
	 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1183_T83_Function_With_Null_Arg()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
def foo:int ( a : int )
{	
	return = a;
}
[Associative]
{
	b = foo( null );
}
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1187_T87_Function_Returning_From_Imp_Block_Inside_Assoc()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int( a : int)
{
  temp = [Imperative]
  {
      if ( a == 0 )
      {
          return = 0; 
      }
    return = a ;
  } 
  return = temp;
}
x = foo( 0 );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1188_T88_Function_With_Collection_Argument()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : double (arr : double[])
{
    return = 0;
}
arr = [1,2,3,4];
sum = foo(arr);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1189_T89_Function_With_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : double (arr1 : double[], arr2 : double[] )
{
    return  = arr1[0] + arr2[0];
}
arr = [  [2.5,3], [1.5,2] ];
two = foo (arr, arr);
t1 = two[0];
t2 = two[1];
//arr1 = {2.5,3};
//arr2 = {1.5,2};
//two = foo(arr1, arr2);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1191_T91_Function_With_Default_Arg()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:double(x:int = 2, y:double = 5.0)
{
	return = x + y;
}
a = foo();
b = foo(1, 3.0);
c = foo();
d;e;f;
[Imperative]
{
	d = foo();
	e = foo(1, 3.0);
	f = foo();
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1192_T92_Function_With_Default_Arg_Overload()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:double()
{
	return = 0.0;
}
def foo:double(x : int = 1, y : double = 2.0)
{
	return = x + y;
}
a = foo();
b = foo(3);
c = foo(3.4); // not found, null
d = foo(3, 4.0);
e = foo(1, 2.0, 3); // not found, null
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1204_TV101_Indexing_IntoArray_InFunctionCall_1463234()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo()
{
return = [1,2];
}
t = foo()[0];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1206_TV101_Indexing_Intoemptyarray_InFunctionCall_1463234_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo()
{
return = [];
}
t = foo()[0];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1207_TV101_Indexing_IntoNested_FunctionCall_1463234_5()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def foo()
	{
		return = [foo2()[0],foo2()[1]];
	}
def foo2()
{
return = [1,2];
}
a=test.test()[0];
t = foo()[0];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1208_TV101_Indexing_Intosingle_InFunctionCall_1463234_()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo()
{
return = [1,2];
}
t = foo()[0];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1209_TV101_Indexing_Intosingle_InFunctionCall_1463234_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo()
{
return = [1];
}
t = foo()[0];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1210_TV101_Indexing_Intovariablenotarray_InFunctionCall_1463234_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo()
{
return = 1;
}
t = foo()[0];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1220_TV12_Function_With_Argument_Update_Associative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"e;f;
[Associative]
{
	def update:int( a:int, b:int )
	{
		a = a + 1;
		b = b + 1;
		return = a + b;
	}
	
	c = 5;
	d = 5;
	e = update(c,d);
	e = c;
	f = d;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1222_TV14_Empty_Functions_Associative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
[Associative]
{
	def foo:int ( a : int )
	{
	}
	
	b = foo( 1 );
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1224_TV16_Function_With_No_Return_Statement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
	def foo : int( a : int )
	{
		a = a + 1;
	}
	[Imperative]
	{
		b = foo( 1 );
	}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1225_TV17_Function_Access_Local_Variables_Outside()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def foo: int ( a : int )
	{
		b = a + 1;
		c = a * 2;
		return = a;
	}
e;f;
[Imperative]
{	
	d = foo( 1 );
	e = b;
	f = c;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1226_TV18_Function_Access_Global_Variables_Inside()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"	global = 5;
	global2 = 6;
	d;
	
	def foo: int ( a : int )
	{
		b = a + global;
		c = a * 2 * global2;
		return = b + c;
	}
[Imperative]
{	
	d = foo( 1 );
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1227_TV19_Function_Modify_Global_Variables_Inside()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"	global = 5;
	e;
	
	def foo: int ( a : int )
	{
		global = a + global;
		
		return = a;
	}
[Imperative]
{	
	d = foo( 1 );
	e = global;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1231_TV23_Defect_1455152()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
d;
def foo : int ( a : int )
{
    b = a + 1;
}	 
[Associative]
{
     c = foo(1);
}
[Imperative]
{
     d = foo(1);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1232_TV24_Defect_1454958()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo: int( a : int , b : int)
{
	return = a + b;
}
b;c;
[Associative]
{
	b = Foo( 1,2 );
}
[Imperative]
{
	c = foo( 1 );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1234_TV26_Defect_1454923_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def function1: int ( a : int, b : int )
{
	return = -1 * (a * b );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1235_TV27_Defect_1454688()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Associative]
{
	a = function1(1,2,3);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1236_TV28_Defect_1454688_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = function(1,2,3);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1237_TV29_Overloading_Different_Number_Of_Parameters()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int ( a : int )
{
	return = a + 1;
}
def foo:int ( a : int, b : int, c: int )
{
	return = a + b + c ;
}
c = foo( 1 );
d = foo( 3, 2, 0 );
a;
[Imperative]
{
	a = foo( 1, 2, 3 );
}	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1238_TV30_Overloading_Different_Parameter_Types()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int ( a : int )
{
	return = 2 * a;
}
def foo:int ( a : double )
{
	return = 2;
}
	b = foo( 2 );
	c = foo(3.4);
d;
[Imperative]
{
	d = foo(-2.4);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1239_TV31_Overloading_Different_Return_Types()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo: int( a: int )
{
	return = 1;
}
// This is the same definition regardless of return type
def foo: double( a : int )
{
	return = 2.3;
}
b = foo ( 1 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1240_TV32_Function_With_Default_Argument_Value()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int ( a = 5, b = 5 )
{
	return =  a +  b;
}
c1;c2;c3;c4;
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1, 2 );
	c4 = foo ( 1, 2, 3 );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1241_TV32_Function_With_Default_Argument_Value_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo  ( a : int = 5, b : double = 5.5, c : bool = true )
{
	return = c == true ? a  : b;
}
def foo  ( a : double = 5, b : double = 5.5, c : bool = true )
{
	return = c == true ? a  : b;
}
c1;c2;c3;c4;
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1.5, 2 );
	c4 = foo ( 1, 2, 0 );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1242_TV32_Function_With_Default_Argument_Value_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo  ( a : int, b : double = 5, c : bool = true)
{
	return = c == true ? a  : b;
}
def foo2  ( a , b = 5, c = true)
{
	return = c == true ? a  : b;
}
c1;c3;c3;c4;
d1 = foo2 (  );
d2 = foo2 ( 1 );
d3 = foo2 ( 2, 3 );
d4 = foo2 ( 4, 5, false );
d5 = 
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 2, 3 );
	c4 = foo ( 4, 5, false );
	return = [ c1, c2, c3, c4 ];
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1243_TV33_Function_Overloading()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo  ( a : int = 5, b : double = 5.5, c : bool = true )
{
	return = c == true ? a  : b;
}
def foo  ( a : double = 6, b : double = 5.5, c : bool = true )
{
	return = c == true ? a  : b;
}
c1;c2;c3;c4;
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1.5, 2 );
	c4 = foo ( 1, 2, 0 );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1244_TV33_Function_Overloading_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo  ( a : int , b : double , c : bool  )
{
	return = a;
}
def foo  ( a : double, b : double , c : int  )
{
	return = b;
}
c4;c5;c6;
[Imperative]
{
	c4 = foo ( 1, 2, 0 );
	c5 = foo ( 1.5, 2.5, 0 );
	c6 = foo ( 1.5, 2.5, true );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1245_TV33_Overloading_Different_Order_Of_Parameters()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int ( a :int, b : double )
{
	return = 2;
}
def foo : int( c : double, d : int )
{
	return = 3;
}
c = foo( 1,2.5 );
d = foo ( 2.5, 1 );
//e = foo ( 2.5, 2.5 );
f = foo ( 1, 2 );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1247_TV35_Implicit_Conversion_Int_To_Double()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : double ( a: double )
{
	return = a + 2.5;
}
	b = foo( 2 );
	c;
[Imperative]
{
	c = foo( 3 );
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1248_TV36_Implicit_Conversion_Return_Type()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo: bool ( a : double, b : double )
{
	return = 0.5;
}
c = foo ( 2.3 , 3 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1249_TV37_Overloading_With_Type_Conversion()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int ( a : double, b : double )
{
	return = 1;
}
def foo : int ( a : int, b : int )
{
	return = 2;
}
def foo : int ( a : int, b : double )
{
	return = 3;
}
a = foo ( 1,2 );
b = foo ( 2,2 );
c = foo ( 1, 2.3 );
d = foo ( 2.3, 2 );
 ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1252_TV40_Defect_1449956_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;y;
[Associative]
{
	def recursion  : int( a : int)
	{
		temp = [Imperative]
		{
			if ( a ==0 || a < 0)
			{
				return = 0;	
			}
			return = a + recursion( a - 1 );
		}		 
		return = temp;
	}
	x = recursion( 4 );
	y = recursion( -1 );
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1254_TV42_Defect_1454959()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;
def Level1 : int (a : int)
{
    return = Level2(a+1);
}
 
def Level2 : int (a : int)
{
    return = a + 1;
}
input = 3;
result = Level1(input); 
[Associative]
{
    a = Level1(4);
	b = foo (a);
	c = [Imperative]
	{
	    return = foo2( foo (a ) );
	}
}
def foo ( a )
{
    return = a + foo2(a);
}
def foo2 ( a ) 
{
    return = a;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1259_TV47_Defect_1456087()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int ( a : double, b : double )
{
	return = 2;
}
def foo : int ( a : int, b : int )
{
	return = 1;
}
def foo : int ( a : int, b : double )
{
	return = 3;
}
a;b;c;d;
[Imperative]
{
	a = foo ( 1, 2 );
	b = foo ( 2.5, 2.5 );
	c = foo ( 1, 2.3 );
	d = foo ( 2.3, 2 );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1260_TV48_Defect_1456110()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo : int( a : int)
{
  temp = [Imperative]
  {
      if ( a == 0 )
      {
          return = 0; 
      }
      return = a ;
  } 
  return = temp;
}
x = foo( 0 );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1261_TV49_Defect_1456110()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def recursion : int(a : int)
{
    loc = [Imperative]
    {
        if (a <= 0)
        {
            return = 0; 
        }
        return = a + recursion(a - 1);
    }
    return = loc;
}
a = 10;
[Imperative]
{
	x = recursion(a); 
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1262_TV49_Defect_1456110_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
def recursion : int(a : int)
{
    loc = [Imperative]
    {
        if (a <= 0)
        {
            return = 0; 
        }
        return = a + recursion(a - 1);
    }
    return = loc;
}
a = 10;
[Imperative]
{
	x = recursion(a); 
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1271_TV55_Defect_1456571()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(arr)
{
    retArr = 3;	
	[Imperative]
    {
		retArr = 5;
	}
    return = retArr;
}
	x = 0.5;
	x = foo(x);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1272_TV56_Defect_1456571_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(arr)
{
    retArr = 3;	
	[Associative]
    {
		retArr = 5;
	}
    return = retArr;
}
x;
	[Imperative]
	{
		x = 0.5;
		x = foo(x);
	}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1275_TV58_Defect_1455090()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"	def foo( a:int[] )
	{
		a[0][0] = 1;
		return = a;
	}
	b = [ [0,2,3], [4,5,6] ];
	d = foo( b );
	c = d[0];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1278_TV59_Defect_1455278_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def multiply : double[] (a : double[])
{    
	temp = [Imperative]
    { 
		b = [0, 10];
		counter = 0; 
		
		for( y in a ) 
		{              
			b[counter] = y * y;   
			counter = counter + 1;           
		}                
        
		return = b;    
	}   
	return = temp;
}
	
	x = [2.5,10.0];
	x_squared = multiply( x );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1279_TV60_Defect_1455278_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def multiply : double[] (a : double[])
{    
	temp = [Imperative]
    { 
		b = [0, 10];
		counter = 0; 
		
		for( y in a ) 
		{              
			b[counter] = y * y;   
			counter = counter + 1;           
		}                
        
		return = b;    
	}   
	return = temp;
}
	
	x = [2.5,10];
	x_squared = multiply( x );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1280_TV60_Defect_1455278_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
    def power(num : double, exponent : int)
    {
		temp = [Imperative]
        {
			result = 1.0;
            counter = 0;            
			while(counter < exponent )            
			{
				result = result * num;                
				counter =  counter + 1;            
			}            
			return = result;        
		}       
		return = temp;
	}    
	
	x = power(3.0, 2);
	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1281_TV61_Defect_1455278_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
    def power(num : double, exponent : int)
    {
		temp = [Imperative]
        {
			result = 1.0;
            counter = 0;            
			while(counter < exponent )            
			{
				result = result * num;                
				counter =  counter + 1;            
			}            
			return = result;        
		}       
		return = temp;
	}    
	
	x = power(3.0, 2);
	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1282_TV61_Defect_1455278_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
    def power(num : double, exponent : int)
    {
		temp = [Imperative]
        {
			result = 1.0;
            counter = 0;            
			if(num > exponent)
			{
				while(counter < exponent )            
				{
					result = result * num;                
					counter =  counter + 1;            
				}  
            }				
			return = result;        
		}       
		return = temp;
	}    
	
	x = power(3.0, 2);
	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1284_TV62_Defect_1455090()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"	def foo:int[]..[] ( a:int[]..[] )
	{
		a[0][0] = 1;
		return = a;
	}
	b = [ [0,2,3], [4,5,6] ];
	d = foo( b );
	c = d[0];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1286_TV63_Defect_1455090_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def foo ( a : double[]..[] )
	{
		a[0][0] = 2.5;
		return = a;
	}
	
	a = [ [2.3,3.5],[4.5,5.5] ];
	
	a = foo( a );
	c = a[0];
	d;
	[Imperative]
	{
		b = [ [2.3], [2.5] ];
		b = foo( b );
		d = b[0];
	}
	
	
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1297_TV71_Defect_1456108_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"        def collectioninc: int[]( a : int[] )
	{
		j = 0;
		a = [Imperative]
		{
			for( i in a )
			{
				a[j] = a[j] + 1;
				j = j + 1;
			}
			return = a;
		}
		return = a;
	}
	d = [ 1,2,3 ];
	c = collectioninc( d );
	b;
        [Imperative]
	{
		b = collectioninc( d );
	}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1301_TV73_Defect_1451831()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"y;
[Associative]
{
  
	a = 1;
	b = 1;
	
	def test:int( a:int, b:int, c : int, d : int )
	{
		 
	    y = [Imperative]
		{
			if( a == b ) 
			{
				return = 1;
			}		
			else
			{
				return = 0;
			}
		}
		
		return = y + c + d;
	}
	
	c = 1;
	d = 1;
	
	y = test ( a , b, c, d);
	
		
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1303_TV75_Defect_1456870()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1 ( b )
{
	return = b == 0 ? 0 : b+1;
	
}
def foo2 ( x )
{
	y = [Imperative]
	{
	    if(x > 0)
		{
		   return = x >=foo1(x) ? x : foo1(x);
		}
		return = x >=2 ? x : 2;
	}
	x1 = y == 0 ? 0 : y;
	return = x1 + y;
}
a1 = foo1(4);
a2 = foo2(3);
//thisTest.Verification(mirror, ""a1"", 5, 0); // fails";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1304_TV76_Defect_1456112()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1 : double (arr : double[])
{
    return = 0;
}
arr = [1,2,3,4];
sum = foo1(arr);
def foo2 : double (arr : double)
{
    return = 0;
}
arr1 = [1.0,2.0,3.0,4.0];
sum1 = foo2(arr1);
sum2 = foo1(arr);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1305_TV76_Defect_1456112_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1 : double (arr : double[][])
{
    return = arr[0][0];
}
sum1;sum2;
[Imperative]
{
	arr1 = [ [1, 2.0], [true, 4] ];
	sum1 = foo1(arr);
	x = 1;
	arr2 = [ [1, 2.0], [x, 4] ];
	sum2 = foo1(arr2);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1314_TV81_Defect_1458187()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a )
{
	x = [Imperative]
	{
		if ( a == 0 )
		return = a;
		else
		return = a + 1;
	}
	return = x;
}
a = foo( 2 );
b = foo(false);
c = foo(true);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1315_TV81_Defect_1458187_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a )
{
	x = (a == 1)?a:0;
	return = x + a;
}
a = foo( 2 );
b = foo(false);
c = foo(true);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1316_TV82_Defect_1460892()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a : int )
{
    return  = a + 1;
}
def foo2 ( b : int, f1 : function )
{
    return = f1( b );
}
a = foo2 ( 10, foo );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1317_TV83_Function_Pointer()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a : bool )
{
    return  = a ;
}
def foo2 ( b : int, f1 : function )
{
    return = f1( b );
}
a = foo2 ( 0, foo );
def poo ( a : int )
{
    return  = a ;
}
def poo2 ( b : bool, f1 : function )
{
    return = f1( b );
}
a2 = poo2 ( false, poo );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1318_TV83_Function_Pointer_Collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def count ( a : bool[]..[] )
{
    c = 0;
	c = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = c ;
}
def foo ( b : bool[]..[], f1 : function )
{
    return = count( b );
}
a = foo ( [ true, false, [ true, true ] ],  count );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1319_TV84_Function_Pointer_Implicit_Conversion()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def count ( a : int[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : double[], f1 : function )
{
    return = count( b );
}
a = foo ( [ 1.0, 2.6 ],  count );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1320_TV84_Function_Pointer_Implicit_Conversion_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def count ( a : double[]..[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[]..[], f1 : function )
{
    return = count( b );
}
a = foo ( [ 1, 2 , [3, 4] ],  count );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1321_TV84_Function_Pointer_Implicit_Conversion_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def count ( a : double[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[], f1 : function )
{
    return = count( b );
}
a = foo ( [ 1, 2,  [ 3, 4 ] ],  count );
d = foo ( [ 2, 2.5, [ 1, 1.5 ], 1 , false],  count );
// boolean can't be converted to double, so the following statement
// will generate a method resultion fail exception
// b = foo ( { true, false },  count );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1322_TV84_Function_Pointer_Implicit_Conversion_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def count ( a : double[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[], f1 : function )
{
    return = count( b );
}
[Imperative]
{
	a = foo ( [ 1, 2,  [ 3, 4 ] ],  count );
	d = foo ( [ 2, 2.5, [ 1, 1.5 ], 1 , false],  count );
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1323_TV84_Function_Pointer_Negative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def greater ( a , b )
{
    c = [Imperative]
	{
	    if ( a > b )
		    return = a;
		else
		   return = b;
	}
	return  = c ;
}
def greatest ( a : double[], f : function )
{
    c = a[0];
	[Imperative]
	{
	    for ( i in a )
		{
		    c = f( i, c );
		}	
	}
	return  = c ;
}
a;
[Imperative]
{
	a = greatest ( [ 1.5, 6, 3, -1, 0 ], greater2 );
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1324_TV84_Function_Pointer_Using_2_Functions()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def greater ( a , b )
{
    c = [Imperative]
	{
	    if ( a > b )
		    return = a;
		else
		   return = b;
	}
	return  = c ;
}
def greatest ( a : double[], greater : function )
{
    c = a[0];
	[Imperative]
	{
	    for ( i in a )
		{
		    c = greater( i, c );
		}	
	}
	return  = c ;
}
def foo ( a : double[], greatest : function , greater : function)
{
    return  = greatest ( a, greater );
}
a;
[Imperative]
{
	a = foo ( [ 1.5, 6, 3, -1, 0 ], greatest, greater );
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1326_TV86_Defect_1456728()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def f1 (arr :  double[] )
{
    return = arr;
}
def f2 (arr :  double[] )
{
    return = [ arr[0], arr[1] ];
}
a = f1( [ null, null ] );
b = f2( [ null, null ] );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1332_TV88_Defect_1463489()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo: bool (  )
{
	return = 0.24;
}
c = foo ( ); //expected true, received 
d = [Imperative]
{
    return = foo();
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1333_TV88_Defect_1463489_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo: bool ( x : bool )
{
	return = x && true;
}
c = foo ( 0.6 ); 
c1 = foo ( 0.0 ); 
d = [Imperative]
{
    return = foo(-3.5);
}
d1 = [Imperative]
{
    return = foo(0.0);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1336_TV89_typeConversion_FunctionArguments_1467060()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : double[])
{
    return = x;
}
a2 = [ 2, 4, 3.5 ];
b2 = foo (a2);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1337_TV89_typeConversion_FunctionArguments_1467060_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : double[])
{
    return = x;
}
a2 = [ 2, 4, 3];
b2 = foo ( a2 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1338_TV89_typeConversion_FunctionArguments_1467060_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : int[])
{
    return = x;
}
a1 = [ 2, 4.1, 3.5];
b1 = foo ( a1 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1339_TV89_typeConversion_FunctionArguments_1467060_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : int)
{
    return = x;
}
a1 = [ 2, 4.1, false];
b1 = foo ( a1 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1340_TV89_typeConversion_FunctionArguments_1467060_5()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : int[])
{
    return = x;
}
a1 = [ 2, 4.1, [1,2]];
b1 = foo ( a1 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1341_TV89_typeConversion_FunctionArguments_1467060_6()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : int[])
{
    return = x;
}
a1 = [ null, 5, 6.0];
b1 = foo ( a1 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1342_TV89_typeConversion_FunctionArguments_1467060_7()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : int[])
{
    return = x;
}
a1 = [ null, null, null];
b1 = foo ( a1 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1343_TV89_typeConversion_FunctionArguments_1467060_8()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int[]( x : int[])
{
    return = x;
}
a1 = [1.1,2.0,3];
b1 = foo ( a1 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1344_TV89_typeConversion_FunctionArguments_1467060_9()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : int[])
{
    return = x;
}
a1 = [ 1, null, 6.0];
b1 = foo ( a1 );";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1346_TV90_Defect_1463474_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 3;
def foo : void  ( )
{
	a = 2;		
}
foo();
b1 = a;	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1347_TV90_Defect_1463474_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 3;
def foo : void  (  )
{
	a = 2;
    return = -3;	
}
c1 = foo();
b1 = a;	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1365_TV97_Heterogenous_Objects_As_Function_Arguments_With_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : double)
{
    return = x;
}
a1 = [ 2.5, 4 ];
b1 = foo ( a1 );
a2 = [ 3, 4, 2.5 ];
b2 = foo ( a2 );
a3 = [ 3, 4, 2 ];
b3 = foo ( a3 );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1367_TV98_Method_Overload_Over_Rank_Of_Array()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(x : int[])
{ 
    return = 1;
}
def foo(x : int[]..[])
{ 
    return = 2;
}
def foo(x : int[][])
{ 
    return = 0;
}
    
x = foo ( [ [ 0,1], [2, 3] ] );
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1369_TV99_Defect_1463456_Array_By_Reference_Issue_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def A (a: int [])
{
return = a;
}
val = [1,2,3];
b = A(val);
b[0] = 100; 
t = val[0]; //expected 100, received 1";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1370_BaseImportAssociative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{
	a = 5;
	def twice : int (val: int)
	{
		return = val * 2;
	}
	b = twice(a);	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1372_BaseImportWithVariableClassInstance()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""T009_BasicImport_TestClassInstanceMethod.ds"");
a = 5;
b = 2*a;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1374_basicImport1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def Scale (arr : double[], scalingFactor : double)
{
    scaledArr = [Imperative]
    {
        counter = 0;
        for(val in arr)
        {
            arr[counter] = scalingFactor * val;
            counter = counter + 1;
        }
        return = arr;
    }
    return = scaledArr;
}
a = 1;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1376_basicImport3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def Scale (arr : double[], scalingFactor : double)
{
    scaledArr = [Imperative]
    {
        counter = 0;
        for(val in arr)
        {
            arr[counter] = scalingFactor * val;
            counter = counter + 1;
        }
        return = arr;
    }
    return = scaledArr * 2;
}
a = 3;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1377_T001_BasicImport_CurrentDirectory()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""basicImport.ds"");
a = [1.1,2.2];
b = 2;
c = Scale(a,b);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1378_T002_BasicImport_AbsoluteDirectory()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""basicImport.ds"");
a = [1.1,2.2];
b = 2;
c = Scale(a,b);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1379_T004_BasicImport_CurrentDirectoryWithDotAndSlash()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import ("".\basicImport.ds"");
a = [1.1,2.2];
b = 2;
c = Scale(a,b);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1380_T005_BasicImport_RelativePath()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import ("".\\ExtraFolderToTestRelativePath\\basicImport.ds"");
a = [1.1,2.2];
b = 2;
c = Scale(a,b);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1381_T006_BasicImport_TestFunction()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""basicImport.ds"");
a = [1.1,2.2];
b = 2;
c = Scale(a,b);
d = Sin(30.0);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1386_T012_BaseImportImperative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""BaseImportImperative.ds"");
a = 1;
b = a;
c;
[Associative]
{
	c = 3 * b;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1390_T016_BaseImportAssociative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""BaseImportAssociative.ds"");
a = 10;
b = 20;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1391_T017_BaseImportWithVariableClassInstance_Associativity()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""BaseImportWithVariableClassInstance.ds"");
c = a + b;
a = 10;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1394_T020_MultipleImport_WithSameFunctionName()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""basicImport1.ds"");
import (""basicImport3.ds"");
arr = [ 1.0, 2.0, 3.0 ];
a1 = Scale( arr, 4.0 );
b = a * 2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1395_T021_Defect_1457354()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""c:\\wrongPath\\test.ds"");
a = 1;
b = a * 2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1396_T021_Defect_1457354_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""basicImport"");
a = 1;
b = a * 2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1397_T021_Defect_1457354_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""basicImport12.ds"");
a = 1;
b = a * 2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1398_T022_Defect_1457740()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""basicImport1.ds"");
import (""basicImport3.ds"");
arr1 = [ 1, 3, 5 ];
temp = Scale( arr1, a );
a = a;
b = 2 * a;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1399_basicImport()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def Scale (arr : double[], scalingFactor : double)
{
    scaledArr = [Imperative]
    {
        counter = 0;
        for(val in arr)
        {
            arr[counter] = scalingFactor * val;
            counter = counter + 1;
        }
        return = arr;
    }
    return = scaledArr;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1400_T003_BasicImport_ParentPath()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""../basicImport.ds"");
a = [1.1,2.2];
b = 2;
c = Scale(a,b);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1402_Test_4_10_contains()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 5;
b = 7;
c = 9;
d = [a, b];
f = Contains(d, a); // true
g = Contains(d, c); // false
h = Contains([10,11],11); // true collection built �on the fly�
				  // with �literal� values
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1403_Test_4_11_indexOf()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 5;
b = 7;
c = 9;
d = [a, b, c];
f = IndexOf(d, b); // 1
g = d[f+1]; // c
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1405_Test_4_13_Transpose()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a =[[1,2],[3,4]];
b = a[0][0]; // b = 1
c = a [0][1]; // c = 2
a = Transpose(a); // b = 1; c =3
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1406_Test_4_14_isUniformDepth()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"myNonUniformDepth2Dcollection = [[1, 2, 3], [4, 5], 6];
individualMemberB = myNonUniformDepth2Dcollection [0][1]; // OK, = B
individualMemberD = myNonUniformDepth2Dcollection [2][0]; // would fail
individualMemberE = myNonUniformDepth2Dcollection [2];    // OK, = 6
// Various collection manipulation functions are provided to assist with these issues, one of these functions is:
testDepthUniform = IsUniformDepth(myNonUniformDepth2Dcollection); // = false
testForDeepestDepth  = Rank(myNonUniformDepth2Dcollection); // = 2; current limitation :  1
 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1408_Test_4_15_someNulls()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [ 1, null, 2, 3 ];
b = Count(a); // 4 after updating a @ line 9 this value become 3.
c = SomeNulls(a); // true after updating a @ line 9 this value become false.
d = a[-2]; // d = 2 note: use of fixed index [-2] 
a = RemoveNulls(a); // {1, 2, 3}... d = 2
f = Count(d); // 2
g = SomeNulls(a); // false
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1409_Test_4_17_arrayAssignment()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0..5;
a[1] = -1; // replace a member of a collection
a[2] = a[2] + 0.5; // modify a member of a collection
a[3] = null; // make a member of a collection = null
a[4] = [ 3.4, 4.5 ]; // allowed, but not advised: subsequently altering the structure of the collection
c = a;
b = [ 0, -1, 2.5, null, [ 3.4, 4.5 ], 5 ]; // however a collection of non-uniform depth and irregular structure can be defined
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1410_Test_4_18_removeByIndex()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
b = 2;
c = 3;
d = 4;
x = [ a, b, c, d ];
u = Remove(x, 0); // remove by content.. u = {b, c, d};
v = Remove(x, -1); // remove by index.. x = {a, b, c};
w = Insert(x, d, 0); // insert after defined index.. x = {d,a,b,c,d};";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1411_Test_4_20_zipped_collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"// Current limitation : 
a = [3, 4, 5];
b = [2, 6];
c = a + b ; // { 5, 10, null}; // Here the length of the resulting variable [c] will be based on the length of the first
//collection encountered [in this case a]
d = b + a; // { 5, 10}; // Here the length of the resulting variable [d] will be based on the length of the first
// collection encountered [in this case b]
// Workaround :
//def sum(a, b)
//{
  //return = a + b;
//}
//d = sum(a, b); // {5, 10}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1412_Test_4_22_replication_guide_with_ragged_collection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"// The use of replication guides with ragged collections can be unpredictable results, as follows:
a = [ 1, [ 3, 4 ] ]; // initial ragged collections
b = [ [ 5, 6 ], 7 ];
c = a + b; // c = { { 6, 7 }, { 10, 11 } } � zipped collection
//d = a<1> + b<2>; // unpredictable
/*
// Woraround : 'flattening' ragged collections and then applying replication give far more predictable results:
f = Flatten(a); // e = { 1, 3, 4 }
g = Flatten(b); // f = { 5, 6, 7 }
h = g + f; // h = { 6, 9, 11 }
i = f<1> + g<2>; // i = { { 6, 7, 8 }, { 8, 9, 10 }, { 9, 10, 11 } }
// Normalising the depth of collections has limited value, if the resulting sub collections are of different length
i = NormalizeDepth(a); // i = {{1},{3,4}};
j = NormalizeDepth(b); // j = {{5,6},{7}};
k = i + j; // unpredictable
l = i<1> + j<2>; // unpredictable*/";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1414_Test_4_9_count()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"count_test1=Count([1,2]);   // 2 .. count of collection
a = [[1,2],3];		   // define a nested/ragged collection
count_test2=Count(a);       // 2 .. count of collection
count_test3=Count(a[0]);    // 2 .. count of sub collection
count_test4=Count(a[0][0]); // 0 .. count of single member
m = a[0][0];
count_test5=Count(a[1]);    // 0 .. count of single member
count_test6=Count([]); 	   // 0 .. count of an empty collection
count_test7=Count(3); 	   // 0 .. count of single value
count_test8=Count(null);    // null .. count of null
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1415_Regress_1467127()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"i = 1..6..#10;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1416_T01_SimpleRangeExpression()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;a1;a2;a3;a4;a5;a6;a7;a8;a9;a10;a11;a12;a13;a14;a17;
[Imperative]
{
	a = 1..-6..-2;
	a1 = 2..6..~2.5; 
	a2 = 0.8..1..0.2; 
	a3 = 0.7..1..0.3; 
	a4 = 0.6..1..0.4; 
	a5 = 0.8..1..0.1; 
	a6 = 1..1.1..0.1; 
	a7 = 9..10..1; 
	a8 = 9..10..0.1;
	a9 = 0..1..0.1; 
	a10 = 0.1..1..0.1;
	a11 = 0.5..1..0.1;
	a12 = 0.4..1..0.1;
	a13 = 0.3..1..0.1;
	a14 = 0.2..1..0.1;
	a17 = (0.5)..(0.25)..(-0.25);
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1417_T02_SimpleRangeExpression()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a15;a16;a18;a19;a20;
[Imperative]
{
	a15 = 1/2..1/4..-1/4;
	a16 = (1/2)..(1/4)..(-1/4);
	a18 = 1.0/2.0..1.0/4.0..-1.0/4.0;
	a19 = (1.0/2.0)..(1.0/4.0)..(-1.0/4.0);
	a20 = 1..3*2; 
	//a21 = 1..-6;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1418_T03_SimpleRangeExpressionUsingCollection()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"w1;w2;w3;w4;w5;
[Imperative]
{
	a = 3 ;
	b = 2 ;
	c = -1;
	w1 = a..b..-1 ; //correct  
	w2 = a..b..c; //correct 
	e1 = 1..2 ; //correct
	f = 3..4 ; //correct
	w3 = e1..f; //correct
	w4 = (3-2)..(w3[1][1])..(c+2) ; //correct
	w5 = (w3[1][1]-2)..(w3[1][1])..(w3[0][1]-1) ; //correct
}
/* expected results : 
    Updated variable a = 3
    Updated variable b = 2
    Updated variable c = -1
    Updated variable w1 = { 3, 2 }
    Updated variable w2 = { 3, 2 }
    Updated variable e1 = { 1, 2 }
    Updated variable f = { 3, 4 }
    Updated variable w3 = { { 1, 2, 3 }, { 2, 3, 4 } }
    Updated variable w4 = { 1, 2, 3 }
    Updated variable w5 = { 1, 2, 3 }
*/
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1420_T05_RangeExpressionWithIncrement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d;e1;f;g;h;i;j;k;l;m;
[Imperative]
{
	d = 0.9..1..0.1;
	e1 = -0.4..-0.5..-0.1;
	f = -0.4..-0.3..0.1;
	g = 0.4..1..0.2;
	h = 0.4..1..0.1;
	i = 0.4..1;
	j = 0.6..1..0.4;
	k = 0.09..0.1..0.01;
	l = 0.2..0.3..0.05;
	m = 0.05..0.1..0.04;
	n = 0.1..0.9..~0.3;
	k = 0.02..0.03..#3;
	l = 0.9..1..#5;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1421_T06_RangeExpressionWithIncrement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;
[Imperative]
{
	a = 0.3..0.1..-0.1;
	b = 0.1..0.3..0.2;
	c = 0.1..0.3..0.1;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1425_T10_RangeExpressionWithReplication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	//step value greater than the end value
	a = 1..2..3;
	b = 3..4..3;
	c = a..b..a[0]; // {{1,2,3}}
	d = 0.5..0.9..0.1;
	e1 = 0.1..0.2..0.05;
	f = e1[1]..d[2]..0.5;
	g = e1..d..0.2;
	h = e1[2]..d[1]..0.5;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1427_T12_RangeExpressionUsingNestedRangeExpressions()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;c;d;e1;f;g;h;i;j;
[Imperative]
{
	x = 1..5..2; // {1,3,5}
	y = 0..6..2; // {0,2,4,6}
	a = (3..12..3)..(4..16..4); // {3,6,9,12} .. {4..8..12..16}
	b = 3..00.6..#5;      // {3.0,2.4,1.8,1.2,0.6}
	//c = b[0]..7..#1;    //This indexed case works
	c = 5..7..#1;         //Compile error here , 5
	d = 5.5..6..#3;       // {5.5,5.75,6.0}
	e1 = -6..-8..#3;      //{-6,-7,-8}
	f = 1..0.8..#2;       //{1,0.8}
	g = 1..-0.8..#3;      // {1.0,0.1,-0.8}
	h = 2.5..2.75..#4;    //{2.5,2.58,2.67,2.75}
	i = x[0]..y[3]..#10;//1..6..#10
	j = 1..0.9..#4;// {1.0, 0.96,.93,0.9}
	k= 1..3..#0;//null
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1430_T15_SimpleRangeExpression_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;d;f;g;h;i;l;
[Imperative]
{
	a = 1..2.2..#3;
	b = 0.1..0.2..#4;
	c = 1..3..~0.2;
	d = (a[0]+1)..(c[2]+0.9)..0.1; 
	e1 = 6..0.5..~-0.3;
	f = 0.5..1..~0.3;
	g = 0.5..0.6..0.01;
	h = 0.51..0.52..0.01;
	i = 0.95..1..0.05;
	j = 0.8..0.99..#10;
	//k = 0.9..1..#1;
	l = 0.9..1..0.1;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1431_T16_SimpleRangeExpression_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;d;e1;f;g;h;
[Imperative]
{
	a = 1.2..1.3..0.1;
	b = 2..3..0.1;
	c = 1.2..1.5..0.1;
	//d = 1.3..1.4..~0.5; //incorrect 
	d = 1.3..1.4..0.5; 
	e1 = 1.5..1.7..~0.2;
	f = 3..3.2..~0.2;
	g = 3.6..3.8..~0.2; 
	h = 3.8..4..~0.2; 
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1432_T17_SimpleRangeExpression_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;d;e1;f;g;h;i;j;k;
[Imperative]
{
	a = 1..2.2..~0.2;
	b = 1..2..#3;
	c = 2.3..2..#3;
	d = 1.2..1.4..~0.2;
	e1 = 0.9..1..0.1;
	f = 0.9..0.99..~0.01;
	g = 0.8..0.9..~0.1;
	h = 0.8..0.9..0.1;
	i = 0.9..1.1..0.1;
	j = 1..0.9..-0.05;
	k = 1.2..1.3..~0.1;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1433_T18_SimpleRangeExpression_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;d;e1;f;g;h;
[Imperative]
{
	a = 2.3..2.6..0.3;
	b = 4.3..4..-0.3;
	c= 3.7..4..0.3;
	d = 4..4.3..0.3;
	e1 = 3.2..3.3..0.3;
	f = 0.4..1..0.1;
	g = 0.4..0.45..0.05;
	h = 0.4..0.45..~0.05; 
	g = 0.4..0.6..~0.05;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1434_T19_SimpleRangeExpression_5()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;c;d;e1;f;g;h;i;
[Imperative]
{
	//a = 0.1..0.2..#1; //giving error
	b = 0.1..0.2..#2;
	c = 0.1..0.2..#3;
	d = 0.1..0.1..#4;
	e1 = 0.9..1..#5;
	f = 0.8..0.89..#3;
	g = 0.9..0.8..#3;
	h = 0.9..0.7..#5;
	i = 0.6..1..#4;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1439_T25_RangeExpression_WithDefaultDecrement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=5..1;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1440_T25_RangeExpression_WithDefaultDecrement_1467121()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=5..1;
b=-5..-1;
c=1..0.5;
d=1..-0.5;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1441_T25_RangeExpression_WithDefaultDecrement_nested_1467121_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=(5..1).. (1..5);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1443_T26_RangeExpression_Function_tilda_associative_1457845_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;a;b;f;g;h;j;k;l;m;
[Associative]
{
	def square : double ( x: double ) 
	{
		return = x * x;
	}
}
[Imperative]
{
	x = 0.1; 
	a = 0..2..~0.5;
	b = 0..0.1..~square(0.1);
	f = 0..0.1..~x;      
	g = 0.2..0.3..~x;    
	h = 0.3..0.2..~-0.1; 
	
	j = 0.8..0.5..~-0.3;
	k = 0.5..0.8..~0.3; 
	l = 0.2..0.3..~0.0;
	m = 0.2..0.3..~1/2; // division 
}
	
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1444_T26_RangeExpression_Function_tilda_multilanguage_1457845_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;a;b;f;g;h;j;k;l;m;
[Associative]
{
	def square : double ( x: double ) 
	{
		return = x * x;
	}
[Imperative]
{
	x = 0.1; 
	a = 0..2..~0.5;
	b = 0..0.1..~square(0.1);
	f = 0..0.1..~x;      
	g = 0.2..0.3..~x;    
	h = 0.3..0.2..~-0.1; 
	
	j = 0.8..0.5..~-0.3;
	k = 0.5..0.8..~0.3; 
	l = 0.2..0.3..~0.0;
	m = 0.2..0.3..~1/2; // division 
}
	}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1446_T27_RangeExpression_Function_Associative_1463472()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z1;
[Associative]
{
	def twice : double( a : double )
	{
		return = 2 * a;
	}
	z1 = 1..twice(4)..twice(1);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1447_T27_RangeExpression_Function_Associative_1463472_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Associative]
{
	def twice : int []( a : double )
	{
		c=1..a;
		return = c;
	}
d=1..4;
c=twice(4);
//	z1 = 1..twice(4)..twice(1);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1448_T27_RangeExpression_Function_Associative_replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z1;
[Associative]
{
	def twice : int[]( a : int )
	{
		c=2*(1..a);
		return = c;
	}
    d=[1,2,3,4];
	z1=twice(d);
//	z1 = 1..twice(4)..twice(1);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1449_T27_RangeExpression_Function_return_1463472()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Associative]
{
	def twice : int []( a : double )
	{
		c=1..a;
		return = c;
	}
d=1..4;
c=twice(4);
//	z1 = 1..twice(4)..twice(1);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1450_TA01_RangeExpressionWithIntegerIncrement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1;a2;
[Imperative]
{
	a1 = 1..5..2;
	a2 = 12.5..20..2;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1451_TA02_RangeExpressionWithDecimalIncrement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1;a2;
[Imperative]
{
	a1 = 2..9..2.7;
	a2 = 10..11.5..0.3;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1452_TA03_RangeExpressionWithNegativeIncrement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;
[Imperative]
{
	a = 10..-1..-2;
	b = -2..-10..-1;
	c = 10..3..-1.5;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1453_TA04_RangeExpressionWithNullIncrement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	a = 1..5..null;
	b = 0..6..(null);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1454_TA05_RangeExpressionWithBooleanIncrement()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	a = 2.5..6..(true);
	b = 3..7..false;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1455_TA06_RangeExpressionWithIntegerTildeValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	a = 1..10..~4;
	b = -2.5..10..~5;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1456_TA07_RangeExpressionWithDecimalTildeValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	a = 0.2..0.3..~0.2; //divide by zero error
	b = 6..13..~1.3;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1457_TA08_RangeExpressionWithNegativeTildeValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	a = 3..1..~-0.5;
	b = 18..13..~-1.3;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1458_TA09_RangeExpressionWithNullTildeValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	a = 1..5..~null;
	b = 5..2..~(null);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1459_TA10_RangeExpressionWithBooleanTildeValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	a = 1..3..(true);
	b = 2..2..false;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1460_TA11_RangeExpressionWithIntegerHashValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;
[Imperative]
{
	a = 1..3.3..#5;
	b = 3..3..#3;
	c = 3..3..#1;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1461_TA12_RangeExpressionWithDecimalHashValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	a = 1..7..#2.5;
	b = 2..10..#2.4;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1462_TA13_RangeExpressionWithNegativeHashValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = 7.5..-2..#-9;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1463_TA14_RangeExpressionWithNullHashValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = 2..10..#null;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1464_TA15_RangeExpressionWithBooleanHashValue()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;
[Imperative]
{
	b = 12..12..#false;
	a = 12..12..#(true);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1465_TA16_RangeExpressionWithIncorrectLogic_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = 5..1..2;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1466_TA17_RangeExpressionWithIncorrectLogic_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = 5.5..10.7..-2;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1467_TA18_RangeExpressionWithIncorrectLogic_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;
[Imperative]
{
	a = 7..7..5;
	b = 8..8..~3;
	c = 9..9..#1;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1468_TA19_RangeExpressionWithIncorrectLogic_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = null..8..2;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1470_TA21_Defect_1454692()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
b;
y;
[Imperative]
{
	x = 0;
	b = 0..3; //{ 0, 1, 2, 3 }
	for( y in b )
	{
		x = y + x;
	}
	
}	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1471_TA21_Defect_1454692_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def length : int (pts : double[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }
        
        return = counter;
    }
    return = numPts;
}
    
arr = 0.0..3.0;//{0.0,1.0,2.0,3.0};
num = length(arr);
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1473_TA21_Defect_1454692_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(i : int[])
{
    count = 0;
	count = [Imperative]
	{
	    for ( x in i )
		{
		    count = count + 1;
		}
		return = count;
	}
	return = count;
	
}
    
arr = 0.0..3.0;//{0.0,1.0,2.0,3.0};
c;x;
[Imperative]
{
	x = 0;
	b = 0..3; //{ 0, 1, 2, 3 }
	for( y in arr )
	{
		x = y + x;
	}
	x1 = 0..3;
	c = foo(x1);
	
}
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1474_TA22_Range_Expression_floating_point_conversion_1467127()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1..6..#10;
b = 0.1..0.6..#10;
c = 0.01..-0.6..#10;
d= -0.1..0.06..#10;
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1475_TA22_Range_Expression_floating_point_conversion_1467127_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0..7..~0.75;
b = 0.1..0.7..~0.075;
c = 0.01..-7..~0.75;
d= -0.1..7..~0.75; 
e = 1..-7..~1;
	";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1476_TA23_Defect_1466085_Update_In_Range_Expr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"y = 1;
y1 = 0..y;
y = 2;
z1 = y1; 
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1477_TA23_Defect_1466085_Update_In_Range_Expr_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0;
b = 10;
c = 2;
y1 = a..b..c;
a = 7;
b = 14;
c = 7;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1478_TA23_Defect_1466085_Update_In_Range_Expr_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( x : int[] )
{
    return = Count(x);
}
a = 0;
b = 10;
c = 2;
y1 = a..b..c;
z1 = foo ( y1 );
z2 = Count( y1 );
c = 5;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

       
        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1481_T003_LanguageBlockScope_ImperativeNestedAssociative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a_inner;
b_inner;
c_inner;
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
	[Associative]	
	{
		a_inner = a;
		b_inner = b;
		c_inner = c;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1482_T004_LanguageBlockScope_AssociativeNestedImperative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a_inner;b_inner;c_inner;
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	[Imperative]	
	{
		a_inner = a;
		b_inner = b;
		c_inner = c;
	}
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1483_T005_LanguageBlockScope_DeepNested_IAI()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a_inner1;a_inner2;
b_inner1;b_inner2;
c_inner1;c_inner2;
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
	[Associative]	
	{
		a_inner1 = a;
		b_inner1 = b;
		c_inner1 = c;
		
		
		[Imperative]
		{
			a_inner2 = a;
			b_inner2 = b;
			c_inner2 = c;
			
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1484_T006_LanguageBlockScope_DeepNested_AIA()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a_inner1;b_inner1;c_inner1;
a_inner2;b_inner2;c_inner2;
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	[Imperative]	
	{
		a_inner1 = a;
		b_inner1 = b;
		c_inner1 = c;
		
		
		[Associative]
		{
			a_inner2 = a;
			b_inner2 = b;
			c_inner2 = c;
			
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1485_T007_LanguageBlockScope_AssociativeParallelImperative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aI;bI;cI;
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	
}
[Imperative]	
{
	aI = a;
	bI = b;
	cI = c;
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1486_T008_LanguageBlockScope_ImperativeParallelAssociative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aA;bA;cA;
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
}
[Associative]	
{
	aA = a;
	bA = b;
	cA = c;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1487_T009_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_IA()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;
newA;newB;newC;
[Imperative]
{
	a = -10;
	b = false;
	c = -20.1;
	[Associative]	
	{
		a = 1.5;
		b = -4;
		c = false;
	}
	
	newA = a;
	newB = b;
	newC = c;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1488_T010_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_AI()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;
newA;newB;newC;
[Associative]
{
	a = -10;
	b = false;
	c = -20.1;
	[Imperative]	
	{
		a = 1.5;
		b = -4;
		c = false;
	}
	
	newA = a;
	newB = b;
	newC = c;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1489_T011_LanguageBlockScope_AssociativeParallelAssociative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aA;bA;cA;
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	
}
[Associative]	
{
	aA = a;
	bA = b;
	cA = c;
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1490_T012_LanguageBlockScope_ImperativeParallelImperative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aI; bI; cI;
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
	
}
[Imperative]	
{
	aI = a;
	bI = b;
	cI = c;
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1491_T013_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aI;bI;cI;
aA;bA;cA;
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	
}
[Imperative]	
{
	aI = a;
	bI = b;
	cI = c;
	
}
[Associative]	
{
	aA = a;
	bA = b;
	cA = c;
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1492_T014_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"aI;bI;cI;
aA;bA;cA;
[Imperative]
{
	a = 10;
	b = true;
	c = 20.1;
}
[Associative]	
{
	aA = a;
	bA = b;
	cA = c;
	
}
[Imperative]	
{
	aI = a;
	bI = b;
	cI = c;
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1493_T015_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;aI1;aA1;aI2;aA2;
[Associative]
{
	a = 10;
	
	[Imperative]	
	{
		aI1 = a;
	}
	aA1 = a;
	
	[Imperative]	
	{
		aI2 = a;
	}
	
	aA2 = a;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1494_T016_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;aA1;aI1;aA2;aI2;
[Imperative]
{
	a = 10;
	[Associative]	
	{
		aA1 = a;
	}
	aI1 = a;
	
	[Associative]	
	{
		aA2 = a;
	}
	aI2 = a;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1498_T020_LanguageBlockScope_AssociativeNestedImperative_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z;
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	[Imperative]	
	{
	x = 20;
	y = 10;
	z = foo (x, y);
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1500_T022_LanguageBlockScope_DeepNested_AIA_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z_1;
z_2;
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	[Imperative]	
	{
		x_1 = 20;
		y_1 = 10;
		z_1 = foo (x_1, y_1);
	
	
	[Associative]
		{
			x_2 = 100;
			y_2 = 100;
			z_2 = foo (x_2, y_2);
			
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1501_T023_LanguageBlockScope_AssociativeParallelImperative_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z;
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	a = 10;
	
}
[Imperative]	
{
	x = 20;
	y = 0;
	z = foo (x, y);
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1503_T025_LanguageBlockScope_AssociativeParallelAssociative_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z;
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	a = 10;
	
}
[Associative]	
{
	x = 20;
	y = 0;
	z = foo (x, y);
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1505_T027_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z_1;
z_2;
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	a = 10;
	
}
[Imperative]	
{
	x_1 = 20;
	y_1 = 0;
	z_1 = foo (x_1, y_1);
	
}
[Associative]
{
	x_2 = 20;
	y_2 = 0;
	z_2 = foo (x_2, y_2);
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1507_T029_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II_Function()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"z_I1;
z_I2;
z_A1;
z_A2;
[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	[Imperative]
	{
	x_I1 = 50;
	y_I1 = 50;
	z_I1 = foo (x_I1, y_I1);
	}
	
	x_A1 = 30;
	y_A1 = 12;
	z_A1 = foo (x_A1, y_A1);
	
	[Imperative]
	{
	x_I2 = 0;
	y_I2 = 12;
	z_I2 = foo (x_I2, y_I2);
	}
	
	x_A2 = 0;
	y_A2 = -10;
	z_A2 = foo (x_A2, y_A2);
	
	
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1510_T032_Cross_Language_Variables()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
a = 5;
b = 2 * a;
sum;
[Imperative] {
	sum = 0;
	arr = 0..b;
	for (i  in arr) {
		sum = sum + 1;
	}
}
a = 10;
// expected: sum = 21
// result: sum = 11";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1511_Z001_LanguageBlockScope_Defect_1453539()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"[Associative]
{	
	a = 10;	
	b = true;	
	c = 20.1;	
}
// [Imperative]	
// {	
// aI = a;	
// bI = a;	
// cI = a;	
// }";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1512_T01_String_IfElse_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""word"";
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1513_T01_String_IfElse_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""w ord"";
b = ""word"";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1514_T01_String_IfElse_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = "" "";
b = """";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1515_T01_String_IfElse_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""a"";
b = ""a"";
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1516_T01_String_IfElse_5()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""  "";//3 whiteSpace
b = ""	"";//tab
result = 
[Imperative]
{
	if (a ==b) return = true;
	else return = false;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1517_T01_String_IfElse_6()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = """";
b = "" "";
result = 
[Imperative]
{
	if (a ==null && b!=null) return = true;
	else return = false;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1518_T01_String_IfElse_7()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""a"";
result = 
[Imperative]
{
	if (a ==true||a == false) return = true;
	else return = false;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1519_T02_String_Not()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""a"";
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1521_T04_Defect_1454320_String()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"str;
[Associative]
{
str = ""hello world!"";
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1522_T04_Defect_1467100_String()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def f(s : string)
{
    return = s;
}
x = f(""hello"");
//expected : x = ""hello""
//received : x = null
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1525_T07_String_Replication()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""a"";
bcd = [""b"",""c"",""d""];
r = a +bcd;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1526_T07_String_Replication_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [""a""];
bc = [""b"",""c""];
str = a + bc;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1527_T07_String_Replication_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""a"";
b = [[""b""],[""c""]];
str = a +b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1528_T08_String_Inline()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""a"";
b = ""b"";
r = a>b? a:b;
r1 = a==b? ""Equal"":""!Equal"";";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1529_T08_String_Inline_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = ""a"";
b = ""b"";
r = a>b? a:b;
r1 = a==b? ""Equal"":""!Equal"";
b = ""a"";";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1530_T09_String_DynamicArr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a[1] = foo(""1"" + 1);
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1532_T11_String_Imperative()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"r =
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
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1536_T00001_Language_001_Variable_Expression_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"A = 10;   	   	// assignment of single literal value
B = 2*A;   	   	// expression involving previously defined variables
A = A + 1; 	   	// expressions modifying an existing variable;
A = 15;		   	// redefine A, removing modifier
A = [1,2,3,4]; 		// redefine A as a collection
A = 1..10..2;  		// redefine A as a range expression (start..end..inc)
A = 1..10..~4; 		// redefine A as a range expression (start..end..approx_inc)
A = 1..10..#4; 		// redefine A as a range expression (start..end..no_of_incs)
A = A + 1; 		// modifying A as a range expression
A = 1..10..2;  		// redefine A as a range expression (start..end..inc)
B[1] = B[1] + 0.5; 	// modify a member of a collection [problem here]
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1537_T00002_Language_001a_array_test_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=1;
b=2;
c=4;
collection = [a,b,c];
collection[1] = collection[1] + 0.5;
d = collection[1];
d = d + 0.1; // updates the result of accessing the collection
b = b + 0.1; // updates the source member of the collection
t1 = collection[0];
t2 = collection[1];
t3 = collection[2];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1538_T00003_Language_001b_replication_expressions()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1,0,-1];
b = [0, 5, 10];
zipped_sum = a + b; // {1, 5, 9}
cartesian_sum  = a<1> + b<2>;
// cartesian_sum =    {{1, 6, 11},
//        			   {0, 5, 10},
//        			   {-1, 4, 9}}
cartesian_sum  = a<2> + b<1>;
t1 = zipped_sum[0];
t2 = zipped_sum[1];
t3 = zipped_sum[2];
t4 = cartesian_sum[0][0];
t5 = cartesian_sum[1][0];
t6 = cartesian_sum[2][0];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1554_T01_Update_Variable_Across_Language_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;b;c;d;
[Associative]
{
    a = 0;
	d = a + 1;
    [Imperative]
    {
		b = 2 + a;
		a = 1.5;
		
    }
	c = 2;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1555_T02_Update_Function_Argument_Across_Language_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
[Imperative]
{
    a = 2.5;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1556_T03_Update_Function_Argument_Across_Language_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
def foo ( a1 : int )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
[Imperative]
{
    a = 2.5;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1560_T07_Update_Array_Variable()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1..3;
c = a;
b = [ Imperative ]
{
    count = 0;
	for ( i in a )
	{
	    a[count] = i + 1;
		count = count+1;
	}
	return = a;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1561_T08_Update_Array_Variable()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3988
            string defectID = "MAGN-3988 Defects with Expression Interpreter Test Framework";

            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1..3;
c = a;
b = [ Imperative ]
{
    count = 0;
	for ( i in a )
	{
	    if ( i > 0 )
		{
		    a[count] = i + 1;
		}
		count = count+1;
	}
	return = a;
}
d = [ Imperative ]
{
    count2 = 0;
	while (count2 <= 2 ) 
	{
	    if ( a[count2] > 0 )
		{
		    a[count2] = a[count2] + 1;
		}
		count2 = count2+1;
	}
	return = a;
}
e = b;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map, defectID: defectID);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1562_T09_Update_Across_Multiple_Imperative_Blocks()
        {
            string defectID = "MAGN-3988 Defects with Expression Interpreter Test Framework";
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
b = a;
c = [ Imperative ]
{
    a = 2;
	return = a;
}
d = [ Imperative ]
{
    a = 3;
	return = a;
}
e = c;
f = d;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map, defectID: defectID);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1563_T10_Update_Array_Across_Multiple_Imperative_Blocks()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1..3;
b = a;
c = [Imperative ]
{
    x = [ 10, a[1], a[2] ];
	a[0] = 10;
	return = x;
}
d = [ Imperative ]
{
    a[1] = 20;
	return = a;
}
e = c;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1564_T11_Update_Undefined_Variables()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b = a;
[Imperative]
{
    a = 3;
}
[Associative]
{
    a = 4;	
}
c = b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1565_T12_Update_Undefined_Variables()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b = a;
[Imperative]
{
    a = 3;
}
[Associative]
{
    a = 4;
    d = b + 1;	
}
c = b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1566_T13_Update_Variables_Across_Blocks()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 3;
b = a * 3;
c = [Imperative]
{
    d = b + 3;
	a = 4;
	return = d;
}
f = c + 1;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1573_T15_Defect_1460935_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x = 1;
y = x;
x = true; //if x = false, the update mechanism works fine
yy = y;
x = false;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1577_T16_Defect_1460623()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a2 = 1.0;
test2 = a2;
a2 = 3.0;
a2 = 3.3;
t2 = test2; // expected : 3.3; recieved : 3.0
a1 = [ 1.0, 2.0];
test1 = a1[1]; 
a1[1] = 3.0;
a1[1] = 3.3;
t1 = test1; // expected : 3.3; recieved : 3.0
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1578_T16_Defect_1460623_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a )
{
    return = a;
}
x = 1;
y = foo (x );
x = 2;
x = 3;
[Imperative]
{
    x = 4;
}
z = x;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1579_T16_Defect_1460623_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a )
{
    x = a;
	y = x + 3;
	x = a + 1;
	x = a + 2;
	return = y;
}
x = 1;
y = foo (x );
[Imperative]
{
    x = 2;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1582_T17_Defect_1459759_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1 = [ 1, 2 ];
y = a1[1] + 1;
a1[1] = 3;
a1 = 5;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1583_T18_Update_Variables_In_Inner_Assoc()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c = 2;
b = c * 2;
x = b;
[Imperative]
{
    c = 1;
	b = c + 1;
	d = b + 1;
	y = 1;
	[Associative]
	{
	  	b = c + 2;		
		c = 4;
		z = 1;
	}
}
b = c + 3;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1585_T20_Defect_1461391()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
[Imperative]
{
    a = 2.5;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1586_T20_Defect_1461391_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
def foo ( a1 : double[] )
{
    return = a1[0] + a1[1];
}
b = foo ( c ) ;
c = [ a, a ];
[Imperative]
{
    a = 2.5;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1587_T20_Defect_1461391_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( a ) ;
[Imperative]
{
   a = foo(2);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1590_T20_Defect_1461391_6()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( a : int) 
{
    return = a;
}
y1 = [ 1, 2 ];
y2 = foo ( y1);
[Imperative]
{ 
	count = 0;
	for ( i in y1)
	{
	    y1[count] = y1[count] + 1;	
        count = count + 1;		
	}
}
t1 = y2[0];
t2 = y2[1];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1591_T21_Defect_1461390()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;
[Associative]
{
    a = 0;
    d = a + 1;
    [Imperative]
    {
       b = 2 + a;
       a = 1.5;
              
    }
    c = a + 2; // fail : runtime assertion 
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1592_T21_Defect_1461390_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
b = a + 1;
[Imperative]
{
    a = 2;
    c = b + 1;
	b = a + 2;
    [Associative]
    {
       a = 1.5;
       d = c + 1;
       b = a + 3; 
       a = 2.5; 	   
    }
    b = a + 4;
    a = 3;	
}
f = a + b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1602_T25_Defect_1459759()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"p1 = 2;
p2 = p1+2;
p1 = true;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1603_T25_Defect_1459759_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1 = [ 1, 2 ];
y = a1[1] + 1;
a1[1] = 3;
a1 = 5;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1604_T25_Defect_1459759_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [ 2 , b ,3 ];
b = 3;
c = a[1] + 2;
d = c + 1;
b = [ 1,2 ];";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1605_T25_Defect_1459759_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"p2;
[Imperative]
{
	[Associative]
	{
		p1 = 2;
		p2 = p1+2;
		p1 = true;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1606_T25_Defect_1459759_5()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a;
[Imperative]
{
	a = 2;
	x = [Associative]
	{
		b = [ 2, 3 ];
		c = b[1] + 1;
		b = 2;
		return = c;
	}
	a = x;
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1607_T25_Defect_1459759_6()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
	def foo ( a, b )
	{
		a = b + 1;
		b = true;
		return = [ a , b ];
	}
e;
[Imperative]
{
	c = 3;
	d = 4;
	e = foo( c , d );
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1634_T28_Update_With_Inline_Condition()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x = 3;
a1 = 1;
a2 = 2;
a = x > 2 ? a1: a2;
a1 = 3;
a2 = 4;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1635_T28_Update_With_Inline_Condition_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x = 3;
a1 = [ 1, 2];
a2 = 3;
a = x > 2 ? a2: a1;
a2 = 5;
x = 1;
a1[0] = 0;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1638_T30_Update_Global_Variables_Imperative_Scope()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x  = [0,0,0,0];
count = 0;
i = 0;
sum  = 0;
test = sum;
[Imperative]
{
    for  ( i in x ) 
    {
       x[count] = count;
       count = count + 1;       
    }
    j = 0;
    while ( j < count )
    {
        sum = x[j]+ sum;
        j = j + 1;
    }
}
y = x;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1639_T30_Update_Global_Variables_Imperative_Scope_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"
count = 0;
i = 0;
sum  = 0;
test = sum;
[Imperative]
{
    for  ( i in x ) 
    {
       x[count] = count;
       count = count + 1;       
    }
    j = 0;
    while ( j < count )
    {
        sum = x[j]+ sum;
        j = j + 1;
    }
}
x  = [0,0,0,0];
y = x;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1649_T32_Update_With_Range_Expr()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"y = 1;
y1 = 0..y;
y = 2;
z1 = y1;                             
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1660_T37_Modify_Collections_Referencing_Each_Other()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [1,2,3];
b = a;
c1 = a[0];
b[0] = 10;
c2 = a[0];
testArray = a;
testArrayMember1 = c1;
testArrayMember2 = c2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1674_T42_Defect_1466071_Cross_Update_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"i = 5;
totalLength = 6;
[Associative]
{
	x = totalLength > i ? 1 : 0;
	
	[Imperative]
	{
		for (j in 0..3)
		{
			i = i + 1;
		}	
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1675_T42_Defect_1466071_Cross_Update_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"y = 1;
a = 2;
x = a > y ? 1 : 0;
y = [Imperative]
{
                while (y < 2) // create a simple outer loop
                {
                    y = y + 1;                              
                }
		return = y;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1676_T43_Defect_1463498()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c;d;
[Associative]
{
def foo : int ( a : int, b : int )
{
	a = a + b;
	b = 2 * b;
	return = a + b;
}
a = 1;
b = 2;
c = foo (a, b ); // expected 9, received -3
d = a + b;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1686_T001_implicit_programming_Robert()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"// no paradigm specified, so assume Associative
// some Associative code ....
a = 10;
b = a*2;
a = a +1; 	// expanded modifier, therefore the statement on line 7 is calculated after the statement on line 6 is excuted
c = 0;
//some Imperative code ....
[Imperative]
{
if (a>10) 	// implicit switch to Imperative paradigm
{
	c = b; 	// so statements are treated in lexical order, therefore the statement on line 13
	b=b/2;	// is executed before the statement on line 14 [as would be expected]
}
else
{
	[Associative] 	// explicit switch to Associative paradigm [overrides the Imperative paradigm]
	{
		c = b;    	// c references the final state of b, therefore [because we are in an Associative paradigm] 
		b = b*2;	// the statement on line 21 is executed before the statement on line 20
	}
}
}
// some more Associative code ....
a = a + 2;	// I am assuming that this statement (on line 27) is executed after the if..else has been evaluated and executed, because...
			// effectively, when a Imperative block is nested within an Associative block, lexical order plays a role
			// in that the execution order is:
			//			the part of the Associative graph before the Imperative block
			//			the Imperative block
			//			the part of the Associative graph after the Imperative block
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1687_T001_implicit_programming_Robert_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"// no paradigm specified, so assume Associative
// some Associative code ....
a = 10;
b = a*2;
a = a +1; 	// expanded modifier, therefore the statement on line 7 is calculated after the statement on line 6 is excuted
c = 0;
//some Imperative code ....
[Imperative]
{
	if (a>10) 	// explicit switch to Imperative paradigm
	{
		c = b; 	// so statements are treated in lexical order, therefore the statement on line 13
		b=b/2;	// is executed before the statement on line 14 [as would be expected]
	}
	else
	{
		[Associative] 	// explicit switch to Associative paradigm [overrides the Imperative paradigm]
		{
			c = b;    	// c references the final state of b, therefore [because we are in an Associative paradigm] 
			b = b*2;	// the statement on line 21 is executed before the statement on line 20
		}
	}
}
// some more Associative code ....
a = a + 2;	// I am assuming that this statement (on line 27) is executed after the if..else has been evaluated and executed, because...
			// effectively, when a Imperative block is nested within an Associative block, lexical order plays a role
			// in that the execution order is:
			//			the part of the Associative graph before the Imperative block
			//			the Imperative block
			//			the part of the Associative graph after the Imperative block
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1688_T002_limits_to_replication_1_Robert()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 0..10..2; 
b = a>5? 0:1; 
[Imperative]
{
	c = a * 2; // replication within an Imperative block [OK?]
	d = a > 5 ? 0:1; // in-line conditional.. operates on a collection [inside an Imperative block, OK?]
	if( c[2] > 4 ) x = 10; // if statement evaluates a single term [OK]
	
	if( c > 4 ) // but... replication within a regular 'if..else' any support for this?
	{
		y = 1;
	}
	else
	{
		y = -1;
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }


        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1690_T004_simple_order_1_Robert()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1 = 10;        // =1
b1 = 20;        // =1
a2 = a1 + b1;   // =3
b2 = b1 + a2;   // =3
b  = b2 + 2;    // 5
a  = a2 + b;    // 6";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1692_T006_grouped_1_Robert()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a1 = 10;        // =1
a2 = a1 + b1;   // =3
a  = a2 + b;    // 6    
    
b1 = 20;        // =1
b2 = b1 + a2;   // =3
b  = b2 + 2;    // 5";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1696_T010_imperative_if_inside_for_loop_1_Robert()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	x = 0;
	
	for ( i in 1..10..2)
	{
		x = i;
		if(i>5) x = i*2; // tis is ignored
		// if(i<5) x = i*2; // this causes a crash
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1700_Comments_1467117()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"/*
/*
*/
a=5;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1713_error_LineNumber_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a=b;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1715_FunctionCall_With_Default_Arg()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo:int(t : int = 4)
	{
		return = t;
	}
	// Type is recognized as A, actual type is B
testFoo1 = foo(6); // foo1 does not exist in A, function not found warning; testFoo1 =6;
testFoo2 = foo();";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1720_Use_Keyword_Array_1463672()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def Create2DArray( col : int)
{
	result = [Imperative]
    {
		array = [ 1, 2 ];
		counter = 0;
		while( counter < col)
		{
			array[counter] = [ 1, 2];
			counter = counter + 1;
		}
		return = array;
	}
    return = result;
}
x = Create2DArray( 2) ;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1721_Regress_1452951()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Associative]
{
	a = [ 4,5 ];
   
	[Imperative]
	{
	       //a = { 4,5 }; // works fine
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
	}
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1722_Regress_1454511()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
[Imperative]
{
	x = 0;
	
	for ( i in b )
	{
		x = x + 1;
	}
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1723_Regress_1454692()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"x;
y;
[Imperative]
{
	x = 0;
	b = 0..3; //{ 0, 1, 2, 3 }
	for( y in b )
	{
		x = y + x;
	}
	
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1724_Regress_1454692_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def length : int (pts : double[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }
        
        return = counter;
    }
    return = numPts;
}
    
arr = 0.0..3.0;//{0.0,1.0,2.0,3.0};
num = length(arr);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1725_Regress_1454918_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d;
[Associative] // expected 2.5
{
	 def Divide : double (a:int, b:int)
	 {
	  return = a/b;
	 }
	 d = Divide (5,2);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1726_Regress_1454918_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d;
[Associative] // expected error
{
	 def foo : int (a:double)
	 {
		  return = a;
	 }
	 d = foo (5.5);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1727_Regress_1454918_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d;
[Associative] // expected d = 5.0
{
	 def foo : double (a:double)
	 {
		  return = a;
	 }
	 d = foo (5.0);
} ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1728_Regress_1454918_4()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"d;
[Associative] // expected error
{
	 def foo : double (a:bool)
	 {
		  return = a;
	 }
	 d = foo (true);
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1730_Regress_1454918_6()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"     def foo : double ()
	 {
		  return = 5;
	 }
	 d = foo ();
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1731_Regress_1454926()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"result;
result2;
[Imperative]
{	 
	 d1 = null;
	 d2 = 0.5;	 
	 result = d1 * d2; 
	 result2 = d1 + d2; 
 
}";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1754_Regress_1455738()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"b;
[Associative]
{
    a = 3;
    b = a * 2;
    a = 4;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1765_Regress_1456713()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 2.3;
b = a * 3;
c = 2.32;
d = c * 3;
e1=0.31;
f=3*e1;
g=1.1;
h=g*a;
i=0.99999;
j=2*i;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1766_Regress_1456758()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = true && true ? -1 : 1;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1775_Regress_1457179()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import (""TestImport.ds"");
def Sin : double (val : double)
{
    return = dc_sin(val);
}
result1 = Sin(90);
result2 = Sin(90.0);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1777_Regress_1457885()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"c = 5..7..#1;
a = 0.2..0.3..~0.2;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1778_Regress_1457903()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1..7..#2.5;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1779_Regress_1458187()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"//b=true;
  //              x = (b == 0) ? b : b+1;
def foo1 ( b  )
{
                x = (b == 0) ? b : b+1;
                return = x;
}
a=foo1(5.0);
b=foo1(5);
c=foo1(0);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1780_Regress_1458187_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1 ( b )
{
x = (b == 0) ? b : b+1;
return = x;
}
a=foo1(true); ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1781_Regress_1458187_3()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo1 ( b )
{
x = (b == 0) ? b : b+1;
return = x;
}
a=foo1(null); ";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1782_Regress_1458475()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = [ 1,2 ];
b1 = a[-1];//b1=2";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1785_Regress_1458567()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"a = 1;
b = a[1];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1787_Regress_1458785_2()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo ( i:int[])
{
return = i;
}
x =  1;
a1 = foo(x);
a2 = 3;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1800_Regress_1459372()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"collection = [ 2, 2, 2 ];
collection[1] = 3;
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1801_Regress_1459512()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def length : int (pts : int[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = numPts;
}
z=length([1,2]);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1815_Regress_1462308()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"import(TestData from ""FFITarget.dll"");
f = TestData.IncrementByte(101); 
F = TestData.ToUpper(f);
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1816_Regress_1467091()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(x:int)
{
    return =  x + 1;
}
y1 = test.foo(2);
y2 = ding().foo(3);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1817_Regress_1467094_1()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"t = [];
x = t[3];
t[2] = 1;
y = t[3];
z = t[-1];
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1820_Regress_1467107()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def foo(x:int)
{
    return =  x + 1;
}
m=null;
y = m.foo(2);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1821_Regress_1467117()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"/*
/*
*/
a = 1;";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1822_test()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def length : int (pts : int[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = numPts;
}
z=length([1,2]);";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }

        [Test]
        [Category("WatchFx Tests")]
        public void DebugWatch1823_TestImport()
        {
            Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
            string src = @"def dc_sqrt : double (val : double )
{
    return = val/2.0;
}
def dc_factorial : int (val : int )
{
    return = val * val ;
}
def dc_sin : double (val : double)
{
    return = val + val;
}
";
            WatchTestFx.GeneratePrintStatements(src, ref map);
            WatchTestFx fx = new WatchTestFx(); fx.CompareRunAndWatchResults(null, src, map);
        }
    }
}
