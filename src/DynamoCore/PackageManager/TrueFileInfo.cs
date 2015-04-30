using System.IO;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An IFileInfo representing a real file on disk
    /// </summary>
    internal class TrueFileInfo : IFileInfo
    {
        private readonly System.IO.FileInfo fileInfo;

        public TrueFileInfo(string path)
        {
            this.fileInfo = new FileInfo(path);
        }

        public string Name
        {
            get { return fileInfo.Name;  }
        }

        public long Length
        {
            get { return fileInfo.Length; }
        }
    }
}