using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

using Autodesk.Revit.DB;

using Dynamo.Utilities;
using Dynamo.Measure;

using System.Reflection;

namespace Dynamo.Controls
{
    /// <summary>
    /// Converts input in project units to decimal feet.
    /// </summary>
    public class RevitProjectUnitsConverter : IValueConverter
    {
        DisplayUnitType getDisplayUnitTypeOfFormatUnits()
        {
            Type RevitDoc = typeof(Autodesk.Revit.DB.Document);

            var propertyInfo = RevitDoc.GetProperties();

            Object unitObject = null;
            Type ProjectUnitType = null;

            foreach (PropertyInfo propertyInfoItem in propertyInfo)
            {
                if (propertyInfoItem.Name == "ProjectUnit")
                {
                    //r2013
                    System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(RevitDoc);
                    ProjectUnitType = revitAPIAssembly.GetType("Autodesk.Revit.DB.ProjectUnit", false);
                    unitObject = (Object)propertyInfoItem.GetValue((Object)dynRevitSettings.Doc.Document, null);
                    break;
                }
            }
            if (unitObject == null)
            {
                MethodInfo[] docMethods =  RevitDoc.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                foreach (MethodInfo ds in docMethods)
                {
                    if (ds.Name == "GetUnits")
                    {
                        //r2014
                        object[] argsM = new object[0];
                        unitObject = ds.Invoke(dynRevitSettings.Doc.Document, argsM);
                        System.Reflection.Assembly revitAPIAssembly = System.Reflection.Assembly.GetAssembly(RevitDoc);
                        ProjectUnitType = revitAPIAssembly.GetType("Autodesk.Revit.DB.Units", false);
                        break;
                    }
                }
            }

            if (unitObject != null)
            {
                MethodInfo[] unitsMethods = ProjectUnitType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
 
                foreach (MethodInfo ms in unitsMethods)
                {
                    if (ms.Name == "GetFormatOptions" || ms.Name == "get_FormatOptions")
                    {
                        object[] argsM = new object[1];
                        argsM[0] = UnitType.UT_Length;

                        FormatOptions LengthFormatOptions = (FormatOptions)ms.Invoke(unitObject, argsM);
                        if (LengthFormatOptions != null)
                        {
                            Type FormatOptionsType = typeof(Autodesk.Revit.DB.FormatOptions);
                            var FormatOptionsPropertyInfo = FormatOptionsType.GetProperties();
                            foreach (PropertyInfo propertyInfoItem2 in FormatOptionsPropertyInfo)
                            {
                                if (propertyInfoItem2.Name == "Units")
                                {
                                    //r2013
                                    return (DisplayUnitType)propertyInfoItem2.GetValue((Object)LengthFormatOptions, null);
                                }
                                else if (propertyInfoItem2.Name == "DisplayUnits")
                                {
                                    //r2014
                                    return (DisplayUnitType)propertyInfoItem2.GetValue((Object)LengthFormatOptions, null);
                                }
                            }
                        }
                    }
                }
            }
            return new DisplayUnitType();
        }
        
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
            double length = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);

            var lengthObj = (DynamoLength<Foot>)parameter;
            lengthObj.Item.Length = length;

            DisplayUnitType displayUnit = getDisplayUnitTypeOfFormatUnits();

            switch (displayUnit)
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
            //first try to parse the input as a number
            //it it's parsable, then just cram it into
            //whatever the project units are
            double total = 0.0;
            if (double.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.CurrentCulture, out total))
            {
                DisplayUnitType displayUnit = getDisplayUnitTypeOfFormatUnits();
                switch (displayUnit)
                {
                    case DisplayUnitType.DUT_CENTIMETERS:
                        return total*0.032808;

                    case DisplayUnitType.DUT_MILLIMETERS:
                        return total * 0.003281;

                    case DisplayUnitType.DUT_METERS:
                        return total*3.28084;

                    case DisplayUnitType.DUT_FRACTIONAL_INCHES:
                        return total/12.0;

                    case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
                        return total;

                    case DisplayUnitType.DUT_DECIMAL_INCHES:
                        return total/12;

                    case DisplayUnitType.DUT_DECIMAL_FEET:
                        return total;
                }
            }

            string pattern = @"(((?<ft>((\+|-)?\d+([.,]\d{1,2})?))('|ft))*\s*((?<in>(?<num>(\+|-)?\d+([.,]\d{1,2})?)/(?<den>\d+([.,]\d{1,2})?)*(""|in))|(?<in>(?<wholeInch>(\+|-)?\d+([.,]\d{1,2})?)*(\s|-)*(?<num>(\+|-)?\d+([.,]\d{1,2})?)/(?<den>\d+([.,]\d{1,2})?)*(""|in))|(?<in>(?<wholeInch>(\+|-)?\d+([.,]\d{1,2})?)(""|in)))?)*((?<m>((\+|-)?\d+([.,]\d{1,2})?))m($|\s))*((?<cm>((\+|-)?\d+([.,]\d{1,2})?))cm($|\s))*((?<mm>((\+|-)?\d+([.,]\d{1,2})?))mm($|\s))*";
            
            int feet = 0;
            int inch = 0;
            int mm = 0;
            int cm = 0;
            int m = 0;
            double numerator = 0.0;
            double denominator = 0.0;
            double fractionalInch = 0.0;

            const RegexOptions opts = RegexOptions.None;
            var regex = new Regex(pattern, opts);
            Match match = regex.Match(value.ToString().Trim().ToLower());
            if (match.Success)
            {
                //parse imperial values
                int.TryParse(match.Groups["ft"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out feet);
                int.TryParse(match.Groups["wholeInch"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out inch);
                double.TryParse(match.Groups["num"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture,
                                out numerator);
                double.TryParse(match.Groups["den"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture,
                                out denominator);

                //parse metric values
                int.TryParse(match.Groups["m"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out m);
                int.TryParse(match.Groups["cm"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out cm);
                int.TryParse(match.Groups["mm"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out mm);

            }

            if (denominator != 0)
                fractionalInch = numerator / denominator;

            if (feet < 0)
                total = feet - inch / 12.0 - fractionalInch / 12.0;
            else
                total = feet + inch / 12.0 + fractionalInch / 12.0;

            total += m*3.28084;
            total += cm*0.032808;
            total += mm*0.003281;

            return total;
        }
    }
}
