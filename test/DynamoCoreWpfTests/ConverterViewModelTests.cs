using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using CoreNodeModels;
using CoreNodeModels.Input;
using CoreNodeModelsWpf.Controls;
using Dynamo.Tests;
using Dynamo.Utilities;
using DynamoCoreWpfTests;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace CoreNodeModelsWpf
{
    [TestFixture]
    public class ConverterViewModelTests : DynamoTestUIBase
    {
        private AssemblyHelper assemblyHelper;

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            SetupTests();
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DynamoConversions.dll");
            libraries.Add("DynamoUnits.dll");
            libraries.Add("CoreNodeModelsWpf.dll");
            base.GetLibrariesToPreload(libraries);
        }

        public void SetupTests()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var moduleRootFolder = Path.GetDirectoryName(assemblyPath);

            var resolutionPaths = new[]
            {
                // The CoreNodeModelsWpf.dll needed is under "nodes" folder.
                Path.Combine(moduleRootFolder, "nodes")
            };

            assemblyHelper = new AssemblyHelper(moduleRootFolder, resolutionPaths);
            AppDomain.CurrentDomain.AssemblyResolve += assemblyHelper.ResolveAssembly;
        }

        [TestFixtureTearDown]
        public void RunAfterAllTests()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= assemblyHelper.ResolveAssembly;
            assemblyHelper = null;
        }

        /// <summary>
        /// This test method will execute the next methods/properties from the ConverterViewModel.cs file:
        /// public ConverterViewModel(DynamoConvert model,NodeView nodeView)
        /// public ConversionMetricUnit SelectedMetricConversion
        /// public ConversionUnit SelectedFromConversion
        /// public ConversionUnit SelectedToConversion
        /// private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        /// </summary>
        [Test]
        public void ConverterViewModel_LengthInchesToCentimetersTest()
        {
            float inchesToCmFactor = (float)2.54;
            // open file
            Open(@"core\library\converterNodeTest.dyn");
            Run();

            var NumberNode = Model.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("d6b824d37d1741b6ba6b9e15a7bf14ab");
            NumberNode.Value = "200";
            var WatchNode = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("6805c0f154474e539b7c7cb0d1bf4de0");

            var nodeView = NodeViewWithGuid("b30c51f0-00ce-4fa3-96cf-dd43b0db45a6");//NodeViewOf<DynamoConverterControl>();

            //Get the list of all the comboboxes
            var comboBoxes = nodeView.inputGrid.ChildrenOfType<ComboBox>();

            var selectConversionMetricCombo = (from combo in comboBoxes where combo.Name.Equals("SelectConversionMetric") select combo).FirstOrDefault();
            var selectConversionFrom = (from combo in comboBoxes where combo.Name.Equals("SelectConversionFrom") select combo).FirstOrDefault();
            var selectConversionTo = (from combo in comboBoxes where combo.Name.Equals("SelectConversionTo") select combo).FirstOrDefault();

            selectConversionMetricCombo.SelectedItem = DynamoConversions.ConversionMetricUnit.Length;
            selectConversionFrom.SelectedItem = DynamoConversions.ConversionUnit.Inches;
            selectConversionTo.SelectedItem = DynamoConversions.ConversionUnit.Centimeters;

            //Validates that the ComboBox items were selected correctly
            Assert.AreEqual(selectConversionMetricCombo.SelectedItem, DynamoConversions.ConversionMetricUnit.Length);
            Assert.AreEqual(selectConversionFrom.SelectedItem, DynamoConversions.ConversionUnit.Inches);
            Assert.AreEqual(selectConversionTo.SelectedItem, DynamoConversions.ConversionUnit.Centimeters);

            //Validates that the conversion from Inches to Centimeters was successful
            Assert.AreEqual(inchesToCmFactor * float.Parse(NumberNode.Value), float.Parse(WatchNode.CachedValue.ToString()));
        }

        /// <summary>
        /// This test method will execute the next methods/properties from the ConverterViewModel.cs file:
        /// public bool IsSelectionFromBoxEnabled
        /// public string SelectionFromBoxToolTip
        /// </summary>
        [Test]
        public void ConverterViewModel_TooltipTest()
        {
            float inchesToCmFactor = (float)2.54;
            // open file
            Open(@"core\library\converterNodeTest.dyn");

            var WatchNode = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("6805c0f154474e539b7c7cb0d1bf4de0");

            var nodeView = NodeViewWithGuid("b30c51f0-00ce-4fa3-96cf-dd43b0db45a6");//NodeViewOf<DynamoConverterControl>();
            var NumberNode = Model.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("d6b824d37d1741b6ba6b9e15a7bf14ab");
            NumberNode.Value = "200";

            var dynamoConverterControl = nodeView.grid.ChildrenOfType<DynamoConverterControl>().FirstOrDefault();
            //Get the list of all the comboboxes
            var comboBoxes = nodeView.inputGrid.ChildrenOfType<ComboBox>();

            var selectConversionMetricCombo = (from combo in comboBoxes where combo.Name.Equals("SelectConversionMetric") select combo).FirstOrDefault();
            var selectConversionFrom = (from combo in comboBoxes where combo.Name.Equals("SelectConversionFrom") select combo).FirstOrDefault();
            var selectConversionTo = (from combo in comboBoxes where combo.Name.Equals("SelectConversionTo") select combo).FirstOrDefault();

            var directionButton = nodeView.inputGrid.ChildrenOfType<Button>().FirstOrDefault();

            //Getting the ConverterViewModel fromn the WPF DynamoConververControl
            var converterViewModel = dynamoConverterControl.DataContext as ConverterViewModel;
            converterViewModel.IsSelectionFromBoxEnabled = false;
            converterViewModel.SelectionFromBoxToolTip = "Testing Tooltip";

            selectConversionMetricCombo.SelectedItem = DynamoConversions.ConversionMetricUnit.Length;
            selectConversionFrom.SelectedItem = DynamoConversions.ConversionUnit.Inches;
            selectConversionTo.SelectedItem = DynamoConversions.ConversionUnit.Centimeters;

            Assert.IsFalse(selectConversionFrom.IsEnabled);
            Assert.AreEqual(selectConversionFrom.ToolTip.ToString(), "Testing Tooltip");

            //Validates that the conversion from Inches to Centimeters was successful
            Assert.AreEqual(inchesToCmFactor * float.Parse(NumberNode.Value), float.Parse(WatchNode.CachedValue.ToString()));
        }

        /// <summary>
        /// This test method will execute the next methods/properties from the ConverterViewModel.cs file:
        /// public List<ConversionUnit> SelectedFromConversionSource
        /// public List<ConversionUnit> SelectedToConversionSource
        /// </summary>
        [Test]
        public void ConverterViewModel_ItemsSourceCentimetersToMetersTest()
        {
            Open(@"core\library\converterNodeTest.dyn");

            var WatchNode = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("6805c0f154474e539b7c7cb0d1bf4de0");

            var nodeView = NodeViewWithGuid("b30c51f0-00ce-4fa3-96cf-dd43b0db45a6");//NodeViewOf<DynamoConverterControl>();
            var NumberNode = Model.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("d6b824d37d1741b6ba6b9e15a7bf14ab");
            NumberNode.Value = "200";
            //Get the list of all the comboboxes
            var comboBoxes = nodeView.inputGrid.ChildrenOfType<ComboBox>();

            var selectConversionMetricCombo = (from combo in comboBoxes where combo.Name.Equals("SelectConversionMetric") select combo).FirstOrDefault();
            var selectConversionFrom = (from combo in comboBoxes where combo.Name.Equals("SelectConversionFrom") select combo).FirstOrDefault();
            var selectConversionTo = (from combo in comboBoxes where combo.Name.Equals("SelectConversionTo") select combo).FirstOrDefault();
           
            var dynamoConverterControl = nodeView.grid.ChildrenOfType<DynamoConverterControl>().FirstOrDefault();

            //Getting the ConverterViewModel fromn the WPF DynamoConververControl
            var converterViewModel = dynamoConverterControl.DataContext as ConverterViewModel;
            
            converterViewModel.SelectedFromConversionSource = new List<DynamoConversions.ConversionUnit>() { DynamoConversions.ConversionUnit.Centimeters };
            converterViewModel.SelectedFromConversion = DynamoConversions.ConversionUnit.Centimeters;

            converterViewModel.SelectedToConversionSource = new List<DynamoConversions.ConversionUnit>() { DynamoConversions.ConversionUnit.Meters };
            converterViewModel.SelectedToConversion = DynamoConversions.ConversionUnit.Meters;

            Run();

            //Validates that the convertion from Centimeters to Meters was correctly
            Assert.AreEqual(selectConversionFrom.SelectedItem, DynamoConversions.ConversionUnit.Centimeters);
            Assert.AreEqual(selectConversionTo.SelectedItem, DynamoConversions.ConversionUnit.Meters);
            Assert.AreEqual(float.Parse(NumberNode.Value)/100, float.Parse(WatchNode.CachedValue.ToString()));
        }
    }
}
