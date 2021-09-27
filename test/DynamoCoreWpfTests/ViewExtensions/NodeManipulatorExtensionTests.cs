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
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void GizmoWarningTest()
        {
            RaiseLoadedEvent(this.View);

            var pntNode = new DSFunction(Model.LibraryServices.GetFunctionDescriptor("Autodesk.DesignScript.Geometry.Point.ByCoordinates"));
            Model.ExecuteCommand(new DynamoModel.CreateNodeCommand(pntNode, 0, 0, true, false));

            pntNode.IsSelected = true;

            var dme = View.viewExtensionManager.ViewExtensions.OfType<DynamoManipulationExtension>().FirstOrDefault();

            pntNode.Warning("TestPersistentWarning", true);
            Assert.AreEqual(ElementState.PersistentWarning, pntNode.State);
            Assert.AreEqual("TestPersistentWarning", pntNode.ToolTipText);

            using (new MousePointManipulatorTest(pntNode, dme))
            {
                Assert.AreEqual(ElementState.Warning, pntNode.State);
                Assert.AreEqual("TestPersistentWarning" + "\n" + 
                    "Failed to create manipulator for Direct Manipulation of geometry: Exception of type 'System.Exception' was thrown.", 
                    pntNode.ToolTipText);
            }

            Assert.AreEqual(ElementState.PersistentWarning, pntNode.State);
            Assert.AreEqual("TestPersistentWarning", pntNode.ToolTipText);
        }
    }
}
