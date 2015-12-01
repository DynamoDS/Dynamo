using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Nodes;

namespace Dynamo.Manipulation
{
    public sealed class PointOnCurveManipulatorFactory : INodeManipulatorFactory
    {
        public INodeManipulator Create(NodeModel node, DynamoManipulationExtension manipulatorContext)
        {
            return new PointOnCurveManipulator(node as DSFunction, manipulatorContext);
        }
    }

    class PointOnCurveManipulator : NodeManipulator
    {
        private const double NewNodeOffsetX = 350;
        private const double NewNodeOffsetY = 50;

        private Point pointOnCurve;

        private Curve curve;

        private Vector tangent;

        private NodeModel inputNode;

        private TranslationGizmo gizmo;

        internal PointOnCurveManipulator(DSFunction node, DynamoManipulationExtension manipulatorContext)
            : base(node, manipulatorContext)
        {
        }

        #region abstract method implementation

        /// <summary>
        /// Returns all the gizmos supported by this manipulator
        /// </summary>
        /// <param name="createOrUpdate">
        /// If true: Create a new gizmo or update a gizmo if already present.
        /// If false: Query for existing gizmos</param>
        /// <returns></returns>
        protected override IEnumerable<IGizmo> GetGizmos(bool createOrUpdate)
        {
            if (gizmo == null && !createOrUpdate)
                yield break;

            if (createOrUpdate)
            {
                UpdateGizmo();
            }

            yield return gizmo;
        }

        private void UpdateGizmo()
        {
            if (null == gizmo)
                gizmo = new TranslationGizmo(pointOnCurve, tangent, 6);
            else
                gizmo.UpdateGeometry(pointOnCurve, tangent, null, null, 6);
        }

        protected override void AssignInputNodes()
        {
            curve = null;
            NodeModel curveNode;
            CanManipulateInputNode(0, out curveNode);
            if (null == curveNode)
                return; //Not enough input to manipulate

            if (!CanManipulateInputNode(1, out inputNode))
                return;

            try
            {
                curve = GetFirstValueFromNode(curveNode) as Curve;
                if (null == curve)
                    return;
            }
            catch (Exception ex) 
            { 
                return; 
            }
        }

        protected override void UpdatePosition()
        {
            Active = false;
            if (curve == null) //Curve is not initialized, can't be manipulated now.
                return;

            if (pointOnCurve == null)
                pointOnCurve = curve.StartPoint;

            //Node output could be a collection, consider the first item as origin.
            Point pt = GetFirstValueFromNode(Node) as Point;
            if (pt == null) return; //The node output is not Point, could be a function object.

            var param = curve.ParameterAtPoint(pt);
            tangent = curve.TangentAtParameter(param);
            
            //Don't cache pt directly here, need to create a copy, because 
            //pt may be GC'ed by VM.
            pointOnCurve = Point.ByCoordinates(pt.X, pt.Y, pt.Z); 
        
            Active = tangent != null;
        }

        protected override IEnumerable<NodeModel> OnGizmoClick(IGizmo gizmo, object hitObject)
        {
            var axis = hitObject as Vector;
            if (null == axis) return null;
            
            if(null == inputNode)
            {
                inputNode = CreateAndConnectInputNode(0, 1);
            }

            return new[] { inputNode };
        }

        protected override Point OnGizmoMoved(IGizmo gizmo, Vector offset)
        {
            var newPosition = pointOnCurve.Add(offset);
            newPosition = curve.ClosestPointTo(newPosition);
            var param = curve.ParameterAtPoint(newPosition);
            param = Math.Round(param, 3);
            newPosition = curve.PointAtParameter(param);
            if (inputNode != null)
            {
                dynamic uinode = inputNode;
                uinode.Value = param;
            }

            return newPosition;
        }

        #endregion
    }
}
