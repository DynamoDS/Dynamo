using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CoreNodeModels;
using CoreNodeModels.Input;
using CoreNodeModelsWpf;
using CoreNodeModelsWpf.Controls;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class SliderViewModelTests : DynamoTestUIBase
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

        [TearDown]
        public void RunAfterAllTests()
        {
            assemblyHelper = null;
        }

        /// <summary>
        /// This test method will 
        /// </summary>
        [Test]
        public void SliderViewModel_PropertiesTest()
        {
            Open(@"core\library\NumberSliderNodeTest.dyn");

            var nodeView = NodeViewWithGuid("17ef3a6e-4742-45b6-8a2e-7ec3901e7726");//NodeViewOf<DoubleSlider>();
            var WatchNode = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("213fc6a3f4ff415d95747fdd2d385dfd");

            Run();

            DispatcherUtil.DoEvents();

            var dynamoSlider = nodeView.grid.ChildrenOfType<DynamoSlider>().FirstOrDefault();
            var sliderBaseModel = dynamoSlider.DataContext as SliderViewModel<double>;

            //Getting the model using reflection
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = sliderBaseModel.GetType().GetField("model", bindFlags);
            SliderBase<double> sliderModel = (SliderBase<double>)field.GetValue(sliderBaseModel);

            //Setting the model values
            sliderModel.Max = 200.0;
            sliderModel.Min = 0.0;
            sliderModel.Step = 10;

            sliderBaseModel.Value = 0;
            sliderBaseModel.Value += 20;

            var expander = nodeView.inputGrid.ChildrenOfType<Expander>().FirstOrDefault();
            expander.IsExpanded = true;

            DispatcherUtil.DoEvents();
            var textBoxes = expander.ChildrenOfType<DynamoTextBox>();
            var minTextBox = (from textBox in textBoxes where textBox.Name.Equals("MinTb") select textBox).FirstOrDefault();
            var maxTextBox = (from textBox in textBoxes where textBox.Name.Equals("MaxTb") select textBox).FirstOrDefault();
            var stepTextBox = (from textBox in textBoxes where textBox.Name.Equals("StepTb") select textBox).FirstOrDefault();

            Assert.IsNotNull(WatchNode);
            Assert.AreEqual("0", minTextBox.Text);
            Assert.AreEqual("200", maxTextBox.Text);
            Assert.AreEqual("10", stepTextBox.Text);

            Assert.AreEqual("20", WatchNode.CachedValue.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SliderViewModel_ValueTest()
        {

            Open(@"core\library\NumberSliderNodeTest.dyn");
            Run();

            var nodeView = NodeViewWithGuid("17ef3a6e-4742-45b6-8a2e-7ec3901e7726");//NodeViewOf<DoubleSlider>();
            var WatchNode = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("213fc6a3f4ff415d95747fdd2d385dfd");

            DispatcherUtil.DoEvents();

            var dynamoSlider = nodeView.grid.ChildrenOfType<DynamoSlider>().FirstOrDefault();
            var sliderBaseModel = dynamoSlider.DataContext as SliderViewModel<double>;
            sliderBaseModel.Value = 200.0;//Set the Max value
            sliderBaseModel.Value = -200.0;//Set the Min value
            sliderBaseModel.Value = 0.0;//Set the Value

            var expander = nodeView.inputGrid.ChildrenOfType<Expander>().FirstOrDefault();
            expander.IsExpanded = true;

            DispatcherUtil.DoEvents();
            var textBoxes = expander.ChildrenOfType<DynamoTextBox>();
            var minTextBox = (from textBox in textBoxes where textBox.Name.Equals("MinTb") select textBox).FirstOrDefault();
            var maxTextBox = (from textBox in textBoxes where textBox.Name.Equals("MaxTb") select textBox).FirstOrDefault();
            var stepTextBox = (from textBox in textBoxes where textBox.Name.Equals("StepTb") select textBox).FirstOrDefault();

            Assert.IsNotNull(WatchNode);
            Assert.AreEqual("-200", minTextBox.Text);
            Assert.AreEqual("200", maxTextBox.Text);
        }
    }
}
