using DynamoPython;
using NUnit.Framework;

namespace DynamoPythonTests
{
    [TestFixture]
    internal class CodeCompletionTests
    {

        [Test]
        public void CanGetSystemType()
        {
            var provider = new PythonConsoleCompletionDataProvider();
            var completions = provider.GenerateCompletionData("System.Collections");
            Assert.AreEqual(29, completions.Length);
        }
    }
}
