namespace Dynamo.PackageManager
{
    internal class MutatingCompressor : ICompressor
    {
        public IFileInfo Zip(string directoryPath)
        {
            return new TrueFileInfo(Greg.Utility.FileUtilities.Zip(directoryPath));
        }
    }
}