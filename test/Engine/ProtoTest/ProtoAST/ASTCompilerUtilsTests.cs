using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using NUnit.Framework;
using ProtoCore;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoCore.AST.AssociativeAST;
using ProtoTest.TD;
using ProtoScript.Runners;
using ProtoTestFx.TD;

namespace ProtoTest.ProtoAST
{
    class ASTCompilerUtilsTests : ProtoTestBase
    {
        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestBuildAST_01()
        {
            //==============================================
            //
            // import("ProtoGeometry.dll");
            // p = Point.Bycoordinates(0.0, 2.0, 1.0);
            // xval = p.X;
            //
            //==============================================


            //==============================================
            // Build the import Nodes
            //==============================================
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            List<string> libs = new List<string>();
            libs.Add("ProtoGeometry.dll");
            liveRunner.ResetVMAndImportLibrary(libs);

            string type = "Point";
            long hostInstancePtr = 0;
            string functionName = "ByCoordinates";
            List<IntPtr> userDefinedArgs = null;
            List<string> primitiveArgs = new List<string>();
            primitiveArgs.Add("0");
            primitiveArgs.Add("2");
            primitiveArgs.Add("1");
            string formatString = "ddd";
            string symbolName = "";
            string code = ""; 

            AssociativeNode assign1 = ASTCompilerUtils.BuildAST(type, hostInstancePtr, functionName, userDefinedArgs, primitiveArgs, formatString, liveRunner.Core, 
                ref symbolName, ref code);

            liveRunner.UpdateGraph(assign1);

            primitiveArgs.Clear();
            primitiveArgs.Add("10");
            primitiveArgs.Add("0");
            primitiveArgs.Add("0");
            
            functionName = "Translate";
            AssociativeNode assign2 = ASTCompilerUtils.BuildAST(symbolName, hostInstancePtr, functionName, userDefinedArgs, primitiveArgs, formatString, liveRunner.Core,
                ref symbolName, ref code);

            liveRunner.UpdateGraph(assign2);

            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = p.X;
            //==============================================
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListNode = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListNode.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode(symbolName);
            identListNode.Optr = ProtoCore.DSASM.Operator.dot;
            identListNode.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("X");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmt2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("xval"),
                identListNode,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(stmt2);
            //==============================================
            //
            // import("ProtoGeometry.dll");
            // p = Point.Bycoordinates(0.0, 20.0, 1.0);
            // q = p.Translate(10.0, 0.0, 0.0);
            // xval = p.X;
            //
            //==============================================

            // update graph
            CodeBlockNode cNode = new CodeBlockNode();
            cNode.Body = astList;
            liveRunner.UpdateGraph(cNode);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("xval");
            Assert.IsTrue((double)mirror.GetData().Data == 10.0);
        }
    }
}
