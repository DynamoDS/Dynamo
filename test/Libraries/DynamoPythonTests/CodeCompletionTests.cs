using Dynamo;
using Dynamo.Python;
using Dynamo.PythonServices;
using NUnit.Framework;

namespace DynamoPythonTests
{
    [TestFixture]
    internal class CodeCompletionTests : UnitTestBase
    {
        [Test]
        public void SharedCoreCanFindFirstLoadedIfNotMatch()
        {
            var provider = new SharedCompletionProvider(PythonEngineVersion.CPython3, "");
            Assert.IsNotNull(provider);
        }
    }
}
