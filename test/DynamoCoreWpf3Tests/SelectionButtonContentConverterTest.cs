using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreNodeModelsWpf.Converters;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class SelectionButtonContentConverterTest
    {
        /// <summary>
        /// This test method will validate the Convert() and ConvertBack() methods from the SelectionButtonContentConverter class.
        /// </summary>
        [Test]
        public void SelectionButtonContentConverter_ConvertConvertBackStringTest()
        {

            var list = new List<object>();
            var buttonContentConverter = new SelectionButtonContentConverter();

            var selectConverted = buttonContentConverter.Convert(list, null, null, null);

            //Validates that the Convert function returned the expected result when passing a IEnumerable<object>.
            Assert.IsNotNull(selectConverted);
            Assert.AreEqual(selectConverted.ToString(), "Select");

            //Validates that the Convert function returned the expected result when passing a string object.
            var changeConterted = buttonContentConverter.Convert("test1", null, null, null);
            Assert.IsNotNull(changeConterted);
            Assert.AreEqual(changeConterted.ToString(), "Change");

            //Validates that the ConvertBack function return null no matter the parameter passed
            Assert.IsNull(buttonContentConverter.ConvertBack(list, null, null, null));
        }
    }
}
