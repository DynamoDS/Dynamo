using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;
using ProtoTestFx.TD;

namespace ProtoTest.GraphCompiler
{
    [TestFixture]
    class NewFrontEndTests : ProtoTestBase
    {
        [Test]
        public void ReproMAGN3603()
        {

            string code = @"a = 1 + (2 * 3);
                            b = (1 + 2) * 3;
                            c = 1 + 2 * 3;";

            ParseParam parseParam = new ParseParam(Guid.NewGuid(), code);
            Assert.IsTrue(CompilerUtils.PreCompileCodeBlock(thisTest.CreateTestCore(), ref parseParam));
            Assert.IsTrue(parseParam.ParsedNodes != null && parseParam.ParsedNodes.Count() > 0);

            var parsedNode = parseParam.ParsedNodes.ElementAt(0);

            BinaryExpressionNode n = parsedNode as BinaryExpressionNode;
            FunctionCallNode funcCall = n.RightNode as FunctionCallNode;
            Assert.IsTrue(n != null && funcCall != null);
            IdentifierNode identNode = funcCall.Function as IdentifierNode;
            Assert.IsTrue(identNode != null && identNode.Value == "%add");
            var args = funcCall.FormalArguments;
            Assert.IsTrue(args.Count == 2);
            Assert.IsTrue(args[0] is IntNode);
            FunctionCallNode nestedFuncCall = args[1] as FunctionCallNode;
            Assert.IsTrue(nestedFuncCall != null && (nestedFuncCall.Function as IdentifierNode).Value == "%mul");

            parsedNode = parseParam.ParsedNodes.ElementAt(1);

            n = parsedNode as BinaryExpressionNode;
            funcCall = n.RightNode as FunctionCallNode;
            Assert.IsTrue(n != null && funcCall != null);
            identNode = funcCall.Function as IdentifierNode;
            Assert.IsTrue(identNode != null && identNode.Value == "%mul");
            args = funcCall.FormalArguments;
            Assert.IsTrue(args.Count == 2);
            Assert.IsTrue(args[1] is IntNode);
            nestedFuncCall = args[0] as FunctionCallNode;
            Assert.IsTrue(nestedFuncCall != null && (nestedFuncCall.Function as IdentifierNode).Value == "%add");

            parsedNode = parseParam.ParsedNodes.ElementAt(2);

            n = parsedNode as BinaryExpressionNode;
            funcCall = n.RightNode as FunctionCallNode;
            Assert.IsTrue(n != null && funcCall != null);
            identNode = funcCall.Function as IdentifierNode;
            Assert.IsTrue(identNode != null && identNode.Value == "%add");
            args = funcCall.FormalArguments;
            Assert.IsTrue(args.Count == 2);
            Assert.IsTrue(args[0] is IntNode);
            nestedFuncCall = args[1] as FunctionCallNode;
            Assert.IsTrue(nestedFuncCall != null && (nestedFuncCall.Function as IdentifierNode).Value == "%mul");
        }

        
    }
}
