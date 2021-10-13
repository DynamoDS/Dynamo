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
        private static ForgeUnitsCLR.UnitsEngine unitsEngine;

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