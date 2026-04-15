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

            // Find a type with a non-null TypeId for testing
            var pointType = DSCore.Data.DataNodeDynamoTypeList.First(t => t.Type == typeof(Autodesk.DesignScript.Geometry.Point));
            Assert.IsNotNull(pointType.TypeId, "Point type should have a valid TypeId");
            
            // Set the node to Point type
            node.SelectedIndex = DSCore.Data.DataNodeDynamoTypeList.IndexOf(pointType);
            
            Assert.AreEqual(pointType.TypeId, node.ValueTypeId);
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

        [Test]
        [Category("UnitTests")]
        public void ValueTypeIdHandlesNullTypeId()
        {
            var node = new DefineData();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);

            // Default selection (bool) has null TypeId
            var firstType = DSCore.Data.DataNodeDynamoTypeList.First();
            Assert.IsNull(firstType.TypeId, "First type (bool) should have null TypeId");
            
            // Should fall back to SelectedString when TypeId is null
            Assert.AreEqual(node.SelectedString, node.ValueTypeId);
        }
    }
}
