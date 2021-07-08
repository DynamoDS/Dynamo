using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using Newtonsoft.Json;
using ProtoCore.AST.ImperativeAST;
using AstFactory = ProtoCore.AST.AssociativeAST.AstFactory;
using DoubleNode = ProtoCore.AST.AssociativeAST.DoubleNode;
using System.Collections;
using DynamoUnits.Properties;
using Autodesk.DesignScript.Runtime;
using System.Reflection;
using System.IO;

namespace DynamoUnits
{

    /// <summary>
    /// This enum enables quick selection of value (string representation) formatting. It also enables us to add
    /// additional formats should they be needed.
    /// </summary>
    public enum NumberFormat
    {
        None = 0,
        Decimal = 1,
        Fraction = 2
    }
    public static class Utilities
    {
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

        public static double ParseExpression(string expression)
        {
            return ForgeUnitsEngine.parseUnitless(expression);
        }

        /// <summary>
        /// Returns a formatted unit value string using unitType and unitSymbol type ids.
        /// Sample: '12.345 m3' is a value of '12.345', of unit 'meter', of symbol 'meter cubed (m3)', of number format 'decimal', of precision '3'.
        /// </summary>
        /// <param name="numValue"></param>
        /// <param name="unitTypeId"></param>
        /// <param name="unitSymbolId"></param>
        /// <param name="precision"></param>
        /// <param name="decimalFormat"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static string ReturnFormattedString(double numValue, string unitTypeId, string unitSymbolId, int precision, string numberFormat)
        {
            NumberFormat actualNumberFormat = StringToNumberFormat(numberFormat);

            Unit unit = Unit.ByTypeID(unitTypeId);
            UnitSymbol unitSymbol = UnitSymbol.ByTypeID(unitSymbolId);

            return ReturnFormattedString(numValue, unit, unitSymbol, precision, actualNumberFormat);
        }

        public static NumberFormat StringToNumberFormat(object stringObject)
        {
            string inputString = stringObject as string;
            switch (inputString)
            {
                case string s when s.Contains("Decimal"):
                    return NumberFormat.Decimal;
                case string s when s.Contains("Fraction"):
                    return NumberFormat.Fraction;
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
        /// <param name="unitSymbol"></param>
        /// <param name="precision"></param>
        /// <param name="decimalFormat"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static string ReturnFormattedString(double numValue, Unit unit, UnitSymbol unitSymbol, int precision, NumberFormat numberFormat)
        {
            switch (numberFormat)
            {
                case NumberFormat.Decimal:
                    return UnitSymbol.StringifyDecimal(numValue, precision, unitSymbol, true);
                case NumberFormat.Fraction:
                    return UnitSymbol.StringifyFraction(numValue, precision, unitSymbol);
                default:
                    throw new Exception("Cannot stringify as there is no correct number format provided.");
            }
        }

        /// <summary>
        /// Helper function to cast an object to a type UnitSymbol.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static UnitSymbol CastToUnitSymbol(object value)
        {
            if (value is null) return null;

            var symbol = value as UnitSymbol;
            if (symbol is null)
            {
                throw new ArgumentException($"Unable to cast {value.GetType()} to {typeof(UnitSymbol)}");
            }
            return symbol;
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
        public static List<UnitSymbol> ConvertSymbolDictionaryToList(
            Dictionary<string, ForgeUnitsCLR.Symbol> forgeDictionary)
        {
            var dynSymbols = new List<UnitSymbol>();
            var values = forgeDictionary.Values.ToArray();
            for (int i = 0; i < values.Length - 1; i++)
            {
                if (TypeIdShortName(values[i].getTypeId()).Equals(TypeIdShortName(values[i + 1].getTypeId())))
                    continue;

                dynSymbols.Add(new UnitSymbol(values[i]));
            }

            dynSymbols.Add(new UnitSymbol(values.Last()));

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