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


       public static List<Point> FindPointsOfEdgeAD(List<Unfold_Planar.EdgeLikeEntity> edges, Point StartPointOnSharedEdge, Unfold_Planar.EdgeLikeEntity sharedEdge)
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

            Curve sharedcurve = sharedEdge.Curve;
            Surface refsurface = referenceFace.SurfaceEntity;
            Surface rotsurface = facetoRotate.SurfaceEntity;

            List<Unfold_Planar.EdgeLikeEntity> refedegs = referenceFace.EdgeLikeEntities;
            List<Unfold_Planar.EdgeLikeEntity> rotedges = referenceFace.EdgeLikeEntities;

            Vector AB = Vector.ByTwoPoints(sharedcurve.StartPoint, sharedcurve.EndPoint);

            List<Point> edgePoints = AlignPlanarFaces.FindPointsOfEdgeAD(rotedges,sharedEdge.Start,sharedEdge);


        }
    }
}
