using CoreNodeModels.Input;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    class NumberTests
    {

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
        public static void SettingValueToNumberSucceeds()
        {
            var number = new DoubleInput();
            number.Value = "5";
            Assert.True(number.Value == "5");
        }

        [Test]
        [Category("UnitTests")]
        public static void SettingValueToLetterSucceeds()
        {
            var number = new DoubleInput();
            number.Value = "a";
            Assert.True(number.Value == "a");
        }
    }
}
