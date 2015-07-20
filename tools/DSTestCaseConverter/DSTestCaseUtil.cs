using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Roslyn.Compilers;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DSTestCaseUtils
{
    class TestCaseUtils
    {
        public static string AssemblyFilePath
        {
            get
            {
                return @"C:\Users\qing\Documents\github\Dynamo\bin\AnyCPU\Debug\ProtoTest.dll";
            }
        }


        //TODO: delete
        public static string ProjPath
        {
            get
            {
                return @"C:\Users\qing\Documents\github\Dynamo\test\Engine\ProtoTest\ProtoTest.csproj";
            }
        }

        public static string SolutionPath
        {
            get
            {
                // Roslyn not working with VS2013 solution 
                return @"C:\Users\qing\Documents\github\Dynamo\src\Dynamo.All.2012.sln";
            }
        }

        internal static CommonSyntaxTree FileTree { get; set; }
        public static string TestmapAdd { get { return "testmap.Add"; } }

        public static CompilationUnitSyntax FileRoot { get; private set; }
        //public static CompilationUnitSyntax NewRoot { get; private set; }
        public static IProject ProtoTestProj { get; private set; }

        public static bool IsTestCase(MethodInfo method)
        {
            foreach (var attribute in method.GetCustomAttributes(true))
            {
                if (attribute.ToString() == "NUnit.Framework.TestAttribute")
                {
                    return true;
                }
            }
            return false;
        }



        private static bool SearchAndReplaceMethodsForTextCSharp(IDocument document, string textToSearch)
        {
            SyntaxNode syntaxNode = null;
            var syntaxNodes = from methodDeclaration in FileRoot.DescendantNodes()
                                  .Where(x => x is MethodDeclarationSyntax)
                              select methodDeclaration;
            if (syntaxNodes != null && syntaxNodes.Count() > 0)
            {
                foreach (MethodDeclarationSyntax ele in syntaxNodes)
                {
                    string bodyText = RemoveComments((ele).GetText().ToString());
                    if (Regex.IsMatch(bodyText, "\\b" + textToSearch + "\\b"))
                    {

                        syntaxNode = ele;
                        break;
                    }
                    else
                        continue;
                }
            }

            if (syntaxNode != null)
            {
                MemberDeclarationSyntax method = syntaxNode as MemberDeclarationSyntax;
                if (method != null)
                {
                    ReplaceMethodTextCSharp(method, document, textToSearch);
                    //update node
                    return true;
                }

            }
            //File.WriteAllText(@"G:\test.cs", FileRoot.ToString());

            return false;
        }

        private static SyntaxNode RemoveComments(SyntaxNode ele)
        {
            if (ele.HasLeadingTrivia)
            {
                var oldCommentsTrivias = from trivia in ele.GetLeadingTrivia().Where(
                    x => (x.Kind == SyntaxKind.MultiLineCommentTrivia || x.Kind == SyntaxKind.SingleLineCommentTrivia))
                                         select trivia;
                foreach (var comment in oldCommentsTrivias)
                {
                    ele = ele.ReplaceTrivia(comment, SyntaxTriviaList.Empty);                    

                }
            }
            if (ele.DescendantTrivia() != null && ele.DescendantTrivia().Count() > 0)
            {
                var containingComments = from trivia in ele.DescendantTrivia().Where(
                    x => (x.Kind == SyntaxKind.MultiLineCommentTrivia || x.Kind == SyntaxKind.SingleLineCommentTrivia))
                                         select trivia;
                foreach (var comment in containingComments)
                {
                    ele = ele.ReplaceTrivia(comment, SyntaxTriviaList.Empty);
                }
            }

            return ele;
        }

        private static void ReplaceMethodTextCSharp(SyntaxNode node, IDocument document, string textToSearch)
        {
            string methodText = node.GetText().ToString();
            bool isMethod = node is MethodDeclarationSyntax;
            string methodOrPropertyDefinition = isMethod ? "Method: " : " Invalid - not Method ";

            object methodName = ((MethodDeclarationSyntax)node).Identifier.Value;
            var body = (node as MethodDeclarationSyntax).Body;
            var statements = body.Statements;
            var originalTrailingTrivia = body.GetTrailingTrivia();
            SyntaxList<StatementSyntax> newStatements = new SyntaxList<StatementSyntax>();
            StatementSyntax statementNewTestmap = Syntax.ParseStatement("var testmap = new Dictionary<string, object>();");
            newStatements = newStatements.Add(statementNewTestmap);
            SyntaxList<StatementSyntax> flattenedStmts = Flatten(statements);
            foreach (var statement in flattenedStmts)
            {
                //string stmtString = RemoveComments(statement).ToString();

                string stmtString = statement.ToString();
                stmtString = RemoveComments(stmtString);

                if (Regex.IsMatch(stmtString, "\\b" + textToSearch + "\\b"))
                {
                    StatementSyntax newStatement = null;
                    //string[] parameters = stmtString.Split(new char[] { '(', ',', ')' });
                    //if(parameters.Length == 4)
                    //{
                    //    if(parameters.Last() == "1")
                    //    {
                    //        Console.WriteLine(stmtString);
                    //    }
                    //}
                    //stmtString = TestmapAdd + "(" + parameters.ElementAt(1) + ", " + parameters.ElementAt(2) + ");" ;
                    var invocation = Syntax.ParseExpression(RemoveComments(stmtString)) as InvocationExpressionSyntax;
                    var args = invocation.ArgumentList.Arguments;
                    //var args = GetParams(stmtString);
                    if (args.Count() > 3)
                    {
                        throw new Exception();
                    }
                    if (args.Count() == 3)
                    {
#if DEBUG
                        if (args.Last().ToString() != "0")
                            Console.WriteLine(stmtString);
#endif
                        // Console.WriteLine(stmtString);
                        //stmtString = TestmapAdd + "(" + args.ElementAt(0) + ", " + args.ElementAt(1) + ");";
                        try
                        {
                            var registerArgs = new List<ArgumentSyntax>
                            {
                                args.ElementAt(0),
                                    args.ElementAt(1)
                            };
                            var argSeparators = Enumerable.Repeat(Syntax.Token(SyntaxKind.CommaToken), registerArgs.Count - 1).ToList();
                            ArgumentListSyntax newArgList = Syntax.ArgumentList(
                                arguments: Syntax.SeparatedList(
                                    registerArgs,
                                    argSeparators));

                            InvocationExpressionSyntax newInvokationNode = Syntax.InvocationExpression(
                                Syntax.MemberAccessExpression(
                                    kind: SyntaxKind.MemberAccessExpression,
                                    expression: Syntax.ParseName("testmap"),
                                    name: Syntax.IdentifierName("Add"),
                                    operatorToken: Syntax.Token(SyntaxKind.DotToken)),
                                newArgList);
                            stmtString = newInvokationNode.ToString() + ";";
                            newStatement = Syntax.ParseStatement(stmtString);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e);
                        }


                    }
                    if(args.Count() == 2)
                    {
                        stmtString = stmtString.Replace(textToSearch, TestmapAdd);
                    }
                    Console.WriteLine(stmtString);
                    newStatement = Syntax.ParseStatement(stmtString);
                    try
                    {
                        newStatements = newStatements.Add(newStatement);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    //stmtString.Replace(textToSearch, "thisTest.GenericVerify");
                }
                else
                    newStatements = newStatements.Add(statement);
            }
            newStatements = newStatements.Add(Syntax.ParseStatement("thisTest.GenericVerify(testmap);"));
            FileRoot = FileRoot.ReplaceNode(body,
                Syntax.Block(newStatements).WithTrailingTrivia(originalTrailingTrivia)).NormalizeWhitespace();

        }

        private static SyntaxList<StatementSyntax> Flatten(SyntaxList<StatementSyntax> statements)
        {
            SyntaxList<StatementSyntax> result = new SyntaxList<StatementSyntax>();
            foreach(var stmt in statements)
            {
                if (!(stmt is BlockSyntax))
                    result = result.Add(stmt);
                else
                    result = Flatten(stmt,  result);
            }
            return result;
        }

        private static SyntaxList<StatementSyntax> Flatten(StatementSyntax stmt,  SyntaxList<StatementSyntax> result)
        {
            if (!(stmt is BlockSyntax))
                result = result.Add(stmt);
            else
            {
                foreach(var item in (stmt as BlockSyntax).Statements)
                {
                    result = Flatten(item, result);
                }
            }
            return result;
        }

        private static List<string> GetParams(string stmtString)
        {
            string extractFuncRegex = @"\b[^()]+\((.*)\)$";
            string extractArgsRegex = @"([^,]+\(.+?\))|([^,]+)";

            //Your test string
            

            var match = Regex.Match(stmtString, extractFuncRegex);
            string innerArgs = match.Groups[1].Value;
            var matches = Regex.Matches(innerArgs, extractArgsRegex);
            List<string> parameters = new List<string>(); ;
            for (int i = 0; i < matches.Count; i++)
                parameters.Add(matches[i].Value);
            return parameters;
            
        }

        private static string RemoveComments(string stmtString)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";
            string noComments = Regex.Replace(stmtString,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);

            return noComments;
        }

        internal static void ReplaceBasic()
        {
            IProject proj = ProtoTestProj;
            foreach (var doc in proj.Documents)
            {
                CommonSyntaxTree syntax = doc.GetSyntaxTree();
                FileTree = syntax;
                FileRoot = (CompilationUnitSyntax)syntax.GetRoot();

                bool containingMethodToReplace = true;
                while (containingMethodToReplace)
                {
                    containingMethodToReplace = SearchAndReplaceMethodsForTextCSharp(doc, "thisTest.Verify");
                    FileTree = SyntaxTree.Create(FileRoot);
                    FileRoot = (CompilationUnitSyntax)FileTree.GetRoot();
                }
                var UsingStmtForDict = Syntax.QualifiedName(Syntax.IdentifierName("System.Collections"),
                    Syntax.IdentifierName("Generic"));
                FileRoot = FileRoot.AddUsings(Syntax.UsingDirective(UsingStmtForDict).NormalizeWhitespace()).NormalizeWhitespace();

                File.WriteAllText(doc.FilePath, FileRoot.ToString());

                //Console.WriteLine(result);
            }

        }

        internal static void InitProcess()
        {
            ProtoTestProj = GetProject();
        }

        private static IProject GetProject()
        {
            try
            {
                var w = Workspace.LoadSolution(SolutionPath, "Debug", "AnyCPU", true);
                //var w = Workspace.LoadStandAloneProject(ProjPath);
                return w.CurrentSolution.Projects.Where(proj => proj.Name == "ProtoTest").First();

                //var solution = Solution.Load(SolutionPath);
                //return solution.Projects.Where(proj => proj.Name == "ProtoTest").First();


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
