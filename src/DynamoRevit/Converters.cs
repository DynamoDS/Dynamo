using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Measure;

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

            var lengthObj = (DynamoLength<Foot>)parameter;
            lengthObj.Item.Length = length;

            Autodesk.Revit.DB.ProjectUnit projectUnit = dynRevitSettings.Doc.Document.ProjectUnit;
            FormatOptions formatOptions = projectUnit.get_FormatOptions(UnitType.UT_Length);

            switch (formatOptions.Units)
            {
                case DisplayUnitType.DUT_CENTIMETERS:
                    return lengthObj.ToDisplayString(DynamoUnitDisplayType.CENTIMETERS);

                case DisplayUnitType.DUT_MILLIMETERS:
                    return lengthObj.ToDisplayString(DynamoUnitDisplayType.MILLIMETERS);

                case DisplayUnitType.DUT_METERS:
                    return lengthObj.ToDisplayString(DynamoUnitDisplayType.METERS);

                case DisplayUnitType.DUT_FRACTIONAL_INCHES:
                    return lengthObj.ToDisplayString(DynamoUnitDisplayType.FRACTIONAL_INCHES);

                case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
                    return lengthObj.ToDisplayString(DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES);

                case DisplayUnitType.DUT_DECIMAL_INCHES:
                    return lengthObj.ToDisplayString(DynamoUnitDisplayType.DECIMAL_INCHES);

                case DisplayUnitType.DUT_DECIMAL_FEET:
                    return lengthObj.ToDisplayString(DynamoUnitDisplayType.DECIMAL_FEET);
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
            //try and parse the value first
            //if it parses as a number, then just accept it
            double length;
            if (double.TryParse(value.ToString(), out length))
                return length;

            //The data binding engine calls this method when it propagates a value from the binding target to the binding source.
            var lengthObj = (DynamoLength<Foot>)parameter;

            Autodesk.Revit.DB.ProjectUnit projectUnit = dynRevitSettings.Doc.Document.ProjectUnit;
            FormatOptions formatOptions = projectUnit.get_FormatOptions(UnitType.UT_Length);

            switch (formatOptions.Units)
            {
                case DisplayUnitType.DUT_CENTIMETERS:
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.CENTIMETERS);
                    break;

                case DisplayUnitType.DUT_MILLIMETERS:
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.MILLIMETERS);
                    break;

                case DisplayUnitType.DUT_METERS:
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.METERS);
                    break;

                case DisplayUnitType.DUT_FRACTIONAL_INCHES:
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.FRACTIONAL_INCHES);
                    break;

                case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES);
                    break;

                case DisplayUnitType.DUT_DECIMAL_INCHES:
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.DECIMAL_INCHES);
                    break;

                case DisplayUnitType.DUT_DECIMAL_FEET:
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.DECIMAL_FEET);
                    break;
            }

            return lengthObj.Item.Length;
        }
    }
}
