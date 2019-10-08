using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes.ZeroTouch;
using NUnit.Framework;

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
            // Test KeepReferenceThisAttribute could avoid a C# object from
            // being disposed even it is out function scope. 
            //
            // The .dyn file uses ReferenceThis from FFITarget.dll. Note the
            // return value of GetItems() has reference to "this" object:
            //
            // class ReferenceThis {
            //  ....
            //  public IEnumerable<ReferenceThisItem> GetItems() {
            //      return new [] { new ReferenceThisItem(this), ... };
            //  }
            // }
            //
            // So if we call GetItems() in a function, refThis object will be
            // disposed and returned object has reference to disposed
            // ReferenceThis.
            //
            // def foo() {
            //   refThis = ReferenceThis();
            //   return = refThis.GetItems();
            // }      
            //
            // But if applying KeepReferenceThisAttribute, refThis will be
            // referenced in the returned object and won't be disposed. This
            // test case is to verify ReferenceThis.Disposed is false when 
            // returns from a function.
            var dynFilePath = Path.Combine(TestDirectory, @"core\dsevaluation\referenceThis.dyn");
            OpenModel(dynFilePath);
            var node1 = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Graph.Nodes.CustomNodes.Function>().FirstOrDefault();
            var node2 = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSFunction>().FirstOrDefault();
            // Not connect at the beginning to ensure GC() is invoked.
            ConnectorModel.Make(node1, node2, 0, 0);
            RunCurrentModel();
            AssertPreviewValue("763c4e98-dbe0-4bb7-b00f-69e5b79b09b0", new object[] { false, false });
        }
    }
}
