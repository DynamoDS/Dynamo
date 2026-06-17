using System.Collections.Generic;
using System.Linq;
using CoreNodeModels;
using Dynamo.Graph.Nodes;
using NUnit.Framework;

namespace Dynamo.Tests.Nodes
{
    [TestFixture]
    internal class DefineDataTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
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
            node.SelectedIndex = -1;
            Assert.DoesNotThrow(() => { var _ = node.ValueTypeId; });
            Assert.AreEqual(node.SelectedString, node.ValueTypeId);
        }

        [Test]
        [Category("UnitTests")]
        public void ValueTypeIdReturnsPrimitiveTypeId()
        {
            var node = new DefineData();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);

            var boolType = DSCore.Data.DataNodeDynamoTypeList.First(t => t.Type == typeof(bool));
            node.SelectedIndex = DSCore.Data.DataNodeDynamoTypeList.IndexOf(boolType);

            Assert.AreEqual("Bool", node.ValueTypeId);
        }

        [Test]
        [Category("UnitTests")]
        public void ValueTypeIdReturnsFloat64ForNumber()
        {
            var node = new DefineData();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);

            var numberType = DSCore.Data.DataNodeDynamoTypeList.First(t => t.Type == typeof(double));
            node.SelectedIndex = DSCore.Data.DataNodeDynamoTypeList.IndexOf(numberType);

            Assert.AreEqual("Float64", node.ValueTypeId);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenUnconnectedThenShowsMissingUpstreamConnectionInfo()
        {
            var node = new DefineData();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);

            Assert.AreEqual(ElementState.PersistentInfo, node.State);
            Assert.IsTrue(node.Infos.Any(x =>
                x.State == ElementState.PersistentInfo &&
                x.Message.Equals(CoreNodeModels.Properties.Resources.DefineDataMissingUpstreamConnectionInfoMessage)));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenConnectedThenClearsMissingUpstreamConnectionInfo()
        {
            var defineData = new DefineData();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(defineData, false);

            CurrentDynamoModel.NodeFactory.CreateNodeFromTypeName("CoreNodeModels.Input.DoubleInput", out NodeModel numberNode);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(numberNode, false);
            defineData.ConnectInput(0, 0, numberNode);

            Assert.IsFalse(defineData.Infos.Any(x => x.State == ElementState.PersistentInfo));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenPlayerValueIsSetThenDoesNotShowMissingUpstreamConnectionInfo()
        {
            var node = new DefineData { Value = "1.0" };
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);

            Assert.IsFalse(node.Infos.Any(x => x.State == ElementState.PersistentInfo));
        }
    }
}
