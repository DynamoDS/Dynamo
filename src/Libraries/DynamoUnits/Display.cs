using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DynamoUnits
{
    public class Display
    {
        /// <summary>
        /// Precision format for number of decimals to display within Dynamo UI
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static string PrecisionFormat { get; set; } = "f4";

        /// <summary>
        /// Number of Decimals to display within Dynamo UI
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static int Precision => Convert.ToInt16(PrecisionFormat.ToCharArray().Last().ToString());

        /// <summary>
        /// Display format for Unitized values. Decimal vs Fractional
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public enum NumberFormat
        {
            /// <summary>
            /// 
            /// </summary>
            Decimal = 0,
            /// <summary>
            /// 
            /// </summary>
            Fraction = 1
        }

        /// <summary>
        /// Format to display unitized values within the Dynamo UI
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public static NumberFormat UnitNumberFormat { get; set; } = NumberFormat.Decimal;

        /// <summary>
        /// Remove trailing zeros for unitized decimal values within the Dynamo UI
        /// </summary>
        public static bool RemoveTrailingZeros { get; set; } = true;

        internal static Symbol SetSymbolCases(Unit unit)
        {
            Symbol symbol;
            var specialCaseLast = new string[6] { "Angle", "Flow", "Power", "Pressure", "Stress", "Temperature Interval" };

            var quantity = unit.QuantitiesContainingUnit;
            var quantityName = quantity.First().Name;

            if (quantityName == "Length")
            {
                if (unit.Name == "Feet")
                {
                    symbol = Symbol.SymbolsByUnit(unit).Find(s => s.Text == "ft");
                }
                else
                {
                    symbol = Symbol.SymbolsByUnit(unit).First();
                }
            }
            else if (quantityName == "Currency")
            {
                symbol = Symbol.SymbolsByUnit(unit).Find(s => s.Text == "$");
            }
            else if (specialCaseLast.Contains(quantityName))
            {
                symbol = Symbol.SymbolsByUnit(unit).First();
            }
            else
            {
                symbol = Symbol.SymbolsByUnit(unit).Last();
            }

            return symbol;
        }
    }
}