using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Runtime;

namespace DynamoConversions
{
    public enum ConversionDirection { To, From }
    public enum ConversionUnit { Feet, Inches, Millimeters, Centimeters, Meters, Degrees, Radians, Kilograms, Pounds }
    public enum ConversionMetricUnit { Length,Mass,Area,Volume,Angle} 

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

        public static readonly Dictionary<ConversionMetricUnit, double> ConversionMetricDictionary =
            new Dictionary<ConversionMetricUnit, double>()
            {
                {ConversionMetricUnit.Length, 1.0},
                {ConversionMetricUnit.Mass, 2.0},
                {ConversionMetricUnit.Angle, 3.0}
            };
            
        public static readonly Dictionary<ConversionMetricUnit, List<ConversionUnit>> ConversionMetricLookup =
            new Dictionary<ConversionMetricUnit, List<ConversionUnit>>()
            {               
                {ConversionMetricUnit.Length, new List<ConversionUnit>()
                                    {ConversionUnit.Feet,ConversionUnit.Inches,ConversionUnit.Millimeters,ConversionUnit.Centimeters, 
                                        ConversionUnit.Meters}},
                {ConversionMetricUnit.Mass, new List<ConversionUnit>()
                                    {ConversionUnit.Pounds,ConversionUnit.Kilograms}},
                   {ConversionMetricUnit.Angle, new List<ConversionUnit>()
                                    {ConversionUnit.Degrees,ConversionUnit.Radians}}
            };

        public static readonly Dictionary<ConversionMetricUnit, ConversionUnit> ConversionDefaults =
            new Dictionary<ConversionMetricUnit, ConversionUnit>()
            {
                {ConversionMetricUnit.Length, ConversionUnit.Meters},
                {ConversionMetricUnit.Mass, ConversionUnit.Kilograms},
                {ConversionMetricUnit.Angle, ConversionUnit.Degrees},                    
            };

       
        public static double ConvertUnitTypes(double value, double conversion, double conversionto)
        {
            var convertValue =  value / conversionto;
            var returnval =  convertValue * conversion;
            return returnval;
        }
    }
}
