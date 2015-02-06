using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

namespace DynamoConversionsUI
{
    public enum ConversionDirection { To, From }
    public enum ConversionUnit { Feet, Inches, Millimeters, Centimeters, Meters, Degrees, Radians, Kilograms, Pounds }

    [IsVisibleInDynamoLibrary(false)]
    public static class Conversions
    {
        public static readonly Dictionary<ConversionUnit, double> ConversionDictionary = new Dictionary<ConversionUnit, double>()
        {
            {ConversionUnit.Feet, .3048},
            {ConversionUnit.Inches, .0254},
            {ConversionUnit.Millimeters, .001},
            {ConversionUnit.Centimeters, .01},
            {ConversionUnit.Meters, 1.0},
            {ConversionUnit.Degrees, 0.0174532925},
            {ConversionUnit.Radians, 1},
            {ConversionUnit.Pounds, 0.453592},
            {ConversionUnit.Kilograms, 1}
        };

        public static double ConvertToSI(double value, double conversion, double conversionto)
        {
            return value * conversion * conversionto;
        }

        public static double ConverFromSI(double value, double conversion)
        {
            return value * conversion;
        }
    }
}
