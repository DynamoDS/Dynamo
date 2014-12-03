using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoScript.Runners;
using ProtoTestFx.TD;

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
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);

            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);
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
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);

            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);
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
            returnType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
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
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == result1);


            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            Console.WriteLine(code);

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == result1);

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
            arg1Type.UID = (int)ProtoCore.PrimitiveType.kTypeInt;
            arg1Type.Name = ProtoCore.DSDefinitions.Keyword.Int;
            arg1Decl.ArgumentType = arg1Type;
            funcDefNode.Signature.AddArgument(arg1Decl);


            // Function Return type
            ProtoCore.Type returnType = new ProtoCore.Type();
            returnType.Initialize();
            returnType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
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
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == result1);


            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astList);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == result1);

        }

        [Test]
        public void TestRoundTrip_ClassDecl_PropertyAccess_01()
        {
            int result1 = 10;
            ExecutionMirror mirror = null;

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Create an exact copy of the AST list to pass to the source conversion
            // This needs to be done because the astlist to be run will be SSA'd on the AST execution run
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astListcopy= new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // 1. Build AST

            //  class bar
            //  {
            //       f : var;
            //  }
            //
            //  p = bar.bar();
            //  p.f = 10;
            //  a = p.f;


            // Create the class node AST
            ProtoCore.AST.AssociativeAST.ClassDeclNode classDefNode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classDefNode.className = "bar";

            // Create the property AST
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.Name = "f";
            varDeclNode.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");
            varDeclNode.ArgumentType = new ProtoCore.Type()
            {
                Name = "int",
                rank = 0,
                UID = (int)ProtoCore.PrimitiveType.kTypeInt
            };
            classDefNode.varlist.Add(varDeclNode);

            astList.Add(classDefNode);
            astListcopy.Add(new ProtoCore.AST.AssociativeAST.ClassDeclNode(classDefNode));


            // p = bar.bar();
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListConstrcctorCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListConstrcctorCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");
            identListConstrcctorCall.RightNode = constructorCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtInitClass = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                identListConstrcctorCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtInitClass);
            astListcopy.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(stmtInitClass));


            //  p.f = 10;
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListPropertySet = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListPropertySet.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListPropertySet.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertySet = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                identListPropertySet,
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertySet);
            astListcopy.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(stmtPropertySet));


            //  a = p.f; 
            ProtoCore.AST.AssociativeAST.IdentifierListNode identListPropertyAccess = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListPropertyAccess.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListPropertyAccess.RightNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertyAccess = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                identListPropertyAccess,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertyAccess);
            astListcopy.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(stmtPropertyAccess));



            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);

            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astListcopy);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);
        }

        [Test]
        public void TestRoundTrip_ClassDecl_MemFunctionCall_01()
        {
            int result1 = 20;
            ExecutionMirror mirror = null;

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Create an exact copy of the AST list to pass to the source conversion
            // This needs to be done because the astlist to be run will be SSA'd on the AST execution run
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astListcopy = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // 1. Build AST

            //  class bar
            //  {
            //       f : var
            //       def foo (b:int)
            //       {
            //           b = 10;
            //           return = b + 10;
            //       }
            //  }
            //
            //  p = bar.bar();
            //  a = p.foo();


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
            returnType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            returnType.Name = ProtoCore.DSDefinitions.Keyword.Var;
            funcDefNode.ReturnType = returnType;

            // Create the class node AST
            ProtoCore.AST.AssociativeAST.ClassDeclNode classDefNode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classDefNode.className = "bar";

            // Add the member function 'foo'
            classDefNode.funclist.Add(funcDefNode);


            // Create the property AST
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.Name = "f";
            varDeclNode.NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("f");
            varDeclNode.ArgumentType = new ProtoCore.Type()
            {
                Name = "int",
                rank = 0,
                UID = (int)ProtoCore.PrimitiveType.kTypeInt
            };
            classDefNode.varlist.Add(varDeclNode);


            // Add the constructed class AST
            astList.Add(classDefNode);
            astListcopy.Add(new ProtoCore.AST.AssociativeAST.ClassDeclNode(classDefNode));


            // p = bar.bar();
            ProtoCore.AST.AssociativeAST.FunctionCallNode constructorCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            constructorCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListConstrcctorCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListConstrcctorCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("bar");
            identListConstrcctorCall.RightNode = constructorCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtInitClass = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("p"),
                identListConstrcctorCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtInitClass);
            astListcopy.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(stmtInitClass));


            //  a = p.f; 
            ProtoCore.AST.AssociativeAST.FunctionCallNode functionCall = new ProtoCore.AST.AssociativeAST.FunctionCallNode();
            functionCall.Function = new ProtoCore.AST.AssociativeAST.IdentifierNode("foo");

            ProtoCore.AST.AssociativeAST.IdentifierListNode identListFunctionCall = new ProtoCore.AST.AssociativeAST.IdentifierListNode();
            identListFunctionCall.LeftNode = new ProtoCore.AST.AssociativeAST.IdentifierNode("p");
            identListFunctionCall.RightNode = functionCall;

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode stmtPropertyAccess = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                identListFunctionCall,
                ProtoCore.DSASM.Operator.assign);

            astList.Add(stmtPropertyAccess);
            astListcopy.Add(new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(stmtPropertyAccess));


            // 2. Execute AST and verify
            mirror = thisTest.RunASTSource(astList);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);


            // 3. Convert AST to source
            ProtoCore.CodeGenDS codegenDS = new ProtoCore.CodeGenDS(astListcopy);
            string code = codegenDS.GenerateCode();

            // 4. Execute source and verify
            mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == result1);
        }

        [Test]
        [Category("Failure")]
        public void TestAstToCode()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4393

            // Convert a list of code -> AST nodes -> code -> AST nodes
            // Compare two AST node lists are equal

            List<string> statements = new List<string>
            {
                "x = {};",
                @"x = {1, true, false, 1.234, 41, null, ""hello world"", 'x', foo(a<1>, b<2>)[3]}[0]<1>;",
                "x = (a..b..c)<1>..(m<1>..n<2>..~1)<2>..(i..j..#k<1>)<3>;",
                "x = (a > b ? foo(1, 2)<1> : bar(2, 3)<2>) ? ding(1, 2)<1> : dong(1, 2)<2>;", 
                "x = (a == b) ? ((a >= b) ? (a && b) : (a || b)) : (a < b);",
                "x = foo(a<1>, b<2>, c<3>)[0]<1>;",
                "x = Point.ByCoordinates({1,2,3}, {4,5,6}<1>, {7, 8, 9}<2>)<0>;",
                "x = this.foo(1, 2);",
                "x = this.X;",
                "return = {1, 2, 3};",
                "return = Math.PI;",
                "return = Math.Cos(1.2);", 
                "class Foo { f0; f1 = 1; f2 = 2 + 3; f3: int = 3 + 4; f4: int[] = 5; f5: int[]..[] = {1,2,3} ; x:var; y:int[][][]; z:double[]..[]; constructor Foo(p:int[]) { x = p; } def foo:var[](p:int[]..[]) { return = 0; }  static def bar() { return = null; }}",
                "x[0][1] = {};",
                
            };

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> nodes = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> new_nodes = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            List<string> new_statements = new List<string>();

            foreach (var stmt in statements)
            {
                var cbn = ProtoCore.Utils.ParserUtils.Parse(stmt) as ProtoCore.AST.AssociativeAST.CodeBlockNode;
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
                var cbn = ProtoCore.Utils.ParserUtils.Parse(stmt) as ProtoCore.AST.AssociativeAST.CodeBlockNode;
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