using System.Collections.Generic;

using Autodesk.DesignScript.Runtime;

namespace DynamoConversions
{
    [IsVisibleInDynamoLibrary(false)]
    public enum ConversionDirection { To, From }

    [IsVisibleInDynamoLibrary(false)]
    public enum ConversionUnit
    {
        Feet, Inches, Millimeters, Centimeters, Decimeters, Meters,
        SquareMeter, SquareFoot, SquareInch, SquareCentimeter, SquareMillimeter,
        Acres, Hectares, CubicMeters, CubicFoot, CubicYards, CubicInches, CubicCentimeter,
        CubicMillimeter, Litres, USGallons
    }

    [IsVisibleInDynamoLibrary(false)]
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
            {ConversionUnit.Decimeters, .1},
            {ConversionUnit.Meters, 1.0},    
            {ConversionUnit.SquareMeter,1},
            {ConversionUnit.SquareFoot,0.093}, 
            {ConversionUnit.SquareInch,0.0006451612}, 
            {ConversionUnit.SquareCentimeter,0.0001}, 
            {ConversionUnit.SquareMillimeter,0.000001}, 
            {ConversionUnit.Acres,4046.86}, 
            {ConversionUnit.Hectares,10000},
            {ConversionUnit.CubicMeters,1},  
            {ConversionUnit.CubicFoot,0.0283},     
            {ConversionUnit.CubicYards,0.765},         
            {ConversionUnit.CubicInches,1/61023.7},
            {ConversionUnit.CubicCentimeter,0.000001}, 
            {ConversionUnit.CubicMillimeter,0.00000001},
            {ConversionUnit.Litres,0.001}, 
            {ConversionUnit.USGallons,0.003785}, 
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
                                        ConversionUnit.Meters,ConversionUnit.Decimeters}},
                {ConversionMetricUnit.Area, new List<ConversionUnit>()
                                    {ConversionUnit.SquareMeter,ConversionUnit.SquareFoot,ConversionUnit.SquareInch,
                                        ConversionUnit.SquareCentimeter,ConversionUnit.SquareMillimeter,
                                        ConversionUnit.Acres,ConversionUnit.Hectares}},
                   {ConversionMetricUnit.Volume, new List<ConversionUnit>()
                   {
                       ConversionUnit.CubicMeters,ConversionUnit.CubicFoot,ConversionUnit.CubicInches,
                       ConversionUnit.CubicCentimeter,ConversionUnit.CubicMillimeter,ConversionUnit.CubicYards,
                       ConversionUnit.Litres,ConversionUnit.USGallons
                   }}
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
