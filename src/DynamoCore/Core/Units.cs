using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Dynamo.Measure
{
    public enum DynamoUnitType
    {
        Inches, 
        Feet,
        Millimeters,
        Centimeters,
        Meters
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

    /// <summary>
    /// An Inch.
    /// </summary>
    public static class Inch
    {
        public static double ConvertTo(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Feet:
                    return Math.Round(value * 0.083333, 4);
                case DynamoUnitType.Millimeters:
                    return Math.Round(value * 25.4, 4);
                case DynamoUnitType.Centimeters:
                    return Math.Round(value * 2.54, 4);
                case DynamoUnitType.Meters:
                    return Math.Round(value * .0254, 4);
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Feet:
                    return value / 0.083333;
                case DynamoUnitType.Millimeters:
                    return value / 25.4;
                case DynamoUnitType.Centimeters:
                    return value / 2.54;
                case DynamoUnitType.Meters:
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
                    return ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoUnitType.Feet);
                case DynamoUnitDisplayType.DecimalInches:
                    return ConvertFrom(Utils.ParseUnit(value, "in"), DynamoUnitType.Inches);
                case DynamoUnitDisplayType.DecimalFeet:
                    return ConvertFrom(Utils.ParseUnit(value, "ft"), DynamoUnitType.Feet);
                case DynamoUnitDisplayType.FractionalFeetInches:
                    return ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoUnitType.Feet);
                case DynamoUnitDisplayType.Millimeters:
                    return ConvertFrom(Utils.ParseUnit(value, "mm"), DynamoUnitType.Millimeters);
                case DynamoUnitDisplayType.Centimeters:
                    return ConvertFrom(Utils.ParseUnit(value, "cm"), DynamoUnitType.Centimeters);
                case DynamoUnitDisplayType.Meters:
                    return ConvertFrom(Utils.ParseUnit(value, "m"), DynamoUnitType.Meters);
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
                    return ConvertTo(value, DynamoUnitType.Inches).ToString("0.00", CultureInfo.CurrentCulture) + " in";

                case DynamoUnitDisplayType.DecimalFeet:
                    return ConvertTo(value, DynamoUnitType.Feet).ToString("0.00", CultureInfo.CurrentCulture) + " ft";

                case DynamoUnitDisplayType.FractionalFeetInches:
                    return Utils.ToFeetAndFractionalInches(ConvertTo(value, DynamoUnitType.Feet));

                case DynamoUnitDisplayType.Millimeters:
                    return ConvertTo(value, DynamoUnitType.Millimeters).ToString("0.00", CultureInfo.CurrentCulture) + " mm";

                case DynamoUnitDisplayType.Centimeters:
                    return ConvertTo(value, DynamoUnitType.Centimeters).ToString("0.00", CultureInfo.CurrentCulture) + " cm";

                case DynamoUnitDisplayType.Meters:
                    return ConvertTo(value, DynamoUnitType.Meters).ToString("0.00", CultureInfo.CurrentCulture) + " m";

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
        public static double ConvertTo(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Inches:
                    return value * 12.0;
                case DynamoUnitType.Millimeters:
                    return value * 304.8;
                case DynamoUnitType.Centimeters:
                    return value * 30.48;
                case DynamoUnitType.Meters:
                    return value * .3048;
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Inches:
                    return value / 12.0;
                case DynamoUnitType.Millimeters:
                    return value / 304.8;
                case DynamoUnitType.Centimeters:
                    return value / 30.48;
                case DynamoUnitType.Meters:
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
                    return ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoUnitType.Feet);
                case DynamoUnitDisplayType.DecimalInches:
                    return ConvertFrom(Utils.ParseUnit(value, "in"), DynamoUnitType.Inches);
                case DynamoUnitDisplayType.DecimalFeet:
                    return ConvertFrom(Utils.ParseUnit(value, "ft"), DynamoUnitType.Feet);
                case DynamoUnitDisplayType.FractionalFeetInches:
                    return ConvertFrom(Utils.FromFeetAndFractionalInches(value), DynamoUnitType.Feet);
                case DynamoUnitDisplayType.Millimeters:
                    return ConvertFrom(Utils.ParseUnit(value, "mm"), DynamoUnitType.Millimeters);
                case DynamoUnitDisplayType.Centimeters:
                    return ConvertFrom(Utils.ParseUnit(value, "cm"), DynamoUnitType.Centimeters);
                case DynamoUnitDisplayType.Meters:
                    return ConvertFrom(Utils.ParseUnit(value, "m"), DynamoUnitType.Meters);
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
        public static double ConvertTo(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Feet:
                    return value * .003281;
                case DynamoUnitType.Inches:
                    return value * .03937;
                case DynamoUnitType.Centimeters:
                    return value * 0.1;
                case DynamoUnitType.Meters:
                    return value * 0.001;
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoUnitType unitType)
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
        public static double ConvertTo(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Feet:
                    return value * 0.032808;
                case DynamoUnitType.Inches:
                    return value * 0.393701;
                case DynamoUnitType.Millimeters:
                    return value * 10;
                case DynamoUnitType.Meters:
                    return value * .01;
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Feet:
                    return value / 0.032808;
                case DynamoUnitType.Inches:
                    return value / 0.393701;
                case DynamoUnitType.Millimeters:
                    return value / 10;
                case DynamoUnitType.Meters:
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
        public static double ConvertTo(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Feet:
                    return value * 3.28084;
                case DynamoUnitType.Inches:
                    return value * 39.370079;
                case DynamoUnitType.Millimeters:
                    return value * 1000;
                case DynamoUnitType.Centimeters:
                    return value * 100;
                default:
                    return value;
            }
        }

        public static double ConvertFrom(double value, DynamoUnitType unitType)
        {
            switch (unitType)
            {
                case DynamoUnitType.Feet:
                    return value / 3.28084;
                case DynamoUnitType.Inches:
                    return value / 39.370079;
                case DynamoUnitType.Millimeters:
                    return value / 1000;
                case DynamoUnitType.Centimeters:
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
    }
}
