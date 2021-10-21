using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using System.Reflection;
using System.IO;
using System.Configuration;

namespace DynamoUnits
{
    public static class Utilities
    {
        private static ForgeUnitsCLR.UnitsEngine unitsEngine;

        public static readonly string SchemaDirectory = Path.Combine(AssemblyDirectory, "unit");

        static Utilities()
        {
            var assemblyFilePath = Assembly.GetExecutingAssembly().Location;

            var config = ConfigurationManager.OpenExeConfiguration(assemblyFilePath);
            var key = config.AppSettings.Settings["schemaPath"];
            string path = null;
            if (key != null)
            {
                path = key.Value;
            }

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                SchemaDirectory = path;
            }

            try
            {
                unitsEngine = new ForgeUnitsCLR.UnitsEngine();
                SchemasCLR.SchemaUtility.addDefinitionsFromFolder(SchemaDirectory, unitsEngine);
                unitsEngine.resolveSchemas();
            }
            catch
            {
                unitsEngine = null;
                //There was an issue initializing the schemas at the specified path.
            }
            
        }

        /// <summary>
        /// Converts a value from one Unit System to another Unit System
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="fromUnit">Unit object</param>
        /// <param name="toUnit">Unit object</param>
        /// <returns name="double">Converted value</returns>
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
        [IsVisibleInDynamoLibrary(false)]
        public static double ConvertByUnitIds(double value, string fromUnit, string toUnit)
        {
            return ForgeUnitsEngine.convert(value, fromUnit, toUnit);
        }

        /// <summary>
        /// Parses a string containing values with units and math functions to a unit value.
        /// For example, "1ft + 2.54cm + 3in" could be converted to 14in
        /// </summary>
        /// <param name="targetUnit">Unit system to target</param>
        /// <param name="expression">String to convert to a value</param>
        /// <returns name="double">Converted value</returns>
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
        /// Parses a string containing math functions to a unit value.
        /// For example, "(1 + 3)^2 - 4 * 2" could be converted to 8
        /// </summary>
        /// <param name="expression">String to convert to a value</param>
        /// <returns name="double">Converted value</returns>
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
                    throw new Exception("There was an issue loading Unit Schemas from the specified path: " 
                                        + SchemaDirectory);
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
            var dynQuantities = new List<Quantity>();

            if (!forgeDictionary.Any())
            {
                return dynQuantities;
            }

            var values = forgeDictionary.Values.ToArray();
            for (int i = 0; i < values.Length - 1; i++)
            {
                if (TypeIdShortName(values[i].getTypeId()).Equals(TypeIdShortName(values[i + 1].getTypeId())))
                    continue;

                dynQuantities.Add(new Quantity(values[i]));
            }

            dynQuantities.Add(new Quantity(values.Last()));

            return dynQuantities;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<Symbol> ConvertSymbolDictionaryToList(
            Dictionary<string, ForgeUnitsCLR.Symbol> forgeDictionary)
        {
            var dynSymbols = new List<Symbol>();

            if (!forgeDictionary.Any())
            {
                return dynSymbols;
            }

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

            if (!forgeDictionary.Any())
            {
                return dynUnits;
            }

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