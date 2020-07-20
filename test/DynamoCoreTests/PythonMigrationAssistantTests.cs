using Dynamo.PythonMigration.MigrationAssistant;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dynamo.Tests
{
    public class PythonMigrationAssistantTests
    {
        [Test]
        public void CanMigratePyton2CodeToPython3Code()
        {
            // Arrange
            var expectedPython3Code = @"print(""Hello, Dynamo!"")";

            var python2Code = @"print ""Hello, Dynamo!""";

            // Act
            var migratedScript = ScriptMigrator.MigrateCode(python2Code);

            // Assert
            Assert.AreNotEqual(python2Code, migratedScript);
            Assert.AreEqual(expectedPython3Code, migratedScript);
        }

        [Test]
        public void MigrationWillNotChangePython3CompatibleCode()
        {
            // Arrange
            var expectedPython3Code = @"for x in range(1, 5): print(x)";

            // Act
            var migratedScript = ScriptMigrator.MigrateCode(expectedPython3Code);

            // Assert
            Assert.AreEqual(expectedPython3Code, migratedScript);
        }
    }
}
