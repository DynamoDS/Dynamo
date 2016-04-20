using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dynamo.Graph.Connectors;
using System.IO;
using Dynamo.Graph.Nodes.ZeroTouch;

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
        public void TestKeepReferenceThisAttribute()
        {
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\referenceThis.dyn");
            OpenModel(dynFilePath);
            var node1 = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Graph.Nodes.CustomNodes.Function>().FirstOrDefault();
            var node2 = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSFunction>().FirstOrDefault();
            ConnectorModel.Make(node1, node2, 0, 0);
            RunCurrentModel();
            AssertPreviewValue("763c4e98-dbe0-4bb7-b00f-69e5b79b09b0", new object[] { false, false });
        }
    }
}
