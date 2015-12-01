using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoCore.AST.ImperativeAST;
using ProtoCore.SyntaxAnalysis;
using NUnit.Framework;
using ProtoCore.Utils;

namespace ProtoTest.UtilsTests
{
    class TestImperativeAstReplacer: ImperativeAstReplacer 
    {
        private Func<string, string> mapper;

        public TestImperativeAstReplacer(Func<string, string> mapper) 
        {
            this.mapper = mapper;
        }

        public override ImperativeNode VisitIdentifierNode(IdentifierNode node)
        {
            string variable = node.Value;
            string newVariable = mapper(variable);

            var newIdent = new IdentifierNode(node);
            newIdent.Value = newVariable;

            return base.VisitIdentifierNode(newIdent);
        }

        public override ImperativeNode VisitIdentifierListNode(IdentifierListNode node)
        {
            node.LeftNode = node.LeftNode.Accept(this);
            return node;
        }
    }

    [TestFixture]
    class ImperativeAstVistorTest
    {
        private TestImperativeAstReplacer replacer;
        private Dictionary<string, string> nameMap;

        public ImperativeAstVistorTest()
        {
            nameMap = new Dictionary<string, string>()
            {
                {"foo", "bar" },
                {"a", "a1" },
                {"b", "b1" },
                {"c", "c1" }
            };

            replacer = new TestImperativeAstReplacer((variable) =>
            {
                return nameMap.ContainsKey(variable) ? nameMap[variable] : variable;
            });
        }

        private CodeBlockNode GetCodeBlockNode(ProtoCore.AST.AssociativeAST.CodeBlockNode codeBlockNode)
        {
            Assert.IsTrue(codeBlockNode.Body.Any());

            var languageBlockNode = codeBlockNode.Body[0] as ProtoCore.AST.AssociativeAST.LanguageBlockNode;
            Assert.IsNotNull(languageBlockNode);

            var imperativeCodeBlockNode = languageBlockNode.CodeBlockNode as CodeBlockNode;
            Assert.IsNotNull(imperativeCodeBlockNode);

            return imperativeCodeBlockNode;
        }

        private void TestMapping(string originalCode, string expectedCode)
        {
            var originalResult = ParserUtils.Parse(originalCode);
            var cbn = GetCodeBlockNode(originalResult);
            var mappedCBN = cbn.Accept(replacer) as CodeBlockNode;
            Assert.IsNotNull(mappedCBN);
            Assert.IsTrue(mappedCBN.Body.Any());

            var expectedResult = ParserUtils.Parse(expectedCode);
            var expectedCBN = GetCodeBlockNode(expectedResult);
            Assert.IsNotNull(expectedCBN);
            Assert.IsTrue(expectedCBN.Body.Any());

            Assert.IsTrue(expectedCBN.Equals(mappedCBN), mappedCBN.ToString());
        }

        [Test]
        public void TestBinaryExpression()
        {
            TestMapping(
@"
[Imperative]
{
    y = foo + x;
}",

@"
[Imperative]
{
    y = bar + x;
}");
        }

        [Test]
        public void TestArrayIndexing()
        {
            TestMapping(@"
[Imperative]
{
    r = foo[a][b][c][d];
}",
@"
[Imperative]
{
    r = bar[a1][b1][c1][d];
}");
        }

        [Test]
        public void TestExpressionList()
        {
            TestMapping(@"
[Imperative]
{
    foo = {a, b, c, d, e};
}",
@"
[Imperative]
{
    bar = {a1, b1, c1, d, e};
}");
        }

        [Test]
        public void TestFunctionParameter()
        {
            TestMapping(@"
[Imperative]
{
    t = foo(a, b, c, d);
}",
@"
[Imperative]
{
    t = bar(a1, b1, c1, d);
}");
        }

        [Test]
        public void TestInlineCondition()
        {
            TestMapping(@"
[Imperative]
{
    foo = a ? b : c + d;
}",
@"
[Imperative]
{
    bar = a1 ? b1 : c1 + d;
}");
        }

        [Test]
        public void TestRangeExpression()
        {
            TestMapping(@"
[Imperative]
{
    foo = a..b..#c;
}",
@"
[Imperative]
{
    bar = a1..b1..#c1;
}");
        }

        [Test]
        public void TestIfElse()
        {
            TestMapping(@"
[Imperative]
{
    if (a)
    {
        foo = 10;
    }
    else if (b)
    {
        foo = 20;
    }
    else if (c)
    {
        foo = 30;
    }
}",
@"
[Imperative]
{
    if (a1)
    {
        bar = 10;
    }
    else if (b1)
    {
        bar = 20;
    }
    else if (c1)
    {
        bar = 30;
    }
}");
        }

        [Test]
        public void TestForLoop()
        {
            TestMapping(@"
[Imperative]
{
    for (a in (0..100))
    {
        foo = foo + b;
    }
}",
@"
[Imperative]
{
    for (a1 in (0..100))
    {
        bar = bar + b1;
    }
}");
        }

        [Test]
        public void TestWhileLoop()
        {
            TestMapping(@"
[Imperative]
{
    a = 1;
    while (a < 100) {
        foo = foo + a;
    }
}",
@"
[Imperative]
{
    a1 = 1;
    while (a1 < 100) {
        bar = bar + a1;
    }
}");
        }

        [Test]
        public void TestUnaryExpression()
        {
            TestMapping(
@"
[Imperative]
{
    foo = -a;
}
",
@"
[Imperative]
{
    bar = -a1;
}");
        }

        [Test]
        public void TestTypedIdentifier()
        {
            TestMapping(
@"
[Imperative]
{
    a : int = b;
}",
@"
[Imperative]
{
    a1: int = b1;
}");
        }

        [Test]
        public void TestGroupExpression()
        {
            TestMapping(
@"
[Imperative]
{
    foo = (a + b(c)) * d;
}",
@"
[Imperative]
{
    bar = (a1 + b1(c1)) * d;
}");
        }

        [Test]
        public void TestPropertyAccessing()
        {
            TestMapping(
@"
[Imperative]
{
    foo = Autodesk.foo();
    t = foo.foo;
}",
@"
[Imperative]
{
    bar = Autodesk.foo();
    t = bar.foo;
}");
        }
    }
}
