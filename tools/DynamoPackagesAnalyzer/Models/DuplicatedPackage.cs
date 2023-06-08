namespace DynamoPackagesAnalyzer.Models
{
    /// <summary>
    /// Represents a duplicated DLL and the numbers of times that other packages references a DLL with the same name, and can be written to a csv file using <see cref="Helper.CsvHandler.WriteDuplicatedCsv(IEnumerable{DuplicatedPackage}, DateTime)"/>
    /// </summary>
    internal class DuplicatedPackage
    {
        internal string ArtifactName { get; set; }
        internal int Count { get; set; }
        internal string[] PackageNames { get; set; }

        internal DuplicatedPackage()
        {

        }

        internal DuplicatedPackage(string articfactName, int count, string[] packages)
        {
            ArtifactName = articfactName;
            Count = count;
            PackageNames = packages ?? Array.Empty<string>();
        }
    }
}
