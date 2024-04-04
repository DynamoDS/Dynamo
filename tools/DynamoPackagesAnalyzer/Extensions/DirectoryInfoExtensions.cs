namespace System.IO
{
    /// <summary>
    /// Provides extensions method for DirectoryInfo
    /// </summary>
    internal static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Deletes a directory recursively, avoids the error when the directory is in use
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="recursive"></param>
        internal static void TryDelete(this DirectoryInfo directoryInfo, bool recursive = false)
        {
            try
            {
                directoryInfo.Delete(recursive);
            }
            catch (Exception) { }
        }
    }
}
