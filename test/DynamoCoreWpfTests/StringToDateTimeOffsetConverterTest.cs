using System;
using CoreNodeModelsWpf.Converters;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{

    [TestFixture]
    class StringToDateTimeOffsetConverterTest
    {
        private const string DefaultDateFormat = "MMMM dd, yyyy h:mm tt";

        /// <summary>
        /// This test method will validate the Convert() and ConvertBack() methods from the StringToDateTimeOffsetConverter class.
        /// </summary>
        [Test]
        public void StringToDateTimeOffsetConverter_ConvertConvertBackStringDateTest()
        {

            var dateConverter = new StringToDateTimeConverter();
            var testDate = new DateTime(2020, 12, 12, 5, 50, 00);

            //Passing a DateTime to the Convert() method
            var dateConverted = (String)dateConverter.Convert(testDate, null, null, null);

            //Validates that the conversion from DateTime to String was successful
            Assert.IsNotNull(dateConverted);
            Assert.AreEqual(dateConverted.GetType(), typeof(String));
            Assert.AreEqual(dateConverted.ToString(), "December 12, 2020 5:50 AM");

            //Passing a string with a specific data format to the ConvertBack() method
            var dateContertedBack = (DateTime)dateConverter.ConvertBack("December 12, 2020 5:50 AM", null, null, null);

            //Validates that the conversion from String to DateTime was successful
            Assert.IsNotNull(dateContertedBack);
            Assert.AreEqual(dateContertedBack.GetType(), typeof(DateTime));
            Assert.AreEqual(dateContertedBack.ToString(DefaultDateFormat), testDate.ToString(DefaultDateFormat));

        }
    }
}
