using System.IO;
using NUnit.Framework;
using Dynamo.Applications;
using System.Reflection;

namespace IntegrationTests
{
    [TestFixture]
    class AssemblyConflictCheckerTest
    {
        AssemblyConflictChecker ConflictChecker;
        string testAssemblyConflictPath;

        [Test]
        public void ConflictOnAssemblyLoad()
        {
            testAssemblyConflictPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\test\assemblyConflict"));
            ConflictChecker = new AssemblyConflictChecker();

            var OldAssemPath = Path.Combine(testAssemblyConflictPath, "old", "Test.dll");
            Assembly.LoadFrom(OldAssemPath);

            var NewAssemPath = Path.Combine(testAssemblyConflictPath, "new", "Test.dll");
            Assembly.LoadFrom(NewAssemPath);

            Assert.AreEqual(ConflictChecker.Exceptions.Length, 1);

            ConflictChecker.Dispose();
        }
    }
}
