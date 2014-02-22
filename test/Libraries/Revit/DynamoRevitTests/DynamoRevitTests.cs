using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.FSchemeInterop;
using Dynamo.Interfaces;
using Dynamo.Units;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;
using RevitServices.Persistence;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using Plane = Autodesk.Revit.DB.Plane;
using SketchPlane = Autodesk.Revit.DB.SketchPlane;
using Transaction = Autodesk.Revit.DB.Transaction;

namespace Dynamo.Tests
{
    [TestFixture]
    public abstract class DynamoRevitUnitTestBase
    {
        protected Transaction _trans;
        protected string _testPath;
        protected string _samplesPath;
        protected string _defsPath;
        protected string _emptyModelPath1;
        protected string _emptyModelPath;
        protected DynamoController Controller;

        //Called before each test method
        
        [SetUp]
        public void SetupPaths()
        {
            //it doesn't make sense to do these steps before every test
            //but when running from the revit plugin we are not loading the 
            //fixture, so the initfixture method is not called.

            //get the test path
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\..\test\revit\");
            _testPath = Path.GetFullPath(testsLoc);

            //get the samples path
            string samplesLoc = Path.Combine(assDir, @"..\..\..\doc\distrib\Samples\");
            _samplesPath = Path.GetFullPath(samplesLoc);

            //set the custom node loader search path
            string defsLoc = Path.Combine(assDir, @".\dynamo_packages\Dynamo Sample Custom Nodes\dyf\");
            _defsPath = Path.GetFullPath(defsLoc);

            _emptyModelPath = Path.Combine(_testPath, "empty.rfa");

            if (DocumentManager.GetInstance().CurrentUIApplication.Application.VersionNumber.Contains("2014") &&
                DocumentManager.GetInstance().CurrentUIApplication.Application.VersionName.Contains("Vasari"))
            {
                _emptyModelPath = Path.Combine(_testPath, "emptyV.rfa");
                _emptyModelPath1 = Path.Combine(_testPath, "emptyV1.rfa");
            }
            else
            {
                _emptyModelPath = Path.Combine(_testPath, "empty.rfa");
                _emptyModelPath1 = Path.Combine(_testPath, "empty1.rfa");
            }
        }

        private void StartDynamo()
        {
            try
            {
                var units = new UnitsManager
                {
                    HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot,
                    HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot,
                    HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot
                };

                //create a new instance of the ViewModel
                Controller = new DynamoController(new ExecutionEnvironment(), typeof (DynamoViewModel), Context.NONE, new UpdateManager.UpdateManager(), units, new DefaultWatchHandler(), new PreferenceSettings())
                    {
                        Testing = true
                    };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Creates two model curves separated in Z.
        /// </summary>
        /// <param name="mc1"></param>
        /// <param name="mc2"></param>
        protected void CreateTwoModelCurves(out ModelCurve mc1, out ModelCurve mc2)
        {
            //create two model curves 
            using (_trans = _trans = new Transaction(DocumentManager.GetInstance().CurrentUIDocument.Document, "CreateTwoModelCurves"))
            {
                _trans.Start();

                Plane p1 = new Plane(XYZ.BasisZ, XYZ.Zero);
                Plane p2 = new Plane(XYZ.BasisZ, new XYZ(0, 0, 5));

                SketchPlane sp1 = DocumentManager.GetInstance().CurrentUIDocument.Document.FamilyCreate.NewSketchPlane(p1);
                SketchPlane sp2 = DocumentManager.GetInstance().CurrentUIDocument.Document.FamilyCreate.NewSketchPlane(p2);
                Curve c1 = DocumentManager.GetInstance().CurrentUIApplication.Application.Create.NewLineBound(XYZ.Zero, new XYZ(1, 0, 0));
                Curve c2 = DocumentManager.GetInstance().CurrentUIApplication.Application.Create.NewLineBound(new XYZ(0, 0, 5), new XYZ(1, 0, 5));
                mc1 = DocumentManager.GetInstance().CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c1, sp1);
                mc2 = DocumentManager.GetInstance().CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c2, sp2);

                _trans.Commit();
            }
        }

        /// <summary>
        /// Creates one model curve on a plane with an origin at 0,0,0
        /// </summary>
        /// <param name="mc1"></param>
        protected void CreateOneModelCurve(out ModelCurve mc1)
        {
            //create two model curves 
            using (_trans = _trans = new Transaction(DocumentManager.GetInstance().CurrentUIDocument.Document, "CreateTwoModelCurves"))
            {
                _trans.Start();

                Plane p1 = new Plane(XYZ.BasisZ, XYZ.Zero);

                SketchPlane sp1 = DocumentManager.GetInstance().CurrentUIDocument.Document.FamilyCreate.NewSketchPlane(p1);
                Curve c1 = DocumentManager.GetInstance().CurrentUIApplication.Application.Create.NewLineBound(XYZ.Zero, new XYZ(1, 0, 0));
                mc1 = DocumentManager.GetInstance().CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c1, sp1);

                _trans.Commit();
            }
        }

        /// <summary>
        /// Opens and activates a new model, and closes the old model.
        /// </summary>
        protected void SwapCurrentModel(string modelPath)
        {
            Document initialDoc = DocumentManager.GetInstance().CurrentUIDocument.Document;
            DocumentManager.GetInstance().CurrentUIApplication.OpenAndActivateDocument(modelPath);
            initialDoc.Close(false);
        }

        protected void OpenAndRun(string subPath)
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, subPath);
            string testPath = Path.GetFullPath(samplePath);

            Assert.IsTrue(File.Exists(testPath), string.Format("Could not find file: {0} for testing.", testPath));

            model.Open(testPath);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
    }
}
