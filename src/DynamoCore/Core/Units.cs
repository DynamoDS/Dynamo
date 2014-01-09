using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dynamo.Measure
{
    public enum DynamoLengthUnit
    {
        DecimalInch,
        FractionalInch,
        DecimalFoot,
        FractionalFoot,
        Millimeter,
        Centimeter,
        Meter
    }

    public enum DynamoAreaUnit
    {
        SquareInch, 
        SquareFoot,
        SquareMillimeter,
        SquareCentimeter,
        SquareMeter
    }

    public enum DynamoVolumeUnit
    {
        CubicInch,
        CubicFeet,
        CubicMillimeter,
        CubicCentimeter,
        CubicMeter
    }

    public enum DynamoUnitDisplayType
    {
        DecimalInches,
        DecimalFeet,
        FractionalInches,
        FractionalFeetInches,
        Millimeters,
        Centimeters,
        Meters
    }

    public class UnitsManager
    {
        private static UnitsManager _instance;

        public static UnitsManager Instance
        {
            get { return _instance ?? (_instance = new UnitsManager()); }
        }

        public DynamoVolumeUnit VolumeUnit { get; set; }

        public DynamoAreaUnit AreaUnit { get; set; }

        public DynamoLengthUnit LengthUnit { get; set; }

        public UnitsManager()
        {
            //default units to be set to SI
            LengthUnit = DynamoLengthUnit.FractionalFoot;
            VolumeUnit = DynamoVolumeUnit.CubicMeter;
            AreaUnit = DynamoAreaUnit.SquareMeter;
        }
    }

    public abstract class MeasurementBase
    {
        internal double _value;

        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }

        protected MeasurementBase(double value)
        {
            _value = value;
        }

        /// <summary>
        /// Converts a string representation to an internal double value in SI units.
        /// </summary>
        /// <param name="value"></param>
        public abstract void SetValueFromString(string value);
    }

    /// <summary>
    /// A length stored as meters.
    /// </summary>
    public class Length : MeasurementBase
    {
        public Length(double value):base(value){}

        /// <summary>
        /// Sets the internal value by parsing the string and converting to SI units.
        /// </summary>
        /// <param name="value"></param>
        public override void SetValueFromString(string value)
        {
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (double.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out total))
            {
                switch (UnitsManager.Instance.LengthUnit)
                {
                    case DynamoLengthUnit.Centimeter:
                        _value =  total / 100;
                        return;

                    case DynamoLengthUnit.Millimeter:
                        _value = total / 1000;
                        return;

                    case DynamoLengthUnit.Meter:
                        _value = total;
                        return;

                    case DynamoLengthUnit.FractionalInch:
                        _value = total * .0254;
                        return;

                    case DynamoLengthUnit.FractionalFoot:
                        _value = total * .3048;
                        return;

                    case DynamoLengthUnit.DecimalInch:
                        _value = total * .0254;
                        return;

                    case DynamoLengthUnit.DecimalFoot:
                        _value = total * .3048;
                        return;
                }
            }

            double fractionalInch = 0.0;
            double feet, inch, m, cm, mm, numerator, denominator;
            Utils.ParseLengthFromString(value, out feet, out inch, out m, out cm, out mm, out numerator, out denominator);

            if (denominator != 0)
                fractionalInch = numerator / denominator;

            if (feet < 0)
                total = (feet - inch / 12.0 - fractionalInch / 12.0)*.3048;
            else
                total = (feet + inch / 12.0 + fractionalInch / 12.0)*.3048;

            total += m;
            total += cm/100;
            total += mm/1000;

            _value = total;
        }

        /// <summary>
        /// Returns a string representation of the length in the project units.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (UnitsManager.Instance.LengthUnit)
            {
                case DynamoLengthUnit.Millimeter:
                    return ToMillimeterString();
                case DynamoLengthUnit.Centimeter:
                    return ToCentimeterString();
                case DynamoLengthUnit.Meter:
                    return ToMeterString();
                case DynamoLengthUnit.DecimalInch:
                    return ToDecimalInchString();
                case DynamoLengthUnit.FractionalInch:
                    return ToFractionalInchString();
                case DynamoLengthUnit.DecimalFoot:
                    return ToDecimalFootString();
                case DynamoLengthUnit.FractionalFoot:
                    return ToFractionalFootString();
                default:
                    return ToMeterString();
            }
        }

        private double ToMillimeters()
        {
            return _value * 1000;
        }

        private double ToCentimeters()
        {
            return _value * 100;
        }

        private double ToMeters()
        {
            return _value;
        }

        private double ToInches()
        {
            return _value * 39.370079;
        }

        private double ToFeet()
        {
            return _value * 3.28084;
        }

        private string ToMillimeterString()
        {
            return ToMillimeters().ToString("0.00", CultureInfo.InvariantCulture) + " mm";
        }

        public string ToCentimeterString()
        {
            return ToCentimeters().ToString("0.00", CultureInfo.InvariantCulture) + " cm";
        }

        public string ToMeterString()
        {
            return ToMeters().ToString("0.00", CultureInfo.InvariantCulture) + " m";
        }

        public string ToDecimalInchString()
        {
            return ToInches().ToString("0.00", CultureInfo.CurrentCulture) + " in";
        }

        public string ToFractionalInchString()
        {
            return Utils.ToFractionalInches(ToInches());
        }

        public string ToDecimalFootString()
        {
            return ToFeet().ToString("0.00", CultureInfo.CurrentCulture) + " ft";
        }

        public string ToFractionalFootString()
        {
            return Utils.ToFeetAndFractionalInches(ToFeet());
        }
    }

    /// <summary>
    /// An area stored as square meters.
    /// </summary>
    public class Area : MeasurementBase
    {
        public Area(double value) : base(value){}

        public override void SetValueFromString(string value)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            switch (UnitsManager.Instance.LengthUnit)
            {
                case DynamoLengthUnit.Millimeter:
                    return ToSquareMillimeterString();
                case DynamoLengthUnit.Centimeter:
                    return ToSquareCentimeterString();
                case DynamoLengthUnit.Meter:
                    return ToSquareMeterString();
                case DynamoLengthUnit.DecimalInch:
                    return ToSquareInchString();
                case DynamoLengthUnit.FractionalInch:
                    return ToSquareInchString();
                case DynamoLengthUnit.DecimalFoot:
                    return ToSquareFootString();
                case DynamoLengthUnit.FractionalFoot:
                    return ToSquareFootString();
                default:
                    return ToSquareMeterString();  
            }
        }

        private double ToSquareMillimeters()
        {
            return _value*1000000;
        }

        private double ToSquareCentimeters()
        {
            return _value*10000;
        }

        private double ToSquareMeters()
        {
            return _value;
        }

        private double ToSquareInches()
        {
            return _value*1550;
        }

        private double ToSquareFeet()
        {
            return _value*10.7639;
        }

        private string ToSquareMillimeterString()
        {
            return ToSquareMillimeters().ToString("0.00", CultureInfo.InvariantCulture) + " mm²";
        }

        private string ToSquareCentimeterString()
        {
            return ToSquareCentimeters().ToString("0.00", CultureInfo.InvariantCulture) + " cm²";
        }

        private string ToSquareMeterString()
        {
            return ToSquareMeters().ToString("0.00", CultureInfo.InvariantCulture) + " m²";
        }

        private string ToSquareInchString()
        {
            return ToSquareInches().ToString("0.00", CultureInfo.InvariantCulture) + " in²";
        }

        private string ToSquareFootString()
        {
            return ToSquareFeet().ToString("0.00", CultureInfo.InvariantCulture) + " ft²";
        }
    }

    /// <summary>
    /// A volume stored as cubic meters.
    /// </summary>
    public class Volume : MeasurementBase
    {
        public Volume(double value) : base(value){}

        public override void SetValueFromString(string value)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            switch (UnitsManager.Instance.LengthUnit)
            {
                case DynamoLengthUnit.Millimeter:
                    return ToCubicMillimeterString();
                case DynamoLengthUnit.Centimeter:
                    return ToCubicCentimeterString();
                case DynamoLengthUnit.Meter:
                    return ToCubicMeterString();
                case DynamoLengthUnit.DecimalInch:
                    return ToCubicInchString();
                case DynamoLengthUnit.FractionalInch:
                    return ToCubicInchString();
                case DynamoLengthUnit.DecimalFoot:
                    return ToCubicFootString();
                case DynamoLengthUnit.FractionalFoot:
                    return ToCubicFootString();
                default:
                    return ToCubicMeterString();
            }
        }

        private double ToCubicMillimeters()
        {
            return _value * 1000000000;
        }

        private double ToCubicCentimeters()
        {
            return _value * 1000000;
        }

        private double ToCubicMeters()
        {
            return _value;
        }

        private double ToCubicInches()
        {
            return _value * 61023.7;
        }

        private double ToCubicFeet()
        {
            return _value * 35.3147;
        }

        private string ToCubicMillimeterString()
        {
            return ToCubicMillimeters().ToString("0.00", CultureInfo.InvariantCulture) + " mm³";
        }

        private string ToCubicCentimeterString()
        {
            return ToCubicCentimeters().ToString("0.00", CultureInfo.InvariantCulture) + " cm³";
        }

        private string ToCubicMeterString()
        {
            return ToCubicMeters().ToString("0.00", CultureInfo.InvariantCulture) + " m³";
        }

        private string ToCubicInchString()
        {
            return ToCubicInches().ToString("0.00", CultureInfo.InvariantCulture) + " in³";
        }

        private string ToCubicFootString()
        {
            return ToCubicFeet().ToString("0.00", CultureInfo.InvariantCulture) + " ft³";
        }
    }

    /*
    /// <summary>
    /// An Inch.
    /// </summary>
    public static class Inch
    {
        public static double ConvertTo(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Foot:
                    return Math.Round(value * 0.083333, 4);
                case DynamoLengthUnit.Millimeter:
                    return Math.Round(value * 25.4, 4);
                case DynamoLengthUnit.Centimeter:
                    return Math.Round(value * 2.54, 4);
                case DynamoLengthUnit.Meter:
                    return Math.Round(value * .0254, 4);
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Foot:
                    return value / 0.083333;
                case DynamoLengthUnit.Millimeter:
                    return value / 25.4;
                case DynamoLengthUnit.Centimeter:
                    return value / 2.54;
                case DynamoLengthUnit.Meter:
                    return value / .0254;
                default:
                    return value;
            }
        }

        public static double FromDisplayString(string value, DynamoUnitDisplayType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitDisplayType.FractionalInches:
                    return ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoLengthUnit.Foot);
                case DynamoUnitDisplayType.DecimalInches:
                    return ConvertFrom(Utils.ParseUnit(value, "in"), DynamoLengthUnit.Inch);
                case DynamoUnitDisplayType.DecimalFeet:
                    return ConvertFrom(Utils.ParseUnit(value, "ft"), DynamoLengthUnit.Foot);
                case DynamoUnitDisplayType.FractionalFeetInches:
                    return ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoLengthUnit.Foot);
                case DynamoUnitDisplayType.Millimeters:
                    return ConvertFrom(Utils.ParseUnit(value, "mm"), DynamoLengthUnit.Millimeter);
                case DynamoUnitDisplayType.Centimeters:
                    return ConvertFrom(Utils.ParseUnit(value, "cm"), DynamoLengthUnit.Centimeter);
                case DynamoUnitDisplayType.Meters:
                    return ConvertFrom(Utils.ParseUnit(value, "m"), DynamoLengthUnit.Meter);
                default:
                    return 0.0;
            }
        }

        public static string ToDisplayString(double value, DynamoUnitDisplayType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitDisplayType.FractionalInches:
                    return Utils.ToFractionalInches(ConvertTo(value, DynamoLengthUnit.Inch));

                case DynamoUnitDisplayType.DecimalInches:
                    return ConvertTo(value, DynamoLengthUnit.Inch).ToString("0.00", CultureInfo.CurrentCulture) + " in";

                case DynamoUnitDisplayType.DecimalFeet:
                    return ConvertTo(value, DynamoLengthUnit.Foot).ToString("0.00", CultureInfo.CurrentCulture) + " ft";

                case DynamoUnitDisplayType.FractionalFeetInches:
                    return Utils.ToFeetAndFractionalInches(ConvertTo(value, DynamoLengthUnit.Foot));

                case DynamoUnitDisplayType.Millimeters:
                    return ConvertTo(value, DynamoLengthUnit.Millimeter).ToString("0.00", CultureInfo.CurrentCulture) + " mm";

                case DynamoUnitDisplayType.Centimeters:
                    return ConvertTo(value, DynamoLengthUnit.Centimeter).ToString("0.00", CultureInfo.CurrentCulture) + " cm";

                case DynamoUnitDisplayType.Meters:
                    return ConvertTo(value, DynamoLengthUnit.Meter).ToString("0.00", CultureInfo.CurrentCulture) + " m";

                default:
                    return value.ToString();
            }
        }

        public static string AsString(double value)
        {
            return Utils.ToFractionalInches(value);
        }
    }

    /// <summary>
    /// A Foot.
    /// </summary>
    public static class Foot
    {
        public static double ConvertTo(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Inch:
                    return value * 12.0;
                case DynamoLengthUnit.Millimeter:
                    return value * 304.8;
                case DynamoLengthUnit.Centimeter:
                    return value * 30.48;
                case DynamoLengthUnit.Meter:
                    return value * .3048;
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Inch:
                    return value / 12.0;
                case DynamoLengthUnit.Millimeter:
                    return value / 304.8;
                case DynamoLengthUnit.Centimeter:
                    return value / 30.48;
                case DynamoLengthUnit.Meter:
                    return value / .3048;
                default:
                    return value;
            }
        }

        public static double FromDisplayString(string value, DynamoUnitDisplayType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitDisplayType.FractionalInches:
                    return ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoLengthUnit.Foot);
                case DynamoUnitDisplayType.DecimalInches:
                    return ConvertFrom(Utils.ParseUnit(value, "in"), DynamoLengthUnit.Inch);
                case DynamoUnitDisplayType.DecimalFeet:
                    return ConvertFrom(Utils.ParseUnit(value, "ft"), DynamoLengthUnit.Foot);
                case DynamoUnitDisplayType.FractionalFeetInches:
                    return ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoLengthUnit.Foot);
                case DynamoUnitDisplayType.Millimeters:
                    return ConvertFrom(Utils.ParseUnit(value, "mm"), DynamoLengthUnit.Millimeter);
                case DynamoUnitDisplayType.Centimeters:
                    return ConvertFrom(Utils.ParseUnit(value, "cm"), DynamoLengthUnit.Centimeter);
                case DynamoUnitDisplayType.Meters:
                    return ConvertFrom(Utils.ParseUnit(value, "m"), DynamoLengthUnit.Meter);
                default:
                    return 0.0;
            }
        }

        public static string ToDisplayString(double value, DynamoUnitDisplayType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitDisplayType.FractionalInches:
                    return Utils.ToFractionalInches(ConvertTo(value, DynamoUnitType.Inches));

                case DynamoUnitDisplayType.DecimalInches:
                    return ConvertTo(value, DynamoUnitType.Inches).ToString("0.00", CultureInfo.CurrentCulture) + "in";

                case DynamoUnitDisplayType.DecimalFeet:
                    return ConvertTo(value, DynamoUnitType.Feet).ToString("0.00", CultureInfo.CurrentCulture) + "ft";

                case DynamoUnitDisplayType.FractionalFeetInches:
                    return Utils.ToFeetAndFractionalInches(ConvertTo(value, DynamoUnitType.Feet));

                case DynamoUnitDisplayType.Millimeters:
                    return ConvertTo(value, DynamoUnitType.Millimeters).ToString("0.00", CultureInfo.CurrentCulture) + "mm";

                case DynamoUnitDisplayType.Centimeters:
                    return ConvertTo(value, DynamoUnitType.Centimeters).ToString("0.00", CultureInfo.CurrentCulture) + "cm";

                case DynamoUnitDisplayType.Meters:
                    return ConvertTo(value, DynamoUnitType.Meters).ToString("0.00", CultureInfo.CurrentCulture) + "m";

                default:
                    return value.ToString();
            }
        }

        public static string AsString(double value)
        {
            return Utils.ToFeetAndFractionalInches(value);
        }
    }

    /// <summary>
    /// A Millimeter.
    /// </summary>
    public static class Millimeter
    {
        public static double ConvertTo(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Foot:
                    return value * .003281;
                case DynamoLengthUnit.Inch:
                    return value * .03937;
                case DynamoLengthUnit.Centimeter:
                    return value * 0.1;
                case DynamoLengthUnit.Meter:
                    return value * 0.001;
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Feet:
                    return value / .003281;
                case DynamoUnitType.Inches:
                    return value / .03937;
                case DynamoUnitType.Centimeters:
                    return value / 0.1;
                case DynamoUnitType.Meters:
                    return value / 0.001;
                default:
                    return value;
            }
        }

        public static string AsString(double value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture) + " mm";
        }
    }

    /// <summary>
    /// A Centimeter.
    /// </summary>
    public static class Centimeter
    {
        public static double ConvertTo(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Foot:
                    return value * 0.032808;
                case DynamoLengthUnit.Inch:
                    return value * 0.393701;
                case DynamoLengthUnit.Millimeter:
                    return value * 10;
                case DynamoLengthUnit.Meter:
                    return value * .01;
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Foot:
                    return value / 0.032808;
                case DynamoLengthUnit.Inch:
                    return value / 0.393701;
                case DynamoLengthUnit.Millimeter:
                    return value / 10;
                case DynamoLengthUnit.Meter:
                    return value / .01;
                default:
                    return value;
            }
        }

        public static string AsString(double value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture) + " cm";
        }
    }

    /// <summary>
    /// A Meter.
    /// </summary>
    public static class Meter
    {
        public static double ConvertTo(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Foot:
                    return value * 3.28084;
                case DynamoLengthUnit.Inch:
                    return value * 39.370079;
                case DynamoLengthUnit.Millimeter:
                    return value * 1000;
                case DynamoLengthUnit.Centimeter:
                    return value * 100;
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoLengthUnit unitType)
        {
            switch (unitType)
            {
                case DynamoLengthUnit.Foot:
                    return value / 3.28084;
                case DynamoLengthUnit.Inch:
                    return value / 39.370079;
                case DynamoLengthUnit.Millimeter:
                    return value / 1000;
                case DynamoLengthUnit.Centimeter:
                    return value / 100;
                default:
                    return value;
            }
        }

        public static string AsString(double value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture) + " m";
        }
    }
    */

    public static class Extensions
    {
        public static bool AlmostEquals(this double double1, double double2, double precision)
        {
            return (Math.Abs(double1 - double2) <= precision);
        }
    }

    /// <summary>
    /// Utility class for operating on units of measure.
    /// </summary>
    public class Utils
    {
        public static string ParseWholeInchesToString(double value)
        {
            double result = value <0 ? 
                Math.Abs(System.Math.Ceiling(value)): 
                Math.Abs(System.Math.Floor(value));

            if (result.AlmostEquals(0.0, 0.00001))
                return "";
            return result.ToString();
        }

        public static string ParsePartialInchesToString(double value, double precision)
        {
            string result = value < 0? 
                Utils.CreateFraction(Math.Abs(value - Math.Ceiling(value)), 0.015625):
                Utils.CreateFraction(Math.Abs(value - Math.Floor(value)), 0.015625);

            return result;
        }

        /// <summary>
        /// Given a double value, create a string fraction with given precision.
        /// </summary>
        /// <param name="value">The value to convert to a fractional representation.</param>
        /// <param name="precision">Fractional precision described as a double. i.e. 1/64th -> 0.015625</param>
        /// <returns>A string representing the fraction.</returns>
        public static string CreateFraction(double value, double precision)
        {
            double numerator = 0.0;
            numerator = value < 0 ?
                Math.Floor(value / precision) :
                Math.Ceiling(value / precision);

            double denominator = 1 / precision;

            if (numerator.AlmostEquals(denominator, 0.00001))
                return "1";

            if (numerator != 0.0)
            {
                while (numerator % 2 == 0.0)
                {
                    numerator = numerator/2;
                    denominator = denominator/2;
                }

                return string.Format("{0}/{1}", numerator, denominator);
            }

            return "";
        }

        public static double ParseUnit(string value, string unitSymbol)
        {
            double m;

            if (value.ToLower().Contains(unitSymbol))
                value = value.Replace(unitSymbol, "");

            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out m))
            {
                return 0.0;
            }

            return m;
        }

        ///<summary>
        ///Convert from feet and fractional inches to decimal feet.
        ///</summary>
        ///<param name="value"></param>
        ///<returns></returns>
        public static double FromFeetAndFractionalInches(string value)
        {
            double fractionalInch = 0.0;

            double feet, inch, m, cm, mm, numerator, denominator;
            Utils.ParseLengthFromString(value.ToString(), out feet, out inch, out m, out cm, out mm, out numerator, out denominator);

            if (denominator != 0.0)
                fractionalInch = numerator / denominator;

            double sign = 1;
            if (value.StartsWith("-"))
            {
                sign *= -1;
            }

            if (feet < 0)
                return feet - inch / 12.0 - fractionalInch / 12.0;
            else
                return feet + inch / 12.0 + fractionalInch / 12.0;
        }

        /// <summary>
        /// Convert from decimal feet to feet and fractional inches
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToFeetAndFractionalInches(double decimalFeet)
        {
            double wholeFeet = 0.0;
            double partialFeet = 0.0;

            if (decimalFeet < 0)
            {
                wholeFeet = Math.Ceiling(decimalFeet);
                if (wholeFeet == 0)
                    partialFeet = decimalFeet;
                else
                    partialFeet = wholeFeet - decimalFeet;
            }
            else
            {
                wholeFeet = Math.Floor(decimalFeet);
                partialFeet = decimalFeet - wholeFeet;
            }

            string fractionalInches = ToFractionalInches(Math.Round(partialFeet * 12.0,4));

            if (fractionalInches == "11 1\"")
            {
                //add a foot to the whole feet
                wholeFeet += 1.0;
                fractionalInches = "";
            }

            string feet = "";
            if (wholeFeet != 0.0)
                feet = string.Format("{0}'", wholeFeet);

            if (wholeFeet.AlmostEquals(0.0, 0.00001) && (partialFeet * 12.0).AlmostEquals(0.0,0.00001))
                feet = "0'";

            return string.Format("{0} {1}", feet, fractionalInches).Trim();
        }

        public static string ToFractionalInches(double decimalInches)
        {
            string inches = Utils.ParseWholeInchesToString(decimalInches);
            string fraction = Utils.ParsePartialInchesToString(decimalInches, 0.015625);

            string sign = decimalInches < 0?"-":"";
            
            if(string.IsNullOrEmpty(inches) && string.IsNullOrEmpty(fraction))
                return "0\"";
            else if(string.IsNullOrEmpty(fraction))
                return string.Format("{0}{1}\"", sign, inches).Trim();
            else if(string.IsNullOrEmpty(inches))
                return string.Format("{0}{1}\"", sign, fraction).Trim();
            
            return string.Format("{0}{1} {2}\"", sign, inches, fraction).Trim();
        }
    
        public static void ParseLengthFromString(string value, out double feet, 
            out double inch, out double m, out double cm, out double mm, out double numerator, out double denominator )
        {
            string pattern = @"(((?<ft>((\+|-)?\d+([.,]\d{1,2})?))('|ft))*\s*((?<in>(?<num>(\+|-)?\d+([.,]\d{1,2})?)/(?<den>\d+([.,]\d{1,2})?)*(""|in))|(?<in>(?<wholeInch>(\+|-)?\d+([.,]\d{1,2})?)*(\s|-)*(?<num>(\+|-)?\d+([.,]\d{1,2})?)/(?<den>\d+([.,]\d{1,2})?)*(""|in))|(?<in>(?<wholeInch>(\+|-)?\d+([.,]\d{1,2})?)(""|in)))?)*((?<m>((\+|-)?\d+([.,]\d{1,2})?))m($|\s))*((?<cm>((\+|-)?\d+([.,]\d{1,2})?))cm($|\s))*((?<mm>((\+|-)?\d+([.,]\d{1,2})?))mm($|\s))*";

            feet = 0.0;
            inch = 0.0;
            m = 0.0;
            cm = 0.0;
            mm = 0.0;
            numerator = 0.0;
            denominator = 0.0;

            const RegexOptions opts = RegexOptions.None;
            var regex = new Regex(pattern, opts);
            Match match = regex.Match(value.Trim().ToLower());
            if (match.Success)
            {
                //parse imperial values
                double.TryParse(match.Groups["ft"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out feet);
                double.TryParse(match.Groups["wholeInch"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out inch);
                double.TryParse(match.Groups["num"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                                out numerator);
                double.TryParse(match.Groups["den"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture,
                                out denominator);

                //parse metric values
                double.TryParse(match.Groups["m"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out m);
                double.TryParse(match.Groups["cm"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out cm);
                double.TryParse(match.Groups["mm"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out mm);
            }
        }

        public static void ParseVolumeFromString()
        {
            
        }

        public static void ParseAreaFromString()
        {
        
        }
    }
}
