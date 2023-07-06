using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
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
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// This test method will validate the next properties from the SliderViewModel class:
        ///  public T Max
        ///  public T Min
        ///  public T Step
        /// </summary>
        [Test]
        public void SliderViewModel_PropertiesTest()
        {
            Open(@"core\library\NumberSliderNodeTest.dyn");

            //Getting the view of a DoubleSlider node;
            var nodeView = NodeViewWithGuid("17ef3a6e-4742-45b6-8a2e-7ec3901e7726");
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
            //Get the current min, max y step values in the TextBoxes
            var textBoxes = expander.ChildrenOfType<DynamoTextBox>();
            var minTextBox = (from textBox in textBoxes where textBox.Name.Equals("MinTb") select textBox).FirstOrDefault();
            var maxTextBox = (from textBox in textBoxes where textBox.Name.Equals("MaxTb") select textBox).FirstOrDefault();
            var stepTextBox = (from textBox in textBoxes where textBox.Name.Equals("StepTb") select textBox).FirstOrDefault();

            //Validate that the TextBoxes have the right value according to the Model
            Assert.IsNotNull(WatchNode);
            Assert.AreEqual("0", minTextBox.Text);
            Assert.AreEqual("200", maxTextBox.Text);
            Assert.AreEqual("10", stepTextBox.Text);

            //Validates the result gotten from the Watch node
            Assert.AreEqual("20", WatchNode.CachedValue.ToString());
        }

        /// <summary>
        /// This test method will validate the "public T Value" property in SliderViewModel.cs
        /// </summary>
        [Test]
        public void SliderViewModel_ValueTest()
        {
            Open(@"core\library\NumberSliderNodeTest.dyn");
            Run();

            //Getting the view of a DoubleSlider node;
            var nodeView = NodeViewWithGuid("17ef3a6e-4742-45b6-8a2e-7ec3901e7726");
            var WatchNode = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("213fc6a3f4ff415d95747fdd2d385dfd");

            DispatcherUtil.DoEvents();

            var dynamoSlider = nodeView.grid.ChildrenOfType<DynamoSlider>().FirstOrDefault();
            //Setting the Value property from the SliderViewModel
            var sliderBaseModel = dynamoSlider.DataContext as SliderViewModel<double>;
            sliderBaseModel.Value = 200.0;//Set the Max value
            sliderBaseModel.Value = -200.0;//Set the Min value
            sliderBaseModel.Value = 0.0;//Set the Value

            var expander = nodeView.inputGrid.ChildrenOfType<Expander>().FirstOrDefault();
            expander.IsExpanded = true;

            DispatcherUtil.DoEvents();
            //Get the current min, max y step values in the TextBoxes
            var textBoxes = expander.ChildrenOfType<DynamoTextBox>();
            var minTextBox = (from textBox in textBoxes where textBox.Name.Equals("MinTb") select textBox).FirstOrDefault();
            var maxTextBox = (from textBox in textBoxes where textBox.Name.Equals("MaxTb") select textBox).FirstOrDefault();
            var stepTextBox = (from textBox in textBoxes where textBox.Name.Equals("StepTb") select textBox).FirstOrDefault();

            //Validates that the max and min values were set correctly when setting the Value property (SliderViewModel)
            Assert.IsNotNull(WatchNode);
            Assert.AreEqual("-200", minTextBox.Text);
            Assert.AreEqual("200", maxTextBox.Text);
        }

        /// <summary>
        /// This test will validate that setting the string in a , dec seperator culture does not
        /// modify the value.
        /// </summary>
        [Test]
        public void SliderViewModel_ValueTest_Localized()
        {
            //change current thread culture to German.
            var deCulture = CultureInfo.CreateSpecificCulture("de-DE");

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = deCulture;
            Thread.CurrentThread.CurrentUICulture = deCulture;

            //create a slider
            var slider = new CoreNodeModels.Input.DoubleSlider();
            Model.CurrentWorkspace.AddAndRegisterNode(slider);

            DispatcherUtil.DoEvents();

            //get viewmodel.
            var nodeViews = View.NodeViewsInFirstWorkspace();
            var nodeView = nodeViews.OfNodeModelType<CoreNodeModels.Input.DoubleSlider>().FirstOrDefault();
            var dynamoSliderControl = nodeView.grid.ChildrenOfType<DynamoSlider>().FirstOrDefault();
            //Setting the Value property from the SliderViewModel
            var sliderViewModel = dynamoSliderControl.DataContext as SliderViewModel<double>;
            sliderViewModel.Value = 10.7;

            DispatcherUtil.DoEvents();

            Assert.AreEqual(10.7,slider.Value);
            //reset culture
            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUICulture;

        }
    }
}
