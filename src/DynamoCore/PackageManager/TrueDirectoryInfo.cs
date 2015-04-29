namespace Dynamo.PackageManager
{
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