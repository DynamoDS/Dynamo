using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FamilyTests:DynamoRevitUnitTestBase
    {
        [Test]
        public void GetFamilyInstancesByType()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstancesByType.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            var node =
                (GetFamilyInstancesByType)dynSettings.Controller.DynamoModel.Nodes.First(x => x is GetFamilyInstancesByType);
            Assert.IsTrue(node.OldValue.IsList);

            var list = ((FScheme.Value.List)node.OldValue).Item;
            Assert.AreEqual(100, list.Count());
        }

        [Test]
        public void GetFamilyInstanceLocation()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CanLocateAdaptiveComponent()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CanLocateAdaptiveComponentInDividedSurface()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
    }
}
