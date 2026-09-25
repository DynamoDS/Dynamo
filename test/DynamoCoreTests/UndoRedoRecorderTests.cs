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

            // Real models (NodeModel, NoteModel, AnnotationModel, ConnectorModel) all write
            // their guid when serializing for undo, and the recorder uses it to recognize a
            // model it has already recorded in the action group it is recording into. Writing
            // it here too keeps that identity check live for these tests.
            helper.SetAttribute("guid", this.GUID);
            helper.SetAttribute(DummyModel.RadiusName, this.Radius);
            helper.SetAttribute(DummyModel.IdName, this.Identifier);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            XmlElementHelper helper = new XmlElementHelper(nodeElement);
            this.GUID = helper.ReadGuid("guid", this.GUID);
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

        internal bool WasMarkedAsModified { get; private set; }

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

        public void UpdateUndoRedoStack()
        {

        }

        public void MarkAsModified()
        {
            WasMarkedAsModified = true;
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

        /// <summary>
        /// Regression test for DYN-10717: recording a modification (e.g. a node/note/group
        /// drag or resize) must notify the client so it can mark itself dirty, since that
        /// state would otherwise be silently unsavable once Save is gated on the dirty flag.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestRecordModificationForUndoMarksClientAsModified()
        {
            workspace.AddModel(new DummyModel(0, 10));
            Assert.IsFalse(workspace.WasMarkedAsModified);

            workspace.ModifyModel(0);

            Assert.IsTrue(workspace.WasMarkedAsModified);
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
            using (recorder.BeginActionGroup())
            {
                using (recorder.BeginActionGroup())
                {
                    //inner actinon gorup gets flattened into the parent
                }
            }
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
        public void TestBeginActionGroupNestedOne()
        {
            // Ensure the recorder is in its default states.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            //this should get flattened into a single undo group of two actions
            using (recorder.BeginActionGroup())
            {
                workspace.AddModel(new DummyModel(1, 10)); //nested undo group, gets flattened into parent
                using (recorder.BeginActionGroup())
                {
                    workspace.AddModel(new DummyModel(1, 10)); //further nested undo group, gets flattened together too
                }
            }

            // Make sure we can now undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo the creation.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);
        }

        [Test]
        [Category("UnitTests")]
        public void TestBeginActionGroupNestedTwo()
        {
            // Ensure the recorder is in its default states.
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            workspace.AddModel(new DummyModel(1, 10)); // this is itself an undo group

            //this should get flattened into a single undo group of two actions
            using (recorder.BeginActionGroup())
            {
                workspace.AddModel(new DummyModel(1, 10)); //nested undo group, gets flattened into parent
                using (recorder.BeginActionGroup())
                {
                    workspace.AddModel(new DummyModel(1, 10)); //further nested undo group, gets flattened together too
                }
            }

            // Make sure we can now undo.
            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo(); // Undo the creation.
            Assert.AreEqual(true, recorder.CanUndo); //there should be two undo groups, one undo still leaves another in the stack
            Assert.AreEqual(true, recorder.CanRedo);

            recorder.Undo(); // Undo the creation.
            Assert.AreEqual(false, recorder.CanUndo); //no more actions to be undone
            Assert.AreEqual(true, recorder.CanRedo);
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
            Assert.Throws<InvalidOperationException>(() => { recorder.PopActionGroupFromUndoStack(); });

            //Add models
            workspace.AddModel(new DummyModel(1, 10));
            workspace.AddModel(new DummyModel(2, 10));

            Assert.AreEqual(true, recorder.CanUndo);
            Assert.AreEqual(false, recorder.CanRedo);

            recorder.Undo();

            //Assert that there was an Action Group that was just pushed on top of the undo stack
            Assert.Throws<InvalidOperationException>(() => { recorder.PopFromUndoGroup(); });
        }

        [Test]
        [Category("UnitTests")]
        public void WhenOperationsRecordedInCoalescingScopeThenOneUndoRevertsAllOfThem()
        {
            using (recorder.BeginCoalescingScope())
            {
                workspace.AddModel(new DummyModel(1, 10));
                workspace.AddModel(new DummyModel(2, 20));
                workspace.AddModel(new DummyModel(3, 30));
            }

            recorder.Undo();

            Assert.IsNull(workspace.GetModel(1));
            Assert.IsNull(workspace.GetModel(2));
            Assert.IsNull(workspace.GetModel(3));
            Assert.AreEqual(false, recorder.CanUndo);
        }

        /// <summary>
        /// A coalescing scope must not merge the operations it spans by recording them into a
        /// single action group: RecordActionInternal keeps only the first action recorded for a
        /// given model in a group, so a modification followed by a deletion would lose the
        /// deletion and leave undo unable to bring the model back.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenModelModifiedThenDeletedInCoalescingScopeThenUndoRestoresIt()
        {
            workspace.AddModel(new DummyModel(1, 10));

            using (recorder.BeginCoalescingScope())
            {
                workspace.ModifyModel(1); // Doubles the radius to 20.
                workspace.RemoveModel(1);
            }

            Assert.IsNull(workspace.GetModel(1));

            recorder.Undo();

            var restored = workspace.GetModel(1);
            Assert.IsNotNull(restored);
            Assert.AreEqual(10, restored.Radius);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCoalescingScopeIsOpenThenUndoAndRedoDoNothing()
        {
            workspace.AddModel(new DummyModel(1, 10));

            var scope = recorder.BeginCoalescingScope();

            recorder.Undo();
            Assert.IsNotNull(workspace.GetModel(1)); // Undo was refused.

            recorder.Redo();
            Assert.AreEqual(false, recorder.CanRedo); // Redo was refused.

            scope.Dispose();

            recorder.Undo();
            Assert.IsNull(workspace.GetModel(1));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCoalescingScopeIsOpenedAndDisposedThenIsCoalescingScopeOpenTracksIt()
        {
            Assert.AreEqual(false, recorder.IsCoalescingScopeOpen);

            var scope = recorder.BeginCoalescingScope();
            Assert.AreEqual(true, recorder.IsCoalescingScopeOpen);

            scope.Dispose();
            Assert.AreEqual(false, recorder.IsCoalescingScopeOpen);
        }

        /// <summary>
        /// A coalescing scope is meant to be held across several separate calls rather than in a
        /// using block, so a caller can reach a second dispose on an error or teardown path.
        /// EndCoalescingScope throws when no scope is open, so that must be absorbed rather than
        /// surfacing as an exception or leaving the recorder unusable.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenCoalescingScopeIsDisposedTwiceThenSecondDisposeIsHarmless()
        {
            var scope = recorder.BeginCoalescingScope();
            workspace.AddModel(new DummyModel(1, 10));
            workspace.AddModel(new DummyModel(2, 20));
            scope.Dispose();

            Assert.DoesNotThrow(() => scope.Dispose());

            // The recorder is left closed, still holding the batch as a single undo step...
            Assert.AreEqual(false, recorder.IsCoalescingScopeOpen);
            recorder.Undo();
            Assert.IsNull(workspace.GetModel(1));
            Assert.IsNull(workspace.GetModel(2));
            Assert.AreEqual(false, recorder.CanUndo);

            // ...and still recording and undoing normally afterwards.
            workspace.AddModel(new DummyModel(3, 30));
            Assert.AreEqual(true, recorder.CanUndo);

            recorder.Undo();
            Assert.IsNull(workspace.GetModel(3));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenEmptyCoalescingScopeIsClosedThenNoUndoStepIsAdded()
        {
            Assert.AreEqual(false, recorder.CanUndo);

            using (recorder.BeginCoalescingScope())
            {
            }

            Assert.AreEqual(false, recorder.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenNothingRecordedInCoalescingScopeThenUndoStackIsUntouched()
        {
            workspace.AddModel(new DummyModel(1, 10));

            using (recorder.BeginCoalescingScope())
            {
            }

            Assert.AreEqual(true, recorder.CanUndo);

            recorder.Undo();

            Assert.IsNull(workspace.GetModel(1));
            Assert.AreEqual(false, recorder.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCoalescingScopesAreNestedThenOnlyTheOutermostOneMerges()
        {
            using (recorder.BeginCoalescingScope())
            {
                using (recorder.BeginCoalescingScope())
                {
                    workspace.AddModel(new DummyModel(1, 10));
                    workspace.AddModel(new DummyModel(2, 20));
                }

                Assert.AreEqual(true, recorder.IsCoalescingScopeOpen);
                workspace.AddModel(new DummyModel(3, 30));
            }

            recorder.Undo();

            Assert.IsNull(workspace.GetModel(1));
            Assert.IsNull(workspace.GetModel(2));
            Assert.IsNull(workspace.GetModel(3));
            Assert.AreEqual(false, recorder.CanUndo);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCoalescedActionGroupIsUndoneThenRedoReappliesTheWholeBatch()
        {
            using (recorder.BeginCoalescingScope())
            {
                workspace.AddModel(new DummyModel(1, 10));
                workspace.AddModel(new DummyModel(2, 20));
            }

            recorder.Undo();
            Assert.AreEqual(true, recorder.CanRedo);

            recorder.Redo();

            Assert.IsNotNull(workspace.GetModel(1));
            Assert.IsNotNull(workspace.GetModel(2));
            Assert.AreEqual(false, recorder.CanRedo);
        }

        /// <summary>
        /// A merged action group can hold a creation and a later modification of the same model,
        /// which a live action group never does. Undoing it walks the modification first, so the
        /// model is already in the redo group by the time the creation is reached; the redo group
        /// must still recreate the model rather than only try to modify one that no longer exists.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenModelCreatedThenModifiedInCoalescingScopeThenRedoRecreatesItModified()
        {
            using (recorder.BeginCoalescingScope())
            {
                workspace.AddModel(new DummyModel(1, 10));
                workspace.ModifyModel(1); // Doubles the radius to 20.
            }

            recorder.Undo();
            Assert.IsNull(workspace.GetModel(1));

            recorder.Redo();

            var recreated = workspace.GetModel(1);
            Assert.IsNotNull(recreated);
            Assert.AreEqual(20, recreated.Radius);
        }

        /// <summary>
        /// Following on from the redo above, the batch has to keep round-tripping: undoing again
        /// must remove the recreated model, not leave it behind.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenModelCreatedThenModifiedInCoalescingScopeThenUndoAfterRedoRemovesIt()
        {
            using (recorder.BeginCoalescingScope())
            {
                workspace.AddModel(new DummyModel(1, 10));
                workspace.ModifyModel(1);
            }

            recorder.Undo();
            recorder.Redo();
            recorder.Undo();

            Assert.IsNull(workspace.GetModel(1));
            Assert.AreEqual(false, recorder.CanUndo);
            Assert.AreEqual(true, recorder.CanRedo);
        }

        /// <summary>
        /// A model created and then deleted within one batch has no net effect, so neither undo
        /// nor redo of that batch may leave it behind.
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenModelCreatedThenDeletedInCoalescingScopeThenRedoDoesNotRecreateIt()
        {
            using (recorder.BeginCoalescingScope())
            {
                workspace.AddModel(new DummyModel(1, 10));
                workspace.ModifyModel(1);
                workspace.RemoveModel(1);
            }

            recorder.Undo();
            Assert.IsNull(workspace.GetModel(1));

            recorder.Redo();
            Assert.IsNull(workspace.GetModel(1));
        }
    }
}
