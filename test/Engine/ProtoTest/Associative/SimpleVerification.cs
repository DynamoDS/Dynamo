using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class SimpleVerification : ProtoTestBase
    {
        [Test]
        public void TestAssignment01()
        {

            String code =
@"a = 1;";
            thisTest.RunAndVerify(
                code, 
                TestFrameWork.BuildVerifyPair("a", 1)
                );
        }

        [Test]
        public void TestAssignment02()
        {
            string code =
@"a = 1;b = 2;";
            thisTest.RunAndVerify(
                code,
                TestFrameWork.BuildVerifyPair("a", 1),
                TestFrameWork.BuildVerifyPair("b", 2)
                );
        }


        [Test]
        public void TestFunctionCall01()
        {
            string code =
@"def f(){    return = 1;}x = f();";
            thisTest.RunAndVerify(
                code,
                TestFrameWork.BuildVerifyPair("x", 1)
                );
        }
        
        [Test]
        public void TestDouble01()
        {
            string code =
@"a = 1.0;";
            thisTest.RunAndVerify(
                code,
                TestFrameWork.BuildVerifyPair("a", 1.0)
                );
        }

        [Test]
        public void TestDouble02()
        {
            string code =
@"pi = 3.14;e = 2.71828;";
            thisTest.RunAndVerify(
                code,
                TestFrameWork.BuildVerifyPair("pi", 3.14),
                TestFrameWork.BuildVerifyPair("e", 2.71828)
                );
        }

        [Test]
        public void TestDouble03()
        {
            string code =
@"a = 1.1;b = 2.2;c = 3.3;";
            thisTest.RunAndVerify(
                code,
                TestFrameWork.BuildVerifyPair("a", 1.1),
                TestFrameWork.BuildVerifyPair("b", 2.2),
                TestFrameWork.BuildVerifyPair("c", 3.3)
                );
        }

        [Test]
        public void TestArrayAssignment01()
        {
            string code =
@"a = {1,2,3};";
            thisTest.RunAndVerify(
                code,
                TestFrameWork.BuildVerifyPair("a", new object[]{1,2,3})
                );
        }

        [Test]
        public void TestArrayAssignment02()
        {
            string code =
@"i = 2;a = {1, 2, 3 + i};";
            thisTest.RunAndVerify(
                code,
                TestFrameWork.BuildVerifyPair("i", 2),
                TestFrameWork.BuildVerifyPair("a", new object[] { 1, 2, 5 })
                );
        }

        [Test]
        public void TestNestedArrayAssignment01()
        {
            string code =
@"a = {1,2,{3,4}};";
            thisTest.RunAndVerify(
                code,
                TestFrameWork.BuildVerifyPair("a", new object[] { 1, 2, new object[]{ 3, 4 } } )
                );
        }

    }
}