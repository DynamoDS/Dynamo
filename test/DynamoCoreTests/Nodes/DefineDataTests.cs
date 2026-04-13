using System.Collections.Generic;
using System.Linq;
using CoreNodeModels;
using NUnit.Framework;

namespace Dynamo.Tests.Nodes
{
    [TestFixture]
    internal class DefineDataTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void ValueSchemaProviderReturnsTypeId()
        {
            var node = new DefineData();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);

            var firstType = DSCore.Data.DataNodeDynamoTypeList.First();
            Assert.AreEqual(firstType.TypeId, node.ValueTypeId);
            Assert.IsFalse(node.IsListValue);
        }

        [Test]
        [Category("UnitTests")]
        public void ValueTypeIdSafeWhenNoSelection()
        {
            var node = new DefineData();
            Assert.DoesNotThrow(() => { var _ = node.ValueTypeId; });
            Assert.AreEqual(node.SelectedString, node.ValueTypeId);
        }
    }
}
