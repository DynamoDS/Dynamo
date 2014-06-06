﻿using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using RevitTestFramework;

namespace Dynamo.Tests
{
    [TestFixture]
    class RayBounceTests:DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\RayBounce\RayBounce.rvt")]
        public void RayBounce()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\RayBounce\RayBounce.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

            //ensure that the bounce curve count is the same
            var curveColl = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document, DocumentManager.Instance.CurrentUIDocument.ActiveView.Id);
            curveColl.OfClass(typeof(CurveElement));
            Assert.AreEqual(curveColl.ToElements().Count(), 36);
        }
    }
}
