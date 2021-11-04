using Dynamo;
using Dynamo.Python;
using NUnit.Framework;
using PythonNodeModels;

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
