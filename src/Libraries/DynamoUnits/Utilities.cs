using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using System.Reflection;
using System.IO;

namespace DynamoUnits
{
    public static class Utilities
    {
        static Utilities()
        {
            try
            {
                unitsEngine = new ForgeUnitsCLR.UnitsEngine();
                var directory = Path.Combine(AssemblyDirectory, "unit");
                SchemasCLR.SchemaUtility.addDefinitionsFromFolder(directory, unitsEngine);
                unitsEngine.resolveSchemas();
            }
            catch
            {
                //Schemas failed to resolve
            }
            
        }

        /// <summary>
        /// Format used for display of numbers within Dynamo UI
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static string DisplayPrecisionFormat { get; set; } = "f4";

        /// <summary>
        /// This enum enables quick selection of value (string representation) formatting. It also enables us to add
        /// additional formats should they be needed.
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public enum NumberFormat
        {
            /// <summary>
            /// 
            /// </summary>
            None = 0,
            /// <summary>
            /// 
            /// </summary>
            Decimal = 1,
            /// <summary>
            /// 
            /// </summary>
            Fraction = 2
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <returns></returns>
        public static double ConvertByUnits(double value, Unit fromUnit, Unit toUnit)
        {
            return ForgeUnitsEngine.convert(value, fromUnit.TypeId, toUnit.TypeId);
        }

        /// <summary>
        /// Wrapper function that converts between a double value between one unit and another.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <returns></returns>
        public static double ConvertByUnitIds(double value, string fromUnit, string toUnit)
        {
            return ForgeUnitsEngine.convert(value, fromUnit, toUnit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetUnit"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static double ParseExpressionByUnit(Unit targetUnit, string expression)
        {
            return ForgeUnitsEngine.parse(targetUnit.TypeId, expression);
        }

        /// <summary>
        /// Parses a string to a unit value.
        /// </summary>
        /// <param name="targetUnit"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static double ParseExpressionByUnitId(string targetUnit, string expression)
        {
            return ForgeUnitsEngine.parse(targetUnit, expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static double ParseExpression(string expression)
        {
            return ForgeUnitsEngine.parseUnitless(expression);
        }

        /// <summary>
        /// Returns a formatted unit value string using unit and symbol type ids.
        /// Sample: '12.345 m3' is a value of '12.345', of unit 'meter', of symbol 'meter cubed (m3)', of number format 'decimal', of precision '3'.
        /// </summary>
        /// <param name="numValue"></param>
        /// <param name="unitTypeId"></param>
        /// <param name="symbolTypeId"></param>
        /// <param name="precision"></param>
        /// <param name="decimalFormat"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static string ReturnFormattedString(double numValue, string unitTypeId, string symbolTypeId, int precision, string numberFormat)
        {
            NumberFormat actualNumberFormat = StringToNumberFormat(numberFormat);

            Unit unit = Unit.ByTypeID(unitTypeId);
            Symbol symbol = Symbol.ByTypeID(symbolTypeId);

            return ReturnFormattedString(numValue, unit, symbol, precision, actualNumberFormat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static NumberFormat StringToNumberFormat(string format)
        {
            switch (format)
            {
                case string s when s.Contains("Decimal"):
                    return NumberFormat.Decimal;
                case string s when s.Contains("Fraction"):
                    return NumberFormat.Fraction;
                case string s when s.Contains("None"):
                    return NumberFormat.None;
                default:
                    throw new Exception("Incorrect format supplied.");
            }
        }

        /// <summary>
        /// Returns a formatted unit value string with only 2 number formatting options: decimal and fraction.
        /// Sample: '12.345 m3' is a value of '12.345', of unit 'meter', of symbol 'meter cubed (m3)', of number format 'decimal', of precision '3'.
        /// </summary>
        /// <param name="numValue"></param>
        /// <param name="unit"></param>
        /// <param name="symbol"></param>
        /// <param name="precision"></param>
        /// <param name="numberFormat"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static string ReturnFormattedString(double numValue, Unit unit, Symbol symbol, int precision, NumberFormat numberFormat)
        {
            switch (numberFormat)
            {
                case NumberFormat.Decimal:
                    return Symbol.StringifyDecimal(numValue, precision, symbol, true);
                case NumberFormat.Fraction:
                    return Symbol.StringifyFraction(numValue, precision, symbol);
                default:
                    throw new Exception("Cannot stringify as there is no correct number format provided.");
            }
        }

        /// <summary>
        /// Helper function to cast an object to a type Symbol.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Symbol CastToSymbol(object value)
        {
            if (value is null) return null;

            var symbol = value as Symbol;
            if (symbol is null)
            {
                throw new ArgumentException($"Unable to cast {value.GetType()} to {typeof(Symbol)}");
            }
            return symbol;
        }

        /// <summary>
        /// Helper function to cast an object to a Format.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static NumberFormat CastToFormat(object value)
        {
            if (value is null) return NumberFormat.None;

            var format = value is NumberFormat numberFormat ? numberFormat : throw new ArgumentException($"Unable to cast {value.GetType()} to {typeof(NumberFormat)}");

            return format;
        }

        /// <summary>
        /// Helper function to cast an object to a type Unit.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Unit CastToUnit(object value)
        {
            if (value is null) return null;

            var unit = value as Unit;
            if (unit is null)
            {
                throw new ArgumentException($"Unable to cast {value.GetType()} to {typeof(Unit)}");
            }
            return unit;
        }

        private static ForgeUnitsCLR.UnitsEngine unitsEngine;

        /// <summary>
        /// Engine which loads schemas and is responsible for all ForgeUnit operations.
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static ForgeUnitsCLR.UnitsEngine ForgeUnitsEngine
        {
            get
            {
                if (unitsEngine == null)
                {
                    unitsEngine = new ForgeUnitsCLR.UnitsEngine();
                    var directory = Path.Combine(AssemblyDirectory, "unit");
                    SchemasCLR.SchemaUtility.addDefinitionsFromFolder(directory, unitsEngine);
                    unitsEngine.resolveSchemas();
                }

                return unitsEngine;
            }
        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        internal const string BaseAutodeskId = "autodesk.unit.unit:";

        [IsVisibleInDynamoLibrary(false)]
        public static string TypeIdShortName(string typeId)
        {
            var split = typeId.Split(':', '-');
            return split[1];
        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<Quantity> CovertQuantityDictionaryToList(
            Dictionary<string, ForgeUnitsCLR.Quantity> forgeDictionary)
        {
            var dynQauntities = new List<Quantity>();
            var values = forgeDictionary.Values.ToArray();
            for (int i = 0; i < values.Length - 1; i++)
            {
                if (TypeIdShortName(values[i].getTypeId()).Equals(TypeIdShortName(values[i + 1].getTypeId())))
                    continue;

                dynQauntities.Add(new Quantity(values[i]));
            }

            dynQauntities.Add(new Quantity(values.Last()));

            return dynQauntities;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<Symbol> ConvertSymbolDictionaryToList(
            Dictionary<string, ForgeUnitsCLR.Symbol> forgeDictionary)
        {
            var dynSymbols = new List<Symbol>();
            var values = forgeDictionary.Values.ToArray();
            for (int i = 0; i < values.Length - 1; i++)
            {
                if (TypeIdShortName(values[i].getTypeId()).Equals(TypeIdShortName(values[i + 1].getTypeId())))
                    continue;

                dynSymbols.Add(new Symbol(values[i]));
            }

            dynSymbols.Add(new Symbol(values.Last()));

            return dynSymbols;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<Unit> ConvertUnitsDictionaryToList(
            Dictionary<string, ForgeUnitsCLR.Unit> forgeDictionary)
        {
            var dynUnits = new List<Unit>();
            var values = forgeDictionary.Values.ToArray();
            for (int i = 0; i < values.Length - 1; i++)
            {
                if (TypeIdShortName(values[i].getTypeId()).Equals(TypeIdShortName(values[i + 1].getTypeId())))
                    continue;

                dynUnits.Add(new Unit(values[i]));
            }

            dynUnits.Add(new Unit(values.Last()));

            return dynUnits;
        }
    }
}