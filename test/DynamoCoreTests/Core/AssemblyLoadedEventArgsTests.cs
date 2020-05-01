using System.Reflection;
using NUnit.Framework;
using static Dynamo.Models.NodeModelAssemblyLoader;

namespace Dynamo.Tests.Core
{
    [TestFixture]
    public class AssemblyLoadedEventArgsTests
    {
        [Test]
        [Category("UnitTests")]
        public void AssemblyLoadedEventArgsConstructorTests()
        {
            var assemblyMock = Assembly.GetExecutingAssembly();
            AssemblyLoadedEventArgs eventArgs = new AssemblyLoadedEventArgs(assemblyMock);

            Assert.AreEqual(assemblyMock, eventArgs.Assembly);
        }
    }
}