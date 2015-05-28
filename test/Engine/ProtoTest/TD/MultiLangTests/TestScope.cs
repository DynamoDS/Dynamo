using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
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
                //Assert.IsTrue((Int64)mirror.GetValue("a_inner", 2).Payload == 10);
                //Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner", 2).Payload) == true);
                //Assert.IsTrue((double)mirror.GetValue("c_inner", 2).Payload == 20.1);
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
                // Assert.IsTrue((Int64)mirror.GetValue("a_inner", 2).Payload == 10);
                // Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner", 2).Payload) == true);
                // Assert.IsTrue((double)mirror.GetValue("c_inner", 2).Payload == 20.1);
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
            Assert.IsTrue((Int64)mirror.GetValue("a_inner").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner").Payload == 20.1);
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
	[Imperative]	
	{
		a_inner = a;
		b_inner = b;
		c_inner = c;
	}
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a_inner").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner").Payload == 20.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T005_LanguageBlockScope_DeepNested_IAI()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a_inner1").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner1").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner1").Payload == 20.1);
            Assert.IsTrue((Int64)mirror.GetValue("a_inner2").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner2").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner2").Payload == 20.1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T006_LanguageBlockScope_DeepNested_AIA()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a_inner1").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner1").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner1").Payload == 20.1);
            Assert.IsTrue((Int64)mirror.GetValue("a_inner2").Payload == 10);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("b_inner2").Payload));
            Assert.IsTrue((double)mirror.GetValue("c_inner2").Payload == 20.1);
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

            Assert.IsTrue(mirror.GetValue("aI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("bI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("cI").DsasmValue.IsNull);
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
            Assert.IsTrue(mirror.GetValue("aA").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("bA").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("cA").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_IA()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 1.5);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c").Payload) == false);
            Assert.IsTrue((double)mirror.GetValue("newA").Payload == 1.5);
            Assert.IsTrue((Int64)mirror.GetValue("newB").Payload == -4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("newC").Payload) == false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_LanguageBlockScope_UpdateVariableInNestedLanguageBlock_AI()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            Assert.IsTrue((double)mirror.GetValue("a").Payload == 1.5);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("c").Payload) == false);
            Assert.IsTrue((double)mirror.GetValue("newA", 0).Payload == 1.5);
            Assert.IsTrue((Int64)mirror.GetValue("newB", 0).Payload == -4);
            Assert.IsTrue(System.Convert.ToBoolean(mirror.GetValue("newC", 0).Payload) == false);

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
            Assert.IsTrue(mirror.GetValue("aA").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("bA").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("cA").DsasmValue.IsNull);
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
            Assert.IsTrue(mirror.GetValue("aI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("bI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("cI").DsasmValue.IsNull);
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
            Assert.IsTrue(mirror.GetValue("aI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("bI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("cI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("aA").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("bA").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("cA").DsasmValue.IsNull);
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
            Assert.IsTrue(mirror.GetValue("aI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("bI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("cI").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("aA").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("bA").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("cA").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T015_LanguageBlockScope_ParallelInsideNestedBlock_AssociativeNested_II()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aI1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aA1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aI2").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aA2").Payload == 10);

        }

        [Test]
        [Category("SmokeTest")]
        public void T016_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aA1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aI1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aA2").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("aI2").Payload == 10);
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
                // Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 30);           
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
                // Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 30);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T019_LanguageBlockScope_ImperativeNestedAssociative_Function()
        {
            string code = @"
z;
[Imperative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T021_LanguageBlockScope_DeepNested_IAI_Function()
        {
            string code = @"
z_1;
z_2;
[Imperative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	[Associative]	
	{
		x_1 = 20;
		y_1 = 10;
		z_1 = foo (x_1, y_1);
	
	
	[Imperative]
		{
			x_2 = 100;
			y_2 = 100;
			z_2 = foo (x_2, y_2);
			
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("z_1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("z_2").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T023_LanguageBlockScope_AssociativeParallelImperative_Function()
        {
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("z").DsasmValue.IsNull);
            //Fuqiang: If function not found, it will return null and continues to execute.

            //Assert.Fail("1453777: Sprint 15: Rev 617: Scope: DS is able to call function defined in a parallel language block ");
            //HQ:
            //We need negative verification here. By design, we should not be able to call a function defined in a parallelled language block.
            //Should this script throw a compilation error here?
        }

        [Test]
        [Category("SmokeTest")]
        public void T024_LanguageBlockScope_ImperativeParallelAssociative_Function()
        {

            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string src = @"z;
[Imperative]
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("z").DsasmValue.IsNull);
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("z").DsasmValue.IsNull);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }

        [Test]
        [Category("SmokeTest")]
        public void T026_LanguageBlockScope_ImperativeParallelImperative_Function()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string src = @"z;
[Imperative]
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("z").DsasmValue.IsNull);
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("z_1").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("z_2").DsasmValue.IsNull);
            //});
            //Assert.Fail("Sprint 15: Rev 617: Scope: Need sensible error message to show the user that function called in a parallel language block is not defined. ");
        }

        [Test]
        [Category("SmokeTest")]
        public void T028_LanguageBlockScope_MultipleParallelLanguageBlocks_IAI_Function()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string src = @"z_1;
z_2;
[Imperative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
	a = 10;
	
}
[Associative]	
{
	x_1 = 20;
	y_1 = 0;
	z_1 = foo (x_1, y_1);
	
}
[Imperative]
{
	x_2 = 20;
	y_2 = 0;
	z_2 = foo (x_2, y_2);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("z_1").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("z_2").DsasmValue.IsNull);
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
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("z_I1").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("z_I2").Payload == -12);
            Assert.IsTrue((Int64)mirror.GetValue("z_A1").Payload == 18);
            Assert.IsTrue((Int64)mirror.GetValue("z_A2").Payload == 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T030_LanguageBlockScope_ParallelInsideNestedBlock_ImperativeNested_AA()
        {
            string code = @"
z_A1;
z_I1;
z_A2;
z_I2;
[Imperative]
{
	def foo : int(a : int, b : int)
	{
		return = a - b;
	}
	 
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
        public void T031_Defect_1450594()
        {
            string src = @"f;p;q;x;y1;z;y2;
[Imperative]
{
   a = 2;
    [Associative]
    {
        
        i = 3;
    }
    f = i;
}
[Associative]
{
	def foo1 ( i )
	{
		x = 1;
		return = x;
	}
	p = x;
	q = a;
}
y = 1;
[Imperative]
{
   def foo ( i )
   {
		x = 2;
		if( i < x ) 
		{
		    y = 3;
			return = y * i;
		}
		return = y;
	}
	x = y;
	y1 = foo ( 1 );
	y2 = foo ( 3 );
	z = x * 2;
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            Assert.IsTrue(mirror.GetValue("f").DsasmValue.IsNull);
            
            Assert.IsTrue((Int64)mirror.GetValue("p").Payload == 2);
            Assert.IsTrue(mirror.GetValue("q").DsasmValue.IsNull);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("y1").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("y2").Payload == 3);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);

        }

        [Test]
        [Category("SmokeTest")]
        public void T032_Cross_Language_Variables()
        {
            string src = @"
a = 5;
b = 2 * a;
count;
[Imperative] {
	count = 0;
	arr = 0..b;
	for (i  in arr) {
		count = count + 1;
	}
}
a = 10;
// expected: count = 21
// result: count = 11";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("count").Payload == 21);

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
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
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
             @"@a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("@a").Payload == 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_32()
        {
            String code =
               @"_a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("_a").Payload == 1);
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
        public void T050_Test_Identifier_Name_Tailer_31()
        {
            String code =
                @"a@ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a@").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T050_Test_Identifier_Name_Tailer_32()
        {
            String code =
               @"a_ = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a_").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_01()
        {
            String code =
  @"class A
{
  @a : var;
  constructor A(@b:var)
    {
        @a = @b;
    }
  def foo(@c:var)
  {
    @a = @c;
    return = @a;
  }
}
  @a = 1;
  p = A.A(2);
  @t = 3;
  @a = p.foo(@t);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("@a").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_02()
        {
            String code =
              @"
def foo(@a:var)
{
  @b = @a + 1;
  return = @b;
}
@t = 1;
@c = foo(@t);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("@c").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_03()
        {
            String code =
              @"
@c = [Imperative]
{
  @a = 10;
  @b = {10,20,30};
  for (@i in @b)
  {
      @a = @a + @i;
  }
  return = @a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("@c").Payload == 70);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_04()
        {
            String code =
              @"
@c = [Imperative]
{
  @a = 20;
  @b = 10;
  if(@a > 0){
    @a = @a - @b;
    @b = 20;
  }else
  {
    @b = 30;
  }
  return = @a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("@c").Payload == 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_05()
        {
            String code =
              @"
@b;
[Associative]
{
	@a = 1;
	@b = @a + 2;
	@a = 3;
          
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("@b").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_06()
        {
            String code =
        @"
@c;
[Imperative]
{	
   @a = 1;
   @b = 2;
   @c = @a < @b ? @a : @b;			
}
                        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("@c").Payload == 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T051_Test_Identifier_Scope_07()
        {
            String code =
        @"
@a;
[Imperative]
{	
   @a1 = 1;
   @a2 = 5;
   @a3 = 1;
   @a = @a1..@a2..@a3;
			
}
                        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            List<Object> result = new List<Object> { 1, 2, 3, 4, 5 };
            Assert.IsTrue(mirror.CompareArrays("@a", result, typeof(System.Double)));
        }


        [Test]
        [Category("SmokeTest")]
        public void T052_DNL_1467464()
        {
            string code = @"
class test
{
    f;
    constructor test()
    {
    [Associative]
        {
            [Imperative]
            {
                i = 3;
            }
            f = i;
        }
    }
}
a = test.test();
b = a.f;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", null);
        }
    }
}
