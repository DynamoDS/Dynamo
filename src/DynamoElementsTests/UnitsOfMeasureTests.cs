using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Dynamo.Measure;

namespace DynamoElementsTests
{
    [TestFixture]
    internal class UnitsOfMeasureTests
    {
        [Test]
        public void MeasurementConversions()
        {
            //feet tests
            var foot = new DynamoLength<Foot>(1.25);
            Assert.AreEqual("1' 3\"", foot.ToDisplayString(DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES));
            Assert.AreEqual("15\"", foot.ToDisplayString(DynamoUnitDisplayType.FRACTIONAL_INCHES));

            foot.FromDisplayString("1' 3\"", DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES);
            Assert.AreEqual(1.25, foot.Item.Length);

            foot.Item.Length = 1.16667;
            Assert.AreEqual("1' 2\"", foot.ToDisplayString(DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES));

            //inch tests
            var inch = new DynamoLength<Inch>(12.0);
            Assert.AreEqual("30.48 cm", inch.ToDisplayString(DynamoUnitDisplayType.CENTIMETERS));
            Assert.AreEqual("1.00 ft", inch.ToDisplayString(DynamoUnitDisplayType.DECIMAL_FEET));
            Assert.AreEqual("12.00 in", inch.ToDisplayString(DynamoUnitDisplayType.DECIMAL_INCHES));
            Assert.AreEqual("1' 0\"", inch.ToDisplayString(DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES));
            Assert.AreEqual("12\"", inch.ToDisplayString(DynamoUnitDisplayType.FRACTIONAL_INCHES));
            Assert.AreEqual("0.30 m", inch.ToDisplayString(DynamoUnitDisplayType.METERS));
            Assert.AreEqual("304.80 mm", inch.ToDisplayString(DynamoUnitDisplayType.MILLIMETERS));

            //test inches internal unit storage
            inch.FromDisplayString("1' 2 1/2\"", DynamoUnitDisplayType.FRACTIONAL_INCHES);
            Assert.AreEqual(14.5, inch.Item.Length, 0.0001);

            inch.FromDisplayString("14 1/2\"", DynamoUnitDisplayType.FRACTIONAL_INCHES);
            Assert.AreEqual(14.5, inch.Item.Length, 0.0001);
        }

        [Test]
        public void CreateAndConvertInches()
        {
            var inchLength = new DynamoLength<Inch>(1.25);

            //test positive cases
            var mm = inchLength.Item.ConvertTo(DynamoUnitType.MILLIMETERS);
            Assert.AreEqual(25.4 * 1.25, mm, 0.001);

            var cm = inchLength.Item.ConvertTo(DynamoUnitType.CENTIMETERS);
            Assert.AreEqual(2.54 * 1.25, cm, 0.001);

            var m = inchLength.Item.ConvertTo(DynamoUnitType.METERS);
            Assert.AreEqual(0.0254 * 1.25, m, 0.001);

            var inch = inchLength.Item.ConvertTo(DynamoUnitType.INCHES);
            Assert.AreEqual(1.0 * 1.25, inch, 0.001);

            var ft = inchLength.Item.ConvertTo(DynamoUnitType.FEET);
            Assert.AreEqual(0.083333 * 1.25, ft, 0.001);

            Assert.AreEqual("1 1/4\"", inchLength.ToString());

            //test negative cases
            inchLength.Item.Length = -1.25;

            mm = inchLength.Item.ConvertTo(DynamoUnitType.MILLIMETERS);
            Assert.AreEqual(-25.4 * 1.25, mm, 0.001);

            cm = inchLength.Item.ConvertTo(DynamoUnitType.CENTIMETERS);
            Assert.AreEqual(-2.54 * 1.25, cm, 0.001);

            m = inchLength.Item.ConvertTo(DynamoUnitType.METERS);
            Assert.AreEqual(-0.0254 * 1.25, m, 0.001);

            inch = inchLength.Item.ConvertTo(DynamoUnitType.INCHES);
            Assert.AreEqual(-1.0 * 1.25, inch, 0.001);

            ft = inchLength.Item.ConvertTo(DynamoUnitType.FEET);
            Assert.AreEqual(-0.083333 * 1.25, ft, 0.001);

            Assert.AreEqual("-1 1/4\"", inchLength.ToString());

            //test just the fractional case
            inchLength.Item.Length = .25;
            Assert.AreEqual("1/4\"", inchLength.ToString());
            inchLength.Item.Length = -.25;
            Assert.AreEqual("-1/4\"", inchLength.ToString());

            //test just the whole case
            inchLength.Item.Length = 1.0;
            Assert.AreEqual("1\"", inchLength.ToString());
            inchLength.Item.Length = -1.0;
            Assert.AreEqual("-1\"", inchLength.ToString());

            //test some edge cases
            inchLength.Item.Length = 0.0;
            Assert.AreEqual("0\"", inchLength.ToString());

            inchLength.Item.Length = 0.01;
            Assert.AreEqual("1/64\"", inchLength.ToString());

            inchLength.Item.Length = 0.99;
            Assert.AreEqual("1\"", inchLength.ToString());
        }

        [Test]
        public void CreateAndConvertFeet()
        {
            var ftLength = new DynamoLength<Foot>(1.3177);

            //test positive cases
            var mm = ftLength.Item.ConvertTo(DynamoUnitType.MILLIMETERS);
            Assert.AreEqual(304.8 * 1.3177, mm, 0.001);

            var cm = ftLength.Item.ConvertTo(DynamoUnitType.CENTIMETERS);
            Assert.AreEqual(30.48 * 1.3177, cm, 0.001);

            var m = ftLength.Item.ConvertTo(DynamoUnitType.METERS);
            Assert.AreEqual(.3048 * 1.3177, m, 0.001);

            var inch = ftLength.Item.ConvertTo(DynamoUnitType.INCHES);
            Assert.AreEqual(12.0 * 1.3177, inch, 0.001);

            var ft = ftLength.Item.ConvertTo(DynamoUnitType.FEET);
            Assert.AreEqual(1.0 * 1.3177, ft, 0.001);

            Assert.AreEqual("1' 3 13/16\"", ftLength.ToString());

            //test negative cases
            ftLength.Item.Length = -1.3177;
            
            mm = ftLength.Item.ConvertTo(DynamoUnitType.MILLIMETERS);
            Assert.AreEqual(-304.8 * 1.3177, mm, 0.001);

            cm = ftLength.Item.ConvertTo(DynamoUnitType.CENTIMETERS);
            Assert.AreEqual(-30.48 * 1.3177, cm, 0.001);

            m = ftLength.Item.ConvertTo(DynamoUnitType.METERS);
            Assert.AreEqual(-.3048 * 1.3177, m, 0.001);

            inch = ftLength.Item.ConvertTo(DynamoUnitType.INCHES);
            Assert.AreEqual(-12.0 * 1.3177, inch, 0.001);

            ft = ftLength.Item.ConvertTo(DynamoUnitType.FEET);
            Assert.AreEqual(-1.0 * 1.3177, ft, 0.001);

            Assert.AreEqual("-1' 3 13/16\"", ftLength.ToString());

            //test just the fractional case
            ftLength.Item.Length = .25;
            Assert.AreEqual("3\"", ftLength.ToString());
            ftLength.Item.Length = -.25;
            Assert.AreEqual("-3\"", ftLength.ToString());

            //test just the whole case
            ftLength.Item.Length = 1.0;
            Assert.AreEqual("1' 0\"", ftLength.ToString());
            ftLength.Item.Length = -1.0;
            Assert.AreEqual("-1' 0\"", ftLength.ToString());

            //test some edge cases
            ftLength.Item.Length = 0.0;
            Assert.AreEqual("0' 0\"", ftLength.ToString());

            ftLength.Item.Length = 0.01;
            Assert.AreEqual("1/8\"", ftLength.ToString());

            ftLength.Item.Length = 0.99;
            Assert.AreEqual("11 57/64\"", ftLength.ToString());

            ftLength.Item.Length = 0.9999;
            Assert.AreEqual("1'", ftLength.ToString());
        }

        [Test]
        public void CreateAndConvertMillimeters()
        {
            var mmLength = new DynamoLength<Millimeter>(1.0);

            Assert.AreEqual(1.0, mmLength.Item.ConvertTo(DynamoUnitType.MILLIMETERS), 0.001);
            Assert.AreEqual(.1, mmLength.Item.ConvertTo(DynamoUnitType.CENTIMETERS), 0.001);
            Assert.AreEqual(.001, mmLength.Item.ConvertTo(DynamoUnitType.METERS), 0.001);
            Assert.AreEqual(.03937, mmLength.Item.ConvertTo(DynamoUnitType.INCHES), 0.001);
            Assert.AreEqual(.003281, mmLength.Item.ConvertTo(DynamoUnitType.FEET), 0.001);

            Assert.AreEqual("1.00 mm", mmLength.ToString());
        }

        [Test]
        public void CreateAndConvertCentimeters()
        {
            var cmLength = new DynamoLength<Centimeter>(1.0);

            Assert.AreEqual(10.0, cmLength.Item.ConvertTo(DynamoUnitType.MILLIMETERS), 0.001);
            Assert.AreEqual(1.0, cmLength.Item.ConvertTo(DynamoUnitType.CENTIMETERS), 0.001);
            Assert.AreEqual(.01, cmLength.Item.ConvertTo(DynamoUnitType.METERS), 0.001);
            Assert.AreEqual(.39371, cmLength.Item.ConvertTo(DynamoUnitType.INCHES), 0.001);
            Assert.AreEqual(.032808, cmLength.Item.ConvertTo(DynamoUnitType.FEET), 0.001);

            Assert.AreEqual("1.00 cm", cmLength.ToString());
        }

        [Test]
        public void CreateAndConvertMeters()
        {
            var mLength = new DynamoLength<Meter>(1.0);

            Assert.AreEqual(1000.0, mLength.Item.ConvertTo(DynamoUnitType.MILLIMETERS), 0.001);
            Assert.AreEqual(100.0, mLength.Item.ConvertTo(DynamoUnitType.CENTIMETERS), 0.001);
            Assert.AreEqual(1.0, mLength.Item.ConvertTo(DynamoUnitType.METERS), 0.001);
            Assert.AreEqual(39.370079, mLength.Item.ConvertTo(DynamoUnitType.INCHES), 0.001);
            Assert.AreEqual(3.28084, mLength.Item.ConvertTo(DynamoUnitType.FEET), 0.001);

            Assert.AreEqual("1.00 m", mLength.ToString());
        }

        [Test]
        public void FeetAndFractionalInches()
        {
            Assert.AreEqual(1.0, Utils.FromFeetAndFractionalInches("1'"));
            Assert.AreEqual(1.0, Utils.FromFeetAndFractionalInches("1' 0\""));
            Assert.AreEqual(1.25, Utils.FromFeetAndFractionalInches("1' 3\""));
            Assert.AreEqual(-1.25, Utils.FromFeetAndFractionalInches("-1' 3\""));
            Assert.AreEqual(1.0, Utils.FromFeetAndFractionalInches("12\""));
            Assert.AreEqual(1.3177, Utils.FromFeetAndFractionalInches("1' 3 13/16\""), 0.001);
        }

        [Test]
        public void ParseUnitsFromString()
        {
            
        }
    }
}
