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

        #endregion

        #region Serialization/Deserialization Methods

        protected override XmlNode SerializeCore(XmlDocument xmlDocument)
        {
            string typeName = this.GetType().ToString();
            XmlElement element = xmlDocument.CreateElement(typeName);
            XmlElementHelper helper = new XmlElementHelper(element);

            helper.SetAttribute("Radius", this.Radius);
            helper.SetAttribute("Id", this.Identifier);
            return element;
        }

        protected override void DeserializeCore(XmlNode xmlNode)
        {
            XmlElement element = xmlNode as XmlElement;
            XmlElementHelper helper = new XmlElementHelper(element);
            this.Radius = helper.ReadInteger("Radius");
            this.Identifier = helper.ReadInteger("Id");
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
            undoRecorder.RecordCreationForUndo(model);
        }

        internal void ModifyModel(int identifier)
        {
            DummyModel model = GetModel(identifier);
            undoRecorder.RecordModificationForUndo(model);
            model.DoubleRadius();
        }

        internal void RemoveModel(int identifier)
        {
            DummyModel model = GetModel(identifier);
            undoRecorder.RecordDeletionForUndo(model);
            models.Remove(model);
        }

        internal DummyModel GetModel(int identifier)
        {
            return models.First((x) => (x.Identifier == identifier));
        }

        internal UndoRedoRecorder Recorder { get { return undoRecorder; } }

        #endregion

        #region IUndoRedoRecorderClient Members

        public void DeleteModel(XmlElement modelData)
        {
            XmlElementHelper helper = new XmlElementHelper(modelData);
            int identifier = helper.ReadInteger("id");
            models.RemoveAll((x) => (x.Identifier == identifier));
        }

        public void ReloadModel(XmlElement modelData)
        {
            XmlElementHelper helper = new XmlElementHelper(modelData);
            int identifier = helper.ReadInteger("id");
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
    }
}
