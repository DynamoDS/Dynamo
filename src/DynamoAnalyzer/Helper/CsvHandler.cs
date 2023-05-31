using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DynamoAnalyzer.Models;
using Newtonsoft.Json;

namespace DynamoAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to work with csv files
    /// </summary>
    public static class CsvHandler
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
        public static async Task<string> WritePackagesCsv(List<AnalyzedPackage> packages, DateTime date)
        {
            FileInfo file = new FileInfo(Path.Combine(AnalyzeEnvironment.GetWorkspace().FullName, $"results[{date:yyyy-MM-dd_HH-mm-ss}].csv"));

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
        public static async Task<string> WriteDuplicatedCsv(IEnumerable<DuplicatedPackage> packages, DateTime date)
        {
            FileInfo file = new FileInfo(Path.Combine(AnalyzeEnvironment.GetWorkspace().FullName, $"duplicated[{date:yyyy_MM_dd_HH_mm_ss}].csv"));

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
    public class DuplicatedPackageMap : ClassMap<DuplicatedPackage>
    {
        public DuplicatedPackageMap()
        {
            Map(m => m.ArtifactName).Name(nameof(DuplicatedPackage.ArtifactName));
            Map(m => m.Count).Name(nameof(DuplicatedPackage.Count));
            Map(m => m.Packages).TypeConverter<ArrayConverter>();
        }
    }

    /// <summary>
    /// Defines the properties to be written to a file
    /// </summary>
    public class AnalyzedPackageMap : ClassMap<AnalyzedPackage>
    {
        public AnalyzedPackageMap()
        {
            Map(m => m.Index).Name(nameof(AnalyzedPackage.Index));
            Map(m => m.Id).Name(nameof(AnalyzedPackage.Id));
            Map(m => m.Name).Name(nameof(AnalyzedPackage.Name));
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
    public class ArrayConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value.GetType().Name.Equals("String[]"))
            {
                return JsonConvert.SerializeObject(value).Replace(",", "&").Replace("\"", "'");
            }
            return "";
        }
    }

    /// <summary>
    /// Converts an string array to it's string representation
    /// </summary>
    public class ResultArrayConverter : DefaultTypeConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value.GetType().Name.Equals("String[]"))
            {
                string[] values = (string[])value;
                string[] valuesToWrite = new string[6];
                if (values.Length > 5)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        valuesToWrite[i] = values[i].Trim();
                    }

                    int difference = values.Length - valuesToWrite.Length;
                    if (difference > 0)
                    {
                        valuesToWrite[5] = $"+ {values.Length - valuesToWrite.Length} replacements required";
                    }
                }
                else
                {
                    valuesToWrite = values;
                }

                return JsonConvert.SerializeObject(valuesToWrite.Where(f => !string.IsNullOrEmpty(f))).Replace(",", "&").Replace("\"", "'");
            }
            return "";
        }
    }
}
