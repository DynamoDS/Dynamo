using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Core;
using Dynamo.Utilities;
using NUnit.Framework;

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

        protected override XmlNode SerializeCore(XmlDocument xmlDocument)
        {
            string typeName = this.GetType().ToString();
            XmlElement element = xmlDocument.CreateElement(typeName);
            XmlElementHelper helper = new XmlElementHelper(element);

            helper.SetAttribute(DummyModel.RadiusName, this.Radius);
            helper.SetAttribute(DummyModel.IdName, this.Identifier);
            return element;
        }

        protected override void DeserializeCore(XmlNode xmlNode)
        {
            XmlElement element = xmlNode as XmlElement;
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
            model.Deserialize(modelData as XmlNode);
        }

        public void CreateModel(XmlElement modelData)
        {
            DummyModel model = DummyModel.CreateBlankInstance();
            model.Deserialize(modelData as XmlNode);
            models.Add(model);
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
    }
}
