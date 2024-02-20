using CoreNodeModels.Input;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Media3D;
//using Point = Autodesk.DesignScript.Geometry.Point;
//using Vector = Autodesk.DesignScript.Geometry.Vector;
using Point = Autodesk.GeometryPrimitives.Dynamo.Geometry.Point;
using Vector = Autodesk.GeometryPrimitives.Dynamo.Math.Vector3d;
using Line = Autodesk.GeometryPrimitives.Dynamo.Geometry.Line;

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
        private readonly DynamoManipulationExtension manipulatorContext;
        private const double NewNodeOffsetX = 350;
        private const double NewNodeOffsetY = 50;
        private bool active;
        private string warning = string.Empty;
        private Point originBeforeMove;// The manipulator position before the user moves the gizmo
        private Point originAfterMove;// The manipulator position after the user moves the gizmo
        protected const double gizmoScale = 1.2;
        protected readonly int ROUND_UP_PARAM = 3;
        protected readonly double MIN_OFFSET_VAL = 0.001;

        #region properties

        protected IWorkspaceModel WorkspaceModel
        {
            get { return manipulatorContext.WorkspaceModel; }
        }

        protected ICommandExecutive CommandExecutive
        {
            get { return manipulatorContext.CommandExecutive; }
        }

        protected string UniqueId
        {
            get { return manipulatorContext.UniqueId; }
        }

        protected string ExtensionName
        {
            get { return manipulatorContext.Name; }
        }

        protected IGizmo GizmoInAction { get; private set; }

        internal NodeModel Node { get; private set; }

        /// <summary>
        /// Base position of geometry being manipulated by manipulator
        /// Derived classes should update this value in UpdatePosition and OnGizmoMoved
        /// </summary>
        internal abstract Point Origin { get; }

        internal IWatch3DViewModel BackgroundPreviewViewModel
        {
            get { return manipulatorContext.BackgroundPreviewViewModel; }
        }

        internal IRenderPackageFactory RenderPackageFactory
        {
            get { return manipulatorContext.RenderPackageFactory; }
        }

        internal Point3D? CameraPosition { get; private set; }
        
        // This is null for each new manipulator that is initialized.
        private bool? isValidNode;

        /// <summary>
        /// This property is true if the cached value of the manipulator node is non-null.
        /// It caches its value so that it doesn't have to repeatedly query the node value.
        /// NOTE: There could be edge cases where the node value could become null while the
        /// manipulator is being moved, etc. These cases will not be caught and the boolean
        /// value returned would be true even though it should change to false. This case could
        /// lead to a potential crash and is yet to be addressed.
        /// </summary>
        private bool IsValidNode
        {
            get 
            {
                if (isValidNode == null)
                {
                    isValidNode = !IsNodeNull(Node.CachedValue);
                }

                return (bool) isValidNode;
            }
           
        }

        #endregion

        #region abstract methods

        /// <summary>
        /// Returns list of Gizmos used by this manipulator.
        /// </summary>
        /// <param name="createOrUpdate">
        /// Create new gizmos or update existing ones gizmos if parameter is true
        /// otherwise simply query for existing gizmos if false.</param>
        /// <returns>List of Gizmos.</returns>
        protected abstract IEnumerable<IGizmo> GetGizmos(bool createOrUpdate);

        /// <summary>
        /// Implement to analyze inputs to the manipulator node and initialize them
        /// If inputs are numbers or sliders, they are candidates for being manipulator inputs
        /// Responsibility of deciding which inputs are "manipulatable" must lie with the implementor
        /// </summary>
        protected abstract void AssignInputNodes();

        /// <summary>
        /// Implement to update the position(s) of the manipulator including Origin
        /// when the node is updated
        /// </summary>
        /// <returns>return true if position is updated successfully else return false</returns>
        protected abstract bool UpdatePosition();

        /// <summary>
        /// This method is called when a gizmo provided by derived class is hit.
        /// Derived class can create or update the input nodes.
        /// </summary>
        /// <param name="gizmoInAction">The Gizmo that's hit.</param>
        /// <param name="hitObject">The object of Gizmo that's hit.</param>
        /// <returns>List of updated nodes</returns>
        protected abstract IEnumerable<NodeModel> OnGizmoClick(IGizmo gizmoInAction, object hitObject);

        /// <summary>
        /// This method is called when the Gizmo in action is moved during mouse
        /// move event. Derived class must use this notification to update the 
        /// input nodes AND update the manipulator Origin based on the mouse move.
        /// </summary>
        /// <param name="gizmoInAction">The Gizmo in action.</param>
        /// <param name="offset">Offset amount by which Gizmo has moved.</param>
        protected abstract void OnGizmoMoved(IGizmo gizmoInAction, Vector offset);

        #endregion

        #region protected methods

        /// <summary>
        /// Implement to dispose any implementation specific manipulator handlers and/or properties
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            //if (Origin != null) Origin.Dispose();
        }

        /// <summary>
        /// Implements the MouseDown event handler for the manipulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        protected virtual void MouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (!IsValidNode) return;

            active = UpdatePosition();
            if (Origin != null )
            {
                originBeforeMove = new Point(Origin.Position.X, Origin.Position.Y, Origin.Position.Z);
                originAfterMove = new Point(Origin.Position.X, Origin.Position.Y, Origin.Position.Z);
            }

            GizmoInAction = null; //Reset Drag.

            var gizmos = GetGizmos(false);
            if (!IsEnabled() || null == gizmos || !gizmos.Any()) return;

            var ray = BackgroundPreviewViewModel.GetClickRay(mouseButtonEventArgs);
            if (ray == null) return;

            foreach (var item in gizmos)
            {
                var originPt = ray.GetOriginPoint();
                var dirVec = ray.GetDirectionVector();
                if (item.HitTest(originPt, dirVec, out var hitObject))
                {
                    GizmoInAction = item;

                    var nodes = OnGizmoClick(item, hitObject).ToList();
                    if (nodes.Any())
                    {
                        WorkspaceModel.RecordModelsForModification(nodes);
                    }
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

            if (originBeforeMove != null && originAfterMove != null)
            {
                var inputNodesToManipulate = InputNodesToUpdateAfterMove(
                    originAfterMove.Position - originBeforeMove.Position);
                foreach (var (inputNode, amount) in inputNodesToManipulate)
                {
                    if (inputNode == null) continue;

                    if (Math.Abs(amount) < MIN_OFFSET_VAL) continue;

                    dynamic uiNode = inputNode;
                    uiNode.Value = Math.Round(amount, ROUND_UP_PARAM);
                }
            }

            //Update gizmo graphics after every camera view change
            var gizmos = GetGizmos(false);
            foreach (var gizmo in gizmos)
            {
                gizmo.UpdateGizmoGraphics();
            }
        }

        /// <summary>
        /// Implements the MouseMove event handler for the manipulator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseEventArgs"></param>
        protected virtual void MouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!IsEnabled()) return;

            var clickRay = BackgroundPreviewViewModel.GetClickRay(mouseEventArgs);
            if (clickRay == null) return;

            if (GizmoInAction == null)
            {
                HighlightGizmoOnRollOver(clickRay);
                return;
            }

            var offset = GizmoInAction.GetOffset(clickRay.GetOriginPoint(), clickRay.GetDirectionVector());
            if (offset.Magnitude < 0.01) return;

            if (originAfterMove != null)
            {
                var offsetPos = new Point(originAfterMove.Position + offset);
                //originAfterMove.Dispose();
                originAfterMove = offsetPos;
            }

            // Update input nodes attached to manipulator node 
            // Doing this triggers a graph update on scheduler thread
            OnGizmoMoved(GizmoInAction, offset);

            // redraw manipulator at new position synchronously
            var packages = BuildRenderPackage();
            BackgroundPreviewViewModel.AddGeometryForRenderPackages(packages);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">Node for which manipulator is created.</param>
        /// <param name="manipulatorContext">Context for manipulator</param>
        protected NodeManipulator(NodeModel node, DynamoManipulationExtension manipulatorContext)
        {
            this.manipulatorContext = manipulatorContext;
            Node = node;
            
            AttachBaseHandlers();

            CameraPosition = BackgroundPreviewViewModel.GetCameraPosition();

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
                    if (val.Item2 is DoubleSlider) manipulate = true;
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
        /// Retrieves a list of InputNodes that need to be updated after the manipulator is moved. This method is called when MouseUp is triggered.
        /// </summary>
        /// <param name="offset">The offset vector with which the manipulator was moved by the user. This param is calculated as the vector between (Origin at MouseDown) and (Origin at MouseUp)</param>
        /// <returns>A list of InputNodes and the new values that needs to be set to the corresponding input nodes</returns>
        protected virtual List<(NodeModel inputNode, double amount)> InputNodesToUpdateAfterMove(Vector offset)
        {
            return new List<(NodeModel, double)>();
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
                "CoreNodeModels.Input.DoubleSlider", outputPortIndex, inputPortIndex, loc.Item1, loc.Item2, false, false);

            CommandExecutive.ExecuteCommand(command, UniqueId, ExtensionName);

            var inputNode = WorkspaceModel.Nodes.FirstOrDefault(node => node.GUID == command.ModelGuid) as DoubleSlider;

            if (inputNode != null)
            {
                // Assign the input slider to the default value of the node's input port
                var doubleNode = Node.InPorts[inputPortIndex].DefaultValue as DoubleNode;
                if (doubleNode != null) inputNode.Value = doubleNode.Value;
            }
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
        /// Method to draw manipulator. 
        /// It's always called on the UI thread
        /// </summary>
        private void DrawManipulator()
        {
            Debug.Assert(IsMainThread());

            var packages = GenerateRenderPackages();

            // Add manipulator geometry to view asynchronously 
            // since it has to be queued up in UI dispatcher after drawing node geometry
            BackgroundPreviewViewModel.AddGeometryForRenderPackages(packages, true);
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
        /// This function can only be called on the UI thread if there's a node selection OR
        /// called on the scheduler thread if there's a node update
        /// </summary>
        /// <returns>List of render packages</returns>
        private RenderPackageCache GenerateRenderPackages()
        {
            var packages = new RenderPackageCache();

            // This can happen only when the node is updating along with the manipulator
            // and meanwhile it is deselected in the UI before the manipulator is killed
            if (!Node.IsSelected)
            {
                return packages;
            }

            // This check is required if for some reason LibG fails to load, geometry nodes are null
            // and we must return immediately before proceeding with further calls to ProtoGeometry.

            if (IsNodeNull(Node.CachedValue) || Node.CachedValue.IsFunction) return packages;

            AssignInputNodes();
            active = UpdatePosition();
            if (!IsEnabled())
            {
                return packages;
            }
            // Blocking call to build render packages only in UI thread
            // to avoid race condition with gizmo members b/w scheduler and UI threads.
            // Race condition can occur if say one gizmo is moving due to another gizmo
            // and it is highlighted when it comes near the mouse pointer.
            RenderPackageCache result = null;
            BackgroundPreviewViewModel.Invoke(() => result = BuildRenderPackage());
            return result;
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

        /// <summary>
        /// Removes Gizmos from background preview.
        /// </summary>
        private void DeleteGizmos()
        {
            var gizmos = GetGizmos(false);
            foreach (var item in gizmos)
            {
                // Unsubscribe gizmos from camera-change events before updating the scene
                item.Dispose();
                BackgroundPreviewViewModel.DeleteGeometryForIdentifier(item.Name);
            }
        }

        /// <summary>
        /// Highlights/Unhighlights Gizmo drawables on mouse roll-over
        /// </summary>
        /// <param name="clickRay"></param>
        private void HighlightGizmoOnRollOver(IRay clickRay)
        {
            Debug.Assert((IsMainThread()));

            var gizmos = GetGizmos(false);
            foreach (var item in gizmos)
            {
                item.UnhighlightGizmo();

                var originPt = clickRay.GetOriginPoint();
                var dirVec = clickRay.GetDirectionVector();
                if (item.HitTest(originPt, dirVec, out _))
                {
                    item.HighlightGizmo();
                    return;
                }
            }
        }

        internal static bool IsMainThread()
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA &&
                !Thread.CurrentThread.IsThreadPoolThread && Thread.CurrentThread.IsAlive)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Interface methods

        public void Dispose()
        {
            Dispose(true);

            // We only show the manipulator warning while the manipulator is alive.
            // When the owner Node is deselected and the manipulator is disposed,
            // we cleanup the manipulator warning from the owner Node.
            // See PR 7623 for more details
            if (!string.IsNullOrEmpty(warning))
            {
                Node.ClearTransientWarning(warning);
            }

            //if (originBeforeMove != null)
            //    originBeforeMove.Dispose();
     
            //if (originAfterMove != null)
            //    originAfterMove.Dispose();

            DeleteGizmos();
            DetachHandlers();
        }

        /// <summary>
        /// Builds render packages as required for rendering this manipulator.
        /// </summary>
        /// <returns>List of render packages</returns>
        public RenderPackageCache BuildRenderPackage()
        {
            Debug.Assert(IsMainThread());
            warning = string.Empty;
            var packages = new RenderPackageCache();
            try
            {
                var gizmos = GetGizmos(true);
                foreach (var item in gizmos)
                {
                    packages.Add(item.GetDrawables());
                }
            }
            catch (Exception e)
            {
                warning = Properties.Resources.DirectManipulationError + ": " + e.Message;
                Node.Warning(warning);
            }
            return packages;
        }

        #endregion

        /// <summary>
        /// Checks if manipulator is enabled or not. Manipulator is enabled 
        /// only if node is not frozen, its preview visible and value non-null. 
        /// </summary>
        /// <returns>True if enabled and can be manipulated.</returns>
        public bool IsEnabled()
        {
            if (Node.IsFrozen || !Node.IsVisible) return false;

            if (!IsValidNode)
            {
                return false;
            }

            return active;
        }

        private static bool IsNodeNull(MirrorData data)
        {
            if (data == null || data.IsNull) return true;
            
            if (data.IsCollection)
            {
                var elements = data.GetElements();
                foreach (var element in elements)
                {
                    if (IsNodeNull(element)) return true;
                }
            }

            return false;
        }
    }


    internal static class PointExtensions
    {
        public static Point ToPoint(this Point3D point)
        {
            return new Point(point.X, point.Y, point.Z);
        }

        public static Vector ToVector(this Vector3D vec)
        {
            return new Vector(vec.X, vec.Y, vec.Z);
        }
    }

    internal static class RayExtensions
    {
        private const double axisScaleFactor = 100;
        private const double rayScaleFactor = 10000;

        public static Line ToLine(this IRay ray)
        {
            var origin = ray.Origin.ToPoint();
            var direction = ray.Direction.ToVector();
            direction.Scale(rayScaleFactor);
            return new Line(origin.Position, direction);
        }

        public static Line ToOriginCenteredLine(this IRay ray)
        {
            var origin = ray.Origin.ToPoint();
            var direction = ray.Direction.ToVector();
            return ToOriginCenteredLine(origin, direction);
        }

        public static Line ToOriginCenteredLine(Point origin, Vector axis)
        {
            var vec1 = new Vector(axis.X, axis.Y, axis.Z);
            vec1.Scale(-axisScaleFactor);

            var vec2 = new Vector(axis.X, axis.Y, axis.Z);
            vec2.Scale(axisScaleFactor);

            var startPos = origin.Position + vec1;
            var endPos = origin.Position + vec2;
            var dir = endPos - startPos;

            return new Line(startPos, dir);
        }

        public static Point GetOriginPoint(this IRay ray)
        {
            return ray.Origin.ToPoint();
        }

        public static Vector GetDirectionVector(this IRay ray)
        {
            return ray.Direction.ToVector();
        }
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

        public RenderPackageCache BuildRenderPackage()
        {
            var packages = new RenderPackageCache();
            foreach (var subManipulator in subManipulators)
            {
                packages.Add(subManipulator.BuildRenderPackage());
            }

            return packages;
        }

        #endregion
    }
}
