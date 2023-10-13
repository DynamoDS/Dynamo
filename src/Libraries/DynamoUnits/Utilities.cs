using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using System.Reflection;
using System.IO;
using System.Configuration;
using ForgeUnits = Autodesk.ForgeUnits;

namespace DynamoUnits
{
    /// <summary>
    /// Utility functions for unit conversion work flows and helper functions.
    /// </summary>
    public static class Utilities
    {
        private static ForgeUnits.UnitsEngine unitsEngine;

        /// <summary>
        /// Path to the directory used load the schema definitions.
        /// </summary>
        [SupressImportIntoVM]
        public static string SchemaDirectory { get; private set; } = Path.Combine(AssemblyDirectory, "unit");


        static Utilities()
        {
            Initialize();
        }

        /// <summary>
        /// Is used by static constructor, or during testing to reset schemas and units engine to default state.
        /// </summary>
        internal static void Initialize()
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
                unitsEngine = new ForgeUnits.UnitsEngine();
                ForgeUnits.SchemaUtility.addDefinitionsFromFolder(SchemaDirectory, unitsEngine);
                unitsEngine.resolveSchemas();
            }
            catch
            {
                unitsEngine = null;
                //There was an issue initializing the schemas at the specified path.
            }
        }
            
        /// <summary>
        /// only use this method during tests - allows setting a different schema location without
        /// worrying about distributing a test configuration file.
        /// </summary>
        internal static void SetTestEngine(string testSchemaDir)
        {
            try
            {
                unitsEngine = new ForgeUnits.UnitsEngine();
                ForgeUnits.SchemaUtility.addDefinitionsFromFolder(testSchemaDir, unitsEngine);
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
        /// <returns name="double">Converted value</returns>
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
        internal static ForgeUnits.UnitsEngine ForgeUnitsEngine
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
                string path = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Get all the loaded Quantities within the Dynamo session.
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static IEnumerable<Quantity> GetAllQuantities()
        {
            return CovertForgeQuantityDictionaryToCollection(ForgeUnitsEngine.getAllQuantities());
        }

        /// <summary>
        /// Get all the loaded Units within the Dynamo session.
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static IEnumerable<Unit> GetAllUnits()
        {
            return ConvertForgeUnitDictionaryToCollection(ForgeUnitsEngine.getAllUnits());
        }

        /// <summary>
        /// Get all the loaded Symbols within the Dynamo session.
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static IEnumerable<Symbol> GetAllSymbols()
        {
            return ConvertForgeSymbolDictionaryToCollection(ForgeUnitsEngine.getAllSymbols());
        }

        /// <summary>
        /// Converts a dictionary of Forge SDK Quantities to a collection of Quantities
        /// </summary>
        /// <param name="forgeDictionary">A dictionary keyed by a forge typeID and Forge SDK Quantities as values</param>
        /// <returns></returns>
        internal static IEnumerable<Quantity> CovertForgeQuantityDictionaryToCollection(
            Dictionary<string, ForgeUnits.Quantity> forgeDictionary)
        {
            var dynQuantities = new List<Quantity>();

            var versionDictionary = GetAllRegisteredQuantityVersions(forgeDictionary);

            foreach (var item in versionDictionary)
            {
                var typeId = item.Key + "-" + item.Value.ToString();
                if (forgeDictionary.TryGetValue(typeId, out var quantity))
                {
                    dynQuantities.Add(new Quantity(quantity));
                }
            }

            return dynQuantities;
        }

        /// <summary>
        /// Converts a dictionary of Forge SDK Symbols to a collection of Symbols
        /// </summary>
        /// <param name="forgeDictionary">A dictionary keyed by a forge typeID and Forge SDK Symbols as values</param>
        /// <returns></returns>
        internal static IEnumerable<Symbol> ConvertForgeSymbolDictionaryToCollection(
            Dictionary<string, ForgeUnits.Symbol> forgeDictionary)
        {
            var dynSymbols = new List<Symbol>();

            var versionDictionary = GetAllLastestRegisteredSymbolVersions(forgeDictionary);

            foreach (var item in versionDictionary)
            {
                var typeId = item.Key + "-" + item.Value.ToString();
                if (forgeDictionary.TryGetValue(typeId, out var symbol))
                {
                    dynSymbols.Add(new Symbol(symbol));
                }
            }

            return dynSymbols;
        }

        /// <summary>
        /// Converts a dictionary of Forge SDK Units to a collection of Units
        /// </summary>
        /// <param name="forgeDictionary">A dictionary keyed by a forge typeID and Forge SDK Units as values</param>
        /// <returns></returns>
        internal static IEnumerable<Unit> ConvertForgeUnitDictionaryToCollection(
            Dictionary<string, ForgeUnits.Unit> forgeDictionary)
        {
            var dynUnits = new List<Unit>();

            var versionDictionary = GetAllLatestRegisteredUnitVersions(forgeDictionary);

            foreach (var item in versionDictionary)
            {
                var typeId = item.Key + "-" + item.Value.ToString();
                if (forgeDictionary.TryGetValue(typeId, out var unit))
                {
                    dynUnits.Add(new Unit(unit));
                }
            }

            return dynUnits;
        }

        /// <summary>
        /// Get latest versions of registered Quantities within the Dynamo session.
        /// </summary>
        /// <returns>A dictionary keyed by a version-less typeID and the latest registered version as value</returns>
        internal static Dictionary<string, Version> GetAllRegisteredQuantityVersions()
        {
            return GetAllRegisteredQuantityVersions(ForgeUnitsEngine.getAllQuantities());
        }

        /// <summary>
        /// Get latest versions of registered Quantities from a dictionary of Forge SDK Quantities
        /// </summary>
        /// <param name="forgeDictionary">A dictionary keyed by a forge typeID and Forge SDK Quantities as values</param>
        /// <returns>A dictionary keyed by a version-less typeID and the latest registered version as value</returns>
        internal static Dictionary<string, Version> GetAllRegisteredQuantityVersions(Dictionary<string, ForgeUnits.Quantity> forgeDictionary)
        {
            var versionDictionary = new Dictionary<string, Version>();

            foreach (var typeId in forgeDictionary.Keys)
            {
                if (TryParseTypeId(typeId, out string typeName, out Version version))
                {
                    if (versionDictionary.TryGetValue(typeName, out var existingVersion))
                    {
                        if (existingVersion.CompareTo(version) >= 0)
                        {
                            continue;
                        }
                    }

                    versionDictionary[typeName] = version;
                }
            }

            return versionDictionary;
        }

        /// <summary>
        /// Get latest versions of registered Symbols within the Dynamo session.
        /// </summary>
        /// <returns>A dictionary keyed by a version-less typeID and the latest registered version as value</returns>
        internal static Dictionary<string, Version> GetAllLastestRegisteredSymbolVersions()
        {
            return GetAllLastestRegisteredSymbolVersions(ForgeUnitsEngine.getAllSymbols());
        }

        /// <summary>
        /// Get latest versions of registered Symbols from a dictionary of Forge SDK Symbols
        /// </summary>
        /// <param name="forgeDictionary">A dictionary keyed by a forge typeID and Forge SDK Symbols as values</param>
        /// <returns>A dictionary keyed by a version-less typeID and the latest registered version as value</returns>
        internal static Dictionary<string, Version> GetAllLastestRegisteredSymbolVersions(Dictionary<string, ForgeUnits.Symbol> forgeDictionary)
        {
            var versionDictionary = new Dictionary<string, Version>();

            foreach (var typeId in forgeDictionary.Keys)
            {
                if (TryParseTypeId(typeId, out string typeName, out Version version))
                {
                    if (versionDictionary.TryGetValue(typeName, out var existingVersion))
                    {
                        if (existingVersion.CompareTo(version) >= 0)
                        {
                            continue;
                        }
                    }

                    versionDictionary[typeName] = version;
                }
            }

            return versionDictionary;
        }

        /// <summary>
        /// Get latest versions of registered Units within the Dynamo session.
        /// </summary>
        /// <returns>A dictionary keyed by a version-less typeID and the latest registered version as value</returns>
        internal static Dictionary<string, Version> GetAllLatestRegisteredUnitVersions()
        {
            return GetAllLatestRegisteredUnitVersions(ForgeUnitsEngine.getAllUnits());
        }

        /// <summary>
        /// Get latest versions of registered Units from a dictionary of Forge SDK Units
        /// </summary>
        /// <param name="forgeDictionary">A dictionary keyed by a forge typeID and Forge SDK Units as values</param>
        /// <returns>A dictionary keyed by a version-less typeID and the latest registered version as value</returns>
        internal static Dictionary<string, Version> GetAllLatestRegisteredUnitVersions(Dictionary<string, ForgeUnits.Unit> forgeDictionary)
        {
            var versionDictionary = new Dictionary<string, Version>();

            foreach (var typeId in forgeDictionary.Keys)
            {
                if (TryParseTypeId(typeId, out string typeName, out Version version))
                {
                    if (versionDictionary.TryGetValue(typeName, out var existingVersion))
                    {
                        if (existingVersion.CompareTo(version) >= 0)
                        {
                            continue;
                        }
                    }

                    versionDictionary[typeName] = version;
                }
            }

            return versionDictionary;
        }

        /// <summary>
        /// Try to get a valid typeName and version from a Forge TypeID string
        /// By convention the Id should be in the format of "foo-1.0.2" where a "-" is the divider between typeName and version.
        /// </summary>
        /// <param name="typeId">Forge TypeID string</param>
        /// <param name="typeName">Type name for the TypeID. This is the TypeID stripped of version.</param>
        /// <param name="version">Version object</param>
        /// <returns>True if the format is valid</returns>
        internal static bool TryParseTypeId(string typeId, out string typeName, out Version version)
        {
            var split = typeId.Split('-');
            if (split.Length == 2)
            {
                if (Version.TryParse(split[1], out var versionFound))
                {
                    typeName = split[0];
                    version = versionFound;
                    return true;
                }
            }

            typeName = "";
            version = null;

            return false;
        }
    }
}
