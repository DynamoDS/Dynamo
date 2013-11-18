using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class BugTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void MAGN_66()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Bugs\MAGN_66.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void MAGN_102()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Bugs\MAGN_102_projectPointsToFace_selfContained.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void MAGN_122()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Bugs\MAGN_122_wallsAndFloorsAndLevels.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
    }
}
