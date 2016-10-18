using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSCore;
using CoreNodeModels.Input;

namespace DSCoreNodesTests
{
    class NumberTests
    {

        [Test]
        [Category("UnitTests")]
        public static void SettingNumericalValueToRangeExpressionFails()
        {
              var number = new DoubleInput();
            number.NumericalValue = "0..10";
            Assert.IsFalse(number.Value == "0..10");
        }

        [Test]
        [Category("UnitTests")]
        public static void SettingValueToRangeExpressionSucceeds()
        {
            var number = new DoubleInput();
            number.Value = "0..10";
            Assert.True(number.Value == "0..10");
        }

        [Test]
        [Category("UnitTests")]
        public static void validateNumericFailsOnNonNumericInputs()
        {
            var number = new DoubleInput();

            Assert.IsFalse(number.validateInput("0..10"));
            Assert.IsFalse(number.validateInput("0..10..2"));
            Assert.IsFalse(number.validateInput("0..10..#5"));
            Assert.IsFalse(number.validateInput("0.0..10..0.5"));
            Assert.IsFalse(number.validateInput("start..end"));
            Assert.IsFalse(number.validateInput("a..b"));
            Assert.IsFalse(number.validateInput("a"));

        }

        [Test]
        [Category("UnitTests")]
        public static void validateNumericPassesOnNumericInputs()
        {
            var number = new DoubleInput();

            Assert.True(number.validateInput("10"));
            Assert.True(number.validateInput("2147483647"));
            Assert.True(number.validateInput("9223372036854775807"));
            Assert.True(number.validateInput(".00000000001"));

        }
    }
}
