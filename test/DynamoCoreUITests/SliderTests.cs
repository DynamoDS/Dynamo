﻿using System;
using System.IO;
using System.Reflection;

using DSCoreNodesUI.Input;

using NUnit.Framework;
using DoubleSlider = DSCoreNodesUI.Input.DoubleSlider;

namespace DynamoCoreUITests
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
            var slider = new IntegerSlider();
            Assert.NotNull(slider);
        }
    }
}
