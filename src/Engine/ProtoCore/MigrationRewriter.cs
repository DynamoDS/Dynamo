using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamoServices;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.DSASM;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Utils;

namespace ProtoCore
{
    public class MigrationRewriter : AstReplacer
    {
        private readonly IDictionary<string, string> priorNames;
        private readonly LogWarningHandler logWarningHandler;

        public delegate void LogWarningHandler(string oldNodeName, string newNodeName);

        private MigrationRewriter(IDictionary<string, string> priorNames, LogWarningHandler logWarningHandler)
        {
            this.priorNames = priorNames;
            this.logWarningHandler = logWarningHandler;
        }

        /// <summary>
        /// Migrates old method names to new names based on priorNameHints from LibraryServices
        /// </summary>
        /// <param name="astNodes"></param>
        /// <param name="priorNames">dictionary of old names vs. new names for node migration</param>
        /// <param name="logWarningHandler"></param>
        /// <returns>migrated AST nodes after method renaming</returns>
        public static IEnumerable<Node> MigrateMethodNames(IEnumerable<Node> astNodes, IDictionary<string, string> priorNames, 
            LogWarningHandler logWarningHandler)
        {
            var rewriter = new MigrationRewriter(priorNames, logWarningHandler);
            return astNodes.OfType<AssociativeNode>().Select(astNode => astNode.Accept(rewriter)).Cast<Node>().ToList();
        }

        public override AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
        {
            if (node == null)
                return null;

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

            OnLogWarning(oldNodeName, newNodeName);

            return GenerateNewIdentifierList(newNodeName, rightNode);
        }

        public override AssociativeNode VisitFunctionCallNode(FunctionCallNode node)
        {
            if (node == null)
                return null;

            var functionName = node.Function.Name;

            string newNodeName;
            if (!priorNames.TryGetValue(functionName, out newNodeName)) return node;

            OnLogWarning(functionName, newNodeName);

            return GenerateNewIdentifierList(newNodeName, node);
        }

        private void OnLogWarning(string oldNodeName, string newNodeName)
        {
            var handler = logWarningHandler;
            if (handler != null) handler(oldNodeName, newNodeName);
        }

        private IdentifierListNode GenerateNewIdentifierList(string newNodeName, FunctionCallNode funcCall)
        {
            var newNode = CoreUtils.CreateNodeFromString(newNodeName);
            if (newNode == null)
                return null;

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
