using System.Xml;
using Dynamo.Graph;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using TestUINodes;
using UnitsUI;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class UnitsUITests : DynamoTestUIBase
    {
        private LengthFromString lengthFromString;
        private MeasurementInputBaseConcrete measurementInputBase;
        private AssemblyHelper assemblyHelper;

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        [SetUp]
        public void TestsSetup()
        {
            lengthFromString = new LengthFromString();
            measurementInputBase = new MeasurementInputBaseConcrete();
        }

        [Test]
        public void MigrateLengthFromFeetToMetersTest()
        {
            var testDocument = new XmlDocument();
            XmlElement root = testDocument.CreateElement("Main");
            XmlElement systemDouble = testDocument.CreateElement("System.Double");
            
            //Value is 1 feet
            systemDouble.SetAttribute("value", "1");
            root.AppendChild(systemDouble);

            lengthFromString.MigrateLengthFromFeetToMeters(root);

            string oneFeetInMeters = "0.304800000001219";

            Assert.AreEqual(oneFeetInMeters, systemDouble.Attributes[0].Value);
        }

        [Test]
        public void SerializeCoreTest()
        {
            //Loads a value to check
            measurementInputBase.Value = 20;

            //Serializes node into xml
            var testDocument = new XmlDocument();
            testDocument.LoadXml("<ElementTag/>");
            var testElement = testDocument.DocumentElement;
            measurementInputBase.SerializeCore(testElement, SaveContext.None);

            var attributeValue = testElement.FirstChild.Attributes[0].Value;

            Assert.AreEqual("20", attributeValue);
        }

        [Test]
        public void UpdateValueCoreTest()
        {
            measurementInputBase.Value = 20;

            //Update Value
            var updateParam = new UpdateValueParams("Value", "1");
            var result = measurementInputBase.UpdateValueCore(updateParam);
            Assert.IsTrue(result);
        }

        [Test]
        public void PreferenceSettings_PropertyChanged()
        {
            Open(@"UI\UnitsUITests.dyn");
            ViewModel.HomeSpace.RunSettings.RunType = RunType.Manual;

            Run();
            var node = Model.CurrentWorkspace.NodeFromWorkspace<LengthFromString>("5705381c277c4a86bf6650aeda12a468");
            Assert.IsFalse(node.IsModified);

            Model.PreferenceSettings.NumberFormat = "0.0";

            node = Model.CurrentWorkspace.NodeFromWorkspace<LengthFromString>("5705381c277c4a86bf6650aeda12a468");
            Assert.IsTrue(node.IsModified);
        }
    }
}
