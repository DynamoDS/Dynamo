using System.Linq;
using System.Text;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Namespace;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Utils;


namespace Dynamo.Engine
{
    /// <summary>
    /// Replaces a fully qualified class name with a short name.
    /// Ensures that fully qualified namespaces of classes with the same name are
    /// automatically shortened to partial namespaces so that they can still be resolved uniquely.
    /// E.g., given {"A.B.C.D.E", "X.Y.A.B.E.C.E", "X.Y.A.C.B.E"}, all with the same class E,
    /// their shortest unique names would be: {"D.E", "E.E", "C.B.E"}.
    /// 
    /// Also replaces DesignScript.BuiltIn.Get.ValueAtIndex(exp1, exp2) calls to exp1[exp2] expressions.
    /// </summary>
    internal class ShortestQualifiedNameReplacer : AstReplacer
    {
        private readonly ClassTable classTable;
        private readonly ElementResolver resolver;
        private delegate void SetArrayDimensionsDelegate(ref ArrayNameNode indxExp, AssociativeNode indx);

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

        /// <summary>
        /// If node is a DesignScript.BuiltIn.Get.ValueAtIndex(exp1, exp2) function call node.
        /// it is transformed into a node with indexing operator e.g. exp1[exp2].
        /// </summary>
        /// <param name="node">input node</param>
        /// <param name="indexExp">output node</param>
        /// <returns>true if input expression is of the form DesignScript.BuiltIn.Get.ValueAtIndex(exp1, exp2), false otherwise</returns>
        private bool TryBuildIndexingExpression(IdentifierListNode node, out ArrayNameNode indexExp)
        {
            // If indxExp already has a rank >= 1, we need to append another [] to it with indx.
            SetArrayDimensionsDelegate setArrayDimensions = (ref ArrayNameNode indxExp, AssociativeNode indx) =>
            {
                if (indxExp.ArrayDimensions != null)
                {
                    if (indxExp is IdentifierListNode iln)
                    {
                        if (iln.RightNode is ArrayNameNode arr)
                        {
                            arr.ArrayDimensions = indxExp.ArrayDimensions;
                        }
                    }
                    else indxExp = new IdentifierNode(indxExp.ToString());
                }
                indxExp.ArrayDimensions = new ArrayNode(indx, null);
            };

            indexExp = null;
            if(node.RightNode is FunctionCallNode fcn)
            {
                if(fcn.Function.Name == ProtoCore.AST.Node.BuiltinValueAtIndexMethodName)
                {
                    if(node.LeftNode.ToString() == ProtoCore.AST.Node.BuiltinGetValueAtIndexTypeName)
                    {
                        var exp1 = fcn.FormalArguments[0];
                        var exp2 = fcn.FormalArguments[1];
                        
                        // exp1 = expression to be indexed.
                        // Only expressions of ArrayNameNode type can be indexed.
                        if (exp1 is ArrayNameNode ann)
                        {
                            if (exp1 is IdentifierListNode iln1)
                            {
                                if (!TryBuildIndexingExpression(iln1, out indexExp))
                                {
                                    indexExp = iln1;
                                }
                            }
                            else indexExp = ann;

                            // exp2 = expression that evaluates to an index.
                            if (exp2 is IdentifierListNode iln2)
                            {
                                if (TryBuildIndexingExpression(iln2, out ArrayNameNode index))
                                {
                                    setArrayDimensions(ref indexExp, index);
                                    return true;
                                }
                            }
                            setArrayDimensions(ref indexExp, exp2);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
        {
            if (node == null)
                return null;

            if (TryBuildIndexingExpression(node, out ArrayNameNode indexExp))
            {
                if (indexExp is IdentifierListNode iln) node = iln;
                else return indexExp;
            }

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
                Optr = Operator.dot,
                ArrayDimensions = node.ArrayDimensions
            };
            return node;
        }

        public override AssociativeNode VisitIdentifierNode(IdentifierNode node)
        {
            var name = node.ToString();
            string[] strIdentList = name.Split('.');

            // return IdentifierNode if identifier string is not separated by '.'
            if (strIdentList.Length == 1)
            {
                return node;
            }

            // create IdentifierListNode from string such that
            // RightNode is IdentifierNode representing classname
            // and LeftNode is IdentifierNode representing one or more namespaces.
            var rightNode = new IdentifierNode(strIdentList.Last());
            StringBuilder bld = new StringBuilder();
            for (int i = 0; i < strIdentList.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(bld.ToString()))
                    bld.Append(".");
                bld.Append(strIdentList[i]);
            }
            var leftNode = new IdentifierNode(bld.ToString());

            var identListNode = new IdentifierListNode
            {
                LeftNode = leftNode,
                RightNode = rightNode,
                Optr = Operator.dot
            };
            return identListNode.Accept(this);
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

            shortNameNode = CreateNodeFromShortName(className, qualifiedName);
            return shortNameNode != null;
        }

        private AssociativeNode CreateNodeFromShortName(string className, string qualifiedName)
        {
            // Get the list of conflicting namespaces that contain the same class name
            var matchingClasses = classTable.ClassNodes.Where(
                x => x.Name.Split('.').Last().Equals(className)).ToList();

            if (matchingClasses.Count == 0)
                return null;

            if (matchingClasses.Count == 1)
            {
                // If there is no class conflict simply use the class name as the shortest name.
                return CoreUtils.CreateNodeFromString(className);
            }

            var shortName = resolver?.LookupShortName(qualifiedName);
            if (!string.IsNullOrEmpty(shortName))
            {
                return CoreUtils.CreateNodeFromString(shortName);
            }

            // Use the namespace list to derive the list of shortest unique names
            var symbolList =
                matchingClasses.Select(matchingClass => new Symbol(matchingClass.Name));
            var shortNames = Symbol.GetShortestUniqueNames(symbolList);

            // remove hidden class if any from shortNames before proceeding
            var hiddenClassNodes = matchingClasses.
                Where(x => x.ClassAttributes?.HiddenInLibrary ?? false).ToList();
            if(hiddenClassNodes.Any())
            {
                foreach (var hiddenClassNode in hiddenClassNodes)
                {
                    var keyToRemove = new Symbol(hiddenClassNode.Name);
                    shortNames.Remove(keyToRemove);
                }
            }

            // Get the shortest name corresponding to the fully qualified name
            if(shortNames.TryGetValue(new Symbol(qualifiedName), out shortName))
            {
                return CoreUtils.CreateNodeFromString(shortName);
            }

            // If shortName for fully qualified classname is not found, it could be a base class
            // present in class hierarchy of any of the other matchingClasses, in which case
            // set shortName to the one for the derived class.
            var qualifiedClassNode = matchingClasses.FirstOrDefault(x => x.Name == qualifiedName);
            var classHierarchies = matchingClasses.Where(x => x != qualifiedClassNode).
                Select(y => classTable.GetClassHierarchy(y));
            foreach (var hierarchy in classHierarchies)
            {
                // If A derives from B, which derives from C, the hierarchy for A 
                // is listed in that order: [A, B, C], so we start searching in reverse order.
                for (int i = hierarchy.Count - 1; i > 0; i--)
                {
                    if (hierarchy[i] == qualifiedClassNode)
                    {
                        shortName = shortNames[new Symbol(hierarchy[0].Name)];
                        return CoreUtils.CreateNodeFromString(shortName);
                    }
                }
            }
            return null;
        }
    }
}
