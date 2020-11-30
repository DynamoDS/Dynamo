using System.Reflection;
using NUnit.Framework;
using static Dynamo.Models.NodeModelAssemblyLoader;
using Dynamo.Core;

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

    [TestFixture]
    public class AssemblyConflictNotifierTest : DynamoModelTestBase
    {
        public override void Setup()
        {
            // set conflict notifier before we initialize the dynamo model (before we start loading any assemblies).
            AssemblyConflictNotifier.SkipChecks = false;

            base.Setup();
        }

        [Test]
        public void TestAssemblyConflictNotifications()
        {

        }
    }
}