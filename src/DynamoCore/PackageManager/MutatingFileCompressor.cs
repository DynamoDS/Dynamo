using Dynamo.PackageManager.Interfaces;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An IFileCompressor that actually attempts to compress a directory
    /// </summary>
    internal class MutatingFileCompressor : IFileCompressor
    {
        public IFileInfo Zip(IDirectoryInfo directory)
        {
            return new RealFileInfo(Greg.Utility.FileUtilities.Zip(directory.FullName));
        }
    }
}