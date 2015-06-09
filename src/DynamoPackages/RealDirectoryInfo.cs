using Dynamo.PackageManager.Interfaces;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An IDirectoryInfo representing a real DirectoryInfo object
    /// </summary>
    internal class RealDirectoryInfo : IDirectoryInfo
    {
        private readonly System.IO.DirectoryInfo dirInfo;

        public RealDirectoryInfo(System.IO.DirectoryInfo dirInfo)
        {
            this.dirInfo = dirInfo;
        }

        public string FullName
        {
            get { return dirInfo.FullName; }
        }
    }
}