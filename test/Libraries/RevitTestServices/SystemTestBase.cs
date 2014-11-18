using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Applications;
using Dynamo.Applications.Models;
using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.ViewModels;
using Dynamo.Tests;
using Dynamo.Models;
using NUnit.Framework;
using ProtoCore.Mirror;
using RevitNodesTests;
using RevitServices.Persistence;
using RevitServices.Threading;
using RevitServices.Transactions;

namespace RevitTestServices
{
    public class TestSchedulerThread : ISchedulerThread
    {
        public void Initialize(DynamoScheduler owningScheduler)
        {

        }

        public void Shutdown()
        {

        }
    }

    [TestFixture]
    public abstract class SystemTestBase
    {
        protected DynamoViewModel ViewModel;
        protected string workingDirectory;

        [SetUp]
        public void Setup()
        {
            AssemblyResolver.Setup();

            SetupCore();

            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            StartDynamo();

            DocumentManager.Instance.CurrentUIApplication.ViewActivating += CurrentUIApplication_ViewActivating;
        }

        /// <summary>
        /// Implement this method to do any setup neceessary for your tests.
        /// </summary>
        protected virtual void SetupCore()
        {
            
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

        protected void CurrentUIApplication_ViewActivating(object sender, Autodesk.Revit.UI.Events.ViewActivatingEventArgs e)
        {
            ((RevitDynamoModel)this.ViewModel.Model).SetRunEnabledBasedOnContext(e.NewActiveView);
        }

        protected void StartDynamo()
        {
            try
            {
                // create the transaction manager object
                TransactionManager.SetupManager(new AutomaticTransactionStrategy());

                DynamoRevit.InitializeUnits();

                DynamoRevit.RevitDynamoModel = RevitDynamoModel.Start(
                    new RevitDynamoModel.StartConfiguration()
                    {
                        StartInTestMode = true,
                        DynamoCorePath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\"),
                        Context = "Revit 2015",
                        SchedulerThread = new TestSchedulerThread()
                    });

                this.ViewModel = DynamoViewModel.Start(
                    new DynamoViewModel.StartConfiguration()
                    {
                        DynamoModel = DynamoRevit.RevitDynamoModel,
                    });

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
            using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "CreateTwoModelCurves"))
            {
                trans.Start();

                var p1 = new Plane(XYZ.BasisZ, XYZ.Zero);
                var p2 = new Plane(XYZ.BasisZ, new XYZ(0, 0, 5));

                SketchPlane sp1 = SketchPlane.Create(DocumentManager.Instance.CurrentDBDocument, p1);
                SketchPlane sp2 = SketchPlane.Create(DocumentManager.Instance.CurrentDBDocument, p2);
                Curve c1 = Line.CreateBound(XYZ.Zero, new XYZ(1, 0, 0));
                Curve c2 = Line.CreateBound(new XYZ(0, 0, 5), new XYZ(1, 0, 5));

                mc1 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c1, sp1);
                mc2 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c2, sp2);

                trans.Commit();
            }
        }

        /// <summary>
        /// Creates one model curve on a plane with an origin at 0,0,0
        /// </summary>
        /// <param name="mc1"></param>
        protected void CreateOneModelCurve(out ModelCurve mc1)
        {
            //create two model curves 
            using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "CreateTwoModelCurves"))
            {
                trans.Start();

                var p1 = new Plane(XYZ.BasisZ, XYZ.Zero);

                SketchPlane sp1 = SketchPlane.Create(DocumentManager.Instance.CurrentDBDocument, p1);
                Curve c1 = Line.CreateBound(XYZ.Zero, new XYZ(1, 0, 0));

                mc1 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewModelCurve(c1, sp1);

                trans.Commit();
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
            string samplePath = Path.Combine(workingDirectory, subPath);
            string testPath = Path.GetFullPath(samplePath);

            Assert.IsTrue(File.Exists(testPath), string.Format("Could not find file: {0} for testing.", testPath));

            ViewModel.OpenCommand.Execute(testPath);

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        #region Revit unit test helper methods

        public void RunCurrentModel()
        {
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        public void AssertNoDummyNodes()
        {
            var nodes = ViewModel.Model.Nodes;

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

        public NodeModel GetNode<T>(string guid) where T : NodeModel
        {
            var allNodes = ViewModel.Model.Nodes;
            var nodes = allNodes.Where(x => string.CompareOrdinal(x.GUID.ToString(), guid) == 0);
            if (nodes.Count() < 1)
                return null;
            else if (nodes.Count() > 1)
                throw new Exception("There are more than one nodes with the same GUID!");
            return nodes.ElementAt(0) as T;
        }

        public object GetPreviewValue(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().Data;
        }

        /// <summary>
        /// Get a collection from a node's mirror data.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>A list of objects if the data is a collection, else null.</returns>
        public List<object> GetPreviewCollection(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var data = mirror.GetData();
            if (data == null)
            {
                Assert.Fail("The mirror has no data.");
            }

            var dataColl = mirror.GetData().GetElements();
            if (dataColl == null)
            {
                return null;
            }

            var elements = dataColl.Select(x => x.Data).ToList();

            return elements;
        }

        public object GetPreviewValueAtIndex(string guid, int index)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var data = mirror.GetData();
            if (data == null) return null;
            if (!data.IsCollection) return null;
            var elements = data.GetElements();
            return elements[index].Data;
        }

        public List<object> GetFlattenedPreviewValues(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var data = mirror.GetData();
            if (data == null) return null;
            if (!data.IsCollection)
            {
                return data.Data == null ? new List<object>() : new List<object>(){data.Data};
            }
            var elements = data.GetElements();

            var objects = GetSublistItems(elements);

            return objects;
        }

        private static List<object> GetSublistItems(IEnumerable<MirrorData> datas)
        {
            var objects = new List<object>();
            foreach (var data in datas)
            {
                if (!data.IsCollection)
                {
                    objects.Add(data.Data);
                }
                else
                {
                    objects.AddRange(GetSublistItems(data.GetElements()));
                }
            }
            return objects;
        } 

        public void AssertClassName(string guid, string className)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var classInfo = mirror.GetData().Class;
            Assert.AreEqual(classInfo.ClassName, className);
        }

        protected static bool IsFuzzyEqual(double d0, double d1, double tol)
        {
            return System.Math.Abs(d0 - d1) < tol;
        }

        private string GetVarName(string guid)
        {
            var model = ViewModel.Model;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.AstIdentifierBase;
        }

        private RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(varName));
            return mirror;
        }

        protected bool IsNodeInErrorOrWarningState(string guid)
        {
            var model = ViewModel.Model;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.State == Dynamo.Models.ElementState.Error ||
                    node.State == Dynamo.Models.ElementState.Warning;
        }

        #endregion

    }
}
