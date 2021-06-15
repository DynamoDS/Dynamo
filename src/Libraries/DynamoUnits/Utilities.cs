using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Migration;
using Dynamo.Nodes;
using Dynamo.UI.Commands;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using Newtonsoft.Json;
using ProtoCore.AST.ImperativeAST;
using AstFactory = ProtoCore.AST.AssociativeAST.AstFactory;
using DoubleNode = ProtoCore.AST.AssociativeAST.DoubleNode;
using Dynamo.Utilities;
using Dynamo.Engine.CodeGeneration;
using System.Collections;
using DynamoUnits.Properties;
using Autodesk.DesignScript.Runtime;
using System.Reflection;
using System.IO;

namespace DynamoUnits
{
    public static class Utilities
    {
        [NodeName("Convert By Units")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Utilities.ConvertByUnitsDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("Utilities.ConvertByUnitsSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public static double ConvertByUnits(double value, Unit fromUnit, Unit toUnit)
        {
            return ForgeUnitsEngine.convert(value, fromUnit.TypeId, toUnit.TypeId);
        }


        [NodeName("Convert By Unit Ids")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Utilities.ConvertByUnitIdsDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("Utilities.ConvertByUnitIdsSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
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

        /// <summary>
        /// Returns a formatted unit value string.
        /// </summary>
        /// <param name="numValue"></param>
        /// <param name="unit"></param>
        /// <param name="unitSymbol"></param>
        /// <param name="precision"></param>
        /// <param name="decimalFormat"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static string ReturnFormattedString(double numValue, Unit unit, UnitSymbol unitSymbol, int precision, bool decimalFormat)
        {
            string outputString = string.Empty;

            if (decimalFormat)
                outputString = UnitSymbol.StringifyDecimal(numValue, precision, unitSymbol, true);
            else
                outputString = UnitSymbol.StringifyFraction(numValue, precision, unitSymbol);

            return outputString;

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