using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CoreNodeModels;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;
using SystemTestServices;
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
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DynamoConversions.dll");
            libraries.Add("DynamoUnits.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            base.GetLibrariesToPreload(libraries);
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

            string oneFeetInMeters = "0.3048000000012192";

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
            Assert.IsTrue(node.IsModified);
        }

        [Test]
        public void ForgeUnitsLoadsDefaultsCorrectly()
        {
            Open(@"core\units\default_unit_dropdowns.dyn");
            var node1 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "1").FirstOrDefault() as DynamoUnitConvert;
            var node2 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "2").FirstOrDefault();
            var node3 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "3").FirstOrDefault() as DSDropDownBase;
            var node4 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "4").FirstOrDefault() as DSDropDownBase;
            var node5 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "5").FirstOrDefault() as DSDropDownBase;
            Run();
            Assert.AreEqual(0, node1.CachedValue.Data);
            Assert.AreEqual("Function", node2.CachedValue.Class.Name);
            Assert.AreEqual(null, node3.CachedValue.Data);
            Assert.AreEqual(null, node4.CachedValue.Data);
            Assert.AreEqual(null, node5.CachedValue.Data);

            Assert.AreEqual("centimeters", node1.SelectedFromConversion.Name.ToLower());
            Assert.AreEqual("centimeters", node1.SelectedToConversion.Name.ToLower());
            Assert.AreEqual("length", node1.SelectedQuantityConversion.Name.ToLower());

            Assert.AreEqual(-1, node3.SelectedIndex);
            Assert.AreEqual(-1, node4.SelectedIndex);
            Assert.AreEqual(-1, node5.SelectedIndex);
        }

        [Test]
        public void ForgeUnitsEqualityInDynamoListOps()
        {
            Open(@"core\units\unit_dropdowns2.dyn");
            var listequals = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "List.Equals").FirstOrDefault();
            var stringnode = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "String from Object").FirstOrDefault();
            var allindices = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "List.AllIndicesOf").FirstOrDefault();
            var indexof = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "List.IndexOf").FirstOrDefault();
            Run();
            Assert.IsTrue((bool)(listequals.CachedValue.Data));
            Assert.AreEqual("Unit(Name = Millimeters)", stringnode.CachedValue.Data);
            Assert.AreEqual(new List<int>() { 0, 2 }, allindices.CachedValue.GetElements().Select(x=>x.Data).ToList());
            Assert.AreEqual(0, indexof.CachedValue.Data);

        }

        [Test]
        public void ForgeUnitDropdownsLoadWithGoodData()
        {
            Open(@"core\units\unit_dropdowns.dyn");
            var node1 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "1").FirstOrDefault() as DynamoUnitConvert;
            var node2 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "2").FirstOrDefault();
            var node3 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "3").FirstOrDefault() as DSDropDownBase;
            var node4 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "4").FirstOrDefault() as DSDropDownBase;
            var node5 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "5").FirstOrDefault() as DSDropDownBase;
            var node6 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "6").FirstOrDefault() as DSDropDownBase;
            var node7 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "7").FirstOrDefault() as DSDropDownBase;
            var node8 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "8").FirstOrDefault();

            Run();
            Assert.AreEqual(5000, node1.CachedValue.Data);
            Assert.AreEqual(5000, node2.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Quantity.ByTypeID("autodesk.unit.quantity:force-1.0.2"), node3.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Symbol.ByTypeID("autodesk.unit.symbol:mm-1.0.1"), node4.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Unit.ByTypeID("autodesk.unit.unit:millimeters-1.0.1"), node5.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Unit.ByTypeID("autodesk.unit.unit:meters-1.0.1"), node6.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Unit.ByTypeID("autodesk.unit.unit:millimeters-1.0.1"), node7.CachedValue.Data);
            Assert.AreEqual(11, node8.CachedValue.GetElements().Count());

            Assert.AreEqual("meters", node1.SelectedFromConversion.Name.ToLower());
            Assert.AreEqual("millimeters", node1.SelectedToConversion.Name.ToLower());
            Assert.AreEqual("length", node1.SelectedQuantityConversion.Name.ToLower());

        }

        [Test]
        //This test loads a modified .dyn to assert the behavior of units/dropdown nodes when schema data changes. 
        public void ForgeUnitDropdownsLoadWithMalformedData()
        {
            Open(@"core\units\malformed_unit_dropdowns.dyn");
            var node1 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "1").FirstOrDefault() as DynamoUnitConvert;
            var node2 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "2").FirstOrDefault();
            var node3 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "3").FirstOrDefault() as DSDropDownBase;
            var node4 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "4").FirstOrDefault() as DSDropDownBase;
            var node5 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "5").FirstOrDefault() as DSDropDownBase;
            var node6 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "6").FirstOrDefault() as DSDropDownBase;
            var node7 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "7").FirstOrDefault() as DSDropDownBase;
            var node8 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "8").FirstOrDefault();
            var node9 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "9").FirstOrDefault() as DynamoUnitConvert;

            Run();
            Assert.AreNotEqual(5000, node1.CachedValue.Data);
            Assert.AreEqual(5000, node2.CachedValue.Data);
            //we've changed both index and name, so a new item is selected.
            Assert.AreNotEqual(DynamoUnits.Quantity.ByTypeID("autodesk.unit.quantity:force-1.0.2"), node3.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Symbol.ByTypeID("autodesk.unit.symbol:mm-1.0.1"), node4.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Unit.ByTypeID("autodesk.unit.unit:millimeters-1.0.1"), node5.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Unit.ByTypeID("autodesk.unit.unit:meters-1.0.1"), node6.CachedValue.Data);
            Assert.AreEqual(DynamoUnits.Unit.ByTypeID("autodesk.unit.unit:millimeters-1.0.1"), node7.CachedValue.Data);
            Assert.AreEqual(3, node8.CachedValue.GetElements().Count());

            Assert.AreEqual(null, node1.SelectedFromConversion);
            Assert.AreEqual(null, node1.SelectedToConversion);
            Assert.AreEqual(null, node1.SelectedQuantityConversion);

        }

        [Test]
        public void ForgeUnitDropdownsLoadWithDifferentSchemaVersions()
        {
            Open(@"core\units\unit_dropdown_different_schema_version.dyn");
            var node1 = Model.CurrentWorkspace.Nodes.FirstOrDefault() as DynamoUnitConvert;
            var node2 = Model.CurrentWorkspace.Nodes.Skip(1).FirstOrDefault() as DynamoUnitConvert;

            Assert.AreEqual("british thermal units per hour", node1.SelectedFromConversion.Name.ToLower());
            Assert.AreEqual("british thermal units per hour", node1.SelectedToConversion.Name.ToLower());
            Assert.AreEqual("power", node1.SelectedQuantityConversion.Name.ToLower());

            Assert.AreEqual("pascals", node2.SelectedFromConversion.Name.ToLower());
            Assert.AreEqual("pascals", node2.SelectedToConversion.Name.ToLower());
            Assert.AreEqual("stress", node2.SelectedQuantityConversion.Name.ToLower());

        }

        [Test]
        //This test loads a modified .dyn to assert the behavior of units/dropdown nodes when schema data is completely missing. 
        public void ForgeUnitDropdownsLoadWithNoSchemas()
        {
            DynamoUnits.Utilities.SetTestEngine(@"C:\BadPath");

            Open(@"core\units\unit_dropdowns.dyn");
            var node1 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "1").FirstOrDefault() as DynamoUnitConvert;
            var node2 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "2").FirstOrDefault();
            var node3 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "3").FirstOrDefault() as DSDropDownBase;
            var node4 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "4").FirstOrDefault() as DSDropDownBase;
            var node5 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "5").FirstOrDefault() as DSDropDownBase;
            var node6 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "6").FirstOrDefault() as DSDropDownBase;
            var node7 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "7").FirstOrDefault() as DSDropDownBase;
            var node8 = Model.CurrentWorkspace.Nodes.Where(x => x.Name == "8").FirstOrDefault();

            Run();
            Assert.AreEqual(null, node1.CachedValue.Data);
            Assert.AreEqual(null, node2.CachedValue.Data);
            Assert.AreEqual(null, node3.CachedValue.Data);
            Assert.AreEqual(null, node4.CachedValue.Data);
            Assert.AreEqual(null, node5.CachedValue.Data);
            Assert.AreEqual(null, node6.CachedValue.Data);
            Assert.AreEqual(null, node7.CachedValue.Data);
            Assert.AreEqual(null, node8.CachedValue.Data);

            Assert.AreEqual(null, node1.SelectedFromConversion);
            Assert.AreEqual(null, node1.SelectedToConversion);
            Assert.AreEqual(null, node1.SelectedQuantityConversion);

            //assert all nodes are in error
            foreach(var node in Model.CurrentWorkspace.Nodes.Where(x=>!(x is CodeBlockNodeModel)))
            {
                Assert.IsTrue(node.State == Dynamo.Graph.Nodes.ElementState.Warning || node.State == Dynamo.Graph.Nodes.ElementState.PersistentWarning);
            }

            DynamoUnits.Utilities.Initialize();
        }

        [Test]
        public void ForgeUnitConversionsReactCorrectly_ToInteractions()
        {
            Open(@"core\units\unit_dropdown_different_schema_version.dyn");
            var node1 = Model.CurrentWorkspace.Nodes.FirstOrDefault() as DynamoUnitConvert;
            var node2 = Model.CurrentWorkspace.Nodes.Skip(1).FirstOrDefault() as DynamoUnitConvert;

            Assert.AreEqual("british thermal units per hour", node1.SelectedFromConversion.Name.ToLower());
            Assert.AreEqual("british thermal units per hour", node1.SelectedToConversion.Name.ToLower());
            Assert.AreEqual("power", node1.SelectedQuantityConversion.Name.ToLower());

            node1.SelectedQuantityConversion = DynamoUnits.Quantity.ByTypeID("autodesk.unit.quantity:length-1.0.4");
            Assert.AreEqual("centimeters", node1.SelectedToConversion.Name.ToLower());
            Assert.AreEqual("centimeters", node1.SelectedFromConversion.Name.ToLower());
            Assert.AreEqual("length", node1.SelectedQuantityConversion.Name.ToLower());

            node1.SelectedQuantityConversion = DynamoUnits.Quantity.ByTypeID("autodesk.unit.quantity:power-1.0.4");
            Assert.AreEqual("british thermal units per hour", node1.SelectedFromConversion.Name.ToLower());
            Assert.AreEqual("british thermal units per hour", node1.SelectedToConversion.Name.ToLower());
            Assert.AreEqual("power", node1.SelectedQuantityConversion.Name.ToLower());

        }
    }
}
