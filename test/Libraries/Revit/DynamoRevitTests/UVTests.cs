﻿using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitTestFramework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class UVTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void UVRandom()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\UV\UVRandom.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }
    }
}
