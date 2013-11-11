using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using DSRevitNodes;
using Dynamo;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;
using Autodesk.LibG;
using Autodesk.Revit.DB;

namespace DSRevitNodesTests
{
    [TestFixture]
    internal class ReferencePointTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void CanMakeSingleReferencePoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ReferencePoint\DSReferencePoint.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            var node = model.CurrentWorkspace.Nodes.First(x => x.Name == "DSReferencePoint.ByCoordinates");

            Assert.NotNull(node);

            var ptObj = ((FScheme.Value.Container) node.OldValue).Item;

            Assert.IsAssignableFrom(typeof (DSRevitNodes.DSReferencePoint), ptObj);

            var pt = (DSReferencePoint) ptObj;

            Assert.AreEqual(5, pt.X);
            Assert.AreEqual(6, pt.Y);
            Assert.AreEqual(7, pt.Z);

        }
    }


}
