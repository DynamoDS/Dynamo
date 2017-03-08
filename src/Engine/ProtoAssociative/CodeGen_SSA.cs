using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ProtoAssociative
{
    public partial class CodeGen : ProtoCore.CodeGen
    {
        public List<AssociativeNode> EmitSSA(List<AssociativeNode> astList)
        {
            Validity.Assert(null != astList);
            astList = ApplyTransform(astList);
            return BuildSSA(astList, new ProtoCore.CompileTime.Context());
        }

        private BinaryExpressionNode BuildSSAIdentListAssignmentNode(IdentifierListNode identList)
        {
            // Build the final binary expression 
            BinaryExpressionNode bnode = new BinaryExpressionNode();
            bnode.Optr = ProtoCore.DSASM.Operator.assign;

            // Left node
            var identNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
            bnode.LeftNode = identNode;

            //Right node
            bnode.RightNode = identList;
            bnode.isSSAAssignment = true;

            return bnode;
        }

        private List<AssociativeNode> SplitBinaryExpression(BinaryExpressionNode node)
        {
            List<AssociativeNode> nodes = new List<AssociativeNode>();

            if (node.isMultipleAssign)
            {
                DFSEmitSplitAssign_AST(node, ref nodes);
                foreach (AssociativeNode anode in nodes)
                {
                    if (node.Kind == AstKind.BinaryExpression)
                    {
                        var binaryExpression = node as BinaryExpressionNode;
                        NodeUtils.SetNodeLocation(anode, binaryExpression.LeftNode, binaryExpression.RightNode);
                    }
                }
            }
            else
            {
                nodes.Add(node);
            }

            return nodes;
        }

        private void SplitClassDeclarationNode(ClassDeclNode node)
        {
            foreach (AssociativeNode procNode in node.Procedures)
            {
                if (procNode is FunctionDefinitionNode)
                {
                    FunctionDefinitionNode fNode = procNode as FunctionDefinitionNode;
                    if (!fNode.IsExternLib)
                    {
                        fNode.FunctionBody.Body = SplitMulitpleAssignment(fNode.FunctionBody.Body);
                    }
                }
                else if (procNode is ConstructorDefinitionNode)
                {
                    ConstructorDefinitionNode cNode = procNode as ConstructorDefinitionNode;
                    if (!cNode.IsExternLib)
                    {
                        cNode.FunctionBody.Body = SplitMulitpleAssignment(cNode.FunctionBody.Body);
                    }
                }
            }
        }

        private void SplitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            if (!node.IsExternLib)
            {
                node.FunctionBody.Body = SplitMulitpleAssignment(node.FunctionBody.Body);
            }
        }

        private void SplitConstructorDefinitionNode(ConstructorDefinitionNode node)
        {
            if (!node.IsExternLib)
            {
                node.FunctionBody.Body = SplitMulitpleAssignment(node.FunctionBody.Body);
            }
        }

        private List<AssociativeNode> SplitMulitpleAssignment(List<AssociativeNode> astList)
        {
            if (astList == null)
            {
                return new List<AssociativeNode>();
            }

            List<AssociativeNode> newAstList = new List<AssociativeNode>();
            foreach (AssociativeNode node in astList)
            {
                switch (node.Kind)
                {
                    case AstKind.BinaryExpression:
                        newAstList.AddRange(SplitBinaryExpression(node as BinaryExpressionNode));
                        break;
                    case AstKind.ClassDeclaration:
                        SplitClassDeclarationNode(node as ClassDeclNode);
                        newAstList.Add(node);
                        break;
                    case AstKind.FunctionDefintion:
                        SplitFunctionDefinitionNode(node as FunctionDefinitionNode);
                        newAstList.Add(node);
                        break;
                    case AstKind.Constructor:
                        SplitConstructorDefinitionNode(node as ConstructorDefinitionNode);
                        newAstList.Add(node);
                        break;
                    default:
                        newAstList.Add(node);
                        break;
                }
            }
            return newAstList;
        }

        private List<AssociativeNode> ApplyTransform(List<AssociativeNode> astList)
        {
            bool validAst = astList != null && astList.Count > 0;
            if (validAst)
            {
                astList = SplitMulitpleAssignment(astList);
                astList = TransformLHSIdentList(astList);
            }
            return astList;
        }

        /// <summary>
        /// Converts lhs ident lists to a function call
        /// a.x = 10 
        ///     -> t = a.%set_x(10)
        ///     
        /// a.x.y = b + c 
        ///     -> a.x.%set_y(b + c)
        ///     
        /// a.x[0] = 10
        ///     ->  tval = a.%get_x()
        ///         tval[0] = 10         
        ///         tmp = a.%set_x(tval)
        /// </summary>
        /// <param name="astList"></param>
        /// <returns></returns>
        private List<AssociativeNode> TransformLHSIdentList(List<AssociativeNode> astList)
        {
            List<AssociativeNode> newAstList = new List<AssociativeNode>();
            foreach (AssociativeNode node in astList)
            {
                BinaryExpressionNode bNode = node as BinaryExpressionNode;
                if (bNode == null)
                {
                    newAstList.Add(node);
                }
                else
                {
                    bool isLHSIdentList = bNode.LeftNode is IdentifierListNode;
                    if (!isLHSIdentList)
                    {
                        newAstList.Add(node);
                    }
                    else
                    {
                        IdentifierNode lhsTemp = new IdentifierNode(Constants.kTempVar);

                        IdentifierListNode identList = bNode.LeftNode as IdentifierListNode;
                        Validity.Assert(identList != null);

                        AssociativeNode argument = bNode.RightNode;

                        IdentifierNode identFunctionCall = identList.RightNode as IdentifierNode;
                        string setterName = ProtoCore.DSASM.Constants.kSetterPrefix + identList.RightNode.Name;
                        bool isArrayIndexed = identFunctionCall.ArrayDimensions != null;
                        if (isArrayIndexed)
                        {
                            // a.x[0] = 10
                            //      tval = a.%get_x()
                            string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + identList.RightNode.Name;
                            ProtoCore.AST.AssociativeAST.FunctionCallNode fcall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
                            fcall.Function = new IdentifierNode(identList.RightNode.Name);
                            fcall.Function.Name = getterName;

                            IdentifierListNode identList1 = new IdentifierListNode();
                            identList1.LeftNode = identList.LeftNode;
                            identList1.RightNode = fcall;
                            BinaryExpressionNode bnodeGet = new BinaryExpressionNode(
                                lhsTemp,
                                identList1,
                                Operator.assign
                                );
                            newAstList.Add(bnodeGet);

                            //      tval[0] = 10     
                            IdentifierNode lhsTempIndexed = new IdentifierNode(Constants.kTempVar);
                            lhsTempIndexed.ArrayDimensions = identFunctionCall.ArrayDimensions;
                            BinaryExpressionNode bnodeAssign = new BinaryExpressionNode(
                                lhsTempIndexed,
                                argument,
                                Operator.assign
                                );
                            newAstList.Add(bnodeAssign);

                            //      tmp = a.%set_x(tval)
                            ProtoCore.AST.AssociativeAST.FunctionCallNode fcallSet = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
                            fcallSet.Function = identFunctionCall;
                            fcallSet.Function.Name = setterName;
                            List<AssociativeNode> args = new List<AssociativeNode>();
                            IdentifierNode lhsTempAssignBack = new IdentifierNode(Constants.kTempVar);
                            args.Add(lhsTempAssignBack);
                            fcallSet.FormalArguments = args;

                            IdentifierListNode identList2 = new IdentifierListNode();
                            identList2.LeftNode = identList.LeftNode;
                            identList2.RightNode = fcallSet;

                            IdentifierNode lhsTempAssign = new IdentifierNode(Constants.kTempPropertyVar);

                            BinaryExpressionNode bnodeSet = new BinaryExpressionNode(
                                lhsTempAssign,
                                identList2,
                                Operator.assign
                                );
                            newAstList.Add(bnodeSet);
                        }
                        else
                        {
                            List<AssociativeNode> args = new List<AssociativeNode>();
                            args.Add(argument);

                            ProtoCore.AST.AssociativeAST.FunctionCallNode fcall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
                            fcall.Function = identFunctionCall;
                            fcall.Function.Name = setterName;
                            fcall.FormalArguments = args;

                            identList.RightNode = fcall;
                            BinaryExpressionNode convertedAssignNode = new BinaryExpressionNode(lhsTemp, identList, Operator.assign);

                            NodeUtils.CopyNodeLocation(convertedAssignNode, bNode);
                            newAstList.Add(convertedAssignNode);
                        }
                    }
                }
            }
            return newAstList;
        }

        private List<AssociativeNode> BuildSSA(List<AssociativeNode> astList, ProtoCore.CompileTime.Context context)
        {
            if (null != astList && astList.Count > 0)
            {
                Dictionary<string, int> ssaUIDList = new Dictionary<string, int>();

                List<AssociativeNode> newAstList = new List<AssociativeNode>();

                int ssaExprID = 0;
                foreach (AssociativeNode node in astList)
                {
                    List<AssociativeNode> newASTList = new List<AssociativeNode>();
                    if (node is BinaryExpressionNode)
                    {
                        BinaryExpressionNode bnode = (node as BinaryExpressionNode);
                        int generatedUID = ProtoCore.DSASM.Constants.kInvalidIndex;

                        if (context.applySSATransform && core.Options.GenerateSSA)
                        {
                            int ssaID = ProtoCore.DSASM.Constants.kInvalidIndex;
                            string name = ProtoCore.Utils.CoreUtils.GenerateIdentListNameString(bnode.LeftNode);
                            if (ssaUIDList.ContainsKey(name))
                            {
                                ssaUIDList.TryGetValue(name, out ssaID);
                                generatedUID = ssaID;
                            }
                            else
                            {
                                ssaID = core.ExpressionUID++;
                                ssaUIDList.Add(name, ssaID);
                                generatedUID = ssaID;
                            }

                            Stack<AssociativeNode> ssaStack = new Stack<AssociativeNode>();
                            DFSEmitSSA_AST(node, ssaStack, ref newASTList);

                            // Set the unique expression id for this range of SSA nodes
                            foreach (AssociativeNode aNode in newASTList)
                            {
                                Validity.Assert(aNode is BinaryExpressionNode);

                                // Set the exprID of the SSA's node
                                BinaryExpressionNode ssaNode = aNode as BinaryExpressionNode;
                                ssaNode.ExpressionUID = ssaID;
                                ssaNode.SSASubExpressionID = ssaExprID;
                                ssaNode.SSAExpressionUID = core.SSAExpressionUID;
                                ssaNode.guid = bnode.guid;
                                ssaNode.OriginalAstID = bnode.OriginalAstID;
                                ssaNode.IsModifier = node.IsModifier;
                                NodeUtils.SetNodeLocation(ssaNode, node, node);
                            }

                            // Assigne the exprID of the original node 
                            // (This is the node prior to ssa transformation)
                            bnode.ExpressionUID = ssaID;
                            bnode.SSASubExpressionID = ssaExprID;
                            bnode.SSAExpressionUID = core.SSAExpressionUID;
                            newAstList.AddRange(newASTList);
                        }
                        else
                        {
                            bnode.ExpressionUID = generatedUID = core.ExpressionUID++;
                            newAstList.Add(node);
                        }

                        // TODO Jun: How can delta execution functionality be seamlessly integrated in the codegens?
                        SetExecutionFlagForNode(node as BinaryExpressionNode, generatedUID);
                    }
                    else if (node is ClassDeclNode)
                    {
                        ClassDeclNode classNode = node as ClassDeclNode;
                        foreach (AssociativeNode procNode in classNode.Procedures)
                        {
                            if (procNode is FunctionDefinitionNode)
                            {
                                FunctionDefinitionNode fNode = procNode as FunctionDefinitionNode;
                                if (!fNode.IsExternLib)
                                {
                                    fNode.FunctionBody.Body = BuildSSA(fNode.FunctionBody.Body, context);
                                }
                            }
                            else if (procNode is ConstructorDefinitionNode)
                            {
                                ConstructorDefinitionNode cNode = procNode as ConstructorDefinitionNode;
                                if (!cNode.IsExternLib)
                                {

                                    cNode.FunctionBody.Body = ApplyTransform(cNode.FunctionBody.Body);
                                    cNode.FunctionBody.Body = BuildSSA(cNode.FunctionBody.Body, context);
                                }
                            }
                        }
                        newAstList.Add(classNode);
                    }
                    else if (node is FunctionDefinitionNode)
                    {
                        FunctionDefinitionNode fNode = node as FunctionDefinitionNode;
                        if (!fNode.IsExternLib)
                        {
                            fNode.FunctionBody.Body = BuildSSA(fNode.FunctionBody.Body, context);
                        }
                        newAstList.Add(node);
                    }
                    else if (node is ConstructorDefinitionNode)
                    {
                        ConstructorDefinitionNode fNode = node as ConstructorDefinitionNode;
                        if (!fNode.IsExternLib)
                        {
                            fNode.FunctionBody.Body = BuildSSA(fNode.FunctionBody.Body, context);
                        }
                        newAstList.Add(node);
                    }
                    else if (node is LanguageBlockNode)
                    {
                        var lbNode = node as LanguageBlockNode;

                        if (context.applySSATransform && core.Options.GenerateSSA)
                        {
                            int ssaID = ProtoCore.DSASM.Constants.kInvalidIndex;
                            ssaID = core.ExpressionUID++;
                            //ssaUIDList.Add(name, ssaID);

                            Stack<AssociativeNode> ssaStack = new Stack<AssociativeNode>();
                            DFSEmitSSA_AST(node, ssaStack, ref newASTList);

                            // Set the unique expression id for this range of SSA nodes
                            foreach (AssociativeNode aNode in newASTList)
                            {
                                Validity.Assert(aNode is BinaryExpressionNode);

                                // Set the exprID of the SSA's node
                                BinaryExpressionNode ssaNode = aNode as BinaryExpressionNode;
                                ssaNode.ExpressionUID = ssaID;
                                ssaNode.SSASubExpressionID = ssaExprID;
                                ssaNode.SSAExpressionUID = core.SSAExpressionUID;
                                //ssaNode.guid = lbNode.guid;
                                //ssaNode.OriginalAstID = lbNode.OriginalAstID;
                                ssaNode.IsModifier = node.IsModifier;
                                NodeUtils.SetNodeLocation(ssaNode, node, node);
                            }
                            newAstList.AddRange(newASTList);
                        }
                    }
                    else
                    {
                        newAstList.Add(node);
                    }
                    ssaExprID++;
                    core.SSAExpressionUID++;
                }
                return newAstList;
            }
            return astList;
        }

        private void DfsSSAIeentList(AssociativeNode node, ref Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            if (node is IdentifierListNode)
            {
                IdentifierListNode listNode = node as IdentifierListNode;

                bool isSingleDot = !(listNode.LeftNode is IdentifierListNode) && !(listNode.RightNode is IdentifierListNode);
                if (isSingleDot)
                {
                    BinaryExpressionNode bnode = BuildSSAIdentListAssignmentNode(listNode);
                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    DfsSSAIeentList(listNode.LeftNode, ref ssaStack, ref astlist);

                    IdentifierListNode newListNode = node as IdentifierListNode;
                    newListNode.Optr = Operator.dot;

                    AssociativeNode leftnode = ssaStack.Pop();
                    Validity.Assert(leftnode is BinaryExpressionNode);

                    newListNode.LeftNode = (leftnode as BinaryExpressionNode).LeftNode;
                    newListNode.RightNode = listNode.RightNode;

                    BinaryExpressionNode bnode = BuildSSAIdentListAssignmentNode(newListNode);
                    astlist.Add(bnode);
                    ssaStack.Push(bnode);

                }
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode fcNode = node as FunctionCallNode;
                for (int idx = 0; idx < fcNode.FormalArguments.Count; idx++)
                {
                    AssociativeNode arg = fcNode.FormalArguments[idx];

                    Stack<AssociativeNode> ssaStack1 = new Stack<AssociativeNode>();
                    DFSEmitSSA_AST(arg, ssaStack1, ref astlist);
                    AssociativeNode argNode = ssaStack.Pop();
                    fcNode.FormalArguments[idx] = argNode is BinaryExpressionNode ? (argNode as BinaryExpressionNode).LeftNode : argNode;
                }
            }
        }

        private void DFSEmitSSA_AST(AssociativeNode node, Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            Validity.Assert(null != astlist && null != ssaStack);
            if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode astBNode = node as BinaryExpressionNode;
                AssociativeNode leftNode = null;
                AssociativeNode rightNode = null;
                bool isSSAAssignment = false;
                if (ProtoCore.DSASM.Operator.assign == astBNode.Optr)
                {
                    leftNode = astBNode.LeftNode;
                    DFSEmitSSA_AST(astBNode.RightNode, ssaStack, ref astlist);
                    AssociativeNode assocNode = ssaStack.Pop();

                    if (assocNode is BinaryExpressionNode)
                    {
                        rightNode = (assocNode as BinaryExpressionNode).LeftNode;
                    }
                    else
                    {
                        rightNode = assocNode;
                    }
                    isSSAAssignment = false;

                    // Handle SSA if the lhs is not an identifier
                    // A non-identifier LHS is any LHS that is not just an identifier name.
                    //  i.e. a[0], a.b, f(), f(x)
                    var isSingleIdentifier = (leftNode is IdentifierNode) && (leftNode as IdentifierNode).ArrayDimensions == null;
                    if (!isSingleIdentifier)
                    {
                        EmitSSALHS(ref leftNode, ssaStack, ref astlist);
                    }
                }
                else
                {
                    BinaryExpressionNode tnode = new BinaryExpressionNode();
                    tnode.Optr = astBNode.Optr;

                    DFSEmitSSA_AST(astBNode.LeftNode, ssaStack, ref astlist);
                    DFSEmitSSA_AST(astBNode.RightNode, ssaStack, ref astlist);

                    AssociativeNode lastnode = ssaStack.Pop();
                    AssociativeNode prevnode = ssaStack.Pop();

                    tnode.LeftNode = prevnode is BinaryExpressionNode ? (prevnode as BinaryExpressionNode).LeftNode : prevnode;
                    tnode.RightNode = lastnode is BinaryExpressionNode ? (lastnode as BinaryExpressionNode).LeftNode : lastnode;

                    // Left node
                    leftNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));

                    // Right node
                    rightNode = tnode;

                    isSSAAssignment = true;
                }

                var bnode = AstFactory.BuildAssignment(leftNode, rightNode);
                bnode.isSSAAssignment = isSSAAssignment;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is ArrayNode)
            {
                var arrayNode = node as ArrayNode;
                DFSEmitSSA_AST(arrayNode.Expr, ssaStack, ref astlist);

                var bnode = new BinaryExpressionNode { Optr = Operator.assign };

                // Left node
                var tmpIdent = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
                Validity.Assert(null != tmpIdent);
                bnode.LeftNode = tmpIdent;

                if (arrayNode.Expr == null && arrayNode.Type == null)
                {
                    // Right node
                    bnode.RightNode = new NullNode();
                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                    return;
                }

                // pop off the dimension
                var dimensionNode = ssaStack.Pop();
                ArrayNode currentDimensionNode;
                if (dimensionNode is BinaryExpressionNode)
                {
                    currentDimensionNode = new ArrayNode((dimensionNode as BinaryExpressionNode).LeftNode, null);
                }
                else
                {
                    currentDimensionNode = new ArrayNode(dimensionNode, null);
                }

                // Pop the prev SSA node where the current dimension will apply
                AssociativeNode nodePrev = ssaStack.Pop();
                if (nodePrev is BinaryExpressionNode)
                {
                    AssociativeNode rhsIdent = AstFactory.BuildIdentifier((nodePrev as BinaryExpressionNode).LeftNode.Name);
                    (rhsIdent as IdentifierNode).ArrayDimensions = currentDimensionNode;

                    bnode.RightNode = rhsIdent;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);

                    if (null != arrayNode.Type)
                    {
                        DFSEmitSSA_AST(arrayNode.Type, ssaStack, ref astlist);
                    }
                }
                else if (nodePrev is IdentifierListNode)
                {
                    IdentifierNode iNode = (nodePrev as IdentifierListNode).RightNode as IdentifierNode;
                    Validity.Assert(null != iNode);
                    iNode.ArrayDimensions = currentDimensionNode;


                    if (null != arrayNode.Type)
                    {
                        DFSEmitSSA_AST(arrayNode.Type, ssaStack, ref astlist);
                    }
                }
                else
                {
                    Validity.Assert(false);
                }
            }
            else if (node is IdentifierNode)
            {
                IdentifierNode ident = node as IdentifierNode;

                if (core.Options.GenerateSSA)
                {
                    string ssaTempName = string.Empty;
                    if (null == ident.ArrayDimensions)
                    {
                        BinaryExpressionNode bnode = new BinaryExpressionNode();
                        bnode.Optr = ProtoCore.DSASM.Operator.assign;

                        // Left node
                        ssaTempName = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
                        var identNode = AstFactory.BuildIdentifier(ssaTempName);
                        bnode.LeftNode = identNode;

                        // Right node
                        bnode.RightNode = ident;

                        bnode.isSSAAssignment = true;

                        astlist.Add(bnode);
                        ssaStack.Push(bnode);

                        // Associate the first pointer with each SSA temp
                        // It is acceptable to store firstVar even if it is not a pointer as ResolveFinalNodeRefs() will just ignore this entry 
                        // if this ssa temp is not treated as a pointer inside the function
                        string firstVar = ident.Name;
                        ssaTempToFirstPointerMap.Add(ssaTempName, firstVar);
                    }
                    else
                    {
                        EmitSSAArrayIndex(ident, ssaStack, ref astlist);
                    }
                }
                else
                {
                    BinaryExpressionNode bnode = new BinaryExpressionNode();
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;

                    // Left node
                    var identNode = AstFactory.BuildIdentifier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = ident;


                    bnode.isSSAAssignment = true;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
            }
            else if (node is FunctionCallNode || node is FunctionDotCallNode)
            {
                FunctionCallNode fcNode = null;
                if (node is FunctionCallNode)
                {
                    fcNode = new FunctionCallNode(node as FunctionCallNode);

                    List<AssociativeNode> astlistArgs = new List<AssociativeNode>();

                    for (int idx = 0; idx < fcNode.FormalArguments.Count; idx++)
                    {
                        AssociativeNode arg = fcNode.FormalArguments[idx];
                        DFSEmitSSA_AST(arg, ssaStack, ref astlistArgs);
                        AssociativeNode argNode = ssaStack.Pop();

                        if (argNode is BinaryExpressionNode)
                        {
                            BinaryExpressionNode argBinaryExpr = argNode as BinaryExpressionNode;
                            var argLeftNode = argBinaryExpr.LeftNode as IdentifierNode;
                            argLeftNode.ReplicationGuides = GetReplicationGuides(arg);
                            argLeftNode.AtLevel = GetAtLevel(arg);

                            fcNode.FormalArguments[idx] = argBinaryExpr.LeftNode;
                        }
                        else
                        {
                            fcNode.FormalArguments[idx] = argNode;
                        }
                        astlist.AddRange(astlistArgs);
                        astlistArgs.Clear();
                    }
                }

                // Left node
                var leftNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
                if (null != fcNode)
                {
                    leftNode.ReplicationGuides = GetReplicationGuides(fcNode);
                    leftNode.AtLevel = GetAtLevel(fcNode);
                }

                var bnode = AstFactory.BuildAssignment(leftNode, fcNode);
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is IdentifierListNode)
            {
                SSAIdentList(node, ref ssaStack, ref astlist);
                AssociativeNode lastNode = null;
                Validity.Assert(astlist.Count > 0);

                // Get the last identifierlist node and set its flag
                // This is required to reset the pointerlist for every identifierlist traversed
                // i.e. 
                //      a = p.x
                //      reset the flag after traversing p.x
                //
                //      b = p.x + g.y    
                //      reset the flag after traversing p.x and p.y
                //
                int lastIndex = astlist.Count - 1;
                for (int n = lastIndex; n >= 0; --n)
                {
                    lastNode = astlist[n];
                    Validity.Assert(lastNode is BinaryExpressionNode);

                    BinaryExpressionNode bnode = lastNode as BinaryExpressionNode;
                    Validity.Assert(bnode.Optr == Operator.assign);
                    if (bnode.RightNode is IdentifierListNode)
                    {
                        IdentifierListNode identList = bnode.RightNode as IdentifierListNode;
                        identList.IsLastSSAIdentListFactor = true;
                        break;
                    }
                }

                // Assign the first ssa assignment flag
                BinaryExpressionNode firstBNode = astlist[0] as BinaryExpressionNode;
                Validity.Assert(null != firstBNode);
                Validity.Assert(firstBNode.Optr == Operator.assign);
                firstBNode.isSSAFirstAssignment = true;


                //
                // Get the first pointer
                // The first pointer is the lhs of the next dotcall
                //
                // Given:     
                //      a = x.y.z
                // SSA'd to:     
                //      t0 = x      -> 'x' is the lhs of the first dotcall (x.y)
                //      t1 = t0.y
                //      t2 = t1.z
                //      a = t2

                IdentifierNode lhsIdent = null;
                if (firstBNode.RightNode is IdentifierNode)
                {
                    // In this case the rhs of the ident list is an ident
                    // Get the ident name
                    lhsIdent = (firstBNode.RightNode as IdentifierNode);
                }
                else if (firstBNode.RightNode is FunctionCallNode)
                {
                    // In this case the rhs of the ident list is a function
                    // Get the function name
                    lhsIdent = (firstBNode.RightNode as FunctionCallNode).Function as IdentifierNode;
                }
                else
                {

                    lhsIdent = null;
                }

                //=========================================================
                //
                // 1. Backtrack and convert all identlist nodes to dot calls
                //    This can potentially be optimized by performing the dotcall transform in SSAIdentList
                //
                // 2. Associate the first pointer with each SSA temp
                //
                //=========================================================
                AssociativeNode prevNode = null;
                for (int n = 0; n <= lastIndex; n++)
                {
                    lastNode = astlist[n];

                    BinaryExpressionNode bnode = lastNode as BinaryExpressionNode;
                    Validity.Assert(bnode.Optr == Operator.assign);

                    // Get the ssa temp name
                    Validity.Assert(bnode.LeftNode is IdentifierNode);
                    string ssaTempName = (bnode.LeftNode as IdentifierNode).Name;
                    Validity.Assert(CoreUtils.IsSSATemp(ssaTempName));

                    if (bnode.RightNode is IdentifierListNode)
                    {
                        IdentifierListNode identList = bnode.RightNode as IdentifierListNode;
                        if (identList.RightNode is IdentifierNode)
                        {
                            IdentifierNode identNode = identList.RightNode as IdentifierNode;

                            ProtoCore.AST.AssociativeAST.FunctionCallNode rcall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
                            rcall.Function = new IdentifierNode(identNode);
                            rcall.Function.Name = ProtoCore.DSASM.Constants.kGetterPrefix + rcall.Function.Name;

                            ProtoCore.Utils.CoreUtils.CopyDebugData(identList.LeftNode, lhsIdent);
                            FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(identList.LeftNode, rcall, core);
                            dotCall.isLastSSAIdentListFactor = identList.IsLastSSAIdentListFactor;
                            bnode.RightNode = dotCall;
                            ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, lhsIdent);

                            // Update the LHS of the next dotcall
                            //      a = x.y.z
                            //      t0 = x      
                            //      t1 = t0.y   'y' is the lhs of the next dotcall (y.z)
                            //      t2 = t1.z
                            //      a = t2
                            //
                            Validity.Assert(rcall.Function is IdentifierNode);
                            lhsIdent = rcall.Function as IdentifierNode;
                        }
                        else if (identList.RightNode is FunctionCallNode)
                        {
                            FunctionCallNode fCallNode = identList.RightNode as FunctionCallNode;


                            ProtoCore.Utils.CoreUtils.CopyDebugData(identList.LeftNode, lhsIdent);
                            FunctionDotCallNode dotCall = ProtoCore.Utils.CoreUtils.GenerateCallDotNode(identList.LeftNode, fCallNode, core);
                            dotCall.isLastSSAIdentListFactor = identList.IsLastSSAIdentListFactor;
                            bnode.RightNode = dotCall;

                            ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, lhsIdent);

                            // Update the LHS of the next dotcall
                            //      a = x.y.z
                            //      t0 = x      
                            //      t1 = t0.y   'y' is the lhs of the next dotcall (y.z)
                            //      t2 = t1.z
                            //      a = t2
                            //
                            Validity.Assert(fCallNode.Function is IdentifierNode);
                            lhsIdent = fCallNode.Function as IdentifierNode;
                        }
                        else
                        {
                            Validity.Assert(false);
                        }
                    }
                    prevNode = bnode.RightNode;
                }
            }
            else if (node is ExprListNode)
            {
                ExprListNode exprList = node as ExprListNode;
                for (int n = 0; n < exprList.Exprs.Count; n++)
                {
                    List<AssociativeNode> currentElementASTList = new List<AssociativeNode>();
                    DFSEmitSSA_AST(exprList.Exprs[n], ssaStack, ref currentElementASTList);

                    astlist.AddRange(currentElementASTList);
                    currentElementASTList.Clear();
                    AssociativeNode argNode = ssaStack.Pop();
                    exprList.Exprs[n] = argNode is BinaryExpressionNode ? (argNode as BinaryExpressionNode).LeftNode : argNode;
                }

                var leftNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
                var bnode = AstFactory.BuildAssignment(leftNode, exprList);
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is InlineConditionalNode)
            {
                InlineConditionalNode ilnode = node as InlineConditionalNode;

                List<AssociativeNode> inlineExpressionASTList = new List<AssociativeNode>();

                // Emit the boolean condition
                DFSEmitSSA_AST(ilnode.ConditionExpression, ssaStack, ref inlineExpressionASTList);
                AssociativeNode cexpr = ssaStack.Pop();
                ilnode.ConditionExpression = cexpr is BinaryExpressionNode ? (cexpr as BinaryExpressionNode).LeftNode : cexpr;
                var namenode = ilnode.ConditionExpression as ArrayNameNode;
                if (namenode != null)
                {
                    var rightNode = (cexpr is BinaryExpressionNode) ? (cexpr as BinaryExpressionNode).RightNode : cexpr;
                    namenode.ReplicationGuides = GetReplicationGuides(rightNode);
                    namenode.AtLevel = GetAtLevel(rightNode);
                }
                astlist.AddRange(inlineExpressionASTList);
                inlineExpressionASTList.Clear();

                DFSEmitSSA_AST(ilnode.TrueExpression, ssaStack, ref inlineExpressionASTList);
                cexpr = ssaStack.Pop();
                ilnode.TrueExpression = cexpr is BinaryExpressionNode ? (cexpr as BinaryExpressionNode).LeftNode : cexpr;
                namenode = ilnode.TrueExpression as ArrayNameNode;
                if (namenode != null)
                {
                    var rightNode = (cexpr is BinaryExpressionNode) ? (cexpr as BinaryExpressionNode).RightNode : cexpr;
                    namenode.ReplicationGuides = GetReplicationGuides(rightNode);
                    namenode.AtLevel = GetAtLevel(rightNode);
                }
                astlist.AddRange(inlineExpressionASTList);
                inlineExpressionASTList.Clear();

                DFSEmitSSA_AST(ilnode.FalseExpression, ssaStack, ref inlineExpressionASTList);
                cexpr = ssaStack.Pop();
                ilnode.FalseExpression = cexpr is BinaryExpressionNode ? (cexpr as BinaryExpressionNode).LeftNode : cexpr;
                namenode = ilnode.FalseExpression as ArrayNameNode;
                if (namenode != null)
                {
                    var rightNode = (cexpr is BinaryExpressionNode) ? (cexpr as BinaryExpressionNode).RightNode : cexpr;
                    namenode.ReplicationGuides = GetReplicationGuides(rightNode);
                    namenode.AtLevel = GetAtLevel(rightNode);
                }
                astlist.AddRange(inlineExpressionASTList);
                inlineExpressionASTList.Clear();

                var leftNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
                var bnode = AstFactory.BuildAssignment(leftNode, ilnode);
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is RangeExprNode)
            {
                RangeExprNode rangeNode = node as RangeExprNode;

                DFSEmitSSA_AST(rangeNode.From, ssaStack, ref astlist);
                AssociativeNode fromExpr = ssaStack.Pop();
                rangeNode.From = fromExpr is BinaryExpressionNode ? (fromExpr as BinaryExpressionNode).LeftNode : fromExpr;

                DFSEmitSSA_AST(rangeNode.To, ssaStack, ref astlist);
                AssociativeNode toExpr = ssaStack.Pop();
                rangeNode.To = toExpr is BinaryExpressionNode ? (toExpr as BinaryExpressionNode).LeftNode : toExpr;

                if (rangeNode.Step != null)
                {
                    DFSEmitSSA_AST(rangeNode.Step, ssaStack, ref astlist);
                    AssociativeNode stepExpr = ssaStack.Pop();
                    rangeNode.Step = stepExpr is BinaryExpressionNode ? (stepExpr as BinaryExpressionNode).LeftNode : stepExpr;
                }

                var leftNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
                var bnode = AstFactory.BuildAssignment(leftNode, rangeNode);
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is GroupExpressionNode)
            {
                if (core.Options.GenerateSSA)
                {
                    GroupExpressionNode groupExpr = node as GroupExpressionNode;
                    if (null == groupExpr.ArrayDimensions)
                    {
                        DFSEmitSSA_AST(groupExpr.Expression, ssaStack, ref astlist);

                        var leftNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
                        AssociativeNode groupExprBinaryStmt = ssaStack.Pop();
                        var rightNode = (groupExprBinaryStmt as BinaryExpressionNode).LeftNode;

                        var bnode = AstFactory.BuildAssignment(leftNode, rightNode);
                        bnode.isSSAAssignment = true;

                        astlist.Add(bnode);
                        ssaStack.Push(bnode);
                    }
                    else
                    {
                        EmitSSAArrayIndex(groupExpr, ssaStack, ref astlist);
                    }
                }
                else
                {
                    // We never supported SSA on grouped expressions
                    // Keep it that way if SSA flag is off
                    ssaStack.Push(node);
                }
            }
            // We allow a null to be generated an SSA variable
            // TODO Jun: Generalize this into genrating SSA temps for all literals
            else if (node is NullNode)
            {
                var leftNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
                var bnode = AstFactory.BuildAssignment(leftNode, node);
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else if (node is LanguageBlockNode)
            {
                var lbNode = node as LanguageBlockNode;

                var args = lbNode.FormalArguments;
                if (args != null)
                {
                    lbNode.codeblock.CaptureList = new Dictionary<string, AssociativeNode>();
                    for (int i = 0; i < args.Count; i++)
                    {
                        DFSEmitSSA_AST(args[i], ssaStack, ref astlist);
                        var bNode = astlist[i] as BinaryExpressionNode;
                        if (bNode != null)
                        {
                            lbNode.codeblock.CaptureList[args[i].Name] = bNode.LeftNode;
                            args[i] = bNode.LeftNode;
                        }
                    }
                }
                // Left node
                var identNode = AstFactory.BuildIdentifier(CoreUtils.BuildSSATemp(core));
                var bnode = AstFactory.BuildAssignment(identNode, node);
                bnode.isSSAAssignment = true;

                astlist.Add(bnode);
                ssaStack.Push(bnode);
            }
            else
            {
                ssaStack.Push(node);
            }
        }

        private void SSAIdentList(AssociativeNode node, ref Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            if (node is IdentifierNode)
            {
                IdentifierNode ident = node as IdentifierNode;
                if (null == ident.ArrayDimensions)
                {
                    // Build the temp pointer
                    BinaryExpressionNode bnode = new BinaryExpressionNode();
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;
                    bnode.isSSAAssignment = true;
                    bnode.isSSAPointerAssignment = true;
                    bnode.IsFirstIdentListNode = true;

                    // Left node
                    var identNode = AstFactory.BuildIdentifier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    identNode.ReplicationGuides = GetReplicationGuides(ident);
                    identNode.AtLevel = ident.AtLevel;

                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = ident;

                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    EmitSSAArrayIndex(ident, ssaStack, ref astlist, true);
                    (astlist[0] as BinaryExpressionNode).IsFirstIdentListNode = true;
                }

            }
            else if (node is ExprListNode)
            {
                //ExprListNode exprList = node as ExprListNode;
                DFSEmitSSA_AST(node, ssaStack, ref astlist);
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode fcall = node as FunctionCallNode;
                if (null == fcall.ArrayDimensions)
                {
                    // Build the temp pointer
                    BinaryExpressionNode bnode = new BinaryExpressionNode();
                    bnode.Optr = ProtoCore.DSASM.Operator.assign;
                    bnode.isSSAAssignment = true;
                    bnode.isSSAPointerAssignment = true;
                    bnode.IsFirstIdentListNode = true;

                    // Left node
                    var identNode = AstFactory.BuildIdentifier(ProtoCore.Utils.CoreUtils.BuildSSATemp(core));
                    identNode.ReplicationGuides = fcall.ReplicationGuides;
                    identNode.AtLevel = fcall.AtLevel;
                    bnode.LeftNode = identNode;

                    // Right node
                    bnode.RightNode = fcall;


                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    EmitSSAArrayIndex(fcall, ssaStack, ref astlist, true);
                    (astlist[0] as BinaryExpressionNode).IsFirstIdentListNode = true;
                }

            }
            else if (node is IdentifierListNode)
            {
                IdentifierListNode identList = node as IdentifierListNode;

                // Build the rhs identifier list containing the temp pointer
                IdentifierListNode rhsIdentList = new IdentifierListNode();
                rhsIdentList.Optr = Operator.dot;

                bool isLeftNodeExprList = false;

                // Check if identlist matches any namesapce
                bool resolvedCall = false;
                string[] classNames = ProtoCore.Utils.CoreUtils.GetResolvedClassName(core.ClassTable, identList);
                if (classNames.Length == 1)
                {
                    //
                    // The identlist is a class name and should not be SSA'd
                    // such as:
                    //  p = Obj.Create()
                    //
                    var leftNode = AstFactory.BuildIdentifier(classNames[0]);
                    rhsIdentList.LeftNode = leftNode;
                    ProtoCore.Utils.CoreUtils.CopyDebugData(leftNode, node);
                    resolvedCall = true;
                }
                else if (classNames.Length > 0)
                {
                    // There is a resolution conflict
                    // identList resolved to multiple classes 

                    // TODO Jun: Move this warning handler to after the SSA transform
                    // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5221
                    buildStatus.LogSymbolConflictWarning(identList.LeftNode.ToString(), classNames);
                    rhsIdentList = identList;
                    resolvedCall = true;
                }
                else
                {
                    // identList unresolved
                    // Continue to transform this identlist into SSA
                    isLeftNodeExprList = identList.LeftNode is ExprListNode;

                    // Recursively traversse the left of the ident list
                    SSAIdentList(identList.LeftNode, ref ssaStack, ref astlist);

                    AssociativeNode lhsNode = ssaStack.Pop();
                    if (lhsNode is BinaryExpressionNode)
                    {
                        rhsIdentList.LeftNode = (lhsNode as BinaryExpressionNode).LeftNode;
                    }
                    else
                    {
                        rhsIdentList.LeftNode = lhsNode;
                    }
                }

                ArrayNode arrayDimension = null;

                AssociativeNode rnode = null;
                if (identList.RightNode is IdentifierNode)
                {
                    IdentifierNode identNode = identList.RightNode as IdentifierNode;
                    arrayDimension = identNode.ArrayDimensions;
                    rnode = identNode;
                }
                else if (identList.RightNode is FunctionCallNode)
                {
                    FunctionCallNode fcNode = new FunctionCallNode(identList.RightNode as FunctionCallNode);
                    arrayDimension = fcNode.ArrayDimensions;

                    List<AssociativeNode> astlistArgs = new List<AssociativeNode>();
                    for (int idx = 0; idx < fcNode.FormalArguments.Count; idx++)
                    {
                        AssociativeNode arg = fcNode.FormalArguments[idx];
                        var replicationGuides = GetReplicationGuides(arg);
                        if (replicationGuides == null)
                        {
                            replicationGuides = new List<AssociativeNode> { };
                        }
                        else
                        {
                            RemoveReplicationGuides(arg);
                        }

                        var atLevel = GetAtLevel(arg);
                        if (atLevel != null)
                        {
                            RemoveAtLevel(arg);
                        }

                        DFSEmitSSA_AST(arg, ssaStack, ref astlistArgs);

                        var argNode = ssaStack.Pop();
                        var argBinaryExpr = argNode as BinaryExpressionNode;
                        if (argBinaryExpr != null)
                        {
                            var newArgNode = NodeUtils.Clone(argBinaryExpr.LeftNode) as IdentifierNode;
                            newArgNode.ReplicationGuides = replicationGuides;
                            newArgNode.AtLevel = atLevel;
                            fcNode.FormalArguments[idx] = newArgNode;
                        }
                        else
                        {
                            fcNode.FormalArguments[idx] = argNode;
                        }
                        astlist.AddRange(astlistArgs);
                        astlistArgs.Clear();
                    }

                    astlist.AddRange(astlistArgs);
                    rnode = fcNode;
                }
                else
                {
                    Validity.Assert(false);
                }

                Validity.Assert(null != rnode);
                rhsIdentList.RightNode = rnode;

                if (null == arrayDimension)
                {
                    // New SSA expr for the current dot call
                    string ssatemp = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
                    var tmpIdent = AstFactory.BuildIdentifier(ssatemp);
                    BinaryExpressionNode bnode = new BinaryExpressionNode(tmpIdent, rhsIdentList, Operator.assign);
                    bnode.isSSAPointerAssignment = true;
                    if (resolvedCall || isLeftNodeExprList)
                    {
                        bnode.IsFirstIdentListNode = true;
                    }
                    astlist.Add(bnode);
                    ssaStack.Push(bnode);
                }
                else
                {
                    List<AssociativeNode> ssaAstList = new List<AssociativeNode>();
                    EmitSSAArrayIndex(rhsIdentList, ssaStack, ref ssaAstList, true);
                    if (resolvedCall || isLeftNodeExprList)
                    {
                        (ssaAstList[0] as BinaryExpressionNode).IsFirstIdentListNode = true;
                    }
                    astlist.AddRange(ssaAstList);
                }
            }
        }

        /// <summary>
        /// This function applies SSA tranform to an array indexed identifier or identifier list
        /// 
        /// The transform applies as such:
        ///     i = 0
        ///     j = 1
        ///     x = a[i][j]
        ///     
        ///     t0 = a
        ///     t1 = i
        ///     t2 = t0[t1]
        ///     t3 = j
        ///     t4 = t2[t3]
        ///     x = t4
        /// 
        /// </summary>
        /// <param name="idendNode"></param>
        /// <param name="ssaStack"></param>
        /// <param name="astlist"></param>
        private void EmitSSAArrayIndex(AssociativeNode node, Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist, bool isSSAPointerAssignment = false)
        {
            //
            // Build the first SSA binary assignment of the identifier without its indexing
            //  x = a[i][j] -> t0 = a
            //
            BinaryExpressionNode bnode = new BinaryExpressionNode();
            bnode.Optr = ProtoCore.DSASM.Operator.assign;

            // Left node
            string ssaTempName = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
            var tmpName = AstFactory.BuildIdentifier(ssaTempName);
            bnode.LeftNode = tmpName;
            bnode.isSSAAssignment = true;
            bnode.isSSAPointerAssignment = isSSAPointerAssignment;
            bnode.isSSAFirstAssignment = true;

            IdentifierNode identNode = null;
            ArrayNode arrayDimensions = null;
            string firstVarName = string.Empty;
            if (node is IdentifierNode)
            {
                identNode = node as IdentifierNode;

                // Right node - Array indexing will be applied to this new identifier
                firstVarName = identNode.Name;
                bnode.RightNode = AstFactory.BuildIdentifier(firstVarName);
                ProtoCore.Utils.CoreUtils.CopyDebugData(bnode.RightNode, node);

                // Get the array dimensions of this node
                arrayDimensions = identNode.ArrayDimensions;


                // Associate the first pointer with each SSA temp
                // It is acceptable to store firstVar even if it is not a pointer as ResolveFinalNodeRefs() will just ignore this entry 
                // if this ssa temp is not treated as a pointer inside the function
                if (!ssaTempToFirstPointerMap.ContainsKey(ssaTempName))
                {
                    ssaTempToFirstPointerMap.Add(ssaTempName, firstVarName);
                }
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode fcall = node as FunctionCallNode;
                Validity.Assert(fcall.Function is IdentifierNode);
                identNode = fcall.Function as IdentifierNode;

                // Assign the function array and guide properties to the new ident node
                identNode.ArrayDimensions = fcall.ArrayDimensions;
                identNode.ReplicationGuides = fcall.ReplicationGuides;
                identNode.AtLevel = fcall.AtLevel;

                // Right node - Remove the function array indexing.
                // The array indexing to this function will be applied downstream 
                fcall.ArrayDimensions = null;
                bnode.RightNode = fcall;
                //ProtoCore.Utils.CoreUtils.CopyDebugData(bnode.RightNode, node);

                // Get the array dimensions of this node
                arrayDimensions = identNode.ArrayDimensions;
            }
            else if (node is IdentifierListNode)
            {
                // Apply array indexing to the right node of the ident list
                //      a = p.x[i] -> apply it to x
                IdentifierListNode identList = node as IdentifierListNode;
                AssociativeNode rhsNode = identList.RightNode;

                if (rhsNode is IdentifierNode)
                {
                    identNode = rhsNode as IdentifierNode;

                    // Replace the indexed identifier with a new ident with the same name
                    //      i.e. replace x[i] with x
                    AssociativeNode nonIndexedIdent = AstFactory.BuildIdentifier(identNode.Name);
                    identList.RightNode = nonIndexedIdent;

                    // Get the array dimensions of this node
                    arrayDimensions = identNode.ArrayDimensions;
                }
                else if (rhsNode is FunctionCallNode)
                {
                    FunctionCallNode fcall = rhsNode as FunctionCallNode;
                    identNode = fcall.Function as IdentifierNode;

                    AssociativeNode newCall = AstFactory.BuildFunctionCall(identNode.Name, fcall.FormalArguments);

                    // Assign the function array and guide properties to the new ident node
                    identNode.ArrayDimensions = fcall.ArrayDimensions;
                    identNode.ReplicationGuides = fcall.ReplicationGuides;
                    identNode.AtLevel = fcall.AtLevel;

                    identList.RightNode = newCall;

                    // Get the array dimensions of this node
                    arrayDimensions = identNode.ArrayDimensions;
                }
                else
                {
                    Validity.Assert(false, "This token is not indexable");
                }

                // Right node
                bnode.RightNode = identList;
                //ProtoCore.Utils.CoreUtils.CopyDebugData(bnode.RightNode, node);

                bnode.isSSAPointerAssignment = true;
            }
            else if (node is GroupExpressionNode)
            {
                GroupExpressionNode groupExpr = node as GroupExpressionNode;
                DFSEmitSSA_AST(groupExpr.Expression, ssaStack, ref astlist);

                arrayDimensions = groupExpr.ArrayDimensions;
            }
            else
            {
                Validity.Assert(false);
            }

            // Push the first SSA stmt
            astlist.Add(bnode);
            ssaStack.Push(bnode);

            // Traverse the array index
            //      t1 = i
            List<AssociativeNode> arrayDimASTList = new List<AssociativeNode>();
            DFSEmitSSA_AST(arrayDimensions.Expr, ssaStack, ref arrayDimASTList);
            astlist.AddRange(arrayDimASTList);
            arrayDimASTList.Clear();

            //
            // Build the indexing statement
            //      t2 = t0[t1]
            BinaryExpressionNode indexedStmt = new BinaryExpressionNode();
            indexedStmt.Optr = ProtoCore.DSASM.Operator.assign;


            // Build the left node of the indexing statement
            #region SSA_INDEX_STMT_LEFT
            ssaTempName = ProtoCore.Utils.CoreUtils.BuildSSATemp(core);
            AssociativeNode tmpIdent = AstFactory.BuildIdentifier(ssaTempName);
            Validity.Assert(null != tmpIdent);
            indexedStmt.LeftNode = tmpIdent;
            //ProtoCore.Utils.CoreUtils.CopyDebugData(bnode, node);



            // Associate the first pointer with each SSA temp
            // It is acceptable to store firstVar even if it is not a pointer as ResolveFinalNodeRefs() will just ignore this entry 
            // if this ssa temp is not treated as a pointer inside the function
            if (!string.IsNullOrEmpty(firstVarName))
            {
                if (!ssaTempToFirstPointerMap.ContainsKey(ssaTempName))
                {
                    ssaTempToFirstPointerMap.Add(ssaTempName, firstVarName);
                }
            }

            #endregion

            // Build the right node of the indexing statement
            #region SSA_INDEX_STMT_RIGHT

            ArrayNode arrayNode = null;

            // pop off the dimension
            AssociativeNode dimensionNode = ssaStack.Pop();
            if (dimensionNode is BinaryExpressionNode)
            {
                arrayNode = new ArrayNode((dimensionNode as BinaryExpressionNode).LeftNode, null);
            }
            else
            {
                arrayNode = new ArrayNode(dimensionNode, null);
            }

            // pop off the prev SSA variable
            AssociativeNode prevSSAStmt = ssaStack.Pop();

            // Assign the curent dimension to the prev SSA variable
            Validity.Assert(prevSSAStmt is BinaryExpressionNode);
            AssociativeNode rhsIdent = AstFactory.BuildIdentifier((prevSSAStmt as BinaryExpressionNode).LeftNode.Name);
            (rhsIdent as IdentifierNode).ArrayDimensions = arrayNode;

            // Right node of the indexing statement
            indexedStmt.RightNode = rhsIdent;

            astlist.Add(indexedStmt);
            ssaStack.Push(indexedStmt);


            // Traverse the next dimension
            //      [j]
            if (null != arrayDimensions.Type)
            {
                DFSEmitSSA_AST(arrayDimensions.Type, ssaStack, ref arrayDimASTList);
                astlist.AddRange(arrayDimASTList);
                arrayDimASTList.Clear();
            }
            #endregion
        }

        /// <summary>
        /// Emits the SSA form of each dimension in the ArrayNode data structure
        /// The dimensionNode is updated by the function with the SSA'd dimensions
        /// 
        /// Given:
        ///     dimensionNode -> a[b][c] 
        /// Outputs:
        ///     astlist -> t0 = b
        ///             -> t1 = c
        ///     dimensionNode -> a[t0][t1] 
        ///     
        /// </summary>
        /// <param name="dimensionNode"></param>
        /// <param name="astlist"></param>
        private void EmitSSAforArrayDimension(ref ArrayNode dimensionNode, ref List<AssociativeNode> astlist)
        {
            AssociativeNode indexNode = dimensionNode.Expr;

            // Traverse first dimension
            Stack<AssociativeNode> localStack = new Stack<AssociativeNode>();
            DFSEmitSSA_AST(indexNode, localStack, ref astlist);

            AssociativeNode tempIndexNode = localStack.Last();
            if (tempIndexNode.Kind == AstKind.BinaryExpression)
            {
                dimensionNode.Expr = (tempIndexNode as BinaryExpressionNode).LeftNode;

                // Traverse next dimension
                indexNode = dimensionNode.Type;
                while (indexNode is ArrayNode)
                {
                    ArrayNode arrayNode = indexNode as ArrayNode;
                    localStack = new Stack<AssociativeNode>();
                    DFSEmitSSA_AST(arrayNode.Expr, localStack, ref astlist);
                    tempIndexNode = localStack.Last();
                    if (tempIndexNode is BinaryExpressionNode)
                    {
                        arrayNode.Expr = (tempIndexNode as BinaryExpressionNode).LeftNode;
                    }
                    indexNode = arrayNode.Type;
                }
            }
        }

        /// <summary>
        /// Apply SSA to array indices, while preserving the original dimension.
        ///    
        /// Given:
        ///     a[b][c][d] = 1  
        /// Emit:
        ///     t0 = b
        ///     t1 = c
        ///     t2 = d
        ///     a[t0][t1][t3] = 1
        /// 
        /// The function will perform the following
        ///     1. Get the array index list from the node 
        ///     2. For each index:  Apply SSA transform 
        ///     3.                  Replace the original index with the SSA temp. 
        ///                         This replaces the original dimensions in the indexed array  
        ///     
        ///     
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ssaStack"></param>
        /// <param name="astlist"></param>
        /// <param name="isSSAPointerAssignment"></param>
        private void EmitSSAArrayIndexRetainDimension(
            ref AssociativeNode node,
            ref List<AssociativeNode> astlist,
            bool isSSAPointerAssignment = false)
        {
            IdentifierNode identNode = null;
            ArrayNode arrayDimensions = null;
            if (node is IdentifierNode)
            {
                // Retrieve arrayindex data structure (Arraynode) for an identifier
                // a[b][c] -> [b][c]

                identNode = node as IdentifierNode;
                arrayDimensions = identNode.ArrayDimensions;
            }
            else if (node is FunctionCallNode)
            {
                // Retrieve arrayindex data structure (Arraynode) for an identifier
                // a()[b][c] -> [b][c]

                FunctionCallNode fcall = node as FunctionCallNode;
                Validity.Assert(fcall.Function is IdentifierNode);
                identNode = fcall.Function as IdentifierNode;

                // Get the array dimensions of this node
                arrayDimensions = identNode.ArrayDimensions;
            }
            else if (node is IdentifierListNode)
            {
                // Retrieve arrayindex data structure (Arraynode) for an identifier
                // x.y[b][c] -> [b][c]

                // Apply array indexing to the right node of the ident list
                //      a = p.x[i] -> apply it to x
                IdentifierListNode identList = node as IdentifierListNode;
                AssociativeNode rhsNode = identList.RightNode;

                if (rhsNode is IdentifierNode)
                {
                    identNode = rhsNode as IdentifierNode;

                    // Get the array dimensions of this node
                    arrayDimensions = identNode.ArrayDimensions;
                }
                else if (rhsNode is FunctionCallNode)
                {
                    FunctionCallNode fcall = rhsNode as FunctionCallNode;
                    identNode = fcall.Function as IdentifierNode;

                    // Get the array dimensions of this node
                    arrayDimensions = identNode.ArrayDimensions;
                }
                else
                {
                    Validity.Assert(false, "This token is not indexable");
                }
            }
            else if (node is GroupExpressionNode)
            {
                // Retrieve arrayindex data structure (Arraynode) for an identifier
                // (a + b)[b][c] -> [b][c]

                GroupExpressionNode groupExpr = node as GroupExpressionNode;
                arrayDimensions = groupExpr.ArrayDimensions;
            }
            else
            {
                Validity.Assert(false);
            }

            EmitSSAforArrayDimension(ref arrayDimensions, ref astlist);
        }

        /// <summary>
        /// Handle ssa transforms for LHS identifiers
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ssaStack"></param>
        /// <param name="astlist"></param>
        private void EmitSSALHS(ref AssociativeNode node, Stack<AssociativeNode> ssaStack, ref List<AssociativeNode> astlist)
        {
            if (node is IdentifierNode && null != (node as IdentifierNode).ArrayDimensions)
            {
                // Handle LHS ssa for indexed identifiers
                EmitSSAArrayIndexRetainDimension(ref node, ref astlist);
            }

            // Handle other LHS cases here
            else if (node is IdentifierListNode)
            {
                // TODO Handle LHS ssa for identifier lists

                // For LHS idenlist, resolve them to the fully qualified name
                IdentifierListNode identList = node as IdentifierListNode;
                string[] classNames = ProtoCore.Utils.CoreUtils.GetResolvedClassName(core.ClassTable, identList);
                if (classNames.Length == 1)
                {
                    identList.LeftNode.Name = classNames[0];
                    (identList.LeftNode as IdentifierNode).Value = classNames[0];
                }
            }
        }
    }
}
