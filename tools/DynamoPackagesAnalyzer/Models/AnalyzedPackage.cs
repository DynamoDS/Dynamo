namespace DynamoPackagesAnalyzer.Models
{
    /// <summary>
    /// Represents a dynamo package which has been processed and it's result can be written to the csv file using <see cref="Helper.CsvHandler.WritePackagesCsv(List{AnalyzedPackage}, DateTime)"/>
    /// </summary>
    public class AnalyzedPackage
    {
        public int Index { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string ArtifactPath { get; set; }
        public string ArtifactName { get; set; }
        public string ArchiveName { get; set; }
        public string Version { get; set; }
        public bool HasBinaries { get; set; }
        public bool HasSource { get; set; }
        public bool RequirePort { get; set; }
        public bool HasAnalysisError { get; set; }
        public string[] Result { get; set; }
        public FileInfo[] DLLs { get; set; }
        public DirectoryInfo Source { get; set; }

        public AnalyzedPackage()
        {
            DLLs = Array.Empty<FileInfo>();
            Result = Array.Empty<string>();
        }

        public AnalyzedPackage Copy()
        {
            return new AnalyzedPackage
            {
                Index = Index,
                Id = Id,
                Name = Name,
                Version = Version,
                UserId = UserId,
                UserName = UserName,
                HasBinaries = HasBinaries,
                HasSource = HasSource,
                ArchiveName = ArchiveName,
                Result = Array.Empty<string>()
            };
        }
    }
}
