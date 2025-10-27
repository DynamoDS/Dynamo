using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ForgeUnits = Autodesk.ForgeUnits;

namespace DynamoUnits
{
    /// <summary>
    /// Utility functions for unit conversion work flows and helper functions.
    /// </summary>
    public static class Utilities
    {
        private static ForgeUnits.UnitsEngine unitsEngine;
        private static List<string> candidateDirectories = new List<string>();

        /// <summary>
        /// Path to the directory used load the schema definitions.
        /// </summary>
        [SupressImportIntoVM]
        public static string SchemaDirectory { get; private set; } = string.Empty;

        static Utilities()
        {
            Initialize();
        }

        /// <summary>
        /// Is used by static constructor, or during testing to reset schemas and units engine to default state.
        /// </summary>
        internal static void Initialize()
        {
            // Build candidate schema directories list
            candidateDirectories.Clear();

            var assemblyFilePath = Assembly.GetExecutingAssembly().Location;
            var config = ConfigurationManager.OpenExeConfiguration(assemblyFilePath);

            // Add config path if it's valid
            var configPath = config.AppSettings.Settings["schemaPath"]?.Value;
            if (!string.IsNullOrEmpty(configPath) && Directory.Exists(configPath))
            {
                candidateDirectories.Add(configPath);
            }

            // Add ASC schema paths from installed components
            AddAscSchemaPaths(candidateDirectories);

            // Add bundled schema directory as final candidate
            candidateDirectories.Add(BundledSchemaDirectory);

            // Try each candidate directory until we find one that works
            foreach (var directory in candidateDirectories)
            {
                // Always update SchemaDirectory to the current attempt for clearer error
                // reporting. If loading succeeds, SchemaDirectory reflects the working
                // path. Otherwise, it shows the last path tried, which will be displayed
                // in any thrown exception. If all paths fail, the exception message will
                // show the default bundled schema directory, which should never fail.
                SchemaDirectory = directory;

                unitsEngine = TryLoadSchemaFromDirectory(directory);
                if (unitsEngine != null)
                {
                    break; // Found the schema directory, so stop trying.
                }
            }
        }

        /// <summary>
        /// only use this method during tests - allows setting a different schema location without
        /// worrying about distributing a test configuration file.
        /// </summary>
        internal static void SetTestEngine(string testSchemaDir)
        {
            unitsEngine = TryLoadSchemaFromDirectory(testSchemaDir);
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
                    var attemptedPaths = string.Join(", ", candidateDirectories);
                    throw new Exception($"There was an issue loading Unit Schemas. Attempted paths: {attemptedPaths}");
                }

                return unitsEngine;
            }
        }

        private static string BundledSchemaDirectory
        {
            get
            {
                string path = Assembly.GetExecutingAssembly().Location;
                return Path.Combine(Path.GetDirectoryName(path), "unit");
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

        /// <summary>
        /// Adds ASC (Autodesk Shared Components) schema paths to the candidate directories list.
        /// 
        /// We use 'AscSdkWrapper' directly here because 'InstalledAscLookUp' is overkill -- it's got
        /// extra logic we don't need. 'AscSdkWrapper' gives us just the version/path info for ASC
        /// installs, which is all we care about for schema discovery. This also decouples us from
        /// changes in 'InstalledAscLookUp' that could break or complicate schema path resolution.
        /// </summary>
        /// <param name="candidateDirectories">List to add discovered ASC schema paths to</param>
        private static void AddAscSchemaPaths(List<string> candidateDirectories)
        {
            // Currently ASC discovery is only available on Windows. When cross-platform ASC
            // support becomes available, we can extend this to work on other platforms.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            try
            {
                // Use reflection to dynamically load DynamoInstallDetective at runtime.
                // This avoids the need for a direct project reference and InternalsVisibleTo,
                // maintaining cross-platform compatibility for the DynamoUnits library.
                //
                // First check if DynamoInstallDetective is already loaded in the AppDomain
                // (e.g., by DynamoRevit or other host applications)
                var dynamoInstallDetectiveAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "DynamoInstallDetective");
                
                if (dynamoInstallDetectiveAssembly == null)
                {
                    // Not loaded yet, try to load it from the same directory as DynamoUnits
                    var currAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var dllPath = Path.Combine(currAssemblyDir, "DynamoInstallDetective.dll");
                    dynamoInstallDetectiveAssembly = Assembly.LoadFrom(dllPath);
                }

                var ascWrapperType = dynamoInstallDetectiveAssembly.GetType("DynamoInstallDetective.AscSdkWrapper");
                if (ascWrapperType == null)
                {
                    return; // AscSdkWrapper type not found
                }

                // Get major versions using reflection: AscSdkWrapper.GetMajorVersions()
                var getMajorVersionsMethod = ascWrapperType.GetMethod("GetMajorVersions", 
                    BindingFlags.Public | BindingFlags.Static);
                var majorVersions = (string[])getMajorVersionsMethod?.Invoke(null, null);

                if (majorVersions == null) return;

                // Get the ASC_STATUS enum for comparison
                var ascStatusType = ascWrapperType.GetNestedType("ASC_STATUS");
                var successValue = Enum.Parse(ascStatusType, "SUCCESS");

                foreach (var majorVersion in majorVersions)
                {
                    // Create AscSdkWrapper instance: new AscSdkWrapper(majorVersion)
                    var ascWrapper = Activator.CreateInstance(ascWrapperType, majorVersion);

                    // Call GetInstalledPath using reflection
                    var getInstalledPathMethod = ascWrapperType.GetMethod("GetInstalledPath");
                    var parameters = new object[] { string.Empty };
                    var result = getInstalledPathMethod?.Invoke(ascWrapper, parameters);

                    // Check if result equals ASC_STATUS.SUCCESS
                    if (result != null && result.Equals(successValue))
                    {
                        var installPath = (string)parameters[0];
                        var schemaPath = Path.Combine(installPath, "coreschemas", "unit");
                        candidateDirectories.Add(schemaPath);
                    }
                }
            }
            catch
            {
                // Ignore errors when discovering ASC paths - this is optional discovery.
                // DynamoInstallDetective.dll might not be available on some deployments,
                // or ASC might not be installed on the system.
            }
        }

        /// <summary>
        /// Attempts to load schema from the specified directory and create a UnitsEngine.
        /// </summary>
        /// <param name="schemaDirectory">Directory containing the schema definitions</param>
        /// <returns>A ForgeUnits.UnitsEngine instance, or null if loading failed</returns>
        private static ForgeUnits.UnitsEngine TryLoadSchemaFromDirectory(string schemaDirectory)
        {
            try
            {
                // Validate that the directory exists and contains required subdirectories
                if (IsValidSchemaDirectory(schemaDirectory))
                {
                    var engine = new ForgeUnits.UnitsEngine();
                    ForgeUnits.SchemaUtility.addDefinitionsFromFolder(schemaDirectory, engine);
                    engine.resolveSchemas();
                    return engine;
                }

                return null; // Invalid schema directory
            }
            catch
            {
                //There was an issue initializing the schemas at the specified path.
                return null;
            }
        }

        /// <summary>
        /// Validates that a directory contains the required schema subdirectories.
        /// </summary>
        /// <param name="schemaDirectory">Directory to validate</param>
        /// <returns>True if the directory contains all required subdirectories</returns>
        private static bool IsValidSchemaDirectory(string schemaDirectory)
        {
            if (string.IsNullOrEmpty(schemaDirectory) || !Directory.Exists(schemaDirectory))
            {
                return false;
            }

            var requiredSubdirectories = new[] { "dimension", "quantity", "symbol", "unit" };

            foreach (var subdirectory in requiredSubdirectories)
            {
                var subdirectoryPath = Path.Combine(schemaDirectory, subdirectory);
                if (!Directory.Exists(subdirectoryPath))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
