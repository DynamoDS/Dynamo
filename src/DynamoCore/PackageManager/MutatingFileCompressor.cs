using System;
using System.IO;
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
            if (directory == null) throw new ArgumentNullException("directory");

            if (!(new MutatingFileSystem()).DirectoryExists(directory.FullName))
            {
                throw new DirectoryNotFoundException(directory.FullName);
            }

            return new RealFileInfo(Greg.Utility.FileUtilities.Zip(directory.FullName));
        }
    }
}