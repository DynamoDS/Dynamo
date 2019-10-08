using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.MultiLangTests
{
    class TestScope : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T001_LanguageBlockScope_AssociativeNestedAssociative()
        // It may not make sense to do so. Could treat as a negative case
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
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
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T002_LanguageBlockScope_ImperativeNestedImperaive()
        // It may not make sense to do so. Could treat as a negative case
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
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
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T003_LanguageBlockScope_ImperativeNestedAssociative()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a_inner", 10);
            thisTest.Verify("b_inner", true);
            thisTest.Verify("c_inner", 20.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T004_LanguageBlockScope_AssociativeNestedImperative()
        {
            string src = @"a_inner;b_inner;c_inner;
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	i = [Imperative]	
	{
		a_inner = a;
		b_inner = b;
		c_inner = c;
        return [a_inner, b_inner, c_inner];
	}
	a_inner=i[0];b_inner=i[1];c_inner=i[2];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a_inner", 10);
            thisTest.Verify("b_inner", true);
            thisTest.Verify("c_inner", 20.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T005_LanguageBlockScope_DeepNested_IAI()
        {
            string src = @"
a_inner1;a_inner2;
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
		
		
		i = [Imperative]
		{
			a_inner2 = a;
			b_inner2 = b;
			c_inner2 = c;
			return [a_inner2, b_inner2, c_inner2];
		}
        a_inner2 = i[0];
		b_inner2 = i[1];
		c_inner2 = i[2];
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a_inner1", 10);
            thisTest.Verify("b_inner1", true);
            thisTest.Verify("c_inner1", 20.1);
            thisTest.Verify("a_inner2", 10);
            thisTest.Verify("b_inner2", true);
            thisTest.Verify("c_inner2", 20.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T006_LanguageBlockScope_DeepNested_AIA()
        {
            string src = @"
a_inner1;b_inner1;c_inner1;
a_inner2;b_inner2;c_inner2;
[Associative]
{
	a = 10;
	b = true;
	c = 20.1;
	i = [Imperative]	
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
        return [a_inner1, b_inner1, c_inner1];
	}
    a_inner1=i[0];b_inner1=i[1];c_inner1=i[2];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a_inner1", 10);
            thisTest.Verify("b_inner1", true);
            thisTest.Verify("c_inner1",20.1);
            thisTest.Verify("a_inner2", 10);
            thisTest.Verify("b_inner2", true);
            thisTest.Verify("c_inner2",20.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T007_LanguageBlockScope_AssociativeParallelImperative()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            thisTest.Verify("aI", null);
            thisTest.Verify("bI", null);
            thisTest.Verify("cI", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T008_LanguageBlockScope_ImperativeParallelAssociative()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("aA", null);
            thisTest.Verify("bA", null);
            thisTest.Verify("cA", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_IA()
        {
            string src = @"
newA = i[0]; newB = i[1]; newC = i[2];
i = [Imperative]
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
    return [newA, newB, newC];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("newA",1.5);
            thisTest.Verify("newB", -4);
            thisTest.Verify("newC", false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_AI()
        {
            string src = @"
a;b;c;
newA;newB;newC;
[Associative]
{
	a = -10;
	b = false;
	c = -20.1;
	i = [Imperative]	
	{
		a = 1.5;
		b = -4;
		c = false;
        return [a,b,c];
	}
	
	a = newA = i[0];
	b = newB = i[1];
	c = newC = i[2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            thisTest.Verify("a",1.5);
            thisTest.Verify("b", -4);
            thisTest.Verify("c", false);
            thisTest.Verify("newA",1.5);
            thisTest.Verify("newB",-4);
            thisTest.Verify("newC", false);

        }

        [Test]
        [Category("SmokeTest")]
        public void T011_LanguageBlockScope_AssociativeParallelAssociative()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("aA", null);
            thisTest.Verify("bA", null);
            thisTest.Verify("cA", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_LanguageBlockScope_ImperativeParallelImperative()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("aI", null);
            thisTest.Verify("bI", null);
            thisTest.Verify("cI", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T013_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("aI", null);
            thisTest.Verify("bI", null);
            thisTest.Verify("cI", null);
            thisTest.Verify("aA", null);
            thisTest.Verify("bA", null);
            thisTest.Verify("cA", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T014_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("aI", null);
            thisTest.Verify("bI", null);
            thisTest.Verify("cI", null);
            thisTest.Verify("aA", null);
            thisTest.Verify("bA", null);
            thisTest.Verify("cA", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T015_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II()
        {
            string src = @"a;aI1;aA1;aI2;aA2;
[Associative]
{
	a = 10;
	
	aI1 = [Imperative]	
	{
		return a;
	}
	aA1 = a;
	
	aI2 = [Imperative]	
	{
		return a;
	}
	
	aA2 = a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", 10);
            thisTest.Verify("aI1", 10);
            thisTest.Verify("aA1", 10);
            thisTest.Verify("aI2", 10);
            thisTest.Verify("aA2", 10);

        }

        [Test]
        [Category("SmokeTest")]
        public void T016_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA()
        {
            string src = @"
a=i[0];aA1=i[1];aI1=i[2];aA2=i[3];aI2=i[4];
i = [Imperative]
{
	a = 10;
	aA1 = [Associative]	
	{
		return a;
	}
	aI1 = a;
	
	aA2 = [Associative]	
	{
		return a;
	}
	aI2 = a;
    return [a, aA1, aI1, aA2, aI2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", 10);
            thisTest.Verify("aA1", 10);
            thisTest.Verify("aI1", 10);
            thisTest.Verify("aA2", 10);
            thisTest.Verify("aI2", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T017_LanguageBlockScope_AssociativeNestedAssociative_Function()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Associative]
{
	def foo : int(a : int, b : int)
	{
		return = a + b;
	}
	[Associative]	
	{
	    x = 10;
	    y = 20;
	    z = foo (x, y);
	}
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                // thisTest.Verify("z", 30);           
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T018_LanguageBlockScope_ImperativeNestedImperaive_Function()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"[Imperative]
{
	def foo : int(a : int, b : int)
	{
		return = a + b;
	}
		[Imperative]	
	{
	x = 10;
	y = 20;
	z = foo (x, y);
	}
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                // thisTest.Verify("z", 30);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T019_LanguageBlockScope_ImperativeNestedAssociative_Function()
        {
            string code = @"
z;
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
[Imperative]
{
	[Associative]	
	{
	x = 20;
	y = 10;
	z = foo (x, y);
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 10);

        }

        [Test]
        [Category("SmokeTest")]
        public void T020_LanguageBlockScope_AssociativeNestedImperative_Function()
        {
            string src = @"z;
def foo : int(a : int, b : int)
{
    return = a - b;
}
[Associative]
{
	z = [Imperative]	
	{
	    x = 20;
	    y = 10;
	    return foo (x, y);
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T021_LanguageBlockScope_DeepNested_IAI_Function()
        {
            string code = @"
z_1;
z_2;
def foo : int(a : int, b : int)
{
	return = a - b;
}
[Imperative]
{
	[Associative]
	{
		x_1 = 20;
		y_1 = 10;
		z_1 = foo (x_1, y_1);
	
	    z_2 = [Imperative]
		{
			x_2 = 100;
			y_2 = 100;
			return foo (x_2, y_2);
		}
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z_1", 10);
            thisTest.Verify("z_2", 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T022_LanguageBlockScope_DeepNested_AIA_Function()
        {
            string src = @"
z_1;
z_2;
def foo : int(a : int, b : int)
{
	return = a - b;
}
[Associative]
{
	z_1 = [Imperative]	
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
        return z_1;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z_1", 10);
            thisTest.Verify("z_2", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T023_LanguageBlockScope_AssociativeParallelImperative_Function()
        {
            string src = @"
def foo : int(a : int, b : int)
{
    return = a - b;
}
[Associative]
{
	a = 10;
}
z = [Imperative]	
{
	x = 20;
	y = 0;
	return foo (x, y);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void T024_LanguageBlockScope_ImperativeParallelAssociative_Function()
        {

            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string src = @"z;
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
[Imperative]
{
	a = 10;
	
}
[Associative]	
{
	x = 20;
	y = 0;
	z = foo (x, y);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z", 20);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }

        [Test]
        [Category("SmokeTest")]
        public void T025_LanguageBlockScope_AssociativeParallelAssociative_Function()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string src = @"z;
def foo : int(a : int, b : int)
{
    return = a - b;
}
	 
[Associative]
{
	a = 10;
}
[Associative]	
{
	x = 20;
	y = 0;
	z = foo (x, y);
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void T026_LanguageBlockScope_ImperativeParallelImperative_Function()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string src = @"
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}

z = [Imperative]	
{
	x = 20;
	y = 0;
	z = foo (x, y);
	return z;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z", 20);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }

        [Test]
        [Category("SmokeTest")]
        public void T027_LanguageBlockScope_MultipleParallelLanguageBlocks_AIA_Function()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string src = @"z_1;
z_2;
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
[Associative]
{
	a = 10;
}
z_1 = [Imperative]	
{
	x_1 = 20;
	y_1 = 0;
	return foo (x_1, y_1);
	
}
[Associative]
{
	x_2 = 20;
	y_2 = 0;
	z_2 = foo (x_2, y_2);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z_1", 20);
            thisTest.Verify("z_2", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void T028_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI_Function()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string src = @"z_1;
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
[Imperative]
{
	a = 10;
	
}
[Associative]	
{
	x_1 = 20;
	y_1 = 0;
	z_1 = foo (x_1, y_1);
	
}
z_2 = [Imperative]
{
	x_2 = 20;
	y_2 = 0;
	return foo (x_2, y_2);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z_1", 20);
            thisTest.Verify("z_2", 20);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }

        [Test]
        [Category("SmokeTest")]
        public void T029_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II_Function()
        {
            string src = @"z_I1;
z_I2;
z_A1;
z_A2;
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
[Associative]
{
	z_I1 = [Imperative]
	{
	    x_I1 = 50;
	    y_I1 = 50;
	    return foo (x_I1, y_I1);
	}
	
	x_A1 = 30;
	y_A1 = 12;
	z_A1 = foo (x_A1, y_A1);
	
	z_I2 = [Imperative]
	{
	    x_I2 = 0;
	    y_I2 = 12;
	    return foo (x_I2, y_I2);
	}
	
	x_A2 = 0;
	y_A2 = -10;
	z_A2 = foo (x_A2, y_A2);
	
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("z_I1", 0);
            thisTest.Verify("z_I2", -12);
            thisTest.Verify("z_A1", 18);
            thisTest.Verify("z_A2", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T030_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA()
        {
            string code = @"
z_A1=i[0];
z_I1=i[1];
z_A2=i[2];
z_I2=i[3];
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
i = [Imperative]
{
	[Associative]
	{
		x_A1 = 30;
		y_A1 = 12;
		z_A1 = foo (x_A1, y_A1);
	
	}
	
	x_I1 = 50;
	y_I1 = 50;
	z_I1 = foo (x_I1, y_I1);
	
	[Associative]
	{
		x_A2 = 0;
		y_A2 = -10;
		z_A2 = foo (x_A2, y_A2);
	}
	
	x_I2 = 0;
	y_I2 = 12;
	z_I2 = foo (x_I2, y_I2);
	
	return [z_A1, z_I1, z_A2, z_I2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z_A1", 18);
            thisTest.Verify("z_I1", 0);
            thisTest.Verify("z_A2", 10);
            thisTest.Verify("z_I2", -12);



        }
        //Defect Regress Test Cases

        [Test]
        [Category("SmokeTest")]
        public void Z001_LanguageBlockScope_Defect_1453539()
        //1453539 - Sprint 15: Rev 585: Unexpected high memory usage when running a script with some comment out imperative codes
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            //No verification needed, just need to run the case. 
        }

        [Test]
        [Category("SmokeTest")]
        public void T032_Cross_Language_Variables()
        {
            string src = @"
a = 5;
b = 2 * a;
count = [Imperative] {
	count = 0;
	arr = 0..b;
	for (i  in arr) {
		count = count + 1;
	}
    return count;
}
a = 10;
// expected: count = 21
// result: count = 11";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("count", 21);

        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_01()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"~a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_02()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"`a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_03()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"!a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_04()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"#a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_05()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"$a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_06()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"%a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_07()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"^a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_08()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"&a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_09()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"*a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_10()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"(a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_11()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @")a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_12()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"-a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_13()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"=a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_14()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"+a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_15()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"?a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_16()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"[a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_17()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"]a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_18()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"{a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_19()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"}a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_20()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"\\a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_21()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"|a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_22()
        {
            String code =
               @";a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_23()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @":a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_24()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"\'a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_25()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"""a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_26()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @",a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_27()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @".a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_28()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"/a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_29()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"<a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_30()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @">a = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_31()
        {

            String code =
             @"a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_32()
        {
            String code =
               @"_a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("_a", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_01()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a~ = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_02()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a` = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_03()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a! = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_04()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a# = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_05()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a$ = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_06()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a% = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_07()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a^ = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_08()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a& = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_09()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a* = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_10()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a( = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_11()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a) = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_12()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a- = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_13()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a+ = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_14()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a= = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_15()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a? = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_16()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a[ = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_17()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a] = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_18()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a{ = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_19()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a} = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_20()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a\\ = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_21()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a| = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_22()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a; = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_23()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a: = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_24()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a\' = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_25()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a"" = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_26()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a< = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_27()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a> = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_28()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a/ = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_29()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a, = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_30()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                String code =
                 @"a. = 1;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_32()
        {
            String code =
               @"a_ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a_", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_01()
        {
            String code =
  @"

  def foo(c:var)
  {
    a = c;
    return = a;
  }

  a = 1;
  t = 3;
  a = foo(t);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_02()
        {
            String code =
              @"
def foo(a:var)
{
  b = a + 1;
  return = b;
}
t = 1;
c = foo(t);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_03()
        {
            String code =
              @"
c = [Imperative]
{
  a = 10;
  b = [10,20,30];
  for (i in b)
  {
      a = a + i;
  }
  return = a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 70);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_04()
        {
            String code =
              @"
c = [Imperative]
{
  a = 20;
  b = 10;
  if(a > 0){
    a = a - b;
    b = 20;
  }else
  {
    b = 30;
  }
  return = a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_05()
        {
            String code =
              @"
b;
[Associative]
{
	a = 1;
	b = a + 2;
	a = 3;
          
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_06()
        {
            String code =
        @"
c=
[Imperative]
{	
   a = 1;
   b = 2;
   return a < b ? a : b;			
}
                        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_07()
        {
            String code =
        @"
a=
[Imperative]
{	
   a1 = 1;
   a2 = 5;
   a3 = 1;
   return a1..a2..a3;
			
}
                        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            List<Object> result = new List<Object> { 1, 2, 3, 4, 5 };
            thisTest.Verify("a", result);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T052_DNL_1467464()
        {
            string code = @"
def test()
{
    f;
    [Associative]
    {
        i=[Imperative]
        {
            return 3;
        }
        f = i;
    }
    return = f;
}
b = test();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 3);
        }
    }
}
