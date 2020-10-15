using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Dynamo.Graph;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using TestUINodes;
using UnitsUI;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class MeasurementInputBaseTests : DynamoTestUIBase
    {
        private LengthFromString lengthFromString;
        private MeasurementInputBaseConcrete measurementInputBase;

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
        public void PreferenceSettings_PropertyChangedTest()
        {
            Open(@"core\UnitsUITests.dyn");

            var nodeView = NodeViewWithGuid("5705381c-277c-4a86-bf66-50aeda12a468");

            var editMenuItem = nodeView.MainContextMenu
                .Items;

            //editMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            Assert.IsNotNull(null);
        }

    }
}
