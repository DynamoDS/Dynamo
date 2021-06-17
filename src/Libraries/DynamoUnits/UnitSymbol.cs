using System;
using System.Collections.Generic;
using System.Globalization;
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


        public string TypeId => forgeSymbol.getTypeId();


        public Unit Unit => new Unit(forgeSymbol.getUnit());


        public string Text => forgePrefixOrSuffix != null ? forgePrefixOrSuffix.getText() : "";

        //public ForgeUnitsCLR.Placement Placement => forgePrefixOrSuffix != null ? forgePrefixOrSuffix.getPlacement() : ForgeUnitsCLR.Placement.Suffix;

        public bool Space => forgePrefixOrSuffix == null || forgePrefixOrSuffix.hasSpace();

      
        public static UnitSymbol ByTypeID(string typeId)
        {
            return new UnitSymbol(Utilities.ForgeUnitsEngine.getSymbol(typeId));
        }

        public static List<UnitSymbol> SymbolsByUnit(Unit unit)
        {
            var symbols = Utilities.ForgeUnitsEngine.getSymbols(unit.TypeId);
            return Utilities.ConvertSymbolDictionaryToList(symbols);
        }

        public static string StringifyDecimal(double value, int precision, UnitSymbol symbol,
            bool removeTrailingZeros)
        {
            return Utilities.ForgeUnitsEngine.stringifyFixedPoint(value, (byte)precision, symbol.TypeId,
                removeTrailingZeros);
        }

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
