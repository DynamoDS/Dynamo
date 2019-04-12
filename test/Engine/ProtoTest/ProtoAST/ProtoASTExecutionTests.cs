using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;

namespace ProtoTest.ProtoAST
{
    class ProtoASTExecutionTests : ProtoTestBase
    {
        [Test]
        public void TestProtoASTExecute_Assign01()
        {
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void TestProtoASTExecute_Assign02()
        {
            // Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IntNode(10),
                    new ProtoCore.AST.AssociativeAST.IntNode(20),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", 30);
        }

        [Test]
        public void TestProtoASTExecute_Assign03()
        {
            /*b = 20;*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(20),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b + 50)*(b + 20)*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                        new ProtoCore.AST.AssociativeAST.IntNode(50),
                        ProtoCore.DSASM.Operator.add),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                        new ProtoCore.AST.AssociativeAST.IntNode(20),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.mul),
                ProtoCore.DSASM.Operator.assign);
            /*c = a - 200*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IntNode(200),
                    ProtoCore.DSASM.Operator.sub),
                ProtoCore.DSASM.Operator.assign);
            /*d = b*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign4 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("d"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);
            astList.Add(assign1);
            astList.Add(assign3);
            astList.Add(assign4);
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            //a = 2800, c = 2600, d = b = 20
            thisTest.Verify("a", 2800);
            thisTest.Verify("c", 2600);
            thisTest.Verify("b", 20);
            thisTest.Verify("d", 20);
        }

        [Test]
        public void TestProtoASTExecute_Assign04()
        {
            /*b = 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(30),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b - 10) * 20 + (b + 10) * (b - 20) */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment =
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode(10),
                                ProtoCore.DSASM.Operator.sub),
                            new ProtoCore.AST.AssociativeAST.IntNode(20),
                            ProtoCore.DSASM.Operator.mul),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode(10),
                                ProtoCore.DSASM.Operator.add),
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode(20),
                                ProtoCore.DSASM.Operator.sub),
                            ProtoCore.DSASM.Operator.mul),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.assign);
            /*c = a*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeC = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);
            /*a = a + 1000*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IntNode(1000),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(nodeB);
            astList.Add(assignment);
            astList.Add(nodeC);
            astList.Add(assignment2);
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            //a = 1800, c = a = 1800
            thisTest.Verify("a", 1800);
            thisTest.Verify("c", 1800);
        }

        [Test]
        public void TestProtoASTExecute_Assign05()
        {
            /*b = 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(30),
                ProtoCore.DSASM.Operator.assign);
            /*c = b + 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeC = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                    new ProtoCore.AST.AssociativeAST.IntNode(30),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b + 20) - (c - 10) + c*5 */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                            new ProtoCore.AST.AssociativeAST.IntNode(20),
                            ProtoCore.DSASM.Operator.add),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                            new ProtoCore.AST.AssociativeAST.IntNode(10),
                            ProtoCore.DSASM.Operator.sub),
                        ProtoCore.DSASM.Operator.sub),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                        new ProtoCore.AST.AssociativeAST.IntNode(5),
                        ProtoCore.DSASM.Operator.mul),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(nodeB);
            astList.Add(nodeC);
            astList.Add(assignment);
            /*a = 300, b = 30, c= 60 */
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", 300);
        }

        #region Array Index
        [Test]
        public void TestProtoASTExecute_ArrayIndex_LHS_Assign01()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = {1, 2, 3, 4};
            int[] input = { 1, 2, 3, 4 };
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = CreateDeclareArrayNode("a", input);
            astList.Add(declareNodeA);

            // a[3] = 0;
            ProtoCore.AST.AssociativeAST.IdentifierNode nodeALHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeALHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode {
                Expr = new ProtoCore.AST.AssociativeAST.IntNode(3)};

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeALHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                nodeALHS,
                new ProtoCore.AST.AssociativeAST.IntNode(0),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(nodeALHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", new [] { 1, 2, 3, 0});
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_LHS_Assign02()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = {1, 2, 3, 4};
            int[] input = { 1, 2, 3, 4 };
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = CreateDeclareArrayNode("a", input);
            astList.Add(declareNodeA);

            // b = 2;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(2),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(declareNodeB);

            // a[b] = b;
            ProtoCore.AST.AssociativeAST.IdentifierNode nodeALHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeALHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode {
                Expr = new ProtoCore.AST.AssociativeAST.IdentifierNode("b") };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeALHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                nodeALHS,
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(nodeALHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", new [] { 1, 2, 2, 4});
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_LHS_Assign03()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = {1, 2, 3, 4};
            int[] input = { 1, 2, 3, 4 };
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = CreateDeclareArrayNode("a", input);
            astList.Add(declareNodeA);

            // b = 3;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(3),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(declareNodeB);

            // a[b - 3] = 0;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode operation1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(3),
                ProtoCore.DSASM.Operator.sub);

            ProtoCore.AST.AssociativeAST.IdentifierNode nodeALHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeALHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode() {
                Expr = operation1 };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeALHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                nodeALHS,
                new ProtoCore.AST.AssociativeAST.IdentifierNode("0"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(nodeALHSAssignment);

            // Verify the results
            thisTest.RunASTSource(astList);
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_LHS_Assign04()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = {1, 2, 3, 4};
            int[] input = { 1, 2, 3, 4 };
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = CreateDeclareArrayNode("a", input);
            astList.Add(declareNodeA);

            // b = 4;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(4),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(declareNodeB);

            // def foo(){
            //    return = -2;
            // }
            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                new ProtoCore.AST.AssociativeAST.IntNode(-2),
                ProtoCore.DSASM.Operator.assign);
            cbn.Body.Add(returnNode);

            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode() {
                Name = functionName,
                FunctionBody = cbn };

            // Function Return Type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.Var;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            astList.Add(funcDefNode);

            // a[b + foo()] = -1;
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode(functionName);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode operation1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                functionCall,
                ProtoCore.DSASM.Operator.add);

            ProtoCore.AST.AssociativeAST.IdentifierNode nodeALHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeALHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode();
            nodeALHS.ArrayDimensions.Expr = operation1;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeALHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                nodeALHS,
                new ProtoCore.AST.AssociativeAST.IntNode(-1),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(nodeALHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", new [] { 1, 2, -1, 4});
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_LHS_Assign05()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = { { 0, 1 }, { 3, 4, 5 } };
            int[] input1 = { 0, 1 };
            int[] input2 = { 3, 4, 5 };
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> arrayList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            arrayList.Add(CreateExprListNodeFromArray(input1));
            arrayList.Add(CreateExprListNodeFromArray(input2));

            ProtoCore.AST.AssociativeAST.ExprListNode arrayExpr = new ProtoCore.AST.AssociativeAST.ExprListNode { Exprs = arrayList };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                arrayExpr,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(declareNodeA);

            // b = 2;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(2),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(declareNodeB);

            // a[0][b] = b;
            ProtoCore.AST.AssociativeAST.IdentifierNode nodeALHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeALHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode
            {
                Expr = new ProtoCore.AST.AssociativeAST.IntNode(0),
                Type = new ProtoCore.AST.AssociativeAST.ArrayNode
                {
                    Expr = new ProtoCore.AST.AssociativeAST.IdentifierNode("b")
                }
            };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeALHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                nodeALHS,
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(nodeALHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", new [] { new [] { 0, 1, 2}, new [] { 3, 4, 5}});
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_RHS_Assign01()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = {1, 2, 3, 4};
            int[] input = { 1, 2, 3, 4 };
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = CreateDeclareArrayNode("a", input);
            astList.Add(declareNodeA);

            // b = a[2];
            ProtoCore.AST.AssociativeAST.IdentifierNode nodeALHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeALHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode {
                Expr = new ProtoCore.AST.AssociativeAST.IntNode(2) };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeALHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                nodeALHS,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(nodeALHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("b", 3);
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_RHS_Assign02()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = {1, 2, 3, 4};
            int[] input = { 1, 2, 3, 4 };
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = CreateDeclareArrayNode("a", input);
            astList.Add(declareNodeA);

            // b = 3;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(3),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(declareNodeB);

            // c = a[b];
            ProtoCore.AST.AssociativeAST.IdentifierNode nodeARHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeARHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode {
                Expr = new ProtoCore.AST.AssociativeAST.IdentifierNode("b") };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeARHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                nodeARHS,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(nodeARHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("c", 4);
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_RHS_Assign03()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = {1, 2, 3, 4};
            int[] input = { 1, 2, 3, 4 };
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = CreateDeclareArrayNode("a", input);
            astList.Add(declareNodeA);

            // b = -1;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(-1),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(declareNodeB);

            // c = a[b + 3];
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode operation1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(3),
                ProtoCore.DSASM.Operator.add);

            ProtoCore.AST.AssociativeAST.IdentifierNode nodeALHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeALHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode {
                Expr = operation1};

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeALHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                nodeALHS,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(nodeALHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("c", 3);
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_RHS_Assign04()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = {1, 2, 3, 4};
            int[] input = { 1, 2, 3, 4 };
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = CreateDeclareArrayNode("a", input);
            astList.Add(declareNodeA);

            // b = 4;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(5),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(declareNodeB);

            // def foo(){
            //    return = -4;
            // }
            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                new ProtoCore.AST.AssociativeAST.IntNode(-4),
                ProtoCore.DSASM.Operator.assign);
            cbn.Body.Add(returnNode);

            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode() {
                Name = functionName,
                FunctionBody = cbn };

            // Function Return Type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.Var;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            astList.Add(funcDefNode);

            // c = a[b + foo()];
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode(functionName);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode operation1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                functionCall,
                ProtoCore.DSASM.Operator.add);

            ProtoCore.AST.AssociativeAST.IdentifierNode nodeALHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeALHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode {
                Expr = operation1 };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeALHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                nodeALHS,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(nodeALHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("c", 2);
        }

        [Test]
        public void TestProtoASTExecute_ArrayIndex_RHS_Assign05()
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // a = { { 0, 1 }, { 3, 4, 5 } };
            int[] input1 = { 0, 1 };
            int[] input2 = { 2, 3, 4 };
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> arrayList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            arrayList.Add(CreateExprListNodeFromArray(input1));
            arrayList.Add(CreateExprListNodeFromArray(input2));

            ProtoCore.AST.AssociativeAST.ExprListNode arrayExpr = new ProtoCore.AST.AssociativeAST.ExprListNode { Exprs = arrayList };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeA = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                arrayExpr,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(declareNodeA);

            // b = 2;
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareNodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(2),
                ProtoCore.DSASM.Operator.assign);
            astList.Add(declareNodeB);

            // c = a[1][b];
            ProtoCore.AST.AssociativeAST.IdentifierNode nodeARHS = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");
            nodeARHS.ArrayDimensions = new ProtoCore.AST.AssociativeAST.ArrayNode
            {
                Expr = new ProtoCore.AST.AssociativeAST.IntNode(1),
                Type = new ProtoCore.AST.AssociativeAST.ArrayNode
                {
                    Expr = new ProtoCore.AST.AssociativeAST.IdentifierNode("b")
                }
            };

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeARHSAssignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                nodeARHS,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(nodeARHSAssignment);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("c", 4);
        }

        private ProtoCore.AST.AssociativeAST.BinaryExpressionNode CreateDeclareArrayNode(string name, int[] intList)
        {
            ProtoCore.AST.AssociativeAST.ExprListNode expr = CreateExprListNodeFromArray(intList);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode declareArrayNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(name),
                expr,
                ProtoCore.DSASM.Operator.assign);

            return declareArrayNode;
        }

        private ProtoCore.AST.AssociativeAST.ExprListNode CreateExprListNodeFromArray(int[] intList)
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listIntNode = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            for (int i = 0; i < intList.Length; i++)
                listIntNode.Add(new ProtoCore.AST.AssociativeAST.IntNode(intList[i]));

            ProtoCore.AST.AssociativeAST.ExprListNode expr = new ProtoCore.AST.AssociativeAST.ExprListNode { Exprs = listIntNode };

            return expr;
        }

        #endregion

        [Test]
        public void TestProtoASTExecute_FunctionDefAndCall_01()
        {
            //  def foo()
            //  {
            //    b = 10;
            //    return = b + 10;
            //  }
            //  
            //  x = foo();

            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();


            // Build the function body
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.add);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                returnExpr,
                ProtoCore.DSASM.Operator.assign);
            cbn.Body.Add(assignment1);
            cbn.Body.Add(returnNode);


            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = functionName;
            funcDefNode.FunctionBody = cbn;

            // Function Return type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.Var;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(funcDefNode);

            // Build the statement that calls the function foo
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode(functionName);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode callstmt = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                functionCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(callstmt);


            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("x", 20);
        }

        [Test]
        public void TestProtoASTExecute_FunctionDefAndCall_02()
        {
            //  def foo(a : int)
            //  {
            //    b = 10;
            //    return = b + a;
            //  }
            //  
            //  x = foo(1);

            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();


            // Build the function body
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.add);


            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                returnExpr,
                ProtoCore.DSASM.Operator.assign);

            cbn.Body.Add(assignment1);
            cbn.Body.Add(returnNode);


            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = functionName;
            funcDefNode.FunctionBody = cbn;

            // build the args signature
            funcDefNode.Signature = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
            ProtoCore.AST.AssociativeAST.VarDeclNode arg1Decl = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            arg1Decl.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");

            // Build the type of arg1
            ProtoCore.Type arg1Type = new ProtoCore.Type();
            arg1Type.Initialize();
            arg1Type.UID = (int)ProtoCore.PrimitiveType.Integer;
            arg1Type.Name = ProtoCore.DSDefinitions.Keyword.Int;
            arg1Decl.ArgumentType = arg1Type;
            funcDefNode.Signature.AddArgument(arg1Decl);


            // Function Return type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.Var;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            // Build the statement that calls the function foo
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode(functionName);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(funcDefNode);

            // Function call
            // Function args
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> args = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            args.Add(new ProtoCore.AST.AssociativeAST.IntNode(1));
            functionCall.FormalArguments = args;

            // Call the function
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode callstmt = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                functionCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(callstmt);


            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("x", 11);
        }

        [Test]
        public void TestProtoASTExecute_FunctionDefAndCall_03()
        {
            //  def add(a : int, b : int)
            //  {
            //    return = a + b;
            //  }
            //  
            //  x = add(2,3);

            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();


            // Build the function body
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.add);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode(ProtoCore.DSDefinitions.Keyword.Return),
                returnExpr,
                ProtoCore.DSASM.Operator.assign);

            cbn.Body.Add(returnNode);


            // Build the function definition foo
            const string functionName = "foo";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = functionName;
            funcDefNode.FunctionBody = cbn;

            // build the args signature
            funcDefNode.Signature = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();

            // Build arg1
            ProtoCore.AST.AssociativeAST.VarDeclNode arg1Decl = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            arg1Decl.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("a");

            // Build the type of arg1
            ProtoCore.Type arg1Type = new ProtoCore.Type();
            arg1Type.Initialize();
            arg1Type.UID = (int)ProtoCore.PrimitiveType.Integer;
            arg1Type.Name = ProtoCore.DSDefinitions.Keyword.Int;
            arg1Decl.ArgumentType = arg1Type;
            funcDefNode.Signature.AddArgument(arg1Decl);

            // Build arg2
            ProtoCore.AST.AssociativeAST.VarDeclNode arg2Decl = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            arg2Decl.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("b");

            // Build the type of arg2
            ProtoCore.Type arg2Type = new ProtoCore.Type();
            arg2Type.Initialize();
            arg2Type.UID = (int)ProtoCore.PrimitiveType.Integer;
            arg2Type.Name = ProtoCore.DSDefinitions.Keyword.Int;
            arg2Decl.ArgumentType = arg2Type;
            funcDefNode.Signature.AddArgument(arg2Decl);


            // Function Return type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.Var;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            // Build the statement that calls the function foo
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode(functionName);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(funcDefNode);

            // Function call
            // Function args
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> args = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            args.Add(new ProtoCore.AST.AssociativeAST.IntNode(2));
            args.Add(new ProtoCore.AST.AssociativeAST.IntNode(3));
            functionCall.FormalArguments = args;

            // Call the function
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode callstmt = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                functionCall,
                ProtoCore.DSASM.Operator.assign);
            astList.Add(callstmt);


            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("x", 5);
        }

        [Test]
        public void TestCodeGenDS_Assign01()
        {
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            // emit the DS code from the AST tree
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();
            // Verify the results
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void TestCodeGenDS_Assign02()
        {
            // Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IntNode(10),
                    new ProtoCore.AST.AssociativeAST.IntNode(20),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            // emit the DS code from the AST tree
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();
            // Verify the results
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 30);
        }

        [Test]
        public void TestCodeGenDS_Assign03()
        {
            /*b = 20;*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(20),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b + 50)*(b + 20)*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                        new ProtoCore.AST.AssociativeAST.IntNode(50),
                        ProtoCore.DSASM.Operator.add),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                        new ProtoCore.AST.AssociativeAST.IntNode(20),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.mul),
                ProtoCore.DSASM.Operator.assign);
            /*c = a - 200*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IntNode(200),
                    ProtoCore.DSASM.Operator.sub),
                ProtoCore.DSASM.Operator.assign);
            /*d = b*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign4 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("d"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);
            astList.Add(assign1);
            astList.Add(assign3);
            astList.Add(assign4);

            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //a = 2800, c = 2600, d = b = 20
            thisTest.Verify("a", 2800);
            thisTest.Verify("c", 2600);
            thisTest.Verify("b", 20);
            thisTest.Verify("d", 20);
        }

        [Test]
        public void TestCodeGenDS_Assign04()
        {
            /*b = 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(30),
                ProtoCore.DSASM.Operator.assign);

            /*a = (b - 10) * 20 + (b + 10) * (b - 20) */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment =
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode(10),
                                ProtoCore.DSASM.Operator.sub),
                            new ProtoCore.AST.AssociativeAST.IntNode(20),
                            ProtoCore.DSASM.Operator.mul),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode(10),
                                ProtoCore.DSASM.Operator.add),
                            new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                                new ProtoCore.AST.AssociativeAST.IntNode(20),
                                ProtoCore.DSASM.Operator.sub),
                            ProtoCore.DSASM.Operator.mul),
                        ProtoCore.DSASM.Operator.add),
                    ProtoCore.DSASM.Operator.assign);
            /*c = a*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeC = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);
            /*a = a + 1000*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IntNode(1000),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(nodeB);
            astList.Add(assignment);
            astList.Add(nodeC);
            astList.Add(assignment2);
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //a = 1800, c = a = 1800
            thisTest.Verify("a", 1800);
            thisTest.Verify("c", 1800);
        }

        [Test]
        public void TestCodeGenDS_Assign05()
        {
            /*b = 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeB = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(30),
                ProtoCore.DSASM.Operator.assign);
            /*c = b + 30*/
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode nodeC = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                    new ProtoCore.AST.AssociativeAST.IntNode(30),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            /*a = (b + 20) - (c - 10) + c*5 */
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                            new ProtoCore.AST.AssociativeAST.IntNode(20),
                            ProtoCore.DSASM.Operator.add),
                        new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                            new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                            new ProtoCore.AST.AssociativeAST.IntNode(10),
                            ProtoCore.DSASM.Operator.sub),
                        ProtoCore.DSASM.Operator.sub),
                    new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                        new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                        new ProtoCore.AST.AssociativeAST.IntNode(5),
                        ProtoCore.DSASM.Operator.mul),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(nodeB);
            astList.Add(nodeC);
            astList.Add(assignment);
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();
            /*a = 300, b = 30, c= 60 */
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 300);
        }

        [Test]
        public void TestCodeGenDS_FunctionDefNode1()
        {
            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assignment1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnExpr = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.add);

            var returnIdent = new ProtoCore.AST.AssociativeAST.IdentifierNode("return");
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode returnNode = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(returnIdent, returnExpr);
            cbn.Body.Add(assignment1);
            cbn.Body.Add(returnNode);
            ///
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            funcDefNode.Name = "foo";
            funcDefNode.FunctionBody = cbn;
            /* def foo()
             * {
             *   b = 10;
             *   return = b + 10;
             * }*/
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(funcDefNode);
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();
        }



        [Test]
        public void TestcodegenDS_Imperative_Assign01()
        {
            //
            //  a = [Imperative]
            //  {
            //      return = 10;
            //  }
            //

            List<ProtoCore.AST.ImperativeAST.ImperativeNode> imperativeList = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();

            // return = 10
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode imperativeAssign = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("return"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(imperativeAssign);


            // Build the language block
            ProtoCore.AST.ImperativeAST.CodeBlockNode imperativeCodeBlock = new ProtoCore.AST.ImperativeAST.CodeBlockNode();
            imperativeCodeBlock.Body = imperativeList;

            ProtoCore.AST.AssociativeAST.LanguageBlockNode langblock = new ProtoCore.AST.AssociativeAST.LanguageBlockNode();
            langblock.codeblock = new ProtoCore.LanguageCodeBlock(ProtoCore.Language.Imperative);
            langblock.CodeBlockNode = imperativeCodeBlock;


            // Build an assignment where the rhs is the imperative block
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                langblock,
                ProtoCore.DSASM.Operator.assign);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Generate the script
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();


            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void TestCodegenDS_Imperative_IfStatement01()
        {
            //
            //  a = [Imperative]
            //  {
            //      b = 10;
            //      if (b == 10)
            //      {
            //          b = 11;
            //      }
            //      return = b;
            //  }
            //

            List<ProtoCore.AST.ImperativeAST.ImperativeNode> imperativeList = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();

            // b = 10
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode imperativeAssign = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(imperativeAssign);

            // if (b == 10)
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode equality = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.eq);

            ProtoCore.AST.ImperativeAST.IfStmtNode ifNode = new ProtoCore.AST.ImperativeAST.IfStmtNode();
            ifNode.IfExprNode = equality;

            // b = 11
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode ifCodeBlockStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(11),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.ImperativeAST.ImperativeNode> ifCodeBlock = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();
            ifCodeBlock.Add(ifCodeBlockStmt);
            ifNode.IfBody = ifCodeBlock;

            imperativeList.Add(ifNode);

            // return = b
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode returnStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("return"),
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(returnStmt);


            // Build the language block
            ProtoCore.AST.ImperativeAST.CodeBlockNode imperativeCodeBlock = new ProtoCore.AST.ImperativeAST.CodeBlockNode();
            imperativeCodeBlock.Body = imperativeList;

            ProtoCore.AST.AssociativeAST.LanguageBlockNode langblock = new ProtoCore.AST.AssociativeAST.LanguageBlockNode();
            langblock.codeblock = new ProtoCore.LanguageCodeBlock(ProtoCore.Language.Imperative);
            langblock.CodeBlockNode = imperativeCodeBlock;


            // Build an assignment where the rhs is the imperative block
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                langblock,
                ProtoCore.DSASM.Operator.assign);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Generate the script
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();


            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 11);
        }

        [Test]
        public void TestCodegenDS_Imperative_IfStatement02()
        {
            //
            //  a = [Imperative]
            //  {
            //      b = 10;
            //      if (b > 10)
            //      {
            //          b = 11;
            //      }
            //      else
            //      {
            //          b = 12
            //      }
            //      return = b;
            //  }
            //

            List<ProtoCore.AST.ImperativeAST.ImperativeNode> imperativeList = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();

            // b = 10
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode imperativeAssign = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(imperativeAssign);

            // if (b > 10)
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode equality = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.gt);

            ProtoCore.AST.ImperativeAST.IfStmtNode ifNode = new ProtoCore.AST.ImperativeAST.IfStmtNode();
            ifNode.IfExprNode = equality;

            // if body
            // b = 11
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode ifCodeBlockStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(11),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.ImperativeAST.ImperativeNode> ifCodeBlock = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();
            ifCodeBlock.Add(ifCodeBlockStmt);
            ifNode.IfBody = ifCodeBlock;

            // else body
            // b = 12
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode elseCodeBlockStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(12),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.ImperativeAST.ImperativeNode> elseCodeBlock = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();
            elseCodeBlock.Add(elseCodeBlockStmt);
            ifNode.ElseBody = elseCodeBlock;

            imperativeList.Add(ifNode);

            // return = b
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode returnStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("return"),
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(returnStmt);


            // Build the language block
            ProtoCore.AST.ImperativeAST.CodeBlockNode imperativeCodeBlock = new ProtoCore.AST.ImperativeAST.CodeBlockNode();
            imperativeCodeBlock.Body = imperativeList;

            ProtoCore.AST.AssociativeAST.LanguageBlockNode langblock = new ProtoCore.AST.AssociativeAST.LanguageBlockNode();
            langblock.codeblock = new ProtoCore.LanguageCodeBlock(ProtoCore.Language.Imperative);
            langblock.CodeBlockNode = imperativeCodeBlock;


            // Build an assignment where the rhs is the imperative block
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                langblock,
                ProtoCore.DSASM.Operator.assign);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Generate the script
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(astList);
            string code = codegen.GenerateCode();


            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 12);
        }

        [Test]
        public void TestProtoASTExecute_Imperative_Assign01()
        {
            //
            //  a = [Imperative]
            //  {
            //      return = 10;
            //  }
            //

            List<ProtoCore.AST.ImperativeAST.ImperativeNode> imperativeList = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();

            // return = 10
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode imperativeAssign = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("return"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(imperativeAssign);


            // Build the language block
            ProtoCore.AST.ImperativeAST.CodeBlockNode imperativeCodeBlock = new ProtoCore.AST.ImperativeAST.CodeBlockNode();
            imperativeCodeBlock.Body = imperativeList;

            ProtoCore.AST.AssociativeAST.LanguageBlockNode langblock = new ProtoCore.AST.AssociativeAST.LanguageBlockNode();
            langblock.codeblock = new ProtoCore.LanguageCodeBlock(ProtoCore.Language.Imperative);
            langblock.CodeBlockNode = imperativeCodeBlock;


            // Build an assignment where the rhs is the imperative block
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                langblock,
                ProtoCore.DSASM.Operator.assign);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void TestProtoASTExecute_Imperative_IfStatement01()
        {
            //
            //  a = [Imperative]
            //  {
            //      b = 10;
            //      if (b == 10)
            //      {
            //          b = 11;
            //      }
            //      return = b;
            //  }
            //

            List<ProtoCore.AST.ImperativeAST.ImperativeNode> imperativeList = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();

            // b = 10
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode imperativeAssign = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(imperativeAssign);

            // if (b == 10)
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode equality = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.eq);

            ProtoCore.AST.ImperativeAST.IfStmtNode ifNode = new ProtoCore.AST.ImperativeAST.IfStmtNode();
            ifNode.IfExprNode = equality;

            // b = 11
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode ifCodeBlockStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(11),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.ImperativeAST.ImperativeNode> ifCodeBlock = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();
            ifCodeBlock.Add(ifCodeBlockStmt);
            ifNode.IfBody = ifCodeBlock;

            imperativeList.Add(ifNode);

            // return = b
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode returnStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("return"),
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(returnStmt);


            // Build the language block
            ProtoCore.AST.ImperativeAST.CodeBlockNode imperativeCodeBlock = new ProtoCore.AST.ImperativeAST.CodeBlockNode();
            imperativeCodeBlock.Body = imperativeList;

            ProtoCore.AST.AssociativeAST.LanguageBlockNode langblock = new ProtoCore.AST.AssociativeAST.LanguageBlockNode();
            langblock.codeblock = new ProtoCore.LanguageCodeBlock(ProtoCore.Language.Imperative);
            langblock.CodeBlockNode = imperativeCodeBlock;


            // Build an assignment where the rhs is the imperative block
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                langblock,
                ProtoCore.DSASM.Operator.assign);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", 11);
        }

        [Test]
        public void TestProtoASTExecute_Imperative_IfStatement02()
        {
            //
            //  a = [Imperative]
            //  {
            //      b = 10;
            //      if (b > 10)
            //      {
            //          b = 11;
            //      }
            //      else
            //      {
            //          b = 12
            //      }
            //      return = b;
            //  }
            //

            List<ProtoCore.AST.ImperativeAST.ImperativeNode> imperativeList = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();

            // b = 10
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode imperativeAssign = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(imperativeAssign);

            // if (b > 10)
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode equality = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.gt);

            ProtoCore.AST.ImperativeAST.IfStmtNode ifNode = new ProtoCore.AST.ImperativeAST.IfStmtNode();
            ifNode.IfExprNode = equality;

            // if body
            // b = 11
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode ifCodeBlockStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(11),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.ImperativeAST.ImperativeNode> ifCodeBlock = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();
            ifCodeBlock.Add(ifCodeBlockStmt);
            ifNode.IfBody = ifCodeBlock;

            // else body
            // b = 12
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode elseCodeBlockStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                new ProtoCore.AST.ImperativeAST.IntNode(12),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.ImperativeAST.ImperativeNode> elseCodeBlock = new List<ProtoCore.AST.ImperativeAST.ImperativeNode>();
            elseCodeBlock.Add(elseCodeBlockStmt);
            ifNode.ElseBody = elseCodeBlock;

            imperativeList.Add(ifNode);

            // return = b
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode returnStmt = new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(
                new ProtoCore.AST.ImperativeAST.IdentifierNode("return"),
                new ProtoCore.AST.ImperativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);
            imperativeList.Add(returnStmt);


            // Build the language block
            ProtoCore.AST.ImperativeAST.CodeBlockNode imperativeCodeBlock = new ProtoCore.AST.ImperativeAST.CodeBlockNode();
            imperativeCodeBlock.Body = imperativeList;

            ProtoCore.AST.AssociativeAST.LanguageBlockNode langblock = new ProtoCore.AST.AssociativeAST.LanguageBlockNode();
            langblock.codeblock = new ProtoCore.LanguageCodeBlock(ProtoCore.Language.Imperative);
            langblock.CodeBlockNode = imperativeCodeBlock;


            // Build an assignment where the rhs is the imperative block
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                langblock,
                ProtoCore.DSASM.Operator.assign);


            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", 12);
        }

        [Test]
        public void TestLocalizedStringInAST()
        {
            // Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("x"),
                new ProtoCore.AST.AssociativeAST.StringNode() { Value = "中文字符" });
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Verify the results
            ExecutionMirror mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("x", "中文字符");
        }
    }
}
