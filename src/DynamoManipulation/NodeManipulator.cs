using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using DSCoreNodesUI.Input;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;
using ProtoCore.Mirror;

namespace Dynamo.Manipulation
{
    
    public abstract class NodeManipulatorFactory : INodeManipulatorFactory
    {
        /// <summary>
        /// Create a set of manipulators for the given node by inspecting its numeric inputs
        /// </summary>
        /// <param name="node"></param>
        /// <param name="manipulatorContext"></param>
        /// <returns></returns>
        public abstract INodeManipulator Create(NodeModel node, DynamoManipulationExtension manipulatorContext);
    }

    /// <summary>
    /// Base class of all Manipulator types
    /// Authors of Manipulators must derive from this class
    /// </summary>
    public abstract class NodeManipulator : INodeManipulator
    {
        #region properties

        private const double NewNodeOffsetX = 350;
        private const double NewNodeOffsetY = 50;
        private Point newPosition;

        protected NodeModel Node { get; set; }

        protected IWorkspaceModel WorkspaceModel { get; private set; }

        protected IWatch3DViewModel BackgroundPreviewViewModel { get; private set; }

        protected IRenderPackageFactory RenderPackageFactory { get; private set; }

        protected ICommandExecutive CommandExecutive { get; private set; }

        protected string UniqueId { get; private set; }

        protected string ExtensionName { get; private set; }
        
        protected bool Active { get; set; }

        protected IGizmo GizmoInAction { get; private set; }

        #endregion

        #region abstract methods

        /// <summary>
        /// Returns list of Gizmos used by this manipulator.
        /// </summary>
        /// <param name="createIfNone">Whether to create new gizmo if not already present.</param>
        /// <returns>List of Gizmos.</returns>
        protected abstract IEnumerable<IGizmo> GetGizmos(bool createIfNone);

        /// <summary>
        /// Implement to analyze inputs to the manipulator node and initialize them
        /// If inputs are numbers or sliders, they are candidates for being manipulator inputs
        /// Responsibility of deciding which inputs are "manipulatable" must lie with the implementor
        /// </summary>
        protected abstract void AssignInputNodes();

        /// <summary>
        /// Implement to update the position(s) of all manipulator inputs when the node is updated
        /// </summary>
        protected abstract void UpdatePosition();

        /// <summary>
        /// This method is called when a gizmo provided by derived class is hit.
        /// Derived class can create or update the input nodes.
        /// </summary>
        /// <param name="gizmo">The Gizmo that's hit.</param>
        /// <param name="hitObject">The object of Gizmo that's hit.</param>
        /// <returns>List of updated nodes</returns>
        protected abstract IEnumerable<NodeModel> OnGizmoClick(IGizmo gizmo, object hitObject);

        /// <summary>
        /// This method is called when the Gizmo in action is moved during mouse
        /// move event. Derived class can use this notification to update the 
        /// input nodes.
        /// </summary>
        /// <param name="gizmo">The Gizmo in action.</param>
        /// <param name="offset">Offset amount by which Gizmo has moved.</param>
        /// <returns>New expected position of the Gizmo</returns>
        protected abstract Point OnGizmoMoved(IGizmo gizmo, Vector offset);

        #endregion

        #region protected methods

        /// <summary>
        /// Implement to dispose any implementation specific manipulator handlers and/or properties
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            var gizmos = GetGizmos(false);
            foreach (var item in gizmos)
            {
                BackgroundPreviewViewModel.DeleteGeometryForIdentifier(item.Name);
            }
        }

        /// <summary>
        /// This method is called when mouse is moved, to check if the
        /// Gizmo in action can be moved successfully. If it returns true
        /// then this manipulator will compute the movement and call
        /// OnGizmoMoved.
        /// </summary>
        /// <param name="gizmo">Gizmo in action.</param>
        /// <returns>True if Gizmo is ready to move.</returns>
        protected virtual bool CanMoveGizmo(IGizmo gizmo)
        {
            //Wait until node has been evaluated and has got new origin
            //as expected position.
            return gizmo != null && newPosition.DistanceTo(gizmo.Origin) < 0.01;
        }

        /// <summary>
        /// Implements the MouseDown event handler for the manipulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        protected virtual void MouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            UpdatePosition();
            GizmoInAction = null; //Reset Drag.

            var gizmos = GetGizmos(false);
            if (!Active || null == gizmos || !gizmos.Any())
                return;

            var ray = BackgroundPreviewViewModel.GetClickRay(mouseButtonEventArgs);
            if (ray == null) return;

            object hitObject;
            foreach (var item in gizmos)
            {
                if (item.HitTest(ray.GetOriginPoint(), ray.GetDirectionVector(), out hitObject))
                {
                    GizmoInAction = item;
                    var nodes = OnGizmoClick(item, hitObject);
                    if(null != nodes && nodes.Any())
                    {
                        WorkspaceModel.RecordModelsForModification(nodes);
                    }
                    newPosition = GizmoInAction.Origin;
                    return;
                }
            }
        }

        /// <summary>
        /// Implements the MouseUp event handler for the manipulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void MouseUp(object sender, MouseButtonEventArgs e)
        {
            GizmoInAction = null;
            //Delete all transient axis line geometry
            BackgroundPreviewViewModel.DeleteGeometryForIdentifier(RenderDescriptions.AxisLine);
        }

        /// <summary>
        /// Implements the MouseMove event handler for the manipulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEventArgs"></param>
        protected virtual void MouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!CanMoveGizmo(GizmoInAction))
                return;

            var clickRay = BackgroundPreviewViewModel.GetClickRay(mouseEventArgs);
            if (clickRay == null) return;

            var offset = GizmoInAction.GetOffset(clickRay.GetOriginPoint(), clickRay.GetDirectionVector());
            if (offset.Length < 0.01)
                return;

            newPosition = OnGizmoMoved(GizmoInAction, offset);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node for which manipulator is created.</param>
        /// <param name="manipulatorContext">Context for manipulator</param>
        protected NodeManipulator(NodeModel node, DynamoManipulationExtension manipulatorContext)
        {
            Node = node;
            WorkspaceModel = manipulatorContext.WorkspaceModel;
            BackgroundPreviewViewModel = manipulatorContext.BackgroundPreviewViewModel;
            RenderPackageFactory = manipulatorContext.RenderPackageFactory;
            CommandExecutive = manipulatorContext.CommandExecutive;
            UniqueId = manipulatorContext.UniqueId;
            ExtensionName = manipulatorContext.Name;
                
            AttachBaseHandlers();

            DrawManipulator();
        }

        /// <summary>
        /// Helper method to get the first item from the objects resulted from 
        /// this node.
        /// </summary>
        /// <param name="node">Input Node</param>
        /// <returns>Returns the first item of the node.</returns>
        protected static object GetFirstValueFromNode(NodeModel node)
        {
            return GetElementsFromMirrorData(node.CachedValue).FirstOrDefault();
        }

        /// <summary>
        /// Helper method to get all items from mirror data as flat list recursively.
        /// </summary>
        /// <param name="data">Input data</param>
        /// <returns>List of objects</returns>
        protected static IEnumerable<object> GetElementsFromMirrorData(MirrorData data)
        {
            if (data == null || data.IsNull)
                yield return null;

            if (data.IsCollection)
            {
                var elems = data.GetElements();
                foreach (var item in elems)
                {
                    var objs = GetElementsFromMirrorData(item);
                    foreach (var obj in objs)
                    {
                        yield return obj;
                    }
                }
            }

            yield return data.Data;
        }

        /// <summary>
        /// Find connected node based on the input index and returns if this
        /// input port value can be manipulated.
        /// </summary>
        /// <param name="inputPortIndex">Input index of input port</param>
        /// <param name="inputNode">node connected to inputport</param>
        /// <returns>True if a manipulator can be used for the given input port.</returns>
        protected bool CanManipulateInputNode(int inputPortIndex, out NodeModel inputNode)
        {
            bool manipulate = false;
            inputNode = null;
            Tuple<int, NodeModel> val;
            if (Node.InputNodes.TryGetValue(inputPortIndex, out val))
            {
                if (val != null)
                {
                    inputNode = val.Item2;
                    if (val.Item2 is DSCoreNodesUI.Input.DoubleSlider)
                        manipulate = true;
                }
                else
                {
                    manipulate = true;
                }
            }
            else if (inputPortIndex < Node.InPorts.Count)
            {
                manipulate = true;
            }
            return manipulate;
        }

        /// <summary>
        /// Executes CreateAndConnectNodeCommand to create DoubleSlider and 
        /// connect it's output port to the input port of the node.
        /// </summary>
        /// <param name="outputPortIndex">index of output port of slider</param>
        /// <param name="inputPortIndex">index of input port of the node.</param>
        /// <returns>New slider node</returns>
        protected NodeModel CreateAndConnectInputNode(int outputPortIndex, int inputPortIndex)
        {
            var loc = ComputeInputNodeLocation(inputPortIndex);

            // Create number slider node and connect to Point node
            var inputNodeGuid = Guid.NewGuid();

            var command = new DynamoModel.CreateAndConnectNodeCommand(inputNodeGuid, Node.GUID,
                "DSCoreNodesUI.Input.DoubleSlider", outputPortIndex, inputPortIndex, loc.Item1, loc.Item2, false, false);

            CommandExecutive.ExecuteCommand(command, UniqueId, ExtensionName);

            var inputNode = WorkspaceModel.Nodes.FirstOrDefault(node => node.GUID == command.ModelGuid) as DoubleSlider;
            return inputNode;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Computes location of the slider node to be connected to given input
        /// port.
        /// </summary>
        /// <param name="inputPortIndex">Index of input port</param>
        /// <returns>X, Y values of node location</returns>
        private Tuple<double, double> ComputeInputNodeLocation(int inputPortIndex)
        {
            // node params (e.g. for slider node, specify range)
            double y;
            switch (inputPortIndex)
            {
                case 0:
                    y = Node.Y - NewNodeOffsetY;
                    break;
                case 1:
                    y = Node.Y;
                    break;
                default:
                    y = Node.Y + NewNodeOffsetY;
                    break;
            }

            // location (relative to Node)
            double x = Node.X - NewNodeOffsetX;

            return new Tuple<double, double>(x, y);
        }

        /// <summary>
        /// Method to draw manipulator
        /// </summary>
        private void DrawManipulator()
        {
            var packages = GenerateRenderPackages();

            BackgroundPreviewViewModel.AddGeometryForRenderPackages(packages);
        }

        /// <summary>
        /// Subscribes various event handlers.
        /// </summary>
        private void AttachBaseHandlers()
        {
            BackgroundPreviewViewModel.ViewMouseMove += MouseMove;
            BackgroundPreviewViewModel.ViewMouseDown += MouseDown;
            BackgroundPreviewViewModel.ViewMouseUp += MouseUp;

            Node.RequestRenderPackages += GenerateRenderPackages;
        }

        /// <summary>
        /// Generates render packages for all the drawables of this manipulator.
        /// Before actually generating the render packages it ensures that this
        /// object is properly updated. This method is called when the output 
        /// of the node is rendered.
        /// </summary>
        /// <returns>List of render packages</returns>
        private IEnumerable<IRenderPackage> GenerateRenderPackages()
        {
            // Currently collections are not being supported
            //if(Node.CachedValue.IsCollection) return new List<IRenderPackage>();

            var packages = new List<IRenderPackage>();

            AssignInputNodes();

            if (!Node.IsSelected)
            {
                return packages;
            }

            UpdatePosition();

            if (!Active)
            {
                return packages;
            }

            return BuildRenderPackage();
        }

        /// <summary>
        /// Unsubscribes all the event handlers registered by this manipulator.
        /// </summary>
        private void DetachHandlers()
        {
            BackgroundPreviewViewModel.ViewMouseMove -= MouseMove;
            BackgroundPreviewViewModel.ViewMouseDown -= MouseDown;
            BackgroundPreviewViewModel.ViewMouseUp -= MouseUp;

            Node.RequestRenderPackages -= GenerateRenderPackages;
        }

        #endregion

        #region Interface methods

        public void Dispose()
        {
            Dispose(true);

            DetachHandlers();
        }

        /// <summary>
        /// Builds render packages as required for rendering this manipulator.
        /// </summary>
        /// <returns>List of render packages</returns>
        public IEnumerable<IRenderPackage> BuildRenderPackage()
        {
            var packages = new List<IRenderPackage>();
            var gizmos = GetGizmos(true);
            foreach (var item in gizmos)
            {
                // Append node AST identifier to gizmo name
                // so that it gets added to package description
                item.Name = string.Format("{0}_{1}", item.Name, Node.AstIdentifierBase);
                packages.AddRange(item.GetDrawables(RenderPackageFactory));
            }

            return packages;
        }
        #endregion
    }

    public class CompositeManipulator : INodeManipulator
    {
        private readonly List<INodeManipulator> subManipulators;
        public CompositeManipulator(IEnumerable<INodeManipulator> manipulators)
        {
            subManipulators = manipulators.ToList();
        }

        #region Interface methods

        public void Dispose()
        {
            subManipulators.ForEach(x => x.Dispose());
        }

        public IEnumerable<IRenderPackage> BuildRenderPackage()
        {
            var packages = new List<IRenderPackage>();
            foreach (var subManipulator in subManipulators)
            {
                packages.AddRange(subManipulator.BuildRenderPackage());
            }
            return packages;
        }

        #endregion
    }
}
