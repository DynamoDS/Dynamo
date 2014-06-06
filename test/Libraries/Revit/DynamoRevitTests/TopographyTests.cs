﻿using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitTestFramework;

namespace Dynamo.Tests
{
    [TestFixture]
    class TopographyTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rvt")]
        public void TopographyFromPoints()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Topography\TopographyFromPoints.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\Topography\topography.rvt")]
        public void PointsFromTopography()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Topography\PointsFromTopography.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }
    }
}
