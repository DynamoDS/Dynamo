using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Dynamo.Graph.Connectors;

namespace Dynamo.Tests
{
    class MultiOutputNodeTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestSingleOutputNode()
        {
            RunModel(@"core\multiout\singleoutput.dyn");
            AssertPreviewValue("060e57e1-b889-4b94-a440-8adb0067ae79", null);

            var instanceNode = CurrentDynamoModel.CurrentWorkspace.Nodes.First(n => n.GUID.ToString().Equals("306c8777-ecff-4106-bfdd-dd331e61cf2b"));
            var dictNode = CurrentDynamoModel.CurrentWorkspace.Nodes.First(n => n.GUID.ToString().Equals("41271769-11b8-46fc-bb16-fffd022dc9bc"));
            var connector = ConnectorModel.Make(instanceNode, dictNode, 0, 0);

            RunCurrentModel();
            AssertPreviewValue("060e57e1-b889-4b94-a440-8adb0067ae79", "green");
        }
    }
}
