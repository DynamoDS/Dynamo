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
using DynamoConversions;
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

            Assert.AreEqual(ConversionMetricUnit.Length, converter.SelectedMetricConversion);
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedToConversion);
        }

        [Test]
        public void SetConverterValues()
        {
            var converter = new DynamoConverter() {SelectedMetricConversion =ConversionMetricUnit.Area};
            Assert.NotNull(converter);

            Assert.AreEqual(ConversionMetricUnit.Area, converter.SelectedMetricConversion);
            Assert.AreEqual(ConversionUnit.SquareMeter, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.SquareMeter, converter.SelectedToConversion);
        }

        [Test]
        public void ConverterItemSourceCount()
        {
            var converter = new DynamoConverter() { SelectedMetricConversion = ConversionMetricUnit.Volume };
            Assert.NotNull(converter);

            Assert.AreEqual("8", ((dynamic)converter.SelectedFromConversionSource).Count.ToString());
            Assert.AreEqual("8", ((dynamic)converter.SelectedToConversionSource).Count.ToString());
        }

        [Test]
        public void ConvertSetConversionFromValue()
        {
            var converter = new DynamoConverter() { SelectedMetricConversion = ConversionMetricUnit.Length }; 
            Assert.NotNull(converter);

            Assert.AreEqual(ConversionMetricUnit.Length, converter.SelectedMetricConversion);
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedToConversion);

            converter.SelectedFromConversion = ConversionUnit.Feet;
            Assert.AreEqual(ConversionUnit.Feet, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedToConversion);
        }

        [Test]
        public void ConvertSetConversionToValue()
        {
            var converter = new DynamoConverter() { SelectedMetricConversion = ConversionMetricUnit.Volume };
            Assert.NotNull(converter);

            Assert.AreEqual(ConversionMetricUnit.Volume, converter.SelectedMetricConversion);
            Assert.AreEqual(ConversionUnit.CubicMeters, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.CubicMeters, converter.SelectedToConversion);

            converter.SelectedToConversion = ConversionUnit.CubicInches;
            Assert.AreEqual(ConversionUnit.CubicMeters, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.CubicInches, converter.SelectedToConversion);
        }

        [Test]
        public void ConverterTestToggleState()
        {
            var converter = new DynamoConverter() { SelectedMetricConversion = ConversionMetricUnit.Length };
            Assert.NotNull(converter);

            Assert.AreEqual(ConversionMetricUnit.Length, converter.SelectedMetricConversion);
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedToConversion);

            converter.SelectedFromConversion = ConversionUnit.Feet;
            Assert.AreEqual(ConversionUnit.Feet, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedToConversion);

            converter.ToggleDropdownValues();
            Assert.AreEqual(ConversionUnit.Meters, converter.SelectedFromConversion);
            Assert.AreEqual(ConversionUnit.Feet, converter.SelectedToConversion);
        }
    }
}
