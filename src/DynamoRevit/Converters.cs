using System;
using System.Globalization;
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
            DisplayUnitType displayUnit = getDisplayUnitTypeOfFormatUnits();

            //The data binding engine calls this method when it propagates a value from the binding target to the binding source.
            var lengthObj = (DynamoLength<Foot>)parameter;

            //try and parse the value first
            //if it parses as a number, then just accept it
            double length;
            if (double.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.CurrentCulture, out length))
            {
                #region parse with no units specified
                switch (displayUnit)
                {
                    case DisplayUnitType.DUT_CENTIMETERS:
                        lengthObj.FromDisplayString(value.ToString() + "cm", DynamoUnitDisplayType.CENTIMETERS);
                        break;

                    case DisplayUnitType.DUT_MILLIMETERS:
                        lengthObj.FromDisplayString(value.ToString() + "mm", DynamoUnitDisplayType.MILLIMETERS);
                        break;

                    case DisplayUnitType.DUT_METERS:
                        lengthObj.FromDisplayString(value.ToString() + "m", DynamoUnitDisplayType.METERS);
                        break;

                    case DisplayUnitType.DUT_FRACTIONAL_INCHES:
                        lengthObj.FromDisplayString(value.ToString() + "'", DynamoUnitDisplayType.FRACTIONAL_INCHES);
                        break;

                    case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
                        lengthObj.FromDisplayString(value.ToString() + "'", DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES);
                        break;

                    case DisplayUnitType.DUT_DECIMAL_INCHES:
                        lengthObj.FromDisplayString(value.ToString() + "\"", DynamoUnitDisplayType.DECIMAL_INCHES);
                        break;

                    case DisplayUnitType.DUT_DECIMAL_FEET:
                        lengthObj.FromDisplayString(value.ToString() + "'", DynamoUnitDisplayType.DECIMAL_FEET);
                        break;
                }
                #endregion
            }
            else
            {
                #region parse with units specified
                if (value.ToString().Contains("'") && value.ToString().Contains("\""))
                {
                    //fractional feet and inches
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES);
                }
                else if (value.ToString().Contains("'") && !value.ToString().Contains("\""))
                {
                    //fractional feet only
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.FRACTIONAL_FEET_INCHES);
                }
                else if (value.ToString().Contains("ft"))
                {
                    //decimal feet
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.DECIMAL_FEET);
                }
                else if (value.ToString().Contains("in"))
                {
                    //decimal inches
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.DECIMAL_INCHES);
                }
                else if (value.ToString().Contains("\"") && !value.ToString().Contains("'"))
                {
                    //fractional inches only
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.FRACTIONAL_INCHES);
                }
                else if (value.ToString().ToLower().Contains("cm"))
                {
                    //centimeters
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.CENTIMETERS);
                }
                else if (value.ToString().Contains("mm"))
                {
                    //millimeters
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.MILLIMETERS);
                }
                else if (value.ToString().ToLower().Contains("m"))
                {
                    //meters
                    lengthObj.FromDisplayString(value.ToString(), DynamoUnitDisplayType.METERS);
                }
                else
                {
                    //fall back to parsing the length based on the the display unit type
                    switch (displayUnit)
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
                }
                #endregion
            }
            

            return lengthObj.Item.Length;
        }
    }
}
