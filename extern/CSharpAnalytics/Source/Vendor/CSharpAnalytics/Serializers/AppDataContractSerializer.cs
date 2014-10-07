using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CSharpAnalytics.Serializers
{
    /// <summary>
    /// Provides an easy way to serialize and deserialize simple classes to a user AppData folder in
    /// Windows Forms applications.
    /// </summary>
    internal static class AppDataContractSerializer
    {
        private static readonly string folderPath;

        static AppDataContractSerializer()
        {
            var appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

            var customAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            var folderName = customAttributes.Length > 0 
                ? ((AssemblyCompanyAttribute)customAttributes[0]).Company 
                : "CSharpAnalytics";

            folderPath = Path.Combine(appDataPath, folderName);
        }

        /// <summary>
        /// Restore an object from local folder storage.
        /// </summary>
        /// <param name="filename">Optional filename to use, name of the class if not provided.</param>
        /// <param name="deleteBadData">Optional boolean on whether delete the existing file if deserialization fails, defaults to false.</param>
        /// <returns>Task that holds the deserialized object once complete.</returns>
        public static async Task<T> Restore<T>(string filename = null, bool deleteBadData = false)
        {
            var serializer = new DataContractSerializer(typeof(T), new[] { typeof(DateTimeOffset) });

            try
            {
                var file = GetFilePath<T>(filename);

                try
                {
                    using (var inputStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    {
                        if (inputStream.Length == 0)
                        {
                            return default(T);
                        }

                        using (var memoryStream = new MemoryStream())
                        {
                            await inputStream.CopyToAsync(memoryStream);
                            await inputStream.FlushAsync();
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            return (T)serializer.ReadObject(memoryStream);
                        }
                    }
                }
                catch (SerializationException)
                {
                    if (deleteBadData)
                        File.Delete(file);
                    throw;
                }
            }
            catch (FileNotFoundException)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Save an object to local folder storage asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/> to save.</typeparam>
        /// <param name="value">Object to save to local storage.</param>
        /// <param name="filename">Optional filename to save to, defaults to the name of the class.</param>
        /// <returns>Task that completes once the object is saved.</returns>
        public static async Task Save<T>(T value, string filename = null)
        {
            var serializer = new DataContractSerializer(typeof(T), new[] { typeof(DateTimeOffset) });

            var file = GetFilePath<T>(filename);

            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, value);

                using (var fileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
            }
        }

        /// <summary>
        /// Gets the file path of the given type and file name.
        /// </summary>
        /// <typeparam name="T">The type to get path.</typeparam>
        /// <param name="filename">The file name to get path.</param>
        /// <returns>The full path to the file.</returns>
        private static string GetFilePath<T>(string filename)
        {
            // Ensure directory exists
            Directory.CreateDirectory(folderPath);

            return Path.Combine(folderPath, filename ?? typeof(T).Name);
        }
    }
}