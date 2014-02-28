using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using GraphToDSCompiler;

namespace ProtoTest.GraphCompiler
{

    class MicroFeatureTests
    {
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestAddition01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateOperatorNode(1, "+");
            object o1 = 10;
            gc.CreateLiteralNode(2,o1);
            object o2 = 11;
            gc.CreateLiteralNode(3, o2);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 1, 1);
            string mmx = gc.GetGraph2String();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("temp20001");
            Assert.IsTrue((Int64)o.Payload == 21);
        }
        [Test]
        public void TestSubtraction01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateOperatorNode(1, "-");
            object o1 = 10;
            gc.CreateLiteralNode(2, o1);
            object o2 = 11;
            gc.CreateLiteralNode(3, o2);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 1, 1);
            string mmx = gc.GetGraph2String();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("temp20001");
            Assert.IsTrue((Int64)o.Payload == -1);
        }
        [Test]
        public void TestMultiplication01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateOperatorNode(1, "*");
            object o1 = 10;
            gc.CreateLiteralNode(2, o1);
            object o2 = 11;
            gc.CreateLiteralNode(3, o2);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 1, 1);
            string mmx = gc.GetGraph2String();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("temp20001");
            Assert.IsTrue((Int64)o.Payload == 110);
        }
        [Test, Ignore]
        public void TestDivision01()
        {
            GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
            gc.CreateOperatorNode(1, "/");
            object o1 = 10;
            gc.CreateLiteralNode(2, o1);
            object o2 = 11;
            gc.CreateLiteralNode(3, o2);
            gc.ConnectNodes(2, 0, 1, 0);
            gc.ConnectNodes(3, 0, 1, 1);
            string mmx = gc.GetGraph2String();
            mmx = mmx.Trim();
            ExecutionMirror mirror = thisTest.RunScriptSource(mmx);
            Obj o = mirror.GetValue("temp20001");
            Assert.IsTrue((Double)o.Payload == (10/11));
        }
        //{
        //    GraphToDSCompiler.GraphCompiler gc = GraphToDSCompiler.GraphCompiler.CreateInstance();
        //    gc.CreateFunctionNode(1, "Math.Sin");
        //    object o1 = 123.4;
        //    gc.CreateLiteralNode(2, o1);
        //    gc.ConnectNodes(2, 0, 1, 0);
        //    string mmx = gc.GetGraph2String();
        //    mmx = mmx.Trim();
        //    Assert.IsTrue(mmx.Equals("temp20001=Math.Sin( 123.4 );"));
        //    Console.WriteLine(mmx);
        
        //}
    }
}
