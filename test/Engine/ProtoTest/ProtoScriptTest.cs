using NUnit.Framework;
namespace ProtoTest
{
    [TestFixture]
    class ProtoScriptTest : ProtoTestBase
    {
        [Test]
        public void BasicInfrastructureTest()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            runtimeCore = fsr.Execute(
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                runtimeCore = fsr.Execute(
    @"[imperative{    a = 3}", core);
            });
        }
        [Test]
        public void ParserFailTest2()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                runtimeCore = fsr.Execute(
    @"[{    a = 3}", core);
            });
        }
        [Test]
        public void ParserFailTest3()
        {
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            //@TODO: What exception should this throw
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                runtimeCore = fsr.Execute(
    @"[associative]{	a = 1;	}", core);
            });
        }
    }
}
