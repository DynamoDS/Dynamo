using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;

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

        internal override Point Origin
        {
            get { return pointOnCurve; }
        }

        /// <summary>
        /// Returns all the gizmos supported by this manipulator
        /// </summary>
        /// <param name="createOrUpdate">
        /// If true: Create a new gizmo or update a gizmo if already present.
        /// If false: Query for existing gizmos</param>
        /// <returns></returns>
        protected override IEnumerable<IGizmo> GetGizmos(bool createOrUpdate)
        {
            if (gizmo == null && !createOrUpdate) yield break;

            if (createOrUpdate)
            {
                UpdateGizmo();
            }

            yield return gizmo;
        }

        private void UpdateGizmo()
        {
            if (null == gizmo)
            {
                gizmo = new TranslationGizmo(this, tangent, gizmoScale);
            }
            else gizmo.UpdateGeometry(tangent, null, null, gizmoScale);
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
            catch (Exception) 
            { 
                return; 
            }
        }

        protected override bool UpdatePosition()
        {
            if (curve == null) //Curve is not initialized, can't be manipulated now.
                return false;

            if (pointOnCurve == null)
                pointOnCurve = curve.StartPoint;

            //Node output could be a collection, consider the first item as origin.
            Point pt = GetFirstValueFromNode(Node) as Point;
            if (pt == null) return false; //The node output is not Point, could be a function object.

            var param = curve.ParameterAtPoint(pt);
            tangent = curve.TangentAtParameter(param);
            
            //Don't cache pt directly here, need to create a copy, because 
            //pt may be GC'ed by VM.
            pointOnCurve = Point.ByCoordinates(pt.X, pt.Y, pt.Z);

            return tangent != null;
        }

        protected override IEnumerable<NodeModel> OnGizmoClick(IGizmo gizmoInAction, object hitObject)
        {
            var axis = hitObject as Vector;
            if (null == axis) return null;
            
            if(null == inputNode)
            {
                inputNode = CreateAndConnectInputNode(0, 1);
            }

            return new[] { inputNode };
        }

        protected override void OnGizmoMoved(IGizmo gizmoInAction, Vector offset)
        {
            double param;
            using (var offsetPosition = pointOnCurve.Add(offset))
            {
                using (var closestPosition = curve.ClosestPointTo(offsetPosition))
                {
                    param = curve.ParameterAtPoint(closestPosition);
                }
            }
            param = Math.Round(param, ROUND_UP_PARAM);

            tangent = curve.TangentAtParameter(param);
            pointOnCurve = curve.PointAtParameter(param);
        }

        protected override List<(NodeModel inputNode, double amount)> InputNodesToUpdateAfterMove(Vector offset)
        {
            double param = curve.ParameterAtPoint(pointOnCurve);
            return new List<(NodeModel, double)>() { (inputNode, param) };
        }

        protected override void Dispose(bool disposing)
        {
            if(tangent != null) tangent.Dispose();

            base.Dispose(disposing);
        }

        #endregion
    }
}
