﻿using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitTestFramework;

namespace Dynamo.Tests
{
    [TestFixture]
    class ModelTextTests:DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\ModelText\ModelText.rfa")]
        public void ModelText()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ModelText\ModelText.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }
    }
}
