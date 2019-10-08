using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.Imperative
{
    class ForLoop : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_TestNegativeSyntax_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
{
	a = { 4, 5, 3 };
	x = 0;
 
	for { y in a }
	{
	    x = x + 1;
    }
} ";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_TestNegativeUsage_InAssociativeBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
	a = { 2, 3 };
	x = 1;
	
	for( y in a )
	{
		x = x + 1;
	}
}
	";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_TestNegativeUsage_InUnnamedBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"{
	a = { 2,3 };
	x = 1;
	
	for( y in a )
	{
		x = x + 1;
	}
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_TestNegativeUsage_OutsideBlock_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"
	a = { 2,3 };
	x = 1;
	
	for( y in a )
	{
		x = x + 1;
	}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_TestForLoopInsideNestedBlocks()
        {
            string src = @"
x = [Associative]
{
	a = [ 4, 5 ];
	return [Imperative]
	{
		x = 0;
		b = [ 2,3 ];
		for( y in b )
		{
			x = y + x;
		}
        return x;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T06_TestInsideNestedBlocksUsingCollectionFromAssociativeBlock()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_TestForLoopUsingLocalVariable()
        {
            string src = @"
x = [Imperative]
{
	a = [ 1, 2, 3, 4, 5 ];
	x = 0;
	for( y in a )
	{
		local_var = y + x;	
        x = local_var + y;		
	}
	z = local_var;
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 30);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_TestForLoopInsideFunctionDeclaration()
        {
            string src = @"
def sum : double ( a : double, b : double, c : double )
{   
    return = [Imperative] {
		x = 0;
	    z = [a, b, c];
		for(y in z)
		{
			x = x + y;
		}
		return = x;
    }
}
i = [Imperative]
{
	y = sum ( 1.0, 2.5, -3.5 );
	z = sum ( -4.0, 5.0, 6.0 );
    return [y, z];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {0, 7});
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_TestForLoopWithBreakStatement()
        {
            string src = @"
x = [Imperative]
{
	a = [ 1,2,3 ];
	x = 0;
	for( i in a )
	{
		x = x + 1;
		break;
	}	
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_TestNestedForLoops()
        {
            string src = @"
x = [Imperative]
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
    return x;
}	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 18);
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_TestForLoopWithSingleton()
        {
            string src = @"
x = [Imperative]
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
    return x;
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_TestForLoopWith2DCollection()
        {
            string src = @"
i = [Imperative]
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
	return [x, z];	
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {10, 21});
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_TestForLoopWithNegativeAndDecimalCollection()
        {
            string src = @"
i = [Imperative]
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
	return [x, y];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {-9, 10.2});
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_TestForLoopWithBooleanCollection()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_TestForLoopWithMixedCollection()
        {
            string src = @"
i = [Imperative]
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
	return [x, y];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new object[] {-27, null});
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_TestForLoopInsideIfElseStatement()
        {
            string src = @"
a = [Imperative]
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
    return a;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_TestForLoopInsideNestedIfElseStatement()
        {
            string src = @"
a = [Imperative]
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
    return a;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T18_TestForLoopInsideWhileStatement()
        {
            string src = @"
x = [Imperative]
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
    return x;
}
			";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 15);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_TestForLoopInsideNestedWhileStatement()
        {
            string src = @"
x = [Imperative]
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
    return x;
}	
		
			";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 125);
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_TestForLoopWithoutBracket()
        {
            string src = @"
x = [Imperative]
{
	a = [ 1, 2, 3 ];
    x = 0;
	
	for( y in a )
	    x = y;
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_TestIfElseStatementInsideForLoop()
        {
            string src = @"
x = [Imperative]
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
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_TestWhileStatementInsideForLoop()
        {
            string src = @"
x = [Imperative]
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
    return x;
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 75);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_TestForLoopWithDummyCollection()
        {
            string src = @"
i = [Imperative]
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
    return [a6, a5, a4, a3, a2, a1];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {5, 4, 3, 2, 1, 0});
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_TestForLoopToModifyCollection()
        {
            string src = @"
i = [Imperative]
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
    return [a6, a7];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {7, 8});
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_TestForLoopEmptyCollection()
        {
            string src = @"
x = [Imperative]
{
	a = [];
	x = 0;
	for( i in a )
	{
		x = x + 1;
	}
	return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_TestForLoopOnNullObject()
        {
            string src = @"
x = [Imperative]
{
	x = 0;
	
	for ( i in b )
	{
		x = x + 1;
	}
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T27_TestCallingFunctionInsideForLoop()
        {
            string src = @"
	def function1 : double ( a : double )
	{		
		return = a + 0.7;
	}
x = [Imperative]
{
	a = [ 1.3, 2.3, 3.3, 4.3 ];
	
	x = 3;
	
	for ( i in a )
	{	
		x = x + function1( i );
	}
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 17);
        }

        [Test]
        [Category("SmokeTest")]
        public void T28_Defect_1452966()
        {
            string src = @"
x = [Imperative]
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
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 18);
        }

        [Test]
        [Category("SmokeTest")]
        public void T29_Defect_1452966_2()
        {
            string src = @"
x = [Imperative]
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
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 21);
        }

        [Test]
        [Category("SmokeTest")]
        public void T30_ForLoopNull()
        {
            string src = @"
x = [Imperative]
{
	a = [ 1,null,null ];
	x = 1;
	
	for( i in a )
	{
		x = x + 1;
	}
    return x;
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_ForLoopSyntax01_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    string src = @"[Imperative]
{
	a = { 1,2,3,4 };
	
	x = 0;
	
	for ( (i in a) )
	{
		x = x + i;
	}
}";
                    ExecutionMirror mirror = thisTest.RunScriptSource(src);
                });
        }

        [Test]
        [Category("SmokeTest")]
        public void T32_ForLoopSyntax02_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
{
	a = { 1,2,3,4 };
	
	x = 0;
	
	for  (i in a) 
	{
	{
		x = x + i;
	}
	}
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T33_ForLoopToReplaceReplicationGuides()
        {
            string code = @"
a = [ 1, 2 ];
b = [ 3, 4 ];
//c = a<1> + b <2>;
dummyArray = [ [ 0, 0 ], [ 0, 0 ] ];
counter1 = 0;
counter2 = 0;
dummyArr = [Imperative]
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
	return dummyArray;
}
a1 = dummyArr[0][0];
a2 = dummyArr[0][1];
a3 = dummyArr[1][0];
a4 = dummyArr[1][1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 4, 0);
            thisTest.Verify("a2", 5, 0);
            thisTest.Verify("a3", 5, 0);
            thisTest.Verify("a4", 6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Defect_1452966()
        {
            string code = @"
sum = [Imperative]
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
    return sum;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 40, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T35_Defect_1452966_2()
        {
            string code = @"
sum = [Imperative]
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
    return sum;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 21, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T36_Defect_1452966_3()
        {
            string code = @"
b;
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
[Associative]
{
	b =forloop(a);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 21, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T37_Defect_1454517()
        {
            string code = @"
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
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 9, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T38_Defect_1454517_2()
        {
            string code = @"
	a = [ 4,5 ];
	x = [Imperative]
	{
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
        return x;
	}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 9, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T38_Defect_1454517_3()
        {
            string code = @"
def foo ( a : int [] )
{
    x = 0;
	x = [Imperative]
	{	
		for( y in a )
		{
			x = x + y;
		}
		return x;
	}
	return x;
}
a = [ 4,5 ];
b = [Imperative]
{
	return foo(a);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 9);

        }

        [Test]
        [Category("SmokeTest")]
        public void T39_Defect_1452951()
        {
            string code = @"
x = [Associative]
{
	a = [ 4,5 ];
   
	return [Imperative]
	{
	    //a = { 4,5 }; // works fine
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
        return x;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 9);


        }

        [Test]
        [Category("SmokeTest")]
        public void T39_Defect_1452951_1()
        {
            string code = @"
def foo ( a : int[])
{
    a[1] = 4;
	return a;
}
x = [Imperative]
{
	a = [ 4,5 ];

	return [Associative]
	{
	   return foo(a);
	}

};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 4, 4 };
            thisTest.Verify("x", expectedResult);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T39_Defect_1452951_2()
        {
            string code = @"
	def foo :int[] (a1:int[])
	{
	    count = 0;
		return [Imperative]
		{
			for ( i in a1 )
			{
				a1[count]  = i + 1;
				count = count + 1;
			}
            return a1;
		}
	}
   a = [ 10, 4 ];
    a3 = [Imperative]
    {
    	return foo(a);
    }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 11, 5 };
            thisTest.Verify("a3", expectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void T40_Create_3_Dim_Collection_Using_For_Loop()
        {
            string code = @"
x = [ [ [ 0, 0] , [ 0, 0] ], [ [ 0, 0 ], [ 0, 0] ]];
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);
            thisTest.Verify("p9", 0, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T41_Create_3_Dim_Collection_Using_For_Loop_In_Func_Call()
        {
            string code = @"
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T43_Create_3_Dim_Collection_Using_For_Loop_In_Function()
        {
            string code = @"

	def foo :int[]..[](a:int[],b:int[],c:int[] )
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
	};
a = [ 0, 1 ];
b = [ 2, 3];
c = [ 4, 5 ];
y = foo(a,b,c);

p1 = y[0][0][0];
p2 = y[0][0][1];
p3 = y[0][1][0];
p4 = y[0][1][1];
p5 = y[1][0][0];
p6 = y[1][0][1];
p7 = y[1][1][0];
p8 = y[1][1][1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Array")]
    
        public void T43_Create_CollectioninForLoop_1457172()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4081
            string errmsg = "MAGN-4081: Nested forloop fails in a constructor";
            string src = @"
def foo :int[]..[]( a : int[], b:int[])
{
y = [Imperative]
{
x = [ [ 0,0,0 ], [0,0,0] , [0,0,0] ];
c1 = 0;
for ( i in a)
{
c2 = 0;
for ( j in b )
{
x[c1][c2] = i + j ;
c2 = c2+ 1;
}
c1 = c1 + 1;
}
return = x;
}
return = y;
}

a = [ 0, 1, 2 ];
b = [ 3, 4, 5 ];
y = foo( a, b);
p1 = y[0][0];
p2 = y[0][1];
p3 = y[0][2];
p4 = y[1][0];
p5 = y[1][1];
p6 = y[1][2];
p7 = y[2][0];
p8 = y[2][1];
p9 = y[2][2];
";
            thisTest.VerifyRunScriptSource(src, errmsg);
            thisTest.Verify("p1", 3, 0);
            thisTest.Verify("p2", 4, 0);
            thisTest.Verify("p3", 5, 0);
            thisTest.Verify("p4", 4, 0);
            thisTest.Verify("p5", 5, 0);
            thisTest.Verify("p6", 6, 0);
            thisTest.Verify("p7", 5, 0);
            thisTest.Verify("p8", 6, 0);
            thisTest.Verify("p9", 7, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Array")]
        public void T43_Create_CollectioninForLoop_1457172_2()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4081
            string errmsg = "MAGN-4081: Nested forloop fails in a constructor";
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
y = foo( a, b , c);

p1 = y[0][0][0]; //6
p2 = y[0][0][1];//7
p3 = y[0][1][0];//7
p4 = y[0][1][1];//8
p5 = y[1][0][0];//7
p6 = y[1][0][1];//8
p7 = y[1][1][0];//8
p8 = y[1][1][1];//9
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(src, errmsg);
            thisTest.Verify("p1", 6, 0);
            thisTest.Verify("p2", 7, 0);
            thisTest.Verify("p3", 7, 0);
            thisTest.Verify("p4", 8, 0);
            thisTest.Verify("p5", 7, 0);
            thisTest.Verify("p6", 8, 0);
            thisTest.Verify("p7", 8, 0);
            thisTest.Verify("p8", 9, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T44_Use_Bracket_Around_Range_Expr_In_For_Loop()
        {
            string code = @"
s = [Imperative] {
    s = 0;
    for (i in (0..10)) {
	    s = s + i;
    }
    return s;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("s", 55);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T45_Defect_1458284()
        {
            string code = @"
			import(""FFITarget.dll"");

    def length (pts : DummyPoint[])
    {
        numberOfPoints = [Imperative]
        {
            counter = 0;
            for(pt in pts)
            {
                counter = counter + 1;
            }
            
            return = counter;
        }
        return = numberOfPoints;
    }

pt1 = Point.ByCoordinates(0,0,0);
pt2 = Point.ByCoordinates(5,0,0);
pt3 = Point.ByCoordinates(10,0,0);
pt4 = Point.ByCoordinates(15,0,0);
pt5 = Point.ByCoordinates(20,0,0);
pts = [pt1, pt2, pt3, pt4, pt5];

numpts = length(pts);

";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 5);
        }



    }
}
