using System.Text;
using DynamoAnalyzer.Models.UpgradeAssistant;
using Newtonsoft.Json;

namespace DynamoAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to handle sarif files
    /// </summary>
    public static class SarifHelper
    {
        public static async Task<Sarif> ReadSarif(FileInfo sarifFile)
        {
            sarifFile.Refresh();
            FileStream fs = null;
            Sarif f = null;
            byte[] data = null;

            await Task.Run(() =>
            {
                fs = sarifFile.OpenRead();
                data = new byte[sarifFile.Length];
                fs.Position = 0;
                fs.Read(data, 0, Convert.ToInt32(sarifFile.Length));
            });

            string text = Encoding.UTF8.GetString(data).Trim(new char[] { '\uFEFF' });
            f = JsonConvert.DeserializeObject<Sarif>(text);

            fs.Dispose();
            return f;
        }
    }
}
