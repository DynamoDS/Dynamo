namespace DynamoPackagesAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to write information to the console 
    /// </summary>
    internal static class LogHelper
    {
        /// <summary>
        /// Writes a message to the console
        /// </summary>
        /// <param name="package"></param>
        /// <param name="message"></param>
        internal static void Log(string package, string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}] Package: {package} -> {message}");
        }
    }
}
