namespace DynamoAnalyzer.Models
{
    /// <summary>
    /// Represents a duplicated DLL and the numbers of times that other packages references a DLL with the same name
    /// </summary>
    public class DuplicatedPackage
    {
        public string ArtifactName { get; set; }
        public int Count { get; set; }
        public string[] Packages { get; set; }

        public DuplicatedPackage()
        {

        }

        public DuplicatedPackage(string articfactName, int count, string[] packages)
        {
            ArtifactName = articfactName;
            Count = count;
            Packages = packages ?? Array.Empty<string>();
        }
    }
}
