using System.IO;

namespace Dynamo.PackageManager
{
    public class PackageFileInfo
    {
        public FileInfo Model { get; private set; }
        private readonly string packageRoot;

        /// <summary>
        /// Filename relative to the package root directory
        /// </summary>
        public string RelativePath
        {
            get
            {
                return Model.FullName.Substring(packageRoot.Length);
            }
        }

        public PackageFileInfo(string packageRoot, string filename)
        {
            this.packageRoot = packageRoot;
            this.Model = new FileInfo(filename);
        }
    }
}