using System.IO;
using Dynamo.PackageManager.Interfaces;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     An IFileInfo representing a real file on disk
    /// </summary>
    internal class RealFileInfo : IFileInfo
    {
        private readonly System.IO.FileInfo fileInfo;

        public RealFileInfo(string path)
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