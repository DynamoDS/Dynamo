using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class Builtin_Functions : ProtoTestBase
    {
        
        [Test]
        public void test()
        {
            string code = @"import(""DSCoreNodes.dll"");
a = [];
b = Math.Average(a);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

       
        [Test]
        [Category("SmokeTest")]
        public void T012_CountTrue_IfElse()
        {
            string code = @"import(""DSCoreNodes.dll"");
result =
[Imperative]
{
	arr1 = [true,[[[[true]]]],null];
	arr2 = [[true],[false],null];
	if(List.CountTrue(arr1) > 1)
	{
		arr2 = arr1;
	}
	return = List.CountTrue(arr2);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T013_CountTrue_ForLoop()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = 
[Imperative]
{
	a = [1,3,5,7,[]];
	b = [null,null,[1,true]];
	c = [List.CountTrue([1,null])];
	
	d = [a,b,c];
	j = 0;
	e = [];
	
	for(i in d)
	{
		e[j]= List.CountTrue(i);
		j = j+1;
	}
	return  = e;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T014_CountTrue_WhileLoop()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = 
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
		e[j]= List.CountTrue(d[i]);
		i = i+1;
		j = j+1;
	}
	return = e ;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T015_CountTrue_Function()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo(x:var[]..[])
{
	a = [];
	i = 0;
	return [Imperative]
	{
		for(j in x)
		{
			a[i] = List.CountTrue(j);
			i = i+1;
		}
        return a;
	}
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 0, 3, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T016_CountTrue_Class()
        {
            string code = @"import(""DSCoreNodes.dll"");
def create (x:var[]..[])
{
	return = List.CountTrue(x);
}
	
def foo(y:var[]..[], a : int)
{
	return = List.CountTrue(y)+ a;
}
b = [1, null, true,[[true],false]];
m = create(b);
n = foo(b, m);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("m", 2, 0);
            thisTest.Verify("n", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T017_CountTrue_Inline()
        {
            string code = @"import(""DSCoreNodes.dll"");

def foo(x:var[]..[])
{
    return [Imperative]
    {
	    if(List.CountTrue(x) > 0)
		    return = true;
	    return = false;
    }
}
result = [Imperative]
{
    a = [null,1];//0
b = [null,20,30,null,[10,0],true,[false,0,[true,[false],5,2,false]]];//2
c = [1,2,foo(b)];
    return List.CountTrue(c) > 0 ? List.CountTrue(a):List.CountTrue(b);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T018_CountTrue_RangeExpression_01()
        {
            string code = @"import(""DSCoreNodes.dll"");

i = [Imperative]
{
	a1 = [1,true, null];//1
	a2 = 8;
	a3 = [2,[true,[true,1]],[false,x, true]];//3
	a = List.CountTrue(a1)..a2..List.CountTrue(a3);//{1,4,7}
	
	result = List.CountTrue(a);
    return [result, a];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            var v1 = new Object[] { 1, 4, 7 };

            thisTest.Verify("i", new object[] {0, v1});
        }
        /* 
[Test]
         public void T018_CountTrue_RangeExpression_02()
         {
           Assert.Fail("");
           ExecutionMirror mirror = thisTest.RunScript(testPath, "T018_CountTrue_RangeExpression_02.ds");
           thisTest.Verify("result", 0);
           List<Object> li = new List<Object>() { 1, 4, 9 };
           Assert.IsTrue(mirror.CompareArrays("a", li, typeof(System.Int64)));
         }*/

        [Test]
        [Category("SmokeTest")]
        public void T018_CountTrue_RangeExpression_03()
        {
            //Assert.Fail("");
            string code = @"import(""DSCoreNodes.dll"");
a = [Imperative]
{
	a1 = [1,true, null];//1
//1;
	a2 = 8;
	a3 = [2,[true,[true,1]],[false,x, true]];//3
//3;

	return List.CountTrue(a1)..a2..#List.CountTrue(a3);

};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 4.5, 8 };
            thisTest.Verify("a", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T019_CountTrue_Replication()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo(x:int)
{
	return = x +1;
}
a = [true,[true],1];//2
b = [null];
c = [[[true]]];//1
d = [[true],[false,[true,true]]];//3
arr = [List.CountTrue(a),List.CountTrue(b),List.CountTrue(c),List.CountTrue(d)];
result = foo(arr);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 3, 1, 2, 4 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T020_CountTrue_DynamicArray()
        {
            string code = @"import(""DSCoreNodes.dll"");

i = [Imperative]
{
	a1 = [
	[[true],[]],
	[a,2,[false]],
	[a,b,null],//{null,null,null}
	[null]
	];
	a2 = [];
	i = 0;
	j = 0; 
	while(i < List.CountTrue(a1))
	{
		if(List.CountTrue(a1[i])>0)
		{
			a2[j] = a1[i];
			j = j+1;
			
		}
		i = i+1;
	}
	return [a2, List.CountTrue(a2)];
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { true };
            Object[] v2 = new Object[] { };
            Object[] v3 = new Object[] { v1, v2 };
            Object[] v4 = new Object[] { v3 };
            thisTest.Verify("i", new object[] { v4, 1 });
        }
        [Test]
        [Category("SmokeTest")]
        public void T022_CountTrue_ImperativeAssociative()
        {
            string code = @"import(""DSCoreNodes.dll"");

i = [Imperative]
{
	a1 = [true,0,1,1.0,null];           // {true, 0, 1, 1, null}
	a2 = [false, List.CountTrue(a1),0.0];    // {false, 1, 0}
	a3 = a1;                            // {true, 0, 1, 1, null}
	b = [Associative]
	{
		a1 = [true,[true]];             // {true,{true}}
		a4 = a2;                        // {true}
		a2 = [true];                    // {true}
		return List.CountTrue(a4);              // 1
	}
	
	c = List.CountTrue(a3);                  //1
	return [b, c];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] { 1, 1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T023_CountFalse_IfElse()
        {
            string code = @"import(""DSCoreNodes.dll"");
result =
[Imperative]
{
	arr1 = [false,[[[[false]]]],null,0];
	arr2 = [[true],[false],null,null];
	if(List.CountFalse(arr1) > 1)
	{
		arr2 = arr1;
	}
	return = List.CountFalse(arr2);//2
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T024_CountFalse_ForLoop()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = 
[Imperative]
{
	a = [1,3,5,7,[]];
	b = [null,null,[0,false]];
	c = [List.CountFalse([[false],null])];
	
	d = [a,b,c];
	j = 0;
	e = [];
	
	for(i in d)
	{
		e[j]= List.CountFalse(i);
		j = j+1;
	}
	return  = e;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T025_CountFalse_WhileLoop()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = 
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
		e[j]= List.CountFalse(d[i]);
		i = i+1;
		j = j+1;
	}
	return = e ;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T026_CountFalse_Function()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo(x:var[]..[])
{
	a = [];
	i = 0;
	return [Imperative]
	{
		for(j in x)
		{
			a[i] = List.CountFalse(j);
			i = i+1;
		}
        return a;
	}
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 0, 3, 0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T028_CountFalse_Inline()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo(x:var[]..[])
{
    return = [Imperative]
    {
	    if(List.CountFalse(x) > 0)
		    return true;
	    return false;
    }
}
result = [Imperative]
{
    a = [null,0];//0
b = [null,20,30,null,[10,0],false,[true,0,[true,[false],5,2,true]]];//2
c = [1,2,foo(b)];
    return List.CountFalse(c) > 0 ? List.CountFalse(a):List.CountFalse(b);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T029_CountFalse_RangeExpression_01()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = [Imperative]
{
	a1 = [0,false, null];//1
	a2 = 8;
	a3 = [2,[false,[false,1]],[false,x, true]];//3
	a = List.CountFalse(a1)..a2..List.CountFalse(a3);//{1,4,7}
	
	return [List.CountFalse(a), a];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 4, 7 };

            thisTest.Verify("result", new object[] { 0, v1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T029_CountFalse_RangeExpression_02()
        {
            //Assert.Fail("");
            string code = @"import(""DSCoreNodes.dll"");
a = [Imperative]
{
	a1 = [1,false, null];//1
	a2 = 8;
	a3 = [2,[false,[false,1]],[false,x, true]];//3
	return List.CountFalse(a1)..a2..#List.CountFalse(a3);//{}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 4.5, 8 };
            thisTest.Verify("a", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T030_CountFalse_Replication()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo(x:int)
{
	return = x +1;
}
a = [false,[false],0];//2
b = [List.CountFalse([a[2]])];
c = [[[false]]];//1
d = [[false],[false,[true,false,0]]];//3
arr = [List.CountFalse(a),List.CountFalse(b),List.CountFalse(c),List.CountFalse(d)];
result = foo(arr);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 3, 1, 2, 4 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T031_CountFalse_DynamicArray()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = 
[Imperative]
{
	a1 = [
	[[false],[]],
	[a,2,[true]],
	[a,b,null],//{null,null,null}
	[null]
	];
	a2 = [];
	i = 0;
	j = 0; 
	while(i < List.CountFalse(a1))
	{
		if(List.CountFalse(a1[i])>0)
		{
			a2[j] = a1[i];
			j = j+1;
			
		}
		i = i+1;
	}
	return = [a2, List.CountFalse(a2)];
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { false };
            Object[] v2 = new Object[] { };
            Object[] v3 = new Object[] { v1, v2 };
            Object[] v4 = new Object[] { v3 };
            thisTest.Verify("result", new object[] { v4, 1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T033_CountFalse_ImperativeAssociative()
        {
            string code = @"import(""DSCoreNodes.dll"");
i = 
[Imperative]
{
	a1 = [false,0,1,1.0,null];
	a2 = [true, List.CountFalse(a1),0.0];
	a3 = a1;
	b = [Associative]
	{
		a1 = [false,[false]];
		a4 = a2;
		a2 = [false];
		return List.CountFalse(a4);//1
	}
	
	c = List.CountFalse(a3);//1
	return [b, c];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] { 1, 1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T034_AllFalse_IfElse()
        {
            string code = @"import(""DSCoreNodes.dll"");
a = [false, false];//true
b = [[false]];//true
c = [false, 0];//false
result = [Imperative]
{
    result = {};
	if(List.AllFalse(a)){
		a[2] = 0;
		result[0] = List.AllFalse(a);//false
	} 
	if(!List.AllFalse(b)){
		
		result[1] = List.AllFalse(b);//false
	}else
	{result[1]= null;}
	if(!List.AllFalse(c)){
		result[2] = List.AllFalse(c);
	}
    return result;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { false, null, false };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T035_AllFalse_ForLoop()
        {
            string code = @"import(""DSCoreNodes.dll"");
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
		if(List.AllFalse(i)!=true){
			f[j] = List.AllFalse(i);
			j = j+1;
		}
		
	}
return = f;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { false, false, false };
            thisTest.Verify("result", v1, 0);
            //Assert.Fail("1467074 - Sprint23 : rev :2650 : Built-in function List.AllFalse() doesn't behave as expected ");
        }

        [Test]
        [Category("SmokeTest")]
        public void T036_AllFalse_WhileLoop()
        {
            string code = @"import(""DSCoreNodes.dll"");
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
	while(!List.AllFalse(e[i])&& i < Count(e))
	{	
		if(List.AllFalse(e[i])!=true){
			f[j] = List.AllFalse(e[i]);
			j = j+1;
		}
		i = i+1;
		
	}
return = f;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { false, false};
            thisTest.Verify("result", v1, 0);
            //Assert.Fail("1467071 - Sprint23 : rev 2635 : Build-in function List.AllFalse issue : When the array is empty,it returns true");
        }

        [Test]
        [Category("SmokeTest")]
        public void T036_1_Null_Check()
        {
            // Assert.Fail("1467095 - Sprint24 : rev :2747 : when 'false' is expected in a verification, if it's 'null' instead, the test case should not pass");
            string code = @"
result = null;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //thisTest.Verify("result", false, 0)
            thisTest.Verify("result", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T037_AllFalse_Function()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo( x : bool)
{	
	return = !x;
}
a1 = [0];
a2 = [null];
a3 = [!true];
b = [a1,a2,a3];
result = [foo(List.AllFalse(a1)),foo(List.AllFalse(a2)),foo(List.AllFalse(a3))];//true,true,false
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { true, true, false };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T039_AllFalse_Inline()
        {
            string code = @"import(""DSCoreNodes.dll"");
a1 = [false,[false]];
a = List.AllFalse(a1);//true
b1 = [null,null];
b = List.AllFalse(b1);//false
c = List.AllFalse([b]);//t
result = a? c:b;//t";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", true, 0);
            thisTest.Verify("b", false, 0);
            thisTest.Verify("c", true, 0);
            thisTest.Verify("result", true, 0);
        }
        /* 
[Test]
         public void T040_AllFalse_Replication()
         {
           ExecutionMirror mirror = thisTest.RunScript(testPath, "T040_AllFalse_Replication.ds");
       
           thisTest.Verify("result", true, 0);
         }p*/

        [Test]
        [Category("SmokeTest")]
        public void T042_AllFalse_DynamicArray()
        {
            string code = @"import(""DSCoreNodes.dll"");
b = [];
a = [[true],[false],[false],
	[false,[true,false]]];
	
	i = 0;
	result2 = 
	[Imperative]
	{
		while(i<Count(a))
		{
			b[i] = List.AllFalse(a[i]);
			i = i+1;
		}
		return = b;
	}
	result = List.AllFalse(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { false, true, true, false };
            thisTest.Verify("result", false, 0);
            thisTest.Verify("result2", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T044_AllFalse_ImperativeAssociative()
        {
            string code = @"import(""DSCoreNodes.dll"");
m;
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
		
		m = List.AllFalse(c);//f
		n = List.AllFalse(d);//t
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("n", true, 0);
            thisTest.Verify("m", false, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T045_Defect_CountArray_1()
        {
            string code = @"
a = [0,1,null];
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 3, 3, 4 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("Array")]
        public void T045_Defect_CountArray_2()
        {
            string code = @"
result=
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
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T045_Defect_CountArray_3()
        {
            string code = @"
a = [];
b = [null,1+2];
a[0] = b;
a[1] = b[1];
result = Count(a);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T046_Sum_IfElse()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4103
            string code = @"import(""DSCoreNodes.dll"");
result = 
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
	
	if(Math.Sum(a)>=0) m[0] = Math.Sum(a);	
	if(Math.Sum(b)>=0) m[1] = Math.Sum(b);
	if(Math.Sum(c)>=0) m[2] = Math.Sum(c);
	if(Math.Sum(d)>=0) m[3] = Math.Sum(d); 
	if(Math.Sum(e)>=0) m[4] = Math.Sum(e);
	if(Math.Sum(f)>=0) m[5] = Math.Sum(f);
	if(Math.Sum(g)>=0) m[6] = Math.Sum(g);
	
	return = m;
}
";
            string err = "MAGN-4103 Type coercion issue from conversion of bool, null, empty arrays to numbers";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 10, 10.0, 10.0, -1, 10, 11, -1 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest"), Category("Failure")]
        public void T047_Sum_ForLoop()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4103
            string code = @"import(""DSCoreNodes.dll"");
result = 
[Imperative]
{
	a = [0,0.0];
	b = [[]];
	c = [m,Math.Sum(a),b,10.0];
	
	d = [a,b,c];
	j = 0;
	
	for(i in d)
	{
		d[j] = Math.Sum(i);
		j = j+1;
	}
	
	return = d; 
}";
            string err = "MAGN-4103 Type coercion issue from conversion of bool, null, empty arrays to numbers";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 0, 0, 10.0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest"), Category("Failure")]
        public void T048_Sum_WhileLoop()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = 
[Imperative]
{
	a = [-2,0.0];
	b = [[]];
	c = [m,Math.Sum(a),b,10.0];
	
	d = [a,b,c];
	j = 0;
	k = 0;
	e = [];
	
	while(j<Count(d))
	{
		if(Math.Sum(d[j])!=0)
		{
			e[k] = Math.Sum(d[j]);
			k = k+1;
		}
		j = j+1;
	}
	
	return = e; 
}";
            string err = "MAGN-4103 Type coercion issue from conversion of bool, null, empty arrays to numbers";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { -2.0, 8.0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("Failure")]
        [Category("SmokeTest")]
        public void T049_Sum_Function()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo(x:var[])
{
	return =
	[Imperative]
	{
		return = Math.Sum(x);
	}
}
a = [-0.1,true,[],null,1];
b = [m+n,[[[1]]]];
c = [Math.Sum(a),Math.Sum(b)];
result = foo(c);
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4171
            string errmsg = "MAGN-4171: Replication method resolution";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, errmsg);
            thisTest.Verify("result", 1.9, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Sum_Inline()
        {
            string code = @"import(""DSCoreNodes.dll"");
a = [1,[2,-3.00]];//0.0
sum = Math.Sum(a);
b = Math.Sum(a) -1;//-1.0
c = Math.Sum([a,b,-1]);//-2.0;
result = Math.Sum(a)==0&& b==-1.00? b :c;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", -1, 0);
        }

        [Test]
        [Category("SmokeTest"), Category("Failure")]
        public void T052_Sum_RangeExpression()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = 
[Imperative]
{
	a1 = [1,true, null];//1
	a2 = 8;
	a3 = [2,[true,[true,1.0]],[false,x, true]];//3.0
	a = Math.Sum(a1)..a2..Math.Sum(a3);//{1,4,7}
	
	return = Math.Sum(a);//12.0
}";
            string err = "MAGN-4103 Type coercion issue from conversion of bool, null, empty arrays to numbers";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("result", 12.0, 0);
        }
        /*
[Test]
        [Category("SmokeTest")]
        public void T053_Sum_Replication()
        {
          ExecutionMirror mirror = thisTest.RunScript(testPath, "T053_Sum_Replication.ds");
          thisTest.Verify("result", 12.0, 0);
        }*/

        [Test]
        [Category("SmokeTest")]
        public void T054_Sum_DynamicArr()
        {
            string code = @"import(""DSCoreNodes.dll"");
a = [];
b = [1.0,2,3.0];
c = [null,m,""1""];
a[0]=Math.Sum(b);//6.0
a[1] = Math.Sum(c);//0
a[2] = Math.Sum([a[0],a[1]]);//6.0
result = Math.Sum(a);//12.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 12.0, 0);
        }

        [Test]
        [Category("SmokeTest"), Category("Failure")]
        public void T056_Sum_AssociativeImperative()
        {
            string code = @"import(""DSCoreNodes.dll"");
a = [1,0,0.0];
b = [2.0,0,true];
b1 = [b,1];
[Imperative]
{
	c = a[2];
	a[1] = 1;
	m = a;
	sum1 = Math.Sum([c]);//0.0
	[Associative]
	{
		 b[1] = 1;
		 sum2 = Math.Sum( b1);////4.0
	}
	
	a[2]  =1;
}
";
            string err = "MAGN-4103 Type coercion issue from conversion of bool, null, empty arrays to numbers";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("sum1", 0.0, 0);
            thisTest.Verify("sum2", 4.0, 0);
            thisTest.Verify("sum3", 0.0, 0);
        }
        //datatype

        [Test]
        [Category("Design Issue"), Category("Failure")]
        public void T057_Average_DataType_01()
        {
            string code = @"import(""DSCoreNodes.dll"");
a = [];
b = [1,2,3];
c = [0.1,0.2,0.3,1];
d = [true, false, 1];
a1 = Math.Average(a);
b1 = Math.Average(b);
c1 = Math.Average(c);
d1 = Math.Average(d);";

            string err = "MAGN-4103 Type coercion issue from conversion of bool, null, empty arrays to numbers";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            Object v1 = null;
            //Assert.Fail("1467164 - Sprint 25 - Rev 3125: Built-in function: Average() should ignore the elements which can't be converted to int/double in the array");
            thisTest.Verify("a1", v1, 0);
            thisTest.Verify("b1", 2.0, 0);
            thisTest.Verify("c1", 0.4, 0);
            thisTest.Verify("d1", 1.0, 0);//significant digits
        }

        [Test]
        [Category("SmokeTest")]
        public void T059_Defect_Flatten_RangeExpression()
        {
            string code = @"
a = 0..10..5;
b = 20..30..2;
c = [a, b];
d = List.Flatten([a,b]);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 5, 10, 20, 22, 24, 26, 28, 30 };
            thisTest.Verify("d", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T059_Defect_Flatten_RangeExpression_1()
        {
            string code = @"
a = [[null]];
b = [1,2,[3]];
c = [a,b];
d = List.Flatten(c);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { null, 1, 2, 3 };
            thisTest.Verify("d", v1, 0);
        }

        [Test]
        [Category("Design Issue"), Category("Failure")]
        public void T060_Average_ForLoop()
        {
            string code = @"import(""DSCoreNodes.dll"");
result = 
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
		e[j] = Math.Average(i);
		 j = j+1;
		
	}
	return = e;
	
}";
            string err = "MAGN-4103 [Design Issue]: Type coercion issue from conversion of bool, null, empty arrays to numbers";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 0.0, 1.5, 10.0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T061_Average_Function()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo : double (x :var[]..[])
{
	
	return = Math.Average(x);
}
a = [1,2,2,1];
b = [1,[]];
c = Math.Average(a);
result = [foo(a),foo(b)];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1.5, 1.0 };
            thisTest.Verify("result", v1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T062_Average_Function()
        {
            string code = @"import(""DSCoreNodes.dll"");
def foo(m:var[]..[], n:var[]..[])
{
	return = Math.Average([m,n]);
}
a = [1,[2],[[[3]]]];
b = [0.1,2,0];
x = Math.Average(a);
y = Math.Average(b);
n = foo(x,y);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2.0, 0);
            thisTest.Verify("y", 0.7, 0);
            thisTest.Verify("n", 1.35, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T063_Average_Inline()
        {
            string code = @"import(""DSCoreNodes.dll"");
a = [1.0,2];
b = [[0],1.0,[2]];
result = Math.Average(a)>Math.Average(b)?true:false;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T064_Average_RangeExpression()
        {
            string code = @"import(""DSCoreNodes.dll"");
a = 0..6..3;//0,3,6
b = 0..10..~3;//0,3.3,6.6,10
m = Math.Average(a);//3
n = Math.Average(b);//5.0
c = Math.Average([m])..Math.Average([n]);//3.0,4.0,5.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 3.0, 4.0, 5.0 };
            thisTest.Verify("m", 3.0, 0);
            thisTest.Verify("n", 5.0, 0);
            thisTest.Verify("c", v1, 0);
        }
        
        [Test]
        [Category("Built in Functions")]
        public void T068_Average_Negative_Cases()
        {
            String code =
 @"
//x1 = Math.Average(a) ;// returns null, also throws runtime error ? 
x2 = Math.Average(a) ;// returns -1
//x3 = Math.Average(()); // returns null, also throws runtime error ?
x4 = Math.Average(null) ;// returns -1
x5 = Math.Average([]) ;// returns 0.0
x6 = Math.Average([null]) ;// returns 0.0
";
            Object n1 = null;
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "DNL-1467301 rev 3778 : Builtin method 'Average' should return null for all negative cases";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x2", n1);
            thisTest.Verify("x4", n1);
            thisTest.Verify("x5", n1);
            thisTest.Verify("x6", n1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void CountInClass_1467364()
        {
            String code =
                @"
                def foo()
                {
                    y : int[];
                    z;
                    y = [ 1, 2, 3, 4, 5 ];
                    rows = [ 1, 2, 3 ];
                    return = y[Count(rows)];
                }
                c = foo();
                ";
            string error = "1467364 Sprint 27 - Rev 4053 if built-in function is used to index into an array inside class , compile error is thrown ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("c", 4);
        }

        [Test]
        [Category("Built in Functions")]
        public void T068_Sort_UsingFunctionPointer()
        {
            string code = @"
            def Compare(x : bool, y : bool)
            {
                return = [Imperative]
                {
                    if (x == y)
                        return = 0;
        
                    return = !x ? -1 : 1;
                }
            }
            def Compare(x : int, y : bool)
            {
                return = 1;
            }
            def Compare(x : bool, y : int)
            {
                return = -1;
            }
            def Compare(x : int, y : int)
            {
                return = (x - y);
            }
            arr = [ 3, 5, 1, true, false, 5, 3, 0, 4, 7, true, 5, false, 12];
            sorted = Sort(Compare, arr);
           ";
            string error = "T068_Sort_UsingFunctionPointer failed!!";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("sorted", new Object[] { false, false, true, true, 0, 1, 3, 3, 4, 5, 5, 5, 7, 12 });
        }

        [Test]
        [Category("Built in Functions"), Category("Failure")]
        public void TV_1467348_Rank()
        {
            // TODO pratapa: List.Rank() should throw method not found warning
            string code = @"
import(""BuiltIn.ds"");
r1 = List.Rank(); //null
r2 = List.Rank(1);//0
r3 = List.Rank([ ]);//1
r4 = List.Rank([ [  ] ]);//2
r5 = List.Rank([ [ ""123"" ] ]);//2
               ";
            thisTest.RunScriptSource(code);
            //thisTest.SetErrorMessage("1467348 - Language: Rank(3) should return 0");
            thisTest.Verify("r1", null);
            thisTest.Verify("r2", 0);
            thisTest.Verify("r3", 1);
            thisTest.Verify("r4", 2);
            thisTest.Verify("r5", 2);
        }

        [Test]
        [Category("Built in Functions")]
        public void TV_1467322_CountTrue_1()
        {
            string code = @"
                a = [ [ ], true, false ];//1
b = [ 1, true ];//1
c = [ ""c"" ];//0
d = [ 0 ];//0
e = [ [ true ], true ]; //2
f = [ ];//0
g = 1; //0
h = null; //null
i = [ [ ] ]; //0
j = [ null ]; //0
k = ""string"";//0
ra = List.CountTrue(a);
rb = List.CountTrue(b);
rc = List.CountTrue(c);
rd = List.CountTrue(d);
re = List.CountTrue(e);
rf = List.CountTrue(f);
rg = List.CountTrue(g);
rh = List.CountTrue(h);
ri = List.CountTrue(i);
rj = List.CountTrue(j);
rk = List.CountTrue(k);
               ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ra", 1);
            thisTest.Verify("rb", 1);
            thisTest.Verify("rc", 0);
            thisTest.Verify("rd", 0);
            thisTest.Verify("re", 2);
            thisTest.Verify("rf", 0);
            thisTest.Verify("rg", 0);
            thisTest.Verify("rh", 0);
            thisTest.Verify("ri", 0);
            thisTest.Verify("rj", 0);
            thisTest.Verify("rk", 0);
        }

        [Test]
        [Category("Built in Functions")]
        public void TV_1467322_CountTrue_2()
        {
            string code = @"
a = [ [ ], true, false ];//1
b = [ 1, true ];//1
c = [ ""c"" ];//0
d = [ 0 ];//0
e = [ [ true ], true ]; //2
f = [ ];//0
g = 1; //0
h = null; //null
i = [ [ ] ]; //0
j = [ null ]; //0
k = ""string"";//0
arr = [a,b,c,d,e,f,g,h,i,j,k];
r = List.CountTrue(arr);
               ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 4);
        }

        [Test]
        [Category("Built in Functions")]
        public void TV_1467348_Rank_2()
        {
            String code =
@"import(""BuiltIn.ds"");
a = List.Rank(1);
a1 =List.Rank([ ]);
a2 =List.Rank([ [ ] ]);
a3 =List.Rank([ 1 ]);
a4 =List.Rank([ [ [ 1 ] ] ]);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 0);
            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 2);
            thisTest.Verify("a3", 1);
            thisTest.Verify("a4", 3);
        }

        [Test]
        [Category("Built in Functions")]
        [Category("Failure")]
        public void TV_1467350_Flatten()
        {
            String code =
@"
a = [];
b = 1;
c = [[]];
d = [[[]]];
e = [1,2,[3,4]];
f = [null];
g = [[null]];
h = [null,[1]];
i = [""1234"", true];
j = [true,[],null];
fa = List.Flatten(a);
fb = List.Flatten(b);
fc = List.Flatten(c);
fd = List.Flatten(d);
fe = List.Flatten(e);
ff = List.Flatten(f);
fg = List.Flatten(g);
fh = List.Flatten(h);
fi = List.Flatten(i);
fj = List.Flatten(j);
";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("MAGN-1684 REV:4495 When passing in a single value in a built-in function which takes in an array, the single value should be upgraded to one dimension array");
            Object[] v1 = new Object[] { };
            //Object v2 = null;
            Object[] v3 = new Object[] { 1, 2, 3, 4 };
            Object[] v4 = new Object[] { "1234", true };
            Object[] v5 = new Object[] { 1 };
            Object[] v6 = new Object[] { true, null };
            Object[] v7 = new Object[] { null };
            Object[] v8 = new Object[] { null, 1 };
            thisTest.Verify("fa", v1);
            thisTest.Verify("fb", v5);
            thisTest.Verify("fc", v1);
            thisTest.Verify("fd", v1);
            thisTest.Verify("fe", v3);
            thisTest.Verify("ff", v7);
            thisTest.Verify("fg", v7);
            thisTest.Verify("fh", v8);
            thisTest.Verify("fi", v4);
            thisTest.Verify("fj", v6);
        }

        [Test]
        [Category("Built in Functions")]
        public void T069_IsRectangular_DataType()
        {
            String code =
@"
a = [1];
b = [1,2];
c = [];
d = [[],[]];
e = 1;
ra = IsRectangular(a);
rb = IsRectangular(b);
rc = IsRectangular(c);
rd = IsRectangular(d);
re = IsRectangular(e);
";
            thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("ra", false);
            thisTest.Verify("rb", false);
            thisTest.Verify("rc", false);
            thisTest.Verify("rd", true);
            thisTest.Verify("re", v1);
        }

        [Test]
        [Category("Built in Functions")]
        public void T070_1467416_Count_Single()
        {
            String code =
@"
a = Count(1);
";
            string error = "1467416 Count returns null if the input argument is single value ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("Built in Functions")]
        public void T071_Insert_NegativeIndex01()
        {
            string code = @"
x = [1, 2];
y = 3;
z = Insert(x, y, -5);";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("z", new object[] { 3, null, null, 1, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T072_Insert_NegativeIndex02()
        {
            string code = @"
x = [1, 2];
y = 3;
z = Insert(x, y, -1);";
            thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("z", new object[] { 1, 3, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T073_Insert_ShallowCopy()
        {
            string code = @"
x = [ 1, 2 ];
y = [ 4, 5 ];
z = Insert(x, y, 0);
y[0] = 100;";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("z", new object[] { new object[] { 100, 5 }, 1, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T074_CountTrue()
        {
            string code = @"
C1 = List.CountTrue(1);
// expect C = 0, get C = null
C2 = List.CountTrue(true);
// expect C = 1, get C = null";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("C1", 0);
            thisTest.Verify("C2", 1);
        }

        [Test]
        [Category("Built in Functions")]
        public void T075_CountFalse()
        {
            string code = @"
C1 = List.CountFalse(1);
// expect C = 0, get C = null
C2 = List.CountFalse(false);
// expect C = 1, get C = null";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("C1", 0);
            thisTest.Verify("C2", 1);
        }

        [Test]
        [Category("Built in Functions")]
        public void T076_CountSingle()
        {
            string code = @"
a = Count(1);
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index()
        {
            string code =
@"
x = [ 1, 2 ];
y = 3;
z = Insert(x, y, -5);  
// expect z = {3, null, null, 1, 2}
//            -5  -4    -3   -2  -1
// but got warning Index out of range and z = null. 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 3, n1, n1, 1, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_2()
        {
            string code =
@"
x = [ 1, 2 ];
y = [3,3];
z = Insert(x, y, -1);  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 3, 3 }, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_3()
        {
            string code =
@"
z = [Imperative]
{
    x = [ 1, 2 ];
    y = [3,3];
    return Insert(x, y, -1);  
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 3, 3 }, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_4()
        {
            string code =
@"
def foo ()
{
    x = [ 1, 2 ];
    y = [3,3];
    z = Insert(x, y, -1);  
    return = z;
}
z = foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 3, 3 }, 2 });
        }

        [Test]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_5()
        {
            string code =
@"
def foo ()
{
    x = [ 1, 2 ];
    y = [3,3];
    z = Insert(1..2, 1..2, -1);  
    return = z;
}
z = foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 1, 2 }, 2 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Built in Functions")]
        public void T077_Defect_1467425_negative_index_6()
        {
            string code =
@"
def foo ()
{
    x = [ 1, 2 ];
    y = [3,3];
    z = Insert([1,2], 1..2, -1);  
    return = z;
}

z = foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", new Object[] { 1, new Object[] { 1, 2 }, 2 });
        }

        [Test]
        //Test 1467446
        public void BIM31_Sort()
        {
            String code =
@"a = [ 3, 1, 2 ];
def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}
sort = Sort(sorterFunction, a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }

        [Test]
        //Test "1467446"
        public void BIM31_Sort_null()
        {
            String code =
@"c = [ 3, 1, 2,null ];
def sorterFunction(a : int, b : int)
{
    return = [Imperative]
    {
        if (a == null)
            return = -1;
        if (b == null)
            return = 1;
        return = a > b ? 10 : -10;
    }
}
sort = Sort(sorterFunction, c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { null, 1, 2, 3 });
        }

        [Test]
        //Test "1467446"
        public void BIM31_Sort_duplicate()
        {
            String code =
@"c = [ 3, 1, 2, 2,null ];
def sorterFunction(a : int, b : int)
{
    return = [Imperative]
    {
        if (a == null)
            return = -1;
        if (b == null)
            return = 1;
        return = a - b;
    }
}
sort = Sort(sorterFunction, c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { null, 1, 2, 2, 3 });
        }

        [Test]
        //Test "1467446"
        public void BIM31_Sort_Associative()
        {
            String code =
@"
sort;
a = [ 3, 1, 2 ];
def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}
[Associative]
{
    sort = Sort(sorterFunction, a);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }

        [Test]
        [Category("Failure")]
        public void BIM32_Sort_class()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4099
            String code =
@"
class test{
a = { 3, 1, 2 };
sort;
def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}
def create ()
{
sort = Sort(sorterFunction, a);
return = sort;
}
}
z=test.test();
y=z.create();
";
            string err = "MAGN-4099 Member function cannot be used as function pointer";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
            
        }

        [Test]
        [Category("Failure")]
        public void BIM33_Sort_class_2()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4099
            String code =
@"
class test{
a = { 3, 1, 2 };
sort;
def sorterFunction(a : double, b : int)
{
    return = a > b ? 1 : -1;
}
def create ()
    {
y=test.test();        
sort = Sort(y.sorterFunction, a);
return = sort;
}
}
z=test.test();
y=z.create();
";
            string err = "MAGN-4099 Member function cannot be used as function pointer";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("y", new object[] { 1, 2, 3 });
        }

        [Test]
        //Test "1467446"
        public void BIM34_Sort_imperative()
        {
            String code =
@"
def sorterFunction(a : double, b : int)
{
    return a > b ? 1 : -1;
}
sort = [Imperative]
{
    a1 =  [ 3, 1, 2 ];
    return Sort(sorterFunction, a1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }

        [Test]
        public void BIM35_Sort_modifierblocks_1467446()
        {//1467446
            String code =
            @"
            def sorterFunction(a : double, b : int)
            {
                return = a > b ? 1 : -1;
            }
            sort = [Imperative]
            {
                a1 =  [ 3, 1, 2 ];
                return Sort(sorterFunction,a1);
            }
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }

        [Test]
        public void BIM37_Sort_nested_blocks_1467446()
        {//1467446
            String code =
            @"
                def sorterFunction(a : double, b : int)
                {
                    return = a > b ? 1 : -1;
                }
                sort = [Associative]
                {
                    return [Imperative]
                    {
                        return [Associative]
                        {
                            return [Imperative]
                            {
                                a1 = [ 3, 1, 2 ];
                                return Sort(sorterFunction, a1);
                            }
                        }
                    }
                } 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }

        [Test]
        public void BIM38_Sort_nested_blocks_1467446_2()
        {//1467446
            String code =
            @"
                sort;
                a1;
                def sorterFunction(a : double, b : int)
                {
                    return = a > b ? 1 : -1;
                }
                [Imperative]
                {
                [Associative]
                {
                [Imperative]
                {
                [Associative]
                {
                a1 = [ 3, 1, 2 ];
                // c = List.Flatten(a1);
               
                sort = Sort(sorterFunction, a1);
                }
                }
                }
                } 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 3 });
        }

        [Test]
        public void BIM39_Sort_multiarray_1467446()
        {
            String code =
            @"
                sort;
                a1;
                a1 = [ [ 4, 2, 3 ], [ 2, 5, 1 ], [ 8, 4, 6 ] ];
                // c = List.Flatten(a1);
                def sorterFunction(a : int, b : int)
                {
                    return = a > b ? 1 : -1;
                }
                def foo(a : int[])
                {
                    sort = Sort(sorterFunction, a);
                    return = sort;
                }
                d = foo(a1);
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { 2, 3, 4 }, new object[] { 1, 2, 5 }, new object[] { 4, 6, 8 } });
        }

        [Test]
        public void BIM40_Sort_multiarray_1467446_2()
        {//1467446
            String code =
            @"
                sort;
                a1;
                a1 = [  4, 2, 3 ,2, 5, 1 , 8, 4, 6  ];
                def sorterFunction(a : int, b : int)
                {
                    return = a > b ? 1 : -1;
                }
                def foo(a : int[])
                {
                    sort = Sort(sorterFunction, a);
                    return = sort;
                }
                sort = foo(a1);
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sort", new object[] { 1, 2, 2, 3, 4, 4, 5, 6, 8 });
        }

        [Test]
        public void BIM41_Sort_updateinput_1467446()
        {//1467446
            String code =
            @"
          sort;
a1;
a1 = [ [ 4, 2, 3 ], [ 2, 5, 1 ], [ 8, 4, 6 ] ];
def sorterFunction(a : int, b : int)
{
    return = a > b ? 1 : -1;
}
    def foo(a : int[])
    {
        sort = Sort(sorterFunction, a);
        return = sort;
    }
sorted= foo(a1);
a1 = [ [ 4, 2, 3 ], [ 2, 5, 1 ],[ 2,11,7], [ 8, 4, 6 ]  ];
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sorted", new object[] { new object[] { 2, 3, 4 }, new object[] { 1, 2, 5 }, new object[] { 2, 7, 11 }, new object[] { 4, 6, 8 } });
        }

        [Test]
        public void BIM42_Sort_imperative_while_1467446()
        {//1467446
            String code =
            @"
a1 = [ [ 4, 2, 3 ], [ 2, 5, 1 ], [ 8, 4, 6 ] ];
def sorterFunction(a : int, b : int)
{
    return a > b ? 1 : -1;
}

d = [];

def foo(a : int[])
{
    return Sort(sorterFunction, a);
}

i = [Imperative]
{
    dim = Count(a1);
    i = 0;
    while(i < dim)
    {
        d[i] = foo(a1[i]);
        i = i + 1;
    }
    return d;
};
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new object[] { new object[] { 2, 3, 4 }, new object[] { 1, 2, 5 }, new object[] { 4, 6, 8 } });
        }

        [Test]
        public void BIM45_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
           input = [ true, true, [ true, false ], [ true,false ] ];
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { true, new object[] { true, false } });
        }

        [Test]
        public void BIM46_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
           input = [  [ false,true ], [ true,false ] ];
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { new object[] { false, true }, new object[] { true, false } });
        }

        [Test]
        public void BIM47_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
           input = [  [ false,true , true,false ] ];
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { new object[] { false, true, true, false } });
        }

        [Test]
        public void BIM48_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
            input =   [ true ,false, true,false ];
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { true, false });
        }

        [Test]
        public void BIM50_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
            input =  [ true ,false, true,false,null ];
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { true, false, null });
        }

        [Test]
        public void BIM51_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
            input = [ 1,-1,3,6,[1] ];
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { 1, -1, 3, 6, new object[] { 1 } });
        }

        [Test]
        public void BIM52_RemoveDuplicates_1467447()
        {//1467446
            String code =
            @"
            input = [ 1,-1,3,6,0,[1] ];
            removeDuplicatesSetInsert = RemoveDuplicates(input); 
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("removeDuplicatesSetInsert", new object[] { 1, -1, 3, 6, 0, new object[] { 1 } });
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void BIM53_RemoveDuplicates_geoemtry_1467447()
        {//1467446
            String code =
            @"
            import(""FFITarget.dll"");
            pt = DummyPoint.ByCoordinates(1, 1, 1);
            input = [ pt, pt];
            removeDuplicatesSetInsert = RemoveDuplicates(input);
            count = Count(removeDuplicatesSetInsert);
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("count", 1);
        }

        [Test]
        public void BIM54_RemoveDuplicates_imperative_1467447()
        {
            String code =
            @"
            result = [Imperative]
            {
                a = [ true, true, [ false, true ] ];
                return RemoveDuplicates(a); 
            }
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", new object[] { true, new object[] { false, true } });
        }

        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV()
        {
            String code =
@"a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test.csv"";
b = Data.ImportCSV(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_2()
        {
            String code =
@"a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test.csv"";
b = Data.ImportCSV(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_replicated_3()
        {
            String code =
@"a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test.csv"";
b = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test2.csv"";
c = [ a, b ];
d = Data.ImportCSV(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_replicated_4()
        {
            String code =
@"a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test.csv"";
b = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test2.csv"";
c = [ a, b ];
d = Data.ImportCSV(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_replicated_5()
        {
            String code =
@"a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test.csv"";
b = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test2.csv"";
c = [ a, b ];
d = Data.ImportCSV(c);
";
            string err = "";
            string path = @"C:\designscript\autodeskresearch\trunk\DesignScript\Prototype\Scripts\TD\";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err, path);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_imperative_5()
        {
            String code =
@"
d = [Imperative]
{
    a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test.csv"";
    b = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test2.csv"";
    c = [ a, b ];
    d = Data.ImportCSV(c);
    return d;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 
                new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM23_LoadCSV_nested_imperative_6()
        {
            String code =
@"
d;
[Associative]
{
[Imperative]
{
[Associative]
{
a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test.csv"";
b = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test2.csv"";
c = [ a, b ];
d = Data.ImportCSV(c);
}
}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new object[] { new object[] { new object[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, new object[] { 2.0, 3.0, 4.0, 5.0, 6.0 }, new object[] { 3.0, 4.0, 5.0, 6.0, 7.0 } }, new object[] { new object[] { 11.0, 12.0, 13.0, 14.0, 15.0 }, new object[] { 12.0, 13.0, 14.0, 15.0, 16.0 }, new object[] { 13.0, 14.0, 15.0, 16.0, 17.0 } } }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM24_RemoveIfNot()
        {
            String code =
@"
import(""BuiltIn.ds"");
a = [ true,null,false,true];
b = List.RemoveIfNot(a, ""bool"");
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { true, false, true }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM24_RemoveIfNot_Imperative()
        {
            String code =
@"
import(""BuiltIn.ds"");
b = [Imperative]
{
    a = [ true,null,false,true];
    return List.RemoveIfNot(a, ""bool"");
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { true, false, true }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM25_RemoveIfNot_variable()
        {
            String code =
@"
import(""BuiltIn.ds"");
b;
    a = [ true,null,false,true];
    c=""bool"";
    b = List.RemoveIfNot(a, c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { true, false, true }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM26_RemoveIfNot_int()
        {
            String code =
@"
import(""BuiltIn.ds"");
b;
    a = [ true,null,false,true];
    c=""int"";
    b = List.RemoveIfNot(a, c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM26_RemoveIfNot_double()
        {
            String code =
@"
import(""BuiltIn.ds"");
b;
    a = [ 1.0,null,1,2];
    c=""double"";
    b = List.RemoveIfNot(a, c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 1.0 }
);
        }

        [Test]
        //Test "LoadCSV"
        public void BIM27_RemoveIfNot_heterogenous()
        {
            String code =
@"
import(""BuiltIn.ds"");
b;
    a = [ true,null,[true],false];
    c=""array"";
    b = List.RemoveIfNot(a, c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { true } }
);
        }

        [Test]
        public void BIM27_1467445_Transpose()
        {
            String code =
@"
a = [ 1, [ 2, 3 ], [ 4, 5, [ 6, 7 ] ] ];
b = Transpose(a);
c = Transpose(Transpose(a));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { 4, 5, new object[] { 6, 7 } } });
            thisTest.Verify("b", new object[] { new object[] { 1, 2, 4 }, new object[] { null, 3, 5 }, new object[] { null, null, new object[] { 6, 7 } } });
            thisTest.Verify("c", new object[] { new object[] { 1, null, null }, new object[] { 2, 3, null }, new object[] { 4, 5, new object[] { 6, 7 } } });
        }

        [Test]
        public void BIM27_1467445_Transpose_2()
        {
            String code =
@"
a = [ 1, [ 2, 3 ], [  [ 6, 7 ] ] ];
b = Transpose(a);
c = Transpose(Transpose(a));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { new object[] { 6, 7 } } });
            thisTest.Verify("b", new object[] { new object[] { 1, 2, new object[] { 6, 7 } }, new object[] { null, 3, null } });
            thisTest.Verify("c", new object[] { new object[] { 1, null }, new object[] { 2, 3 }, new object[] { new object[] { 6, 7 }, null } });
        }

        [Test]
        public void BIM27_1467445_Transpose_3()
        {
            String code =
@"
a = [ 1, [ 2, 3 ], [ [ [ 6, 7 ] ] ] ];
b = Transpose(a);
c = Transpose(Transpose(a));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { new object[] { new object[] { 6, 7 } } } });
            thisTest.Verify("b", new object[] { new object[] { 1, 2, new object[] { new object[] { 6, 7 } } }, new object[] { null, 3, null } });
        }

        [Test]
        public void BIM27_1467445_Transpose_4()
        {
            String code =
@"
a = 4;
b = Transpose(a);
c = Transpose(Transpose(a));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
            thisTest.Verify("b", 4);
            thisTest.Verify("c", 4);
        }

        [Test]
        public void BIM27_1467445_Transpose_5()
        {
            String code =
@"
a = null;
b = Transpose(a);
c = Transpose(Transpose(a));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
        }

        [Test]
        public void BIM27_1467445_Transpose_6()
        {
            String code =
@"
def foo()
{
    return = [ 1, [ 2, 3 ], 4 ];
}
b = Transpose(foo());
c = Transpose(Transpose(foo()));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 1, 2, 4 }, new object[] { null, 3, null } });
            thisTest.Verify("c", new object[] { new object[] { 1, null }, new object[] { 2, 3 }, new object[] { 4, null } });
        }

        [Test]
        public void BIM27_1467445_Transpose_7()
        {
            String code =
@"
 a = [ 1, [ 2, 3 ], [ 4, 5, [ 6, 7 ] ] ];
 i = [Imperative]
 {
    b = Transpose(a);
    c = Transpose(Transpose(a));
    return [b, c];
 }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1, new object[] { 2, 3 }, new object[] { 4, 5, new object[] { 6, 7 } } });
            var b = new object[] { new object[] { 1, 2, 4 }, new object[] { null, 3, 5 }, new object[] { null, null, new object[] { 6, 7 } } };
            var c = new object[] { new object[] { 1, null, null }, new object[] { 2, 3, null }, new object[] { 4, 5, new object[] { 6, 7 } } };
            thisTest.Verify("i", new object[] {b, c});
        }

        [Test]
        public void T069_Defect_1467556_Sort_Over_Derived_Classes()
        {
            String code =
@"
class Test
{
    X;
    constructor(a)
    {
        X = a;
    }
}
class MyTest extends Test
{
    Y;
    constructor(a,b)
    {
        X = a;
        Y = b;
    }
}
def sorter(p1:Test,p2:Test) 
{
    return = (p1.X < p2.X) ? -1 : ((p1.X > p2.X) ? 1 : 0);
}
t1 = Test(1);
t2 = MyTest(2,2);
a = [t1,t2];
b = Sort(sorter,a).X; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 1, 2 });
        }

        [Test]
        public void T069_Defect_1467556_Sort_Over_Derived_Classes_2()
        {
            String code =
@"
class Test
{
    X;
    constructor(a)
    {
        X = a;
    }
}
class MyTest extends Test
{
    Y;
    constructor(a,b)
    {
        X = a;
        Y = b;
    }
}
def sorter(p1:Test,p2:Test) 
{
    return = p1.X<=p2.X?true:false;
}
t1 = Test(1);
t2 = MyTest(2,2);
a = [t1,t2];
b = Sort(sorter,a).X; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.VerifyRuntimeWarningCount(2);
            // This sorter function is invalid. Meaningless to verify the 
            // garbage result because it is totally undefined. 
            // thisTest.Verify("b", null);
        }

        [Test]
        public void T069_Defect_1467556_Sort_Over_Derived_Classes_3()
        {
            String code =
@"
class Test
{
    X;
    constructor(a)
    {
        X = a;
    }
}
class MyTest extends Test
{
    Y;
    constructor(a,b)
    {
        X = a;
        Y = b;
    }
}
def sorter(p1:Test,p2:MyTest) 
{
    return = p1.X<=p2.X?1:0;
}
t1 = Test(1);
t2 = MyTest(2,2);
a = [t1,t2];
b = Sort(sorter,a).X; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // thisTest.VerifyRuntimeWarningCount(3);
            // This sorter function is invalid. Meaningless to verify the 
            // garbage result because it is totally undefined.
            // thisTest.Verify("b", null);
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly()
        {
            String code =
@"
a = [3,1,2];
def sorterFunction(a:double, b:double)
{
    return = 0;
}
b = Sort(sorterFunction, a);  
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", new Object[] { 2, 1, 3 });
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly_2()
        {
            String code =
@"
a = [3,1,2];
def sorterFunction(a:double, b:double)
{
    return = 1;
}
b = Sort(sorterFunction, a);  
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", new Object[] { 3, 1, 2 });
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly_5()
        {
            String code =
@"
a = [3,1,2];
def sorterFunction(a:double, b:double)
{
    return = -1;
}
b = Sort(sorterFunction, a);  
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // thisTest.VerifyRuntimeWarningCount(1);
            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", null);
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly_3()
        {
            String code =
@"
a = [3,1,2];
def sorterFunction(a:double, b:double)
{
    return = 35;
}
b = Sort(sorterFunction, a);  
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", new Object[] { 3, 1, 2 });
        }

        [Test]
        public void T070_Defect_1467466_Sort_Not_Defined_Porperly_4()
        {
            String code =
@"
a = [3,1,2];
def sorterFunction(a:double, b:double)
{
    return = 1.5;
}
b = Sort(sorterFunction, a);  
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // It is meaningless to verify the result returned from a Sort()
            // function which uses an invalid sorterFunction() to sort an array.
            // thisTest.Verify("b", new Object[] { 3, 1, 2 });
        }

        [Test]
        public void T071_Defect_ExD_10016_Rand()
        {
            String code =
@"
a = [3,1,2];
def sorterFunction(a:double, b:double)
{
    return = 1.5;
}
b = List.SortByFunction(sorterFunction, a);  
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // The test doesn't make sense. 
            // thisTest.Verify("b", new Object[] { 3, 1, 2 });
        }

        [Test]
        [Category("Failure")]
        public void T072_defect_1467577()
        {
            String code =
@"
index= IndexOf(1..10..1 , [1,2]);
";
            string err = "MAGN-4106 IndexOf Built-in method fails to replicate";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("index", new Object[] { 0, 1 });
        }

        [Test]
        [Category("Failure")]
        public void T072_defect_1467577_2()
        {
            String code =
@"
def testRepl(val : var[], index:int)
{
    return = IndexOf(val,index);
}
z = testRepl(1..5, 1..2);
z1 = IndexOf(1..5, [ 1, 2 ]);
";
            string err = "MAGN-4106 IndexOf Built-in method fails to replicate";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("z", new Object[] { 0, 1 });
            thisTest.Verify("z1", new Object[] { 0, 1 });
        }

        [Test]
        public void T073_Defect_ImportFromCSV_1467579()
        {
            String code =
@"import(""DSCoreNodes.dll"");
a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/trailing_comma.csv"";
b = Data.ImportCSV(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 10, 40 }, new object[] { 20.0, 50 }, new object[] { 30.0, 60.00 } }
);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T074_Defect_1467750()
        {
            String code =
@"import(""DSCoreNodes.dll"");
x = A.A();
x1 = List.Flatten(a) ; 
x2 = List.Flatten(3) ;
x3 = List.Flatten(3.5) ;
x4 = List.Flatten(true) ;
x6 = List.Flatten(null) ;
x7 = List.Flatten([]) ;
x8 = List.Flatten([null]) ;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", null);
            thisTest.Verify("x2", 3);
            thisTest.Verify("x3", 3.5);
            thisTest.Verify("x4", true);
            thisTest.Verify("x6", null);
            thisTest.Verify("x8", new Object[] { null });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T074_Defect_1467750_2()
        {
            String code =
@"import(""DSCoreNodes.dll"");
test = 
[Imperative]
{
    x = A.A();
    x1 = List.Flatten(a) ; 
    x2 = List.Flatten(3) ;
    x3 = List.Flatten(3.5) ;
    x4 = List.Flatten(true) ;
    x6 = List.Flatten(null) ;
    x7 = List.Flatten([]) ;
    x8 = List.Flatten([null]) ;
    return = [ x1, x2, x3, x4, x6, x8 ];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { null, 3, 3.5, true, null, new Object[] { null } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T074_Defect_1467750_3()
        {
            String code =
@"import(""DSCoreNodes.dll"");
test = foo();
def foo ()
{
    return = [Imperative]
    {
        x1 = List.Flatten(a) ; 
        x2 = List.Flatten(3) ;
        x3 = List.Flatten(3.5) ;
        x4 = List.Flatten(true) ;
        x6 = List.Flatten(null) ;
        x7 = List.Flatten([]) ;
        x8 = List.Flatten([null]) ;
        return = [ x1, x2, x3, x4, x6, x8 ];
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { null, 3, 3.5, true, null, new Object[] { null } });
        }

        [Test]
        public void T074_Defect_1467301()
        {
            String code =
@"import(""DSCoreNodes.dll"");
x1 = Math.Average(a) ;
x2 = Math.Average(a) ;
x4 = Math.Average(null) ;
x5 = Math.Average([]) ;
x6 = Math.Average([null]) ;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("x1", null);
            thisTest.Verify("x2", null);
            thisTest.Verify("x4", null);
            thisTest.Verify("x5", null);
            thisTest.Verify("x6", null);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T075_Defect_1467323()
        {
            String code =
@"
x1 = Count(a) ; 
x2 = Count(3) ;
x3 = Count(3.5) ;
x4 = Count(true) ;
x6 = Count(null) ;
x7 = Count([]) ;
x8 = Count([null]) ;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 1);
            thisTest.Verify("x3", 1);
            thisTest.Verify("x4", 1);
            thisTest.Verify("x6", 1);
            thisTest.Verify("x7", 0);
            thisTest.Verify("x8", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T075_Defect_1467323_2()
        {
            String code =
@"
test = 
[Imperative]
{
    x1 = Count(a) ; 
    x2 = Count(3) ;
    x3 = Count(3.5) ;
    x4 = Count(true) ;
    x6 = Count(null) ;
    x7 = Count([]) ;
    x8 = Count([null]) ;
    return = [ x1, x2, x3, x4, x6, x7, x8 ];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 1, 1, 1, 1, 1, 0, 1 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T075_Defect_1467323_3()
        {
            String code =
@"
test = foo();
def foo ()
{
    return = [Imperative]
    {
        x1 = Count(a) ; 
        x2 = Count(3) ;
        x3 = Count(3.5) ;
        x4 = Count(true) ;
        x6 = Count(null) ;
        x7 = Count([]) ;
        x8 = Count([null]) ;  
        return = [ x1, x2, x3, x4, x6, x7, x8 ]; 
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 1, 1, 1, 1, 1, 0, 1 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T075_Defect_1467323_4()
        {
            String code =
@"
test = foo();
def foo ()
{
    return = [Imperative]
    {
        x1 = Count(a) ; 
        x2 = Count(3) ;
        x3 = Count(3.5) ;
        x4 = Count(true) ;
        x6 = Count(null) ;
        x7 = Count([]) ;
        x8 = Count([null]) ;
        return = [ x1, x2, x3, x4, x6, x7, x8 ];
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 1, 1, 1, 1, 1, 0, 1 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T075_Defect_1467323_5()
        {
            String code =
@"
test = foo();
def foo ()
{
    return = [Imperative]
    {
        if ( 1 )
        {
            x1 = Count(a) ; 
            x2 = Count(3) ;
            x3 = Count(3.5) ;
            x4 = Count(true) ;
            x6 = Count(null) ;
            x7 = Count([]) ;
            x8 = Count([null]) ;
            return = [ x1, x2, x3, x4, x6, x7, x8 ];                
        }
        else return = 1;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 1, 1, 1, 1, 1, 0, 1 });
        }

        [Test]
        //Test "LoadCSV"
        public void Defect_ImportFromCSV_1467622()
        {
            String code =
            @"import(""DSOffice.dll"");
            a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/test.csv"";
            b = Data.ImportCSV(a);
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 1, 2, 3, 4, 5 }, new object[] { 2, 3, 4, 5, 6 },
            new object[] { 3, 4, 5, 6, 7 } }
);
        }

        [Test]
        //Test "LoadCSV"
        public void Defect_ImportFromCSV_1467622_2()
        {
            String code =
            @"import(""DSOffice.dll"");
            a = ""../../../test/Engine/ProtoTest/ImportFiles/CSV/Set2/trailing_comma.csv"";
            b = Data.ImportCSV(a);
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { new object[] { 10, 40 }, new object[] { 20, 50 },
            new object[] { 30, 60 }, new object[] { null, null } }
);
        }

        [Test]
        public void TestMapToWithReverserRange()
        {
            string code = @"import(""DSCoreNodes.dll"");
r = Math.MapTo(5, 10, 5..10, 2, 0);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 2.0, 1.6, 1.2, 0.8, 0.4, 0 });
        }
    }

}
