using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Dynamo.Models;
using Dynamo.Tests;

using NUnit.Framework;

namespace Dynamo.Nodes
{
    [TestFixture]
    public class HigherOrder : DSEvaluationUnitTest
    {
        string TestFolder { get { return GetTestDirectory(); } }

        [Test]
        public void ComposeOrder()
        {
            string testFilePath = Path.Combine(TestFolder, "core", "compose", "compose_order.dyn");
            RunModel(testFilePath);

            AssertPreviewValue("1ac93e79-9793-44d4-a6cc-c898d47e7ba8", 75.0);
        }
    }
}
