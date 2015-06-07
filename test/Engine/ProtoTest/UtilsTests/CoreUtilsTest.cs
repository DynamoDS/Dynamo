using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;
namespace ProtoTest.UtilsTests
{
    [TestFixture]
    class CoreUtilsTest : ProtoTestBase
    {
        private bool Test_GetIdentifierStringUntilFirstParenthesis(string input, string expected)
        {
            List<AssociativeNode> astList = CoreUtils.BuildASTList(core, input);
            IdentifierListNode identList = (astList[0] as BinaryExpressionNode).RightNode as IdentifierListNode;

            // Verify expected string
            string identListString = CoreUtils.GetIdentifierStringUntilFirstParenthesis(identList);
            return identListString.Equals(expected); 
        }

        [Test]
        public void Test_GetIdentifierStringUntilFirstParenthesis_01()
        {
            // Given: A.B()
            //     Return: "A.B"
            string input = "p = A.B();";
            string expected = "A.B";
            Assert.IsTrue(Test_GetIdentifierStringUntilFirstParenthesis(input, expected));
        }

        [Test]
        public void Test_GetIdentifierStringUntilFirstParenthesis_02()
        {
            // Given: A.B.C()[0]
            //     Return: "A.B.C"
            string input = "p = A.B.C()[0];";
            string expected = "A.B.C";
            Assert.IsTrue(Test_GetIdentifierStringUntilFirstParenthesis(input, expected));
        }

        [Test]
        public void Test_GetIdentifierStringUntilFirstParenthesis_03()
        {
            // Given: A.B().C()
            //     Return: "A.B"
            string input = "p = A.B().C();";
            string expected = "A.B";
            Assert.IsTrue(Test_GetIdentifierStringUntilFirstParenthesis(input, expected));
        }

        [Test]
        public void Test_GetIdentifierStringUntilFirstParenthesis_04()
        {
            // Given: A.B[0].C
            //     Return: "A.B[0].C"
            string input = "p = A.B[0].C;";
            string expected = "A.B[0].C";
            Assert.IsTrue(Test_GetIdentifierStringUntilFirstParenthesis(input, expected));
        }

        private bool Test_GetIdentifierExceptMethodName(string input, string expected)
        {
            var astList = CoreUtils.BuildASTList(core, input);
            var identList = (astList[0] as BinaryExpressionNode).RightNode as IdentifierListNode;

            // Verify expected string
            string identListString = CoreUtils.GetIdentifierExceptMethodName(identList);
            return identListString.Equals(expected);
        }

        [Test]
        public void Test_GetIdentifierExceptMethodName_01()
        {
            // Given: A.B()
            //     Return: "A"
            string input = "p = A.B();";
            string expected = "A";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));
        }

        [Test]
        public void Test_GetIdentifierExceptMethodName_02()
        {
            // Given: A.B.C()[0]
            // Return: "A.B"
            string input = "p = A.B.C()[0];";
            string expected = "A.B";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));
        }

        [Test]
        public void Test_GetIdentifierExceptMethodName_03()
        {
            // Given: A.B().C
            // Return: "A"
            string input = "p = A.B().C;";
            string expected = "A";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));
        }

        [Test]
        public void Test_GetIdentifierExceptMethodName_04()
        {
            // Given: A.B[0].C
            // Return: "A.B[0].C"
            string input = "p = A.B[0].C;";
            string expected = "A.B[0].C";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));
        }

        [Test]
        public void Test_GetIdentifierExceptMethodName_05()
        {
            // Given: A().X (global function)
            // Return: empty string
            string input = "p = A().B;";
            string expected = "";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));

            input = "p = A().B[0].C;";
            expected = "";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));

            input = "p = A().B().C;";
            expected = "";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));

            input = "p = A().B.C();";
            expected = "";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));

            input = "p = A().B.C()[0];";
            expected = "";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));
        }

        [Test]
        public void Test_GetIdentifierExceptMethodName_06()
        {
            // Given: A.B[0].C()
            //     Return: "A.B[0]"
            string input = "p = A.B[0].C();";
            string expected = "A.B[0]";
            Assert.IsTrue(Test_GetIdentifierExceptMethodName(input, expected));
        }
    }
}
