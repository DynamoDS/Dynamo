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


          var unfoldsurfaces =  UnfoldPlanar.PlanarUnfolder.DSPLanarUnfold(faces);
          return unfoldsurfaces;
        }

        public static List<Surface> UnfoldListOfSurfaces(List<Surface> surfaces)
        {


            var unfoldsurfaces = UnfoldPlanar.PlanarUnfolder.DSPLanarUnfold(surfaces);
            return unfoldsurfaces;
        }

        public static List<Surface> UnfoldDevelopableSurfaceViaTesselation(List<Surface> surfaces)
        {

            //handle tesselation here
            
            //convert triangles to surfaces
            var unfoldsurfaces = UnfoldPlanar.PlanarUnfolder.DSPLanarUnfold(surfaces);
            return unfoldsurfaces;
        }
       


    }
}
