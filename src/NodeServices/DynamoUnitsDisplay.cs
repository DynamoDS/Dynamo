using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DynamoUnits
{
    //TODO see obsolete note below - lets get rid of this - seems to overlap with PreferenceSettings.NumberFormat and ProtoCore.PrecisionFormat.

    /// <summary>
    /// Data used to set display and formatting preferences for Dynamo UI
    /// </summary>
    [Obsolete("This type will be removed in a future version of Dynamo, please use PreferenceSettings.NumberFormat instead. ")]
    [IsVisibleInDynamoLibrary(false)]
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
        [IsVisibleInDynamoLibrary(false)]
        public static bool RemoveTrailingZeros { get; set; } = true;
    }
}
