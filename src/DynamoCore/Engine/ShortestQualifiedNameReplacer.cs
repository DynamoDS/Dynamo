using System.Linq;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Namespace;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Utils;


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
            return node;
        }

        public override AssociativeNode VisitIdentifierNode(IdentifierNode node)
        {
            var name = node.ToString();
            AssociativeNode iNode;
            string[] strIdentList = name.Split('.');

            if (strIdentList.Length == 1)
            {
                return new IdentifierNode(strIdentList[0]);
            }

            var rightNode = new IdentifierNode(strIdentList.Last());
            string ident = "";
            for (int i = 0; i < strIdentList.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(ident))
                    ident += ".";
                ident += strIdentList[i];
            }
            var leftNode = new IdentifierNode(ident);

            var identListNode = new IdentifierListNode
            {
                LeftNode = leftNode,
                RightNode = rightNode,
                Optr = Operator.dot
            };
            return VisitIdentifierListNode(identListNode);
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

            if(qualifiedName != node.ToString())
                return false;

            string className = qualifiedName.Split('.').Last();

            //if (matchingClasses.Length == 1)
            //{
            //    className = matchingClasses[0];
            //}
            shortNameNode = CreateNodeFromShortName(className, qualifiedName);
            return shortNameNode != null;
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
                shortName = resolver?.LookupShortName(qualifiedName);

                if (string.IsNullOrEmpty(shortName))
                {
                    // Use the namespace list as input to derive the list of shortest unique names
                    var symbolList =
                        matchingClasses.Select(matchingClass => new Symbol(matchingClass)).ToList();
                    var shortNames = Symbol.GetShortestUniqueNames(symbolList);

                    // Get the shortest name corresponding to the fully qualified name
                    shortName = shortNames[new Symbol(qualifiedName)];
                }
            }
            // Rewrite the AST using the shortest name
            return CoreUtils.CreateNodeFromString(shortName);

        }
    }
}
