using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Applications;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoUnits;
using Dynamo.UpdateManager;

using DynamoUtilities;

using NUnit.Framework;
using ProtoCore.Mirror;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Threading;
using RevitServices.Transactions;
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
        public void Setup()
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string assDir = fi.DirectoryName;

            // Setup the core paths
            DynamoPathManager.Instance.InitializeCore(Path.GetFullPath(assDir + @"\.."));

            StartDynamo();

            DocumentManager.Instance.CurrentUIApplication.ViewActivating += CurrentUIApplication_ViewActivating;

            //it doesn't make sense to do these steps before every test
            //but when running from the revit plugin we are not loading the 
            //fixture, so the initfixture method is not called.

            //get the test path
            string testsLoc = Path.Combine(assDir, @"..\..\..\..\test\System\revit\");
            _testPath = Path.GetFullPath(testsLoc);

            //get the samples path
            string samplesLoc = Path.Combine(assDir, @"..\..\..\..\doc\distrib\Samples\");
            _samplesPath = Path.GetFullPath(samplesLoc);

            //set the custom node loader search path
            string defsLoc = Path.Combine(DynamoPathManager.Instance.Packages, "Dynamo Sample Custom Nodes", "dyf");
            _defsPath = Path.GetFullPath(defsLoc);

            _emptyModelPath = Path.Combine(_testPath, "empty.rfa");

            if (DocumentManager.Instance.CurrentUIApplication.Application.VersionNumber.Contains("2014") &&
                DocumentManager.Instance.CurrentUIApplication.Application.VersionName.Contains("Vasari"))
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

        [TearDown]
        public void TearDown()
        {
            // Automatic transaction strategy requires that we 
            // close the transaction if it hasn't been closed by 
            // by the end of an evaluation. It is possible to 
            // run the test framework without running Dynamo, so
            // we ensure that the transaction is closed here.
            TransactionManager.Instance.ForceCloseTransaction();
        }

        void CurrentUIApplication_ViewActivating(object sender, Autodesk.Revit.UI.Events.ViewActivatingEventArgs e)
        {
            DynamoRevit.SetRunEnabledBasedOnContext(e.NewActiveView);
        }

        private void StartDynamo()
        {
            try
            {
                var updater = new RevitServicesUpdater(DynamoRevitApp.ControlledApplication, DynamoRevitApp.Updaters);
                updater.ElementAddedForID += ElementMappingCache.GetInstance().WatcherMethodForAdd;
                updater.ElementsDeleted += ElementMappingCache.GetInstance().WatcherMethodForDelete;

                SIUnit.HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot;
                SIUnit.HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot;
                SIUnit.HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot;

                var logger = new DynamoLogger(DynamoPathManager.Instance.Logs);
                dynSettings.DynamoLogger = logger;
                var updateManager = new UpdateManager.UpdateManager(logger);

                Controller = DynamoRevit.CreateDynamoRevitControllerAndViewModel(updater, logger, Context.NONE);
                DynamoController.IsTestMode = true;

                // create the transaction manager object
                TransactionManager.SetupManager(new AutomaticTransactionStrategy());

                // Because the test framework does not work in the idle thread. 
                // We need to trick Dynamo into believing that it's in the idle
                // thread already.
                IdlePromise.InIdleThread = true;
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
            using (_trans = _trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "CreateTwoModelCurves"))
            {
                _trans.Start();

                var p1 = new Plane(XYZ.BasisZ, XYZ.Zero);
                var p2 = new Plane(XYZ.BasisZ, new XYZ(0, 0, 5));

                SketchPlane sp1 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewSketchPlane(p1);
                SketchPlane sp2 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewSketchPlane(p2);
                Curve c1 = DocumentManager.Instance.CurrentUIApplication.Application.Create.NewLineBound(XYZ.Zero, new XYZ(1, 0, 0));
                Curve c2 = DocumentManager.Instance.CurrentUIApplication.Application.Create.NewLineBound(new XYZ(0, 0, 5), new XYZ(1, 0, 5));
                mc1 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c1, sp1);
                mc2 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c2, sp2);

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
            using (_trans = _trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "CreateTwoModelCurves"))
            {
                _trans.Start();

                var p1 = new Plane(XYZ.BasisZ, XYZ.Zero);

                SketchPlane sp1 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewSketchPlane(p1);
                Curve c1 = DocumentManager.Instance.CurrentUIApplication.Application.Create.NewLineBound(XYZ.Zero, new XYZ(1, 0, 0));
                mc1 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c1, sp1);

                _trans.Commit();
            }
        }

        /// <summary>
        /// Opens and activates a new model.
        /// </summary>
        /// <param name="modelPath"></param>
        protected UIDocument OpenAndActivateNewModel(string modelPath)
        {
            DocumentManager.Instance.CurrentUIApplication.OpenAndActivateDocument(modelPath);
            return DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;
        }

        /// <summary>
        /// Opens and activates a new model, and closes the old model.
        /// </summary>
        protected void SwapCurrentModel(string modelPath)
        {
            Document initialDoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument.Document;
            DocumentManager.Instance.CurrentUIApplication.OpenAndActivateDocument(modelPath);
            initialDoc.Close(false);
        }

        protected void OpenAndRun(string subPath)
        {
            string samplePath = Path.Combine(_testPath, subPath);
            string testPath = Path.GetFullPath(samplePath);

            Assert.IsTrue(File.Exists(testPath), string.Format("Could not find file: {0} for testing.", testPath));

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        #region Revit unit test helper methods

        public void OpenModel(string relativeFilePath)
        {
            string samplePath = Path.Combine(_samplesPath, relativeFilePath);
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
        }

        public void RunCurrentModel()
        {
            Assert.DoesNotThrow(() => Controller.RunExpression(null));
        }

        public void AssertNoDummyNodes()
        {
            var nodes = Controller.DynamoModel.Nodes;

            double dummyNodesCount = nodes.OfType<DSCoreNodesUI.DummyNode>().Count();
            if (dummyNodesCount >= 1)
            {
                Assert.Fail("Number of dummy nodes found in Sample: " + dummyNodesCount);
            }
        }

        public void AssertPreviewCount(string guid, int count)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            var data = mirror.GetData();
            Assert.IsTrue(data.IsCollection);
            Assert.AreEqual(count, data.GetElements().Count);
        }

        public object GetPreviewValue(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().Data;
        }

        public object GetPreviewValueAtIndex(string guid, int index)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().GetElements()[index].Data;
        }

        public void AssertClassName(string guid, string className)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var classInfo = mirror.GetData().Class;
            Assert.AreEqual(classInfo.ClassName, className);
        }

        private string GetVarName(string guid)
        {
            var model = Controller.DynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.AstIdentifierBase;
        }

        private RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = Controller.EngineController.GetMirror(varName));
            return mirror;
        }

        #endregion

    }
}
