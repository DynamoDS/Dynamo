using NUnit.Framework;
namespace ProtoTest
{
    [TestFixture]
    class ProtoScriptTest : ProtoTestBase
    {
        [Test]
        public void BasicInfrastructureTest()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            fsr.Execute(
@"[Imperative]{	x = 987654321;	[Associative]	{		 px = 1234321;	}}", core);
        }
    }

    [TestFixture]
    class MultiLangNegitiveTests : ProtoTestBase
    {
        //Negitive Tests with distortions of the Language def block
        [Test]
        public void ParserFailTest1()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                fsr.Execute(
    @"[imperative{    a = 3}", core);
            });
        }
        [Test]
        public void ParserFailTest2()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                fsr.Execute(
    @"[{    a = 3}", core);
            });
        }
        [Test]
        public void ParserFailTest3()
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                fsr.Execute(
    @"[associative]{	a = 1;	}", core);
            });
        }
    }
}
