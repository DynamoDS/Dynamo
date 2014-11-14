using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RevitSystemTests
{
    [TestFixture]
    public class RevitLibraryTests : SystemTest
    {
        [Test]
        public void TestNonBrowsableClasses()
        {
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Element") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Document") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Category") == ProtoCore.DSASM.Constants.kInvalidIndex);
            
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.DividedPath") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.DividedSurface") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.FamilyInstance") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Family") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.FamilySymbol") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Floor") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.FloorType") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Form") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Grid") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Level") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ModelCurve") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.CurveByPoints") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ModelText") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ModelTextType") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ReferencePlane") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ReferencePoint") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.SketchPlane") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Wall") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.WallType") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Mullion") == ProtoCore.DSASM.Constants.kInvalidIndex);
            //Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.View3D") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ViewPlan") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ViewSection") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ViewSheet") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.ViewDrafting") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(GetClassIndex("Autodesk.Revit.DB.Panel") == ProtoCore.DSASM.Constants.kInvalidIndex);

        }

        private int GetClassIndex(string className)
        {
            var engineController = ViewModel.Model.EngineController;
            return engineController.LiveRunnerCore.ClassTable.IndexOf(className);
        } 
    }
}
