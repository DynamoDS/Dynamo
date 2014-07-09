using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold.Unfold_Planar;

namespace Unfold
{
   public static class AlignPlanarFaces
    {


       public static List<Point> FindPointsOfEdge_StartingAtPoint(List<Unfold_Planar.EdgeLikeEntity> edges, Point StartPointOnSharedEdge, Unfold_Planar.EdgeLikeEntity sharedEdge)
       {
           Unfold_Planar.EdgeLikeEntity found_edge = null;

           Point Start = null;
           Point End = null;
           
           // we are searching for an edge that has the same start point as the shared edge, but is not the shared edge
           foreach (var edge in edges)
           {
               if ((edge.End.IsAlmostEqualTo(StartPointOnSharedEdge)) || ( edge.Start.IsAlmostEqualTo(StartPointOnSharedEdge))){
                   if (!(edge.Start.IsAlmostEqualTo(StartPointOnSharedEdge)&&(edge.End.IsAlmostEqualTo(sharedEdge.End)))){
                       found_edge = edge;
                   }
               }
           }

           //reverse if needed
           if (!found_edge.Start.IsAlmostEqualTo(sharedEdge.Start)){

               Start = found_edge.End;
               End = found_edge.Start;
           }

           return new List<Point>() { Start, End };

       }
        public static Dictionary<string, object> CheckNormalConsistency(Unfold_Planar.FaceLikeEntity facetoRotate, Unfold_Planar.FaceLikeEntity referenceFace, Unfold_Planar.EdgeLikeEntity sharedEdge)
        {
            // assumption that face A is rotation face
            // and face B is reference face.
            // D is a vert on A
            //C is a vert on B
            //Point A is the start point of the shared edge

            Curve sharedcurve = sharedEdge.Curve;

            Surface refsurface = referenceFace.SurfaceEntity;
            Surface rotsurface = facetoRotate.SurfaceEntity;

            List<Unfold_Planar.EdgeLikeEntity> refedegs = referenceFace.EdgeLikeEntities;
            List<Unfold_Planar.EdgeLikeEntity> rotedges = referenceFace.EdgeLikeEntities;

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

        }
    }
}
