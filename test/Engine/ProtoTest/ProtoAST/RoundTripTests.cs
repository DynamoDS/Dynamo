using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;

namespace ProtoTest.ProtoAST
{
    class RoundTripTests : ProtoTestBase
    {
        [Test]
        public void TestRoundTrip_Assign01()
        {
            //=================================
            // 1. Build AST 
            // 2. Execute AST and verify
            // 3. Convert AST to source
            // 4. Execute source and verify
            //=================================

            int result1 = 10;
            ExecutionMirror mirror = null;

            // 1. Build AST 
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", result1);

            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", result1);
        }

        [Test]
        public void TestRoundTrip_Assign02()
        {
            //=================================
            // 1. Build AST 
            // 2. Execute AST and verify
            // 3. Convert AST to source
            // 4. Execute source and verify
            //=================================

            int result1 = 30;
            ExecutionMirror mirror = null;

            // 1. Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IntNode(10),
                    new ProtoCore.AST.AssociativeAST.IntNode(20),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("a", result1);

            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", result1);
        }


        [Test]
        public void TestRoundTrip_FunctionDefAndCall_01()
        {

            //=================================
            // 1. Build AST 
            // 2. Execute AST and verify
            // 3. Convert AST to source
            // 4. Execute source and verify
            //=================================
            int result1 = 20;
            ExecutionMirror mirror = null;



            // 1. Build the AST tree

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


            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("x", result1);


            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            Console.WriteLine(code);

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", result1);

        }

        [Test]
        public void TestRoundTrip_FunctionDefAndCall_02()
        {
            //=================================
            // 1. Build AST 
            // 2. Execute AST and verify
            // 3. Convert AST to source
            // 4. Execute source and verify
            //=================================
            int result1 = 11;
            ExecutionMirror mirror = null;

            // 1. Build the AST tree


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



            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            thisTest.Verify("x", result1);


            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", result1);

        }

        [Test]
        [Category("DSDefinedClass_Ported"), Category("Failure")]
        public void TestAstToCode()
        {
            // TODO pratapa: Regression post Dictionary changes for Get.ValueAtIndex method

            // Convert a list of code -> AST nodes -> code -> AST nodes
            // Compare two AST node lists are equal

            List<string> statements = new List<string>
            {
                "x = [];",
                @"x = [1, true, false, 1.234, 41, null, ""hello world"", 'x', foo(a<1>, b<2>)[3]][0]<1>;",
                "x = (a..b..c)<1>..(m<1>..n<2>..~1)<2>..(i..j..#k<1>)<3>;",
                "x = (a > b ? foo(1, 2)<1> : bar(2, 3)<2>) ? ding(1, 2)<1> : dong(1, 2)<2>;", 
                "x = (a == b) ? ((a >= b) ? (a && b) : (a || b)) : (a < b);",
                "x = foo(a<1>, b<2>, c<3>)[0]<1>;",
                "x = Point.ByCoordinates([1,2,3], [4,5,6]<1>, [7, 8, 9]<2>)<0>;",
                "x = this.foo(1, 2);",
                "x = this.X;",
                "return = [1, 2, 3];",
                "return = Math.PI;",
                "return = Math.Cos(1.2);", 
                "x[0][1] = [];",
                
            };

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> nodes = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> new_nodes = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            List<string> new_statements = new List<string>();

            foreach (var stmt in statements)
            {
                var cbn = ProtoCore.Utils.ParserUtils.Parse(stmt);
                if (cbn != null)
                {
                    foreach (var item in cbn.Body)
                    {
                        nodes.Add(item);

                        string code = item.ToString();
                        new_statements.Add(code);
                        Console.WriteLine(code);
                    }
                }
            }

            foreach (var stmt in new_statements)
            {
                var cbn = ProtoCore.Utils.ParserUtils.Parse(stmt);
                if (cbn != null)
                {
                    foreach (var item in cbn.Body)
                    {
                        new_nodes.Add(item);
                    }
                }
            }

            Assert.AreEqual(nodes.Count, new_nodes.Count);
            int count = nodes.Count;

            for (int i = 0; i < count; ++i)
            {
                Console.WriteLine("Compare: ");
                Console.WriteLine(nodes[i].ToString());
                Console.WriteLine(new_nodes[i].ToString());
                Assert.IsTrue(nodes[i].Equals(new_nodes[i]));
            }
        }
    }
}