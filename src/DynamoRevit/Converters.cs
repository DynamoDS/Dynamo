using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

using Autodesk.Revit.DB;

using Dynamo.Utilities;
using Dynamo.Measure;

using System.Reflection;
using RevitServices.Persistence;

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
                    unitObject = (Object)propertyInfoItem.GetValue((Object)DocumentManager.GetInstance().CurrentUIDocument.Document, null);
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
                        unitObject = ds.Invoke(DocumentManager.GetInstance().CurrentUIDocument.Document, argsM);
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
            DisplayUnitType displayUnit = getDisplayUnitTypeOfFormatUnits();

            var feet = (double) value;

            switch (displayUnit)
            {
                case DisplayUnitType.DUT_CENTIMETERS:
                    return Foot.ToDisplayString(feet, DynamoUnitDisplayType.Centimeters);

                case DisplayUnitType.DUT_MILLIMETERS:
                    return Foot.ToDisplayString(feet, DynamoUnitDisplayType.Millimeters);

                case DisplayUnitType.DUT_METERS:
                    return Foot.ToDisplayString(feet, DynamoUnitDisplayType.Meters);

                case DisplayUnitType.DUT_FRACTIONAL_INCHES:
                    return Foot.ToDisplayString(feet, DynamoUnitDisplayType.FractionalInches);

                case DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES:
                    return Foot.ToDisplayString(feet, DynamoUnitDisplayType.FractionalFeetInches);

                case DisplayUnitType.DUT_DECIMAL_INCHES:
                    return Foot.ToDisplayString(feet, DynamoUnitDisplayType.DecimalInches);

                case DisplayUnitType.DUT_DECIMAL_FEET:
                    return Foot.ToDisplayString(feet, DynamoUnitDisplayType.DecimalFeet);
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
                        return total * (1 / 30.48);  

                    case DisplayUnitType.DUT_MILLIMETERS:
                        return total * (1 / 304.8); 

                    case DisplayUnitType.DUT_METERS:
                        return total * (1 / 0.3048);

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

            double fractionalInch = 0.0;
            double feet, inch, m, cm, mm, numerator, denominator;
            Utils.ParseLengthFromString(value.ToString(), out feet, out inch, out m, out cm, out mm, out numerator, out denominator);

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
