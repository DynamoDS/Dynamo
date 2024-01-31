using System;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class OpenFileCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods:
        /// static string TryFindFile(string xmlFilePath, string uriString = null)
        /// protected override void SerializeCore(XmlElement element)
        /// void ExecuteCore(DynamoModel dynamoModel)
        /// void TrackAnalytics()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestTryFindFile()
        {
            //Arrange
            string wspath = Path.Combine(TestDirectory, @"core\callsite\RebindingSingleDimension.dyn");
            var fileCommand = new DynamoModel.OpenFileCommand(wspath, false);          

            //We need a XmlDocument instance to create a new custom XmlElement
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");

            //Act
            var command = new OpenFileCommandDerivedTest(wspath);
            command.SerializeCoreTest(elemTest);
            var deserializedCommand = OpenFileCommand.DeserializeCore(elemTest);

            //We need to set up the DynamoModel inside the OpenFileCommandDerivedTest class before calling TrackAnalytics()
            command.ExecuteTest(CurrentDynamoModel);
            command.TrackAnalytics();

            //Assert
            //It will validate that the Deserialized element is valid
            Assert.IsNotNull(deserializedCommand);
        }

        /// <summary>
        ///  This test method will execute the TryFindFile (the final part of the function)
        ///  Then the dyn file passed as parameter doesn't exists
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestTryFindFileUri()
        {
            //Arrange
            //The dyn file passed as parameter doesn't exists, so a different part in TryFindFile method will be executed
            string wspath = Path.Combine(TestDirectory, @"core\callsite\RebindingSingleDimension1.dyn");

            var fileCommand = new DynamoModel.OpenFileCommand(wspath, false);

            CustomXmlDocument xmlDocument = new CustomXmlDocument();

            //We set the URI to be fake so when it's split on TryFindFile it will execute a different code section
            xmlDocument.CustomBaseURL = "http://localhost/test";
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");

            //Act
            var command = new OpenFileCommandDerivedTest(wspath);
            command.SerializeCoreTest(elemTest);

            //Assert
            Assert.Throws<FileNotFoundException>(() => OpenFileCommand.DeserializeCore(elemTest));
        }
        

        [Test]
        [Category("UnitTests")]
        public void TestTryInsertFile()
        {
            //Arrange
            string wspath = Path.Combine(TestDirectory, @"core\callsite\RebindingSingleDimension.dyn");
            var fileCommand = new DynamoModel.InsertFileCommand(wspath);

            //We need a XmlDocument instance to create a new custom XmlElement
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");

            //Act
            var command = new InsertFileCommandDerivedTest(wspath);
            command.SerializeCoreTest(elemTest);
            var deserializedCommand = InsertFileCommand.DeserializeCore(elemTest);

            //We need to set up the DynamoModel inside the InsertFileCommandDerivedTest class before calling TrackAnalytics()
            command.ExecuteTest(CurrentDynamoModel);
            command.TrackAnalytics();

            //Assert
            //It will validate that the Deserialized element is valid
            Assert.IsNotNull(deserializedCommand);
        }


        [Test]
        [Category("UnitTests")]
        public void TestInsertNodesCount()
        {
            // Home space contains 0 nodes
            var currentCount = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Count();
            Assert.AreEqual(0, currentCount);

            //Act
            string wspath = Path.Combine(TestDirectory, @"core\callsite\RebindingSingleDimension.dyn");
            this.CurrentDynamoModel.InsertFileFromPath(wspath);

            //Assert
            currentCount = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Count();
            Assert.AreNotEqual(0, currentCount);

            //Assert 2x
            this.CurrentDynamoModel.InsertFileFromPath(wspath);
            var updatedCount = this.CurrentDynamoModel.CurrentWorkspace.Nodes.Count();
            Assert.AreEqual(currentCount * 2, updatedCount);
        }

        [Test]
        [Category("UnitTests")]
        public void TestInsertNotes()
        {
            // Home space contains 0 nodes
            var currentCount = this.CurrentDynamoModel.CurrentWorkspace.Notes.Count();
            Assert.AreEqual(0, currentCount);

            //Act
            string wspath = Path.Combine(TestDirectory, @"core\callsite\insert_annotations.dyn");
            this.CurrentDynamoModel.InsertFileFromPath(wspath);

            //Assert
            currentCount = this.CurrentDynamoModel.CurrentWorkspace.Notes.Count();
            Assert.AreNotEqual(0, currentCount);

            //Assert pinned/unpinned notes
            var pinnedNotesCount = this.CurrentDynamoModel.CurrentWorkspace.Notes.Count(x => x.PinnedNode != null);
            var unpinnedNotesCount = this.CurrentDynamoModel.CurrentWorkspace.Notes.Count(x => x.PinnedNode == null);
            Assert.AreEqual(1, pinnedNotesCount, unpinnedNotesCount);
        }

        [Test]
        [Category("UnitTests")]
        public void TestInsertAnnotations()
        {
            // Home space contains 0 nodes
            var currentCount = this.CurrentDynamoModel.CurrentWorkspace.Annotations.Count();
            Assert.AreEqual(0, currentCount);

            //Act
            string wspath = Path.Combine(TestDirectory, @"core\callsite\insert_annotations.dyn");
            this.CurrentDynamoModel.InsertFileFromPath(wspath);

            //Assert
            currentCount = this.CurrentDynamoModel.CurrentWorkspace.Notes.Count();
            Assert.AreEqual(2, currentCount);

            //Assert nested groups
            var nestedGroupsCount = this.CurrentDynamoModel.CurrentWorkspace.Annotations.Count(x => x.Nodes.Count(y => y is Dynamo.Graph.Annotations.AnnotationModel) > 0);
            Assert.AreEqual(1, nestedGroupsCount);
        }
    }
    
    /// <summary>
    /// This test class was created due that we need to set the baseURL but the property is private, then one way is using a derived class
    /// </summary>
    public class CustomXmlDocument : XmlDocument
    {
        public string CustomBaseURL { get; set; }

        public override string BaseURI
        {
            get { return this.CustomBaseURL; }
        }
    }

    /// <summary>
    /// This test class was created with the purpose of execute in the Derived class the next protected methods 
    /// void ExecuteCore(DynamoModel dynamoModel)
    /// void SerializeCore(XmlElement element)
    /// </summary>
    class OpenFileCommandDerivedTest : OpenFileCommand
    {
        public OpenFileCommandDerivedTest(string filePath) : base(filePath, false)
        {
        }

        public void ExecuteTest(DynamoModel dynamoModel)
        {
            ExecuteCore(dynamoModel);
        }

        public void SerializeCoreTest(System.Xml.XmlElement element)
        {
            SerializeCore(element);
        }

        protected override void ExecuteCore(DynamoModel dynamoModel)
        {
            try
            {
                //This will raise a NotImplementedException since ExecuteCore should not be implemented for a PausePlaybackCommand 
                base.ExecuteCore(dynamoModel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void SerializeCore(System.Xml.XmlElement element)
        {
            base.SerializeCore(element);
        }
    }

    /// <summary>
    /// This test class was created with the purpose of execute in the Derived class the next protected methods 
    /// void ExecuteCore(DynamoModel dynamoModel)
    /// void SerializeCore(XmlElement element)
    /// </summary>
    class InsertFileCommandDerivedTest : InsertFileCommand
    {
        public InsertFileCommandDerivedTest(string filePath) : base(filePath)
        {
        }

        public void ExecuteTest(DynamoModel dynamoModel)
        {
            ExecuteCore(dynamoModel);
        }

        public void SerializeCoreTest(System.Xml.XmlElement element)
        {
            SerializeCore(element);
        }

        protected override void ExecuteCore(DynamoModel dynamoModel)
        {
            try
            {
                //This will raise a NotImplementedException since ExecuteCore should not be implemented for a PausePlaybackCommand 
                base.ExecuteCore(dynamoModel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void SerializeCore(System.Xml.XmlElement element)
        {
            base.SerializeCore(element);
        }
    }
}
