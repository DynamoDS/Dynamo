using System.Globalization;
using NUnit.Framework;
using Dynamo.Measure;

namespace Dynamo.Tests
{
    internal class UnitsOfMeasureTests
    {
        [Test]
        public void MeasurementConversionsValidInput()
        {
            //feet tests
            Assert.AreEqual("1' 3\"", Foot.ToDisplayString(1.25, DynamoUnitDisplayType.FractionalFeetInches));
            Assert.AreEqual("15\"", Foot.ToDisplayString(1.25, DynamoUnitDisplayType.FractionalInches));
            Assert.AreEqual("1' 3\"", Utils.ToFeetAndFractionalInches(1.25));
            Assert.AreEqual("15\"", Utils.ToFractionalInches(1.25*12));
            Assert.AreEqual(1.25, Foot.FromDisplayString("1' 3\"", DynamoUnitDisplayType.FractionalFeetInches));
            Assert.AreEqual("1' 2\"", Foot.ToDisplayString(1.16667, DynamoUnitDisplayType.FractionalFeetInches));
            Assert.AreEqual("1' 2\"",Utils.ToFeetAndFractionalInches(1.16667));

            //inch tests
            Assert.AreEqual(string.Format("{0} cm", (30.48).ToString(CultureInfo.CurrentCulture)), Inch.ToDisplayString(12.0, DynamoUnitDisplayType.Centimeters));
            Assert.AreEqual(string.Format("{0} ft", (1.0).ToString("0.00", CultureInfo.CurrentCulture)), Inch.ToDisplayString(12.0, DynamoUnitDisplayType.DecimalFeet));
            Assert.AreEqual(string.Format("{0} in", (12.00).ToString("0.00", CultureInfo.CurrentCulture)), Inch.ToDisplayString(12.0, DynamoUnitDisplayType.DecimalInches));
            Assert.AreEqual("1' 0\"", Inch.ToDisplayString(12.0, DynamoUnitDisplayType.FractionalFeetInches));
            Assert.AreEqual("12\"", Inch.ToDisplayString(12.0, DynamoUnitDisplayType.FractionalInches));
            Assert.AreEqual(string.Format("{0} m", (0.30).ToString("0.00", CultureInfo.CurrentCulture)), Inch.ToDisplayString(12.0, DynamoUnitDisplayType.Meters));
            Assert.AreEqual(string.Format("{0} mm", (304.80).ToString("0.00", CultureInfo.CurrentCulture)), Inch.ToDisplayString(12.0, DynamoUnitDisplayType.Millimeters));
            Assert.AreEqual(14.5, Inch.FromDisplayString("1' 2 1/2\"", DynamoUnitDisplayType.FractionalInches), 0.0001);
            Assert.AreEqual(14.5, Inch.FromDisplayString("14 1/2\"", DynamoUnitDisplayType.FractionalInches), 0.0001);
        }

        [Test]
        public void CreateAndConvertInchesValidINput()
        {
            //test positive cases
            var mm = Inch.ConvertTo(1.25, DynamoUnitType.Millimeters);
            Assert.AreEqual(25.4 * 1.25, mm, 0.001);

            var cm = Inch.ConvertTo(1.25, DynamoUnitType.Centimeters);
            Assert.AreEqual(2.54 * 1.25, cm, 0.001);

            var m = Inch.ConvertTo(1.25, DynamoUnitType.Meters);
            Assert.AreEqual(0.0254 * 1.25, m, 0.001);

            var inch = Inch.ConvertTo(1.25, DynamoUnitType.Inches);
            Assert.AreEqual(1.0 * 1.25, inch, 0.001);

            var ft = Inch.ConvertTo(1.25, DynamoUnitType.Feet);
            Assert.AreEqual(0.083333 * 1.25, ft, 0.001);

            Assert.AreEqual("1 1/4\"", Inch.AsString(1.25));

            //test negative cases
            mm = Inch.ConvertTo(-1.25, DynamoUnitType.Millimeters);
            Assert.AreEqual(-25.4 * 1.25, mm, 0.001);

            cm = Inch.ConvertTo(-1.25, DynamoUnitType.Centimeters);
            Assert.AreEqual(-2.54 * 1.25, cm, 0.001);

            m = Inch.ConvertTo(-1.25, DynamoUnitType.Meters);
            Assert.AreEqual(-0.0254 * 1.25, m, 0.001);

            inch = Inch.ConvertTo(-1.25, DynamoUnitType.Inches);
            Assert.AreEqual(-1.0 * 1.25, inch, 0.001);

            ft = Inch.ConvertTo(-1.25, DynamoUnitType.Feet);
            Assert.AreEqual(-0.083333 * 1.25, ft, 0.001);

            Assert.AreEqual("-1 1/4\"", Inch.AsString(-1.25));

            //test just the fractional case
            Assert.AreEqual("1/4\"", Inch.AsString(.25));
            Assert.AreEqual("-1/4\"", Inch.AsString(-.25));

            //test just the whole case
            Assert.AreEqual("1\"", Inch.AsString(1.0));
            Assert.AreEqual("-1\"", Inch.AsString(-1.0));

            //test some edge cases
            Assert.AreEqual("0\"", Inch.AsString(0.0));
            Assert.AreEqual("1/64\"", Inch.AsString(0.01));
            Assert.AreEqual("1\"", Inch.AsString(0.99));
        }

        [Test]
        public void CreateAndConvertFeetValidInput()
        {
            //test positive cases
            var mm = Foot.ConvertTo(1.3177, DynamoUnitType.Millimeters);
            Assert.AreEqual(304.8 * 1.3177, mm, 0.001);

            var cm = Foot.ConvertTo(1.3177, DynamoUnitType.Centimeters);
            Assert.AreEqual(30.48 * 1.3177, cm, 0.001);

            var m = Foot.ConvertTo(1.3177, DynamoUnitType.Meters);
            Assert.AreEqual(.3048 * 1.3177, m, 0.001);

            var inch = Foot.ConvertTo(1.3177, DynamoUnitType.Inches);
            Assert.AreEqual(12.0 * 1.3177, inch, 0.001);

            var ft = Foot.ConvertTo(1.3177, DynamoUnitType.Feet);
            Assert.AreEqual(1.0 * 1.3177, ft, 0.001);

            Assert.AreEqual("1' 3 13/16\"", Foot.AsString(1.3177));

            mm = Foot.ConvertTo(-1.3177, DynamoUnitType.Millimeters);
            Assert.AreEqual(-304.8 * 1.3177, mm, 0.001);

            cm = Foot.ConvertTo(-1.3177, DynamoUnitType.Centimeters);
            Assert.AreEqual(-30.48 * 1.3177, cm, 0.001);

            m = Foot.ConvertTo(-1.3177, DynamoUnitType.Meters);
            Assert.AreEqual(-.3048 * 1.3177, m, 0.001);

            inch = Foot.ConvertTo(-1.3177, DynamoUnitType.Inches);
            Assert.AreEqual(-12.0 * 1.3177, inch, 0.001);

            ft = Foot.ConvertTo(-1.3177, DynamoUnitType.Feet);
            Assert.AreEqual(-1.0 * 1.3177, ft, 0.001);

            //Assert.AreEqual("-1' 3 13/16\"", ftLength.ToString());

            //test just the fractional case
            Assert.AreEqual("3\"", Foot.AsString(.25));
            Assert.AreEqual("-3\"", Foot.AsString(-.25));

            //test just the whole case
            Assert.AreEqual("1' 0\"", Foot.AsString(1.0));
            Assert.AreEqual("-1' 0\"", Foot.AsString(-1.0));

            //test some edge cases
            Assert.AreEqual("0' 0\"", Foot.AsString(0.0));
            Assert.AreEqual("1/8\"", Foot.AsString(0.01));
            Assert.AreEqual("11 57/64\"", Foot.AsString(0.99));
            Assert.AreEqual("1'", Foot.AsString(0.999));
        }

        [Test]
        public void CreateAndConvertMillimetersValidInput()
        {
            Assert.AreEqual(1.0, Millimeter.ConvertTo(1.0,DynamoUnitType.Millimeters), 0.001);
            Assert.AreEqual(.1, Millimeter.ConvertTo(1.0, DynamoUnitType.Centimeters), 0.001);
            Assert.AreEqual(.001, Millimeter.ConvertTo(1.0, DynamoUnitType.Meters), 0.001);
            Assert.AreEqual(.03937, Millimeter.ConvertTo(1.0, DynamoUnitType.Inches), 0.001);
            Assert.AreEqual(.003281, Millimeter.ConvertTo(1.0, DynamoUnitType.Feet), 0.001);
            Assert.AreEqual("1.00 mm", Millimeter.AsString(1.0));
        }

        [Test]
        public void CreateAndConvertCentimetersValidInput()
        {
            Assert.AreEqual(10.0, Centimeter.ConvertTo(1.0, DynamoUnitType.Millimeters), 0.001);
            Assert.AreEqual(1.0, Centimeter.ConvertTo(1.0, DynamoUnitType.Centimeters), 0.001);
            Assert.AreEqual(.01, Centimeter.ConvertTo(1.0, DynamoUnitType.Meters), 0.001);
            Assert.AreEqual(.39371, Centimeter.ConvertTo(1.0, DynamoUnitType.Inches), 0.001);
            Assert.AreEqual(.032808, Centimeter.ConvertTo(1.0, DynamoUnitType.Feet), 0.001);
            Assert.AreEqual("1.00 cm", Centimeter.AsString(1.0));
        }

        [Test]
        public void CreateAndConvertMetersValidInput()
        {
            Assert.AreEqual(1000.0, Meter.ConvertTo(1.0, DynamoUnitType.Millimeters), 0.001);
            Assert.AreEqual(100.0, Meter.ConvertTo(1.0, DynamoUnitType.Centimeters), 0.001);
            Assert.AreEqual(1.0, Meter.ConvertTo(1.0, DynamoUnitType.Meters), 0.001);
            Assert.AreEqual(39.370079, Meter.ConvertTo(1.0, DynamoUnitType.Inches), 0.001);
            Assert.AreEqual(3.28084, Meter.ConvertTo(1.0, DynamoUnitType.Feet), 0.001);
            Assert.AreEqual("1.00 m", Meter.AsString(1.0));
        }

        [Test]
        public void FeetAndFractionalInchesValidInput()
        {
            Assert.AreEqual(1.0, Utils.FromFeetAndFractionalInches("1'"));
            Assert.AreEqual(1.0, Utils.FromFeetAndFractionalInches("1' 0\""));
            Assert.AreEqual(1.25, Utils.FromFeetAndFractionalInches("1' 3\""));
            Assert.AreEqual(-1.25, Utils.FromFeetAndFractionalInches("-1' 3\""));
            Assert.AreEqual(1.0, Utils.FromFeetAndFractionalInches("12\""));
            Assert.AreEqual(1.3177, Utils.FromFeetAndFractionalInches("1' 3 13/16\""), 0.001);
        }

        [Test]
        public void ParseLengthFromString()
        {
            double feet = 0;
            double inch = 0;
            double m = 0;
            double cm = 0;
            double mm = 0;
            double numerator = 0;
            double denominator = 0;

            string length = "-8' 3/32\"";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(-8, feet);
            Assert.AreEqual(0, inch);
            Assert.AreEqual(3, numerator);
            Assert.AreEqual(32, denominator);

            length = "8'";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(8, feet);
            Assert.AreEqual(0, inch);
            Assert.AreEqual(0, numerator);
            Assert.AreEqual(0, denominator);

            length = "-8.25' -3.25\"";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(-8.25, feet);
            Assert.AreEqual(-3.25, inch);
            Assert.AreEqual(0, numerator);
            Assert.AreEqual(0, denominator);

            length = "hello";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(0, feet);
            Assert.AreEqual(0, inch);
            Assert.AreEqual(0, numerator);
            Assert.AreEqual(0, denominator);

            length = "-12.2' 3-3/32\"";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(-12.2, feet);
            Assert.AreEqual(3, inch);
            Assert.AreEqual(3, numerator);
            Assert.AreEqual(32, denominator);

            length = "2 3/32\"";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(0, feet);
            Assert.AreEqual(2, inch);
            Assert.AreEqual(3, numerator);
            Assert.AreEqual(32, denominator);

            length = "8ft 12in";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(8, feet);
            Assert.AreEqual(12, inch);
            Assert.AreEqual(0, numerator);
            Assert.AreEqual(0, denominator);

            length = "1m 100cm 5mm";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(1, m);
            Assert.AreEqual(100, cm);
            Assert.AreEqual(5, mm);

            length = "-1m -100cm -5mm";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(-1, m);
            Assert.AreEqual(-100, cm);
            Assert.AreEqual(-5, mm);
        }

        [Test]
        public void CreateFraction()
        {
            Assert.AreEqual("", Utils.ParsePartialInchesToString(0.0, 0.015625));
            Assert.AreEqual("1/2", Utils.ParsePartialInchesToString(0.5, 0.015625));
            Assert.AreEqual("3/8", Utils.ParsePartialInchesToString(0.375, 0.015625));
            Assert.AreEqual("3/4", Utils.ParsePartialInchesToString(0.75, 0.015625));
            Assert.AreEqual("7/64", Utils.ParsePartialInchesToString(0.109375, 0.015625));
            Assert.AreEqual("3/32", Utils.ParsePartialInchesToString(0.09375, 0.015625));
            Assert.AreEqual("17/32", Utils.ParsePartialInchesToString(0.53125, 0.015625));
            Assert.AreEqual("1/64", Utils.ParsePartialInchesToString(.015625, 0.015625)); //1/64"
            Assert.AreEqual("63/64", Utils.ParsePartialInchesToString(.984375, 0.015625)); //63/64"
            Assert.AreEqual("1", Utils.ParsePartialInchesToString(.99, 0.015625));
        }

        [Test]
        public void FeetAndFractionalInchesInvalidInput()
        {
            Assert.AreEqual(0.0, Utils.FromFeetAndFractionalInches("--1'"));
            Assert.AreEqual(0.0, Utils.FromFeetAndFractionalInches("turtles"));
            Assert.AreEqual(0.0, Utils.FromFeetAndFractionalInches("isn't this nice!"));
            Assert.AreEqual(0.0, Utils.FromFeetAndFractionalInches("ft"));
            Assert.AreEqual(0.0, Utils.FromFeetAndFractionalInches("\""));
            Assert.AreEqual(0.0, Utils.FromFeetAndFractionalInches("6.5"));
        }
    }
}
