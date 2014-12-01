using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using SystemTestServices;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Applications;
using Dynamo.Applications.Models;
using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.ViewModels;
using NUnit.Framework;

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
    public abstract class RevitSystemTestBase : SystemTestBase
    {
        #region public methods

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            DocumentManager.Instance.CurrentUIApplication.ViewActivating += CurrentUIApplication_ViewActivating;
        }

        [TearDown]
        public override void TearDown()
        {
            // Automatic transaction strategy requires that we 
            // close the transaction if it hasn't been closed by 
            // by the end of an evaluation. It is possible to 
            // run the test framework without running Dynamo, so
            // we ensure that the transaction is closed here.
            TransactionManager.Instance.ForceCloseTransaction();
        }

        public override void StartDynamo()
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
                        Context = "Revit 2014",
                        SchedulerThread = new TestSchedulerThread()
                    });

                Model = DynamoRevit.RevitDynamoModel;

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

        #endregion

        #region protected methods

        protected void CurrentUIApplication_ViewActivating(object sender, Autodesk.Revit.UI.Events.ViewActivatingEventArgs e)
        {
            ((RevitDynamoModel)this.ViewModel.Model).SetRunEnabledBasedOnContext(e.NewActiveView);
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

        #endregion
    }
}
