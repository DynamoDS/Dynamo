using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DynamoPackagesAnalyzer.Models;
using Newtonsoft.Json;

namespace DynamoPackagesAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to work with csv files
    /// </summary>
    internal static class CsvHandler
    {
        private static CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
        };

        /// <summary>
        /// Writes to a file the result of all the analyzed packages and it's DLLs
        /// </summary>
        /// <param name="packages"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static async Task<string> WritePackagesCsv(List<AnalyzedPackage> packages, DateTime date)
        {
            FileInfo file = new FileInfo(Path.Combine(WorkspaceHelper.GetWorkspace().FullName, $"results[{date:yyyy-MM-dd_HH-mm-ss}].csv"));

            using (var writer = new StreamWriter(file.FullName))
            using (var csv = new CsvWriter(writer, csvConfiguration))
            {
                csv.Context.RegisterClassMap<AnalyzedPackageMap>();
                await csv.WriteRecordsAsync(packages);
            }

            return file.FullName;
        }

        /// <summary>
        /// Writes to a file the number of times that a DLL name is referenced from other packages
        /// </summary>
        /// <param name="packages"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        internal static async Task<string> WriteDuplicatedCsv(IEnumerable<DuplicatedPackage> packages, DateTime date)
        {
            FileInfo file = new FileInfo(Path.Combine(WorkspaceHelper.GetWorkspace().FullName, $"duplicated[{date:yyyy_MM_dd_HH_mm_ss}].csv"));

            using (var writer = new StreamWriter(file.FullName))
            using (var csv = new CsvWriter(writer, csvConfiguration))
            {
                csv.Context.RegisterClassMap<DuplicatedPackageMap>();
                await csv.WriteRecordsAsync(packages);
            }

            return file.FullName;
        }
    }

    /// <summary>
    /// Defines the properties to be written to a file
    /// </summary>
    internal class DuplicatedPackageMap : ClassMap<DuplicatedPackage>
    {
        internal DuplicatedPackageMap()
        {
            Map(m => m.ArtifactName).Name(nameof(DuplicatedPackage.ArtifactName)).TypeConverter<StringConverter>();
            Map(m => m.Count).Name(nameof(DuplicatedPackage.Count));
            Map(m => m.PackageNames).TypeConverter<ArrayConverter>();
        }
    }

    /// <summary>
    /// Defines the properties to be written to a file
    /// </summary>
    internal class AnalyzedPackageMap : ClassMap<AnalyzedPackage>
    {
        internal AnalyzedPackageMap()
        {
            Map(m => m.Index).Name(nameof(AnalyzedPackage.Index));
            Map(m => m.Id).Name(nameof(AnalyzedPackage.Id));
            Map(m => m.Name).Name(nameof(AnalyzedPackage.Name)).TypeConverter<StringConverter>();
            Map(m => m.Version).Name(nameof(AnalyzedPackage.Version));

            Map(m => m.UserId).Name(nameof(AnalyzedPackage.UserId));
            Map(m => m.UserName).Name(nameof(AnalyzedPackage.UserName));

            Map(m => m.ArchiveName).Name(nameof(AnalyzedPackage.ArchiveName));
            Map(m => m.ArtifactPath).Name(nameof(AnalyzedPackage.ArtifactPath));
            Map(m => m.ArtifactName).Name(nameof(AnalyzedPackage.ArtifactName));
            Map(m => m.HasBinaries).Name(nameof(AnalyzedPackage.HasBinaries));
            Map(m => m.HasSource).Name(nameof(AnalyzedPackage.HasSource));
            Map(m => m.RequirePort).Name(nameof(AnalyzedPackage.RequirePort));
            Map(m => m.HasAnalysisError).Name(nameof(AnalyzedPackage.HasAnalysisError));
            Map(m => m.DLLs).Ignore();
            Map(m => m.Source).Ignore();
            Map(m => m.Result).TypeConverter<ResultArrayConverter>();
        }
    }

    /// <summary>
    /// Converts an string array to it's string representation
    /// </summary>
    internal class ArrayConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            switch (value)
            {
                case string[] data:
                    return JsonConvert.SerializeObject(data).Replace(",", "&").Replace("\"", "'");
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Converts an string array to it's string representation and truncates the lenght of the array to 5 values due Microsft Excel truncates cells with very long text and breaks the columns
    /// </summary>
    internal class ResultArrayConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            switch (value)
            {
                case string[] data:
                    string[] valuesToWrite = new string[6];
                    if (data.Length > 5)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            valuesToWrite[i] = data[i].Trim();
                        }

                        int difference = data.Length - valuesToWrite.Length;
                        if (difference > 0)
                        {
                            valuesToWrite[5] = $"+ {difference} replacements required";
                        }
                    }
                    else
                    {
                        valuesToWrite = data;
                    }

                    return JsonConvert.SerializeObject(valuesToWrite.Where(f => !string.IsNullOrEmpty(f))).Replace(",", "&").Replace("\"", "'");
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Converts problematic characters that could break the csv file columns
    /// </summary>
    internal class StringConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            switch (value)
            {
                case string text:
                    return text.Replace(",", "&");
                default:
                    return "";
            }
        }
    }
}
