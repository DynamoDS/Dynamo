using System;
using Dynamo.PythonMigration.MigrationAssistant;
using NUnit.Framework;

namespace DynamoPythonTests
{
    [TestFixture]
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

        [Test]
        public void MigrationWillNormalizeWhiteSpaceIfCodeContainsTabs()
        {
            var original = "import sys" + Environment.NewLine +
                "class MyClass:" + Environment.NewLine +
                "  def __init__(self):" + Environment.NewLine +
                "\t  pass" + Environment.NewLine +
                "MyClass()" + Environment.NewLine;
            var expected = "import sys" + Environment.NewLine +
                "class MyClass:" + Environment.NewLine +
                "    def __init__(self):" + Environment.NewLine +
                "        pass" + Environment.NewLine +
                "MyClass()" + Environment.NewLine;
            var actual = ScriptMigrator.MigrateCode(original);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MigrationWontChangeWhiteSpaceIfCodeDoesNotContainTabs()
        {
            var original = "import sys" + Environment.NewLine +
                "class MyClass:" + Environment.NewLine +
                "  def __init__(self):" + Environment.NewLine +
                "    pass" + Environment.NewLine +
                "MyClass()" + Environment.NewLine;
            var actual = ScriptMigrator.MigrateCode(original);
            Assert.AreEqual(original, actual);
        }

        [Test]
        public void CanMigrateDotNoneToPython3GetAttr()
        {
            var original = @"
import sys
import clr
clr.AddReference('System.Windows.Forms')
from System.Windows.Forms import *
groupBox1 = GroupBox();
textBox1 = TextBox();
groupBox1.Controls.Add(textBox1);
groupBox1.Text = ""MyGroupBox"";
groupBox1.Dock = DockStyle.None;
OUT = groupBox1
";
            var expected = @"
import sys
import clr
clr.AddReference('System.Windows.Forms')
from System.Windows.Forms import *
groupBox1 = GroupBox();
textBox1 = TextBox();
groupBox1.Controls.Add(textBox1);
groupBox1.Text = ""MyGroupBox"";
groupBox1.Dock = getattr(DockStyle, 'None');
OUT = groupBox1
";
            var actual = ScriptMigrator.MigrateCode(original);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanMigrateFixAddReferenceToFileAndPathToPython3AddReference()
        {
            var original = @"
import sys
import clr
clr.AddReferenceToFileAndPath('System.Windows.Forms')
";
            var expected = @"
import sys
import clr
clr.AddReference('System.Windows.Forms')
";
            var actual = ScriptMigrator.MigrateCode(original);
            Assert.AreEqual(expected, actual);
        }

    }
}
