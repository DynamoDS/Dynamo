using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.DesignScript.Runtime;

namespace DynamoUnits
{
    public static class Utilities
    {
        public static double ConvertByUnits(double value, Unit fromUnit, Unit toUnit)
        {
            return ForgeUnitsEngine.convert(value, fromUnit.TypeId, toUnit.TypeId);
        }

        public static double ConvertByUnitIds(double value, string fromUnit, string toUnit)
        {
            return ForgeUnitsEngine.convert(value, fromUnit, toUnit);
        }

        public static double ParseExpressionByUnit(Unit targetUnit, string expression)
        {
            return ForgeUnitsEngine.parse(targetUnit.TypeId, expression);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static double ParseExpressionByUnitId(string targetUnit, string expression)
        {
            return ForgeUnitsEngine.parse(targetUnit, expression);
        }

        public static double ParseExpression(string expression)
        {
            return ForgeUnitsEngine.parseUnitless(expression);
        }

        private static ForgeUnitsCLR.UnitsEngine unitsEngine;

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