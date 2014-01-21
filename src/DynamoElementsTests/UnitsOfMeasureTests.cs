﻿using NUnit.Framework;
using Dynamo.Units;

namespace Dynamo.Tests
{
    internal class UnitsOfMeasureTests : UnitTestBase
    {
        [Test]
        public void SetLengthsFromString()
        {
            //feet tests
            var length = new Dynamo.Units.Length(1.0);

            length.SetValueFromString("1' 3\"");
            Assert.AreEqual(0.381, length.Value, 0.001);

            length.SetValueFromString("1' 2\"");
            Assert.AreEqual(0.3556, length.Value, 0.001);

            length.SetValueFromString("30.48 cm");
            Assert.AreEqual(0.3048, length.Value, 0.001);

            length.SetValueFromString("1 ft");
            Assert.AreEqual(0.3048, length.Value, 0.001);

            length.SetValueFromString("12 in");
            Assert.AreEqual(0.3048, length.Value, 0.001);

            length.SetValueFromString("1' 0\"");
            Assert.AreEqual(0.3048, length.Value, 0.001);

            length.SetValueFromString("0.3 m");
            Assert.AreEqual(0.3, length.Value, 0.001);

            length.SetValueFromString("304.8 mm");
            Assert.AreEqual(0.3048, length.Value, 0.001);

            length.SetValueFromString("1' 2 1/2\"");
            Assert.AreEqual(0.3683, length.Value, 0.001);
            
            length.SetValueFromString("14 1/2\"");
            Assert.AreEqual(0.3683, length.Value, 0.001);
        }

        [Test]
        public void SetAreaFromString()
        {
            var area = new Area(1.0);

            area.SetValueFromString("1 mm²");
            Assert.AreEqual(1.0e-6 , area.Value, 0.001);

            area.SetValueFromString("1mm²");
            Assert.AreEqual(1.0e-6, area.Value, 0.001);

            area.SetValueFromString("1 mm2");
            Assert.AreEqual(1.0e-6, area.Value, 0.001);

            area.SetValueFromString("1 sqmm");
            Assert.AreEqual(1.0e-6, area.Value, 0.001);


            area.SetValueFromString("1 cm²");
            Assert.AreEqual(0.0001, area.Value, 0.001);

            area.SetValueFromString("1cm²");
            Assert.AreEqual(0.0001, area.Value, 0.001);

            area.SetValueFromString("1 cm2");
            Assert.AreEqual(0.0001, area.Value, 0.001);

            area.SetValueFromString("1 sqcm");
            Assert.AreEqual(0.0001, area.Value, 0.001);


            area.SetValueFromString("1 m²");
            Assert.AreEqual(1, area.Value, 0.001);

            area.SetValueFromString("1m²");
            Assert.AreEqual(1, area.Value, 0.001);

            area.SetValueFromString("1 m2");
            Assert.AreEqual(1, area.Value, 0.001);

            area.SetValueFromString("1 sqm");
            Assert.AreEqual(1, area.Value, 0.001);


            area.SetValueFromString("1 in²");
            Assert.AreEqual(0.00064516, area.Value, 0.001);

            area.SetValueFromString("1in²");
            Assert.AreEqual(0.00064516, area.Value, 0.001);

            area.SetValueFromString("1 in2");
            Assert.AreEqual(0.00064516, area.Value, 0.001);

            area.SetValueFromString("1 sqin");
            Assert.AreEqual(0.00064516, area.Value, 0.001);


            area.SetValueFromString("1 ft²");
            Assert.AreEqual(0.092903, area.Value, 0.001);

            area.SetValueFromString("1ft²");
            Assert.AreEqual(0.092903, area.Value, 0.001);

            area.SetValueFromString("1 ft2");
            Assert.AreEqual(0.092903, area.Value, 0.001);

            area.SetValueFromString("1 sqft");
            Assert.AreEqual(0.092903, area.Value, 0.001);

        }

        [Test]
        public void SetVolumeFromString()
        {
            var volume = new Volume(1.0);

            volume.SetValueFromString("1 mm³");
            Assert.AreEqual(1.0e-9, volume.Value, 0.001);

            volume.SetValueFromString("1mm³");
            Assert.AreEqual(1.0e-9, volume.Value, 0.001);

            volume.SetValueFromString("1 mm3");
            Assert.AreEqual(1.0e-9, volume.Value, 0.001);

            volume.SetValueFromString("1 cumm");
            Assert.AreEqual(1.0e-9, volume.Value, 0.001);


            volume.SetValueFromString("1 cm³");
            Assert.AreEqual(1.0e-6, volume.Value, 0.001);

            volume.SetValueFromString("1cm³");
            Assert.AreEqual(1.0e-6, volume.Value, 0.001);

            volume.SetValueFromString("1 cm3");
            Assert.AreEqual(1.0e-6, volume.Value, 0.001);

            volume.SetValueFromString("1 cucm");
            Assert.AreEqual(1.0e-6, volume.Value, 0.001);


            volume.SetValueFromString("1 m³");
            Assert.AreEqual(1, volume.Value, 0.001);

            volume.SetValueFromString("1m³");
            Assert.AreEqual(1, volume.Value, 0.001);

            volume.SetValueFromString("1 m3");
            Assert.AreEqual(1, volume.Value, 0.001);

            volume.SetValueFromString("1 cum");
            Assert.AreEqual(1, volume.Value, 0.001);


            volume.SetValueFromString("1 in³");
            Assert.AreEqual(1.6387e-5, volume.Value, 0.001);

            volume.SetValueFromString("1in³");
            Assert.AreEqual(1.6387e-5, volume.Value, 0.001);

            volume.SetValueFromString("1 in3");
            Assert.AreEqual(1.6387e-5, volume.Value, 0.001);

            volume.SetValueFromString("1 cuin");
            Assert.AreEqual(1.6387e-5, volume.Value, 0.001);


            volume.SetValueFromString("1 ft³");
            Assert.AreEqual(0.0283168, volume.Value, 0.001);

            volume.SetValueFromString("1ft³");
            Assert.AreEqual(0.0283168, volume.Value, 0.001);

            volume.SetValueFromString("1 ft3");
            Assert.AreEqual(0.0283168, volume.Value, 0.001);

            volume.SetValueFromString("1 cuft");
            Assert.AreEqual(0.0283168, volume.Value, 0.001);
        }

        [Test]
        public void ToFractonialInchRepresentation()
        {
            var length = new Dynamo.Units.Length(0.03175); //1.25"
            Assert.AreEqual("1 1/4\"", length.ToString(DynamoLengthUnit.FractionalInch));

            length.Value = -0.03175;
            Assert.AreEqual("-1 1/4\"", length.ToString(DynamoLengthUnit.FractionalInch));

            //test just the fractional case
            length.Value = 0.00635; //1/4"
            Assert.AreEqual("1/4\"", length.ToString(DynamoLengthUnit.FractionalInch));

            length.Value = -0.00635; //-1/4"
            Assert.AreEqual("-1/4\"", length.ToString(DynamoLengthUnit.FractionalInch));

            //test just the whole case
            length.Value = 0.0254; //1"
            Assert.AreEqual("1\"", length.ToString(DynamoLengthUnit.FractionalInch));

            length.Value = -0.0254;
            Assert.AreEqual("-1\"", length.ToString(DynamoLengthUnit.FractionalInch));

            //test some edge cases
            length.Value = 0.0;
            Assert.AreEqual("0\"", length.ToString(DynamoLengthUnit.FractionalInch));

            length.Value = 0.000396875; //1/64"
            Assert.AreEqual("1/64\"", length.ToString(DynamoLengthUnit.FractionalInch));

            length.Value = 0.025146; //.99"
            Assert.AreEqual("1\"", length.ToString(DynamoLengthUnit.FractionalInch));
        }

        [Test]
        public void ToFractionalFootRepresentations()
        {
            //test just the fractional case
            var length = new Units.Length(0.0762); //.25"
            Assert.AreEqual("3\"", length.ToString(DynamoLengthUnit.FractionalFoot));

            length.Value = -0.0762;
            Assert.AreEqual("-3\"", length.ToString(DynamoLengthUnit.FractionalFoot));

            length.Value = 0.3048; //1ft.
            Assert.AreEqual("1' 0\"", length.ToString(DynamoLengthUnit.FractionalFoot));

            length.Value = -0.3048;
            Assert.AreEqual("-1' 0\"", length.ToString(DynamoLengthUnit.FractionalFoot));

            //test some edge cases
            length.Value = 0.0;
            Assert.AreEqual("0' 0\"", length.ToString(DynamoLengthUnit.FractionalFoot));

            length.Value = 0.003175; //1/8"
            Assert.AreEqual("1/8\"", length.ToString(DynamoLengthUnit.FractionalFoot));

            length.Value = 0.301752; //.99ft
            Assert.AreEqual("11 57/64\"", length.ToString(DynamoLengthUnit.FractionalFoot));

            length.Value = 0.3044952; //.999ft
            Assert.AreEqual("1'", length.ToString(DynamoLengthUnit.FractionalFoot));
        }

        [Test]
        public void FromFeetAndFractionalInches()
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

            //test for some spaces between values and units
            length = "1 m";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(1, m);

            length = "1 cm";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(1, cm);

            length = "1 mm";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(1, mm);

            length = "1 ft";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(1, feet);

            length = "1 in";
            Utils.ParseLengthFromString(length, out feet, out inch, out m, out cm, out mm, out numerator,
                                        out denominator);
            Assert.AreEqual(1, inch);
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

        [Test]
        public void UnitsMath()
        {
            var length = new Units.Length(2.0);
            var area = new Units.Area(2.0);
            var volume = new Units.Volume(2.0);

            //addition
            var length_add = length + length;
            Assert.AreEqual(4, length_add.Value);
            var area_add = area + area;
            Assert.AreEqual(4, area_add.Value);
            var volume_add = volume + volume;
            Assert.AreEqual(4, volume_add.Value);

            Assert.Throws<UnitsException>(() => { var test = length + area; });
            Assert.Throws<UnitsException>(() => { var test = area + volume; });
            Assert.Throws<UnitsException>(() => { var test = length + volume; });
            
            //subtraction
            var length_sub = length - length;
            Assert.AreEqual(0, length_sub.Value);
            var area_sub = area - area;
            Assert.AreEqual(0, area_sub.Value);
            var volume_sub = volume - volume;
            Assert.AreEqual(0, volume_sub.Value);

            Assert.Throws<UnitsException>(() => { var test = length - area; });
            Assert.Throws<UnitsException>(() => { var test = area - volume; });
            Assert.Throws<UnitsException>(() => { var test = length - volume; });
            Assert.Throws<UnitsException>(() => { var test = area - length; });
            Assert.Throws<UnitsException>(() => { var test = volume - area; });
            Assert.Throws<UnitsException>(() => { var test = volume - length; });

            //multiplication
            Assert.AreEqual(4, (length * length).Value);
            Assert.Throws<UnitsException>(() => { var test = area * area; });
            Assert.Throws<UnitsException>(() => { var test = volume * area; });
            Assert.Throws<UnitsException>(() => { var test = length * volume; });

            //division
            Assert.AreEqual(1, length / length);
            Assert.AreEqual(1, area / area);
            Assert.AreEqual(1, volume / volume);
            Assert.Throws<UnitsException>(() => { var test = length / area; });
            Assert.Throws<UnitsException>(() => { var test = area / volume; });
            Assert.Throws<UnitsException>(() => { var test = length / volume; });

            //modulo
            var length_mod = length%length;
            Assert.AreEqual(0, length_mod.Value);
            var area_mode = area%area;
            Assert.AreEqual(0, area_mode.Value);
            var volume_mod = volume%volume;
            Assert.AreEqual(0, volume_mod.Value);
            Assert.Throws<UnitsException>(() => { var test = length % area; });
            Assert.Throws<UnitsException>(() => { var test = area % volume; });
            Assert.Throws<UnitsException>(() => { var test = length % volume; });
            Assert.Throws<UnitsException>(() => { var test = area % length; });
            Assert.Throws<UnitsException>(() => { var test = volume % area; });
            Assert.Throws<UnitsException>(() => { var test = volume % length; });
        }

        [Test]
        public void UnitsNegatives()
        {
            //construction
            Assert.DoesNotThrow(() => { var test = new Units.Length(-2.0); });
            Assert.Throws<MathematicalArgumentException>(() => { var test = new Area(-2.0); });
            Assert.Throws<MathematicalArgumentException>(() => { var test = new Volume(-2.0); });

            var length = new Units.Length(2.0);
            var area = new Area(2.0);
            var volume = new Volume(2.0);

            Assert.Throws<MathematicalArgumentException>(() => { var test = new Area(10.0) - new Area(12.0); });
            Assert.Throws<MathematicalArgumentException>(() => { var test = new Volume(10.0) - new Volume(12.0); });
        }

        [Test]
        public void Extensions()
        {
            const double x = 5.0;

            var length = x.ToLength();
            Assert.AreEqual("5.00 m", length.ToString(DynamoLengthUnit.Meter));

            var area = x.ToArea();
            Assert.AreEqual("5.00 m²", area.ToString(DynamoAreaUnit.SquareMeter));

            var volume = x.ToVolume();
            Assert.AreEqual("5.00 m³", volume.ToString(DynamoVolumeUnit.CubicMeter));

        }

        [Test]
        public void UiRounding()
        {
            UnitsManager.Instance.LengthUnit = DynamoLengthUnit.FractionalFoot;

            var length = Units.Length.FromFeet(1.5);
            Assert.AreEqual("2' 0\"", ((Units.Length)length.Round()).ToString(DynamoLengthUnit.FractionalFoot));
            Assert.AreEqual("2' 0\"", ((Units.Length)length.Ceiling()).ToString(DynamoLengthUnit.FractionalFoot));
            Assert.AreEqual("1' 0\"", ((Units.Length)length.Floor()).ToString(DynamoLengthUnit.FractionalFoot));

            length = Units.Length.FromFeet(1.2);
            Assert.AreEqual("1' 0\"", ((Units.Length)length.Round()).ToString(DynamoLengthUnit.FractionalFoot));
            Assert.AreEqual("2' 0\"", ((Units.Length)length.Ceiling()).ToString(DynamoLengthUnit.FractionalFoot));
            Assert.AreEqual("1' 0\"", ((Units.Length)length.Floor()).ToString(DynamoLengthUnit.FractionalFoot));

            length = Units.Length.FromFeet(-1.5);
            Assert.AreEqual("-2' 0\"", ((Units.Length)length.Round()).ToString(DynamoLengthUnit.FractionalFoot));
            Assert.AreEqual("-1' 0\"", ((Units.Length)length.Ceiling()).ToString(DynamoLengthUnit.FractionalFoot));
            Assert.AreEqual("-2' 0\"", ((Units.Length)length.Floor()).ToString(DynamoLengthUnit.FractionalFoot));

            length = Units.Length.FromFeet(-1.2);
            Assert.AreEqual("-1' 0\"", ((Units.Length)length.Round()).ToString(DynamoLengthUnit.FractionalFoot));
            Assert.AreEqual("-1' 0\"", ((Units.Length)length.Ceiling()).ToString(DynamoLengthUnit.FractionalFoot));
            Assert.AreEqual("-2' 0\"", ((Units.Length)length.Floor()).ToString(DynamoLengthUnit.FractionalFoot));

            //this fails as explained here:
            //http://msdn.microsoft.com/en-us/library/wyk4d9cy(v=vs.110).aspx
            //length = Units.Length.FromFeet(.5);
            //Assert.AreEqual("1' 0\"", ((Units.Length)length.Round()).ToString(DynamoLengthUnit.FractionalFoot));
        }
    }
}
