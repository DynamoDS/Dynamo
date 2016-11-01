﻿using System;
using System.Collections.Generic;
using System.Linq;
using DynamoUnits;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class UnitsOfMeasureTests : UnitTestBase
    {
        [SetUp]
        public void Setup()
        {
            BaseUnit.NumberFormat = "f4";
        }

        [Test]
        [Category("UnitTests")]
        public void SetLengthsFromString()
        {
            //feet tests
            var length = Length.FromDouble(1.0);

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
        [Category("UnitTests")]
        public void SetAreaFromString()
        {
            var area = Area.FromDouble(1.0);

            area.SetValueFromString("1 mm²");
            Assert.AreEqual(1.0e-6, area.Value, 0.001);

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
        [Category("UnitTests")]
        public void SetVolumeFromString()
        {
            var volume = Volume.FromDouble(1.0);

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
        [Category("UnitTests")]
        public void ToFractonialInchRepresentation()
        {
            var length = Length.FromDouble(0.03175); //1.25"

            length.LengthUnit = LengthUnit.FractionalInch;
            Assert.AreEqual("1 1/4\"", length.ToString());

            length.Value = -0.03175;
            Assert.AreEqual("-1 1/4\"", length.ToString());

            //test just the fractional case
            length.Value = 0.00635; //1/4"
            Assert.AreEqual("1/4\"", length.ToString());

            length.Value = -0.00635; //-1/4"
            Assert.AreEqual("-1/4\"", length.ToString());

            //test just the whole case
            length.Value = 0.0254; //1"
            Assert.AreEqual("1\"", length.ToString());

            length.Value = -0.0254;
            Assert.AreEqual("-1\"", length.ToString());

            //test some edge cases
            length.Value = 0.0;
            Assert.AreEqual("0\"", length.ToString());

            length.Value = 0.000396875; //1/64"
            Assert.AreEqual("1/64\"", length.ToString());

            length.Value = 0.025146; //.99"
            Assert.AreEqual("63/64\"", length.ToString());
        }

        [Test]
        [Category("UnitTests")]
        public void ToFractionalFootRepresentations()
        {
            //test just the fractional case
            var length = Length.FromDouble(0.0762); //.25'
            length.LengthUnit = LengthUnit.FractionalFoot;

            Assert.AreEqual("3\"", length.ToString());

            length.Value = -0.0762;
            Assert.AreEqual("-3\"", length.ToString());

            length.Value = 0.3048; //1ft.
            Assert.AreEqual("1' 0\"", length.ToString());

            length.Value = -0.3048;
            Assert.AreEqual("-1' 0\"", length.ToString());

            //test some edge cases
            length.Value = 0.0;
            Assert.AreEqual("0' 0\"", length.ToString());

            length.Value = 0.003175; //1/8"
            Assert.AreEqual("0.125\"", length.ToString());

            length.Value = 0.301752; //.99ft
            Assert.AreEqual("11.88\"", length.ToString());

            length.Value = 0.3044952; //.999ft
            Assert.AreEqual("11.988\"", length.ToString());

            length.Value = 0.35560000000142239; //1'2"
            Assert.AreEqual("1' 2\"", length.ToString());

            length.Value = -0.35560000000142239; //-1'2"
            Assert.AreEqual("-1' 2\"", length.ToString());

        }

        [Test]
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
        public void ParseLengthInFeetFromString()
        {
            double feet = 0;
            double numeratorInFeet = 0;
            double denominatorInFeet = 0;

            string length = "1/32";
            Utils.ParseLengthInFeetFromString(length, out feet, out numeratorInFeet,
                                        out denominatorInFeet);
            Assert.AreEqual(0, feet);
            Assert.AreEqual(1, numeratorInFeet);
            Assert.AreEqual(32, denominatorInFeet);

            length = "1/128\'";
            Utils.ParseLengthInFeetFromString(length, out feet, out numeratorInFeet,
                                        out denominatorInFeet);
            Assert.AreEqual(0, feet);
            Assert.AreEqual(1, numeratorInFeet);
            Assert.AreEqual(128, denominatorInFeet);

            length = "8.25";
            Utils.ParseLengthInFeetFromString(length, out feet, out numeratorInFeet,
                                        out denominatorInFeet);
            Assert.AreEqual(8.25, feet);
            Assert.AreEqual(0, numeratorInFeet);
            Assert.AreEqual(0, denominatorInFeet);
        }

        [Test]
        [Category("UnitTests")]
        public void CreateFraction()
        {
            Func<int, int, int> gcd = (x, y) =>
            {
                while (x != 0 && y != 0)
                {
                    if (x > y)
                        x %= y;
                    else
                        y %= x;
                }

                return (x == 0) ? y : x;
            };

            int denominator = 64;
            double precision = 0.015625;

            for (int numerator = 1; numerator <= denominator - 1; numerator++)
            {
                int commonDivisor = gcd(numerator, denominator);
                double inches = ((double)numerator) / denominator;
                string expectedString = (numerator / commonDivisor) + "/" + (denominator / commonDivisor);
                string parsedString = Utils.ParsePartialInchesToString(inches, precision);
                Assert.AreEqual(expectedString, parsedString);
            }
            Assert.AreEqual("1", Utils.ParsePartialInchesToString(0.99999, 0.015625));
        }

        [Test]
        [Category("UnitTests")]
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
        [Category("UnitTests")]
        public void UnitsMath()
        {
            var length = Length.FromDouble(2.0);
            var area = Area.FromDouble(2.0);
            var volume = Volume.FromDouble(2.0);

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
            Assert.IsInstanceOf<Area>(length * length);
            Assert.AreEqual(4, (length * area).Value);
            Assert.IsInstanceOf<Volume>(length * area);
            Assert.Throws<UnitsException>(() => { var test = area * area; });
            Assert.Throws<UnitsException>(() => { var test = volume * area; });
            Assert.Throws<UnitsException>(() => { var test = length * volume; });
            Assert.Throws<UnitsException>(() => { var test = volume * volume; });

            //division
            Assert.AreEqual(1, length / length);
            Assert.AreEqual(1, area / area);
            Assert.AreEqual(1, volume / volume);
            Assert.Throws<UnitsException>(() => { var test = length / area; });
            Assert.Throws<UnitsException>(() => { var test = area / volume; });
            Assert.Throws<UnitsException>(() => { var test = length / volume; });

            //modulo
            var length_mod = length % length;
            Assert.AreEqual(0, length_mod.Value);
            var area_mode = area % area;
            Assert.AreEqual(0, area_mode.Value);
            var volume_mod = volume % volume;
            Assert.AreEqual(0, volume_mod.Value);
            Assert.Throws<UnitsException>(() => { var test = length % area; });
            Assert.Throws<UnitsException>(() => { var test = area % volume; });
            Assert.Throws<UnitsException>(() => { var test = length % volume; });
            Assert.Throws<UnitsException>(() => { var test = area % length; });
            Assert.Throws<UnitsException>(() => { var test = volume % area; });
            Assert.Throws<UnitsException>(() => { var test = volume % length; });

            //ensure that when a formula is unit + double it returns a unit
            //and when it is double + unit, it returns a double

            Assert.AreEqual(Length.FromDouble(length.Value + 2.0), length + 2.0);
            Assert.AreEqual(4.0, 2.0 + length);

            Assert.AreEqual(Area.FromDouble(area.Value + 2.0), area + 2.0);
            Assert.AreEqual(4.0, 2.0 + area);

            Assert.AreEqual(Volume.FromDouble(volume.Value + 2.0), volume + 2.0);
            Assert.AreEqual(4.0, 2.0 + volume);
        }

        [Test]
        [Category("UnitTests")]
        public void UnitsNegatives()
        {
            var length = Length.FromDouble(-2.0);
            var area = Area.FromDouble(-2.0);
            var volume = Volume.FromDouble(-2.0);

            Assert.AreEqual(-2.0, length.Value);
            Assert.AreEqual(-2.0, area.Value);
            Assert.AreEqual(-2.0, volume.Value);

            Assert.AreEqual(Length.FromDouble(-2.0), Length.FromDouble(10.0) - Length.FromDouble(12.0));
            Assert.AreEqual(Area.FromDouble(-2.0), Area.FromDouble(10.0) - Area.FromDouble(12.0));
            Assert.AreEqual(Volume.FromDouble(-2.0), Volume.FromDouble(10.0) - Volume.FromDouble(12.0));
        }

        [Test]
        [Category("UnitTests")]
        public void Extensions()
        {
            const double x = 5.0;

            var length = x.ToLength();
            length.LengthUnit = LengthUnit.Meter;
            Assert.AreEqual("5.0000m", length.ToString());

            var area = x.ToArea();
            area.AreaUnit = AreaUnit.SquareMeter;
            Assert.AreEqual("5.0000m²", area.ToString());

            var volume = x.ToVolume();
            volume.VolumeUnit = VolumeUnit.CubicMeter;
            Assert.AreEqual("5.0000m³", volume.ToString());

        }

        [Test]
        [Category("UnitTests")]
        public void UiRounding()
        {
            var length = Length.FromFeet(1.5, LengthUnit.FractionalFoot);
            Assert.AreEqual("2' 0\"", ((Length)length.Round()).ToString());
            Assert.AreEqual("2' 0\"", ((Length)length.Ceiling()).ToString());
            Assert.AreEqual("1' 0\"", ((Length)length.Floor()).ToString());

            length = Length.FromFeet(1.2, LengthUnit.FractionalFoot);
            Assert.AreEqual("1' 0\"", ((Length)length.Round()).ToString());
            Assert.AreEqual("2' 0\"", ((Length)length.Ceiling()).ToString());
            Assert.AreEqual("1' 0\"", ((Length)length.Floor()).ToString());

            length = Length.FromFeet(-1.5, LengthUnit.FractionalFoot);
            Assert.AreEqual("-2' 0\"", ((Length)length.Round()).ToString());
            Assert.AreEqual("-1' 0\"", ((Length)length.Ceiling()).ToString());
            Assert.AreEqual("-2' 0\"", ((Length)length.Floor()).ToString());

            length = Length.FromFeet(-1.2, LengthUnit.FractionalFoot);
            Assert.AreEqual("-1' 0\"", ((Length)length.Round()).ToString());
            Assert.AreEqual("-1' 0\"", ((Length)length.Ceiling()).ToString());
            Assert.AreEqual("-2' 0\"", ((Length)length.Floor()).ToString());

            //this fails as explained here:
            //http://msdn.microsoft.com/en-us/library/wyk4d9cy(v=vs.110).aspx
            //length = Units.Length.FromFeet(.5);
            //Assert.AreEqual("1' 0.0000\"", ((Units.Length)length.Round()).ToString(LengthUnit.FractionalFoot));
        }

        [Test]
        [Category("UnitTests")]
        public void Sorting()
        {
            //tests of units IComparability
            var l1 = Length.FromDouble(-13.0);
            var l2 = Length.FromDouble(27.0);
            var l3 = Length.FromDouble(0.0);
            var l4 = Length.FromDouble(.0000001);

            var lengths = new List<Length> { l4, l3, l1, l2 };
            lengths.Sort();

            Assert.AreEqual(l1.Value, lengths[0].Value);
            Assert.AreEqual(l2.Value, lengths[3].Value);
            Assert.AreEqual(l3.Value, lengths[1].Value);
            Assert.AreEqual(l4.Value, lengths[2].Value);

            var a2 = Area.FromDouble(27.0);
            var a3 = Area.FromDouble(0.0);
            var a4 = Area.FromDouble(.0000001);

            var areas = new List<Area> { a4, a3, a2 };
            areas.Sort();

            Assert.AreEqual(a2.Value, areas[2].Value);
            Assert.AreEqual(a3.Value, areas[0].Value);
            Assert.AreEqual(a4.Value, areas[1].Value);

            var v2 = Volume.FromDouble(27.0);
            var v3 = Volume.FromDouble(0.0);
            var v4 = Volume.FromDouble(.0000001);

            var volumes = new List<Volume> { v4, v3, v2 };
            volumes.Sort();

            Assert.AreEqual(v2.Value, volumes[2].Value);
            Assert.AreEqual(v3.Value, volumes[0].Value);
            Assert.AreEqual(v4.Value, volumes[1].Value);

            //test that we're not comparing units 
            //that can't be compared
            var mixedList = new List<SIUnit> { l2, a4, v4 };
            Assert.Throws<InvalidOperationException>(mixedList.Sort);

        }

        [Test, Category("UnitTests")]
        public void LengthConversions()
        {
            Assert.AreEqual(1.0, Length.Conversions[Length.METERS]);
            Assert.AreEqual(Length.ToMillimeter, Length.Conversions[Length.MILLIMETERS]);
            Assert.AreEqual(Length.ToCentimeter, Length.Conversions[Length.CENTIMETERS]);
            Assert.AreEqual(Length.ToInch, Length.Conversions[Length.INCHES]);
            Assert.AreEqual(Length.ToFoot, Length.Conversions[Length.FEET]);
        }

        [Test, Category("UnitTests")]
        public void AreaConversions()
        {
            Assert.AreEqual(1.0, Area.Conversions[Area.SQUARE_METERS]);
            Assert.AreEqual(Area.ToSquareMillimeters, Area.Conversions[Area.SQUARE_MILLIMETERS]);
            Assert.AreEqual(Area.ToSquareCentimeters, Area.Conversions[Area.SQUARE_CENTIMETERS]);
            Assert.AreEqual(Area.ToSquareInch, Area.Conversions[Area.SQUARE_INCHES]);
            Assert.AreEqual(Area.ToSquareFoot, Area.Conversions[Area.SQUARE_FEET]);
        }

        [Test, Category("UnitTests")]
        public void VolumeConversions()
        {
            Assert.AreEqual(1.0, Volume.Conversions[Volume.CUBIC_METERS]);
            Assert.AreEqual(Volume.ToCubicMillimeter, Volume.Conversions[Volume.CUBIC_MILLIMETERS]);
            Assert.AreEqual(Volume.ToCubicCentimeter, Volume.Conversions[Volume.CUBIC_CENTIMETERS]);
            Assert.AreEqual(Volume.ToCubicInch, Volume.Conversions[Volume.CUBIC_INCHES]);
            Assert.AreEqual(Volume.ToCubicFoot, Volume.Conversions[Volume.CUBIC_FEET]);
        }

        [Test, Category("UnitTests")]
        public void InsolationConversions()
        {
            Assert.AreEqual(1.0, Insolation.Conversions[Insolation.WATT_HOURS_PER_SQUARE_METER]);
            Assert.AreEqual(Insolation.ToKwhMeter2, Insolation.Conversions[Insolation.KILLOWATT_HOURS_PER_SQUARE_METER]);
            Assert.AreEqual(Insolation.ToBTUFoot2, Insolation.Conversions[Insolation.BTU_PER_SQUARE_FOOT]);
        }

        [Test, Category("UnitTests")]
        public void Location_ValidArgs()
        {
            var loc = Location.ByLatitudeAndLongitude(20.0, -20.0);
            Assert.NotNull(loc);
            Assert.AreEqual(loc.Latitude, 20.0);
            Assert.AreEqual(loc.Longitude, -20.0);
        }

        [Test, Category("UnitTests")]
        public void Location_InvalidArgs()
        {
            Assert.Throws<Exception>(() => Location.ByLatitudeAndLongitude(500.0, 10.0));
            Assert.Throws<Exception>(() => Location.ByLatitudeAndLongitude(10.0, 500.0));
        }

        [Test, Category("UnitTests")]
        public void UnitValues()
        {
            var length = Length.FromDouble(1.0, LengthUnit.DecimalFoot);
            Assert.AreEqual(length.UnitValue, Length.ToFoot);

            var area = Area.FromDouble(1.0, AreaUnit.SquareFoot);
            Assert.AreEqual(area.UnitValue, Area.ToSquareFoot);

            var volume = Volume.FromDouble(1.0, VolumeUnit.CubicFoot);
            Assert.AreEqual(volume.UnitValue, Volume.ToCubicFoot);
        }
    }

    internal class ViewModelUnitsOfMeasureDynTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");
            libraries.Add("DynamoUnits.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void CanMapOverUnits()
        {
            var length = Enumerable.Range(1, 5).Select(x => Length.FromDouble(x)).ToList();
            var area = Enumerable.Range(1, 5).Select(x => Area.FromDouble(x)).ToList();
            var volume = Enumerable.Range(1, 5).Select(x => Volume.FromDouble(x)).ToList();

            RunModel(@"core\units\map-numbers-to-units.dyn");

            AssertPreviewValue("97fdd4df-e9dd-4f7f-9494-b2adabfdbdeb", length);
            AssertPreviewValue("4e830faa-d358-4086-ba4c-9b7e70f96681", area);
            AssertPreviewValue("e6ae471f-9cd8-4cbb-bb83-ecdf1785c35f", volume);
            AssertPreviewValue("178b5d28-fbbd-459c-9340-0739fa4946b6", length);
            AssertPreviewValue("6c321ba2-754d-4165-bc9d-59fc27d34014", area);
            AssertPreviewValue("4aa8240b-29cd-420d-8e3f-9d699e6bedd0", volume);
        }
    }
}
