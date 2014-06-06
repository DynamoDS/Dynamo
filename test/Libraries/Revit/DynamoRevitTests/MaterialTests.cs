﻿using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitTestFramework;

namespace Dynamo.Tests
{
    [TestFixture]
    class MaterialTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\Material\GetMaterialByName.rfa")]
        public void GetMaterialByName()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Material\GetMaterialByName.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }
    }
}
