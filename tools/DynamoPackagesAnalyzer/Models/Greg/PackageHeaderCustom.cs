using Greg.Responses;

namespace DynamoPackagesAnalyzer.Models.Greg
{
    /// <summary>
    /// Greg Client dto
    /// </summary>
    internal class PackageHeaderCustom: PackageHeader
    {
        /// <summary>
        /// Used to order the dlls when saving to CSV
        /// </summary>
        internal int Index { get; set; }
    }
}
