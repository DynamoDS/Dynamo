using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Tests;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using CurveByPoints = Autodesk.Revit.DB.CurveByPoints;
using DividedSurface = Autodesk.Revit.DB.DividedSurface;
using ModelCurve = Autodesk.Revit.DB.ModelCurve;
using Plane = Autodesk.Revit.DB.Plane;
using SketchPlane = Autodesk.Revit.DB.SketchPlane;
using Transaction = Autodesk.Revit.DB.Transaction;
using Value = Dynamo.FScheme.Value;

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

        //[SetUp]
        //public void Init()
        //{
        //    SetupPaths();
        //    StartDynamo();
        //}

        //[TearDown]
        //public void Cleanup()
        //{
        //    try
        //    {
        //        DynamoLogger.Instance.FinishLogging();
        //        Controller.ShutDown();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.StackTrace);
        //    }
        //}

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
            string defsLoc = Path.Combine(assDir, @".\definitions\");
            _defsPath = Path.GetFullPath(defsLoc);

            _emptyModelPath = Path.Combine(_testPath, "empty.rfa");

            if (dynRevitSettings.Revit.Application.VersionNumber.Contains("2014") &&
                dynRevitSettings.Revit.Application.VersionName.Contains("Vasari"))
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
                DynamoLogger.Instance.StartLogging();

                //create a new instance of the ViewModel
                Controller = new DynamoController(new ExecutionEnvironment(), typeof (DynamoViewModel), Context.NONE)
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
            using (_trans = _trans = new Transaction(dynRevitSettings.Doc.Document, "CreateTwoModelCurves"))
            {
                _trans.Start();

                Plane p1 = new Plane(XYZ.BasisZ, XYZ.Zero);
                Plane p2 = new Plane(XYZ.BasisZ, new XYZ(0, 0, 5));

                SketchPlane sp1 = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(p1);
                SketchPlane sp2 = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(p2);
                Curve c1 = dynRevitSettings.Revit.Application.Create.NewLineBound(XYZ.Zero, new XYZ(1, 0, 0));
                Curve c2 = dynRevitSettings.Revit.Application.Create.NewLineBound(new XYZ(0, 0, 5), new XYZ(1, 0, 5));
                mc1 = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurve(c1, sp1);
                mc2 = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurve(c2, sp2);

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
            using (_trans = _trans = new Transaction(dynRevitSettings.Doc.Document, "CreateTwoModelCurves"))
            {
                _trans.Start();

                Plane p1 = new Plane(XYZ.BasisZ, XYZ.Zero);

                SketchPlane sp1 = dynRevitSettings.Doc.Document.FamilyCreate.NewSketchPlane(p1);
                Curve c1 = dynRevitSettings.Revit.Application.Create.NewLineBound(XYZ.Zero, new XYZ(1, 0, 0));
                mc1 = dynRevitSettings.Doc.Document.FamilyCreate.NewModelCurve(c1, sp1);

                _trans.Commit();
            }
        }

        /// <summary>
        /// Opens and activates a new model, and closes the old model.
        /// </summary>
        protected void SwapCurrentModel(string modelPath)
        {
            Document initialDoc = dynRevitSettings.Doc.Document;
            dynRevitSettings.Revit.OpenAndActivateDocument(modelPath);
            initialDoc.Close(false);
        }
    }

    [TestFixture]
    public class DynamoRevitUnitTests : DynamoRevitUnitTestBase
    {

        [Test]
        public void ElementNodeReassociation()
        {
            var model = dynSettings.Controller.DynamoModel;

            string testPath = Path.Combine(_testPath, "ReferencePoint.dyn");
            model.Open(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count);

            dynSettings.Controller.RunExpression();

            var refPtNode = model.CurrentWorkspace.FirstNodeFromWorkspace<ReferencePointByXyz>();

            var oldVal = (ReferencePoint)((Value.Container)refPtNode.OldValue).Item;
            refPtNode.ResetOldValue();

            var oldId = oldVal.Id;
            var oldPos = oldVal.Position;

            model.CurrentWorkspace.FirstNodeFromWorkspace<DoubleInput>().Value = "1";

            dynSettings.Controller.RunExpression();

            var newVal = (ReferencePoint)((Value.Container)refPtNode.OldValue).Item;

            Assert.AreEqual(oldId, newVal.Id);
            Assert.AreNotEqual(oldPos, newVal.Position);
        }

        [Test]
        public void Loft()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Loft.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            var fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(GenericForm));

            //verify one loft created
            int count = fec.ToElements().Count;
            Assert.IsInstanceOf(typeof(Form), fec.ToElements().First());
            Assert.AreEqual(1, count);
        }

        [Test]
        public void XYZFromReferencePoint()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\XYZFromReferencePoint.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            ReferencePoint rp;
            using(_trans = new Transaction(dynRevitSettings.Doc.Document))
            {
                _trans.Start("Create a reference point.");

                rp = dynRevitSettings.Doc.Document.FamilyCreate.NewReferencePoint(new XYZ());

                _trans.Commit();

            }
            FSharpList<Value> args = FSharpList<Value>.Empty;
            args = FSharpList<Value>.Cons(Value.NewContainer(rp), args);

            //find the XYZFromReferencePoint node
            var node = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is XyzFromReferencePoint).First();

            Value v = ((NodeWithOneOutput)node).Evaluate(args);
            Assert.IsInstanceOf(typeof(XYZ), ((Value.Container)v).Item);
        }

        [Test]
        public void AdaptiveComponent()
        {
            var model = dynSettings.Controller.DynamoModel;

            //string path = Path.Combine(_testPath, @".\AdaptiveComponent.rfa");
            //string modelPath = Path.GetFullPath(path);
            //SwapCurrentModel(modelPath);

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponent.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            //the .dyn has the slider set at 5. let's make sure that
            //if you set the slider to something else before running, that it get the correct number
            var slider = dynSettings.Controller.DynamoModel.Nodes.First(x => x is DoubleSliderInput);
            ((BasicInteractive<double>)slider).Value = 1;

            dynSettings.Controller.RunExpression(true);

            //get all the family instances in the document
            var acs = GetAllFamilyInstancesWithTypeName("3PointAC_wireTruss");
            Assert.AreEqual(1, acs.Count());

            //change the number slider
            ((BasicInteractive<double>)slider).Value = 3;

            dynSettings.Controller.RunExpression(true);
            acs = GetAllFamilyInstancesWithTypeName("3PointAC_wireTruss");
            Assert.AreEqual(3, acs.Count());

        }

        [Test]
        public void SwitchDocuments()
        {
            var model = dynSettings.Controller.DynamoModel;

            //open the workflow and run the expression
            string testPath = Path.Combine(_testPath, "ReferencePointTest.dyn");
            model.Open(testPath);
            Assert.AreEqual(3, dynSettings.Controller.DynamoModel.Nodes.Count());
            dynSettings.Controller.RunExpression(true);

            //verify we have a reference point
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //open a new document and activate it
            UIDocument initialDoc = dynRevitSettings.Revit.ActiveUIDocument;
            string shellPath = Path.Combine(_testPath, @"empty1.rfa");
            dynRevitSettings.Revit.OpenAndActivateDocument(shellPath);
            initialDoc.Document.Close(false);

            //assert that the doc is set on the controller
            Assert.IsNotNull(dynRevitSettings.Doc.Document);

            //update the double node so the graph reevaluates
            var doubleNodes = dynSettings.Controller.DynamoModel.Nodes.Where(x => x is BasicInteractive<double>);
            BasicInteractive<double> node = doubleNodes.First() as BasicInteractive<double>;
            node.Value = node.Value + .1;

            //run the expression again
            dynSettings.Controller.RunExpression(true);
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //finish out by restoring the original
            initialDoc = dynRevitSettings.Revit.ActiveUIDocument;
            shellPath = Path.Combine(_testPath, @"empty.rfa");
            dynRevitSettings.Revit.OpenAndActivateDocument(shellPath);
            initialDoc.Document.Close(false);

        }

        [Test]
        public void CanChangeLacingAndHaveElementsUpdate()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\LacingTest.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            var xyzNode = dynSettings.Controller.DynamoModel.Nodes.First(x => x is Xyz);
            Assert.IsNotNull(xyzNode);

            //test the first lacing
            xyzNode.ArgumentLacing = LacingStrategy.First;
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //test the shortest lacing
            xyzNode.ArgumentLacing = LacingStrategy.First;
            dynSettings.Controller.RunExpression(true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(1, fec.ToElements().Count());

            //test the longest lacing
            xyzNode.ArgumentLacing = LacingStrategy.Longest;
            dynSettings.Controller.RunExpression(true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(5, fec.ToElements().Count());

            //test the cross product lacing
            xyzNode.ArgumentLacing = LacingStrategy.CrossProduct;
            dynSettings.Controller.RunExpression(true);
            fec = null;
            fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(ReferencePoint));
            Assert.AreEqual(20, fec.ToElements().Count());
        }

        [Test]
        public void DividedSurface()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\DividedSurface.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //var shellPath = Path.Combine(_testPath, "shell.rfa");
            //SwapCurrentModel(shellPath);

            model.Open(testPath);
            dynSettings.Controller.RunExpression(true);

            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof (DividedSurface));

            //did it create a divided surface?
            Assert.AreEqual(1, fec.ToElements().Count());

            var ds = (DividedSurface) fec.ToElements()[0];
            Assert.AreEqual(5, ds.USpacingRule.Number);
            Assert.AreEqual(5, ds.VSpacingRule.Number);

            //can we change the number of divisions
            var numNode = dynSettings.Controller.DynamoModel.Nodes.OfType<DoubleInput>().First();
            numNode.Value = "10";
            dynSettings.Controller.RunExpression(true);

            //did it create a divided surface?
            Assert.AreEqual(10, ds.USpacingRule.Number);
            Assert.AreEqual(10, ds.VSpacingRule.Number);

            //does it throw an error when we try to set a negative number of divisions
            numNode.Value = "-5";
            Assert.Throws(typeof(AssertionException),
                          () => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void UVRandom()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\UVRandom.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void AdaptiveComponentByFace()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponentByFace.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void AdaptiveComponentByCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\AdaptiveComponentByCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void GetCurveDomain()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\GetCurveDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void GetSurfaceDomain()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\GetSurfaceDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void GetMaterialByName()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\GetMaterialByName.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        /// <summary>
        /// Sanity Check should always throw an error.
        /// </summary>
        [Test]
        public void SanityCheck()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\SanityCheck.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.Throws(typeof(AssertionException), () => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void Length()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Length.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void MAGN_66()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\MAGN_66.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }
        
        [Test]
        public void CurvebyPointsEllipse()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\CurvebyPointsEllipse.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
            
            
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(CurveElement));

            Assert.AreEqual(fec.ToElements().Count(), 1);

            CurveByPoints mc = (CurveByPoints)fec.ToElements().ElementAt(0);
        }

        [Test]
        public void ModelText()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\ModelText.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void OverrideElementColorInView()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\OverrideElementColorInView.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void Level()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Level.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true)); 

            //ensure that the level count is the same
            var levelColl = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            levelColl.OfClass(typeof (Autodesk.Revit.DB.Level));
            Assert.AreEqual(levelColl.ToElements().Count(), 6);

            //change the number and run again
            var numNode = (DoubleInput)dynRevitSettings.Controller.DynamoModel.Nodes.First(x => x is DoubleInput);
            numNode.Value = "0..20..2";
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true)); 

            //ensure that the level count is the same
            levelColl = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            levelColl.OfClass(typeof(Autodesk.Revit.DB.Level));
            Assert.AreEqual(levelColl.ToElements().Count(), 11);
        }

        [Test]
        public void RayBounce()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\RayBounce.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //ensure that the bounce curve count is the same
            var curveColl = new FilteredElementCollector(dynRevitSettings.Doc.Document, dynRevitSettings.Doc.ActiveView.Id);
            curveColl.OfClass(typeof(CurveElement));
            Assert.AreEqual(curveColl.ToElements().Count(), 36);
        }

        /// <summary>
        /// Automated creation of regression test cases.
        /// </summary>
        /// <param name="dynamoFilePath">The path of the dynamo workspace.</param>
        /// <param name="revitFilePath">The path of the Revit rfa or rvt file.</param>
        [Test, TestCaseSource("SetupRevitRegressionTests")]
        public void Regressions(string dynamoFilePath, string revitFilePath)
        {
            //ensure that the incoming arguments are not empty or null
            //if a dyn file is found in the regression tests directory
            //and there is no corresponding rfa or rvt, then an empty string
            //or a null will be passed into here.
            Assert.IsNotNullOrEmpty(dynamoFilePath, "Dynamo file path is invalid or missing.");
            Assert.IsNotNullOrEmpty(revitFilePath, "Revit file path is invalid or missing.");

            //open the revit model
            SwapCurrentModel(revitFilePath);

            //open the dyn file
            Assert.True(dynSettings.Controller.DynamoViewModel.OpenCommand.CanExecute(dynamoFilePath));
            dynSettings.Controller.DynamoViewModel.OpenCommand.Execute(dynamoFilePath);

            //run the expression and assert that it does not
            //throw an error
            Assert.DoesNotThrow(()=> dynSettings.Controller.RunExpression(false));
        }

        /// <summary>
        /// Method referenced by the automated regression testing setup method.
        /// Populates the test cases based on file pairings in the regression tests folder.
        /// </summary>
        /// <returns></returns>
        static List<object[]> SetupRevitRegressionTests()
        {
            var testParameters = new List<object[]>();

            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;
            string testsLoc = Path.Combine(assDir, @"..\..\test\revit\regression\");
            var regTestPath = Path.GetFullPath(testsLoc);

            var di = new DirectoryInfo(regTestPath);
            var dyns = di.GetFiles("*.dyn");
            foreach (var fileInfo in dyns)
            {
                var data = new object[2];
                data[0] = fileInfo.FullName;

                //find the corresponding rfa or rvt file
                var nameBase = fileInfo.FullName.Remove(fileInfo.FullName.Length - 4);
                var rvt = nameBase + ".rvt";
                var rfa = nameBase + ".rfa";

                //add test parameters for rvt, rfa, or both
                if (File.Exists(rvt))
                {
                    data[1] = rvt;
                }
                
                if (File.Exists(rfa))
                {
                    data[1] = rfa;
                }

                testParameters.Add(data);
            }

            return testParameters;
        }

        /// <summary>
        /// Retrieves all family instances of the named type from the active document.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static IEnumerable<FamilyInstance> GetAllFamilyInstancesWithTypeName(string typeName)
        {
            FilteredElementCollector fec = new FilteredElementCollector(dynRevitSettings.Doc.Document);
            fec.OfClass(typeof(FamilyInstance));
            return fec.ToElements().Where(x => ((FamilyInstance)x).Symbol.Name == typeName).Cast<FamilyInstance>();
        }
    }
}
