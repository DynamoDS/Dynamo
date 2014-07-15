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
   public class DSUnfold
    {   
        public static List<Surface> UnfoldTest(List<Face> faces){


          var surfaces =  UnfoldPlanar.PlanarUnfolder.DSPLanarUnfoldFullFromFace(faces);
          return surfaces;
        }
    }
}
