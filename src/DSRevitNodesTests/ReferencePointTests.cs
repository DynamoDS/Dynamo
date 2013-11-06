using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;
using Autodesk.LibG;
using Autodesk.Revit.DB;

namespace DSRevitNodesTests
{
    [TestFixture]
    class ReferencePointTests : DynamoRevitUnitTestBase
    {
        [Test]
        public void CanMakeSingleReferencePoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ReferencePoint\DSReferencePoint.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            // make some assertions here

        }
    }
}
