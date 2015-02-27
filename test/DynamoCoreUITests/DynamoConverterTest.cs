using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using DSCoreNodesUI.Input;
using Dynamo.Models;
using Dynamo.Wpf;
using Dynamo.Wpf.Controls;
using NUnit.Framework;
using DynamoConverter = DSCoreNodesUI.DynamoConvert;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class DynamoConverterTest
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
        public void CanConstructConverterNode()
        {
            var converter = new DynamoConverter();
            Assert.NotNull(converter);
        }

        [Test]
        public void ConverterDefaultValues()
        {
            var converter = new DynamoConverter();
            Assert.NotNull(converter);

            Assert.AreEqual("Length", converter.SelectedMetricConversion.ToString());
            Assert.AreEqual("Meters", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("Meters", converter.SelectedToConversion.ToString());
        }

        [Test]
        public void SetConverterValues()
        {
            var converter = new DynamoConverter() {SelectedMetricConversion = "Area"};
            Assert.NotNull(converter);

            Assert.AreEqual("Area", converter.SelectedMetricConversion.ToString());
            Assert.AreEqual("SquareMeter", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("SquareMeter", converter.SelectedToConversion.ToString());
        }

        [Test]
        public void ConverterItemSourceCount()
        {
            var converter = new DynamoConverter() { SelectedMetricConversion = "Volume" };
            Assert.NotNull(converter);

            Assert.AreEqual("8", ((dynamic)converter.SelectedFromConversionSource).Count.ToString());
            Assert.AreEqual("8", ((dynamic)converter.SelectedToConversionSource).Count.ToString());
        }

        [Test]
        public void ConvertSetConversionFromValue()
        {
            var converter = new DynamoConverter() { SelectedMetricConversion = "Length" }; 
            Assert.NotNull(converter);

            Assert.AreEqual("Length", converter.SelectedMetricConversion.ToString());
            Assert.AreEqual("Meters", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("Meters", converter.SelectedToConversion.ToString());

            converter.SelectedFromConversion = "Feet";
            Assert.AreEqual("Feet", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("Meters", converter.SelectedToConversion.ToString());
        }

        [Test]
        public void ConvertSetConversionToValue()
        {
            var converter = new DynamoConverter() { SelectedMetricConversion = "Volume" };
            Assert.NotNull(converter);

            Assert.AreEqual("Volume", converter.SelectedMetricConversion.ToString());
            Assert.AreEqual("CubicMeters", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("CubicMeters", converter.SelectedToConversion.ToString());

            converter.SelectedToConversion = "CubicInches";
            Assert.AreEqual("CubicMeters", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("CubicInches", converter.SelectedToConversion.ToString());
        }

        [Test]
        public void ConverterTestToggleState()
        {
            var converter = new DynamoConverter() { SelectedMetricConversion = "Length" };
            Assert.NotNull(converter);

            Assert.AreEqual("Length", converter.SelectedMetricConversion.ToString());
            Assert.AreEqual("Meters", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("Meters", converter.SelectedToConversion.ToString());

            converter.SelectedFromConversion = "Feet";
            Assert.AreEqual("Feet", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("Meters", converter.SelectedToConversion.ToString());

            converter.ToggleDropdownValues();
            Assert.AreEqual("Meters", converter.SelectedFromConversion.ToString());
            Assert.AreEqual("Feet", converter.SelectedToConversion.ToString());
        }
    }
}
