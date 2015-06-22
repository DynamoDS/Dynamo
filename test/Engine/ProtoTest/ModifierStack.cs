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
        [Category("ModifierBlock")] [Category("Failure")]
        public void SimpleExpr()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.Execute(
                        @"
                        [Associative]
                        {
                            a = 10;
                        }
                        ", core, out runtimeCore);
        }
        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void SimpleFuncDef()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.Execute(
                        @"
                        [Associative]
                        {
                            def foo : int (b : int)
                            {
                                return = 2;
                            }
                            x = foo(2);
                        }
                        ", core, out runtimeCore);
        }
        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void SimpleExprInModifierStack()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
                        @"
a;
                        [Associative]
                        {
                            a = 
                                {
                                    10;
                                }
                        }
                        ", core, out runtimeCore);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 10);

        }
        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void TwoSimpleExprInModifierStack()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
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
                        ", core, out runtimeCore);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 20);
        }
        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void TwoExprInModifierStackWithOp()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
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
                        ", core, out runtimeCore);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 60);
        }


        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void ModifierStackWithName()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
                        @"
a;a@init;
                        [Associative]
                        {
                            a = 
                                {
                                2 => a@init;
                                +4;
                                -3;
                                *2;
                                 }
                        }
                        ", core, out runtimeCore);
            Assert.IsTrue((Int64)mirror.GetValue("a@init", 0).Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 6);
        }
        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void ModifierStackWithTwoNames()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
                        @"
a;a@init;a@first;
                        [Associative]
                        {
                            a = 
                                {
                                    3 => a@init;
                                    +1 => a@first;
                                    +a@first;
                                    *2;
                                }
                        }
                        ", core, out runtimeCore);
            Assert.IsTrue((Int64)mirror.GetValue("a@init", 0).Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("a@first", 0).Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 16);
        }
        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void ModifierStackWithArray()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
                        @"
a;
a@init;
a@first;
                        [Associative]
                        {
                            a = 
                                {
                                    {3, 2, 1} => a@init;
                                    1 => a@first;
                                }
                        }
                        ", core, out runtimeCore);

            Obj o = mirror.GetValue("a@init");
            List<Obj> os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os[0].Payload == 3);
            Assert.IsTrue((Int64)os[1].Payload == 2);
            Assert.IsTrue((Int64)os[2].Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("a@first", 0).Payload == 1);
        }
        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void ModifierStackWithArrayAndFunction()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
                        @"a@init;a@first;b;
                        [Associative]
                         {
	                         def foo : int(x : int)
	                         {
		                        a = x+2;
		                        return = a;
	                         }
                             b = 
                                 {
                                     {3, 2, 1} => a@init;
                                     foo(7) => a@first;
                                 }
                         }
                        ", core, out runtimeCore);
            Obj o = mirror.GetValue("a@init");
            List<Obj> os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os[0].Payload == 3);
            Assert.IsTrue((Int64)os[1].Payload == 2);
            Assert.IsTrue((Int64)os[2].Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("a@first", 0).Payload == 9);
            Assert.IsTrue((Int64)mirror.GetValue("b", 0).Payload == 9);
        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void ModifierStackWithArrayAndFunction2()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
                        @"b;a@init;a@first;
                         [Associative]
                         {
	                         def foo : int(x : int)
	                         {
		                        a = x+2;
		                        return = a;
	                         }
                             b = 
                                 {
                                     8 => a@init;
                                     foo(a@init) => a@first;
                                 }
                         }
                        ", core, out runtimeCore);
            Assert.IsTrue((Int64)mirror.GetValue("a@init", 0).Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("a@first", 0).Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("b", 0).Payload == 10);
        }
        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void ModifierStackWithArrayAndFunctionReplication()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(
                        @"a@init;a@first;b;
                         [Associative]
                         {
	                         def foo : int(x : int)
	                         {
		                        a = x+2;
		                        return = a;
	                         }
                             b = 
                                 {
                                     {1,2,3} => a@init;
                                     foo(a@init) => a@first;
                                 }
                         }
                        ", core, out runtimeCore);
            Obj o = mirror.GetValue("a@init");
            List<Obj> os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os[0].Payload == 1);
            Assert.IsTrue((Int64)os[1].Payload == 2);
            Assert.IsTrue((Int64)os[2].Payload == 3);
            o = mirror.GetValue("a@first");
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
        [Category("ModifierBlock")] [Category("Failure")]
        public void ClassTest()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
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
                            ", core, out runtimeCore);
            //Object o = mirror.GetValue("point.mx");
            //Assert.IsTrue((long)o == 10);
        }
    }
}
