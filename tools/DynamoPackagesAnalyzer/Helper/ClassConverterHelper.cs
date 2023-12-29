using DynamoPackagesAnalyzer.Models;
using DynamoPackagesAnalyzer.Models.DirectorySource;
using DynamoPackagesAnalyzer.Models.Greg;

namespace DynamoPackagesAnalyzer.Helper
{
    internal static class ClassConverterHelper
    {
        internal static AnalyzedPackage ToAnalyzedPackage(PkgJson json)
        {
            return new AnalyzedPackage()
            {
                Name = json.Name,
                Version = json.Version,
            };
        }

        internal static AnalyzedPackage ToAnalyzedPackage(PackageHeaderCustom package)
        {
            var version = package.versions.LastOrDefault();
            var mantainer = package.maintainers.FirstOrDefault();

            return new AnalyzedPackage()
            {
                ArchiveName = version.url,
                HasSource = !string.IsNullOrEmpty(package.repository_url),
                Id = package._id,
                Name = package.name,
                UserId = mantainer._id,
                UserName = mantainer.username,
                Version = version.version,
                Index = package.Index
            };
        }
    }
}
