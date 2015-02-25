using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Runtime;

namespace DynamoConversions
{
    public enum ConversionDirection { To, From }
    public enum ConversionUnit 
    { Feet, Inches, Millimeters, Centimeters, Meters, Degrees, Radians, Kilograms, Pounds,
        CubicMeters,CubicFoot,SquareMeter,SquareFoot }

    public enum ConversionMetricUnit
    {
        Length,
        Area,
        Volume
    }

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
            {ConversionUnit.Kilograms, 1},
            {ConversionUnit.CubicMeters,1},           
            {ConversionUnit.CubicFoot,1.639},
            {ConversionUnit.SquareMeter,1},
            {ConversionUnit.SquareFoot,0.093},        
        };

        public static readonly Dictionary<ConversionMetricUnit, double> ConversionMetricDictionary =
            new Dictionary<ConversionMetricUnit, double>()
            {
                {ConversionMetricUnit.Length, 1.0},
                {ConversionMetricUnit.Area, 2.0},
                {ConversionMetricUnit.Volume, 3.0}
            };
            
        public static readonly Dictionary<ConversionMetricUnit, List<ConversionUnit>> ConversionMetricLookup =
            new Dictionary<ConversionMetricUnit, List<ConversionUnit>>()
            {               
                {ConversionMetricUnit.Length, new List<ConversionUnit>()
                                    {ConversionUnit.Feet,ConversionUnit.Inches,ConversionUnit.Millimeters,ConversionUnit.Centimeters, 
                                        ConversionUnit.Meters}},
                {ConversionMetricUnit.Area, new List<ConversionUnit>()
                                    {ConversionUnit.SquareMeter,ConversionUnit.SquareFoot}},
                   {ConversionMetricUnit.Volume, new List<ConversionUnit>()
                                    {ConversionUnit.CubicMeters,ConversionUnit.CubicFoot}}
            };

        public static readonly Dictionary<ConversionMetricUnit, ConversionUnit> ConversionDefaults =
            new Dictionary<ConversionMetricUnit, ConversionUnit>()
            {
                {ConversionMetricUnit.Length, ConversionUnit.Meters},
                {ConversionMetricUnit.Area, ConversionUnit.SquareMeter},
                {ConversionMetricUnit.Volume, ConversionUnit.CubicMeters},                    
            };


        
        public static double ConvertUnitTypes(double value, double conversion, double conversionto)
        {
            var convertValue =  value / conversionto;
            var returnval =  convertValue * conversion;
            return returnval;
        }
    }
}
