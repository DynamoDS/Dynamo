using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Core;
using Dynamo.Graph;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    #region Sample Test Classes

    class DummyModel : ModelBase
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

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(nodeElement);
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
            using (undoRecorder.BeginActionGroup())
            {
                undoRecorder.RecordCreationForUndo(model);
                
            }
        }

        internal void ModifyModel(int identifier)
        {
            DummyModel model = GetModel(identifier);
            using (undoRecorder.BeginActionGroup())
            {
                undoRecorder.RecordModificationForUndo(model);
            }
            model.DoubleRadius();
        }

        internal void RemoveModel(int identifier)
        {
            DummyModel model = GetModel(identifier);
            using (undoRecorder.BeginActionGroup())
            {
                undoRecorder.RecordDeletionForUndo(model);
            }
            models.Remove(model);
        }

        internal void RemoveModels(int []identifiers)
        {
            using (undoRecorder.BeginActionGroup())
            {
                foreach (int identifier in identifiers)
                {
                    DummyModel model = GetModel(identifier);
                    undoRecorder.RecordDeletionForUndo(model);
                    models.Remove(model);
                }
            }
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
        [Category("UnitTests")]
        public void TestDefaultRecorderStates()
        {
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);
        }

        [Test]
        [Category("UnitTests")]
        public void TestConstructor()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                UndoRedoRecorder temp = new UndoRedoRecorder(null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestBeginActionGroup00()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                recorder.BeginActionGroup();
                recorder.BeginActionGroup(); // Exception.
            });
        }

        [Test]
        [Category("UnitTests")]
        public void TestBeginActionGroup01()
        {
            using (recorder.BeginActionGroup()) { }
            using (recorder.BeginActionGroup()) { }
        }

        [Test]
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
        public void TestClearingStacks01()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                recorder.BeginActionGroup();
                recorder.Clear(); // Clearing with an open group.
            });
        }

        [Test]
        [Category("UnitTests")]
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
}
