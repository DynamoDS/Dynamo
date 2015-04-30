namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An IFileCompressor that actually attempts to compress a directory
    /// </summary>
    internal class MutatingFileCompressor : IFileCompressor
    {
        public IFileInfo Zip(string directoryPath)
        {
            return new TrueFileInfo(Greg.Utility.FileUtilities.Zip(directoryPath));
        }
    }
}