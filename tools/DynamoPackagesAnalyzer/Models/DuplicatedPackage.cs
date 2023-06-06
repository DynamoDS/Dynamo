namespace DynamoPackagesAnalyzer.Models
{
    /// <summary>
    /// Represents a duplicated DLL and the numbers of times that other packages references a DLL with the same name, and can be written to a csv file using <see cref="Helper.CsvHandler.WriteDuplicatedCsv(IEnumerable{DuplicatedPackage}, DateTime)"/>
    /// </summary>
    public class DuplicatedPackage
    {
        public string ArtifactName { get; set; }
        public int Count { get; set; }
        public string[] PackageNames { get; set; }

        public DuplicatedPackage()
        {

        }

        public DuplicatedPackage(string articfactName, int count, string[] packages)
        {
            ArtifactName = articfactName;
            Count = count;
            PackageNames = packages ?? Array.Empty<string>();
        }
    }
}
