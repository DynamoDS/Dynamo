using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Utils;

namespace ProtoCore
{
    public class MigrationRewriter : AstReplacer
    {
        private readonly IDictionary<string, string> priorNames;
        public MigrationRewriter(IDictionary<string, string> priorNames)
        {
            this.priorNames = priorNames;
        }

        public static IEnumerable<Node> MigrateMethodNames(IEnumerable<Node> astNodes, IDictionary<string, string> priorNames)
        {
            var rewriter = new MigrationRewriter(priorNames);
            return astNodes.OfType<AssociativeNode>().Select(astNode => astNode.Accept(rewriter)).Cast<Node>().ToList();
        }

        public override AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
        {
            var leftNodeName = node.LeftNode.ToString();
            var rightNode = node.RightNode as FunctionCallNode;
            string rightNodeName = string.Empty;
            if (rightNode != null)
            {
                rightNodeName = rightNode.Function.Name;
            }
            var oldNodeName = leftNodeName + '.' + rightNodeName;
            string newNodeName;
            if(!priorNames.TryGetValue(oldNodeName, out newNodeName)) return node;

            return GenerateNewIdentifierList(newNodeName, rightNode);
        }

        public override AssociativeNode VisitFunctionCallNode(FunctionCallNode node)
        {
            var functionName = node.Function.Name;

            string newNodeName;
            if (!priorNames.TryGetValue(functionName, out newNodeName)) return node;

            return GenerateNewIdentifierList(newNodeName, node);
        }

        private IdentifierListNode GenerateNewIdentifierList(string newNodeName, FunctionCallNode funcCall)
        {
            var newNode = CoreUtils.CreateNodeFromString(newNodeName);

            // append argument list from original method to newNode
            var newMethodName = ((IdentifierListNode)newNode).RightNode.Name;

            var newMethod = new FunctionCallNode
            {
                Function = AstFactory.BuildIdentifier(newMethodName),
                FormalArguments = funcCall.FormalArguments
            };
            var newIdentList = new IdentifierListNode
            {
                LeftNode = ((IdentifierListNode)newNode).LeftNode,
                RightNode = newMethod,
                Optr = Operator.dot
            };

            return newIdentList;
        }
    }
}
