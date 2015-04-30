namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An IDirectoryInfo representing a real DirectoryInfo object
    /// </summary>
    internal class TrueDirectoryInfo : IDirectoryInfo
    {
        private readonly System.IO.DirectoryInfo dirInfo;

        public TrueDirectoryInfo(System.IO.DirectoryInfo dirInfo)
        {
            this.dirInfo = dirInfo;
        }

        public string FullName
        {
            get { return dirInfo.FullName; }
        }
    }
}