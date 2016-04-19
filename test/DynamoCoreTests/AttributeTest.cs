using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dynamo.Graph.Connectors;
using System.IO;

namespace Dynamo.Tests
{
    class AttributeTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestAttributeKeepReferenceThis()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\referenceThis.dyn");
            OpenModel(dynFilePath);
            AssertPreviewValue("763c4e98-dbe0-4bb7-b00f-69e5b79b09b0", new object[] { false, false });
        }
    }
}
