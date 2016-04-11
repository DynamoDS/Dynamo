using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
namespace ProtoTest
{
    [TestFixture]
    class ModifierStackTests : ProtoTestBase
    {
        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void SimpleExpr()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
                        @"
                        [Associative]
                        {
                            a = 10;
                        }
                        ", core);
        }
        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void SimpleFuncDef()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
                        @"
                        [Associative]
                        {
                            def foo : int (b : int)
                            {
                                return = 2;
                            }
                            x = foo(2);
                        }
                        ", core);
        }
        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void SimpleExprInModifierStack()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
                        @"
a;
                        [Associative]
                        {
                            a = 
                                {
                                    10;
                                }
                        }
                        ", core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            thisTest.Verify("a", 10);

        }
        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void TwoSimpleExprInModifierStack()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore  = fsr.Execute(
                        @"
a;
                        [Associative]
                        {
                            a = 
                                {
                                    10;
                                    20;
                                }
                        }
                        ", core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            thisTest.Verify("a", 20);
        }
        [Test]
        [Ignore]
        [Category("ModifierBlock")]
        public void TwoExprInModifierStackWithOp()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore  = fsr.Execute(
                        @"
a;
                        [Associative]
                        {
                            a = 
                                {
                                    10;
                                    +20;
                                    *2;
                                }
                        }
                        ", core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            thisTest.Verify("a", 60);
        }


        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void ModifierStackWithName()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore  = fsr.Execute(
                        @"
a;ainit;
                        [Associative]
                        {
                            a = 
                                {
                                2 => ainit;
                                +4;
                                -3;
                                *2;
                                 }
                        }
                        ", core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            thisTest.Verify("ainit", 2);
            thisTest.Verify("a", 6);
        }
        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void ModifierStackWithTwoNames()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore  = fsr.Execute(
                        @"
a;ainit;afirst;
                        [Associative]
                        {
                            a = 
                                {
                                    3 => ainit;
                                    +1 => afirst;
                                    +afirst;
                                    *2;
                                }
                        }
                        ", core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            thisTest.Verify("ainit", 3);
            thisTest.Verify("afirst", 4);
            thisTest.Verify("a", 16);
        }
        [Test]
        [Ignore]
        [Category("ModifierBlock")]
        public void ModifierStackWithArray()
        {
            string code = 
                        @"
a;
ainit;
afirst;
                        [Associative]
                        {
                            a = 
                                {
                                    {3, 2, 1} => ainit;
                                    1 => afirst;
                                }
                        }
                        ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("ainit", new[] { 3, 2, 1 });
            thisTest.Verify("afirst", 1);
        }
        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void ModifierStackWithArrayAndFunction()
        {
            string code = 
                        @"ainit;afirst;b;
                        [Associative]
                         {
	                         def foo : int(x : int)
	                         {
		                        a = x+2;
		                        return = a;
	                         }
                             b = 
                                 {
                                     {3, 2, 1} => ainit;
                                     foo(7) => afirst;
                                 }
                         }
                        ";
            thisTest.Verify("ainit", new[] { 3, 2, 1 });
            thisTest.Verify("afirst", 9);
            thisTest.Verify("b", 9);
        }

        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void ModifierStackWithArrayAndFunction2()
        {
            string code = 
                        @"b;ainit;afirst;
                         [Associative]
                         {
	                         def foo : int(x : int)
	                         {
		                        a = x+2;
		                        return = a;
	                         }
                             b = 
                                 {
                                     8 => ainit;
                                     foo(ainit) => afirst;
                                 }
                         }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ainit", 8);
            thisTest.Verify("afirst", 10);
            thisTest.Verify("b", 10);
        }
        [Test]
        [Ignore]
        [Category("ModifierBlock")] 
        public void ModifierStackWithArrayAndFunctionReplication()
        {
           string code =  
                        @"ainit;afirst;b;
                         [Associative]
                         {
	                         def foo : int(x : int)
	                         {
		                        a = x+2;
		                        return = a;
	                         }
                             b = 
                                 {
                                     {1,2,3} => ainit;
                                     foo(ainit) => afirst;
                                 }
                         }
                        ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ainit", new object[] { 1, 2, 3 });
            thisTest.Verify("afirst", new object[] {3, 4, 5});
            thisTest.Verify("b", new object[] {3, 4, 5});
        }
        [Test]
        [Ignore][Category("DSDefinedClass_Ignored_EmptyTest")]
        [Category("ModifierBlock")] 
        public void ClassTest()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            fsr.Execute(
                       @"
                            [Associative]
                            {
                                class Point
                              {		
                                  mx : var;
                                  my : var;
                                  mz : var;
                                  constructor Point(xx : double, yy : double, zz : double)
                                  {
                                      mz = xx;
                                      my = yy;
                                      mx = zz;
                                  }
                              }
                                point = Point.Point(10,10,10);
                            }
                            ", core);
            //Object o = mirror.GetValue("point.mx");
            //Assert.IsTrue((long)o == 10);
        }
    }
}
