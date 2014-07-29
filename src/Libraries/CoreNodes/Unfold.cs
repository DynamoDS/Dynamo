using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unfold;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;


namespace DSCore
{
    
   public class Unfolding
    {   
        public static List<Surface> UnfoldListOfFaces(List<Face> faces){


          var unfoldsurfaces =  GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(faces);
          return unfoldsurfaces;
        }

        public static List<Surface> UnfoldListOfSurfaces(List<Surface> surfaces)
        {


            var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(surfaces);
            return unfoldsurfaces;
        }

        public static List<Surface> UnfoldDevelopableSurfaceViaTesselation(List<Surface> surfaces)
        {

            //handle tesselation here
            var pointtuples = Tessellate.Tesselate(surfaces);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>(){x[0], x[1], x[2]})).ToList();

            var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(trisurfaces);
            return unfoldsurfaces;
        }

       // method is for debugging the BFS output visually in dynamo, very useful
        public static object BFSTestTesselation(List<Surface> surfaces)
        {

            //handle tesselation here
            var pointtuples = Tessellate.Tesselate(surfaces);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

            var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);

            //perform BFS on the graph and get back the tree
            var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
            object treegeo = nodereturn["tree geo"];

            return treegeo;
        }
       // exposes node to tesselate surfaces in dynamo, returns triangular surfaces
       // Peter will hate this :)
       public static object TesselateSurfaces(List<Surface> surfaces){
           var pointtuples = Tessellate.Tesselate(surfaces);
           //convert triangles to surfaces
           List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

           return trisurfaces;
           
       }


    }
}
