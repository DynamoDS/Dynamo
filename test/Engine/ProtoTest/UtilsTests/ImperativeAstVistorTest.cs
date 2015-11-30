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
            return newIdent;
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

        [Test]
        public void TestReplaceIdentifierNode()
        {
            var result = ParserUtils.Parse(@"
[Imperative]
{
    y = foo + x;
}");
            var cbn = GetCodeBlockNode(result);
            var mappedCBN = cbn.Accept(replacer) as CodeBlockNode;
            Assert.IsNotNull(mappedCBN);
            Assert.IsTrue(mappedCBN.Body.Any());

            var expectedResult = ParserUtils.Parse(@"
[Imperative]
{
    y = bar + x;
}");
            var expectedCBN = GetCodeBlockNode(expectedResult);
            Assert.IsNotNull(expectedCBN);
            Assert.IsTrue(expectedCBN.Body.Any());

            Assert.IsTrue(expectedCBN.Equals(mappedCBN));
        }

    }
}
