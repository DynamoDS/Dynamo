using System;
using System.IO;
using System.Xml;
using Dynamo.Migration;
using NUnit.Framework;

namespace Dynamo.Tests.Migrations
{
    [TestFixture]
    class WorkspaceMigrationsTests : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will create an instance of the WorkspaceMigrations and check the function Migrate_0_5_3_to_0_6_0
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestWorkspaceMigrations()
        {
            string consoleOutput;

            //Arrange
            string openPath = Path.Combine(TestDirectory, @"core\NodeStates.dyn");
            RunModel(openPath);
            string logFileText = string.Empty;
            XmlDocument xmlDoc = new XmlDocument();

            using (StringWriter stringWriter = new StringWriter())
            {
                TextWriter standardOutput = Console.Out;

                // Set the console out to a string object to verify the log message.
                Console.SetOut(stringWriter);

                //Act
                //Create a instance of WorkspaceMigration and call the function that writes to the log
                var workMigration = new WorkspaceMigrations(CurrentDynamoModel);
                workMigration.Migrate_0_5_3_to_0_6_0(xmlDoc);

                consoleOutput = stringWriter.ToString();

                // Set the console out back to std out. 
                Console.SetOut(standardOutput);
            }

            //Checks that the console output text contains the migration info
            Assert.IsTrue(consoleOutput.Contains("migration from 0.5.3.x to 0.6.1.x"));
        }
    }
}
