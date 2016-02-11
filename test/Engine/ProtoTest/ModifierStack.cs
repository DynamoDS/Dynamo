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
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 10);

        }
        [Test]
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
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 20);
        }
        [Test]
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
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 60);
        }


        [Test]
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
            Assert.IsTrue((Int64)mirror.GetValue("ainit", 0).Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 6);
        }
        [Test]
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
            Assert.IsTrue((Int64)mirror.GetValue("ainit", 0).Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("afirst", 0).Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 16);
        }
        [Test]
        [Category("ModifierBlock")]
        public void ModifierStackWithArray()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore  = fsr.Execute(
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
                        ", core);

            ExecutionMirror mirror = runtimeCore.Mirror;
            Obj o = mirror.GetValue("ainit");
            List<Obj> os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os[0].Payload == 3);
            Assert.IsTrue((Int64)os[1].Payload == 2);
            Assert.IsTrue((Int64)os[2].Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("afirst", 0).Payload == 1);
        }
        [Test]
        [Category("ModifierBlock")] 
        public void ModifierStackWithArrayAndFunction()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore  = fsr.Execute(
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
                        ", core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            Obj o = mirror.GetValue("ainit");
            List<Obj> os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os[0].Payload == 3);
            Assert.IsTrue((Int64)os[1].Payload == 2);
            Assert.IsTrue((Int64)os[2].Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("afirst", 0).Payload == 9);
            Assert.IsTrue((Int64)mirror.GetValue("b", 0).Payload == 9);
        }

        [Test]
        [Category("ModifierBlock")] 
        public void ModifierStackWithArrayAndFunction2()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore  = fsr.Execute(
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
                        ", core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            Assert.IsTrue((Int64)mirror.GetValue("ainit", 0).Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("afirst", 0).Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("b", 0).Payload == 10);
        }
        [Test]
        [Category("ModifierBlock")] 
        public void ModifierStackWithArrayAndFunctionReplication()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore  = fsr.Execute(
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
                        ", core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            Obj o = mirror.GetValue("ainit");
            List<Obj> os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os[0].Payload == 1);
            Assert.IsTrue((Int64)os[1].Payload == 2);
            Assert.IsTrue((Int64)os[2].Payload == 3);
            o = mirror.GetValue("afirst");
            os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os[0].Payload == 3);
            Assert.IsTrue((Int64)os[1].Payload == 4);
            Assert.IsTrue((Int64)os[2].Payload == 5);
            o = mirror.GetValue("b");
            os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os[0].Payload == 3);
            Assert.IsTrue((Int64)os[1].Payload == 4);
            Assert.IsTrue((Int64)os[2].Payload == 5);
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
