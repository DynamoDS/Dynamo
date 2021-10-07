using System;
using System.Collections.Generic;

namespace DynamoUnits
{
    public class Symbol
    {
        internal readonly ForgeUnitsCLR.Symbol forgeSymbol;
        internal readonly ForgeUnitsCLR.PrefixOrSuffix forgePrefixOrSuffix;

        internal Symbol(ForgeUnitsCLR.Symbol symbol)
        {
            this.forgeSymbol = symbol ?? throw new ArgumentNullException();
            this.forgePrefixOrSuffix = symbol.getPrefixOrSuffix();
        }

        /// <summary>
        /// Returns the forge TypeId of this Symbol
        /// </summary>
        public string TypeId => forgeSymbol.getTypeId();
        
        /// <summary>
        /// Returns the corresponding unit of this Symbol
        /// </summary>
        public Unit Unit => new Unit(forgeSymbol.getUnit());

        /// <summary>
        /// Returns the string representation of thi Symbol.
        /// </summary>
        public string Text => forgePrefixOrSuffix != null ? forgePrefixOrSuffix.getText() : "";

        /// <summary>
        /// 
        /// </summary>
        public bool Space => forgePrefixOrSuffix == null || forgePrefixOrSuffix.hasSpace();

        /// <summary>
        /// Returns a Symbol from its typeId.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static Symbol ByTypeID(string typeId)
        {
            return new Symbol(Utilities.ForgeUnitsEngine.getSymbol(typeId));
        }

        /// <summary>
        /// Returns all available Symbols, given a Unit.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static List<Symbol> SymbolsByUnit(Unit unit)
        {
            var symbols = Utilities.ForgeUnitsEngine.getSymbols(unit.TypeId);
            return Utilities.ConvertSymbolDictionaryToList(symbols);
        }

        /// <summary>
        /// Returns the string expression of a decimal value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="precision"></param>
        /// <param name="symbol"></param>
        /// <param name="removeTrailingZeros"></param>
        /// <returns></returns>
        public static string StringifyDecimal(double value, int precision, Symbol symbol,
            bool removeTrailingZeros)
        {
            return Utilities.ForgeUnitsEngine.stringifyFixedPoint(value, (byte)precision, symbol.TypeId,
                removeTrailingZeros);
        }

        /// <summary>
        /// Returns the string expression of a fraction value..
        /// </summary>
        /// <param name="value"></param>
        /// <param name="precision"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static string StringifyFraction(double value, int precision, Symbol symbol)
        {
            return Utilities.ForgeUnitsEngine.stringifyFraction(value, (byte)precision, symbol.TypeId);
        }

        public override string ToString()
        {
            return Text != "" ? "Symbol" + "(Text = " + Text + ")" : "Symbol";
        }
    }
}
