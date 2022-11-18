using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.DesignScript.Runtime;

#if NET6_0_OR_GREATER
using ForgeUnitsCLR = Autodesk.ForgeUnits;
#endif

namespace DynamoUnits
{
    /// <summary>
    /// A text symbol used to associate a value with a Unit system.
    /// </summary>
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
        /// Gets the Forge type schema identifier for a Symbol
        /// </summary>
        /// <returns name="string">Forge TypeId</returns>
        public string TypeId => forgeSymbol.getTypeId();

        /// <summary>
        /// Gets the corresponding Unit of a Symbol
        /// </summary>
        /// <returns name="Unit">Unit object</returns>
        public Unit Unit => new Unit(forgeSymbol.getUnit());

        /// <summary>
        /// Gets the string representation of a Symbol.
        /// </summary>
        /// <returns name="string">Symbol text</returns>
        public string Text => forgePrefixOrSuffix != null ? Encoding.UTF8.GetString(Encoding.Default.GetBytes(forgePrefixOrSuffix.getText())) : "";

        /// <summary>
        /// Gets a boolean indicating if there is typically a space between the unit value and symbol.
        /// </summary>
        /// <returns name="bool">Space between unit and symbol</returns>
        public bool Space => forgePrefixOrSuffix == null || forgePrefixOrSuffix.hasSpace();

        /// <summary>
        /// Creates a Symbol object from its Forge type schema identifier string.
        /// </summary>
        /// <param name="typeId">Forge TypeId string</param>
        /// <returns name="Symbol">Symbol object</returns>
        public static Symbol ByTypeID(string typeId)
        {
            try
            {
                return new Symbol(Utilities.ForgeUnitsEngine.getSymbol(typeId));
            }
            catch (Exception e)
            {
                //The exact match for the Forge TypeID failed.  Test for a fallback.  This can be either earlier or later version number.
                if (Utilities.TryParseTypeId(typeId, out string typeName, out Version version))
                {
                    var versionDictionary = Utilities.GetAllLastestRegisteredSymbolVersions();
                    if (versionDictionary.TryGetValue(typeName, out var existingVersion))
                    {
                        return new Symbol(Utilities.ForgeUnitsEngine.getSymbol(typeName + "-" + existingVersion.ToString()));
                    }
                }

                //else re-throw existing exception as there is no fallback
                throw;
            }
        }

        /// <summary>
        /// Gets all available Symbols associated with a Unit.
        /// </summary>
        /// <param name="unit">Unit object</param>
        /// <returns name="Symbol[]">List of Symbols</returns>
        public static IEnumerable<Symbol> SymbolsByUnit(Unit unit)
        {
            var symbols = Utilities.ForgeUnitsEngine.getSymbols(unit.TypeId);
            return Utilities.ConvertForgeSymbolDictionaryToCollection(symbols);
        }

        /// <summary>
        /// Returns the formatted unit expression for a given value and symbol in a decimal format.
        /// </summary>
        /// <param name="value">Number value for the unit</param>
        /// <param name="precision">Decimal precision for the expression</param>
        /// <param name="symbol">Symbol type</param>
        /// <param name="removeTrailingZeros">Remove trailing zeros in the output</param>
        /// <returns name="string">Formatted unit expression</returns>
        public static string StringifyDecimal(double value, int precision, Symbol symbol,
            bool removeTrailingZeros = false)
        {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(Utilities.ForgeUnitsEngine.stringifyFixedPoint(value, (byte)precision, symbol.TypeId,
                removeTrailingZeros)));
        }

        /// <summary>
        /// Returns the formatted expression for a given value and symbol in a fraction format.
        /// </summary>
        /// <param name="value">Number value for the expression</param>
        /// <param name="precision">Bits of precision for the resulting fraction - i.e 2-> 1/4, 7 -> 1/128. Max supported precision is 9 bits</param>
        /// <param name="symbol">Symbol type</param>
        /// <returns name="string">Formatted unit expression</returns>
        public static string StringifyFraction(double value, int precision, Symbol symbol)
        {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(Utilities.ForgeUnitsEngine.stringifyFraction(value, (byte)precision, symbol.TypeId)));
        }

        public override string ToString()
        {
            return Text != "" ? "Symbol" + "(Text = " + Text + ")" : "Symbol";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Text ?? String.Empty).GetHashCode() ^
                       (TypeId ?? String.Empty).GetHashCode();
            }
        }

        public override bool Equals(object obj) => this.EqualsImpl(obj as Symbol);

        internal bool EqualsImpl(Symbol u)
        {
            if (u is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, u))
            {
                return true;
            }

            if (this.GetType() != u.GetType())
            {
                return false;
            }

            return (Text == u.Text) && (TypeId == u.TypeId);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static bool operator ==(Symbol lhs, Symbol rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.EqualsImpl(rhs);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static bool operator !=(Symbol lhs, Symbol rhs) => !(lhs == rhs);
    }
}
