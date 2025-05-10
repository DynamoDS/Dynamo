using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Autodesk.GeometryPrimitives.Dynamo.Math;
using Autodesk.GeometryPrimitives.Dynamo.Geometry;
using Vector = Autodesk.GeometryPrimitives.Dynamo.Math.Vector3d;

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

        private Autodesk.DesignScript.Geometry.Curve curve;

        private Vector tangent;

        private NodeModel inputNode;

        private TranslationGizmo gizmo;

        internal PointOnCurveManipulator(DSFunction node, DynamoManipulationExtension manipulatorContext)
            : base(node, manipulatorContext)
        {
        }

        #region abstract method implementation

        internal override Point Origin => pointOnCurve;

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
            CanManipulateInputNode(0, out var curveNode);
            if (null == curveNode)
                return; //Not enough input to manipulate

            if (!CanManipulateInputNode(1, out inputNode))
                return;

            try
            {
                curve = GetFirstValueFromNode(curveNode) as Autodesk.DesignScript.Geometry.Curve;
                if (null == curve)
                    return;
            }
            catch
            {
            }
        }

        // TODO: Implement this method 
        private static Point PointAtParameter(Autodesk.DesignScript.Geometry.Curve curve, double param)
        {
            var pt = curve.PointAtParameter(param);
            return new Point(pt.X, pt.Y, pt.Z);
        }

        // TODO: Implement this method
        private static double ParameterAtPoint(Autodesk.DesignScript.Geometry.Curve curve, Point pt)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                pt.Position.X, pt.Position.Y, pt.Position.Z);
            return curve.ParameterAtPoint(point);
        }

        // TODO: Implement this method
        private static Vector TangentAtParameter(Autodesk.DesignScript.Geometry.Curve curve, double param)
        {
            var tangent = curve.TangentAtParameter(param);
            return new Vector(tangent.X, tangent.Y, tangent.Z);
        }

        // TODO: Implement this method
        private static Point ClosestPointTo(Autodesk.DesignScript.Geometry.Curve curve, Point3d pt)
        {
            var point = Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                pt.X, pt.Y, pt.Z);
            var closestPt = curve.ClosestPointTo(point);
            return new Point(closestPt.X, closestPt.Y, closestPt.Z);
        }

        protected override bool UpdatePosition()
        {
            if (curve == null) //Curve is not initialized, can't be manipulated now.
                return false;

            pointOnCurve ??= PointAtParameter(curve, 0);

            //Node output could be a collection, consider the first item as origin.
            var pt = GetFirstValueFromNode(Node) as Autodesk.DesignScript.Geometry.Point;
            if (pt == null) return false; //The node output is not Point, could be a function object.

            pointOnCurve = new Point(pt.X, pt.Y, pt.Z);
            var param = ParameterAtPoint(curve, pointOnCurve);
            tangent = TangentAtParameter(curve, param);

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
            var offsetPosition = pointOnCurve.Position + offset;
            var closestPosition = ClosestPointTo(curve, offsetPosition);
            param = ParameterAtPoint(curve, closestPosition);
            param = Math.Round(param, ROUND_UP_PARAM);

            tangent = TangentAtParameter(curve, param);
            pointOnCurve = PointAtParameter(curve, param);
        }

        protected override List<(NodeModel inputNode, double amount)> InputNodesToUpdateAfterMove(Vector offset)
        {
            double param = ParameterAtPoint(curve, pointOnCurve);
            return new List<(NodeModel, double)>() { (inputNode, param) };
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion
    }
}
