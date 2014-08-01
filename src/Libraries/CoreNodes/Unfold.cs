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


        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
       public static Dictionary<string, object> UnfoldListOfFacesAndReturnTransforms(List<Face> faces)
       {
           var unfolding = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(faces);
           return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)},
                    
                };

       }

        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
        public static Dictionary<string, object> UnfoldListOfSurfacesAndReturnTransforms(List<Surface> surfaces)
        {
            var unfolding = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(surfaces);
            return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {" unfoldingObject",(unfolding)},
                    
                };

        }


       public static Geometry MapGeometryToUnfoldingByID(GeneratePlanarUnfold.PlanarUnfolder.PlanarUnfolding<GeneratePlanarUnfold.EdgeLikeEntity,GeneratePlanarUnfold.FaceLikeEntity> unfolding, Geometry geometryToTransform, int id){

          

           // grab all transforms that were applied to this surface id
           var map = unfolding.Maps;
           var applicableTransforms = map.Where(x => x.IDS.Contains(id));
           var transforms = applicableTransforms.Select(x => x.CS).ToList();

           // now test
           //transform geo from first to last.

          

           geometryToTransform = geometryToTransform.Transform(transforms.First());


           var myBox = geometryToTransform.BoundingBox;
           var geoStartPoint = myBox.MinPoint.Add((myBox.MaxPoint.Subtract(myBox.MinPoint.AsVector()).AsVector().Scale(.5)));

           geometryToTransform = geometryToTransform.Translate(Vector.ByTwoPoints(geoStartPoint,unfolding.StartingPoints[id])) ;

           Geometry aggregatedGeo = geometryToTransform;
           for (int i = 0; i + 1 < transforms.Count; i++) {
               aggregatedGeo = aggregatedGeo.Transform(transforms[i + 1]);
               

            }


           return aggregatedGeo;
           
           
       }


        public static List<Surface> UnfoldListOfFaces(List<Face> faces){


          var unfoldsurfaces =  GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(faces);
          return unfoldsurfaces.UnfoldedSurfaceSet;
        }

        public static List<Surface> UnfoldListOfSurfaces(List<Surface> surfaces)
        {


            var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(surfaces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
        }

        public static List<Surface> UnfoldDevelopableSurfaceViaTesselation(List<Surface> surfaces)
        {

            //handle tesselation here
            var pointtuples = Tessellate.Tesselate(surfaces);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>(){x[0], x[1], x[2]})).ToList();

            var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(trisurfaces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
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
