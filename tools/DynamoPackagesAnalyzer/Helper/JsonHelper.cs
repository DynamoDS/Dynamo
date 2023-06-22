using System.Text;
using Newtonsoft.Json;

namespace DynamoPackagesAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to handle json files
    /// </summary>
    internal static class JsonHelper
    {
        /// <summary>
        /// Reads a json file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static async Task<T> Read<T>(FileInfo file)
        {
            file.Refresh();
            FileStream fs = null;
            T f = default;
            byte[] data = null;

            await Task.Run(() =>
            {
                fs = file.OpenRead();
                data = new byte[file.Length];
                fs.Position = 0;
                fs.Read(data, 0, Convert.ToInt32(file.Length));
            });

            string text = Encoding.UTF8.GetString(data).Trim(new char[] { '\uFEFF' });
            f = JsonConvert.DeserializeObject<T>(text);

            fs.Dispose();
            return f;
        }
    }
}
