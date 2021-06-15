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

namespace DynamoUnits
{
    public class UnitSymbol
    {
        internal readonly ForgeUnitsCLR.Symbol forgeSymbol;
        internal readonly ForgeUnitsCLR.PrefixOrSuffix forgePrefixOrSuffix;

        internal UnitSymbol(ForgeUnitsCLR.Symbol symbol)
        {
            this.forgeSymbol = symbol ?? throw new ArgumentNullException();
            this.forgePrefixOrSuffix = symbol.getPrefixOrSuffix();
        }

        [NodeName("Type Id")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("UnitSymbol.TypeIdDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("UnitSymbol.TypeIdSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public string TypeId => forgeSymbol.getTypeId();

        [NodeName("Unit")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("UnitSymbol.UnitDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("UnitSymbol.UnitSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public Unit Unit => new Unit(forgeSymbol.getUnit());

        [NodeName("Text")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("UnitSymbol.TextDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("UnitSymbol.TextSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public string Text => forgePrefixOrSuffix != null ? forgePrefixOrSuffix.getText() : "";

        //public ForgeUnitsCLR.Placement Placement => forgePrefixOrSuffix != null ? forgePrefixOrSuffix.getPlacement() : ForgeUnitsCLR.Placement.Suffix;


        [NodeName("Space")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("UnitSymbol.SpaceDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("UnitSymbol.SpaceSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public bool Space => forgePrefixOrSuffix == null || forgePrefixOrSuffix.hasSpace();

        [NodeName("By Type Id")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("UnitSymbol.ByTypeIDDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("UnitSymbol.ByTypeIDSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public static UnitSymbol ByTypeID(string typeId)
        {
            return new UnitSymbol(Utilities.ForgeUnitsEngine.getSymbol(typeId));
        }

        [NodeName("Symbols By Unit")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("UnitSymbol.SymbolsByUnitDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("UnitSymbol.SymbolsByUnitSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public static List<UnitSymbol> SymbolsByUnit(Unit unit)
        {
            var symbols = Utilities.ForgeUnitsEngine.getSymbols(unit.TypeId);
            return Utilities.ConvertSymbolDictionaryToList(symbols);
        }

        [NodeName("Stringify Decimal")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("UnitSymbol.StringifyDecimalDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("UnitSymbol.StringifyDecimalSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public static string StringifyDecimal(double value, int precision, UnitSymbol symbol,
            bool removeTrailingZeros)
        {
            return Utilities.ForgeUnitsEngine.stringifyFixedPoint(value, (byte)precision, symbol.TypeId,
                removeTrailingZeros);
        }

        [NodeName("Stringify Fraction")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("UnitSymbol.StringifyFractionDescription", typeof(DynamoUnits.Properties.Resources))]
        [NodeSearchTags("UnitSymbol.StringifyFractionSearchTags", typeof(DynamoUnits.Properties.Resources))]
        [IsDesignScriptCompatible]
        public static string StringifyFraction(double value, int precision, UnitSymbol symbol)
        {
            return Utilities.ForgeUnitsEngine.stringifyFraction(value, (byte)precision, symbol.TypeId);
        }

        public override string ToString()
        {
            return Text != "" ? "Symbol" + "(Text = " + Text + ")" : "Symbol";
        }
    }
}
