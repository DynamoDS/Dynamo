using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold;

namespace Unfold
{
    public static class AlignPlanarFaces
    {

        /// <summary>
        /// method that searches for an edge in a set of edges that shares the same start point as another edge but is not that edge
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="StartPointOnSharedEdge"></param>
        /// <param name="sharedEdge"></param>
        /// <returns></returns>
        public static List<Point> FindPointsOfEdge_StartingAtPoint(List<UnfoldPlanar.EdgeLikeEntity> edges, 
            Point StartPointOnSharedEdge, UnfoldPlanar.EdgeLikeEntity sharedEdge)
        {
            UnfoldPlanar.EdgeLikeEntity found_edge = null;

            Point Start = null;
            Point End = null;

            // we are searching for an edge that has the same start point as the shared edge, but is not the shared edge
            foreach (var edge in edges)
            {
                if ((edge.End.IsAlmostEqualTo(StartPointOnSharedEdge)) || (edge.Start.IsAlmostEqualTo(StartPointOnSharedEdge)))
                {
                    if (!(edge.Start.IsAlmostEqualTo(StartPointOnSharedEdge) && (edge.End.IsAlmostEqualTo(sharedEdge.End))))
                    {
                        found_edge = edge;
                    }
                }
            }

            //reverse if needed
            if (!found_edge.Start.IsAlmostEqualTo(sharedEdge.Start))
            {

                Start = found_edge.End;
                End = found_edge.Start;
            }
            else
            {
                Start = found_edge.Start;
                End = found_edge.End;
            }
            return new List<Point>() { Start, End };

        }
        
        /// <summary>
        /// Method that returns and integer signifying if the normals between the two faces were as expected or if they were facing opposite directions as defined by the
        /// shared edge betweeen them
        /// </summary>
        /// <param name="facetoRotate"></param>
        /// <param name="referenceFace"></param>
        /// <param name="sharedEdge"></param>
        /// <returns></returns>
        public static int CheckNormalConsistency(UnfoldPlanar.FaceLikeEntity facetoRotate, 
            UnfoldPlanar.FaceLikeEntity referenceFace, UnfoldPlanar.EdgeLikeEntity sharedEdge)
        {
            // assumption that face A is rotation face
            // and face B is reference face.
            // D is a vert on A
            //C is a vert on B
            //Point A is the start point of the shared edge

            int rotFaceNormalOK = -1;
            int refFaceNormalOK = -1;


            Curve sharedcurve = sharedEdge.Curve;

            Surface refsurface = referenceFace.SurfaceEntity;
            Surface rotsurface = facetoRotate.SurfaceEntity;

            List<UnfoldPlanar.EdgeLikeEntity> refedegs = referenceFace.EdgeLikeEntities;
            List<UnfoldPlanar.EdgeLikeEntity> rotedges = facetoRotate.EdgeLikeEntities;

            Vector AB = Vector.ByTwoPoints(sharedcurve.StartPoint, sharedcurve.EndPoint); //Edge from A TO B// this is the shared edge
            Vector ABnorm = AB.Normalized();

            List<Point> ADedgePoints = AlignPlanarFaces.FindPointsOfEdge_StartingAtPoint(rotedges,sharedEdge.Start,sharedEdge);

            Point ADstart = ADedgePoints[0];
            Point ADend = ADedgePoints[1];

           Vector ADVector =  Vector.ByTwoPoints(ADstart,ADend);
           Vector ADnorm = ADVector.Normalized();

           Vector firstCross = ABnorm.Cross(ADnorm);

          Vector  rotFaceNormal = rotsurface.NormalAtParameter(.5, .5);


           double abxad_dot_normal_rot_face =  firstCross.Dot(rotFaceNormal);

            // replace this with almost equal
           if (abxad_dot_normal_rot_face == -1.0)
           {
               rotFaceNormalOK = 1;
           }

           List<Point> ACedgePoints = AlignPlanarFaces.FindPointsOfEdge_StartingAtPoint(refedegs,sharedEdge.Start , sharedEdge);
           Point ACstart = ACedgePoints[0];
           Point ACend = ACedgePoints[1];

           Vector ACVector = Vector.ByTwoPoints(ACstart, ACend);
           Vector ACnorm = ACVector.Normalized();

           Vector secondCross = ACnorm.Cross(ABnorm);

           Vector refFaceNormal = refsurface.NormalAtParameter(.5, .5);

           double acxab_dot_normal_ref_face = secondCross.Dot(refFaceNormal);

           if (acxab_dot_normal_ref_face == -1)
           {
               refFaceNormalOK = 1;
           }

           int result = refFaceNormalOK * rotFaceNormalOK;

           return result;

        }
        /// <summary>
        /// method that rotates one surface to be coplanar with another, around a shared edge.
        /// </summary>
        /// <param name="normalconsistency"></param> des
        /// <param name="facetoRotate"></param>
        /// <param name="referenceFace"></param>
        /// <param name="sharedEdge"></param>
        /// <returns></returns>
        public static Geometry MakeGeometryCoPlanarAroundEdge( int normalconsistency, UnfoldPlanar.FaceLikeEntity facetoRotate,
            UnfoldPlanar.FaceLikeEntity referenceFace, UnfoldPlanar.EdgeLikeEntity sharedEdge)
        {

            Vector rotFaceNorm = facetoRotate.SurfaceEntity.NormalAtParameter(.5,.5);
            Vector refFaceNorm = referenceFace.SurfaceEntity.NormalAtParameter(.5,.5);

            Vector BXACrossedNormals = refFaceNorm.Cross(rotFaceNorm);


            var PlaneRotOrigin = Plane.ByOriginNormal(sharedEdge.End,BXACrossedNormals);

            var radians = -1.0 * Math.Asin(BXACrossedNormals.Length) * normalconsistency;

            var degrees = radians * (180 / Math.PI);

            Geometry rotatedFace = facetoRotate.SurfaceEntity.Rotate(PlaneRotOrigin, degrees);

            return rotatedFace;
                
        }
    }
}
