using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Manipulation;
using Dynamo.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamoCoreWpfTests.ViewExtensions
{
    public class MousePointManipulatorTest : MousePointManipulator
    {
        internal MousePointManipulatorTest(DSFunction node, DynamoManipulationExtension manipulatorContext)
            : base(node, manipulatorContext)
        {
        }
        protected override IEnumerable<IGizmo> GetGizmos(bool createOrUpdate)
        {
            //Don't create a new gizmo if not requested
            if (createOrUpdate)
            {
                throw new Exception();
            }
            return base.GetGizmos(createOrUpdate);
        }
    }
    public class NodeManipulatorExtensionTests : DynamoTestUIBase
    {
        [SetUp]
        public virtual void Start()
        {
            // Forcing the dispatcher to execute all of its tasks within these tests causes crashes in Helix.
            // For now just skip it.
            SkipDispatcherFlush = true;
            base.Start();
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [TestOnSeparateThread]
        public void GizmoWarningTest()
        {
            RaiseLoadedEvent(this.View);

            var pntNode = new DSFunction(Model.LibraryServices.GetFunctionDescriptor("Autodesk.DesignScript.Geometry.Point.ByCoordinates"));
            Model.ExecuteCommand(new DynamoModel.CreateNodeCommand(pntNode, 0, 0, true, false));

            pntNode.IsSelected = true;

            var dme = View.viewExtensionManager.ViewExtensions.OfType<DynamoManipulationExtension>().FirstOrDefault();

            pntNode.Warning("TestPersistentWarning", true);
            Assert.AreEqual(ElementState.PersistentWarning, pntNode.State);
            Assert.IsTrue(pntNode.Infos.Any(x => x.Message.Equals("TestPersistentWarning") && x.State == ElementState.PersistentWarning));

            using (new MousePointManipulatorTest(pntNode, dme))
            {
                Assert.AreEqual(ElementState.Warning, pntNode.State);
                Assert.AreEqual(2, pntNode.Infos.Count);
                Assert.IsTrue(pntNode.Infos.Any(x => x.Message.Equals("TestPersistentWarning") && x.State == ElementState.PersistentWarning));
                Assert.IsTrue(pntNode.Infos.Any(x => x.Message.Equals(
                    "Failed to create manipulator for Direct Manipulation of geometry: Exception of type 'System.Exception' was thrown.") &&
                    x.State == ElementState.Warning));
            }
            Assert.AreEqual(ElementState.PersistentWarning, pntNode.State);
            Assert.IsTrue(pntNode.Infos.Any(x => x.Message.Equals("TestPersistentWarning") && x.State == ElementState.PersistentWarning));
        }

        [TestOnSeparateThread]
        public void NodeManipulatorUnselectedNodeTest()
        {
            RaiseLoadedEvent(this.View);

            var pntNode = new DSFunction(Model.LibraryServices.GetFunctionDescriptor("Autodesk.DesignScript.Geometry.Point.ByCoordinates"));
            Model.ExecuteCommand(new DynamoModel.CreateNodeCommand(pntNode, 0, 0, true, false));

            pntNode.IsSelected = false;

            var dme = View.viewExtensionManager.ViewExtensions.OfType<DynamoManipulationExtension>().FirstOrDefault();
            var manipulator = new MousePointManipulator(pntNode, dme);
            Assert.IsNotNull(manipulator);

            var manipulatorType = typeof(MousePointManipulator);
            var method = manipulatorType.GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(method);

            method.Invoke(manipulator, new object[] { });
        }
    }
}
