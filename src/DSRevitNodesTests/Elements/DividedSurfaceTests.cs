using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSRevitNodes;
using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class DividedSurfaceTests 
    {
        [Test]
        public void ByFaceAndUVDivisions_ValidArgs()
        {
            var ele = DocumentManager.GetInstance().GetElements<Autodesk.Revit.DB.Form>().FirstOrDefault();
            Assert.NotNull(ele);

            var opts = new Autodesk.Revit.DB.Options { ComputeReferences = true };
            var obj = ele.get_Geometry(opts).First();
            Assert.IsAssignableFrom(typeof(Autodesk.Revit.DB.Solid), obj);

            var solid = obj as Autodesk.Revit.DB.Solid;
            var f = solid.Faces.Cast<Autodesk.Revit.DB.Face>().FirstOrDefault();

            Assert.NotNull(f);

            //var df = new DSFace(face);



        }
    }
}
