using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dynamo.Measure
{
    public enum DynamoUnitType
    {
        INCHES, 
        FEET,
        MILLIMETERS,
        CENTIMETERS,
        METERS
    }

    public enum DynamoUnitDisplayType
    {
        DECIMAL_INCHES,
        DECIMAL_FEET,
        FRACTIONAL_INCHES,
        FRACTIONAL_FEET_INCHES,
        MILLIMETERS,
        CENTIMETERS,
        METERS
    }

    public interface IDynamoLength
    {
        double Length { get; set; }
        double ConvertTo(DynamoUnitType unitType);
        void ConvertFrom(double value, DynamoUnitType unitType);
    }

    /// <summary>
    /// Base class for all units of measure.
    /// </summary>
    public class DynamoLength<T> where T:IDynamoLength
    {
        private T _item;
        public T Item
        {
            get
            {
                return _item;
            }
            set
            {
                if (_item == null || !_item.Equals(value))
                {
                    _item = value;
                }
            }
        }

        public DynamoLength(double length)
        {
            Item = (T)Activator.CreateInstance(typeof(T), new object[] { });
            Item.Length = length;
        }

        public override string ToString()
        {
            return Item.ToString();
        }

        public string ToDisplayString(DynamoUnitDisplayType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitDisplayType.FRACTIONAL_INCHES:
                    return Utils.ToFractionalInches(Item.ConvertTo(DynamoUnitType.INCHES));

                case DynamoUnitDisplayType.DECIMAL_INCHES:
                    return Item.ConvertTo(DynamoUnitType.INCHES).ToString("0.00", CultureInfo.InvariantCulture) + " in";

                case DynamoUnitDisplayType.DECIMAL_FEET:
                    return Item.ConvertTo(DynamoUnitType.FEET).ToString("0.00", CultureInfo.InvariantCulture) + " ft";

                case DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES:
                    return Utils.ToFeetAndFractionalInches(Item.ConvertTo(DynamoUnitType.FEET));

                case DynamoUnitDisplayType.MILLIMETERS:
                    return Item.ConvertTo(DynamoUnitType.MILLIMETERS).ToString("0.00", CultureInfo.InvariantCulture) + " mm";

                case DynamoUnitDisplayType.CENTIMETERS:
                    return Item.ConvertTo(DynamoUnitType.CENTIMETERS).ToString("0.00", CultureInfo.InvariantCulture) + " cm";

                case DynamoUnitDisplayType.METERS:
                    return Item.ConvertTo(DynamoUnitType.METERS).ToString("0.00", CultureInfo.InvariantCulture) + " m";

                default:
                    return ToString();
            }
        }

        public void FromDisplayString(string value, DynamoUnitDisplayType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitDisplayType.FRACTIONAL_INCHES:
                    Item.ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoUnitType.FEET);
                    break;

                case DynamoUnitDisplayType.DECIMAL_INCHES:
                    Item.ConvertFrom(Utils.ParseUnit(value, "in"), DynamoUnitType.INCHES);
                    break;

                case DynamoUnitDisplayType.DECIMAL_FEET:
                    Item.ConvertFrom(Utils.ParseUnit(value, "ft"), DynamoUnitType.FEET);
                    break;

                case DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES:
                    Item.ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoUnitType.FEET);
                    break;

                case DynamoUnitDisplayType.MILLIMETERS:
                    Item.ConvertFrom(Utils.ParseUnit(value, "mm"), DynamoUnitType.MILLIMETERS);
                    break;

                case DynamoUnitDisplayType.CENTIMETERS:
                    Item.ConvertFrom(Utils.ParseUnit(value, "cm"), DynamoUnitType.CENTIMETERS);
                    break;

                case DynamoUnitDisplayType.METERS:
                    Item.ConvertFrom(Utils.ParseUnit(value, "m"), DynamoUnitType.METERS);
                    break;

                default:
                    Item.Length = 0.0;
                    break;
            }
        }
    }

    /// <summary>
    /// An Inch.
    /// </summary>
    public class Inch : IDynamoLength
    {
        public double Length { get; set; }

        public Inch() {}

        public double ConvertTo(DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.FEET:
                    return Math.Round(Length * 0.083333,4);
                case DynamoUnitType.MILLIMETERS:
                    return Math.Round(Length * 25.4, 4);
                case DynamoUnitType.CENTIMETERS:
                    return Math.Round(Length * 2.54,4);
                case DynamoUnitType.METERS:
                    return Math.Round(Length * .0254, 4);
                default:
                    return Length;
            }
        }

        public void ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.FEET:
                    Length = value / 0.083333;
                    break;

                case DynamoUnitType.MILLIMETERS:
                    Length = value / 25.4;
                    break;

                case DynamoUnitType.CENTIMETERS:
                    Length = value / 2.54;
                    break;

                case DynamoUnitType.METERS:
                    Length = value / .0254;
                    break;

                default:
                    Length = value;
                    break;
            }
        }

        public override string ToString()
        {
            return Utils.ToFractionalInches(Length);
        }

    }

    /// <summary>
    /// A Foot.
    /// </summary>
    public class Foot : IDynamoLength
    {
        public double Length { get; set; }

        public Foot() { }

        public double ConvertTo(DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.INCHES:
                    return Length * 12.0;
                case DynamoUnitType.MILLIMETERS:
                    return Length * 304.8;
                case DynamoUnitType.CENTIMETERS:
                    return Length * 30.48;
                case DynamoUnitType.METERS:
                    return Length * .3048;
                default:
                    return Length;
            }
        }

        public void ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.INCHES:
                    Length = value / 12.0;
                    break;

                case DynamoUnitType.MILLIMETERS:
                    Length = value / 304.8;
                    break;

                case DynamoUnitType.CENTIMETERS:
                    Length = value / 30.48;
                    break;

                case DynamoUnitType.METERS:
                    Length = value / .3048;
                    break;

                default:
                    Length = value;
                    break;
            }
        }

        public override string ToString()
        {
            return Utils.ToFeetAndFractionalInches(Length);
        }
    }

    /// <summary>
    /// A Millimeter.
    /// </summary>
    public class Millimeter : IDynamoLength
    {
        public double Length { get; set; }

        public Millimeter() { }

        public double ConvertTo(DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.FEET:
                    return Length * .003281;
                case DynamoUnitType.INCHES:
                    return Length * .03937;
                case DynamoUnitType.CENTIMETERS:
                    return Length * 0.1;
                case DynamoUnitType.METERS:
                    return Length * 0.001;
                default:
                    return Length;
            }
        }

        public void ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.FEET:
                    Length = value / .003281;
                    break;

                case DynamoUnitType.INCHES:
                    Length = value / .03937;
                    break;

                case DynamoUnitType.CENTIMETERS:
                    Length = value / 0.1;
                    break;

                case DynamoUnitType.METERS:
                    Length = value / 0.001;
                    break;

                default:
                    Length = value;
                    break;
            }
        }

        public override string ToString()
        {
            return Length.ToString("0.00", CultureInfo.InvariantCulture) + " mm";
        }
    }

    /// <summary>
    /// A Centimeter.
    /// </summary>
    public class Centimeter : IDynamoLength
    {
        public double Length { get; set; }

        public Centimeter() { }

        public double ConvertTo(DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.FEET:
                    return Length * 0.032808;
                case DynamoUnitType.INCHES:
                    return Length * 0.393701;
                case DynamoUnitType.MILLIMETERS:
                    return Length * 10;
                case DynamoUnitType.METERS:
                    return Length * .01;
                default:
                    return Length;
            }
        }

        public void ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.FEET:
                    Length = value / 0.032808;
                    break;

                case DynamoUnitType.INCHES:
                    Length = value / 0.393701;
                    break;

                case DynamoUnitType.MILLIMETERS:
                    Length = value / 10;
                    break;

                case DynamoUnitType.METERS:
                    Length = value / .01;
                    break;

                default:
                    Length = value;
                    break;
            }
        }

        public override string ToString()
        {
            return Length.ToString("0.00", CultureInfo.InvariantCulture) + " cm";
        }
    }

    /// <summary>
    /// A Meter.
    /// </summary>
    public class Meter : IDynamoLength
    {
        public double Length { get; set; }

        public Meter() { }

        public double ConvertTo(DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.FEET:
                    return Length * 3.28084;
                case DynamoUnitType.INCHES:
                    return Length * 39.370079;
                case DynamoUnitType.MILLIMETERS:
                    return Length * 1000;
                case DynamoUnitType.CENTIMETERS:
                    return Length * 100;
                default:
                    return Length;
            }
        }

        public void ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.FEET:
                    Length = value / 3.28084;
                    break;

                case DynamoUnitType.INCHES:
                    Length = value / 39.370079;
                    break;

                case DynamoUnitType.MILLIMETERS:
                    Length = value / 1000;
                    break;

                case DynamoUnitType.CENTIMETERS:
                    Length = value / 100;
                    break;

                default:
                    Length = value;
                    break;
            }
        }

        public override string ToString()
        {
            return Length.ToString("0.00", CultureInfo.InvariantCulture) + " m";
        }
    }

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
            m = 0.0;
            if (!double.TryParse(value, out m))
            {
                return 0.0;
            }

            return m;
        }

        /// <summary>
        /// Convert from feet and fractional inches to decimal feet.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double FromFeetAndFractionalInches(string value)
        {
            //for decimals in the length
            //http://stackoverflow.com/questions/308122/simple-regular-expression-for-a-decimal-with-a-precision-of-2

            //to test .net regex
            //http://regexhero.net/tester/

            //The following pattern will accept imperial units specified in
            //any of the following ways:
            //1' -> only fee specified
            //1' 2" -> feet and inches specified
            //1' 2 3/32" -> feet, inches, and fractional inches specified
            //2 3/32" -> inches and fractional inches specified
            //3/32" -> only fractional inches specified

            //string pattern = "(((?<feet>[\-\+]?[0-9])')*\s*("+
            //    "(?<inches>(?<num>[\-\+]?[0-9]+)/(?<den>[0-9]+)*\")|"+
            //    "(?<inches>(?<whole_inch>[\-\+]?[0-9]+)*\s*(?<num>[\-\+]?[0-9]+)/(?<den>[0-9]+)*\")|"+
            //    "(?<inches>(?<whole_inch>[\-\+]?[0-9]+)\"))?)*";

            //unescaped form
            //(((?<feet>[\-\+]?\d+(\.\d{1,2})?)')*\s*((?<inches>(?<num>[\-\+]?\d+(\.\d{1,2})?)/(?<den>\d+(\.\d{1,2})?)*\")|(?<inches>(?<whole_inch>[\-\+]?\d+(\.\d{1,2})?)*\s*(?<num>[\-\+]?\d+(\.\d{1,2})?)/(?<den>\d+(\.\d{1,2})?)*\")|(?<inches>(?<whole_inch>[\-\+]?\d+(\.\d{1,2})?)\"))?)*

            //modified to allow decimals as well
            string pattern = "(((?<feet>[\\-\\+]?\\d+(\\.\\d{1,2})?)')*\\s*(" +
                "(?<inches>(?<num>[\\-\\+]?\\d+(\\.\\d{1,2})?)/(?<den>\\d+(\\.\\d{1,2})?)*\")|" +
                "(?<inches>(?<whole_inch>[\\-\\+]?\\d+(\\.\\d{1,2})?)*\\s*(?<num>[\\-\\+]?\\d+(\\.\\d{1,2})?)/(?<den>\\d+(\\.\\d{1,2})?)*\")|" +
                "(?<inches>(?<whole_inch>[\\-\\+]?\\d+(\\.\\d{1,2})?)\"))?)*";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(value);
            if (match.Success)
            {
                int feet = 0;
                int.TryParse(match.Groups["feet"].Value, out feet);
                int inch = 0;
                int.TryParse(match.Groups["whole_inch"].Value, out inch);

                double fractionalInch = 0.0;
                double numerator = 0.0;
                double denominator = 0.0;
                double.TryParse(match.Groups["num"].Value, out numerator);
                double.TryParse(match.Groups["den"].Value, out denominator);
                if (denominator != 0)
                    fractionalInch = numerator / denominator;

                if (feet < 0)
                    return feet - inch / 12.0 - fractionalInch / 12.0;
                else
                    return feet + inch / 12.0 + fractionalInch / 12.0;
            }
            else return 0.0;
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
    }
}
