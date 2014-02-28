using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Ray Bounce")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_QUERY)]
    [NodeDescription("Conduct a ray trace analysis from an origin and direction, providing the maximum number of bounces.")]
    class RayBounce : RevitTransactionNode
    {
        private Face currFace;

        private PortData intersections = new PortData("intersections", "The collection of intersection points.",
                                                      typeof(FScheme.Value.List));

        private PortData elements = new PortData("elements", "The elements intersected by the ray.", typeof(FScheme.Value.List));

        public RayBounce()
        {
            InPortData.Add(new PortData("origin", "The origin of the ray.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("direction", "The direction of the ray.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("maxBounces", "The maximum number of bounces allowed.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("view", "The view in which to conduct the analysis.", typeof(FScheme.Value.Container)));

            OutPortData.Add(intersections);
            OutPortData.Add(elements);

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            var origin = (XYZ)((FScheme.Value.Container)args[0]).Item;
            var direction = (XYZ)((FScheme.Value.Container)args[1]).Item;
            var rayLimit = ((FScheme.Value.Number)args[2]).Item;
            var view = (View3D)((FScheme.Value.Container)args[3]).Item;

            XYZ startpt = origin;
            int rayCount = 0;

            var bouncePts = FSharpList<FScheme.Value>.Empty;
            var bounceElements = FSharpList<FScheme.Value>.Empty;
            bouncePts = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(origin), bouncePts);

            for (int ctr = 1; ctr <= rayLimit; ctr++)
            {
                var referenceIntersector = new ReferenceIntersector(view);
                IList<ReferenceWithContext> references = referenceIntersector.Find(startpt, direction);
                ReferenceWithContext rClosest = null;
                rClosest = FindClosestReference(references);
                if (rClosest == null)
                {
                    break;
                }
                else
                {
                    var reference = rClosest.GetReference();
                    var referenceElement = DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(reference);
                    bounceElements = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(referenceElement), bounceElements);
                    var referenceObject = referenceElement.GetGeometryObjectFromReference(reference);
                    var endpt = reference.GlobalPoint;
                    if (startpt.IsAlmostEqualTo(endpt))
                    {
                        break;
                    }
                    else
                    {
                        rayCount = rayCount + 1;
                        currFace = referenceObject as Face;
                        var endptUV = reference.UVPoint;
                        var FaceNormal = currFace.ComputeDerivatives(endptUV).BasisZ;  // face normal where ray hits
                        FaceNormal = rClosest.GetInstanceTransform().OfVector(FaceNormal); // transformation to get it in terms of document coordinates instead of the parent symbol
                        var directionMirrored = direction - 2 * direction.DotProduct(FaceNormal) * FaceNormal; //http://www.fvastro.org/presentations/ray_tracing.htm
                        direction = directionMirrored; // get ready to shoot the next ray
                        startpt = endpt;

                        bouncePts = FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(endpt), bouncePts);
                    }
                }
            }
            bouncePts.Reverse();
            bounceElements.Reverse();

            outPuts[intersections] = FScheme.Value.NewList(bouncePts);
            outPuts[elements] = FScheme.Value.NewList(bounceElements);
        }

        /// <summary>
        /// Find the first intersection with a face
        /// </summary>
        /// <param name="references"></param>
        /// <returns></returns>
        public Autodesk.Revit.DB.ReferenceWithContext FindClosestReference(IList<ReferenceWithContext> references)
        {
            ReferenceWithContext rClosest = null;

            double face_prox = System.Double.PositiveInfinity;
            double edge_prox = System.Double.PositiveInfinity;
            foreach (ReferenceWithContext r in references)
            {
                Reference reference = r.GetReference();
                Element referenceElement = DocumentManager.GetInstance().CurrentUIDocument.Document.GetElement(reference);
                GeometryObject referenceGeometryObject = referenceElement.GetGeometryObjectFromReference(reference);
                currFace = null;
                currFace = referenceGeometryObject as Face;
                Edge edge = null;
                edge = referenceGeometryObject as Edge;
                if (currFace != null)
                {
                    if ((r.Proximity < face_prox) && (r.Proximity > System.Double.Epsilon))
                    {
                        rClosest = r;
                        face_prox = Math.Abs(r.Proximity);
                    }
                }
                else if (edge != null)
                {
                    if ((r.Proximity < edge_prox) && (r.Proximity > System.Double.Epsilon))
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
