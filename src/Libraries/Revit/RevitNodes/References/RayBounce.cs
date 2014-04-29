using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Face = Autodesk.Revit.DB.Face;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.References
{
    public static class RayBounce
    {
        [MultiReturn(new[] { "points", "elements" })]
        public static Dictionary<string,object> ByOriginDirection(Point origin, Vector direction, int maxBounces, Elements.Views.View3D view)
        {
            var startpt = origin.ToXyz();
            var rayCount = 0;

            var bouncePts = new List<Point> {origin};
            var bounceElements = new List<Elements.Element>();

            for (int ctr = 1; ctr <= maxBounces; ctr++)
            {
                var referenceIntersector = new ReferenceIntersector((View3D)view.InternalElement);
                IList<ReferenceWithContext> references = referenceIntersector.Find(startpt, direction.ToXyz());
                ReferenceWithContext rClosest = null;
                rClosest = FindClosestReference(references);
                if (rClosest == null)
                {
                    break;
                }
                else
                {
                    var reference = rClosest.GetReference();
                    var referenceElement = DocumentManager.Instance.CurrentDBDocument.GetElement(reference);
                    var referenceObject = referenceElement.GetGeometryObjectFromReference(reference);
                    bounceElements.Add(referenceElement.ToDSType(true));
                    var endpt = reference.GlobalPoint;
                    if (startpt.IsAlmostEqualTo(endpt))
                    {
                        break;
                    }
                    else
                    {
                        rayCount = rayCount + 1;
                        var currFace = referenceObject as Face;
                        var endptUV = reference.UVPoint;
                        var FaceNormal = currFace.ComputeDerivatives(endptUV).BasisZ;  // face normal where ray hits
                        FaceNormal = rClosest.GetInstanceTransform().OfVector(FaceNormal); // transformation to get it in terms of document coordinates instead of the parent symbol
                        var directionMirrored = direction.ToXyz() - 2 * direction.ToXyz().DotProduct(FaceNormal) * FaceNormal; //http://www.fvastro.org/presentations/ray_tracing.htm
                        direction = directionMirrored.ToVector(); // get ready to shoot the next ray
                        startpt = endpt;
                        bouncePts.Add(endpt.ToPoint());
                    }
                }
            }

            return new Dictionary<string, object>
            {
                { "points", bouncePts },
                { "elements", bounceElements }
            };
        }

        /// <summary>
        /// Find the first intersection with a face
        /// </summary>
        /// <param name="references"></param>
        /// <returns></returns>
        private static ReferenceWithContext FindClosestReference(IEnumerable<ReferenceWithContext> references)
        {
            ReferenceWithContext rClosest = null;

            var face_prox = Double.PositiveInfinity;
            var edge_prox = Double.PositiveInfinity;

            foreach (ReferenceWithContext r in references)
            {
                var reference = r.GetReference();
                var referenceElement = DocumentManager.Instance.CurrentDBDocument.GetElement(reference);
                var referenceGeometryObject = referenceElement.GetGeometryObjectFromReference(reference);
                Face currFace = null;
                currFace = referenceGeometryObject as Autodesk.Revit.DB.Face;
                Autodesk.Revit.DB.Edge edge = null;
                edge = referenceGeometryObject as Autodesk.Revit.DB.Edge;
                if (currFace != null)
                {
                    if ((r.Proximity < face_prox) && (r.Proximity > Double.Epsilon))
                    {
                        rClosest = r;
                        face_prox = Math.Abs(r.Proximity);
                    }
                }
                else if (edge != null)
                {
                    if ((r.Proximity < edge_prox) && (r.Proximity > Double.Epsilon))
                    {
                        edge_prox = Math.Abs(r.Proximity);
                    }
                }
            }
            if (edge_prox <= face_prox)
            {
                // stop bouncing if there is an edge at least as close as the nearest face - there is no single angle of reflection for a ray striking a line
                //m_outputInfo.Add("there is an edge at least as close as the nearest face - there is no single angle of reflection for a ray striking a line");
                rClosest = null;
            }

            return rClosest;
        }
    }
}
