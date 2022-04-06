using System;
using NUnit.Framework;
namespace ProtoTest.Associative
{
    class VerificationErrorFormat : ProtoTestBase
    {
        [Test]
        public void TestSyntaxError01()
        {
            String code =
            @"
// No semicolon
a = 10 
";
            thisTest.RunAndVerifySyntaxError(code);
        }

        [Test]
        public void TestCompileWarning01()
        {
            const string code = @"
a = f();
";
            thisTest.RunAndVerifyBuildWarning(
                code, 
                ProtoCore.BuildData.WarningID.FunctionNotFound
                );
        }

        [Test]
        public void TestRuntimeWarning01()
        {
            string code =

@"
a = 2 % 0;
b = 2.1 % 0;
";

            thisTest.RunAndVerifyRuntimeWarning(code, ProtoCore.Runtime.WarningID.ModuloByZero);
        }

        [Test]
        public void TestStaticCyclicDependency01()
        {
            string code = @"
a = 1;
b = a;
a = b;";
            thisTest.RunAndVerifyBuildWarning(code, ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency);
        }
    }
}