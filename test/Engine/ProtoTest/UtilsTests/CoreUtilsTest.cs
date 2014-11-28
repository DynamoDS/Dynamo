using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;
using ProtoFFI;
using ProtoTestFx.TD;
namespace ProtoTest.UtilsTests
{
    [TestFixture]
    class CoreUtilsTest : ProtoTestBase
    {
        private bool Test_GetIdentifierStringUntilFirstParenthesis(string input, string expected)
        {
            List<AssociativeNode> astList = ProtoCore.Utils.CoreUtils.BuildASTList(core, input);
            IdentifierListNode identList = (astList[0] as BinaryExpressionNode).RightNode as IdentifierListNode;

            // Verify expected string
            string identListString = ProtoCore.Utils.CoreUtils.GetIdentifierStringUntilFirstParenthesis(identList);
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
    }
}
