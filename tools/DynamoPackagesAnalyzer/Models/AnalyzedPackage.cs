namespace DynamoPackagesAnalyzer.Models
{
    /// <summary>
    /// Represents a dynamo package which has been processed and it's result can be written to the csv file using <see cref="Helper.CsvHandler.WritePackagesCsv(List{AnalyzedPackage}, DateTime)"/>
    /// </summary>
    internal class AnalyzedPackage
    {
        internal int Index { get; set; }
        internal string Id { get; set; }
        internal string UserId { get; set; }
        internal string UserName { get; set; }
        internal string Name { get; set; }
        internal string ArtifactPath { get; set; }
        internal string ArtifactName { get; set; }
        internal string ArchiveName { get; set; }
        internal string Version { get; set; }
        internal bool HasBinaries { get; set; }
        internal bool HasSource { get; set; }
        internal bool RequirePort { get; set; }
        internal bool HasAnalysisError { get; set; }
        internal string[] Result { get; set; }
        internal FileInfo[] DLLs { get; set; }
        internal DirectoryInfo Source { get; set; }

        internal AnalyzedPackage()
        {
            DLLs = Array.Empty<FileInfo>();
            Result = Array.Empty<string>();
        }

        internal AnalyzedPackage Copy()
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
