using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Namespace;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Utils;
using System.Linq;


namespace Dynamo.Engine
{
    /// <summary>
    /// Replace a fully qualified function call with short name. 
    /// </summary>
    internal class ShortestQualifiedNameReplacer : AstReplacer
    {
        private readonly ClassTable classTable;
        private readonly ElementResolver resolver;

        public ShortestQualifiedNameReplacer(ClassTable classTable, ElementResolver resolver)
        {
            this.classTable = classTable;
            this.resolver = resolver;
        }

        public override AssociativeNode VisitTypedIdentifierNode(TypedIdentifierNode node)
        {
            if (node == null)
                return null;

            // If type is primitive type
            if (node.datatype.UID != (int)PrimitiveType.InvalidType &&
                node.datatype.UID < (int)PrimitiveType.MaxPrimitive)
                return node;

            // build an idlistnode from the full type name
            var identListNode = CoreUtils.CreateNodeFromString(node.TypeAlias);
            if (identListNode == null)
                return node;

            // visit the idlist 
            var shortNameNode = identListNode.Accept(this);
            if (shortNameNode == null)
                return node;


            var resultingNode = new TypedIdentifierNode
            {
                Name = node.Name,
                Value = node.Name,
                datatype = node.datatype
            };


            //return a typedIdNode built from the shortNameId returned above.
            resultingNode.TypeAlias = shortNameNode.ToString();

            //modify the AST node that was passed to the visitor.
            node.Name = resultingNode.Name;
            node.Value = resultingNode.Value;
            node.datatype = resultingNode.datatype;
            node.TypeAlias = resultingNode.TypeAlias;

            return node;
        }


        public override AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
        {
            if (node == null)
                return null;

            // First pass attempt to resolve the node class name 
            // and shorten it before traversing it deeper
            AssociativeNode shortNameNode;
            if (TryShortenClassName(node, out shortNameNode))
            {
                return shortNameNode;
            }

            var rightNode = node.RightNode;
            var leftNode = node.LeftNode;

            rightNode = rightNode.Accept(this);
            var newLeftNode = leftNode.Accept(this);

            node = new IdentifierListNode
            {
                LeftNode = newLeftNode,
                RightNode = rightNode,
                Optr = Operator.dot
            };
            return leftNode != newLeftNode ? node : RewriteNodeWithShortName(node);
        }

        private bool TryShortenClassName(IdentifierListNode node, out AssociativeNode shortNameNode)
        {
            shortNameNode = null;

            string qualifiedName = CoreUtils.GetIdentifierExceptMethodName(node);

            // if it is a global method with no class
            if (string.IsNullOrEmpty(qualifiedName))
                return false;

            // Make sure qualifiedName is not a property
            var matchingClasses = classTable.GetAllMatchingClasses(qualifiedName);
            if (matchingClasses.Length == 0)
                return false;

            string className = qualifiedName.Split('.').Last();

            var symbol = new ProtoCore.Namespace.Symbol(qualifiedName);
            if (!symbol.Matches(node.ToString()))
                return false;

            shortNameNode = CreateNodeFromShortName(className, qualifiedName);
            return shortNameNode != null;
        }

        private IdentifierListNode RewriteNodeWithShortName(IdentifierListNode node)
        {
            // Get class name from AST
            string qualifiedName = CoreUtils.GetIdentifierExceptMethodName(node);

            // if it is a global method
            if (string.IsNullOrEmpty(qualifiedName))
                return node;

            // Make sure qualifiedName is not a property
            var lNode = node.LeftNode;
            var matchingClasses = classTable.GetAllMatchingClasses(qualifiedName);
            while (matchingClasses.Length == 0 && lNode is IdentifierListNode)
            {
                qualifiedName = lNode.ToString();
                matchingClasses = classTable.GetAllMatchingClasses(qualifiedName);
                lNode = ((IdentifierListNode)lNode).LeftNode;
            }
            qualifiedName = lNode.ToString();
            string className = qualifiedName.Split('.').Last();

            var newIdentList = CreateNodeFromShortName(className, qualifiedName);
            if (newIdentList == null)
                return node;

            // Replace class name in input node with short name (newIdentList)
            node = new IdentifierListNode
            {
                LeftNode = newIdentList,
                RightNode = node.RightNode,
                Optr = Operator.dot
            };
            return node;
        }

        private AssociativeNode CreateNodeFromShortName(string className, string qualifiedName)
        {
            // Get the list of conflicting namespaces that contain the same class name
            var matchingClasses = CoreUtils.GetResolvedClassName(classTable, AstFactory.BuildIdentifier(className));
            if (matchingClasses.Length == 0)
                return null;

            string shortName;
            // if there is no class conflict simply use the class name as the shortest name
            if (matchingClasses.Length == 1)
            {
                shortName = className;
            }
            else
            {
                shortName = resolver != null ? resolver.LookupShortName(qualifiedName) : null;

                if (string.IsNullOrEmpty(shortName))
                {
                    // Use the namespace list as input to derive the list of shortest unique names
                    var symbolList =
                        matchingClasses.Select(matchingClass => new ProtoCore.Namespace.Symbol(matchingClass))
                            .ToList();
                    var shortNames = ProtoCore.Namespace.Symbol.GetShortestUniqueNames(symbolList);

                    // Get the shortest name corresponding to the fully qualified name
                    shortName = shortNames[new ProtoCore.Namespace.Symbol(qualifiedName)];
                }
            }
            // Rewrite the AST using the shortest name
            var newIdentList = CoreUtils.CreateNodeFromString(shortName);
            return newIdentList;

        }
    }
}
