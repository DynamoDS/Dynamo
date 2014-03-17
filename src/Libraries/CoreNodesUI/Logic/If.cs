using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.AST.ImperativeAST;
using CodeBlockNode = ProtoCore.AST.AssociativeAST.CodeBlockNode;
using LanguageBlockNode = ProtoCore.AST.AssociativeAST.LanguageBlockNode;

namespace DSCoreNodesUI.Logic
{
    [NodeName("If")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Conditional statement")]
    [IsDesignScriptCompatible]
    public class If : NodeModel
    {
        public If()
        {
            InPortData.Add(new PortData("test", "Test block"));
            InPortData.Add(new PortData("true", "True block"));
            InPortData.Add(new PortData("false", "False block"));

            OutPortData.Add(new PortData("result", "Result"));

            RegisterAllPorts();

            //TODO: Default Values
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //var guidStr = GUID.ToString().Replace("-", "");
            //var testTmp = "__temp_test_" + guidStr;
            //var trueTmp = "__temp_true_" + guidStr;
            //var falseTmp = "__temp_false_" + guidStr;

            return new[]
            {
                //First, assign out inputs to temp variables, so we can cross from Associative to Imperative.
                //AstFactory.BuildAssignment(AstFactory.BuildIdentifier(testTmp), inputAstNodes[0]),
                //AstFactory.BuildAssignment(AstFactory.BuildIdentifier(trueTmp), inputAstNodes[1]),
                //AstFactory.BuildAssignment(AstFactory.BuildIdentifier(falseTmp), inputAstNodes[2]),

                // <output> = [Imperative]
                // {
                //   if (<test>)
                //   {
                //     return = <true>;
                //   }
                //   return = <false>;
                // };
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    new LanguageBlockNode
                    {
                        codeblock = new LanguageCodeBlock(Language.kImperative),
                        CodeBlockNode = new ProtoCore.AST.ImperativeAST.CodeBlockNode
                        {
                            Body = new List<ImperativeNode>
                            {
                                new IfStmtNode
                                {
                                    IfExprNode = inputAstNodes[0].ToImperativeAST(),
                                    IfBody = new List<ImperativeNode>
                                    {
                                        new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                                            new ProtoCore.AST.ImperativeAST.IdentifierNode("return"),
                                            inputAstNodes[1].ToImperativeAST(),
                                            ProtoCore.DSASM.Operator.assign)
                                    }
                                },
                                new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                                    new ProtoCore.AST.ImperativeAST.IdentifierNode("return"),
                                    inputAstNodes[2].ToImperativeAST(),
                                    ProtoCore.DSASM.Operator.assign)
                            }
                        }
                    })
            };
        }
    }
}
