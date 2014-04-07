using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Dynamo.Core;
using Dynamo.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;
using DSCoreNodesUI;
using NUnit.Framework;
using System.Reflection;

namespace Dynamo.Tests
{
    #region Sample Test Classes

    class DummyModel : Models.ModelBase
    {
        #region Public Class Methods/Properties

        internal static DummyModel CreateBlankInstance()
        {
            return new DummyModel();
        }

        protected DummyModel() { }

        internal DummyModel(int identifier, int radius)
        {
            this.Identifier = identifier;
            this.Radius = radius;
        }

        internal void DoubleRadius()
        {
            this.Radius = 2 * this.Radius;
        }

        internal int Identifier { get; private set; }
        internal int Radius { get; private set; }

        internal const string RadiusName = "Radius";
        internal const string IdName = "Id";

        #endregion

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            helper.SetAttribute(DummyModel.RadiusName, this.Radius);
            helper.SetAttribute(DummyModel.IdName, this.Identifier);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(element);
            this.Radius = helper.ReadInteger(DummyModel.RadiusName);
            this.Identifier = helper.ReadInteger(DummyModel.IdName);
        }

        #endregion
    }

    class DummyWorkspace : IUndoRedoRecorderClient
    {
        private List<DummyModel> models = new List<DummyModel>();
        private UndoRedoRecorder undoRecorder = null;

        #region Public Class Operational Methods

        internal DummyWorkspace()
        {
            undoRecorder = new UndoRedoRecorder(this);
        }

        internal void AddModel(DummyModel model)
        {
            models.Add(model);
            undoRecorder.BeginActionGroup();
            undoRecorder.RecordCreationForUndo(model);
            undoRecorder.EndActionGroup();
        }

        internal void ModifyModel(int identifier)
        {
            DummyModel model = GetModel(identifier);
            undoRecorder.BeginActionGroup();
            undoRecorder.RecordModificationForUndo(model);
            undoRecorder.EndActionGroup();
            model.DoubleRadius();
        }

        internal void RemoveModel(int identifier)
        {
            DummyModel model = GetModel(identifier);
            undoRecorder.BeginActionGroup();
            undoRecorder.RecordDeletionForUndo(model);
            undoRecorder.EndActionGroup();
            models.Remove(model);
        }

        internal void RemoveModels(int []identifiers)
        {
            undoRecorder.BeginActionGroup();

            foreach (int identifier in identifiers)
            {
                DummyModel model = GetModel(identifier);
                undoRecorder.RecordDeletionForUndo(model);
                models.Remove(model);
            }

            undoRecorder.EndActionGroup();
        }

        internal DummyModel GetModel(int identifier)
        {
            return models.Find((x)=>(x.Identifier == identifier));
        }

        internal UndoRedoRecorder Recorder { get { return undoRecorder; } }

        #endregion

        #region IUndoRedoRecorderClient Members

        public void DeleteModel(XmlElement modelData)
        {
            XmlElementHelper helper = new XmlElementHelper(modelData);
            int identifier = helper.ReadInteger(DummyModel.IdName);
            models.RemoveAll((x) => (x.Identifier == identifier));
        }

        public void ReloadModel(XmlElement modelData)
        {
            XmlElementHelper helper = new XmlElementHelper(modelData);
            int identifier = helper.ReadInteger(DummyModel.IdName);
            DummyModel model = models.First((x) => (x.Identifier == identifier));
            model.Deserialize(modelData, SaveContext.Undo);
        }

        public void CreateModel(XmlElement modelData)
        {
            DummyModel model = DummyModel.CreateBlankInstance();
            model.Deserialize(modelData, SaveContext.Undo);
            models.Add(model);
        }

        public ModelBase GetModelForElement(XmlElement modelData)
        {
            XmlElementHelper helper = new XmlElementHelper(modelData);
            int identifier = helper.ReadInteger(DummyModel.IdName);
            return (models.Find((x) => (x.Identifier == identifier)));
        }

        #endregion
    }

    #endregion

    internal class UndoRedoRecorderTests
    {
        private DummyWorkspace workspace = null;
        private UndoRedoRecorder recorder = null;

        [SetUp]
        public void SetupTests()
        {
            workspace = new DummyWorkspace();
            recorder = workspace.Recorder;
        }

        [TearDown]
        public void TearDownTests()
        {
            workspace = null;
        }

        [Test]
        public void TestDefaultRecorderStates()
        {
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);
        }

        [Test]
        public void TestConstructor()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                UndoRedoRecorder temp = new UndoRedoRecorder(null);
            });
        }

        [Test]
        public void TestBeginActionGroup00()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                recorder.BeginActionGroup();
                recorder.BeginActionGroup(); // Exception.
            });
        }

        [Test]
        public void TestBeginActionGroup01()
        {
            recorder.BeginActionGroup();
            recorder.EndActionGroup();
            recorder.BeginActionGroup();
            recorder.EndActionGroup(); // Successful.
        }

        [Test]
        public void TestEndActionGroup00()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                recorder.EndActionGroup(); // Without begin.
            });
        }

        [Test]
        public void TestEndActionGroup01()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                recorder.BeginActionGroup();
                recorder.EndActionGroup();
                recorder.EndActionGroup(); // Without begin.
            });
        }

        [Test]
        public void TestCreationUndoRedo()
        {
            // Ensure the recorder is in its default states.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Add a model into workspace, make sure it exists.
            workspace.AddModel(new DummyModel(1, 10));
            Assert.AreNotEqual(null, workspace.GetModel(1));

            // Make sure we can now undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo the creation.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the creation has been undone.
            Assert.AreEqual(null, workspace.GetModel(1));

            recorder.Redo(); // Redo the creation.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Make sure the creation has been redone.
            Assert.AreNotEqual(null, workspace.GetModel(1));
        }

        [Test]
        public void TestDeletionUndoRedo()
        {
            // Ensure the recorder is in its default states.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Add a model into workspace, make sure it exists.
            workspace.AddModel(new DummyModel(1, 10));
            Assert.AreNotEqual(null, workspace.GetModel(1));

            // Make sure we can now undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Delete the inserted model and make sure it is gone.
            workspace.RemoveModel(1);
            Assert.AreEqual(null, workspace.GetModel(1));

            // Make sure we can now undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo the deletion (undo's still possible).
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the deletion has been undone.
            Assert.AreNotEqual(null, workspace.GetModel(1));

            recorder.Undo(); // Undo the creation.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the creation has been undone.
            Assert.AreEqual(null, workspace.GetModel(1));

            recorder.Redo(); // Redo the creation (redo's still possible).
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the creation has been redone.
            Assert.AreNotEqual(null, workspace.GetModel(1));

            recorder.Redo(); // Redo the deletion.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Make sure the model has been deleted.
            Assert.AreEqual(null, workspace.GetModel(1));
        }

        [Test]
        public void TestDeletionsUndoRedo()
        {
            // Ensure the recorder is in its default states.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Add models into workspace, make sure they exist.
            workspace.AddModel(new DummyModel(1, 10));
            workspace.AddModel(new DummyModel(2, 20));
            Assert.AreNotEqual(null, workspace.GetModel(1));
            Assert.AreNotEqual(null, workspace.GetModel(2));

            // Make sure we can now undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Delete the inserted models and make sure they're gone.
            workspace.RemoveModels(new int[] { 1, 2 });
            Assert.AreEqual(null, workspace.GetModel(1));
            Assert.AreEqual(null, workspace.GetModel(2));

            // Make sure we can now undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo the deletion (undo's still possible).
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the deletion has been undone.
            Assert.AreNotEqual(null, workspace.GetModel(1));
            Assert.AreNotEqual(null, workspace.GetModel(2));

            recorder.Undo(); // Undo the creation.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the creation of '2' has been undone.
            Assert.AreNotEqual(null, workspace.GetModel(1));
            Assert.AreEqual(null, workspace.GetModel(2));

            recorder.Undo(); // Undo the creation.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the creation of '1' has been undone.
            Assert.AreEqual(null, workspace.GetModel(1));
            Assert.AreEqual(null, workspace.GetModel(2));

            recorder.Redo(); // Redo the creation of '1'.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the creation of '1' has been redone.
            Assert.AreNotEqual(null, workspace.GetModel(1));
            Assert.AreEqual(null, workspace.GetModel(2));

            recorder.Redo(); // Redo the creation of '2'.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the creation of '2' has been redone.
            Assert.AreNotEqual(null, workspace.GetModel(1));
            Assert.AreNotEqual(null, workspace.GetModel(2));

            recorder.Redo(); // Redo the deletion.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Make sure the model has been deleted.
            Assert.AreEqual(null, workspace.GetModel(1));
            Assert.AreEqual(null, workspace.GetModel(2));
        }

        [Test]
        public void TestModificationUndoRedo00()
        {
            // Ensure the recorder is in its default states.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Add a model into workspace, make sure it exists.
            workspace.AddModel(new DummyModel(1, 10));
            DummyModel inserted = workspace.GetModel(1);
            Assert.AreNotEqual(null, inserted);
            Assert.AreEqual(10, inserted.Radius);

            // Make sure we can now undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Double the radius property...
            workspace.ModifyModel(1);
            DummyModel modified = workspace.GetModel(1);
            Assert.AreNotEqual(null, modified);
            Assert.AreEqual(20, modified.Radius);

            // Make sure we can still undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo the modification (undo's still possible).
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Make sure the modification has been undone
            DummyModel undone = workspace.GetModel(1);
            Assert.AreNotEqual(null, undone);
            Assert.AreEqual(10, undone.Radius);

            recorder.Redo(); // Redo the modification.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Make sure the modification has been undone
            DummyModel redone = workspace.GetModel(1);
            Assert.AreNotEqual(null, redone);
            Assert.AreEqual(20, redone.Radius);
        }

        [Test]
        public void TestModificationUndoRedo01()
        {
            // Add a model into workspace, make sure it exists.
            workspace.AddModel(new DummyModel(1, 10));
            DummyModel model = workspace.GetModel(1);
            Assert.AreEqual(10, model.Radius);

            workspace.ModifyModel(1); // Double radius to 20.
            Assert.AreEqual(20, workspace.GetModel(1).Radius);

            workspace.ModifyModel(1); // Double radius to 40.
            Assert.AreEqual(40, workspace.GetModel(1).Radius);

            recorder.Undo(); // Should go back to 20.
            Assert.AreEqual(20, workspace.GetModel(1).Radius);

            recorder.Redo(); // Should go back to 40.
            Assert.AreEqual(40, workspace.GetModel(1).Radius);

            recorder.Undo(); // Should go back to 20.
            Assert.AreEqual(20, workspace.GetModel(1).Radius);

            recorder.Undo(); // Should go back to 10.
            Assert.AreEqual(10, workspace.GetModel(1).Radius);

            recorder.Redo(); // Should go back to 20.
            Assert.AreEqual(20, workspace.GetModel(1).Radius);

            recorder.Undo(); // Should go back to 10.
            Assert.AreEqual(10, workspace.GetModel(1).Radius);

            recorder.Undo(); // Should undo creation.
            Assert.AreEqual(null, workspace.GetModel(1));

            recorder.Redo(); // Should redo creation.
            Assert.AreEqual(10, workspace.GetModel(1).Radius);
        }

        [Test]
        public void TestRedoStackWipeOut()
        {
            // Add a model into workspace, make sure it exists.
            workspace.AddModel(new DummyModel(1, 10));
            DummyModel model = workspace.GetModel(1);
            Assert.AreEqual(10, model.Radius);

            // Only undo should be enabled.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo creation.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            // Scenario 1: Creating a new model while 
            // redo-stack is non-empty wipes the redo stack out.
            workspace.AddModel(new DummyModel(2, 10));
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo); // Redo stack wiped out.

            workspace.ModifyModel(2); // Modify the model once.
            Assert.AreEqual(20, workspace.GetModel(2).Radius);
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            workspace.ModifyModel(2); // Modify the model once more.
            Assert.AreEqual(40, workspace.GetModel(2).Radius);
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo the second modification.
            Assert.AreEqual(20, workspace.GetModel(2).Radius);
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo); // We can now redo.

            // Scenario 2: Modifying an existing model while 
            // redo-stack is non-empty wipes the redo stack out.
            workspace.ModifyModel(2); // Push another modification.
            Assert.AreEqual(40, workspace.GetModel(2).Radius);
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo); // Redo stack wiped out.

            workspace.RemoveModel(2); // Delete the model.
            Assert.AreEqual(null, workspace.GetModel(2));
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo deletion.
            Assert.AreEqual(40, workspace.GetModel(2).Radius);
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo); // Redo stack is back.

            // Scenario 3: Deleting an existing model while 
            // redo-stack is non-empty wipes the redo stack out.
            workspace.RemoveModel(2); // Delete the model again.
            Assert.AreEqual(null, workspace.GetModel(2));
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo); // Redo stack wiped out.
        }

        [Test]
        public void TestClearingStacks00()
        {
            // Ensure the recorder is in its default states.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            // Create two models and undo once (so both undo-redo are enabled).
            workspace.AddModel(new DummyModel(1, 10));
            workspace.AddModel(new DummyModel(2, 20));

            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo();
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);

            recorder.Clear(); // Clear recorded undo/redo actions.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);
        }

        [Test]
        public void TestClearingStacks01()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                recorder.BeginActionGroup();
                recorder.Clear(); // Clearing with an open group.
            });
        }

        [Test]
        public void TestPopFromUndoGroup()
        {
            //Assert that it cannot pop from an empty undostack
            Assert.Throws<InvalidOperationException>(() => { recorder.PopFromUndoGroup(); });

            //Add models
            workspace.AddModel(new DummyModel(1, 10));
            workspace.AddModel(new DummyModel(2, 10));

            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo();

            //Assert that there was an Action Group that was just pushed on top of the undo stack
            Assert.Throws<InvalidOperationException>(() => { recorder.PopFromUndoGroup(); });
        }
    }

    internal class SerializationTests : DynamoUnitTest
    {
        [Test]
        public void TestBasicAttributes()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //model.CreateNode(400, 100, "Dynamo.Nodes.Addition");
            //var sumNode = Controller.DynamoViewModel.Model.Nodes[0] as Addition;

            ////Assert inital values
            //Assert.AreEqual(400, sumNode.X);
            //Assert.AreEqual(100, sumNode.Y);
            //Assert.AreEqual("Dynamo.Nodes.Addition", sumNode.GetType().ToString());
            //Assert.AreEqual("Add", sumNode.NickName);
            //Assert.AreEqual(LacingStrategy.Longest, sumNode.ArgumentLacing);
            //Assert.AreEqual(true, sumNode.IsVisible);
            //Assert.AreEqual(true, sumNode.IsUpstreamVisible);
            //Assert.AreEqual(true, sumNode.InteractionEnabled);
            //Assert.AreEqual(ElementState.Dead, sumNode.State);

            ////Serialize node and then change values
            //XmlDocument xmlDoc = new XmlDocument();
            //XmlElement serializedEl = sumNode.Serialize(xmlDoc, SaveContext.Undo);
            //sumNode.X = 250;
            //sumNode.Y = 0;
            //sumNode.NickName = "TestNode";
            //sumNode.ArgumentLacing = LacingStrategy.CrossProduct;
            //sumNode.IsVisible = false;
            //sumNode.IsUpstreamVisible = false;
            //sumNode.InteractionEnabled = false;
            //sumNode.State = ElementState.Active;

            ////Assert New Changes
            //Assert.AreEqual(250, sumNode.X);
            //Assert.AreEqual(0, sumNode.Y);
            //Assert.AreEqual("TestNode", sumNode.NickName);
            //Assert.AreEqual(LacingStrategy.CrossProduct, sumNode.ArgumentLacing);
            //Assert.AreEqual(false, sumNode.IsVisible);
            //Assert.AreEqual(false, sumNode.IsUpstreamVisible);
            //Assert.AreEqual(false, sumNode.InteractionEnabled);
            //Assert.AreEqual(ElementState.Active, sumNode.State);

            ////Deserialize and Assert Old values
            //sumNode.Deserialize(serializedEl, SaveContext.Undo);
            //Assert.AreEqual(400, sumNode.X);
            //Assert.AreEqual(100, sumNode.Y);
            //Assert.AreEqual("Dynamo.Nodes.Addition", sumNode.GetType().ToString());
            //Assert.AreEqual("Add", sumNode.NickName);
            //Assert.AreEqual(LacingStrategy.Longest, sumNode.ArgumentLacing);
            //Assert.AreEqual(true, sumNode.IsVisible);
            //Assert.AreEqual(true, sumNode.IsUpstreamVisible);
            //Assert.AreEqual(true, sumNode.InteractionEnabled);
            //Assert.AreEqual(ElementState.Dead, sumNode.State);

            Assert.Inconclusive("Porting : Addition");
        }

        [Test]
        public void TestDoubleInput()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //model.CreateNode(400, 0, "Number");

            //var numNode = Controller.DynamoViewModel.Model.Nodes[0] as DoubleInput;
            //numNode.Value = "0.0";
            //numNode.X = 400; //To check if base Serialization method is being called

            ////Assert initial values
            //Assert.AreEqual(400, numNode.X);
            //Assert.AreEqual("0.0", numNode.Value);

            ////Serialize node and then change values
            //XmlDocument xmlDoc = new XmlDocument();
            //XmlElement serializedEl = numNode.Serialize(xmlDoc, SaveContext.Undo);
            //numNode.X = 250;
            //numNode.Value = "4";

            ////Assert new changes
            //Assert.AreEqual(250, numNode.X);
            //Assert.AreEqual("4", numNode.Value);

            ////Deserialize and aasert old values
            //numNode.Deserialize(serializedEl, SaveContext.Undo);
            //Assert.AreEqual(400, numNode.X);
            //Assert.AreEqual("0.0", numNode.Value);

            Assert.Inconclusive("Porting : DoubleInput");
        }

        [Test]
        public void TestDoubleSliderInput()
        {
            //var model = dynSettings.Controller.DynamoModel;
            //model.CreateNode(400, 0, "Number Slider");

            //var numNode = Controller.DynamoViewModel.Model.Nodes[0] as DoubleSliderInput;
            //numNode.X = 400; //To check if NodeModel base Serialization method is being called
            //numNode.Value = 50.0; //To check if Double class's Serialization methods work
            //numNode.Max = 100.0;
            //numNode.Min = 0.0;

            ////Assert initial values
            //Assert.AreEqual(400, numNode.X);
            //Assert.AreEqual(50.0, numNode.Value);
            //Assert.AreEqual(0.0, numNode.Min);
            //Assert.AreEqual(100.0, numNode.Max);

            ////Serialize node and then change values
            //XmlDocument xmlDoc = new XmlDocument();
            //XmlElement serializedEl = numNode.Serialize(xmlDoc, SaveContext.Undo);
            //numNode.X = 250;
            //numNode.Value = 4.0;
            //numNode.Max = 189.0;
            //numNode.Min = 2.0;

            ////Assert new changes
            //Assert.AreEqual(250, numNode.X);
            //Assert.AreEqual(4.0, numNode.Value);
            //Assert.AreEqual(2.0, numNode.Min);
            //Assert.AreEqual(189.0, numNode.Max);

            ////Deserialize and aasert old values
            //numNode.Deserialize(serializedEl, SaveContext.Undo);
            //Assert.AreEqual(400, numNode.X);
            //Assert.AreEqual(50.0, numNode.Value);
            //Assert.AreEqual(0.0, numNode.Min);
            //Assert.AreEqual(100.0, numNode.Max);

            Assert.Inconclusive("Porting : DoubleSliderInput");
        }

        [Test]
        public void TestBool()
        {
            var model = dynSettings.Controller.DynamoModel;
            model.CreateNode(0, 0, "Boolean");

            var boolNode = Controller.DynamoViewModel.Model.Nodes[0] as DSCoreNodesUI.BoolSelector;
            boolNode.Value = false;
            boolNode.X = 400; //To check if base Serialization method is being called

            //Assert initial values
            Assert.AreEqual(400, boolNode.X);
            Assert.AreEqual(false, boolNode.Value);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = boolNode.Serialize(xmlDoc, SaveContext.Undo);
            boolNode.X = 250;
            boolNode.Value = true;

            //Assert new changes
            Assert.AreEqual(250, boolNode.X);
            Assert.AreEqual(true, boolNode.Value);

            //Deserialize and aasert old values
            boolNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, boolNode.X);
            Assert.AreEqual(false, boolNode.Value);
        }

        [Test]
        public void TestStringInput()
        {
            //var strNode = new StringInput();
            //strNode.Value = "Enter";
            //strNode.X = 400; //To check if base Serialization method is being called

            ////Assert initial values
            //Assert.AreEqual(400, strNode.X);
            //Assert.AreEqual("Enter", strNode.Value);

            ////Serialize node and then change values
            //XmlDocument xmlDoc = new XmlDocument();
            //XmlElement serializedEl = strNode.Serialize(xmlDoc, SaveContext.Undo);
            //strNode.X = 250;
            //strNode.Value = "Exit";

            ////Assert new changes
            //Assert.AreEqual(250, strNode.X);
            //Assert.AreEqual("Exit", strNode.Value);

            ////Deserialize and aasert old values
            //strNode.Deserialize(serializedEl, SaveContext.Undo);
            //Assert.AreEqual(400, strNode.X);
            //Assert.AreEqual("Enter", strNode.Value);

            Assert.Inconclusive("Porting : StringInput");
        }

        [Test]
        public void TestStringFileName()
        {
            //// "StringDirectory" class validates the directory name, so here we use one that we 
            //// know for sure exists so the validation process won't turn it into empty string.
            //var validFilePath = Assembly.GetExecutingAssembly().Location;
            //var validDirectoryName = Path.GetDirectoryName(validFilePath);

            //var strNode = new StringDirectory();
            //strNode.Value = validDirectoryName;
            //strNode.X = 400; //To check if base Serialization method is being called

            ////Assert initial values
            //Assert.AreEqual(400, strNode.X);
            //Assert.AreEqual(validDirectoryName, strNode.Value);

            ////Serialize node and then change values
            //XmlDocument xmlDoc = new XmlDocument();
            //XmlElement serializedEl = strNode.Serialize(xmlDoc, SaveContext.Undo);
            //strNode.X = 250;
            //strNode.Value = "Invalid file path";

            ////Assert new changes
            //Assert.AreEqual(250, strNode.X);
            //Assert.AreEqual("Invalid file path", strNode.Value);

            ////Deserialize and aasert old values
            //strNode.Deserialize(serializedEl, SaveContext.Undo);
            //Assert.AreEqual(400, strNode.X);
            //Assert.AreEqual(validDirectoryName, strNode.Value);

            Assert.Inconclusive("Porting : StringDirectory");
        }

        [Test]
        public void TestVariableInput()
        {
            //var listNode = new Dynamo.Nodes.NewList();
            //listNode.X = 400; //To check if base Serialization method is being called
            //listNode.InPortData.Add(new PortData("index 1", "Item Index #1", typeof(object)));
            //listNode.InPortData.Add(new PortData("index 2", "Item Index #2", typeof(object)));

            ////Assert initial values
            //Assert.AreEqual(400, listNode.X);
            //Assert.AreEqual(3, listNode.InPortData.Count);

            ////Serialize node and then change values
            //XmlDocument xmlDoc = new XmlDocument();
            //XmlElement serializedEl = listNode.Serialize(xmlDoc, SaveContext.Undo);
            //listNode.X = 250;
            //listNode.InPortData.RemoveAt(listNode.InPortData.Count - 1);

            ////Assert new changes
            //Assert.AreEqual(250, listNode.X);
            //Assert.AreEqual(2, listNode.InPortData.Count);

            ////Deserialize and aasert old values
            //listNode.Deserialize(serializedEl, SaveContext.Undo);
            //Assert.AreEqual(400, listNode.X);
            //Assert.AreEqual(3, listNode.InPortData.Count);
            //Assert.AreEqual("index 2", listNode.InPortData.ElementAt(2).NickName);

            Assert.Inconclusive("Porting : NewList");
        }

        [Test]
        public void TestSublists()
        {
            /*
            var strNode = new Sublists();
            strNode.Value = "Enter";
            strNode.X = 400; //To check if base Serialization method is being called

            //Assert initial values
            Assert.AreEqual(400, strNode.X);
            Assert.AreEqual("Enter", strNode.Value);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = strNode.Serialize(xmlDoc, SaveContext.Undo);
            strNode.X = 250;
            strNode.Value = "Exit";

            //Assert new changes
            Assert.AreEqual(250, strNode.X);
            Assert.AreEqual("Exit", strNode.Value);

            //Deserialize and aasert old values
            strNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, strNode.X);
            Assert.AreEqual("Enter", strNode.Value);
             */
            Assert.Inconclusive("Porting : Sublists");
        }

        [Test]
        public void TestFormula()
        {
            /*
            var model = dynSettings.Controller.DynamoModel;
            model.CreateNode(0, 0, "Formula");

            var formulaNode = Controller.DynamoViewModel.Model.Nodes[0] as Formula;
            formulaNode.FormulaString = "x+y";
            formulaNode.X = 400; //To check if base Serialization method is being called

            //Assert initial values
            Assert.AreEqual(400, formulaNode.X);
            Assert.AreEqual("x+y", formulaNode.FormulaString);
            Assert.AreEqual(2, formulaNode.InPortData.Count);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = formulaNode.Serialize(xmlDoc, SaveContext.Undo);
            formulaNode.X = 250;
            formulaNode.FormulaString = "x+y+z";

            //Assert new changes
            Assert.AreEqual(250, formulaNode.X);
            Assert.AreEqual(3, formulaNode.InPortData.Count);
            Assert.AreEqual("x+y+z", formulaNode.FormulaString);

            //Deserialize and aasert old values
            formulaNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(400, formulaNode.X);
            Assert.AreEqual("x+y", formulaNode.FormulaString);
            Assert.AreEqual(2, formulaNode.InPortData.Count);
             * */

            Assert.Inconclusive("Porting : Formula");
        }

        [Test]
        public void TestFunctionNode()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_serialization\");
            string openPath = Path.Combine(examplePath, "graph function.dyn");
            string openPath2 = Path.Combine(examplePath, "GraphFunction.dyf");
            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(openPath2)!= null);
            model.Open(openPath);

            dynSettings.Controller.RunExpression(null);
            System.Threading.Thread.Sleep(500);

            // check if the node is loaded
            //Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);

            var graphNode = model.CurrentWorkspace.NodeFromWorkspace<Function>("9c8c2279-6f59-417c-8218-3b337230bd99");
            //var graphNode = (Function)model.Nodes.First(x => x is Function);

            //Assert initial values
            Assert.AreEqual(534.75, graphNode.X);
            Assert.AreEqual("07e6b150-d902-4abb-8103-79193552eee7", graphNode.Definition.FunctionId.ToString());
            Assert.AreEqual("GraphFunction", graphNode.NickName);
            Assert.AreEqual(4, graphNode.InPortData.Count);
            Assert.AreEqual("y = f(x)", graphNode.InPortData[3].NickName);

            //Serialize node and then change values
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement serializedEl = graphNode.Serialize(xmlDoc, SaveContext.Undo);
            graphNode.X = 250;
            graphNode.NickName = "NewNode";
            graphNode.InPortData.RemoveAt(graphNode.InPortData.Count - 1);

            //Assert new changes
            Assert.AreEqual(250, graphNode.X);
            Assert.AreEqual(3, graphNode.InPortData.Count);
            Assert.AreEqual("NewNode", graphNode.NickName);

            //Deserialize and aasert old values
            graphNode.Deserialize(serializedEl, SaveContext.Undo);
            Assert.AreEqual(534.75, graphNode.X);
            Assert.AreEqual(4, graphNode.InPortData.Count);
            Assert.AreEqual("GraphFunction", graphNode.NickName);
            Assert.AreEqual("y = f(x)", graphNode.InPortData[3].NickName);
        }

        [Test]
        public void TestDummyNodeInternals00()
        {
            var model = Controller.DynamoModel;
            var folder = Path.Combine(GetTestDirectory(), @"core\migration\");
            model.Open(Path.Combine(folder, "DummyNodeSample.dyn"));

            var workspace = Controller.DynamoModel.CurrentWorkspace;
            var dummyNode = workspace.NodeFromWorkspace<DSCoreNodesUI.DummyNode>(
                Guid.Parse("37bffbb9-3438-4c6c-81d6-7b41b5fb5b87"));

            Assert.IsNotNull(dummyNode);

            // Ensure all properties are loaded from file.
            Assert.AreEqual("Point.ByLuck", dummyNode.LegacyNodeName);
            Assert.AreEqual(3, dummyNode.InputCount);
            Assert.AreEqual(2, dummyNode.OutputCount);

            // Ensure all properties updated data members accordingly.
            Assert.AreEqual("Point.ByLuck", dummyNode.NickName);
            Assert.AreEqual(3, dummyNode.InPorts.Count);
            Assert.AreEqual(2, dummyNode.OutPorts.Count);
        }

        [Test]
        public void TestDummyNodeInternals01()
        {
            var model = Controller.DynamoModel;
            var folder = Path.Combine(GetTestDirectory(), @"core\migration\");
            model.Open(Path.Combine(folder, "DummyNodeSample.dyn"));

            var workspace = Controller.DynamoModel.CurrentWorkspace;
            var dummyNode = workspace.NodeFromWorkspace<DSCoreNodesUI.DummyNode>(
                Guid.Parse("37bffbb9-3438-4c6c-81d6-7b41b5fb5b87"));

            Assert.IsNotNull(dummyNode);
            Assert.AreEqual(3, dummyNode.InPorts.Count);
            Assert.AreEqual(2, dummyNode.OutPorts.Count);

            var xmlDocument = new XmlDocument();
            var element = dummyNode.Serialize(xmlDocument, SaveContext.Undo);

            // Deserialize more than once should not cause ports to accumulate.
            dummyNode.Deserialize(element, SaveContext.Undo);
            dummyNode.Deserialize(element, SaveContext.Undo);
            dummyNode.Deserialize(element, SaveContext.Undo);

            Assert.AreEqual(3, dummyNode.InPorts.Count);
            Assert.AreEqual(2, dummyNode.OutPorts.Count);
        }
    }
}
