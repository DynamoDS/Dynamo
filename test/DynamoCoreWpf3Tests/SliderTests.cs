using System;
using System.IO;
using System.Reflection;
using System.Xml;
using CoreNodeModels.Input;
using CoreNodeModels.Properties;
using Dynamo.Graph;
using Dynamo.Models;
using NUnit.Framework;
using DoubleSlider = CoreNodeModels.Input.DoubleSlider;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class SliderTests
    {
        [SetUp]
        public void Setup()
        {
            // Add an assembly resolver to look in the nodes folder.
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Look in the nodes folder
            string assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "nodes", new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        }

        [Test]
        public void CanConstructDoubleSlider()
        {
            var slider = new DoubleSlider();
            Assert.NotNull(slider);
        }

        [Test]
        public void SliderMinNeverGreaterThanMax()
        {
            var slider = new DoubleSlider();
            Assert.NotNull(slider);

            slider.Min = 101;
            Assert.AreEqual(101.0, slider.Max);
        }

        [Test]
        public void SliderMaxNeverLessThanMin()
        {
            var slider = new DoubleSlider();
            Assert.NotNull(slider);

            slider.Max = -5;
            Assert.AreEqual(-5.0, slider.Min);
        }

        [Test]
        public void SliderValueResetsMin()
        {
            var slider = new DoubleSlider();
            Assert.NotNull(slider);

            slider.Value = -5;
            Assert.AreEqual(-5.0, slider.Min);
        }

        [Test]
        public void SliderValueResetsMax()
        {
            var slider = new DoubleSlider();
            Assert.NotNull(slider);

            slider.Value = 200;
            Assert.AreEqual(200.0, slider.Max);
        }

        [Test]
        public void CanConstructIntegerSlider()
        {
            var slider = new IntegerSlider64Bit();
            Assert.NotNull(slider);
        }

        [Test]
        public void SliderCanNotBeSetGreaterThanMaxIntValue()
        {
            var slider = new IntegerSlider64Bit();
            Assert.NotNull(slider);

            var param = new UpdateValueParams("Value", "9223372036854775807");
            slider.UpdateValue(param);

            Assert.AreEqual(slider.Value, Int64.MaxValue);
        }

        [Test]
        public void  SliderCanNotBeSetLessThanMinIntValue()
        {
            var slider = new IntegerSlider64Bit();
            Assert.NotNull(slider);

            var param = new UpdateValueParams("Value", "-9223372036854775808");
            slider.UpdateValue(param);

            Assert.AreEqual(slider.Value, Int64.MinValue);
        }

        [Test]
        public void SliderMaxResetsToIntMax()
        {
            var slider = new IntegerSlider64Bit();
            Assert.NotNull(slider);

            var param = new UpdateValueParams("Max", "9223372036854775808");
            slider.UpdateValue(param);

            Assert.AreEqual(slider.Max, Int64.MaxValue);
        }

        [Test]
        public void SliderMinResetsToIntMin()
        {
            var slider = new IntegerSlider64Bit();
            Assert.NotNull(slider);

            var param = new UpdateValueParams("Min", "-9223372036854775809");
            slider.UpdateValue(param);

            Assert.AreEqual(slider.Min, Int64.MinValue);
        }

        [Test]
        public void DeserializeCoreTest()
        {
            var slider = new IntegerSlider64Bit();
            var param = new UpdateValueParams("Min", "10");
            slider.UpdateValue(param);

            //Serializes slider into xml
            var testDocument = new XmlDocument();
            testDocument.LoadXml("<ElementTag/>");
            XmlElement xmlElement = slider.Serialize(new XmlDocument(), SaveContext.None);

            //Resets slider to constructor default
            slider = new IntegerSlider64Bit();
            Assert.AreNotEqual(10, slider.Min);

            //Recovers slider from xml
            slider.Deserialize(xmlElement, SaveContext.None);
            Assert.AreEqual(10, slider.Min);
        }

        [Test]
        public void WhenValueTextIsDecimalThenInputIsRejectedAndValueUnchanged()
        {
            var slider = new IntegerSlider64Bit();
            Assert.NotNull(slider);

            var handled = slider.UpdateValue(new UpdateValueParams("ValueText", "3.5"));

            Assert.IsFalse(handled);
            Assert.AreEqual(1, slider.Value);
            Assert.AreEqual(1, slider.Infos.Count);
            Assert.IsTrue(slider.Infos.Any(i => i.Message.Equals(Resources.IntegerSliderNonIntegerInputMessage)));
        }

        [Test]
        public void WhenValueTextIsNonNumericThenInputIsRejectedAndValueUnchanged()
        {
            var slider = new IntegerSlider64Bit();

            var handled = slider.UpdateValue(new UpdateValueParams("ValueText", "abc"));

            Assert.IsFalse(handled);
            Assert.AreEqual(1, slider.Value);
            Assert.IsTrue(slider.Infos.Any(i => i.Message.Equals(Resources.IntegerSliderNonIntegerInputMessage)));
        }

        [Test]
        public void WhenValidIntegerFollowsRejectedInputThenInfoIsCleared()
        {
            var slider = new IntegerSlider64Bit();
            slider.UpdateValue(new UpdateValueParams("ValueText", "3.5"));
            Assert.AreEqual(1, slider.Infos.Count);

            var handled = slider.UpdateValue(new UpdateValueParams("ValueText", "42"));

            Assert.IsTrue(handled);
            Assert.AreEqual(42, slider.Value);
            Assert.AreEqual(0, slider.Infos.Count);
        }

        [Test]
        public void WhenMinTextIsDecimalThenMinIsUnchanged()
        {
            var slider = new IntegerSlider64Bit();

            var handled = slider.UpdateValue(new UpdateValueParams("MinText", "0.5"));

            Assert.IsFalse(handled);
            Assert.AreEqual(0, slider.Min);
        }

        [Test]
        public void WhenMaxIsDecimalThenMaxIsUnchanged()
        {
            // "Max" (no "Text" suffix) is the property name sent by IntegerSliderSettingsControl.
            var slider = new IntegerSlider64Bit();

            var handled = slider.UpdateValue(new UpdateValueParams("Max", "100.5"));

            Assert.IsFalse(handled);
            Assert.AreEqual(100, slider.Max);
        }

        [Test]
        public void WhenStepTextIsNonNumericThenStepIsUnchanged()
        {
            var slider = new IntegerSlider64Bit();

            var handled = slider.UpdateValue(new UpdateValueParams("StepText", "one"));

            Assert.IsFalse(handled);
            Assert.AreEqual(1, slider.Step);
        }
    }
}
