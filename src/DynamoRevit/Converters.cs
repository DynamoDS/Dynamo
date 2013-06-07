using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using Autodesk.Revit.DB;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Controls
{
    /// <summary>
    /// Converts input in project units to decimal feet.
    /// </summary>
    public class RevitProjectUnitsConverter : IValueConverter
    {
        /// <summary>
        /// Convert the value to project units.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double length = System.Convert.ToDouble(value);

            Autodesk.Revit.DB.ProjectUnit projectUnit = dynRevitSettings.Doc.Document.ProjectUnit;
            FormatOptions formatOptions = projectUnit.get_FormatOptions(UnitType.UT_Length);

            switch (formatOptions.Units)
            {
                case DisplayUnitType.DUT_CENTIMETERS:
                    return ToCentimeters(length);
                case DisplayUnitType.DUT_MILLIMETERS:
                    return ToMillimeters(length);
                case DisplayUnitType.DUT_METERS:
                    return ToMeters(length);
                case DisplayUnitType.DUT_FRACTIONAL_INCHES:
                    return ToFractionalInches(length);
                case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
                    return ToFeetAndFractionalInches(length);
                case DisplayUnitType.DUT_DECIMAL_INCHES:
                    return ToDecimalInches(length);
                case DisplayUnitType.DUT_DECIMAL_FEET:
                    return ToDecimalFeet(length);
            }

            return 0.0;
        }

        /// <summary>
        /// Convert the value to decimal feet.       
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //The data binding engine calls this method when it propagates a value from the binding target to the binding source.
            string length = value.ToString();

            Autodesk.Revit.DB.ProjectUnit projectUnit = dynRevitSettings.Doc.Document.ProjectUnit;
            FormatOptions formatOptions = projectUnit.get_FormatOptions(UnitType.UT_Length);

            switch (formatOptions.Units)
            {
                case DisplayUnitType.DUT_CENTIMETERS:
                    return FromCentimeters(length);
                case DisplayUnitType.DUT_MILLIMETERS:
                    return FromMillimeters(length);
                case DisplayUnitType.DUT_METERS:
                    return FromMeters(length);
                case DisplayUnitType.DUT_FRACTIONAL_INCHES:
                    return FromFractionalInches(length);
                case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
                    return FromFeetAndFractionalInches(length);
                case DisplayUnitType.DUT_DECIMAL_INCHES:
                    return FromDecimalInches(length);
                case DisplayUnitType.DUT_DECIMAL_FEET:
                    return FromDecimalFeet(length);
            }

            return 0.0;
        }

        /// <summary>
        /// Convert from decimal feet to fractional inches
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToFractionalInches(double value)
        {
            double inches = value * 12.0; //12.25
            double wholeInches = Math.Floor(inches); //12
            double remainder = inches - wholeInches; //.25

            //1/64" = 0.015625"
            double precision = 0.015625;
            double fractionalPart = Math.Floor(remainder / precision);

            string fraction = "";
            if (fractionalPart != 0.0)
                fraction = string.Format("{0}/64\"", fractionalPart);

            //if there is no fraction, return the whole inches
            if (string.IsNullOrEmpty(fraction))
                return string.Format("{0}\"", wholeInches);

            if (wholeInches != 0.0)
                return string.Format("{0} {1}", wholeInches, fraction);
            else
            {
                if (fractionalPart != 0.0)
                    return string.Format("{0}", fraction);
                else
                    return "0\"";
            }

        }

        /// <summary>
        /// Convert to decimal feet from fractional inches
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double FromFractionalInches(string value)
        {
            return ParseFractionalInches(value);
        }

        private static double ParseFractionalInches(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0.0;

            //ex. 27 3/64"
            //split at any space
            string clean = value.Replace("\"", "");
            string[] parts = clean.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return 0.0;

            double inches = 0.0;
            double remainder = 0.0;

            if (parts.Length == 1)
            {
                if (parts[0].Contains("/"))
                {
                    remainder = CalculateDoubleFromStringFraction(parts[0]);
                }
                else
                {
                    //you have only whole inches ex. 27"
                    if (!double.TryParse(parts[0], out inches))
                        return 0.0;
                }
            }
            else
            {
                //you have only whole inches ex. 27"
                if (!double.TryParse(parts[0], out inches))
                    return 0.0;

                remainder = CalculateDoubleFromStringFraction(parts[1]);
            }

            return inches/12.0 + remainder/12.0;
        }

        private static double CalculateDoubleFromStringFraction(string stringFraction)
        {
            //you have only a fractional part ex. 3/64"
            string[] fractionParts = stringFraction.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            double numerator = 0.0;
            double denominator = 0.0;
            double remainder = 0.0;

            if (!double.TryParse(fractionParts[0], out numerator) || !double.TryParse(fractionParts[1], out denominator))
                remainder = 0.0;
            else
                if (denominator != 0)
                {
                    //don't let any weird math creep in here
                    //if the numerator is 0 then the fraction will
                    //always be zero
                    if (numerator == 0.0)
                        remainder = 0.0;
                    else
                        remainder = numerator / denominator;
                }
                else
                    remainder = 0.0;
            return remainder;
        }

        /// <summary>
        /// Convert from decimal feet to decimal inches
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToDecimalInches(double value)
        {
            return value * 12.0 + "\"";
        }

        /// <summary>
        /// Convert from decimal inches to decimal feet.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double FromDecimalInches(string value)
        {
            double inches = ParseUnit(ref value, "\"");

            return inches / 12.0;
        }

        /// <summary>
        /// Convert from decimal feet to decimal feet
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToDecimalFeet(double value)
        {
            return value + "'";
        }

        /// <summary>
        /// Convert from decimal feet to decimal feet
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double FromDecimalFeet(string value)
        {
            double feet = ParseUnit(ref value, "'");

            return feet;
        }

        /// <summary>
        /// Convert from decimal feet to feet and fractional inches
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToFeetAndFractionalInches(double value)
        {
            double wholeFeet = 0.0;
            double partialFeet = 0.0;

            if (value < 0)
            {
                wholeFeet = Math.Ceiling(value);
                partialFeet = wholeFeet - value;
            }
            else
            {
                wholeFeet = Math.Floor(value);
                partialFeet = value - wholeFeet;
            }

            string feet = "";
            if (wholeFeet != 0.0)
                feet = string.Format("{0}'", wholeFeet);

            return string.Format("{0} {1}", feet, ToFractionalInches(partialFeet));
        }

        /// <summary>
        /// Convert from feet and fractional inches to decimal feet.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double FromFeetAndFractionalInches(string value)
        {
            //ex. 27' 3 3/4"
            string[] parts = value.Split(' ');

            if (parts.Length > 3)
            {
                //we don't know what to do with
                //this value
                return 0.0;
            }

            double feet = 0.0;

            if (parts.Length == 1)
            {
                //you might have inches or feet here
                if(parts[0].Contains("\""))
                {
                    //you only have one part and it is inches
                    //so just return the inches
                    return FromFractionalInches(parts[0]);
                }
            }

            string cleanFeet = parts[0].Replace("'", "");
            if (!double.TryParse(cleanFeet, out feet))
            {
                return 0.0;
            }

            string fractionalInches = "";
            if (parts.Length == 2)
                fractionalInches = parts[1];
            else if(parts.Length == 3)
                fractionalInches = string.Format("{0} {1}", parts[1], parts[2]);

            //if you have 1' 6" you should get 1.5
            if(feet >= 0)
                return feet + FromFractionalInches(fractionalInches);
            //if you have -1' 6" you should get -1.5 NOT -.5
            else
                return feet - FromFractionalInches(fractionalInches);
        }

        /// <summary>
        /// Convert from decimal feet to millimeters
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToMillimeters(double value)
        {
            return value * 304.8 + "mm";
        }

        /// <summary>
        /// Convert from millimeters to decimal feet.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double FromMillimeters(string value)
        {
            double mm = ParseUnit(ref value, "mm");

            return mm / 304.8;
        }

        /// <summary>
        /// Convert from decimal feet to centimeters
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToCentimeters(double value)
        {
            return value * 30.48 + "cm";
        }

        /// <summary>
        /// Convert from centimeters to decimal feet
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double FromCentimeters(string value)
        {
            double cm = ParseUnit(ref value, "cm");

            return cm / 30.48;
        }

        /// <summary>
        /// Convert from decimal feet to meters
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ToMeters(double value)
        {
            return value * 0.3048 + "m";
        }

        /// <summary>
        /// Convert from meters to decimal feet.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double FromMeters(string value)
        {
            double m = ParseUnit(ref value, "m");

            return m / .3048;   
        }

        private static double ParseUnit(ref string value, string unitSymbol)
        {
            double m;
            if (value.Contains(unitSymbol))
                value = value.Replace(unitSymbol, "");
            m = 0.0;
            if (!double.TryParse(value, out m))
            {
                return 0.0;
            }
            return m;
        }
    }
}
