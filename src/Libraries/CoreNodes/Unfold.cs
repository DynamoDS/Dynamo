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
           var unfolding = PlanarUnfolder.DSPLanarUnfold(faces);
           return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)},
                    
                };

       }

        [MultiReturn(new[] { "surfaces", "unfoldingObject" })]
        public static Dictionary<string, object> UnfoldListOfSurfacesAndReturnTransforms(List<Surface> surfaces)
        {
            var unfolding = PlanarUnfolder.DSPLanarUnfold(surfaces);
            return new Dictionary<string, object> 
                {   
                    { "surfaces", (unfolding.UnfoldedSurfaceSet)},
                    {"unfoldingObject",(unfolding)},
                    
                };

        }
        public static List<List<Curve>> GenerateLabels
           (PlanarUnfolder.PlanarUnfolding<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity> unfoldingObject)
        {

            var labels = unfoldingObject.StartingUnfoldableFaces.Select(x =>
              new PlanarUnfolder.UnfoldableFaceLabel<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(x)).ToList();

            return labels.Select(x => x.AlignedLabelGeometry).ToList();

        }




       public static List<List<Curve>> GenerateLabelsAndTransformToUnfold
           (PlanarUnfolder.PlanarUnfolding<GeneratePlanarUnfold.EdgeLikeEntity,GeneratePlanarUnfold.FaceLikeEntity> unfoldingObject){
        
           var labels =   unfoldingObject.StartingUnfoldableFaces.Select(x=>
             new PlanarUnfolder.UnfoldableFaceLabel<GeneratePlanarUnfold.EdgeLikeEntity,GeneratePlanarUnfold.FaceLikeEntity>(x)).ToList();
        
           // need to make one piece of geometry from list of geo...
           var transformedGeo = labels.Select(x=> PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldingObject,x.AlignedLabelGeometry,x.ID)).ToList();

          
           return transformedGeo; 
           
       }


           
       


        public static List<Surface> UnfoldListOfFaces(List<Face> faces){


          var unfoldsurfaces =  PlanarUnfolder.DSPLanarUnfold(faces);
          return unfoldsurfaces.UnfoldedSurfaceSet;
        }

        public static List<Surface> UnfoldListOfSurfaces(List<Surface> surfaces)
        {


            var unfoldsurfaces = PlanarUnfolder.DSPLanarUnfold(surfaces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
        }

        public static List<Surface> UnfoldDevelopableSurfaceViaTesselation(List<Surface> surfaces)
        {

            //handle tesselation here
            var pointtuples = Tessellate.Tesselate(surfaces);
            //convert triangles to surfaces
            List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>(){x[0], x[1], x[2]})).ToList();

            var unfoldsurfaces = PlanarUnfolder.DSPLanarUnfold(trisurfaces);
            return unfoldsurfaces.UnfoldedSurfaceSet;
        }


        // The following methods may be removed from Import eventually
        # region  
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
        #endregion

    }
}
